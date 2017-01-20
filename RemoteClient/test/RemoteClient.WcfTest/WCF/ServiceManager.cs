using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using System.Xml;
using RemoteClient.WcfTest.WCF.Dispatch;

namespace RemoteClient.WcfTest.WCF
{
    public sealed class ServiceManager
    {
        private readonly ServiceInfo[] _services;

        public ServiceManager()
        {
            _services = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => x.GetCustomAttribute<ServiceAttribute>() != null)
                .Select(x => new ServiceInfo(x))
                .ToArray();
        }
        
        public void RunAll(Uri baseAddress)
        {
            foreach (var service in _services)
            {
                service.Start(baseAddress);
            }
        }

        [DebuggerDisplay("{_serviceType.Name}:{_subAddress}")]
        private class ServiceInfo
        {
            private readonly string _subAddress;
            private readonly Type _serviceType;

            public ServiceInfo(Type serviceType)
            {
                _serviceType = serviceType;

                var attribute = serviceType.GetCustomAttribute<ServiceAttribute>();
                _subAddress = attribute.SubAddress;
            }

            public ServiceHost ServiceHost { get; private set; }

            public void Start(Uri baseAddress)
            {
                var serviceInterface = _serviceType.GetInterfaces()
                                                   .First(i => i.GetCustomAttribute<ServiceContractAttribute>(true) != null);
                var attribute = _serviceType.GetCustomAttribute<ServiceBehaviorAttribute>(true);
                if (attribute?.InstanceContextMode == InstanceContextMode.Single)
                {
                    var singleton = Activator.CreateInstance(_serviceType);
                    ServiceHost = new WebServiceHost(singleton, new Uri(baseAddress, _subAddress));
                }
                else
                {
                    ServiceHost = new WebServiceHost(_serviceType, new Uri(baseAddress, _subAddress));
                }
                ServiceHost.Description.Behaviors.Add(new ServiceMetadataBehavior { HttpGetEnabled = true });
                var debugBehavior = ServiceHost.Description.Behaviors.OfType<ServiceDebugBehavior>().FirstOrDefault();
                if (debugBehavior != null)
                {
                    debugBehavior.IncludeExceptionDetailInFaults = true;
                }
                else
                {
                    ServiceHost.Description.Behaviors.Add(new ServiceDebugBehavior { IncludeExceptionDetailInFaults = true });
                }

                var webHttpBinding = new WebHttpBinding
                {
                    MaxReceivedMessageSize = int.MaxValue,
                    MaxBufferSize = int.MaxValue,
                    MaxBufferPoolSize = int.MaxValue,
                    ReaderQuotas =
                            new XmlDictionaryReaderQuotas
                            {
                                MaxArrayLength = int.MaxValue,
                                MaxStringContentLength = int.MaxValue,
                                MaxDepth = 32
                            }
                };

                webHttpBinding.ContentTypeMapper = new NewtonsoftJsonContentTypeMapper();
                ServiceHost.AddServiceEndpoint(serviceInterface, webHttpBinding, string.Empty);

                foreach (var endpoint in ServiceHost.Description.Endpoints)
                {
                    endpoint.Behaviors.Add(new NullableWebHttpBehavior
                    {
                        HelpEnabled = false,
                        DefaultBodyStyle = WebMessageBodyStyle.Bare,
                        DefaultOutgoingRequestFormat = WebMessageFormat.Json,
                        DefaultOutgoingResponseFormat = WebMessageFormat.Json,
                        FaultExceptionEnabled = true
                    });

                    endpoint.Behaviors.Add(new NewtonsoftJsonBehavior());
                }

                ServiceHost.Open();
            }
        }

        public void CloseAll()
        {
            foreach (var sh in _services.Where(x => x.ServiceHost != null).Select(x => x.ServiceHost))
            {
                (sh.SingletonInstance as IDisposable)?.Dispose();
                sh.Close();
            }
        }
    }
}

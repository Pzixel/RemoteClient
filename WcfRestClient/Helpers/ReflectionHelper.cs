using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.ServiceModel.Web;
using WcfRestClient.Core;

namespace WcfRestClient.Helpers
{
    internal static class ReflectionHelper
    {
        public static MethodInfo DisposeMethod { get; } = typeof (IDisposable).GetMethod("Dispose");

        public static ModuleBuilder Builder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("ServiceClientFactoryAssembly"), AssemblyBuilderAccess.Run)
                                                             .DefineDynamicModule("MainModule");

        public static WcfOperationDescriptor GetUriTemplate(MethodInfo methodInfo)
        {
            var webGet = methodInfo.GetCustomAttributes(typeof (WebGetAttribute), true);
            if (webGet.Length > 0)
            {
                var webGetAttribute = (WebGetAttribute) webGet[0];
                return new WcfOperationDescriptor(webGetAttribute.UriTemplate, "GET");
            }
            var webInvoke = methodInfo.GetCustomAttributes(typeof (WebInvokeAttribute), true);
            if (webInvoke.Length > 0)
            {
                var webInvokeAttribute = (WebInvokeAttribute) webInvoke[0];
                return new WcfOperationDescriptor(webInvokeAttribute.UriTemplate, webInvokeAttribute.Method);
            }
            throw new ArgumentException("Method doesn't have WebGet or WebInvoke attribute");
        }

        public static IEnumerable<MethodInfo> GetAllMethods(this Type type)
        {
            return type.GetMethods()
                .Concat(type.BaseType?.GetMethods() ?? new MethodInfo[0])
                .Concat(type.GetInterfaces().SelectMany(x => x.GetMethods()));
        }
    }
}
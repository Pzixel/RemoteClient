using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.ServiceModel.Web;

namespace WcfRestClient.Core.Helpers
{
    internal static class ReflectionHelper
    {
        public static MethodInfo DisposeMethod { get; } = typeof (IDisposable).GetMethod("Dispose");
        public static Type GetPropertyInterfaceImplementation<T>() where T : class => InterfaceImplementator<T>.Value;

        public static ModuleBuilder Builder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("ServiceClientFactoryAssembly"), AssemblyBuilderAccess.Run)
                                                             .DefineDynamicModule("MainModule");

        public static WcfOperationDescriptor GetUriTemplate(MethodInfo methodInfo)
        {
            var webGet = methodInfo.GetCustomAttributes(typeof (WebGetAttribute), true);
            if (webGet.Length > 0)
            {
                var getAttribute = (WebGetAttribute) webGet[0];
                return new WcfOperationDescriptor(getAttribute.UriTemplate, "GET", getAttribute.BodyStyle, getAttribute.RequestFormat, getAttribute.ResponseFormat);
            }
            var webInvoke = methodInfo.GetCustomAttributes(typeof (WebInvokeAttribute), true);
            if (webInvoke.Length > 0)
            {
                var invokeAttribute = (WebInvokeAttribute) webInvoke[0];
                return new WcfOperationDescriptor(invokeAttribute.UriTemplate, invokeAttribute.Method, invokeAttribute.BodyStyle, invokeAttribute.RequestFormat, invokeAttribute.ResponseFormat);
            }
            throw new ArgumentException("Method doesn't have WebGet or WebInvoke attribute");
        }

        public static IEnumerable<MethodInfo> GetAllMethods(this Type type)
        {
            return type.GetMethods()
                .Concat(type.BaseType?.GetMethods() ?? new MethodInfo[0])
                .Concat(type.GetInterfaces().SelectMany(x => x.GetMethods()));
        }

        private static class InterfaceImplementator<T> where T: class 
        {
            [SuppressMessage("ReSharper", "StaticMemberInGenericType")]
            public static Type Value { get; }
            static InterfaceImplementator()
            {
                var interfaceType = typeof(T);
                if (!interfaceType.IsInterface)
                {
                    throw new ArgumentException($"{interfaceType.FullName} should be an interface!");
                }
                var interfaceProps = interfaceType.GetProperties();
                if (interfaceType.GetMethods().Except(interfaceProps.Select(x => x.GetMethod).Concat(interfaceProps.Select(x => x.SetMethod))).Any())
                {
                    throw new ArgumentException($"{interfaceType.FullName} must have properties only!");
                }
                var tb = Builder.DefineType($"<{interfaceType.Name}>__Implementation", TypeAttributes.Class | TypeAttributes.Sealed);
                foreach (var interfaceProp in interfaceProps)
                {
                    var prop = tb.EmitAutoProperty(interfaceProp.Name, interfaceProp.PropertyType);
                    if (interfaceProp.CanRead)
                    {
                        tb.DefineMethodOverride(prop.GetMethod, interfaceProp.GetMethod);
                    }
                    if (interfaceProp.CanWrite)
                    {
                        tb.DefineMethodOverride(prop.SetMethod, interfaceProp.SetMethod);
                    }
                }
                tb.AddInterfaceImplementation(interfaceType);
                Value = tb.CreateType();
            }
        }
    }
}
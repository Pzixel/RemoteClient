using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.ServiceModel.Web;

namespace WcfRestClient.Core.Helpers
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
    }
}
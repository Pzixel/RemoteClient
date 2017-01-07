﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace WcfRestClient.Core.Helpers
{
    internal static class ReflectionHelper
    {
        public static MethodInfo DisposeMethod { get; } = typeof (IDisposable).GetMethod("Dispose");
        public static Type GetPropertyInterfaceImplementation<T>() where T : class => InterfaceImplementator<T>.Value;

        public static ModuleBuilder Builder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("ServiceClientFactoryAssembly"), AssemblyBuilderAccess.Run)
                                                             .DefineDynamicModule("MainModule");

        private const string WebGetFullName = "System.ServiceModel.Web.WebGetAttribute";
        private const string WebInvokeFullName = "System.ServiceModel.Web.WebInvokeAttribute";

        public static WcfOperationDescriptor GetUriTemplate(MethodInfo methodInfo)
        {
            var uriAttribute = methodInfo.GetCustomAttributes().FirstOrDefault(x =>
            {
                var attributeGuid = x.GetType().FullName;
                return attributeGuid == WebGetFullName || attributeGuid == WebInvokeFullName;
            });

            if (uriAttribute == null)
            {
                throw new ArgumentException("Method doesn't have WebGet or WebInvoke attribute");
            }

            var attributeType = uriAttribute.GetType();
            string method = attributeType.FullName == WebGetFullName ? "GET" : (string) attributeType.GetProperty("Method").GetValue(uriAttribute);
            string uriTemplate = (string) attributeType.GetProperty("UriTemplate").GetValue(uriAttribute);
            var requestFormat = (OperationWebMessageFormat) attributeType.GetProperty("RequestFormat").GetValue(uriAttribute);
            var responseFormat = (OperationWebMessageFormat) attributeType.GetProperty("ResponseFormat").GetValue(uriAttribute);

            return new WcfOperationDescriptor(uriTemplate, method, requestFormat, responseFormat);
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
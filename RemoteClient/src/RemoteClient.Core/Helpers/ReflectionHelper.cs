using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace RemoteClient.Core.Helpers
{
    internal static class ReflectionHelper
    {
        public static MethodInfo DisposeMethod { get; } = typeof (IDisposable).GetTypeInfo().GetDeclaredMethod("Dispose");
        public static TypeInfo GetPropertyInterfaceImplementation<T>() where T : class => InterfaceImplementator<T>.Value;

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

            var attributeType = uriAttribute.GetType().GetTypeInfo();
            string method = attributeType.FullName == WebGetFullName ? "GET" : (string) attributeType.GetDeclaredProperty("Method").GetValue(uriAttribute);
            string uriTemplate = (string) attributeType.GetDeclaredProperty("UriTemplate").GetValue(uriAttribute);
            var requestFormat = (OperationWebMessageFormat) attributeType.GetDeclaredProperty("RequestFormat").GetValue(uriAttribute);
            var responseFormat = (OperationWebMessageFormat) attributeType.GetDeclaredProperty("ResponseFormat").GetValue(uriAttribute);

            return new WcfOperationDescriptor(uriTemplate, method, requestFormat, responseFormat);
        }

        public static IEnumerable<MethodInfo> GetAllMethods(this Type type)
        {
            return type.GetRuntimeMethods().Concat(type.GetTypeInfo().ImplementedInterfaces.SelectMany(x => x.GetAllMethods()));
        }

        private static class InterfaceImplementator<T> where T: class 
        {
            [SuppressMessage("ReSharper", "StaticMemberInGenericType")]
            public static TypeInfo Value { get; }
            static InterfaceImplementator()
            {
                var interfaceType = typeof(T);
                var typeInfo = interfaceType.GetTypeInfo();
                if (!typeInfo.IsInterface)
                {
                    throw new ArgumentException($"{typeInfo.FullName} should be an interface!");
                }
                var interfaceProps = typeInfo.DeclaredProperties.ToList();
                if (typeInfo.DeclaredMethods.Except(interfaceProps.Select(x => x.GetMethod).Concat(interfaceProps.Select(x => x.SetMethod))).Any())
                {
                    throw new ArgumentException($"{typeInfo.FullName} must have properties only!");
                }
                var tb = Builder.DefineType($"<{typeInfo.Name}>__Implementation", TypeAttributes.Class | TypeAttributes.Sealed);
                tb.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);
                var ctor = tb.DefineConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                                                CallingConventions.Standard,
                                                interfaceProps.Select(x => x.PropertyType).ToArray()).GetILGenerator();
                int i = 1;
                foreach (var interfaceProp in interfaceProps)
                {
                    var prop = tb.EmitAutoProperty(interfaceProp.Name, interfaceProp.PropertyType);
                    if (interfaceProp.CanRead)
                    {
                        tb.DefineMethodOverride(prop.PropertyInfo.GetMethod, interfaceProp.GetMethod);
                    }
                    if (interfaceProp.CanWrite)
                    {
                        tb.DefineMethodOverride(prop.PropertyInfo.SetMethod, interfaceProp.SetMethod);
                    }
                    ctor.EmitLdarg(0);
                    ctor.EmitLdarg(i++);
                    ctor.Emit(OpCodes.Stfld, prop.FieldInfo);
                }
                ctor.Emit(OpCodes.Ret);
                tb.AddInterfaceImplementation(interfaceType);
                
                Value = tb.CreateTypeInfo();
            }
        }
    }
}
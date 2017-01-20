using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using RemoteClient.Core.Helpers;

namespace RemoteClient.Core
{
    /// <summary>
    /// Generate REST client for specified WCF service contract
    /// </summary>
    /// <typeparam name="T">Client interface. Must be IDisposable in order to properly clean <see cref="IAsyncRequestProcessor"/> parameter</typeparam>
    public static class ServiceClient<T> where T : IDisposable
    {
        [SuppressMessage("ReSharper", "StaticMemberInGenericType")]
        private static readonly Type ClientType = CreateType();

        public static T New(IAsyncRequestProcessor processor)
        {
            return (T)Activator.CreateInstance(ClientType, processor);
        }

        private static Type CreateType()
        {
            var typeName = typeof(T).Name;
            var typeInfo = typeof(T).GetTypeInfo();
            if (!typeInfo.IsInterface)
                throw new Exception($"{typeName} is not an interface!");

            var tb = ReflectionHelper.Builder.DefineType($"<{typeName}>__Client", TypeAttributes.Class | TypeAttributes.Sealed);
            tb.SetParent(typeof(AsyncClientBase));
            tb.CreatePassThroughConstructors<AsyncClientBase>();
            tb.AddInterfaceImplementation(typeof(T));


            var taskTypeInfo = typeof(Task).GetTypeInfo();
            foreach (var interfaceMethod in typeof(T).GetAllMethods())
            {
                if (taskTypeInfo.IsAssignableFrom(interfaceMethod.ReturnType.GetTypeInfo()))
                {
                    ImplementMethod(tb, interfaceMethod);
                }
                else if (ReferenceEquals(interfaceMethod, ReflectionHelper.DisposeMethod))
                {
                    ImplementProcessorProxyMethodCall(tb, interfaceMethod);
                }
                else
                {
                    throw new ClientGenerationException("Interface contains methods with non-task return type");
                }
            }

            return tb.CreateTypeInfo().AsType();
        }

        private static void ImplementProcessorProxyMethodCall(TypeBuilder tb, MethodInfo interfaceMethod)
        {
            var implementation = tb.DefineMethod(interfaceMethod.Name, MethodAttributes.Public | MethodAttributes.Virtual);
            var il = implementation.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, typeof(AsyncClientBase).GetRuntimeFields().First(x => x.Name == "Processor"));
            il.EmitCallWithParams(interfaceMethod, interfaceMethod.GetParameters().Length);
            il.Emit(OpCodes.Ret);
            tb.DefineMethodOverride(implementation, interfaceMethod);
        }

        private static void ImplementMethod(TypeBuilder tb, MethodInfo interfaceMethod)
        {
            var wcfOperationDescriptor = ReflectionHelper.GetUriTemplate(interfaceMethod);
            var implementation = tb.DefineMethod(interfaceMethod.Name, MethodAttributes.Public | MethodAttributes.Virtual, interfaceMethod.CallingConvention,
                                                 interfaceMethod.ReturnType, interfaceMethod.GetParameters().Select(x => x.ParameterType).ToArray());
            var il = implementation.GetILGenerator();
            var uriDict = il.DeclareLocal(typeof(Dictionary<string, object>));
            var bodyDict = il.DeclareLocal(typeof(Dictionary<string, object>));
            var dictionaryAdd = typeof(Dictionary<string, object>).GetRuntimeMethod("Add", new[] { typeof(string), typeof(object) });
            var request = il.DeclareLocal(ReflectionHelper.GetPropertyInterfaceImplementation<IRemoteRequest>().AsType());
            var reqMethod = AsyncRequestProcessorResolver.GetRequestMethod(interfaceMethod);
            il.Emit(OpCodes.Newobj, uriDict.LocalType.GetTypeInfo().DeclaredConstructors.First(x => x.GetParameters().Length == 0));
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Newobj, bodyDict.LocalType.GetTypeInfo().DeclaredConstructors.First(x => x.GetParameters().Length == 0));
            il.Emit(OpCodes.Stloc_1);
            var parameters = interfaceMethod.GetParameters();
            foreach (var parameter in parameters)
            {
                var dictToAdd = wcfOperationDescriptor.UriTemplate.Contains("{" + parameter.Name + "}") ? uriDict : bodyDict;
                il.EmitLdloc(dictToAdd.LocalIndex);
                il.Emit(OpCodes.Ldstr, parameter.Name);
                il.EmitLdarg(parameter.Position + 1);
                if (parameter.ParameterType.GetTypeInfo().IsValueType)
                {
                    il.Emit(OpCodes.Box, parameter.ParameterType);
                }
                il.Emit(OpCodes.Callvirt, dictionaryAdd);
            }
            il.Emit(OpCodes.Ldstr, wcfOperationDescriptor.UriTemplate);
            il.Emit(OpCodes.Ldstr, wcfOperationDescriptor.Method);
            il.Emit(wcfOperationDescriptor.RequestFormat == 0 ? OpCodes.Ldc_I4_0 : OpCodes.Ldc_I4_1);
            il.Emit(wcfOperationDescriptor.ResponseFormat == 0 ? OpCodes.Ldc_I4_0 : OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Newobj, typeof(RemoteOperationDescriptor).GetTypeInfo().DeclaredConstructors.Single());
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Newobj, request.LocalType.GetTypeInfo().DeclaredConstructors.Single(x => x.GetParameters().Length > 0));
            il.Emit(OpCodes.Stloc_2);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, typeof(AsyncClientBase).GetTypeInfo().DeclaredFields.Single());
            il.Emit(OpCodes.Ldloc_2);
            il.Emit(OpCodes.Callvirt, reqMethod);
            il.Emit(OpCodes.Ret);
            tb.DefineMethodOverride(implementation, interfaceMethod);
        }
    }
}
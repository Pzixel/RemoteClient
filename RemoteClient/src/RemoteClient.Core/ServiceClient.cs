using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
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
            return (T) Activator.CreateInstance(ClientType, processor);
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
            foreach (var interfaceMethod in typeInfo.GetAllMethods())
            {
                if (taskTypeInfo.IsAssignableFrom(interfaceMethod.ReturnType))
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
            var parameters = GetLamdaParameters(interfaceMethod);
            var expr = Expression.Lambda(Expression.Call(Expression.Field(parameters[0], "Processor"), interfaceMethod), parameters);
            var implementation = tb.DefineMethod(interfaceMethod.Name, MethodAttributes.Public | MethodAttributes.Virtual);
            implementation.GetILGenerator().Emit(OpCodes.Ret);
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
            var dictionaryAdd = typeof(Dictionary<string, object>).GetTypeInfo().GetMethod("Add");
            var request = il.DeclareLocal(ReflectionHelper.GetPropertyInterfaceImplementation<IRemoteRequest>().AsType());
            var reqMethod = GetRequestMethod(interfaceMethod);
            il.Emit(OpCodes.Newobj, uriDict.LocalType.GetTypeInfo().GetConstructor(Type.EmptyTypes));
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Newobj, bodyDict.LocalType.GetTypeInfo().GetConstructor(Type.EmptyTypes));
            il.Emit(OpCodes.Stloc_1);
            il.Emit(OpCodes.Ldloc_0);
            var parameters = interfaceMethod.GetParameters();
            foreach (var parameter in parameters)
            {
                var dictToAdd = wcfOperationDescriptor.UriTemplate.Contains("{" + parameter.Name + "}") ? uriDict : bodyDict;
                il.EmitLdloc(dictToAdd.LocalIndex);
                il.Emit(OpCodes.Ldstr, parameter.Name);
                il.EmitLdarg(parameter.Position + 1);
                if (parameter.ParameterType.GetTypeInfo().IsValueType)
                {
                    il.Emit(OpCodes.Box);
                }
                il.Emit(OpCodes.Callvirt, dictionaryAdd);
            }
            il.Emit(OpCodes.Ldstr, wcfOperationDescriptor.UriTemplate);
            il.Emit(OpCodes.Ldstr, wcfOperationDescriptor.Method);
            il.Emit(wcfOperationDescriptor.RequestFormat == 0 ? OpCodes.Ldc_I4_0 : OpCodes.Ldc_I4_1);
            il.Emit(wcfOperationDescriptor.ResponseFormat == 0 ? OpCodes.Ldc_I4_0 : OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Newobj, typeof(WcfOperationDescriptor).GetTypeInfo().DeclaredConstructors.Single());
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

        private static MethodInfo GetRequestMethod(MethodInfo interfaceMethod)
        {
            var type = typeof (IAsyncRequestProcessor).GetTypeInfo();
            return interfaceMethod.ReturnType.GenericTypeArguments.Length == 0 ? type.GetMethod("ExecuteAsync") : type.GetMethod("GetResultAsync").MakeGenericMethod(interfaceMethod.ReturnType.GenericTypeArguments);
        }

        private static ParameterExpression[] GetLamdaParameters(MethodInfo interfaceMethod)
        {
            var parameterInfos = interfaceMethod.GetParameters();
            var parameters = new ParameterExpression[parameterInfos.Length + 1];
            parameters[0] = Expression.Parameter(typeof(AsyncClientBase), "this");
            for (int i = 0; i < parameterInfos.Length; i++)
            {
                parameters[i + 1] = Expression.Parameter(parameterInfos[i].ParameterType, parameterInfos[i].Name);
            }
            return parameters;
        }

        private static Expression GetCreateDesriptorExpression(WcfOperationDescriptor descriptor)
        {
            return Expression.New(typeof(WcfOperationDescriptor).GetTypeInfo().DeclaredConstructors.First(),
                Expression.Constant(descriptor.UriTemplate),
                Expression.Constant(descriptor.Method),
                Expression.Constant(descriptor.RequestFormat),
                Expression.Constant(descriptor.ResponseFormat));
        }
    }
}
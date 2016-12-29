using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using WcfRestClient.Core.Helpers;

namespace WcfRestClient.Core
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
            if (!typeof(T).IsInterface)
                throw new Exception($"{typeName} is not an interface!");

            var tb = ReflectionHelper.Builder.DefineType($"<{typeName}>__Client", TypeAttributes.Class | TypeAttributes.Sealed);
            tb.SetParent(typeof(AsyncClientBase));
            tb.CreatePassThroughConstructors<AsyncClientBase>();
            tb.AddInterfaceImplementation(typeof(T));
            foreach (var interfaceMethod in typeof(T).GetAllMethods())
            {
                if (typeof(Task).IsAssignableFrom(interfaceMethod.ReturnType))
                {
                    ImplementMethod(tb, interfaceMethod);
                }
                else if (interfaceMethod == ReflectionHelper.DisposeMethod)
                {
                    ImplementProcessorProxyMethodCall(tb, interfaceMethod);
                }
                else
                {
                    throw new ClientGenerationException("Interface contains methods with non-task return type");
                }
            }

            return tb.CreateType();
        }

        private static void ImplementProcessorProxyMethodCall(TypeBuilder tb, MethodInfo interfaceMethod)
        {
            var parameters = GetLamdaParameters(interfaceMethod);
            var expr = Expression.Lambda(Expression.Call(Expression.Field(parameters[0], "Processor"), interfaceMethod), parameters);
            var implementation = expr.CompileToInstanceMethod(tb, interfaceMethod.Name, MethodAttributes.Public | MethodAttributes.Virtual);
            tb.DefineMethodOverride(implementation, interfaceMethod);
        }

        private static void ImplementMethod(TypeBuilder tb, MethodInfo interfaceMethod)
        {
            var wcfOperationDescriptor = ReflectionHelper.GetUriTemplate(interfaceMethod);
            var parameters = GetLamdaParameters(interfaceMethod);
            var newDict = Expression.New(typeof(Dictionary<string, object>));
            var uriDict = Expression.Variable(newDict.Type);
            var bodyDict = Expression.Variable(newDict.Type);
            var wcfRequest = Expression.Variable(typeof(IWcfRequest));
            var dictionaryAdd = newDict.Type.GetMethod("Add");

            var body = new List<Expression>(parameters.Length)
            {
                Expression.Assign(uriDict, newDict),
                Expression.Assign(bodyDict, newDict)
            };

            for (int i = 1; i < parameters.Length; i++)
            {
                var dictToAdd = wcfOperationDescriptor.UriTemplate.Contains("{" + parameters[i].Name + "}") ? uriDict : bodyDict;
                body.Add(Expression.Call(dictToAdd, dictionaryAdd, Expression.Constant(parameters[i].Name, typeof(string)),
                    Expression.Convert(parameters[i], typeof(object))));
            }

            body.Add(Expression.Assign(wcfRequest, Expression.Convert(GetCreateDesriptorExpression(wcfOperationDescriptor), wcfRequest.Type)));

            var requestMethod = GetRequestMethod(interfaceMethod);
            body.Add(Expression.Call(Expression.Field(parameters[0], "Processor"), requestMethod, wcfRequest));

            var wcfRequestType = ReflectionHelper.GetPropertyInterfaceImplementation<IWcfRequest>();
            var wcfProps = wcfRequestType.GetProperties();
            body.Add(Expression.MemberInit(Expression.New(wcfRequestType),
                Expression.Bind(Array.Find(wcfProps, info => info.Name == "Descriptor"), GetCreateDesriptorExpression(wcfOperationDescriptor)),
                Expression.Bind(Array.Find(wcfProps, info => info.Name == "QueryStringParameters"), Expression.Convert(uriDict, typeof(IReadOnlyDictionary<string, object>))),
                Expression.Bind(Array.Find(wcfProps, info => info.Name == "BodyPrameters"), Expression.Convert(bodyDict, typeof(IReadOnlyDictionary<string, object>)))));

            var bodyExpression = Expression.Lambda
                (
                    Expression.Block(new[] { uriDict, bodyDict, wcfRequest }, body.ToArray()),
                    parameters
                );

            var implementation = bodyExpression.CompileToInstanceMethod(tb, interfaceMethod.Name, MethodAttributes.Public | MethodAttributes.Virtual);
            tb.DefineMethodOverride(implementation, interfaceMethod);
        }

        private static MethodInfo GetRequestMethod(MethodInfo interfaceMethod)
        {
            var type = typeof (IAsyncRequestProcessor);
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
            return Expression.New(typeof(WcfOperationDescriptor).GetConstructors()[0],
                Expression.Constant(descriptor.UriTemplate),
                Expression.Constant(descriptor.Method),
                Expression.Constant(descriptor.BodyStyle),
                Expression.Constant(descriptor.RequestFormat),
                Expression.Constant(descriptor.ResponseFormat));
        }
    }
}
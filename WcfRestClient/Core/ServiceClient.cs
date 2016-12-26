using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using WcfRestClient.Helpers;

namespace WcfRestClient.Core
{
    /// <summary>
    /// Generate REST client for specified WCF service contract
    /// </summary>
    /// <typeparam name="T">Client interface. Must be IDisposable in order to properly clean <see cref="IAsyncRequestProcessor"/> parameter</typeparam>
    public static class ServiceClient<T> where T : IDisposable
    {
        private static readonly Type _clientType = CreateType();

        public static T New(IAsyncRequestProcessor processor)
        {
            return (T) Activator.CreateInstance(_clientType, processor);
        }

        private static Type CreateType()
        {
            var typeName = typeof(T).Name;
            if (!typeof(T).IsInterface)
                throw new Exception($"{typeName} is not an interface!");

            string typename = typeName.TrimStart('I') + "Generated";
            var tb = ReflectionHelper.Builder.DefineType(typename, TypeAttributes.Class | TypeAttributes.Sealed);
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
            var uriTemplate = ReflectionHelper.GetUriTemplate(interfaceMethod);
            var parameters = GetLamdaParameters(interfaceMethod);
            var newDict = Expression.New(typeof(Dictionary<string, object>));
            var uriDict = Expression.Variable(newDict.Type);
            var bodyDict = Expression.Variable(newDict.Type);
            var dictionaryAdd = newDict.Type.GetMethod("Add");

            var body = new List<Expression>(parameters.Length)
            {
                Expression.Assign(uriDict, newDict),
                Expression.Assign(bodyDict, newDict)
            };


            for (int i = 1; i < parameters.Length; i++)
            {
                var dictToAdd = uriTemplate.UriTemplate.Contains("{" + parameters[i].Name + "}") ? uriDict : bodyDict;
                body.Add(Expression.Call(dictToAdd, dictionaryAdd, Expression.Constant(parameters[i].Name, typeof(string)),
                    Expression.Convert(parameters[i], typeof(object))));
            }

            var requestMethod = GetRequestMethod(interfaceMethod);
            body.Add(Expression.Call(Expression.Field(parameters[0], "Processor"), requestMethod, Expression.Constant(uriTemplate.UriTemplate), Expression.Constant(uriTemplate.Method), uriDict, bodyDict));

            var bodyExpression = Expression.Lambda
                (
                    Expression.Block(new[] { uriDict, bodyDict }, body.ToArray()),
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
    }
}
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WcfRestClient.Helpers;

namespace WcfRestClient
{
    public static class ServiceClient<T>
    {
        private static readonly Type ClienType = CreateType();

        public static T New(IAsyncRequestProcessor processor)
        {
            return (T) Activator.CreateInstance(ClienType, processor);
        }

        private static Type CreateType()
        {
            var typeName = typeof(T).Name;
            if (!typeof(T).IsInterface)
                throw new Exception($"{typeName} is not an interface!");
            string typename = Regex.Replace(typeName, @"I(\w+)(Service)?", "$1Client");
            var tb = ReflectionHelper.Builder.DefineType(typename, TypeAttributes.Class | TypeAttributes.Sealed);
            tb.SetParent(typeof(AsyncClientBase));
            tb.CreatePassThroughConstructors<AsyncClientBase>();
            tb.AddInterfaceImplementation(typeof(T));
            foreach (var imethod in typeof(T).GetAllMethods())
            {
                if (!typeof(Task).IsAssignableFrom(imethod.ReturnType))
                    throw new ClientGenerationException("Interface contains methods with non-task return type");
                ImplementMethod(tb, imethod);
            }

            return tb.CreateType();
        }

        private static void ImplementMethod(TypeBuilder tb, MethodInfo imethod)
        {
            var valueString = ReflectionHelper.GetUriTemplate(imethod);
            var parameters = GetLamdaParameters(imethod);
            var newDict = Expression.New(typeof(Dictionary<string, object>));
            var local = Expression.Variable(newDict.Type, "foo");
            var dictionaryAdd = local.Type.GetMethod("Add");

            var body = new List<Expression>(parameters.Length)
            {
                Expression.Assign(local, newDict)
            };


            for (int i = 1; i < parameters.Length; i++)
            {
                body.Add(Expression.Call(local, dictionaryAdd, Expression.Constant(parameters[i].Name, typeof(string)),
                    Expression.Convert(parameters[i], typeof(object))));
            }

            var requestMethod = GetRequestMethod(imethod);
            body.Add(Expression.Call(Expression.Field(parameters[0], "Processor"), requestMethod, Expression.Constant(valueString.UriTemplate), Expression.Constant(valueString.Method), local));

            var bodyExpression = Expression.Lambda
                (
                    Expression.Block(new[] { local }, body.ToArray()),
                    parameters
                );

            var stub = bodyExpression.CompileToInstanceMethod(tb, imethod.Name, MethodAttributes.Public | MethodAttributes.Virtual);
            tb.DefineMethodOverride(stub, imethod);
        }

        private static MethodInfo GetRequestMethod(MethodInfo imethod)
        {
            var type = typeof (IAsyncRequestProcessor);
            return imethod.ReturnType.GenericTypeArguments.Length == 0 ? type.GetMethod("ExecuteAsync") : type.GetMethod("GetResultAsync").MakeGenericMethod(imethod.ReturnType.GenericTypeArguments);
        }

        private static ParameterExpression[] GetLamdaParameters(MethodInfo imethod)
        {
            var parameterInfos = imethod.GetParameters();
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
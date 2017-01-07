using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace WcfRestClient.Core.Helpers
{
    internal static class XLambdaExpression
    {
        public static MethodInfo CompileToInstanceMethod(this LambdaExpression expression, TypeBuilder tb, string methodName, MethodAttributes attributes)
        {
            var paramTypes = expression.Parameters.Select(x => x.Type).ToArray();
            var proxyParamTypes = new Type[paramTypes.Length - 1];
            Array.Copy(paramTypes, 1, proxyParamTypes, 0, proxyParamTypes.Length);
            var proxy = tb.DefineMethod(methodName, MethodAttributes.Public | MethodAttributes.Virtual, expression.ReturnType, proxyParamTypes);
            var method = tb.DefineMethod($"<{proxy.Name}>__StaticProxy", MethodAttributes.Private | MethodAttributes.Static, proxy.ReturnType, paramTypes);
            expression.CompileToMethod(method);

            proxy.GetILGenerator().EmitCallWithParams(method, paramTypes.Length);
            return proxy;
        }
    }
}
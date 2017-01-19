using System.Reflection;

namespace RemoteClient.Core.Helpers
{
    internal static class AsyncRequestProcessorResolver
    {
        private static readonly MethodInfo ExecuteAsync = typeof(IAsyncRequestProcessor).GetRuntimeMethod("ExecuteAsync", new[] {typeof(IRemoteRequest) });
        private static readonly MethodInfo GetResultAsync = typeof(IAsyncRequestProcessor).GetRuntimeMethod("GetResultAsync", new[] { typeof(IRemoteRequest) });

        public static MethodInfo GetRequestMethod(MethodInfo interfaceMethod)
        {
            return interfaceMethod.ReturnType.GenericTypeArguments.Length == 0 ? ExecuteAsync : GetResultAsync.MakeGenericMethod(interfaceMethod.ReturnType.GenericTypeArguments);
        } 
    }
}

namespace RemoteClient.Core.Helpers
{
    internal static class Empty<T>
    {
        public static T[] Array { get; } = new T[0];
    }
}

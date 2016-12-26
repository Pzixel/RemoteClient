using System;

namespace WcfRestClient.Helpers
{
    public static class UriExtension
    {
        private const char Slash = '/';
        public static Uri Concat(this Uri baseUri, string relativeUri)
        {
            if (string.IsNullOrEmpty(relativeUri))
            {
                return baseUri;
            }
            var baseUriString = baseUri.ToString();
            var shouldInsertSlash = baseUriString[baseUriString.Length - 1] != Slash && relativeUri[0] != Slash;
            var resultUri = shouldInsertSlash ? baseUriString + Slash + relativeUri : baseUriString + relativeUri;
            return new Uri(resultUri, baseUri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative);
        }

        public static Uri ToUri(this string source)
        {
            var kind = source.IndexOf("://", StringComparison.Ordinal) >= 0 ? UriKind.Absolute : UriKind.Relative;
            return new Uri(source, kind);
        }

        public static Uri WithSlash(this Uri uri)
        {
            string uriString = uri.ToString();
            if (uriString[uriString.Length - 1] == Slash)
                return uri;
            return new Uri(uriString + Slash);
        }
    }
}
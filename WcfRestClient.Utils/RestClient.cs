using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WcfRestClient.Utils
{
    public sealed class RestClient : IDisposable
    {
        private readonly HttpClient _client;
        private static long _requestId;

        public RestClient(Uri baseUri, TimeSpan? timeout = null)
        {
            _client = new HttpClient
            {
                BaseAddress = baseUri.WithSlash()
            };
            if (timeout.HasValue)
            {
                _client.Timeout = timeout.GetValueOrDefault();
            }
        }

        public Task<T> GetAsync<T>(Uri uri = null)
        {
            return SendAsync<T>(HttpMethod.Get, uri);
        }

        public Task PostAsync(object body, Uri uri = null)
        {
            return SendAsync(HttpMethod.Post, uri, body);
        }

        public Task<T> PostAsync<T>(object body, Uri uri = null)
        {
            return SendAsync<T>(HttpMethod.Post, uri, body);
        }

        public Task PutAsync(object body, Uri uri = null)
        {
            return SendAsync(HttpMethod.Put, uri, body);
        }

        public Task<T> PutAsync<T>(object body, Uri uri = null)
        {
            return SendAsync<T>(HttpMethod.Put, uri, body);
        }

        public Task DeleteAsync(Uri uri = null, object body = null)
        {
            return SendAsync(HttpMethod.Delete, uri, body);
        }

        public Task<T> DeleteAsync<T>(Uri uri = null, object body = null)
        {
            return SendAsync<T>(HttpMethod.Delete, uri, body);
        }

        public Task<T> SendAsync<T>(HttpMethod method, Uri uri = null, object body = null)
        {
            return SendAsync<T>(method, uri, body, true);
        }

        public Task SendAsync(HttpMethod method, Uri uri = null, object body = null)
        {
            return SendAsync<object>(method, uri, body, false);
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        private async Task<T> SendAsync<T>(HttpMethod method, Uri uri, object body, bool deserializeResponse)
        {
            long requestId = Interlocked.Increment(ref _requestId);
            Debug.WriteLine("Preparing to send request #{0}", requestId);
            try
            {
                var message = new HttpRequestMessage(method, uri);
                if (method != HttpMethod.Get)
                {
                    message.Content = ToRequestContent(body);
                }
                Debug.WriteLine("Sending request #{0}: {1} {2}{3}, content length: {4}",
                    requestId,
                    message.Method.Method,
                    _client.BaseAddress,
                    message.RequestUri,
                    message.Content?.Headers.ContentLength);
                var response = await _client.SendAsync(message).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    throw new BadStatusCodeException(response.StatusCode);
                }
                if (!deserializeResponse)
                {
                    return default(T);
                }
                var content = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                return NewtonsoftInterop.DeserializeFromBytes<T>(content);
            }
            catch (TaskCanceledException) // task is non-cancellable -> if task is cancelled then timeout occured
            {
                var ex = new TimeoutException($"Specified timeout {_client.Timeout.TotalMilliseconds}ms expired");
                Debug.WriteLine("Timeout error in request #{0}, request url '{1}/{2}' : {3}", requestId, _client.BaseAddress, uri, ex);
                throw ex;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error in request #{0}, request url '{1}/{2}' : {3}", requestId, _client.BaseAddress, uri, ex);
                throw;
            }
            finally
            {
                Debug.WriteLine("Request #{0} finished", requestId);
            }
        }

        private static HttpContent ToRequestContent(object body)
        {
            var data = body != null ? NewtonsoftInterop.SerializeToBytes(body) : new byte[0];
            return new ByteArrayContent(data);
        }
    }
}

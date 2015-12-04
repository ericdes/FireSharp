using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FireSharp.Config;
using FireSharp.EventStreaming;
using FireSharp.Exceptions;
using FireSharp.Interfaces;
using FireSharp.Response;

namespace FireSharp
{
    public class FirebaseClient : IFirebaseClient, IDisposable
    {
        public ISerializer Serializer { get; private set; }
        private readonly IRequestManager _requestManager;

        private readonly Action<HttpStatusCode, string> _defaultErrorHandler = (statusCode, body) =>
        {
            if (statusCode < HttpStatusCode.OK || statusCode >= HttpStatusCode.BadRequest)
            {
                throw new FirebaseException(statusCode, body);
            }
        };


        public FirebaseClient(IFirebaseConfig config)
        {
            this.Serializer = config.Serializer;
            _requestManager = new RequestManager(config);
        }

        /// <summary>
        /// For testing purposes only (Mock)
        /// </summary>
        /// <param name="requestManager"></param>
        public FirebaseClient(IRequestManager requestManager, IFirebaseConfig config)
        {
            this.Serializer = config.Serializer;
            _requestManager = requestManager;
        }

        public void Dispose()
        {
            using (_requestManager) { }
        }

        public FirebaseResponse Get(string path)
        {
            try
            {
                HttpResponseMessage response = _requestManager.RequestAsync(HttpMethod.Get, path).Result;
                string content = response.Content.ReadAsStringAsync().Result;
                HandleIfErrorResponse(response.StatusCode, content);
                return new FirebaseResponse(this.Serializer, content, response.StatusCode);
            }
            catch (HttpRequestException ex)
            {
                throw new FirebaseException(ex);
            }
        }

        public FirebaseResponse<T> Get<T>(string path)
        {
            try
            {
                HttpResponseMessage response = _requestManager.RequestAsync(HttpMethod.Get, path).Result;
                string content = response.Content.ReadAsStringAsync().Result;
                HandleIfErrorResponse(response.StatusCode, content);
                var firebaseresponse = new FirebaseResponse<T>(this.Serializer, content, response.StatusCode);
                return firebaseresponse;
            }
            catch (HttpRequestException ex)
            {
                throw new FirebaseException(ex);
            }
        }

        public SetResponse<T> Set<T>(string path, T data)
        {
            try
            {
                HttpResponseMessage response = _requestManager.RequestAsync(HttpMethod.Put, path, data).Result;
                string content = response.Content.ReadAsStringAsync().Result;
                HandleIfErrorResponse(response.StatusCode, content);
                return new SetResponse<T>(this.Serializer, content, response.StatusCode);
            }
            catch (HttpRequestException ex)
            {
                throw new FirebaseException(ex);
            }
        }

        public PushResponse Push<T>(string path, T data)
        {
            try
            {
                HttpResponseMessage response = _requestManager.RequestAsync<T>(HttpMethod.Post, path, data).Result;
                string content = response.Content.ReadAsStringAsync().Result;
                HandleIfErrorResponse(response.StatusCode, content);
                return new PushResponse(this.Serializer, content, response.StatusCode);
            }
            catch (HttpRequestException ex)
            {
                throw new FirebaseException(ex);
            }
        }
        public PushResponse Push(string path, object data)
        {
            try
            {
                HttpResponseMessage response = _requestManager.RequestAsync(HttpMethod.Post, path, data).Result;
                string content = response.Content.ReadAsStringAsync().Result;
                HandleIfErrorResponse(response.StatusCode, content);
                return new PushResponse(this.Serializer, content, response.StatusCode);
            }
            catch (HttpRequestException ex)
            {
                throw new FirebaseException(ex);
            }
        }

        public FirebaseResponse Delete(string path)
        {
            try
            {
                HttpResponseMessage response = _requestManager.RequestAsync(HttpMethod.Delete, path).Result;
                string content = response.Content.ReadAsStringAsync().Result;
                HandleIfErrorResponse(response.StatusCode, content);
                return new FirebaseResponse(this.Serializer, content, response.StatusCode);
            }
            catch (HttpRequestException ex)
            {
                throw new FirebaseException(ex);
            }
        }

        public FirebaseResponse<T> Update<T>(string path, T data)
        {
            try
            {
                HttpResponseMessage response = _requestManager.RequestAsync(RequestManager.Patch, path, data).Result;
                string content = response.Content.ReadAsStringAsync().Result;
                HandleIfErrorResponse(response.StatusCode, content);
                return new FirebaseResponse<T>(this.Serializer, content, response.StatusCode);
            }
            catch (HttpRequestException ex)
            {
                throw new FirebaseException(ex);
            }
        }

        public async Task<FirebaseResponse> GetAsync(string path)
        {
            try
            {
                HttpResponseMessage response = await _requestManager.RequestAsync(HttpMethod.Get, path).ConfigureAwait(false);
                string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                HandleIfErrorResponse(response.StatusCode, content);
                return new FirebaseResponse(this.Serializer, content, response.StatusCode);
            }
            catch (HttpRequestException ex)
            {
                throw new FirebaseException(ex);
            }
        }


        public async Task<FirebaseResponse<T>> GetAsync<T>(string path)
        {
            try
            {
                HttpResponseMessage response = await _requestManager.RequestAsync(HttpMethod.Get, path).ConfigureAwait(false);
                string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                HandleIfErrorResponse(response.StatusCode, content);
                return new FirebaseResponse<T>(this.Serializer, content, response.StatusCode);
            }
            catch (HttpRequestException ex)
            {
                throw new FirebaseException(ex);
            }
        }

        public async Task<SetResponse<T>> SetAsync<T>(string path, T data)
        {
            try
            {
                HttpResponseMessage response = await _requestManager.RequestAsync(HttpMethod.Put, path, data).ConfigureAwait(false);
                string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                HandleIfErrorResponse(response.StatusCode, content);
                return new SetResponse<T>(this.Serializer, content, response.StatusCode);
            }
            catch (HttpRequestException ex)
            {
                throw new FirebaseException(ex);
            }
        }

        public async Task<PushResponse> PushAsync<T>(string path, T data)
        {
            try
            {
                HttpResponseMessage response = await _requestManager.RequestAsync<T>(HttpMethod.Post, path, data).ConfigureAwait(false);
                string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                HandleIfErrorResponse(response.StatusCode, content);
                return new PushResponse(this.Serializer, content, response.StatusCode);
            }
            catch (HttpRequestException ex)
            {
                throw new FirebaseException(ex);
            }
        }


        public async Task<PushResponse> PushAsync(string path, object data)
        {
            try
            {
                HttpResponseMessage response = await _requestManager.RequestAsync(HttpMethod.Post, path, data).ConfigureAwait(false);
                string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                HandleIfErrorResponse(response.StatusCode, content);
                return new PushResponse(this.Serializer, content, response.StatusCode);
            }
            catch (HttpRequestException ex)
            {
                throw new FirebaseException(ex);
            }
        }

        public async Task<FirebaseResponse> DeleteAsync(string path)
        {
            try
            {
                HttpResponseMessage response = await _requestManager.RequestAsync(HttpMethod.Delete, path).ConfigureAwait(false);
                string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                HandleIfErrorResponse(response.StatusCode, content);
                return new FirebaseResponse(this.Serializer, content, response.StatusCode);
            }
            catch (HttpRequestException ex)
            {
                throw new FirebaseException(ex);
            }
        }

        public async Task<FirebaseResponse<T>> UpdateAsync<T>(string path, T data)
        {
            try
            {
                HttpResponseMessage response = await _requestManager.RequestAsync(RequestManager.Patch, path, data).ConfigureAwait(false);
                string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                HandleIfErrorResponse(response.StatusCode, content);
                return new FirebaseResponse<T>(this.Serializer, content, response.StatusCode);
            }
            catch (HttpRequestException ex)
            {
                throw new FirebaseException(ex);
            }
        }

        [Obsolete("This method is obsolete use OnAsync instead.")]
        public async Task<EventStreamResponse> ListenAsync(string path, ValueAddedEventHandler added = null,
            ValueChangedEventHandler changed = null,
            ValueRemovedEventHandler removed = null)
        {
            return new EventStreamResponse(await _requestManager.ListenAsync(path).ConfigureAwait(false), added, changed, removed);
        }

        public async Task<EventRootResponse<T>> OnChangeGetAsync<T>(string path, ValueRootAddedEventHandler<T> added = null)
        {
            return new EventRootResponse<T>(await _requestManager.ListenAsync(path).ConfigureAwait(false), added, this, _requestManager, path);
        }

        public async Task<EventStreamResponse> OnAsync(string path, ValueAddedEventHandler added = null, ValueChangedEventHandler changed = null,
            ValueRemovedEventHandler removed = null, ValueMovedEventHandler moved = null)
        {
            return new EventStreamResponse(await _requestManager.ListenAsync(path).ConfigureAwait(false), added, changed, removed, moved);
        }

        private void HandleIfErrorResponse(HttpStatusCode statusCode, string content, Action<HttpStatusCode, string> errorHandler = null)
        {
            if (errorHandler != null)
            {
                errorHandler(statusCode, content);
            }
            else
            {
                _defaultErrorHandler(statusCode, content);
            }
        }
    }
}
﻿using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using FireSharp.Config;
using FireSharp.Exceptions;
using FireSharp.Interfaces;

namespace FireSharp
{
    public class RequestManager : IRequestManager
    {
        public static readonly HttpMethod Patch = new HttpMethod("PATCH");

        private readonly IFirebaseConfig _config;

        internal RequestManager(IFirebaseConfig config)
        {
            _config = config;
        }

        private HttpClient GetClient(HttpClientHandler handler = null)
        {
            var client = handler == null ? new HttpClient() : new HttpClient(handler, true);

            var basePath = _config.BasePath.EndsWith("/") ? _config.BasePath : _config.BasePath + "/";
            client.BaseAddress = new Uri(basePath);

            if (_config.RequestTimeout.HasValue)
            {
                client.Timeout = _config.RequestTimeout.Value;
            }

            return client;
        }

        public void Dispose()
        {
        }

        public async Task<HttpResponseMessage> ListenAsync(string path)
        {
            HttpRequestMessage request;
            var client = PrepareEventStreamRequest(path, out request);

            var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            return response;
        }

        public Task<HttpResponseMessage> RequestAsync(string jsonData, string path, HttpMethod method)
        {
            try
            {
                var request = PrepareRequest(jsonData, path, method);
                return GetClient().SendAsync(request, HttpCompletionOption.ResponseContentRead);
            }
            catch (Exception ex)
            {
                throw new FirebaseException(
                    string.Format("An error occured while execute request. Path : {0} , Method : {1}", path, method), ex);
            }
        }
        public Task<HttpResponseMessage> RequestAsync(HttpMethod method, string path, object payload = null)
        {
            try
            {
                var request = PrepareRequest(method, path, payload);
                var httpResponse = GetClient().SendAsync(request, HttpCompletionOption.ResponseContentRead);
                return httpResponse;
            }
            catch (Exception ex)
            {
                throw new FirebaseException(
                    string.Format("An error occured while execute request. Path : {0} , Method : {1}", path, method), ex);
            }
        }
        public Task<HttpResponseMessage> RequestAsync<T>(HttpMethod method, string path, T payload)
        {
            try
            {
                var request = PrepareRequest<T>(method, path, payload);
                return GetClient().SendAsync(request, HttpCompletionOption.ResponseContentRead);
            }
            catch (Exception ex)
            {
                throw new FirebaseException(
                    string.Format("An error occured while execute request. Path : {0} , Method : {1}", path, method), ex);
            }
        }


        private HttpClient PrepareEventStreamRequest(string path, out HttpRequestMessage request)
        {
            var client = GetClient(new HttpClientHandler { AllowAutoRedirect = true });
            var uri = PrepareUri(path);

            request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

            return client;
        }


        private HttpRequestMessage PrepareRequest(string jsonData, string path, HttpMethod method)
        {
            var uri = PrepareUri(path);
            var request = new HttpRequestMessage(method, uri);
            if (jsonData != null)
            {
                request.Content = new StringContent(jsonData);
            }
            return request;
        }
        private HttpRequestMessage PrepareRequest(HttpMethod method, string path, object payload)
        {
            string json = null;
            if (payload != null)
            {
                json = _config.Serializer.Serialize(payload);
            }
            var request = PrepareRequest(json, path, method);
            return request;
        }
        private HttpRequestMessage PrepareRequest<T>(HttpMethod method, string path, T payload)
        {
            string json = null;
            if (payload != null)
            {
                json = _config.Serializer.Serialize<T>(payload);
            }
            var request = PrepareRequest(json, path, method);
            return request;
        }


        private Uri PrepareUri(string path)
        {
            var authToken = !string.IsNullOrWhiteSpace(_config.AuthSecret)
                ? string.Format("{0}.json?auth={1}", path.Trim(new[] { '/' }), _config.AuthSecret)
                : string.Format("{0}.json", path);

            var url = _config.BasePath.TrimEnd(new[] { '/' }) + "/" + authToken;

            return new Uri(url);
        }
    }
}
using System.Net;
using System.Net.Http;
using FireSharp.Interfaces;

namespace FireSharp.Response
{
    public class FirebaseResponse<T>
    {
        protected ISerializer _serializer;
        private readonly HttpStatusCode _statusCode;

        protected readonly HttpResponseMessage HttpResponse;
        private readonly string _body;

        public FirebaseResponse(ISerializer serializer, string body, HttpStatusCode statusCode, HttpResponseMessage httpResponse)
        {
            _serializer = serializer;
            _statusCode = statusCode;
            _body = body;
            HttpResponse = httpResponse;
        }

        public FirebaseResponse(ISerializer serializer, string body, HttpStatusCode statusCode)
        {
            _serializer = serializer;
            _statusCode = statusCode;
            _body = body;
        }

        public T Result
        {
            get
            {
                return _serializer.Deserialize<T>(this.Body);
            }
        }

        public string Body
        {
            get { return _body; }
        }

        public HttpStatusCode StatusCode
        {
            get { return _statusCode; }
        }
    }

    public class FirebaseResponse
    {
        private ISerializer _serializer;
        private readonly HttpStatusCode _statusCode;

        protected readonly HttpResponseMessage HttpResponse;
        private readonly string _body;

        public FirebaseResponse(ISerializer serializer, string body, HttpStatusCode statusCode, HttpResponseMessage httpResponse)
        {
            _serializer = serializer;
            _statusCode = statusCode;
            _body = body;
            HttpResponse = httpResponse;
        }

        public FirebaseResponse(ISerializer serializer, string body, HttpStatusCode statusCode)
        {
            _serializer = serializer;
            _statusCode = statusCode;
            _body = body;
        }

        //public dynamic Response
        //{
        //    get
        //    {
        //        return _serializer.Deserialize<dynamic>(this.Body);
        //    }
        //}


        public string Body
        {
            get { return _body; }
        }

        public HttpStatusCode StatusCode
        {
            get { return _statusCode; }
        }
    }
}

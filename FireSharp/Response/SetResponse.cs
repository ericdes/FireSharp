using System.Net;
using System.Net.Http;
using FireSharp.Interfaces;

namespace FireSharp.Response
{
    public class SetResponse<T> : FirebaseResponse<T>
    {
        public SetResponse(ISerializer serializer, string body, HttpStatusCode statusCode, HttpResponseMessage httpResponse)
            : base(serializer, body, statusCode, httpResponse)
        {
        }

        public SetResponse(ISerializer serializer, string body, HttpStatusCode statusCode)
            : base(serializer, body, statusCode)
        {
        }
    }
}
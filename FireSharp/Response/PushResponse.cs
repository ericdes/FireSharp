using System.Net;
using System.Net.Http;
using FireSharp.Interfaces;

namespace FireSharp.Response
{
    public class PushResponse : FirebaseResponse<PushResponse>
    {
        public PushResponse(ISerializer serializer, string body, HttpStatusCode statusCode, HttpResponseMessage httpResponse)
            : base(serializer, body, statusCode, httpResponse)
        {
        }

        public PushResponse(ISerializer serializer, string body, HttpStatusCode statusCode)
            : base(serializer, body, statusCode)
        {
        }

        public new PushResult Result
        {
            get
            {
                return _serializer.Deserialize<PushResult>(this.Body);
            }
        }
    }

    public class PushResult
    {
        public string Name { get; set; }
    }

}
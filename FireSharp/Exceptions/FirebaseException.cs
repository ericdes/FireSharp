using System;
using System.Net;

namespace FireSharp.Exceptions
{
    public class FirebaseException : Exception
    {
        public FirebaseException(string message)
            : base(message)
        {
        }

        public FirebaseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public FirebaseException(Exception innerException)
            : base(innerException.Message, innerException)
        {
        }

        public FirebaseException(HttpStatusCode statusCode, string responseBody)
            : base(string.Format("Request responded with status code={0}, response={1}", statusCode, responseBody))
        {

        }
    }

    public class FirebaseEventStreamingException : FirebaseException
    {
        public string EventName { get; private set; }
        public string FirebaseData { get; private set; }

        public FirebaseEventStreamingException(string eventName, string firebaseData, string message)
            : base(message)
        {
            this.EventName = eventName;
            this.FirebaseData = firebaseData;
        }

    }

}
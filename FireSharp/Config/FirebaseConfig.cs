using System;
using FireSharp.Interfaces;

namespace FireSharp.Config
{
    public class FirebaseConfig : IFirebaseConfig
    {
        public string BasePath { get; set; }
        public string AuthSecret { get; set; }
        public TimeSpan? RequestTimeout { get; set; }

        /// <summary>
        /// Method to serialize object to JSON format.
        /// </summary>
        public Func<object, string> JsonSerializer { get; set; }

        /// <summary>
        /// Method to deserialize from JSON format.
        /// </summary>
        public Func<string, object> JsonDeserializer { get; set; }
    }
}

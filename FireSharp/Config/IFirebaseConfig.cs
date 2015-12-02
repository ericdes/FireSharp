using System;
using FireSharp.Interfaces;

namespace FireSharp.Config
{
    public interface IFirebaseConfig
    {
        string BasePath { get; set; }
        string AuthSecret { get; set; }
        TimeSpan? RequestTimeout { get; set; }

        /// <summary>
        /// Method to serialize object to JSON format.
        /// </summary>        
        Func<object, string> JsonSerializer { get; set; }

        /// <summary>
        /// Method to deserialize from JSON format.
        /// </summary>
        Func<string, object> JsonDeserializer { get; set; }
    }
}
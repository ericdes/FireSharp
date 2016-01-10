using System;
using FireSharp.Interfaces;

namespace FireSharp.Config
{
    public interface IFirebaseConfig
    {
        string BasePath { get; }
        string AuthSecret { get; }
        TimeSpan? RequestTimeout { get; }
        ISerializer Serializer { get; }
    }
}
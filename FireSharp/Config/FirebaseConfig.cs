﻿using System;
using FireSharp.Interfaces;

namespace FireSharp.Config
{
    public class FirebaseConfig : IFirebaseConfig
    {
        public string BasePath { get; set; }
        public string AuthSecret { get; set; }
        public TimeSpan? RequestTimeout { get; set; }
        public ISerializer Serializer { get; set; }
    }
}

using System;

namespace FireSharp.EventStreaming
{
    public class ValueMovedEventArgs : EventArgs
    {
        public ValueMovedEventArgs(string path, string data, string oldData)
        {
            Path = path;
            Data = data;
            OldData = oldData;
        }

        public string Path { get; private set; }
        public string Data { get; private set; }
        public string OldData { get; private set; }
    }
}
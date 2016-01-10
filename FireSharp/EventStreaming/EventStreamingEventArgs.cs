using System;
using FireSharp.Response;

namespace FireSharp.EventStreaming
{
    public class EventStreamingEventArgs : EventArgs
    {
        public string Read { get; private set; }

        /// <param name="read">Stream as line read</param>
        public EventStreamingEventArgs(string read)
        {
            this.Read = read;
        }

    }

    public class EventStreamingResponseRawEventArgs : EventArgs
    {
        public string EventName { get; private set; }
        public string DataLine { get; private set; }

        public EventStreamingResponseRawEventArgs(string eventName, string dataLine)
        {
            this.EventName = eventName;
            this.DataLine = dataLine;
        }

    }

    public class EventStreamingResponseEventArgs : EventArgs
    {
        public EventStreamingResponse EventStreamingResponse { get; private set; }

        public EventStreamingResponseEventArgs(EventStreamingResponse eventStreamingResponse)
        {
            this.EventStreamingResponse = eventStreamingResponse;
        }

    }

    public class EventStreamingResponseEventArgs<T> : EventStreamingResponseEventArgs
    {

        public EventStreamingResponseEventArgs(EventStreamingResponse eventStreamingResponse)
            : base(eventStreamingResponse)
        {

        }
        

    }

}
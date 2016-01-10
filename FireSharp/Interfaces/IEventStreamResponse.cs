using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

using FireSharp.EventStreaming;

namespace FireSharp.Interfaces
{
    public interface IEventStreamingResponseData
    {
        string Path { get; }
        string Data { get; }
    }

    public interface IEventStreamResponse<T>
    {
        event ObjectPatchedEventHandler<T> OnObjectPatched;
    }

    public interface IEventStreamingResponse
    {
        string EventName { get; }

        /// <summary>
        /// Reponse element that contains path / data Firebase streaming response
        /// </summary>
        IEventStreamingResponseData Data { get; }
    }
}

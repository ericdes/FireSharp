using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

using FireSharp.EventStreaming;
using System.Threading;

namespace FireSharp.Interfaces
{
    public interface IEventStreamingResponseData
    {
        string Path { get; }
        string Data { get; }
    }

    public interface IEventStreamObserver
    {
        CancellationTokenSource CancellationToken { get; }
        Action<Exception> ExceptionHandler { get; }
    }
    public interface IEventStreamObserver<T> : IEventStreamObserver
    {
        event ObjectPropertyPatchReceivedEventHandler<T> OnObjectPropertyPatchReceived;
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

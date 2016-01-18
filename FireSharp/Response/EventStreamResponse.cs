using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using FireSharp.EventStreaming;
using Newtonsoft.Json;
using FireSharp.Interfaces;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using FireSharp.Exceptions;

namespace FireSharp.Response
{

    public class EventStreamingResponseData : IEventStreamingResponseData
    {
        /// <summary>
        /// Response element that contains the path related to the data received.
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// Reponse element that contains the data.
        /// </summary>
        public JToken DataToken { get; private set; }

        /// <summary>
        /// Reponse element that contains the data in JSON format.
        /// </summary>
        public string Data
        {
            get { return this.DataToken.ToString(Formatting.None); }
        }

        public EventStreamingResponseData(string path, JToken dataToken)
        {
            this.Path = path;
            this.DataToken = dataToken;
        }
    }

    public class EventStreamingResponse : IEventStreamingResponse
    {
        public string EventName { get; private set; }
        public IEventStreamingResponseData Data { get; private set; }

        public EventStreamingResponse(string eventName, IEventStreamingResponseData data)
        {
            this.EventName = eventName;
            this.Data = data;
        }
    }


    public class EventStreamResponse<T> : IEventStreamObserver<T>
    {
        private static bool EXTRA_DEBUG = false;
        private readonly ISerializer _serializer;
        private readonly JsonPatcher _jsonPatchManager;
        private readonly FireCache _fireCache;
        private readonly TemporaryCache _cache;
        private readonly Task _pollingTask;

        public CancellationTokenSource CancellationToken { get; private set; }

        #region Events
        public event EventStreamingEventHandler<T> OnEventStreaming;
        public event EventStreamingResponseRawEventHandler<T> OnEventStreamingResponseRaw;
        public event EventStreamingResponseEventHandler<T> OnEventStreamingResponse;
        public event ObjectPropertyPatchReceivedEventHandler<T> OnObjectPropertyPatchReceived;
        public event ObjectRootPatchReceivedEventHandler<T> OnObjectRootPatchReceived;
        public Action<Exception> ExceptionHandler { get; private set; }
        #endregion


        internal EventStreamResponse(ISerializer serializer, HttpResponseMessage httpResponse,
            ObjectRootPatchReceivedEventHandler<T> rootPatchReceived = null,
            ObjectPropertyPatchReceivedEventHandler<T> propertyPatchReceived = null,
            EventStreamingResponseEventHandler<T> eventStreamingResponse = null,
            EventStreamingResponseRawEventHandler<T> eventStreamingResponseRaw = null,
            EventStreamingEventHandler<T> eventStreaming = null,
            Action<Exception> exceptionHandler = null)
        {
            _serializer = serializer;
            _fireCache = new FireCache();
            _cache = new TemporaryCache();

            this.CancellationToken = new CancellationTokenSource();
            this.ExceptionHandler = exceptionHandler;

            if (rootPatchReceived != null)
            {
                this.OnObjectRootPatchReceived += rootPatchReceived;
            }
            if (propertyPatchReceived != null)
            {
                this.OnObjectPropertyPatchReceived += propertyPatchReceived;
                _jsonPatchManager = new JsonPatcher(_serializer);
            }
            if (eventStreamingResponse != null)
            {
                this.OnEventStreamingResponse += eventStreamingResponse;
            }
            if (eventStreamingResponseRaw != null)
            {
                this.OnEventStreamingResponseRaw += eventStreamingResponseRaw;
            }
            if (eventStreaming != null)
            {
                this.OnEventStreaming += eventStreaming;
            }
            _pollingTask = ReadLoop(httpResponse, CancellationToken.Token);
        }


        private async Task ReadLoop(HttpResponseMessage httpResponse, CancellationToken token)
        {
            try
            {
                await Task.Run(async () =>
                {
                    using (httpResponse)
                    {
                        using (var content = await httpResponse.Content.ReadAsStreamAsync())
                        {
                            using (var sr = new StreamReader(content))
                            {
                                bool? isFirstTimeRetrieval = null;
                                #region ----- While true -----
                                while (true)
                                {
                                    CancellationToken.Token.ThrowIfCancellationRequested();

                                    string line;
                                    #region ----- Read line
                                    line = await sr.ReadLineAsync();
                                    if (string.IsNullOrEmpty(line)) continue;
                                    if (OnEventStreaming != null)
                                    {
                                        OnEventStreaming(this, new EventStreamingEventArgs(line));
                                    }
                                    #endregion

                                    string eventName;
                                    string dataLine;
                                    #region ----- Read event name / data line
                                    if (!line.StartsWith("event: "))
                                    {
                                        var message = string.Format("Expecting line to start with 'event: ', line: {0}", line);
                                        Debug.WriteLine("[StreamingResponse.Exception] " + message);
                                        throw new FirebaseEventStreamingException(null, null, message);
                                    }
                                    eventName = line.Substring(7);
                                    dataLine = await sr.ReadLineAsync();
                                    if (OnEventStreamingResponseRaw != null)
                                    {
                                        OnEventStreamingResponseRaw(this, new EventStreamingResponseRawEventArgs(eventName, dataLine));
                                    }
                                    if (EXTRA_DEBUG) Debug.WriteLine(string.Format("[StreamingResponse.Content] {0} | {1}", line, dataLine));
                                    #endregion

                                    if (eventName == "keep-alive")
                                    {

                                    }
                                    else if (eventName == "put" || eventName == "patch")
                                    {
                                        if (dataLine.StartsWith("data: "))
                                        {
                                            if (OnEventStreamingResponse == null
                                                && OnObjectRootPatchReceived == null
                                                && OnObjectPropertyPatchReceived == null) return; // Not watching

                                            #region ----- Read path / data
                                            var dataRecord = dataLine.Substring(6);
                                            var jData = JObject.Parse(dataRecord);

                                            var pathToken = jData["path"];
                                            if (pathToken == null)
                                            {
                                                var message = "Firebase stream response should contain a token 'path'.";
                                                Debug.WriteLine("[StreamingResponse.##### WARNING #####] " + message);
                                                throw new FirebaseEventStreamingException(eventName, dataRecord, message);
                                            }
                                            if (pathToken.Type != JTokenType.String)
                                            {
                                                var message = "Path token received in Firebase stream response is not of type String.";
                                                Debug.WriteLine("[StreamingResponse.##### WARNING #####] " + message);
                                                throw new FirebaseEventStreamingException(eventName, dataRecord, message);
                                            }

                                            JToken dataToken = jData["data"];
                                            if (dataToken == null)
                                            {
                                                var message = "Firebase stream response should contain a token 'data'.";
                                                Debug.WriteLine("[StreamingResponse.##### WARNING #####] " + message);
                                                throw new FirebaseEventStreamingException(eventName, dataRecord, message);
                                            }

                                            var responseData = new EventStreamingResponseData((string)pathToken, dataToken);
                                            var jsonDataBeginning = responseData.Data.Substring(0, Math.Min(50, responseData.Data.Length)) + (responseData.Data.Length > 50 ? "..." : "");
                                            Debug.WriteLine(string.Format("[StreamingResponse.{0}] {1}", eventName, responseData.Path));
                                            Debug.WriteLine(string.Format("      ---> {0} <---", jsonDataBeginning));

                                            var response = new EventStreamingResponse(eventName, responseData);
                                            if (OnEventStreamingResponse != null)
                                            {
                                                OnEventStreamingResponse(this, new EventStreamingResponseEventArgs<T>(response));
                                            }
                                            #endregion

                                            #region ----- Deal with object data received
                                            var jsonPatch = _jsonPatchManager.GeneratePatchFrom(response);
                                            if (responseData.Path == "/" && eventName == "put")
                                            {
                                                isFirstTimeRetrieval = isFirstTimeRetrieval == null ? true : false;
                                                if (OnObjectRootPatchReceived != null)
                                                {
                                                    // var obj = _serializer.Deserialize<T>(responseData.Data);
                                                    OnObjectRootPatchReceived(this, jsonPatch, isFirstTimeRetrieval.Value);
                                                }
                                            }
                                            else if (OnObjectPropertyPatchReceived != null)
                                            {
                                                if (isFirstTimeRetrieval == null)
                                                {
                                                    var message = "Firebase streaming did not emit first time retrieval of object.";
                                                    Debug.WriteLine("[StreamingResponse.##### WARNING #####] " + message);
                                                    throw new FirebaseEventStreamingException(eventName, dataRecord, message);
                                                }
                                                OnObjectPropertyPatchReceived(this, jsonPatch);
                                            }
                                            #endregion
                                        }
                                        }
                                        else
                                        {
                                            Debug.WriteLine("[StreamingResponse.Exception] " + string.Format("Firebase stream response not handled. Event: {0} | Data: {1}", eventName, dataLine));
                                            throw new FirebaseEventStreamingException(eventName, dataLine, "Firebase stream response not handled.");
                                        }
                                    }
                                #endregion ----- While true (end)
                            } // Using sr = StreamReader
                        } // Using content
                    } // Using httpResponse 
                }, cancellationToken: token); // Task.Run
            }
            catch(Exception exceptionInLongRunningTask)
            {
                if (this.ExceptionHandler == null) throw;
                this.ExceptionHandler(exceptionInLongRunningTask);
            }
        }


        public async void Cancel()
        {
            CancellationToken.Cancel();
            Debug.WriteLine("[StreamingResponse.Warning] Loop stopped.");
        }


        public void Dispose()
        {
            Cancel();
            using (CancellationToken) // Dispose _cancel
            {
            }
        }
    }
}
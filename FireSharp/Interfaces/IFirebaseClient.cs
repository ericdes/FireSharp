using System;
using System.Threading.Tasks;
using FireSharp.EventStreaming;
using FireSharp.Response;

namespace FireSharp.Interfaces
{
    public interface IFirebaseClient
    {
        ISerializer Serializer { get; }
        JsonPatcher JsonPatcher { get; }

        /// <summary>
        /// Apply a JSON patch to a firebase object
        /// </summary>
        /// <param name="objectPath">Path to object.</param>
        /// <param name="patch">Patch to apply to object</param>
        /// <returns></returns>
        FirebaseResponse ApplyJsonPatch(string objectPath, JsonPatch patch);

        /// <summary>
        /// Apply a JSON patch to a firebase object
        /// </summary>
        /// <param name="objectPath">Path to object.</param>
        /// <param name="patch">Patch to apply to object</param>
        /// <returns></returns>
        Task<FirebaseResponse> ApplyJsonPatchAsync(string objectPath, JsonPatch patch);

        FirebaseResponse Get(string path, Action<Exception, string> exceptionHandler = null);
        FirebaseResponse<T> Get<T>(string path);
        Task<FirebaseResponse> GetAsync(string path);
        Task<FirebaseResponse<T>> GetAsync<T>(string path);

        Task<EventRootResponse<T>> OnChangeGetAsync<T>(string path, ValueChangedEventHandler<T> added = null);

        SetResponse Set(string path, object data = null, string jsonData = null);
        SetResponse<T> Set<T>(string path, T data);
        Task<SetResponse> SetAsync(string path, object data = null, string jsonData = null);
        Task<SetResponse<T>> SetAsync<T>(string path, T data);

        PushResponse Push(string path, object data = null, string jsonData = null);
        PushResponse Push<T>(string path, T data);
        Task<PushResponse> PushAsync(string path, object data = null, string jsonData = null);
        Task<PushResponse> PushAsync<T>(string path, T data);

        FirebaseResponse Delete(string path);
        Task<FirebaseResponse> DeleteAsync(string path);

        FirebaseResponse Update(string path, object data = null, string jsonData = null);
        FirebaseResponse<T> Update<T>(string path, T data);
        Task<FirebaseResponse> UpdateAsync(string path, object data = null, string jsonData = null);
        Task<FirebaseResponse<T>> UpdateAsync<T>(string path, T data);

        Task<EventStreamResponse<T>> OnAsync<T>(string path,
            ObjectRootPatchReceivedEventHandler<T> objectCreated = null,
            ObjectPropertyPatchReceivedEventHandler<T> objectPatched = null,
            EventStreamingResponseEventHandler<T> eventStreamingResponse = null,
            EventStreamingResponseRawEventHandler<T> eventStreamingResponseRaw = null,
            EventStreamingEventHandler<T> eventStreaming = null,
            Action<Exception> exceptionHandler = null);

    }
}
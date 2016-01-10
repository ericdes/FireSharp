using System;
using System.Threading.Tasks;
using FireSharp.EventStreaming;
using FireSharp.Response;

namespace FireSharp.Interfaces
{
    public interface IFirebaseClient
    {
        ISerializer Serializer { get; }
        JsonPatchManager JsonPatchManager { get; }
        
        FirebaseResponse Get(string path);
        FirebaseResponse<T> Get<T>(string path);
        Task<FirebaseResponse> GetAsync(string path);
        Task<FirebaseResponse<T>> GetAsync<T>(string path);

        Task<EventRootResponse<T>> OnChangeGetAsync<T>(string path, ValueChangedEventHandler<T> added = null);

        SetResponse<T> Set<T>(string path, T data);
        Task<SetResponse<T>> SetAsync<T>(string path, T data);

        PushResponse Push(string path, object data);
        PushResponse Push<T>(string path, T data);
        Task<PushResponse> PushAsync(string path, object data);
        Task<PushResponse> PushAsync<T>(string path, T data);

        FirebaseResponse Delete(string path);
        Task<FirebaseResponse> DeleteAsync(string path);

        FirebaseResponse<T> Update<T>(string path, T data);
        Task<FirebaseResponse<T>> UpdateAsync<T>(string path, T data);

        Task<EventStreamResponse<T>> OnAsync<T>(string path,
            ObjectCreatedEventHandler<T> objectCreated = null,
            ObjectPatchedEventHandler<T> objectPatched = null,
            EventStreamingResponseEventHandler<T> eventStreamingResponse = null,
            EventStreamingResponseRawEventHandler<T> eventStreamingResponseRaw = null,
            EventStreamingEventHandler<T> eventStreaming = null);

    }
}
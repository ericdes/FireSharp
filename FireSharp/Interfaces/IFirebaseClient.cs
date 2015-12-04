using System;
using System.Threading.Tasks;
using FireSharp.EventStreaming;
using FireSharp.Response;

namespace FireSharp.Interfaces
{
    public interface IFirebaseClient
    {
        ISerializer Serializer { get; }
        FirebaseResponse Get(string path);
        FirebaseResponse<T> Get<T>(string path);
        Task<FirebaseResponse> GetAsync(string path);
        Task<FirebaseResponse<T>> GetAsync<T>(string path);

        Task<EventRootResponse<T>> OnChangeGetAsync<T>(string path, ValueRootAddedEventHandler<T> added = null);

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

        [Obsolete("This method is obsolete use OnAsync instead.")]
        Task<EventStreamResponse> ListenAsync(string path,
            ValueAddedEventHandler added = null,
            ValueChangedEventHandler changed = null,
            ValueRemovedEventHandler removed = null);

        Task<EventStreamResponse> OnAsync(string path,
            ValueAddedEventHandler added = null,
            ValueChangedEventHandler changed = null,
            ValueRemovedEventHandler removed = null);
    }
}
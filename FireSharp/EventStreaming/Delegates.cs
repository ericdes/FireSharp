using FireSharp.Interfaces;

namespace FireSharp.EventStreaming
{
    public delegate void EventStreamingEventHandler<T>(object sender, EventStreamingEventArgs args);
    public delegate void EventStreamingResponseRawEventHandler<T>(object sender, EventStreamingResponseRawEventArgs args);
    public delegate void EventStreamingResponseEventHandler<T>(object sender, EventStreamingResponseEventArgs<T> args);
    public delegate void ObjectPatchedEventHandler<T>(IEventStreamResponse<T> sender, JsonPatch jsonPatch);
    public delegate void ObjectCreatedEventHandler<T>(object sender, T obj);

    //public delegate void PatchReceivedEventHandler(object sender, EventStreamingResponseEventArgs args);
    //public delegate void PatchReceivedEventHandler<T>(object sender, EventStreamingResponseEventArgs args);



    public delegate void ValueAddedEventHandler(object sender, ValueAddedEventArgs args);

    public delegate void ValueChangedEventHandler(object sender, ValueChangedEventArgs args);
    public delegate void ValueChangedEventHandler<T>(object sender, T data);

    public delegate void ValueRemovedEventHandler(object sender, ValueRemovedEventArgs args);
    public delegate void ValueMovedEventHandler(object sender, ValueMovedEventArgs args);

}
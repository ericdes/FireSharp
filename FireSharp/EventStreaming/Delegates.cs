using FireSharp.Interfaces;

namespace FireSharp.EventStreaming
{
    public delegate void EventStreamingEventHandler<T>(object sender, EventStreamingEventArgs args);
    public delegate void EventStreamingResponseRawEventHandler<T>(object sender, EventStreamingResponseRawEventArgs args);
    public delegate void EventStreamingResponseEventHandler<T>(object sender, EventStreamingResponseEventArgs<T> args);
    public delegate void ObjectPropertyPatchReceivedEventHandler<T>(IEventStreamObserver<T> sender, JsonPatch jsonPatch);
    public delegate void ObjectRootPatchReceivedEventHandler<T>(IEventStreamObserver<T> sender, JsonPatch jsonPatch, bool isFirstTimeRetrieval);


    public delegate void ValueAddedEventHandler(object sender, ValueAddedEventArgs args);

    public delegate void ValueChangedEventHandler(object sender, ValueChangedEventArgs args);
    public delegate void ValueChangedEventHandler<T>(object sender, T data);

    public delegate void ValueRemovedEventHandler(object sender, ValueRemovedEventArgs args);
    public delegate void ValueMovedEventHandler(object sender, ValueMovedEventArgs args);

}
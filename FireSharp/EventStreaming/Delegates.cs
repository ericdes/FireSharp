namespace FireSharp.EventStreaming
{
    public delegate void ValueAddedEventHandler(object sender, ValueAddedEventArgs args);
    public delegate void ValueChangedEventHandler<T>(object sender, T data);

    public delegate void ValueChangedEventHandler(object sender, ValueChangedEventArgs args);

    public delegate void ValueRemovedEventHandler(object sender, ValueRemovedEventArgs args);
    public delegate void ValueMovedEventHandler(object sender, ValueMovedEventArgs args);

}
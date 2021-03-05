using System;

public abstract class EventUtility : IReferencePool
{
    public abstract void Recircle();
}

public sealed class EventUtility<T> where T : EventUtility
{
    public static event Action<T, object> Event;

    public static void Invoke(T eventData = null, object sender = null) {
        Event?.Invoke(eventData, sender);
        if (eventData != null)
            ReferencePoolUtility.Recircle(eventData);
    }

    public static bool Check(Action<T, object> action) {
        return Array.IndexOf(Event.GetInvocationList(), action) > 0;
    }

    public static void Clear() {
        Event = null;
    }
}
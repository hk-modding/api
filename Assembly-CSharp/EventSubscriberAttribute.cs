using System;

namespace Modding
{
    /// <summary>
    /// Marks a field as the instance to be used for event subscriptions within the class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class EventSubscriberAttribute : Attribute
    {
    }
}

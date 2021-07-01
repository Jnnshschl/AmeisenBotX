using System;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Event
{
    public interface IEventManager
    {
        /// <summary>
        /// Contains all the subscribed events and functions.
        /// </summary>
        Dictionary<string, List<Action<long, List<string>>>> Events { get; }

        /// <summary>
        /// The current wow event frame name.
        /// </summary>
        string FrameName { get; set; }

        /// <summary>
        /// Whether we should fire events or not.
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Pending subscribe actions.
        /// </summary>
        Queue<(string, Action<long, List<string>>)> SubscribeQueue { get; }

        /// <summary>
        /// Pending unsubscribe actions.
        /// </summary>
        Queue<(string, Action<long, List<string>>)> UnsubscribeQueue { get; }

        /// <summary>
        /// Call this with the new event string received from the hook module.
        /// </summary>
        /// <param name="eventJson">JSON events string</param>
        void OnEventPush(string eventJson);

        /// <summary>
        /// Initializes the event hookand sets it to active.
        /// </summary>
        void Start();

        /// <summary>
        /// Unloads the event hook and unregisters all events ingame.
        /// </summary>
        void Stop();

        /// <summary>
        /// Subscribe to a wow event.
        /// </summary>
        /// <param name="eventName">Wow event name</param>
        /// <param name="onEventFired">Callback</param>
        void Subscribe(string eventName, Action<long, List<string>> onEventFired);

        /// <summary>
        /// Call this periodically to handle the subscription and unsubscription queues.
        /// </summary>
        void Tick();

        /// <summary>
        /// Unsubscribe from a wow event.
        /// </summary>
        /// <param name="eventName">Wow event name</param>
        /// <param name="onEventFired">Callback to remove</param>
        void Unsubscribe(string eventName, Action<long, List<string>> onEventFired);
    }
}
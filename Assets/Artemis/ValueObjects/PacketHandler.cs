using System;
using System.Collections.Generic;
using UnityEngine;

namespace Artemis.ValuesObjects
{
    public class PacketHandler
    {
        private readonly Dictionary<Type, Action<object>> _handlers = new();

        public void RegisterHandler<T>(Action<T> handler)
        {
            _handlers.Add(typeof(T), (obj) => { handler.Invoke((T) obj); });
        }

        internal void TryInvokeHandler(object obj)
        {
            if (_handlers.TryGetValue(obj.GetType(), out var handler))
            {
                handler.Invoke(obj);
            }
            else
            {
                Debug.LogError($"Theres no registered handler for type '{obj.GetType().GetFriendlyName()}'.");
            }
        }
    }
}
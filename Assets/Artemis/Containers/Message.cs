using System;
using Artemis2023.ValueObjects;

namespace Artemis2023.Containers
{
    [Serializable]
    public class Message<T> : AMessage
    {
        public T Payload
        {
            get => (T) ObjectPayload;
            set => ObjectPayload = value;
        }
    }

    [Serializable]
    public abstract class AMessage
    {
        public int Sequence;
        public DeliveryMethod DeliveryMethod;
        public object ObjectPayload;
        [NonSerialized] public Address Source;
    }
}
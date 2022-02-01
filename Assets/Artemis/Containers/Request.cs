using System;
using Artemis2023.Clients;
using Artemis2023.ValueObjects;

namespace Artemis2023.Containers
{
    [Serializable]
    public class Request<T> : ARequest
    {
        public T Payload
        {
            get => (T) ObjectPayload;
            set => ObjectPayload = value;
        }
        
        public void Reply<T>(T payload)
        {
            var response = new Response<T> {Id = Id, Payload = payload};
            Mean.Send(response, Source, DeliveryMethod.Reliable);
        }
    }

    [Serializable]
    public abstract class ARequest
    {
        public string Id;
        public object ObjectPayload;
        [NonSerialized] public Address Source;
        [NonSerialized] public LowLevelClient Mean;
    }
}
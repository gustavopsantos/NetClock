using System;

namespace Artemis2023.Containers
{
    [Serializable]
    public class Response<T> : AResponse
    {
        public T Payload;
    }

    [Serializable]
    public abstract class AResponse
    {
        public string Id;
    }
}
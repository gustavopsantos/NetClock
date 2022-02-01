using System;
using System.Diagnostics;
using Artemis2023.Delegates;
using Artemis2023.Serialization;
using Artemis2023.ValueObjects;
using Debug = UnityEngine.Debug;

namespace Artemis2023.Clients
{
    public class ObjectClient : IDisposable
    {
        private readonly BitClient _bitClient;
        private readonly ObjectMessageHandler _handler;

        internal ObjectClient(ObjectMessageHandler handler)
        {
            _handler = handler;
            _bitClient = new BitClient(HandleBitMessage);
        }
        
        internal ObjectClient(int port, ObjectMessageHandler handler)
        {
            _handler = handler;
            _bitClient = new BitClient(port, HandleBitMessage);
        }

        public void Start()
        {
            _bitClient.Start();
        }

        public void Dispose()
        {
            _bitClient.Dispose();
        }

        public void Send<T>(T obj, Address destination)
        {
            var bytes = BinarySerializer.Serialize(obj);
            _bitClient.Send(bytes, destination);
        }

        private void HandleBitMessage(byte[] bytes, Address source)
        {
            var obj = BinarySerializer.Deserialize(bytes);
            _handler.Invoke(obj, source);
        }
    }
}
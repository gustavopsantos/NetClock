using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Artemis.ValuesObjects;
using Artemis2023.Containers;
using Artemis2023.ValueObjects;

namespace Artemis2023.Clients
{
    public class HighLevelClient : IDisposable
    {
        public readonly PacketHandler Handlers = new PacketHandler();
        private readonly LowLevelClient _client;
        private readonly Dictionary<string, Action<object>> _callbacks = new();
        private readonly Dictionary<string, object> _responses = new Dictionary<string, object>();
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public HighLevelClient()
        {
            _client = new LowLevelClient(HandleMessage);
        }
        
        public HighLevelClient(int port)
        {
            _client = new LowLevelClient(port, HandleMessage);
        }

        private void HandleMessage(AMessage message, Address source)
        {
            switch (message.ObjectPayload)
            {
                case ARequest aRequest:
                    aRequest.Mean = _client;
                    aRequest.Source = source;
                    Handlers.TryInvokeHandler(message.ObjectPayload);
                    break;
                case AResponse aResponse:
                    _responses.Add(aResponse.Id, message.ObjectPayload);

                    if (_callbacks.TryGetValue(aResponse.Id, out var callback))
                    {
                        _callbacks.Remove(aResponse.Id);
                        callback.Invoke(message.ObjectPayload);
                    }

                    break;
                default:
                    Handlers.TryInvokeHandler(message);
                    break;
            }
        }

        public void Start()
        {
            _client.Start();
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        public void Send<T>(T payload, Address destination)
        {
            _client.Send(payload, destination, DeliveryMethod.Unreliable);
        }

        public void Request<TRequestPayload, TResponsePayload>(TRequestPayload payload, Address destination,
            Action<Response<TResponsePayload>> callback)
        {
            var request = new Request<TRequestPayload> {Id = Guid.NewGuid().ToString("N"), Payload = payload};
            _callbacks.Add(request.Id, x => callback((Response<TResponsePayload>) x));
            _client.Send(request, destination, DeliveryMethod.Reliable);
        }

        public async Task<Response<TResponsePayload>> RequestAsync<TRequestPayload, TResponsePayload>(
            TRequestPayload payload, Address destination)
        {
            var request = new Request<TRequestPayload> {Id = Guid.NewGuid().ToString("N"), Payload = payload};
            _client.Send(request, destination, DeliveryMethod.Reliable);

            while (true)
            {
                await Task.Yield();

                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    throw new TaskCanceledException();
                }

                if (_responses.TryGetValue(request.Id, out var response))
                {
                    _responses.Remove(request.Id);
                    return (Response<TResponsePayload>) response;
                }
            }
        }
    }
}
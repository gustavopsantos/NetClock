using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Artemis2023.Containers;
using Artemis2023.ValueObjects;
using UnityEngine;

namespace Artemis2023.Clients
{
    public class LowLevelClient : IDisposable
    {
        private readonly ObjectClient _client;
        private readonly Action<AMessage, Address> _messageHandler;
        private readonly PacketSequenceStorage _outgoingSequenceStorage = new PacketSequenceStorage();
        private readonly PacketSequenceStorage _incomingSequenceStorage = new PacketSequenceStorage();
        private readonly List<(Action, Address, int)> _pendingReliablePackets = new();
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        internal LowLevelClient(Action<AMessage, Address> messageHandler)
        {
            _messageHandler = messageHandler;
            _client = new ObjectClient(HandleObjectMessage);
        }
        
        internal LowLevelClient(int port, Action<AMessage, Address> messageHandler)
        {
            _messageHandler = messageHandler;
            _client = new ObjectClient(port, HandleObjectMessage);
        }

        private void HandleObjectMessage(object obj, Address source)
        {
            switch (obj)
            {
                case Ack ack:
                    HandleReceivedAck(ack, source);
                    break;
                case AMessage message:
                    HandleReceivedMessage(message, source);
                    break;
                default:
                    throw new NotImplementedException($"Message type {obj.GetType().GetFriendlyName()} unhandled");
            }
        }
        
        private void HandleReceivedAck(Ack ack, Address source)
        {
            for (int i = 0; i < _pendingReliablePackets.Count; i++)
            {
                var (resend, destination, sequence) = _pendingReliablePackets[i];

                if (sequence == ack.Sequence && destination.AreEqual(source))
                {
                    _pendingReliablePackets.RemoveAt(i);
                    return;
                }
            }
        }

        private void HandleReceivedMessage(AMessage message, Address source)
        {
            message.Source = source;
            switch (message.DeliveryMethod)
            {
                case DeliveryMethod.Reliable:
                    HandleReliablePacket(message, source);
                    break;
                case DeliveryMethod.Unreliable:
                    HandleUnreliablePacket(message, source);
                    break;
                default:
                    throw new NotImplementedException($"{message.DeliveryMethod} not handled in {nameof(LowLevelClient)}");
            }
        }
        
        private void HandleReliablePacket(AMessage message, Address source)
        {
            var expectedSequence = _incomingSequenceStorage.Get(source, DeliveryMethod.Reliable, 0) + 1;

            if (message.Sequence != expectedSequence)
            {
                Debug.LogWarning($"Discarding reliable sequenced packet #{message.Sequence} as expected sequence is #{expectedSequence}");
                return;
            }

            SendAck(message.Sequence, source);
            _incomingSequenceStorage.Set(source, DeliveryMethod.Reliable, message.Sequence);
            _messageHandler.Invoke(message, source);
        }
        
        private void SendAck(int sequence, Address destination)
        {
            _client.Send(new Ack {Sequence = sequence}, destination);
        }
        
        private void HandleUnreliablePacket(AMessage message, Address source)
        {
            var ordered = message.Sequence > _incomingSequenceStorage.Get(source, DeliveryMethod.Unreliable, 0);

            if (!ordered)
            {
                Debug.LogWarning("Discarding out of order/duplicate packet");
                return;
            }

            _incomingSequenceStorage.Set(source, DeliveryMethod.Unreliable, message.Sequence);
            _messageHandler.Invoke(message, source);
        }

        public void Start()
        {
            _client.Start();
            ResendReliablePackets(_cancellationTokenSource.Token);
        }
        
        private async void ResendReliablePackets(CancellationToken cancellationToken)
        {
            try
            {
                while (true)
                {
                    await Task.Delay(64, cancellationToken);

                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    for (int i = 0; i < Mathf.Clamp(4, 0, _pendingReliablePackets.Count); i++)
                    {
                        _pendingReliablePackets[i].Item1.Invoke();
                    }
                }
            }
            catch (TaskCanceledException)
            {
            }
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        public void Send<T>(T payload, Address destination, DeliveryMethod deliveryMethod)
        {
            var message = new Message<T>
            {
                Payload = payload,
                Sequence = _outgoingSequenceStorage.Get(destination, deliveryMethod, 0) + 1,
            };

            _outgoingSequenceStorage.Set(destination, deliveryMethod, message.Sequence);

            GetSendMethod(deliveryMethod).Invoke(message, destination);
        }

        private Action<AMessage, Address> GetSendMethod(DeliveryMethod deliveryMethod)
        {
            switch (deliveryMethod)
            {
                case DeliveryMethod.Reliable: return SendReliable;
                case DeliveryMethod.Unreliable: return SendUnreliable;
                default: throw new Exception($"Delivery method '{deliveryMethod}' not handled");
            }
        }

        private void SendReliable(AMessage message, Address destination)
        {
            message.DeliveryMethod = DeliveryMethod.Reliable;
            _client.Send(message, destination);
            _pendingReliablePackets.Add((() => _client.Send(message, destination), destination, message.Sequence));
        }

        private void SendUnreliable(AMessage message, Address destination)
        {
            message.DeliveryMethod = DeliveryMethod.Unreliable;
            _client.Send(message, destination);
        }
    }
}
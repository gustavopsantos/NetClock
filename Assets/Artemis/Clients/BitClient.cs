using System;
using System.Net;
using System.Net.Sockets;
using Artemis2023.Delegates;
using Artemis2023.ValueObjects;
using UnityEngine;
using UnityEngine.Assertions;

namespace Artemis2023.Clients
{
    public class BitClient : IDisposable
    {
        private readonly UdpClient _udpClient;
        private readonly BitMessageHandler _handler;
        private IPEndPoint _source = new IPEndPoint(IPAddress.Any, default);

        internal BitClient(BitMessageHandler handler)
        {
            _handler = handler;
            _udpClient = new UdpClient();
        }
        
        internal BitClient(int port, BitMessageHandler handler)
        {
            _handler = handler;
            _udpClient = new UdpClient(port);
        }

        public void Start()
        {
            _udpClient.BeginReceive(ReceivePacket, _udpClient);
        }

        public void Dispose()
        {
            _udpClient.Close();
        }

        public void Send(byte[] bytes, Address destination)
        {
            _udpClient.Send(bytes, bytes.Length, destination.Ip, destination.Port);
        }

        private void ReceivePacket(IAsyncResult ar)
        {
            try
            {
                var bytes = _udpClient.EndReceive(ar, ref _source);
                _handler.Invoke(bytes, Address.FromIPEndPoint(_source));
                _udpClient.BeginReceive(ReceivePacket, _udpClient);
            }
            catch (Exception e) when (e.GetType() != typeof(ObjectDisposedException))
            {
                Debug.LogError(e);
                _udpClient.BeginReceive(ReceivePacket, _udpClient);
            }
        }
    }
}
using System.Net;
using UnityEngine.Assertions;

namespace Artemis2023.ValueObjects
{
    public readonly struct Address
    {
        public readonly string Ip;
        public readonly int Port;

        public Address(string ip, int port)
        {
            Assert.IsTrue(IPAddress.TryParse(ip, out _));
            Ip = ip;
            Port = port;
        }

        public static Address FromIPEndPoint(IPEndPoint ipEndPoint)
        {
            return new Address(ipEndPoint.Address.ToString(), ipEndPoint.Port);
        }

        public bool AreEqual(Address other)
        {
            return Ip == other.Ip && Port == other.Port;
        }

        public override string ToString()
        {
            return $"{Ip}:{Port}";
        }
    }
}
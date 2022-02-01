using System;
using UnityEngine;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace NetClock
{
    public class NtpTimeClient : MonoBehaviour, ITimeClient
    {
        [SerializeField] private string _host = "time.windows.com";

        public async Task<(DateTime, RTT)> Fetch()
        {
            using var client = new UdpClient();
            client.Connect(_host, 123);
            var request = CreateRequest();
            await client.SendAsync(request, request.Length);
            var timeAtRequest = DateTime.Now;
            var response = await client.ReceiveAsync();
            var timeAtResponse = DateTime.Now;
            var rtt = timeAtResponse - timeAtRequest;
            var time = ParseNetworkTime(response.Buffer).Add(rtt);
            return (time, rtt);
        }

        private static byte[] CreateRequest()
        {
            // NTP message size is 16 bytes of the digest (RFC 2030)
            var request = new byte[48];

            // Setting the Leap Indicator, 
            // Version Number and Mode values
            // LI = 0 (no warning)
            // VN = 3 (IPv4 only)
            // Mode = 3 (Client Mode)
            request[0] = 0x1B;

            return request;
        }

        private static DateTime ParseNetworkTime(IReadOnlyList<byte> data)
        {
            var intPart = (ulong) data[40] << 24 | (ulong) data[41] << 16 | (ulong) data[42] << 8 | (ulong) data[43];
            var fractPart = (ulong) data[44] << 24 | (ulong) data[45] << 16 | (ulong) data[46] << 8 | (ulong) data[47];
            var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);
            return new DateTime(1900, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(milliseconds);
        }
    }
}
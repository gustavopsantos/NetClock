using System;
using System.Threading.Tasks;
using Artemis2023.Clients;
using Artemis2023.ValueObjects;
using NetClock;
using UnityEngine;

public class ArtemisTimeClient : MonoBehaviour, ITimeClient
{
    [SerializeField] private string _serverIp = "127.0.0.1";

    public async Task<(DateTime, RTT)> Fetch()
    {
        var serverAddress = new Address(_serverIp, Constants.TimeServerPort);
        using var client = new HighLevelClient();
        client.Start();
        var timeAtRequest = DateTime.Now;
        var response = await client.RequestAsync<TimeRequest, TimeResponse>(new TimeRequest(), serverAddress);
        var timeAtResponse = DateTime.Now;
        var rtt = timeAtResponse - timeAtRequest;
        var time = response.Payload.Time.Add(rtt);
        return (time, rtt);
    }
}
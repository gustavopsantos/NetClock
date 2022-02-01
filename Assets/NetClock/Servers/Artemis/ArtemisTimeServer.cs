using System;
using UnityEngine;
using Artemis2023.Clients;
using Artemis2023.Containers;

public class ArtemisTimeServer : MonoBehaviour
{
    private HighLevelClient _client;

    private void Start()
    {
        _client = new HighLevelClient(Constants.TimeServerPort);
        _client.Handlers.RegisterHandler<Request<TimeRequest>>(HandleTimeRequest);
        _client.Start();
    }

    private void OnDestroy()
    {
        _client.Dispose();
    }

    private static void HandleTimeRequest(Request<TimeRequest> request)
    {
        request.Reply(new TimeResponse(DateTime.UtcNow));
    }
}
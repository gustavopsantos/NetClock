using System;

[Serializable]
public class TimeResponse
{
    public readonly DateTime Time;

    public TimeResponse(DateTime time)
    {
        Time = time;
    }
}
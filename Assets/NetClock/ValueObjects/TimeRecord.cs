using System;

public class TimeRecord
{
    public long RTT;
    public DateTime Value;
    public DateTime StoredAt;

    public TimeRecord()
    {
        
    }
    
    public TimeRecord(DateTime value, long rtt)
    {
        RTT = rtt;
        Value = value;
        StoredAt = DateTime.Now;
    }
}
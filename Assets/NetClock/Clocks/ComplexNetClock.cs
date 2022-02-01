using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using EasyButtons;
using NetClock;
using UnityEngine;
using UnityEngine.Assertions;

public class ComplexNetClock : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private int _samples = 9;
    [SerializeField] private int _rttDeviationTolerance = 1;
    [SerializeField] private int _delayBetweenSampleQuerying = 500;

    [Header("Readonly")]
    [SerializeField, ReadOnly] public bool _fetching;
    [SerializeField, ReadOnly] public double _offset;
    [SerializeField, ReadOnly] public string _rtt;

    [Button]
    private async void Fetch()
    {
        Assert.IsTrue(_samples % 2 == 1, $"{nameof(_samples)} field should be odd!");
        _fetching = true;
        var samples = await FetchSamples(_samples, _delayBetweenSampleQuerying);
        var orderedSamples = samples.OrderBy(s => s.RTT).ToArray();
        var median = orderedSamples[samples.Count / 2];
        var validSamples = samples.Where(s => Math.Abs(TimeSpan.FromTicks(s.RTT).TotalMilliseconds  - TimeSpan.FromTicks(median.RTT).TotalMilliseconds) <= _rttDeviationTolerance).ToArray();
        var mean = ArithmeticMean(validSamples);
        var time = mean.Value.Add(DateTime.Now - mean.StoredAt);
        _offset = (time - DateTime.Now).TotalMilliseconds;
        GetComponent<TimeSyncStatistics>().UpdateData(_offset);
        _rtt = TimeSpan.FromTicks(median.RTT).TotalMilliseconds.ToString(CultureInfo.InvariantCulture);
        _fetching = false;
    }

    private static TimeRecord ArithmeticMean(IReadOnlyCollection<TimeRecord> records)
    {
        var result = new TimeRecord();

        foreach (var record in records)
        {
            result.RTT += record.RTT / records.Count;
            result.Value = new DateTime(result.Value.Ticks + record.Value.Ticks / records.Count);
            result.StoredAt = new DateTime(result.StoredAt.Ticks + record.StoredAt.Ticks / records.Count);
        }

        return result;
    }

    private async Task<List<TimeRecord>> FetchSamples(int amount, int delayBetwwenSampleQuerying)
    {
        var client = GetComponent<ITimeClient>();
        var samples = new List<TimeRecord>(amount);

        for (int i = 0; i < amount; i++)
        {
            var (time, rtt) = await client.Fetch();
            samples.Add(new TimeRecord(time, rtt));
            await Task.Delay(delayBetwwenSampleQuerying);
        }

        return samples;
    }
}
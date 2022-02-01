using System;
using EasyButtons;
using UnityEngine;

[Serializable]
public class TimeSyncStatistics : MonoBehaviour
{
    [SerializeField, ReadOnly] private double _min;
    [SerializeField, ReadOnly] private double _max;
    [SerializeField, ReadOnly] private double _deviation;

    [Button]
    private void Reset()
    {
        _min = double.PositiveInfinity;
        _max = double.NegativeInfinity;
        _deviation = double.PositiveInfinity;
    }

    public void UpdateData(double offset)
    {
        var expected = -TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalMilliseconds;
        var error = Math.Abs(offset - expected);

        if (error < _min)
        {
            _min = error;
        }

        if (error > _max)
        {
            _max = error;
        }

        _deviation = _max - _min;
    }
}
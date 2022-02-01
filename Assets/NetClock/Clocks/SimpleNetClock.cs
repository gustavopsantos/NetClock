using System;
using System.Globalization;
using EasyButtons;
using UnityEngine;

namespace NetClock
{
    public class SimpleNetClock : MonoBehaviour
    {
        [Header("Readonly")]
        [SerializeField, ReadOnly] public bool _fetching;
        [SerializeField, ReadOnly] public double _offset;
        [SerializeField, ReadOnly] public string _rtt;

        [Button]
        private async void Fetch()
        {
            _fetching = true;
            var (time, rtt) = await GetComponent<ITimeClient>().Fetch();
            var localNow = DateTime.Now;
            _offset = (time - localNow).TotalMilliseconds;
            GetComponent<TimeSyncStatistics>().UpdateData(_offset);
            _rtt = TimeSpan.FromTicks(rtt).TotalMilliseconds.ToString(CultureInfo.InvariantCulture);
            _fetching = false;
        }
    }
}
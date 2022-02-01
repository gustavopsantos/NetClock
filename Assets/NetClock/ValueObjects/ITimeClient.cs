using System;
using System.Threading.Tasks;

namespace NetClock
{
    public interface ITimeClient
    {
        Task<(DateTime, RTT)> Fetch();
    }
}
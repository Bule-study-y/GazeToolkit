using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UXI.GazeToolkit
{
    public interface ITimestampedData
    {
        DateTimeOffset Timestamp { get; }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UXI.GazeToolkit.Serialization;

namespace UXI.GazeFilter
{
    public interface IFilterStatistics
    {
        IObserver<object> InputObserver { get; }
        IObserver<object> OutputObserver { get; }
        IEnumerable<object> GetResults();
        Type DataType { get; }
        FileFormat DefaultFormat { get; }
    }

    
}
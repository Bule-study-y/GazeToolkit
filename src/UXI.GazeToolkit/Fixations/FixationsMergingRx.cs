using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using UXI.GazeToolkit.Extensions;
using UXI.GazeToolkit.Utils;

namespace UXI.GazeToolkit.Fixations
{
    public interface IFixationsMergingOptions
    {
        TimeSpan MaxTimeBetweenFixations { get; }
        double MaxAngleBetweenFixations { get; }
    }



    public static class FixationsMergingRx
    {
        // .-|.-|.-|
        public static IObservable<EyeMovement> MergeAdjacentFixations(this IObservable<EyeMovement> movements, TimeSpan maxTimeBetweenFixations, double maxAngleBetweenFixations)
        {
            return movements.Buffer(m => m.MovementType == EyeMovementType.Fixation)
                            .Scan((aggregate, buffer) =>
                            {
                                if (aggregate.Any())
                                {
                                    var lastFixation = aggregate.FirstOrDefault(m => m.MovementType == EyeMovementType.Fixation);
                                    var nextFixation = buffer.FirstOrDefault(m => m.MovementType == EyeMovementType.Fixation);
                                    var nextMovements = buffer.Except(new[] { nextFixation }).ToArray();

                                    if (lastFixation != null && nextFixation != null)
                                    {
                                        var timeBetweenFixations = (nextFixation.StartTime - lastFixation.EndTime);
                                        if (timeBetweenFixations <= (maxTimeBetweenFixations.Ticks / 10))
                                        {
                                            var lastSample = lastFixation.Samples.Last().EyeGazeData;
                                            var nextSample = nextFixation.Samples.First().EyeGazeData;

                                            var averageSample = EyeGazeDataUtils.Average(lastSample, nextSample);

                                            double angle = averageSample.GetVisualAngle(lastFixation.AverageSample, nextFixation.AverageSample);

                                            if (angle <= maxAngleBetweenFixations)
                                            {
                                                // merge
                                                var mergedFixation = new List<EyeMovement>()
                                                {
                                                    new EyeMovement
                                                    (
                                                        aggregate.SelectMany(m => m.Samples).Concat(nextFixation.Samples),
                                                        EyeMovementType.Fixation,
                                                        lastFixation.StartTime
                                                    )
                                                    {
                                                        EndTime = nextFixation.EndTime
                                                    }
                                                };

                                                if (nextMovements != null && nextMovements.Any())
                                                {
                                                    mergedFixation.AddRange(nextMovements);
                                                }
                                                return mergedFixation;
                                            }
                                        }
                                    }
                                }
                                return buffer;
                            })
                            .Where(b => b.Any())
                            .Buffer((first, current) => first.First().StartTime != current.First().StartTime)
                            .Where(b => b.Any())
                            .Select(b => b.Last())
                            .SelectMany(b => b);
        }


        public static IObservable<EyeMovement> MergeAdjacentFixations(this IObservable<EyeMovement> movements, IFixationsMergingOptions options)
        {
            return MergeAdjacentFixations(movements, options.MaxTimeBetweenFixations, options.MaxAngleBetweenFixations);
        }
    }
}

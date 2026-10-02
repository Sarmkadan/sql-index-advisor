using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlIndexAdvisor.Core.Engine
{
    public static class CostEstimator
    {
        public static double EstimateCost(
            double relativeCost,
            long rowCount,
            double selectivity,
            IEnumerable<string> columns)
        {
            ArgumentNullException.ThrowIfNull(columns);
            ArgumentOutOfRangeException.ThrowIfNegative(relativeCost);
            ArgumentOutOfRangeException.ThrowIfNegative(rowCount);
            ArgumentOutOfRangeException.ThrowIfLessThan(selectivity, 0);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(selectivity, 1);

            if (!columns.Any())
            {
                throw new ArgumentException("Column list cannot be empty.", nameof(columns));
            }

            // Cost estimation logic
            return relativeCost * rowCount * selectivity;
        }

        public static double EstimateCost(
            double relativeCost,
            long rowCount,
            double selectivity)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(relativeCost);
            ArgumentOutOfRangeException.ThrowIfNegative(rowCount);
            ArgumentOutOfRangeException.ThrowIfLessThan(selectivity, 0);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(selectivity, 1);

            // Cost estimation logic
            return relativeCost * rowCount * selectivity;
        }
    }
}

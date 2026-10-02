using System;
using System.Collections.Generic;
using SqlIndexAdvisor.Core.Engine;
using Xunit;

namespace SqlIndexAdvisor.Tests
{
    public class CostEstimatorTests
    {
        [Fact]
        public void EstimateCost_WithValidArguments_ReturnsExpectedCost()
        {
            // Arrange
            double relativeCost = 0.5;
            long rowCount = 1000;
            double selectivity = 0.1;
            var columns = new List<string> { "Column1", "Column2" };

            // Act
            double result = CostEstimator.EstimateCost(relativeCost, rowCount, selectivity, columns);

            // Assert
            Assert.Equal(relativeCost * rowCount * selectivity, result);
        }

        [Fact]
        public void EstimateCost_WithNullColumns_ThrowsArgumentNullException()
        {
            // Arrange
            double relativeCost = 0.5;
            long rowCount = 1000;
            double selectivity = 0.1;
            IEnumerable<string> columns = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => CostEstimator.EstimateCost(relativeCost, rowCount, selectivity, columns));
        }

        [Fact]
        public void EstimateCost_WithEmptyColumns_ThrowsArgumentException()
        {
            // Arrange
            double relativeCost = 0.5;
            long rowCount = 1000;
            double selectivity = 0.1;
            var columns = new List<string>();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => CostEstimator.EstimateCost(relativeCost, rowCount, selectivity, columns));
        }

        [Fact]
        public void EstimateCost_WithNegativeRelativeCost_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            double relativeCost = -0.5;
            long rowCount = 1000;
            double selectivity = 0.1;
            var columns = new List<string> { "Column1", "Column2" };

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => CostEstimator.EstimateCost(relativeCost, rowCount, selectivity, columns));
        }

        [Fact]
        public void EstimateCost_WithNegativeRowCount_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            double relativeCost = 0.5;
            long rowCount = -1000;
            double selectivity = 0.1;
            var columns = new List<string> { "Column1", "Column2" };

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => CostEstimator.EstimateCost(relativeCost, rowCount, selectivity, columns));
        }

        [Fact]
        public void EstimateCost_WithSelectivityLessThanZero_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            double relativeCost = 0.5;
            long rowCount = 1000;
            double selectivity = -0.1;
            var columns = new List<string> { "Column1", "Column2" };

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => CostEstimator.EstimateCost(relativeCost, rowCount, selectivity, columns));
        }

        [Fact]
        public void EstimateCost_WithSelectivityGreaterThanOne_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            double relativeCost = 0.5;
            long rowCount = 1000;
            double selectivity = 1.1;
            var columns = new List<string> { "Column1", "Column2" };

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => CostEstimator.EstimateCost(relativeCost, rowCount, selectivity, columns));
        }

        [Fact]
        public void EstimateCost_WithoutColumns_WithValidArguments_ReturnsExpectedCost()
        {
            // Arrange
            double relativeCost = 0.5;
            long rowCount = 1000;
            double selectivity = 0.1;

            // Act
            double result = CostEstimator.EstimateCost(relativeCost, rowCount, selectivity);

            // Assert
            Assert.Equal(relativeCost * rowCount * selectivity, result);
        }

        [Fact]
        public void EstimateCost_WithoutColumns_WithNegativeRelativeCost_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            double relativeCost = -0.5;
            long rowCount = 1000;
            double selectivity = 0.1;

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => CostEstimator.EstimateCost(relativeCost, rowCount, selectivity));
        }

        [Fact]
        public void EstimateCost_WithoutColumns_WithNegativeRowCount_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            double relativeCost = 0.5;
            long rowCount = -1000;
            double selectivity = 0.1;

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => CostEstimator.EstimateCost(relativeCost, rowCount, selectivity));
        }

        [Fact]
        public void EstimateCost_WithoutColumns_WithSelectivityLessThanZero_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            double relativeCost = 0.5;
            long rowCount = 1000;
            double selectivity = -0.1;

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => CostEstimator.EstimateCost(relativeCost, rowCount, selectivity));
        }

        [Fact]
        public void EstimateCost_WithoutColumns_WithSelectivityGreaterThanOne_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            double relativeCost = 0.5;
            long rowCount = 1000;
            double selectivity = 1.1;

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => CostEstimator.EstimateCost(relativeCost, rowCount, selectivity));
        }
    }
}

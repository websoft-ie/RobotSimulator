using System;

namespace MonkeyMotionControl
{
    public class Range
    {
        #region Protected Fields

        protected double min = double.MinValue;

        protected double max = double.MaxValue;

        #endregion

        #region Constructors

        public Range() : this(0, 0) { }
        
        public Range(double min, double max)
        {
            this.min = min;
            this.max = max;
        }

        #endregion

        #region Properties

        public double Min { get { return min; } }

        public double Max { get { return max; } }

        public double Length { get { return Math.Max(min, max) - Math.Min(min, max); } }

        #endregion

        #region Methods

        public bool Contains(double value)
        {
            return value >= min && value <= max;
        }

        public double Clamp(double value, uint roundDigits = 3, double resolution = 0.001)
        {
            var roundedValue = Math.Round(value, (int)roundDigits);
            if (roundedValue.IsLessThan(min, resolution)) return min;
            if (roundedValue.IsGreaterThan(max, resolution)) return max;
            return roundedValue;
        }

        #endregion

        public override string ToString()
        {
            return $"[{min}~{max}]";
        }

    }
}

namespace ClimateDataExaminer
{
    /// <summary>
    /// generic pair struct
    /// </summary>
    public struct RangePair
    {
        /// <summary>
        /// Lower bound
        /// </summary>
        public double LowerBound;

        /// <summary>
        /// Higher bound
        /// </summary>
        public double HigherBound;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="l">Lower bound</param>
        /// <param name="h">Higher bound</param>
        public RangePair(double l, double h)
        {
            this.LowerBound = l;
            this.HigherBound = h;
        }

        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="r">The value for this pair</param>
        public RangePair(double r)
        {
            this.LowerBound = this.HigherBound = r;
        }

        /// <summary>
        /// Returns an item in range between Lower and Higher Bound
        /// </summary>
        /// <param name="d">The random object</param>
        /// <returns>Number in range</returns>
        public double RollInRange(MersenneTwister d)
        {
            if (HigherBound == LowerBound)
                return LowerBound;

            return (d.NextDoublePositive() * (HigherBound - LowerBound) + LowerBound);
        }
    }
}

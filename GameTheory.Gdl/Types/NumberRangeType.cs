namespace GameTheory.Gdl.Types
{
    using System;
    using System.Collections.Concurrent;
    using Intervals;

    public class NumberRangeType : ExpressionType, IInterval<int>
    {
        private static ConcurrentDictionary<(int, int), NumberRangeType> instances = new ConcurrentDictionary<(int, int), NumberRangeType>();

        /// <summary>
        /// Initializes a new instance of the <see cref="NumberRangeType"/> class.
        /// </summary>
        /// <param name="start">The inclusive min value.</param>
        /// <param name="end">The inclusive max value.</param>
        private NumberRangeType(int start, int end)
        {
            this.Start = start;
            this.End = end;
            this.BuiltInType = typeof(int);
        }

        /// <summary>
        /// Gets the inclusive min value.
        /// </summary>
        public int Start { get; }

        /// <summary>
        /// Gets the inclusive max value.
        /// </summary>
        public int End { get; }

        /// <inheritdoc/>
        public override ExpressionType BaseType => NumberType.Instance;

        int IInterval<int>.End => this.End + 1;

        bool IInterval<int>.EndInclusive => false;

        bool IInterval<int>.StartInclusive => true;

        public static NumberRangeType GetInstance(int start, int end) => instances.GetOrAdd((start, end), _ => new NumberRangeType(start, end));

        public override string ToString() => $"{this.Start} to {this.End}";

        IInterval<int> IInterval<int>.Clone(int start, bool startInclusive, int end, bool endInclusive)
        {
            if (!startInclusive)
            {
                throw new NotImplementedException();
            }

            return NumberRangeType.GetInstance(start, endInclusive ? end : end - 1);
        }
    }
}

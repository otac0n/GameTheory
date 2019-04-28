namespace GameTheory.Gdl.Types
{
    using System;
    using System.Collections.Concurrent;
    using Intervals;
    using KnowledgeInterchangeFormat.Expressions;

    public class NumberRangeType : ExpressionType, IInterval<int>
    {
        private static ConcurrentDictionary<(int, int), ExpressionType> instances = new ConcurrentDictionary<(int, int), ExpressionType>();

        /// <summary>
        /// Initializes a new instance of the <see cref="NumberRangeType"/> class.
        /// </summary>
        /// <param name="start">The inclusive min value.</param>
        /// <param name="end">The inclusive max value.</param>
        protected NumberRangeType(int start, int end)
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

        int IInterval<int>.End => this.End + 1;

        bool IInterval<int>.EndInclusive => false;

        bool IInterval<int>.StartInclusive => true;

        public static ExpressionType GetInstance(int start, int end) => instances.GetOrAdd((start, end), _ => new NumberRangeType(start, end));

        public override string ToString() => this.Start == this.End ? this.Start.ToString() : $"{this.Start} to {this.End}";

        IInterval<int> IInterval<int>.Clone(int start, bool startInclusive, int end, bool endInclusive)
        {
            if (!startInclusive)
            {
                throw new NotImplementedException();
            }

            return (IInterval<int>)NumberRangeType.GetInstance(start, endInclusive ? end : end - 1);
        }
    }
}

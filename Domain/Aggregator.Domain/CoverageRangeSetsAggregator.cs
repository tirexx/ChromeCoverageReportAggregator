using System.Collections.Generic;
using System.Linq;
using Aggregator.Contracts;

namespace Aggregator.Domain
{
    public class CoverageRangeSetsAggregator
    {
        public Range[] AggregateRanges(Range[] ranges)
        {
            ranges = ranges.OrderBy(x => x.Start).ToArray();

            var stack = new Stack<Range>();
            stack.Push(ranges[0]);

            for (var i = 1; i < ranges.Length; i++)
            {
                var top = stack.Peek();

                // not overlap
                if (top.End < ranges[i].Start) stack.Push(ranges[i]);

                // overlap and end is further
                if (top.End >= ranges[i].Start) top.End = top.End > ranges[i].End ? top.End : ranges[i].End;
            }

            return stack.OrderBy(x => x.Start).ToArray();
        }
    }
}
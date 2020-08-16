using System;

namespace Aggregator.Contracts
{
    public class ResourceCoverageReport
    {
        public ResourceCoverageReport(Uri url, Range[] ranges)
        {
            Url = url;
            Ranges = ranges;
        }

        public Uri Url { get; }
        public Range[] Ranges { get; }
    }
}
using System;

namespace Aggregator.Contracts
{
    public class CoverageStats
    {
        public CoverageStats(long length, long covered, Uri url)
        {
            Covered = covered;
            Url = url;
            Length = length;
        }

        public long Length { get; set; }
        public long Covered { get; set; }
        public decimal Coverage => Length > 0 ? (decimal)100 * Covered / Length : 0;
        public Uri Url { get; set; }
    }
}
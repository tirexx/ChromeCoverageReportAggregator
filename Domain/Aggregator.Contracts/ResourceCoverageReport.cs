using System;

namespace Aggregator.Contracts
{
    public class ResourceCoverageReport
    {
        public ResourceCoverageReport(Uri resourceUrl, string fileName, Range[] ranges)
        {
            ResourceUrl = resourceUrl;
            FileName = fileName;
            Ranges = ranges;
        }

        public string FileName { get; }
        public Uri ResourceUrl { get; }
        public string FilenameOfNonEmptyContent { get; }
        public Range[] Ranges { get; }
    }
}
using System;

namespace Aggregator.Contracts
{
    public class PageResourceCoverageReport : ResourceCoverageReport
    {
        public PageResourceCoverageReport(string fileName, Uri pageUrl, Uri resourceUrl, Range[] ranges, string contentHash, bool isContentEmpty) :
            base(resourceUrl, fileName, ranges)
        {
            PageUrl = pageUrl;
            ContentHash = contentHash;
            IsContentEmpty = isContentEmpty;
        }

        public bool IsContentEmpty { get; }
        public string ContentHash { get; }
        public Uri PageUrl { get; }
    }
}
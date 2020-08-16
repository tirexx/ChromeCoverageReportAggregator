using System;

namespace Aggregator.Contracts
{
    public class PageResourceCoverageReport : ResourceCoverageReport
    {
        public PageResourceCoverageReport(string fileName, Uri pageUrl, Uri url, Range[] ranges, string contentHash) :
            base(url, ranges)
        {
            FileName = fileName;
            PageUrl = pageUrl;
            ContentHash = contentHash;
        }

        public string ContentHash { get; }
        public string FileName { get; }
        public Uri PageUrl { get; }
    }
}
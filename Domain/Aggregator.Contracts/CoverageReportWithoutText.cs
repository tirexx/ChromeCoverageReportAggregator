using System;
using Newtonsoft.Json;

namespace Aggregator.Contracts
{
    public class CoverageReportWithoutContent
    {
        [JsonProperty("url")] public Uri Url { get; set; }
        [JsonProperty("ranges")] public Range[] Ranges { get; set; }
    }
}
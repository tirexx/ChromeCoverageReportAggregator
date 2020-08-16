using Newtonsoft.Json;

namespace Aggregator.Contracts
{
    public class Range
    {
        [JsonProperty("start")] public long Start { get; set; }
        [JsonProperty("end")] public long End { get; set; }
    }
}
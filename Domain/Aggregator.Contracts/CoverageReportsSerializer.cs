using Newtonsoft.Json;

namespace Aggregator.Contracts
{
    public class CoverageReportsSerializer
    {
        public CoverageReport[] CoverageReportFromJson(string json)
        {
            return JsonConvert.DeserializeObject<CoverageReport[]>(json, SettingFabric.DefaultSettings);
        }

        public CoverageReportWithoutContent[] CoverageReportWithoutContentFromJson(string json)
        {
            return JsonConvert.DeserializeObject<CoverageReportWithoutContent[]>(json, SettingFabric.DefaultSettings);
        }

        public string ToJson(CoverageReport[] self)
        {
            return JsonConvert.SerializeObject(self, SettingFabric.DefaultSettings);
        }
    }
}
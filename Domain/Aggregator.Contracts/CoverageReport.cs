﻿using Newtonsoft.Json;

namespace Aggregator.Contracts
{
    // <auto-generated />
    // By https://app.quicktype.io/#l=cs&r=json2csharp
    // To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
    //
    //    using QuickType;
    //
    //    var welcome = Welcome.FromJson(jsonString);

    public class CoverageReport : CoverageReportWithoutContent
    {
        [JsonProperty("text")] public string Text { get; set; }
    }
}
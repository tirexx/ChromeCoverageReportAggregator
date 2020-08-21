using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Aggregator.Contracts;
using Base.Crypto;
using Base.Io;
using Base.Logging;
using Base.Long;
using Base.Strings;
using Base.Tasks;
using Range = Aggregator.Contracts.Range;

namespace Aggregator.Domain
{
    public class Aggregator
    {
        private readonly CoverageRangeSetsAggregator _coverageRangeSetsAggregator;
        private readonly CoverageReportsSerializer _coverageReportsSerializer;
        private readonly IFileSystem _fileSystem;
        private readonly ILogger _logger;

        public Aggregator(IFileSystem fileSystem, ILogger logger)
        {
            _fileSystem = fileSystem;
            _logger = logger;
            _coverageReportsSerializer = new CoverageReportsSerializer();
            _coverageRangeSetsAggregator = new CoverageRangeSetsAggregator();
        }

        public async Task Aggregate(string inputFolder, string fileMask, string outputFolder,
            CancellationToken cancellationToken = default)
        {
            // Read reports
            var pageResourceCoverageReport =
                await ReadCoverageReports(inputFolder, fileMask, cancellationToken).ConfigureAwait(false);

            // Validate
            var resourcesWithDifferentContent = GetResourcesWithDifferentContent(pageResourceCoverageReport);
            if (resourcesWithDifferentContent.Any())
            {
                await ReportValidationErrorsForResourcesWithDifferentContent(resourcesWithDifferentContent, outputFolder, cancellationToken).ConfigureAwait(false);

                pageResourceCoverageReport =
                    pageResourceCoverageReport.Except(resourcesWithDifferentContent.SelectMany(x => x.ToArray()))
                        .ToArray();
            }

            // Aggregate
            var aggregatedPageResourceCoverageReports = pageResourceCoverageReport
                .GroupBy(x => x.Url.AbsoluteUri.Replace(x.Url.Scheme + "://", ""))
                .Select(x => GetAggregatedCoverage(x.First().Url, x.ToArray())).ToArray();

            // Write covered
            var statistic = await WriteCoveredFiles(outputFolder, aggregatedPageResourceCoverageReports, pageResourceCoverageReport,
                cancellationToken).ConfigureAwait(false);

            // report statistic
            await ReportAggregatedCoverage(statistic, outputFolder, cancellationToken).ConfigureAwait(false);
        }

        public ResourceCoverageReport GetAggregatedCoverage(Uri url, PageResourceCoverageReport[] reports)
        {
            var ranges = reports.SelectMany(x => x.Ranges).ToArray();

            if (!ranges.Any()) return new ResourceCoverageReport(url, new Range[] { });

            var aggregatdeRanges = _coverageRangeSetsAggregator.AggregateRanges(ranges);

            _logger.Info($"Coverage for {url.AbsoluteUri} is aggregated from {reports.Length} reports");
            return new ResourceCoverageReport(url, aggregatdeRanges);
        }

        private (string, CoverageStats) GetCoveredResourceContentAndCalculateStats(Uri url, string resourceContent, Range[] ranges)
        {
            try
            {
                var sb = new StringBuilder();
                long coveredChars = 0;
                foreach (var range in ranges)
                {
                    sb.AppendLine(resourceContent.Substring(Convert.ToInt32(range.Start),
                        Convert.ToInt32(range.End - range.Start)));

                    coveredChars += range.End - range.Start;
                }

                return (sb.ToString(), new CoverageStats(resourceContent.Length, coveredChars, url));
            }
            catch (Exception ex)
            {
                _logger.Error(
                    $"Error when getting covered content of {url.AbsoluteUri}. Error: {(string.IsNullOrEmpty(resourceContent) ? "Content is empty in coverage report, but has nonzero ranges" : ex.ToString())}");
                return (string.Empty, new CoverageStats(resourceContent.Length, 0, url));
            }
        }

        private IGrouping<Uri, PageResourceCoverageReport>[] GetResourcesWithDifferentContent(
            PageResourceCoverageReport[] reports)
        {
            return reports.GroupBy(x => x.Url)
                .Where(g => g.GroupBy(y => y.ContentHash).Count() > 1).ToArray();
        }

        private async Task<PageResourceCoverageReport[]> ReadCoverageReports(string inputFolder, string fileMask,
            CancellationToken cancellationToken = default)
        {
            var files = Directory.EnumerateFiles(inputFolder, fileMask).ToArray();
            if (!files.Any())
            {
                _logger.Info("No files to process");
                return Enumerable.Empty<PageResourceCoverageReport>().ToArray();
            }

            _logger.Info($"{files.Length} coverage files found");

            var tasks = files.Select(fileName => Task.Run(async () =>
            {
                _logger.Info($"Reading file {fileName}");
                var fileContent =
                    await _fileSystem.ReadTextFileAsync(fileName, cancellationToken).ConfigureAwait(false);
                try
                {
                    var coverageReports = _coverageReportsSerializer.CoverageReportFromJson(fileContent);
                    var pageUrl = coverageReports.First().Url;
                    var pageCoverageReports = coverageReports.Select(x =>
                        new PageResourceCoverageReport(fileName, pageUrl, x.Url, x.Ranges, x.Text.GetSha256Hash(), string.IsNullOrEmpty(x.Text)));
                    _logger.Info($"File {fileName} parsed successfully");
                    return pageCoverageReports;
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to parse file {fileName}. Error: {ex}");
                    return Enumerable.Empty<PageResourceCoverageReport>().ToArray();
                }
            })).ToArray();

            return (await tasks.WhenAllTasks(_logger, cancellationToken).ConfigureAwait(false)).SelectMany(x => x)
                .ToArray();
        }

        private async Task ReportAggregatedCoverage(CoverageStats[] statistic, string outputFolder, CancellationToken cancellationToken = default)
        {
            var header = new[] {"|   %   | Not covered|    Covered   |   Length   | Url"};
            var lines = header.Concat(statistic.OrderByDescending(x => x.Length - x.Covered).Select(x =>
                    $"| {x.Coverage.ToString("##.##").PadRight(5)} | {(x.Length - x.Covered).WithSizeSuffix().PadLeft(10)} | {x.Covered.WithSizeSuffix().PadLeft(12)} | {x.Length.WithSizeSuffix().PadLeft(10)} | {(x.Url.AbsoluteUri.Length > 260 ? x.Url.AbsoluteUri.Substring(0, 260) + "..." : x.Url.AbsoluteUri)}"))
                .AggregateLinesToString();

            _logger.Info(lines);

            await _fileSystem.WriteTextToFileAsync(Path.Combine(outputFolder, "stats.txt"), lines, cancellationToken)
                .ConfigureAwait(false);
        }

        private async Task ReportValidationErrorsForResourcesWithDifferentContent(
            IGrouping<Uri, PageResourceCoverageReport>[] resourcesWithDifferentContent, string outputFolder, CancellationToken cancellationToken = default)
        {
            var lines = new string[]
            {
                "Resource excluded from aggregation:"
            }; 
            lines = lines.Concat(resourcesWithDifferentContent.Select(x => $" {x.Key}")).ToArray();

            foreach (var resourceCoverageReports in resourcesWithDifferentContent)
            {
                lines = lines.Concat(new[]
                {
                    $"Can't aggregate coverage reports of {resourceCoverageReports.Key}. The resource content is different and won't be aggregated."
                }).ToArray();
                lines = lines.Concat(resourceCoverageReports.GroupBy(x => x.ContentHash)
                        .Select(group =>
                            $"  {(group.First().IsContentEmpty ? "Content is empty, " : "")}Hash = {group.Key} for file - pages:\r\n{group.Select(x => $"    {x.FileName.GetAfterLast(Path.DirectorySeparatorChar)} - {x.PageUrl}").AggregateLinesToString()}"))
                    .ToArray();

            }

            var content = lines.AggregateLinesToString();
            _logger.Warn(content);

            await _fileSystem.WriteTextToFileAsync(Path.Combine(outputFolder, "excluded.txt"), content, cancellationToken)
                .ConfigureAwait(false);
        }

        private async Task<CoverageStats[]> WriteCoveredFiles(string outputFolder,
            ResourceCoverageReport[] aggregatedPageResourceCoverageReports,
            PageResourceCoverageReport[] pageResourceCoverageReport, CancellationToken cancellationToken = default)
        {
            var tasks = aggregatedPageResourceCoverageReports.Select(x => new
                {
                    AggregatedCoverageReport = x, File = pageResourceCoverageReport.First(y => y.Url == x.Url).FileName
                })
                .Select(x => Task.Run(async () =>
                {
                    var fileContent =
                        await _fileSystem.ReadTextFileAsync(x.File, cancellationToken).ConfigureAwait(false);
                    var coverageReports = _coverageReportsSerializer.CoverageReportFromJson(fileContent);
                    var resourceContent = coverageReports.First(y => y.Url == x.AggregatedCoverageReport.Url).Text;
                    var (coveredResourceContent, stats) =
                        GetCoveredResourceContentAndCalculateStats(x.AggregatedCoverageReport.Url, resourceContent,
                            x.AggregatedCoverageReport.Ranges);

                    var url = x.AggregatedCoverageReport.Url.AbsoluteUri.Replace(
                        x.AggregatedCoverageReport.Url.Scheme + "://", "");
                    var fullPath = Path.Combine(outputFolder,
                        url.Replace('/', Path.DirectorySeparatorChar).Replace(':', '_').ReplaceInvalidPathChars("_"));
                    if (fullPath.EndsWith(Path.DirectorySeparatorChar))
                    {
                        fullPath += "_";
                    }

                    var info = new FileInfo(fullPath);
                    var outputFileName = Path.Combine(info.Directory.FullName, info.Name.ReplaceInvalidFileNameChars("_"));
                    if (outputFileName.Length > 260)
                    {
                        outputFileName = outputFileName.Substring(0, 260);
                    }

                    await _fileSystem.WriteTextToFileAsync(outputFileName, coveredResourceContent, cancellationToken)
                        .ConfigureAwait(false);
                    _logger.Info($"Covered part of resource {x.AggregatedCoverageReport.Url.AbsoluteUri} is written to file {outputFileName}");
                    return stats;
                })).ToArray();

            var statistic = await tasks.WhenAllTasks(_logger, cancellationToken).ConfigureAwait(false);
            return statistic;
        }
    }
}
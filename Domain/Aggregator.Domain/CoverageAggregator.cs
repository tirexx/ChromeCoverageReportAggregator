using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Aggregator.Contracts;
using Base.Crypto;
using Base.Io;
using Base.Logging;
using Base.Strings;
using Base.Tasks;
using Range = Aggregator.Contracts.Range;

namespace Aggregator.Domain
{
    public class Aggregator
    {
        private readonly CoverageReportsSerializer _coverageReportsSerializer;
        private readonly IFileSystem _fileSystem;
        private readonly ILogger _logger;

        public Aggregator(IFileSystem fileSystem, ILogger logger)
        {
            _fileSystem = fileSystem;
            _logger = logger;
            _coverageReportsSerializer = new CoverageReportsSerializer();
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
                ShowValidationErrorsForResourcesWithDifferentContent(resourcesWithDifferentContent);

                pageResourceCoverageReport =
                    pageResourceCoverageReport.Except(resourcesWithDifferentContent.SelectMany(x => x.ToArray()))
                        .ToArray();
            }

            // Aggregate
            var aggregatedPageResourceCoverageReports = pageResourceCoverageReport
                .GroupBy(x => x.Url.AbsoluteUri.Replace(x.Url.Scheme + "://", ""))
                .Select(x => GetAggregatedCoverage(x.First().Url, x.ToArray())).ToArray();

            // Write covered
            await WriteCoveredFiles(outputFolder, aggregatedPageResourceCoverageReports, pageResourceCoverageReport,
                cancellationToken).ConfigureAwait(false);
        }

        private ResourceCoverageReport GetAggregatedCoverage(Uri url, PageResourceCoverageReport[] reports)
        {
            var ranges = reports.SelectMany(x => x.Ranges).OrderBy(x => x.Start).ToArray();

            if (!ranges.Any()) return new ResourceCoverageReport(url, new Range[] { });

            var stack = new Stack<Range>();
            stack.Push(ranges[0]);

            for (var i = 1; i < ranges.Length; i++)
            {
                var top = stack.Peek();

                // not overlap
                if (top.End < ranges[i].Start) stack.Push(ranges[i]);

                // overlap and end is further
                if (top.End < ranges[i].End) top.End = ranges[i].End;
            }
            _logger.Info($"Coverage for {url.AbsoluteUri} is aggregated from {reports.Length} reports");
            return new ResourceCoverageReport(url, stack.OrderBy(x => x.Start).ToArray());
        }

        private string GetCoveredResourceContent(string resourceContent, Range[] ranges)
        {
            var sb = new StringBuilder();
            foreach (var range in ranges)
            {
                var end = range.End;
                if (end >= resourceContent.Length) end = resourceContent.Length - 1;

                sb.AppendLine(resourceContent.Substring(Convert.ToInt32(range.Start),
                    Convert.ToInt32(end - range.Start + 1)));
            }

            return sb.ToString();
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
                        new PageResourceCoverageReport(fileName, pageUrl, x.Url, x.Ranges, x.Text.GetSha256Hash()));
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

        private void ShowValidationErrorsForResourcesWithDifferentContent(
            IGrouping<Uri, PageResourceCoverageReport>[] resourcesWithDifferentContent)
        {
            foreach (var resourceCoverageReports in resourcesWithDifferentContent)
            {
                var lines = new[]
                {
                    $"Can't aggregate coverage reports of {resourceCoverageReports.Key}. The resource content is different and won't be aggregated."
                };
                lines = lines.Concat(resourceCoverageReports.GroupBy(x => x.ContentHash)
                        .Select(group =>
                            $"  Hash = {group.Key} for pages:\r\n{group.Select(x => $"    {x.PageUrl}").AggregateLinesToString()}"))
                    .ToArray();
                _logger.Warn(lines.AggregateLinesToString());
            }
        }

        private async Task WriteCoveredFiles(string outputFolder,
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
                    var coveredResourceContent =
                        GetCoveredResourceContent(resourceContent, x.AggregatedCoverageReport.Ranges);

                    var url = x.AggregatedCoverageReport.Url.AbsoluteUri.Replace(
                        x.AggregatedCoverageReport.Url.Scheme + "://", "");
                    var fullPath = Path.Combine(outputFolder, url.Replace('/', Path.DirectorySeparatorChar));
                    if (fullPath.EndsWith(Path.DirectorySeparatorChar))
                    {
                        fullPath += "_";
                    }

                    var info = new FileInfo(fullPath);
                    var outputFileName = Path.Combine(info.Directory.FullName.ReplaceInvalidPathChars("_"),
                        info.Name.ReplaceInvalidFileNameChars("_"));
                    if (outputFileName.Length > 260)
                    {
                        outputFileName = outputFileName.Substring(0, 260);
                    }
                    
                    await _fileSystem.WriteTextToFileAsync(outputFileName, coveredResourceContent, cancellationToken)
                        .ConfigureAwait(false);
                    _logger.Info($"Covered part of resource {x.AggregatedCoverageReport.Url.AbsoluteUri} is written to file {outputFileName}");
                })).ToArray();

            await tasks.WhenAllTasks(_logger, cancellationToken).ConfigureAwait(false);
        }
    }
}
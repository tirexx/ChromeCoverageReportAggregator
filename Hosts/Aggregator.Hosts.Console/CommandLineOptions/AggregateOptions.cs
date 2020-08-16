using CommandLine;

namespace Aggregator.Hosts.Console.CommandLineOptions
{
    [Verb("aggregate", HelpText = "Aggregates coverage and outputs covered lines.")]
    public class AggregateOptions
    {
        [Option('i', "inputFolder", HelpText = "Folder containing Chrome coverage reports", Required = true)]
        public string InputFolder { get; internal set; }

        [Option('o', "outputFolder", HelpText = "Folder to store output files", Required = true)]
        public string OutputFolder { get; internal set; }

        [Option('m', "fileMask", HelpText = "Files mask. *.json by default", Default = "*.json")]
        public string FileMask { get; internal set; }
    }
}
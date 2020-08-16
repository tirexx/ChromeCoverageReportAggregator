using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Aggregator.Hosts.Console.CommandLineOptions;
using Base.Io;
using Base.Logging;
using Base.Logging.NLog;
using Base.Strings;
using CommandLine;
using NLog;
using NLog.Config;
using ILogger = Base.Logging.ILogger;

namespace Aggregator.Hosts.Console
{
    internal class Program
    {
        public static async Task<int> Main(string[] args /*, CancellationToken cancellationToken = default*/)
        {
            var logger = CreateAndConfigureNLog();
            var cancellationTokenSource = new CancellationTokenSource();
            var done = new ManualResetEventSlim(false);

            AttachTerminationHandlers(cancellationTokenSource, done, logger);
            try
            {
                var parserResult = Parser.Default.ParseArguments<
                    AggregateOptions, OtherOptions>(args);

                var fileSystem = new FileSystem();

                return await parserResult.MapResult(
                    async (AggregateOptions opts) =>
                    {
                        var aggregator = new Domain.Aggregator(fileSystem, logger);
                        await aggregator.Aggregate(opts.InputFolder, opts.FileMask, opts.OutputFolder, cancellationTokenSource.Token);
                        return 0;
                    },
                    async (OtherOptions opts) => 0, //await Other(opts, cancellationToken);
                    errors => Task.FromResult(2));
            }
            finally
            {
                done.Set();
            }
        }

        private static void AttachTerminationHandlers(CancellationTokenSource cts, ManualResetEventSlim resetEvent,
            ILogger logger)
        {
            void Shutdown()
            {
                try
                {
                    cts?.Cancel();
                }
                catch (ObjectDisposedException)
                {
                }

                resetEvent.Wait();
            }

            AssemblyLoadContext.Default.Unloading += assemblyLoadContext =>
            {
                logger.Info("Host has got sig term signal.");

                Shutdown();
            };

            AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) =>
            {
                logger.Info("Host has got process exit signal.");

                Shutdown();
            };
            System.Console.CancelKeyPress += (sender, eventArgs) =>
            {
                logger.Info("Host has got cancel signal (Ctrl+C).");
                Shutdown();
                // Don't terminate the process immediately, wait for the Main thread to exit gracefully.
                eventArgs.Cancel = true;
            };
        }

        private static ILogger CreateAndConfigureNLog()
        {
            var logConfiguringFileName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                .AddPathPart("nlog.config");

            if (!File.Exists(logConfiguringFileName))
                throw new Exception(
                    $"Mandatory nlog configuration file {logConfiguringFileName} is missing.");

            LogManager.ThrowConfigExceptions = true;
            LogManager.Configuration = new XmlLoggingConfiguration(logConfiguringFileName);

            return new LogFabric().GetLog(null);
        }
    }
}
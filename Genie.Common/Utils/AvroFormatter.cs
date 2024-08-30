using Chr.Avro.Serialization;
using CommunityToolkit.HighPerformance;
using Genie.Common.Utils;
using Microsoft.Extensions.Logging;
using System.Buffers;
using ZLogger.Formatters;
using ZLogger.Providers;

// Extending Zlogger namespace for extensions
namespace ZLogger
{
    public static class ZloggerFactory
    {
        public static ILoggerFactory GetFactory(string basePath)
        {
            var factory = LoggerFactory.Create(logging => {
                logging.SetMinimumLevel(LogLevel.Trace);

                logging.AddZLoggerRollingFile(options => {

                    // File name determined by parameters to be rotated
                    options.FilePathSelector = (timestamp, sequenceNumber) => $@"{basePath}\{timestamp.ToLocalTime():yyyy-MM-dd}_{sequenceNumber:000}.log";

                    // The period of time for which you want to rotate files at time intervals.
                    options.RollingInterval = RollingInterval.Day;

                    // Limit of size if you want to rotate by file size. (KB)
                    options.RollingSizeKB = 1024;
                });
            });

            return factory;
        }

        public static ILoggerFactory GetAvroFactory(string basePath)
        {
            var factory = LoggerFactory.Create(logging => {
                logging.SetMinimumLevel(LogLevel.Trace);

                logging.AddZLoggerRollingFile(options => {

                    options.UseAvroFormatter(LogLevel.Information);

                    // File name determined by parameters to be rotated
                    options.FilePathSelector = (timestamp, sequenceNumber) => $@"{basePath}\{timestamp.ToLocalTime():yyyy-MM-dd}_{sequenceNumber:000}.log";

                    // The period of time for which you want to rotate files at time intervals.
                    options.RollingInterval = RollingInterval.Day;

                    // Limit of size if you want to rotate by file size. (KB)
                    options.RollingSizeKB = 1024;
                });
            });

            return factory;
        }
    }

    public static class ZLoggerOptionsAvroExtensions
    {
        public static ZLoggerOptions UseAvroFormatter(this ZLoggerOptions options, LogLevel? logLevel = null, Action<AvroZLoggerFormatter>? configure = null)
        {
            return options.UseFormatter(() =>
            {
                var formatter = new AvroZLoggerFormatter(logLevel);
                configure?.Invoke(formatter);
                return formatter;
            });
        }

    }
}

namespace ZLogger.Formatters
{
    public class AvroZLoggerFormatter(LogLevel? logLevel) : IZLoggerFormatter
    {
        bool IZLoggerFormatter.WithLineBreak => false;
        readonly LogLevel? logLevel = logLevel;


        public void FormatLogEntry(IBufferWriter<byte> writer, IZLoggerEntry entry)
        {
            if (logLevel is not null && logLevel != entry.LogInfo.LogLevel)
                return;

            // BinarySerializerBuilder and BinarySerializer requires type from the log entry not the calling method<TEntry>
            var log = entry.GetParameterValue(0);
            var type = log!.GetType();
            var mi = typeof(BinarySerializerBuilder).GetMethod("BuildDelegate")!.MakeGenericMethod(type);

            var schemaBuilder = AvroSupport.GetSchemaBuilder();
            var serializerBuilder = AvroSupport.GetSerializerBuilder();

            dynamic serializer = mi.Invoke(serializerBuilder,
                [schemaBuilder.BuildSchema(type), null])!;

            // CommunityToolkit.HighPerformance for AsStream() support
            serializer((dynamic)log, new Chr.Avro.Serialization.BinaryWriter(writer.AsStream()));
        }
    }
}
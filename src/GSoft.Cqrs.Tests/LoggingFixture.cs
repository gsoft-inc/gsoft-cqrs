using Microsoft.Extensions.Logging;

using Serilog;
using Serilog.Extensions.Logging;

using ILogger = Serilog.ILogger;

namespace GSoft.Cqrs.Tests;

public sealed class LoggingFixture
{
    private static readonly ILogger Logger = new LoggerConfiguration().WriteTo.TestCorrelator().CreateLogger();

    static LoggingFixture()
    {
        Log.Logger = Logger;
    }

    public ILogger GetLogger()
    {
        return Logger;
    }

    public ILogger<T> GetLogger<T>()
    {
        var logger = this.GetLogger();

        var microsoftLoggerLibrary = new LoggerFactory(
            new[]
            {
                new SerilogLoggerProvider(logger),
            });

        return microsoftLoggerLibrary.CreateLogger<T>();
    }
}
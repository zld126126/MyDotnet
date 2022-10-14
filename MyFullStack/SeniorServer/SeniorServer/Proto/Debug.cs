using Serilog;
using System;
using System.IO;
using System.Text;

/// <summary>
/// 打印日志
/// </summary>
public class Debug:Singleton<Debug>
{
    //日志log
    public ILogger Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy/mm/dd HH:mm:ss}][{Level}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File("logfile.log", rollingInterval: RollingInterval.Day, outputTemplate: "[{Timestamp:yyyy/mm/dd HH:mm:ss}/{Level}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

    public void Log(object message)
    {
        Logger.Debug<object>(message.ToString(), message);
    }

    public void Log(string format, params object[] args)
    {
        Logger.Debug(string.Format(format, args).ToString());
    }

    public void LogInfo(object message)
    {
        Logger.Debug<object>(message.ToString(), message);
    }

    public void LogInfo(string format, params object[] args)
    {
        Logger.Debug(string.Format(format, args).ToString());
    }

    public void LogWarn(object message)
    {
        Logger.Warning<object>(message.ToString(), message);
    }

    public void LogWarn(string format, params object[] args)
    {
        Logger.Warning(string.Format(format, args).ToString());
    }

    public void LogError(object message)
    {
        Logger.Error<object>(message.ToString(), message);
    }

    public void LogError(string format, params object[] args)
    {
        Logger.Error(string.Format(format, args).ToString());
    }

    public void LogFatal(object message)
    {
        Logger.Fatal<object>(message.ToString(), message);
    }

    public void LogFatal(string format, params object[] args)
    {
        Logger.Fatal(string.Format(format, args).ToString());
    }
}

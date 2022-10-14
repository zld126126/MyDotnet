using log4net;
using log4net.Config;
using System;
using System.IO;
using System.Text;

public static class Debug
{
    private static ILog m_Log;

    static Debug()
    {
        XmlConfigurator.ConfigureAndWatch(new FileInfo(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile));
        m_Log = LogManager.GetLogger(typeof(Debug));
    }

    public static void Log(string format, params object[] args)
    {
        if (args.Length == 0)
        {
            m_Log.Debug(format);
            return;
        }
        m_Log.DebugFormat(format, args);
    }

    public static void LogInfo(string format, params object[] args)
    {
        if (args.Length == 0)
        {
            m_Log.Info(format);
            return;
        }
        m_Log.InfoFormat(format, args);
    }

    public static void LogWarn(string format, params object[] args)
    {
        if (args.Length == 0)
        {
            m_Log.Warn(format);
            return;
        }
        m_Log.WarnFormat(format, args);
    }

    public static void LogError(string format, params object[] args)
    {
        if (args.Length == 0)
        {
            m_Log.Error(format);
            return;
        }
        m_Log.ErrorFormat(format, args);
    }

    public static void LogFatal(string format, params object[] args)
    {
        if (args.Length == 0)
        {
            m_Log.Fatal(format);
            return;
        }
        m_Log.FatalFormat(format, args);
    }
}

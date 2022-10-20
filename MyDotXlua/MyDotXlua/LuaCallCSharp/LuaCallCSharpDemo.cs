using System;
using System.Collections.Generic;
using System.Text;
using XLua;

[LuaCallCSharp]
public class LuaCallCSharpDemo
{
    private static readonly DateTime s_UTCTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public LuaCallCSharpDemo() { 
    
    }

    /// <summary>
    /// 获取当前秒
    /// </summary>
    /// <returns></returns>
    public long GetCurrentSeconds()
    {
        return Convert.ToInt64(DateTime.UtcNow.Subtract(s_UTCTime).TotalSeconds);
    }
}

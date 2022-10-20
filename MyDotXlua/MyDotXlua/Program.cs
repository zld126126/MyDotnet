using System;
using XLua;

namespace MyDotXlua
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("CSharp-Hello World!");

            //1.创建一个Xlua
            LuaEnv luaenv = new LuaEnv();

            //2.设置全局LuaCallCSharp方法
            LuaCallCSharpDemo CSharpSystem = new LuaCallCSharpDemo();
            luaenv.Global.Set("CSharpSystem", CSharpSystem);

            //3.执行.lua文件
            luaenv.DoString("dofile('MyDotXlua.lua')");

            //4.关闭Xlua
            //luaenv.Dispose();

            //5.不退出
            Console.ReadLine();
        }
    }
}

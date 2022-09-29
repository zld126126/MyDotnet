using Microsoft.AspNetCore.Mvc;
using MoonSharp.Interpreter;
using System;

namespace MyCore.Controllers
{
    [Route("api/demo")]
    [ApiController]
    public class DemoController : Controller
    {
        [Route("test")]
        public string Test(string test)
        {
            return "Test:" + test;
        }

        [Route("getlua")]
        public string GetLua()
        {
            Console.WriteLine("GetLua...");
            Script script = new Script();
            script.DoString(@"
            function test(a)
                return a*3
            end
            ");
            DynValue luaFunction = script.Globals.Get("test");
            DynValue res = script.Call(luaFunction,5);
            return "GetLua:"+res.Number;
        }
    }
}

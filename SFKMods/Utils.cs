using SFKMod.Mods;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFKMod
{
    public class Utils
    {
        public static void LogStackTrace(int maxStackSize = 10)
        {
            var stackTrace = new StackTrace(1, false);
            var stackFrames = stackTrace.GetFrames().Take(maxStackSize);
            foreach (var frame in stackFrames)
            {
                Plugin.Logger.LogInfo($" at {frame}");
            }
        }
    }
}

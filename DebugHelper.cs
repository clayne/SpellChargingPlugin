using NetScriptFramework.SkyrimSE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellChargingPlugin
{
    public static class DebugHelper
    {
        public static void Print(string message)
        {
            if (Settings.LogDebugMessages)
            {
                SpellCharging._logFile.AppendLine(message);
                //MenuManager.ShowHUDMessage(message, null, true);
            }
        }
    }
}

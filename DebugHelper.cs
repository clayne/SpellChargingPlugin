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
            if(Settings.ShowDebugMessages)
                MenuManager.ShowHUDMessage(message, null, true);
        }
    }
}

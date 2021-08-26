using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellChargingPlugin
{
    public static class Settings
    {
        public static float ChargeInterval { get; internal set; }
            = 1000.0f;
        public static float ChargeIncrement { get; internal set; }
            = 0.1f;
        public static bool ApplyVisuals { get; internal set; }
            = false;
        public static bool ShowDebugMessages { get; internal set; }
            = true;
    }
}

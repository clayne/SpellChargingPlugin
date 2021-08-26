using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellChargingPlugin
{
    public static class Settings
    {
        public const float UpdateRate = 1.0f / 30.0f;
        public static float ChargeInterval { get; internal set; }
            = 0.1f;
        public static float ChargeIncrement { get; internal set; }
            = 0.05f;
        public static bool ApplyVisuals { get; internal set; }
            = false;
        public static bool LogDebugMessages { get; internal set; }
            = true;
        public static float MagickaPercentagePerCharge { get; internal set; }
            = 0.05f;
    }
}

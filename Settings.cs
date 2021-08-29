using NetScriptFramework.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellChargingPlugin
{
    public static class Settings
    {
        public const float UpdateRate = 1.0f / 60.0f;

        [ConfigValue("ChargeInterval", "Charge Speed", "How much time has to pass before gaining a charge? In seconds.")]
        public static float ChargeInterval { get; internal set; }
            = 1f / 5000f;

        [ConfigValue("LogDebugMessages", "Log", "Write debug output to file.")]
        public static bool LogDebugMessages { get; internal set; }
            = true;

        [ConfigValue("CostPerCharge", "Charge Cost", "How much mana is needed for one charge? In percent, based on the spell's mana cost.")]
        public static float CostPerCharge { get; internal set; }
            = 1f / 10f;

        [ConfigValue("PowerPerCharge", "Charge Power", "How much power does one charge add? In percent.")]
        public static float PowerPerCharge { get; internal set; }
            = 1f / 10f;

        [ConfigValue("MaxParticles", "Max Particles", "How many swirly things would you like?")]
        public static float MaxParticles { get; internal set; }
            = 100000f;
    }
}

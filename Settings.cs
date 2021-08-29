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
        // Refresh rate for logic. 48 ups is enough.
        public const float UpdateRate = 1.0f / 48f;

        [ConfigValue("ChargeInterval", "Charge Speed", "How much time has to pass before gaining a charge? In seconds.")]
        public static float ChargeInterval { get; internal set; }
            = 1f / 20f;

        [ConfigValue("LogDebugMessages", "Log", "Write debug output to file.")]
        public static bool LogDebugMessages { get; internal set; }
            = false;

        [ConfigValue("PowerPerHundredCharges", "Charge Power", "How much power will a hundred charges (100 mana spent) add? In percent.")]
        public static float PowerPerHundredCharges { get; internal set; }
            = 1f / 1f;

        [ConfigValue("MaxChargeForParticles", "Max Charge Particles", "At what charge level do the swirly bits stop appearing?")]
        public static float MaxChargeForParticles { get; internal set; }
            = 100f;

        [ConfigValue("HalfPowerWhenMagAndDur", "Halve Power for Duo-Effect", "Should effects that have both a duration and a magnitude receive halved benefit?")]
        public static bool HalfPowerWhenMagAndDur { get; internal set; }
            = true;

        [ConfigValue("ChargesPerParticle", "Charges Per Particle", "Spawn a swirly bit every N charges.")]
        public static int ChargesPerParticle { get; internal set; }
            = 5;
    }
}

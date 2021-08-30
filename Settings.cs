using NetScriptFramework.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellChargingPlugin
{
    public sealed class Settings
    {
        private static Settings _instance;
        public static Settings Instance => _instance ?? (_instance = new Settings());

        static Settings()
        {
            ConfigFile.LoadFrom<Settings>(_instance, "m3SpellCharging", true);
        }

        // Refresh rate for logic. 48 ups is enough.
        public const float MainLoopUPS = 1.0f / 48f;

        [ConfigValue("ChargeInterval", "Charge Speed", "How much time has to pass before gaining a charge? In seconds.")]
        public float ChargeInterval { get; internal set; }
            = 1f / 20f;

        [ConfigValue("LogDebugMessages", "Log", "Write debug output to file.")]
        public bool LogDebugMessages { get; internal set; }
            = false;

        [ConfigValue("PowerPerCharge", "Power per Charge", "One charge will raise spell power by this amount. In percent.")]
        public float PowerPerCharge { get; internal set; }
            = 1f / 100f;

        [ConfigValue("MaxChargeForParticles", "Max Charge Particles", "At what charge level do the swirly bits stop appearing?")]
        public float MaxChargeForParticles { get; internal set; }
            = 100f;

        [ConfigValue("HalfPowerWhenMagAndDur", "Halve Power for Duo-Effect", "Should effects that have both a duration and a magnitude receive halved benefit?")]
        public bool HalfPowerWhenMagAndDur { get; internal set; }
            = true;

        [ConfigValue("ChargesPerParticle", "Charges Per Particle", "Spawn a swirly bit every N charges.")]
        public int ChargesPerParticle { get; internal set; }
            = 5;

        [ConfigValue("EquipBothFormID", "Equip Both FormID", "Don't touch this")]
        public uint EquipBothFormID { get; internal set; }
            = 0x00013F45;

        [ConfigValue("MaxParticles", "Max Particles", "Maximum number of particles to spawn.")]
        public int MaxParticles { get; internal set; }
            = 100;
    }
}

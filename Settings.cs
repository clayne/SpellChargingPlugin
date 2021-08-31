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
        public static Settings Instance => _instance ?? (_instance = Create());

        private static Settings Create()
        {
            var ret = new Settings();
            ConfigFile.LoadFrom<Settings>(ret, "SpellChargingPlugin", true);
            return ret;
        }
        private Settings(){ }

        [ConfigValue("UpdatesPerSecond", "Updates Per Second", "Controls how often the plugin updates its state. Leave at 30 UPS or raise it up to your maximum FPS if you want the particle effects to look smoother.")]
        public uint UpdatesPerSecond { get; set; } 
            = 30;

        [ConfigValue("ChargesPerSecond", "Charges Per Second", "How many charges are gained per second of charging?")]
        public uint ChargesPerSecond { get; internal set; }
            = 5;

        [ConfigValue("PowerPerCharge", "Power Per Charge", "One charge will raise spell power by this amount in percent.")]
        public float PowerPerCharge { get; internal set; }
            = 5.0f;

        [ConfigValue("MagickaPerCharge", "Magicka Per Charge", "How much Magicka does one charge cost? This is a flat value, not a percentage!")]
        public float MagickaPerCharge { get; internal set; }
            = 2.5f;

        [ConfigValue("HalfPowerWhenMagAndDur", "Halve Power for Duo-Effect", "Should effects that have both a duration and a magnitude receive reduced benefit (balance)?")]
        public bool HalfPowerWhenMagAndDur { get; internal set; }
            = true;

        [ConfigValue("ChargesPerParticle", "Charges Per Particle", "Spawn a swirly bit every N charges. Setting this to 1 would spawn a particle on every new charge.")]
        public uint ChargesPerParticle { get; internal set; }
            = 5;

        [ConfigValue("MaxParticles", "Max Particles", "Maximum number of particles to spawn in total (counting both hands). Don't go too crazy as these are somewhat CPU intensive.")]
        public uint MaxParticles { get; internal set; }
            = 200;

        [ConfigValue("ParticleLayers", "Particle Layers", "How many layers of particles are you on? Actual number depends on the spell. Too few layers cause some spells not to spawn visible particles.")]
        public uint ParticleLayers { get; internal set; }
            = 2;

        [ConfigValue("AllowConcentrationSpells", "Allow Concentration Spells", "Apply Spell Charging to Concentration-Type spells? Recommended to disable this for now because it doesn't work properly. Won't cause any issues, but will most likely just waste mana because the stronger magnitude only takes effect after the previous effect expires (usually after one second), so you would need to aim your Flames away for a second while still concentrating, and then reapply Flames on the enemy. Will not work on Healing. Should work with Wards. Because of all that, Concentration spells will drain 90% less mana per charge.")]
        public bool AllowConcentrationSpells { get; internal set; }
            = true;

        [ConfigValue("LogDebugMessages", "Log", "Write debug output to file. If usng MO, look inside your Overwrite folder. Leave disabled unless you experience issues.")]
        public bool LogDebugMessages { get; internal set; }
            = false;

        [ConfigValue("EquipBothFormID", "EquipBoth FormID", "Don't touch this.", ConfigEntryFlags.PreferHex)]
        public uint EquipBothFormID { get; internal set; }
            = 0x00013F45;
    }
}

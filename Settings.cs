using NetScriptFramework.Tools;
using SpellChargingPlugin.Core;
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

        [ConfigValue("OperationMode", "Default Mode", 
            "By default, only a spell's magnitude (damage/healing/armor..) will be affected.\n" +
            "Options: Magnitude, Duration, Disabled\nYou can also switch between them ingame by pressing the hotkey.\n" +
            "Spells with no magnitude (like summons) will not be charged and won't consume magicka in Duration mode and vice versa.\n" +
            "It is possible to switch modes while charging.")]
        public string OperationMode { get; internal set; }
            = "Magnitude";

        [ConfigValue("HotKey", "Hotkey", 
            "Key or key combination to toggle between modes.")]
        public string HotKey { get; internal set; }
            = "Shift + G";

        [ConfigValue("PreChargeDelay", "Pre-charge Delay",
            "Time (in seconds) that a spell must be held before it starts charging.")]
        public float PreChargeDelay { get; internal set; }
            = 0.5f;

        [ConfigValue("PowerPerCharge", "Power Per Charge", 
            "One charge will raise power by this amount in percent.\n" +
            "Recommended to set this and MagickaPerCharge to the same value in order to have spells gain 100% power for every 100 points of Magicka spent on charges. ")]
        public float PowerPerCharge { get; internal set; }
            = 10.0f;

        [ConfigValue("SkillAffectsPower", "Skill affects Power", 
            "Should your skill level further influence the power?\n" +
            "If enabled and with PowerPerCharge set to 10, actual PowerPerCharge would be 20 at 100 skill level (+1% per skill level).")]
        public bool SkillAffectsPower { get; internal set; }
            = true;

        [ConfigValue("MagickaPerCharge", "Magicka Per Charge", 
            "How much Magicka does one charge cost? This is a flat value, not a percentage!")]
        public float MagickaPerCharge { get; internal set; }
            = 10f;

        [ConfigValue("ChargesPerSecond", "Charges Per Second", 
            "How many charges are gained per second of charging?\n" +
            "Set to 0 to disable the particle system.")]
        public uint ChargesPerSecond { get; internal set; }
            = 5;

        [ConfigValue("ChargesPerParticle", "Charges Per Particle", 
            "Spawn a charge indicator particle every #N charges. Setting this to 1 would spawn a particle on every new charge.\n" +
            "With the default settings, every particle would indicate that your spell has become 50% stronger.")]
        public uint ChargesPerParticle { get; internal set; }
            = 5;

        [ConfigValue("ParticleScale", "Particle Scale", 
            "Make the particles larger (>1.0) or smaller (<1.0).")]
        public float ParticleScale { get; internal set; }
            = 1.0f;

        [ConfigValue("AllowConcentrationSpells", "Allow Concentration Spells", 
            "Apply Spell Charging to Concentration-Type spells?")]
        public bool AllowConcentrationSpells { get; internal set; }
           = true;

        [ConfigValue("UpdatesPerSecond", "Updates Per Second", 
            "Performance setting. Controls how often the plugin updates its state.\n" +
            "Leave at 30 UPS or raise it up to your maximum FPS if you want the plugin to be more responsive and the charge particle effects to look a little smoother.")]
        public uint UpdatesPerSecond { get; internal set; }
            = 30;

        [ConfigValue("MaxParticles", "Max Particles", 
            "Performance setting. Maximum number of particles to spawn in TOTAL (counting spells in both hands). Don't go too crazy.\n" +
            "Set to 0 to disable the effect.")]
        public uint MaxParticles { get; internal set; }
            = 100;

        [ConfigValue("ParticleLayers", "Particle Layers", 
            "Debug setting. Too few layers cause some spells not to spawn visible particles while charging. Best to leave at 2.")]
        public uint ParticleLayers { get; internal set; }
            = 2;

        [ConfigValue("LogDebugMessages", "Log", 
            "Write debug output to file. Disable unless you experience issues.")]
        public bool LogDebugMessages { get; internal set; }
            = true;

        [ConfigValue("ArtObjectMagnitude", "ArtObject for Magnitude", 
            "The FormID of the ARTO that gets attached when in 'Magnitude' overcharge mode. Set to 0 to disable.\n" +
            "Note: This (default value) will produce a red glow when combined with ENB Light!\n" +
            "Set to 0 to disable.", ConfigEntryFlags.PreferHex)]
        public uint ArtObjectMagnitude { get; internal set; }
            = 0x74795;

        [ConfigValue("ArtObjectDuration", "ArtObject for Duration", 
            "The FormID of the ARTO that gets attached when in 'Duration' overcharge mode.\n" +
            "Note: This (default value) will produce a green glow when combined with ENB Light!\n" +
            "Set to 0 to disable.", ConfigEntryFlags.PreferHex)]
        public uint ArtObjectDuration { get; internal set; }
            = 0x6DE86;

        [ConfigValue("EquipBothFormID", "EquipBoth FormID", 
            "Don't touch this.", ConfigEntryFlags.PreferHex | ConfigEntryFlags.Hidden)]
        public uint EquipBothFormID { get; internal set; }
            = 0x00013F45;
    }
}

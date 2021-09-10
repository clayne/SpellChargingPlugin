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

        [ConfigValue("OperationMode", "Overcharge Priority", 
            "When a spell has both a Magnitude and a Duration (most Illusion spells, Flesh spells, etc.), which of the two should be charged?\n" +
            "Options: Magnitude, Duration\n" +
            "You can also switch between them ingame by pressing the hotkey.\n" +
            "If a spell does not have the prioritized attribute, it will charge the other one.\n" +
            "If a spell has neither (Clairvoyance and other script-based spells), nothing will happen and no magicka will be spent.\n" +
            "It is possible to switch priorities while in the middle of charging.")]
        public string OperationMode { get; internal set; }
            = "Magnitude";

        [ConfigValue("HotKey", "Hotkey", 
            "Key or key combination to toggle between modes.")]
        public string HotKey { get; internal set; }
            = "Shift + G";

        [ConfigValue("PreChargeDelay", "Pre-charge Delay",
            "Time (in seconds) that a non-concentration spell must be held before it begins overcharging.\n" +
            "Set to 0 to begin overcharging immediately.")]
        public float PreChargeDelay { get; internal set; }
            = 0.5f;

        [ConfigValue("PowerPerCharge", "Power Per Charge", 
            "One charge will raise power by this amount in percent.\n" +
            "Note: 'Spell Power' also includes a spell's projectile speed, size, explosion radius, range and impact force (if it has any of those). Those will charge at a slower rate than the two main ones.")]
        public float PowerPerCharge { get; internal set; }
            = 10.0f;

        [ConfigValue("MagickaPerCharge", "Magicka Per Charge", 
            "How much Magicka does one charge cost? This is a flat value, not a percentage!")]
        public float MagickaPerCharge { get; internal set; }
            = 10f;

        [ConfigValue("SkillAffectsPower", "Skill affects Power",
            "Should your skill level in the spell's school further influence the power per charge?")]
        public bool SkillAffectsPower { get; internal set; }
            = true;

        [ConfigValue("ChargesPerSecond", "Charges Per Second", 
            "How many charges are gained per second of charging?\n" +
            "Increasing this setting while decreasing the ones above will give a more granular gain in power at the cost of a tiny hit to performance and stability.")]
        public uint ChargesPerSecond { get; internal set; }
            = 3;

        [ConfigValue("ChargesPerParticle", "Charges Per Particle", 
            "Spawn a charge indicator particle every #N charges. Setting this to 1 would spawn a particle on every new charge.\n" +
            "Particle appearance depends on your mods (spell changes, custom spells, mesh changes, textures etc). Some spells may not spawn any visible particles at all.\n" +
            "Set to 0 to disable the particle system if you think it looks bad or to increase performance a little.")]
        public uint ChargesPerParticle { get; internal set; }
            = 2;

        [ConfigValue("ParticleScale", "Particle Scale", 
            "Make the particles larger (>1.0) or smaller (<1.0). Smaller usually looks better.")]
        public float ParticleScale { get; internal set; }
            = 1.0f;

        [ConfigValue("AllowConcentrationSpells", "Allow Concentration Spells", 
            "Apply Spell Charging to Concentration-Type spells?")]
        public bool AllowConcentrationSpells { get; internal set; }
           = true;

        [ConfigValue("EnableAcceleration", "Enable Charge Acceleration",
            "Speed up charging rate based on how long you've been charging already?\n" +
            "For the impatient wizard.")]
        public bool EnableAcceleration { get; internal set; }
            = true;

        [ConfigValue("AccelerationHalfTime", "Acceleration Halving Time",
            "After charging for this long (in seconds), charging speed will double.\n" +
            "The acceleration is gradual and scales beyond this time, e.g. twice as fast after 5s, three times as fast after 10s, four times after 15s and so on.")]
        public float AccelerationHalfTime { get; internal set; }
            = 5f;

        [ConfigValue("UpdatesPerSecond", "Updates Per Second", 
            "Controls how often the plugin updates its state.\n" +
            "Raise it up to your maximum FPS if you want the plugin to be more responsive and the particle effects to look a little smoother at a very small performance cost.\n" +
            "Should also probably increase this when you increase ChargesPerSecond and/or reduce AccelerationHalfTime.")]
        public uint UpdatesPerSecond { get; internal set; }
            = 30;

        [ConfigValue("MaxParticles", "Max Particles", 
            "Maximum number of particles to spawn per hand. Don't go too crazy.\n" +
            "Set to 0 to disable particles.")]
        public uint MaxParticles { get; internal set; }
            = 100;

        [ConfigValue("ParticleLayers", "Particle Layers", 
            "Too few layers cause some spells not to spawn visible particles while charging. Too many will either do nothing or produce ugly results.\n" +
            "Best to leave at 2.")]
        public uint ParticleLayers { get; internal set; }
            = 2;

        [ConfigValue("LogDebugMessages", "Log", 
            "Write debug output to file. Disable unless you experience issues.")]
        public bool LogDebugMessages { get; internal set; }
            = true;

        [ConfigValue("ArtObjectMagnitude", "ArtObject for Magnitude", 
            "The FormID of the ARTO that gets attached when in 'Magnitude' overcharge mode. Set to 0 to disable.\n" +
            "Set to 0 to disable the effect.", ConfigEntryFlags.PreferHex | ConfigEntryFlags.Hidden)]
        public uint ArtObjectMagnitude { get; internal set; }
            = 0x74795;

        [ConfigValue("ArtObjectDuration", "ArtObject for Duration", 
            "The FormID of the ARTO that gets attached when in 'Duration' overcharge mode.\n" +
            "Set to 0 to disable the effect.", ConfigEntryFlags.PreferHex | ConfigEntryFlags.Hidden)]
        public uint ArtObjectDuration { get; internal set; }
            = 0x6DE86;

        [ConfigValue("AutoCleanupDelay", "Auto-Cleanup Delay",
            "Best to leave this alone.", ConfigEntryFlags.Hidden)]
        public float AutoCleanupDelay { get; internal set; }
            = 3.37f;
    }
}

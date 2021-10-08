using NetScriptFramework;
using NetScriptFramework.SkyrimSE;
using NetScriptFramework.Tools;
using SpellChargingPlugin.Core;
using SpellChargingPlugin.ParticleSystem;
using System;
using System.Linq;
using System.Text;

namespace SpellChargingPlugin
{
    public partial class SpellCharging : Plugin
    {
        public override string Key => "SpellChargingPlugin";
        public override string Name => "Spellcharging Plugin";
        public override string Author => "m3ttwur5t";
        public override int Version => 1;

        private static float _timePerUpdate = 1.0f / Math.Max(Settings.Instance.UpdatesPerSecond, 1.0f);

        private static ChargingActor _chargingPlayer;
        private static Utilities.SimpleTimer _actorUpdateControlTimer = new Utilities.SimpleTimer();
        private static Utilities.SimpleTimer _activeEffectPurgeControlTimer = new Utilities.SimpleTimer();

        /// <summary>
        /// NetFramework entry
        /// </summary>
        /// <param name="loadedAny"></param>
        /// <returns></returns>
        protected override bool Initialize(bool loadedAny)
        {
            SetupLogFile();
            RegisterHandlers();
            return true;
        }

        private static void SetupLogFile()
        {
            var logFile = new LogFile("SpellChargingPlugin", LogFileFlags.AutoFlush | LogFileFlags.IncludeTimestampInLine);
            DebugHelper.SetLogFile(logFile);
        }

        private static void RegisterHandlers()
        {
            Events.OnFrame.Register(
                handler:    e => Update(Memory.ReadFloat(Utilities.Addresses.addr_TimeSinceFrame)));

            // initialize the player's ChargingActor instance on game load
            Events.OnFrame.Register(
                handler: e => _chargingPlayer = _chargingPlayer ?? ChargingActor.Create(
                    new ChargingActor.ChargingActorCreationArgs() {
                        ParentCharacter = PlayerCharacter.Instance
                    }), 
                priority: 0,
                count: 1);

            Memory.WriteHook(new HookParameters()
            {
                Address = Utilities.Addresses.addr_CalculateDurationAndMagnitude,
                IncludeLength = 0x20,
                ReplaceLength = 0x20,
                Before = ctx =>
                {
                    var activeEffectPtr = ctx.CX;
                    var offenderPtr = ctx.DX;
                    var victimPtr = ctx.R8;

                    // any active effects that needs to be tracked (for magnitude updates) will ALWAYS have a source CHARACTER and a target CHARACTER (creature or another npc)
                    if (activeEffectPtr == IntPtr.Zero || offenderPtr == IntPtr.Zero || victimPtr == IntPtr.Zero)
                        return;
                    var activeEffect = MemoryObject.FromAddress<ActiveEffect>(activeEffectPtr);
                    if (activeEffect == null)
                        return;
                    var offendingCharacter = MemoryObject.FromAddress<Character>(offenderPtr);
                    if (offendingCharacter == null)
                        return;
                    var victimCharacter = MemoryObject.FromAddress<Character>(victimPtr);
                    if (victimCharacter == null)
                        return;

                    // Do this somewhere else
                    if (_activeEffectPurgeControlTimer.HasElapsed(5.0f, out var _))
                        ActiveEffectTracker.Instance.PurgeInvalids();

                    //float elapsed = Memory.ReadFloat(activeEffectPtr + 0x70);
                    //float duration = Memory.ReadFloat(activeEffectPtr + 0x74);
                    float magnitude = Memory.ReadFloat(activeEffectPtr + 0x78);

                    // If there's more than one, something's fucked up.
                    var tracked = ActiveEffectTracker.Instance
                        .Tracked(activeEffectPtr)
                        .FromOffender(offenderPtr)
                        .ForVictim(victimPtr)
                        .SingleOrDefault();
                    if (tracked == default)
                    {
                        ActiveEffectTracker.Instance
                            .Track(activeEffectPtr)
                            .FromOffender(offenderPtr)
                            .ForVictim(victimPtr)
                            .WithActiveEffect(activeEffectPtr)
                            .WithBaseEffectID(activeEffect.EffectData.Effect.FormId);
                        return;
                    }

                    // Damaging spells, despite having a positive magnitute, switch to negative magnitudes on their effects?
                    if (magnitude < 0f)
                        tracked.Sign = -1;
                    Memory.WriteFloat(activeEffectPtr + 0x78, tracked.Magnitude * tracked.Sign, true);
                    Memory.WriteFloat(activeEffectPtr + 0x74, tracked.Duration, true);
                    //DebugHelper.Print($"Updated ActiveEffect {activeEffectPtr.ToHexString()} MAG: {Memory.ReadFloat(activeEffectPtr + 0x78)} DUR: {Memory.ReadFloat(activeEffectPtr + 0x74)}");
                }
            });
        }

        private static void Update(float elapsedSeconds)
        {
            if (NetScriptFramework.SkyrimSE.Main.Instance?.IsGamePaused != false)
                return;

            Utilities.HotkeyBase.UpdateAll();
            _actorUpdateControlTimer.Update(elapsedSeconds);
            _activeEffectPurgeControlTimer.Update(elapsedSeconds);

            // update logic according to the update frequency setting
            if (!_actorUpdateControlTimer.HasElapsed(_timePerUpdate, out var elapsed))
                return;
            _chargingPlayer.Update(elapsed);
        }
    }
}

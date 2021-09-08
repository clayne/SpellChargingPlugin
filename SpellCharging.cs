using NetScriptFramework;
using NetScriptFramework.SkyrimSE;
using NetScriptFramework.Tools;
using SpellChargingPlugin.Core;
using System;
using System.Linq;

namespace SpellChargingPlugin
{
    public partial class SpellCharging : Plugin
    {
        public override string Key => "SpellChargingPlugin";
        public override string Name => "Spellcharging Plugin";
        public override string Author => "m3ttwur5t";
        public override int Version => 1;

        private static Timer _gameActiveTimer = null;
        private static long? _lastOnFrameTime = null;
        private static float _timePerUpdate = 1.0f / Math.Max(Settings.Instance.UpdatesPerSecond, 1);

        private static ChargingActor _chargingPlayer;
        private static Util.SimpleTimer _actorUpdateControlTimer = new Util.SimpleTimer();
        private static Util.SimpleTimer _activeEffectPurgeControlTimer = new Util.SimpleTimer();

        /// <summary>
        /// NetFramework entry
        /// </summary>
        /// <param name="loadedAny"></param>
        /// <returns></returns>
        protected override bool Initialize(bool loadedAny)
        {
            SetLogFile();
            HookAndRegister();
            return true;
        }

        private static void SetLogFile()
        {
            var logFile = new LogFile("SpellChargingPlugin", LogFileFlags.AutoFlush | LogFileFlags.IncludeTimestampInLine);
            DebugHelper.SetLogFile(logFile);
        }

        /// <summary>
        /// Register for OnFrame to avoid any lag and make sure things are taken care of asap
        /// </summary>
        private static void HookAndRegister()
        {
            _gameActiveTimer = new Timer();
            _gameActiveTimer.Start();

            Events.OnFrame.Register(e =>
            {
                long now = _gameActiveTimer.Time;
                long elapsedMilliSeconds = 0;
                if (_lastOnFrameTime.HasValue)
                    elapsedMilliSeconds = now - _lastOnFrameTime.Value;
                _lastOnFrameTime = now;
                Update(elapsedMilliSeconds / 1000.0f);
            });

            Hook_ActiveEffect_CalculateDurationAndMagnitude();
        }

        /// <summary>
        /// I assume this method gets called by the game whenever an actor (or object?) receives an ActiveEffect of some kind.
        /// Except for some reason the ActiveEffect CX points to randomly turns into PathingTaskData sometimes???
        /// May also just be NETFramework spazzing out, I dunno.
        /// </summary>
        private static void Hook_ActiveEffect_CalculateDurationAndMagnitude()
        {
            Memory.WriteHook(new HookParameters()
            {
                Address = Util.addr_CalculateDurationAndMagnitude,
                IncludeLength = 0x20,
                ReplaceLength = 0x20,
                After = ctx => // before? after? i don't know
                {
                    var activeEffectPtr = ctx.CX;
                    var offenderPtr = ctx.DX;
                    var victimPtr = ctx.R8;

                    var activeEffect = MemoryObject.FromAddress<ActiveEffect>(activeEffectPtr);
                    if (activeEffect == null)
                        return;

                    // May not be the best place to put
                    if (_activeEffectPurgeControlTimer.HasElapsed(5.0f, out var _))
                        ActiveEffectTracker.Instance.PurgeInvalids();

                    //float elapsed = Memory.ReadFloat(activeEffectPtr + 0x70);
                    //float duration = Memory.ReadFloat(activeEffectPtr + 0x74);
                    float magnitude = Memory.ReadFloat(activeEffectPtr + 0x78);

                    // If there's none, the ActiveEffect is being freshly applied, so no need to update.
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

                    //DebugHelper.Print($"[SpellCharging] Tracked actor MATCH: {tracked.Me.ToHexString()}");

                    // Damaging spells, despite having a positive magnitute, apparently switch to negative magnitudes on their effects?
                    // Makes sense for "valuemodifier" types, I guess. This approach misbehaves sometimes.
                    if (magnitude < 0f)
                        tracked.Sign = -1;
                    Memory.WriteFloat(activeEffectPtr + 0x78, tracked.Magnitude * tracked.Sign, true);
                    Memory.WriteFloat(activeEffectPtr + 0x74, tracked.Duration, true);

                    //DebugHelper.Print($"Updated ActiveEffect {activeEffectPtr.ToHexString()} MAG: {Memory.ReadFloat(activeEffectPtr + 0x78)} DUR: {Memory.ReadFloat(activeEffectPtr + 0x74)}");
                }
            });
        }

        protected override void Shutdown()
        {
            _chargingPlayer?.CleanArtObj();
            base.Shutdown();
        }

        private static void Update(float elapsedSeconds)
        {
            // Without this check, spell charging would continue while you are inside menus (without SkySouls or other un-pauser plugins)
            var main = NetScriptFramework.SkyrimSE.Main.Instance;
            if (main?.IsGamePaused != false)
                return;

            _actorUpdateControlTimer.Update(elapsedSeconds);
            _activeEffectPurgeControlTimer.Update(elapsedSeconds);

            if (_chargingPlayer == null)
                _chargingPlayer = new ChargingActor(PlayerCharacter.Instance);

            if (_actorUpdateControlTimer.HasElapsed(_timePerUpdate, out var elapsed))
                _chargingPlayer.Update(elapsed);
        }
    }
}

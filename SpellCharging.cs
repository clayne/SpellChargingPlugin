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

        public static CachedFormList FormList { get; private set; }

        private static float _timePerUpdate = 1.0f / Math.Max(Settings.Instance.UpdatesPerSecond, 1);
        private static TESCameraStates _lastCameraState;

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

        private static void CacheFormList()
        {
            FormList = CachedFormList.TryParse("801:SpellChargingPlugin.esp", "Overcharge", "EffectForm");
            if (FormList == null)
                throw new Exception("Failed to parse FormList from plugin SpellChargingPlugin.esp");
            DebugHelper.Print($"FormList cached with {FormList.All.Count} entries:");
            foreach (var item in FormList.All)
                DebugHelper.Print($"  {item}");
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
            Events.OnMainMenu.Register(e =>
            {
                CacheFormList();
            }, 0, 1);

            Events.OnFrame.Register(e =>
            {
                float diff = Memory.ReadFloat(Util.addr_TimeSinceFrame);
                if (diff <= 0.0f)
                    return;
                Update(diff);
            });

            Events.OnUpdateCamera.Register(e =>
            {
                var id = e.Camera.State.Id;
                if (id == _lastCameraState)
                    return;
                _lastCameraState = id;
                if (id == TESCameraStates.FirstPerson || id == TESCameraStates.ThirdPerson1 || id == TESCameraStates.ThirdPerson2)
                    _chargingPlayer?.RefreshSpellParticleNodes();
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
                Before = ctx => // before? after? i don't know
                {
                    var activeEffectPtr = ctx.CX;
                    var offenderPtr = ctx.DX;
                    var victimPtr = ctx.R8;

                    var activeEffect = MemoryObject.FromAddress<ActiveEffect>(activeEffectPtr);
                    if (activeEffect == null)
                        return;

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

            if (!_actorUpdateControlTimer.HasElapsed(_timePerUpdate, out var elapsed))
                return;

            Util.SimpleDeferredExecutor.Update(elapsed);
            _chargingPlayer.Update(elapsed);
        }
    }
}

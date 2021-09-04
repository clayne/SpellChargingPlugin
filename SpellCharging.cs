using NetScriptFramework;
using NetScriptFramework.SkyrimSE;
using NetScriptFramework.Tools;
using SpellChargingPlugin.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NetScriptFramework.SkyrimSE.ActiveEffect;

namespace SpellChargingPlugin
{
    public partial class SpellCharging : Plugin
    {
        public override string Key => "SpellChargingPlugin";
        public override string Name => "Spellcharging Plugin";
        public override string Author => "m3ttwur5t";
        public override int Version => 1;

        private NetScriptFramework.Tools.Timer _gameActiveTimer = null;
        private long? _lastOnFrameTime = null;
        private float _fUpdatesPerSecond = 1.0f / Settings.Instance.UpdatesPerSecond;

        private Core.ChargingActor _chargingPlayer;
        private Util.SimpleTimer _simpleTimer = new Util.SimpleTimer();

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

        private void SetLogFile()
        {
            var logFile = new NetScriptFramework.Tools.LogFile("SpellChargingPlugin", NetScriptFramework.Tools.LogFileFlags.AutoFlush | NetScriptFramework.Tools.LogFileFlags.IncludeTimestampInLine);
            DebugHelper.SetLogFile(logFile);
        }

        /// <summary>
        /// Register for OnFrame to avoid any lag and make sure things are taken care of asap
        /// </summary>
        private void HookAndRegister()
        {
            _gameActiveTimer = new NetScriptFramework.Tools.Timer();
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

                    //float elapsed = Memory.ReadFloat(activeEffectPtr + 0x70);
                    //float duration = Memory.ReadFloat(activeEffectPtr + 0x74);
                    float magnitude = Memory.ReadFloat(activeEffectPtr + 0x78);

                    var tracked = ActiveEffectTracker.Instance
                        .Tracked(activeEffectPtr)
                        .FromOffender(offenderPtr)
                        .ForVictim(victimPtr)
                        .SingleOrDefault();
                    if (tracked == default)
                    {
                        ActiveEffectTracker.Instance
                            .Track(activeEffectPtr)
                            .From(offenderPtr)
                            .For(victimPtr)
                            .WithEffect(activeEffectPtr)
                            .WithMagnitude(magnitude)
                            .WithBaseEffectFormID(activeEffect.EffectData.Effect.FormId);
                        return;
                    }

                    DebugHelper.Print($"[SpellCharging] Tracked actor MATCH: {tracked.Me.ToHexString()}");

                    // Damaging spells, despite having a positive magnitute, apparently switch to negative magnitudes on their effects?
                    // Makes sense for "valuemodifier" types, I guess
                    if (magnitude < 0f)
                        tracked.Sign = -1;
                    Memory.WriteFloat(activeEffectPtr + 0x78, tracked.Magnitude * tracked.Sign, true);
                }
            });
        }

        private void Update(float elapsedSeconds)
        {
            // Without this check, spell charging would work while you are inside menus (without SkySouls or other un-pauser plugins)
            var main = NetScriptFramework.SkyrimSE.Main.Instance;
            if (main?.IsGamePaused != false)
                return;

            _simpleTimer.Update(elapsedSeconds);

             if(_chargingPlayer == null)
                _chargingPlayer = new Core.ChargingActor(PlayerCharacter.Instance);

            if (_simpleTimer.HasElapsed(_fUpdatesPerSecond, out var exact))
                _chargingPlayer.Update(exact);
        }
    }
}

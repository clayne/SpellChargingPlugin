using NetScriptFramework;
using NetScriptFramework.SkyrimSE;
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
        private float _elapsedSecondsSinceUpdate = 0f;

        private float _fUpdatesPerSecond = 1.0f / Settings.Instance.UpdatesPerSecond;

        private Core.ChargingActor _chargingPlayer;

        /// <summary>
        /// NetFramework entry
        /// </summary>
        /// <param name="loadedAny"></param>
        /// <returns></returns>
        protected override bool Initialize(bool loadedAny)
        {
            Init();
            Register();
            return true;
        }

        private void Init()
        {
            var logFile = new NetScriptFramework.Tools.LogFile("SpellChargingPlugin", NetScriptFramework.Tools.LogFileFlags.AutoFlush | NetScriptFramework.Tools.LogFileFlags.IncludeTimestampInLine);
            DebugHelper.SetLogFile(logFile);
            _chargingPlayer = new Core.ChargingActor();
        }

        /// <summary>
        /// Register for OnFrame to avoid any lag and make sure things are taken care of asap
        /// </summary>
        private void Register()
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

            // TODO: try out this very naive approach
            // ActiveEffect::CalculateDurationAndMagnitude_14053DF40
            // void __fastcall sub(ActiveEffect* a1, Character* a2, MagicTarget* a3)
            IntPtr addr_CalculateDurationAndMagnitude = new IntPtr(0x14053DF40).FromBase();
            Memory.WriteHook(new HookParameters()
            {
                Address = addr_CalculateDurationAndMagnitude,
                IncludeLength = 10,
                ReplaceLength = 10,
                Before = ctx =>
                {
                    var activeEffectPtr = ctx.CX;
                    var offenderPtr = ctx.DX;
                    var victimPtr = ctx.R8;

                    ActiveEffect eff;
                    if ((eff = MemoryObject.FromAddress<ActiveEffect>(activeEffectPtr)) == null)
                        return;
                    if (MemoryObject.FromAddress<Character>(offenderPtr) == null)
                        return;
                    if (MemoryObject.FromAddress<MagicTarget>(victimPtr) == null)
                        return;
                    if (offenderPtr != PlayerCharacter.Instance.Cast<Character>())
                        return;
                    if (MemoryObject.FromAddress<Actor>(victimPtr) == null)
                        return;

                    // PeakValueMod doesn't work properly for some reason
                    if (eff.BaseEffect.Archetype == Archetypes.PeakValueMod)
                        return;

                    float elapsed = Memory.ReadFloat(activeEffectPtr + 0x70);
                    float duration = Memory.ReadFloat(activeEffectPtr + 0x74);
                    float magnitude = Memory.ReadFloat(activeEffectPtr + 0x78);
                    DebugHelper.Print($"Eff: {NativeCrashLog.GetValueInfo(activeEffectPtr)} Elapsed: {elapsed}, Dur: {duration}, Mag: {magnitude}");

                    lock (_trackedEffects)
                    {
                        if (!_trackedEffects.ContainsKey(activeEffectPtr))
                        {
                            var itm = new SpellVictimContainer() { Magnitude = magnitude, Sign = 1 };
                            itm.Victims.Add(victimPtr);
                            _trackedEffects.Add(activeEffectPtr, itm);
                        }
                        if (!_trackedEffects[activeEffectPtr].Victims.Contains(victimPtr))
                            _trackedEffects[activeEffectPtr].Victims.Add(victimPtr);

                        var exist = _trackedEffects[activeEffectPtr];
                        if (magnitude < 0)
                            exist.Sign = -1;
                        Memory.WriteFloat(activeEffectPtr + 0x78, exist.Magnitude * exist.Sign, true);
                    }

                }

            });
        }

        public static Dictionary<IntPtr, SpellVictimContainer> _trackedEffects = new Dictionary<IntPtr, SpellVictimContainer>();


        private void Update(float elapsedSeconds)
        {
            // Without this check, spell charging would work while you are inside menus (without SkySouls or other un-pauser plugins)
            var main = NetScriptFramework.SkyrimSE.Main.Instance;
            if (main?.IsGamePaused != false)
                return;

            _elapsedSecondsSinceUpdate += elapsedSeconds;
            if (_elapsedSecondsSinceUpdate < _fUpdatesPerSecond)
                return;
            _chargingPlayer.Update(_elapsedSecondsSinceUpdate);
            _elapsedSecondsSinceUpdate = 0f;
        }
    }
}

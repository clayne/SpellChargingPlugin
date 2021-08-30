using NetScriptFramework;
using NetScriptFramework.SkyrimSE;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellChargingPlugin
{
    public partial class SpellCharging : Plugin
	{
		public override string Key {
			get {
				return "M3SpellCharging";
			}
		}

		public override string Name {
			get {
				return "Spellcharging Plugin";
			}
		}

		public override int Version {
			get {
				return 1;
			}
		}

		private NetScriptFramework.Tools.Timer _gameActiveTimer = null;
		private long? _lastOnFrameTime = null;
		private float _elapsedSecondsSinceUpdate = 0f;

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
            var logFile = new NetScriptFramework.Tools.LogFile("m3SpellCharging", NetScriptFramework.Tools.LogFileFlags.AutoFlush | NetScriptFramework.Tools.LogFileFlags.IncludeTimestampInLine);
			DebugHelper.SetLogFile(logFile);
			
			_chargingPlayer = new Core.ChargingActor(PlayerCharacter.Instance);
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
        }

        private void Update(float elapsedSeconds)
		{
			// Without this check, spell charging would work while you are inside menus (without SkySouls or other un-pauser plugins)
			var main = NetScriptFramework.SkyrimSE.Main.Instance;
			if (main?.IsGamePaused != false)
				return;

			_elapsedSecondsSinceUpdate += elapsedSeconds;
			if (_elapsedSecondsSinceUpdate < Settings.MainLoopUPS)
				return;
			_chargingPlayer.Update(_elapsedSecondsSinceUpdate);
			_elapsedSecondsSinceUpdate = 0f;
		}
	}
}

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

		private NetScriptFramework.Tools.Timer _timer = null;
		private long? _lastUpdateTime = null;
		private float _timeSinceLastUpdate = 0f;
		private Core.ChargingActor _player;

		public static NetScriptFramework.Tools.LogFile _logFile;

		protected override bool Initialize(bool loadedAny)
		{
			_logFile = new NetScriptFramework.Tools.LogFile("spellcharging", NetScriptFramework.Tools.LogFileFlags.AutoFlush | NetScriptFramework.Tools.LogFileFlags.IncludeTimestampInLine);
			_player = new Core.ChargingActor();
           
			_timer = new NetScriptFramework.Tools.Timer();
			_timer.Start();

			Events.OnFrame.Register(e =>
			{
				long now = _timer.Time;
				long elapsedMilliSeconds = 0;
				if (_lastUpdateTime.HasValue)
					elapsedMilliSeconds = now - _lastUpdateTime.Value;
				_lastUpdateTime = now;
				Update(elapsedMilliSeconds / 1000.0f);
			});

			return true;
		}

		private void Update(float elapsedSeconds)
		{
			var main = NetScriptFramework.SkyrimSE.Main.Instance;
			if (main?.IsGamePaused != false)
				return;

			_timeSinceLastUpdate += elapsedSeconds;
			if (_timeSinceLastUpdate < Settings.UpdateRate)
				return;
			_player.Update(_timeSinceLastUpdate);
			_timeSinceLastUpdate = 0f;
		}
	}
}

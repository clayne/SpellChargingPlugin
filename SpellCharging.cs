using NetScriptFramework;
using NetScriptFramework.SkyrimSE;
using System;
using System.Collections.Generic;
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

		private Core.ChargingActor _player;

		protected override bool Initialize(bool loadedAny)
		{
			_player = new Core.ChargingActor(PlayerCharacter.Instance);

			_timer = new NetScriptFramework.Tools.Timer();
			_timer.Start();

			Events.OnFrame.Register(e =>
			{
				long now = _timer.Time;
				long diff = 0;
				if (_lastUpdateTime.HasValue)
					diff = now - _lastUpdateTime.Value;
				_lastUpdateTime = now;
				Update(diff / 1000.0f);
			}, 1);

			return true;
		}

		private void Update(float diff)
		{
			if(diff > 0f)
				_player.Update(diff);
		}
	}
}

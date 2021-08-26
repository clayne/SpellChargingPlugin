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

		private SpellActorState _playerState;

		protected override bool Initialize(bool loadedAny)
		{
			_playerState = new SpellActorState();

			this._timer = new NetScriptFramework.Tools.Timer();
			this._timer.Start();


			Events.OnFrame.Register(e =>
			{
				long now = this._timer.Time;
				long diff = 0;
				if (this._lastUpdateTime.HasValue)
					diff = now - this._lastUpdateTime.Value;
				this._lastUpdateTime = now;
				this.Update(diff / 1000.0f, now / 1000.0f);
			});

			Events.OnSpendMagicCost.Register(e => {
				if (!(e.Item is SpellItem spell))
					return;
				if (!(e.Spender is ActorMagicCaster casterActor))
					return;
				if (!(casterActor is Actor owner))
					return;
				if (!owner.IsPlayer)
					return;
				_playerState.OnSpendMagicka(spell);
			});

			return true;
		}

		private void Update(float diff, float now)
		{
			if(diff > 0f)
				_playerState.OnUpdate(diff);
		}
	}
}

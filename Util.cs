using NetScriptFramework.SkyrimSE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellChargingPlugin
{
    public static class Util
    {
        // ActiveEffect::CalculateDurationAndMagnitude_14053DF40
        // void __fastcall sub(ActiveEffect* a1, Character* a2, MagicTarget* a3)
        public static IntPtr addr_CalculateDurationAndMagnitude = new IntPtr(0x14053DF40).FromBase();

        private static Dictionary<string, NiAVObject> _nifCache = new Dictionary<string, NiAVObject>();
        public static NiAVObject LoadNif(string nifPath)
        {
            if (!_nifCache.TryGetValue(nifPath, out NiAVObject toLoad))
            {
                NiAVObject.LoadFromFile(
                    new NiObjectLoadParameters()
                    {
                        FileName = nifPath,
                        Callback = p =>
                        {
                            if (p.Success)
                            {
                                if (p.Result[0] is NiAVObject obj)
                                {
                                    DebugHelper.Print($"[Util] NIF: {nifPath} loaded");
                                    toLoad = obj;
                                    toLoad.IncRef();
                                    _nifCache.Add(nifPath, toLoad);
                                    DebugHelper.Print($"[Util] NIF: {nifPath} cached");
                                }
                            }
                        }
                    });
            }
            DebugHelper.Print($"[Util] NIF: Returning {toLoad} ({toLoad?.Name})");
            return toLoad;
        }

        public class SimpleTimer
        {
            private float _elapsedSeconds;

            public void Update(float elapsedSeconds)
            {
                _elapsedSeconds = elapsedSeconds;
            }

            /// <summary>
            /// Check if a certain amount of time has passed since this method has been called and reset the timer
            /// </summary>
            /// <param name="seconds"></param>
            /// <param name="elapsed">The current value of the internal timer</param>
            /// <returns></returns>
            public bool HasElapsed(float seconds, out float elapsed, bool reset = true)
            {
                elapsed = _elapsedSeconds;
                if (_elapsedSeconds < seconds)
                    return false;
                if(reset) 
                    _elapsedSeconds = 0.0f;
                return true;
            }
        }
    }
}

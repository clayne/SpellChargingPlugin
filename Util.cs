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

        public class SimpleAverager
        {
            private float[] _history;
            private int _histIndex = 0;
            public SimpleAverager(uint history)
            {
                _history = new float[history];
            }

            public float GetAverage(float current)
            {
                _history[_histIndex++] = current;
                if (_histIndex >= _history.Length)
                    _histIndex = 0;
                return _history.Average();
            }
        }
    }
}

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
    }
}

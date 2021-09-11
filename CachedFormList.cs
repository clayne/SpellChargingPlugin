using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellChargingPlugin
{
    /// <summary>
    /// Cached form list for lookups later.
    /// </summary>
    public sealed class CachedFormList
    {
        /// <summary>
        /// Prevents a default instance of the <see cref="CachedFormList"/> class from being created.
        /// </summary>
        private CachedFormList()
        {
        }

        /// <summary>
        /// The forms.
        /// </summary>
        private readonly List<NetScriptFramework.SkyrimSE.TESForm> Forms = new List<NetScriptFramework.SkyrimSE.TESForm>();

        /// <summary>
        /// The ids.
        /// </summary>
        private readonly HashSet<uint> Ids = new HashSet<uint>();

        /// <summary>
        /// Tries to parse from input. Returns null if failed.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="pluginForLog">The plugin for log.</param>
        /// <param name="settingNameForLog">The setting name for log.</param>
        /// <param name="warnOnMissingForm">If set to <c>true</c> warn on missing form.</param>
        /// <param name="dontWriteAnythingToLog">Don't write any errors to log if failed to parse.</param>
        /// <returns></returns>
        public static CachedFormList TryParse(string input, string pluginForLog, string settingNameForLog, bool warnOnMissingForm = true, bool dontWriteAnythingToLog = false)
        {
            if (string.IsNullOrEmpty(settingNameForLog))
                settingNameForLog = "unknown form list setting";
            if (string.IsNullOrEmpty(pluginForLog))
                pluginForLog = "unknown plugin";

            var ls = new CachedFormList();
            var spl = input.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var x in spl)
            {
                string idstr;
                string file;

                int ix = x.IndexOf(':');
                if (ix <= 0)
                {
                    if (!dontWriteAnythingToLog)
                        NetScriptFramework.Main.Log.AppendLine("Failed to parse " + settingNameForLog + " for " + pluginForLog + "! Invalid input: `" + x + "`.");
                    return null;
                }

                idstr = x.Substring(0, ix);
                file = x.Substring(ix + 1);

                if (!idstr.All(q => (q >= '0' && q <= '9') || (q >= 'a' && q <= 'f') || (q >= 'A' && q <= 'F')))
                {
                    if (!dontWriteAnythingToLog)
                        NetScriptFramework.Main.Log.AppendLine("Failed to parse " + settingNameForLog + " for " + pluginForLog + "! Invalid form ID: `" + idstr + "`.");
                    return null;
                }

                if (string.IsNullOrEmpty(file))
                {
                    if (!dontWriteAnythingToLog)
                        NetScriptFramework.Main.Log.AppendLine("Failed to parse " + settingNameForLog + " for " + pluginForLog + "! Missing file name.");
                    return null;
                }

                uint id = 0;
                if (!uint.TryParse(idstr, System.Globalization.NumberStyles.HexNumber, null, out id))
                {
                    if (!dontWriteAnythingToLog)
                        NetScriptFramework.Main.Log.AppendLine("Failed to parse " + settingNameForLog + " for " + pluginForLog + "! Invalid form ID: `" + idstr + "`.");
                    return null;
                }

                var form = NetScriptFramework.SkyrimSE.TESForm.LookupFormFromFile(id, file);
                if (form == null)
                {
                    if (!dontWriteAnythingToLog && warnOnMissingForm)
                        NetScriptFramework.Main.Log.AppendLine("Failed to find form " + settingNameForLog + " for " + pluginForLog + "! Form ID was " + id.ToString("X") + " and file was " + file + ".");
                    continue;
                }

                if (ls.Ids.Add(form.FormId))
                    ls.Forms.Add(form);
            }

            return ls;
        }

        /// <summary>
        /// Determines whether this list contains the specified form.
        /// </summary>
        /// <param name="form">The form.</param>
        /// <returns></returns>
        public bool Contains(NetScriptFramework.SkyrimSE.TESForm form)
        {
            if (form == null)
                return false;

            return Contains(form.FormId);
        }

        /// <summary>
        /// Determines whether this list contains the specified form identifier.
        /// </summary>
        /// <param name="formId">The form identifier.</param>
        /// <returns></returns>
        public bool Contains(uint formId)
        {
            return this.Ids.Contains(formId);
        }

        /// <summary>
        /// Gets all forms in this list.
        /// </summary>
        /// <value>
        /// All.
        /// </value>
        public IReadOnlyList<NetScriptFramework.SkyrimSE.TESForm> All {
            get {
                return this.Forms;
            }
        }
    }
}

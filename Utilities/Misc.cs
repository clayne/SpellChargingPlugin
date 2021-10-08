using NetScriptFramework.SkyrimSE;
using NetScriptFramework.Tools;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SpellChargingPlugin.Utilities
{
    public static class Misc
    {
        /// <summary>
        /// Includes the character itself
        /// </summary>
        /// <param name="character"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public static IEnumerable<Character> GetCharactersInRange(Character character, float range)
        {
            var charCell = character.ParentCell;
            charCell.CellLock.Lock();
            try
            {
                var set = charCell
                    .References?
                    .Where(ptr => ptr?.Value != null && ptr.Value is Character)
                    .Select(ptr => ptr.Value as Character)
                    .Where(chr => chr.Position.GetDistance(character.Position) <= range)
                    .ToHashSet();
                return set;
            }
            finally
            {
                charCell.CellLock.Unlock();
            }
        }
    }
}

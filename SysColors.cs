using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Reflection;
using System.Drawing;

namespace datebasedW
{
    public class SysColors
    {
        public IEnumerable<Color> SSColors()
        {
            foreach(KnownColor col in Enum.GetValues(typeof(KnownColor)))
            {
                Color co = Color.FromKnownColor(col);

                if (co.IsSystemColor) continue;
                else if (!co.IsNamedColor) continue;
                else if (col == KnownColor.Transparent) continue;
                else
                    yield return co;
            }
        }
    }
}

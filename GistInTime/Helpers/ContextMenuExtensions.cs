using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GistInTime.Helpers
{
    public static class ContextMenuExtensions
    {
        public static void AppendCommand(this ContextMenu cm, string displayName, EventHandler handler)
        {
            var command = new MenuItem(displayName, handler);
            cm.MenuItems.Add(command);
        }

        public static void AppendSeparator(this ContextMenu cm)
        {
            var separator = new MenuItem("-");
            cm.MenuItems.Add(separator);
        }
    }
}

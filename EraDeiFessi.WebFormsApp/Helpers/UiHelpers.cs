using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;

namespace EraDeiFessi.WebFormsApp.Helpers
{
    public static class UiHelpers
    {


        public static Control FindControlRecursive(this Control root, string ID)
        {
            Control control = root.FindControl(ID);
            if (control != null)
                return control;
            else
            {
                foreach (Control item in root.Controls)
                {
                    control = item.FindControlRecursive(ID);
                    if (control != null)
                        return control;
                }
            }
            return null;
        }

        public static IEnumerable<T> FindControlsOfType<T>(this Control parent) where T : Control
        {
            foreach (Control child in parent.Controls)
            {
                if (child is T)
                {
                    yield return (T)child;
                }
                else if (child.Controls.Count > 0)
                {
                    foreach (T grandChild in child.FindControlsOfType<T>())
                    {
                        yield return grandChild;
                    }
                }
            }
        }

        
    }
}

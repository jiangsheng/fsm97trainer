using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FSM97Patcher
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormMain());
        }
        public static void ChangeLanguage(ComponentResourceManager resources, CultureInfo cultureInfo, string lang, Control control)
        {
            resources.ApplyResources(control, control.Name, cultureInfo);
            foreach (Control subControl in control.Controls)
                ChangeLanguage(resources, cultureInfo, lang, subControl);
        }
    }
}

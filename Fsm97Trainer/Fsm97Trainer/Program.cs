using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Fsm97Trainer
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
        public static void CopyProperties<T>(T source, T destination)
        {
            PropertyInfo[] piList = typeof(T).GetProperties();
            foreach (PropertyInfo pi in piList)
            {               
               pi.SetValue(destination, pi.GetValue(source, null), null);
            }
        }
    }
}

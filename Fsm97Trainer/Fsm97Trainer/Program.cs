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
            FormMain mainForm = new FormMain();
            var settings=Properties.Settings.Default;
            settings.Reload();
            mainForm.AutoTrain = settings.AutoTrain;
            mainForm.ContractAutoRenew = settings.ContractAutoRenew;
            mainForm.AutoResetStatus = settings.AutoResetStatus;
            mainForm.ConvertToGK=settings.ConvertToGK;  
            mainForm.MaxEnergy = settings.MaxEnergy;
            mainForm.MaxForm=settings.MaxForm;
            mainForm.MaxMorale = settings.MaxMoral;
            mainForm.MaxPower = settings.MaxStrength;
            mainForm.NoAlternativeTraining = settings.NoAlternativeTraining;            
            Application.Run(mainForm);
            settings.AutoTrain = mainForm.AutoTrain;
            settings.ContractAutoRenew = mainForm.ContractAutoRenew;
            settings.AutoResetStatus = mainForm.AutoResetStatus;
            settings.ConvertToGK = mainForm.ConvertToGK;
            settings.MaxEnergy = mainForm.MaxEnergy;
            settings.MaxForm = mainForm.MaxForm;
            settings.MaxMoral = mainForm.MaxMorale;
            settings.MaxStrength = mainForm.MaxPower;
            settings.AutoResetStatus = mainForm.AutoResetStatus;
            settings.NoAlternativeTraining = mainForm.NoAlternativeTraining;
            settings.Save();
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

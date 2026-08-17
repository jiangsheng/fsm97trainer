using Fsm97Trainer.Models;
using Fsm97Trainer.Properties;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Net;
using System.Reflection;
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
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;

            var mainForm = new FormMain();
            mainForm.Model = LoadSettings();
            var mainForm2 = new FormMain2();
            mainForm2.Model = LoadSettings();
            Application.Run(mainForm2);
            SaveSettings(mainForm2.Model);
        }
        static FormMainModel LoadSettings()
        {
            var mainForm = new FormMainModel();
            var settings = Properties.Settings.Default;

            settings.Reload();
            mainForm.AutoTrain = settings.AutoTrain;
            mainForm.ContractAutoRenew = settings.ContractAutoRenew;
            mainForm.AutoResetStatus = settings.AutoResetStatus;
            mainForm.ConvertToGK = settings.ConvertToGK;
            mainForm.MaxEnergy = settings.MaxEnergy;
            mainForm.MaxForm = settings.MaxForm;
            mainForm.MaxMorale = settings.MaxMoral;
            mainForm.MaxPower = settings.MaxStrength;
            mainForm.NoAlternativeTraining = true;// settings.NoAlternativeTraining;
            mainForm.SavedFormation = settings.SavedFormation;
            if (mainForm.SavedFormation == null)
                mainForm.SavedFormation = new FSM97Lib.Formation();
            mainForm.AutoPositionWithFormation = settings.AutoPositionWithFormation;
            mainForm.CurrentLanguage= settings.CurrentLanguage;
            mainForm.RestoreBoundsLeft = settings.RestoreBounds.Left;   
            mainForm.RestoreBoundsTop = settings.RestoreBounds.Top;
            mainForm.RestoreBoundsRight = settings.RestoreBounds.Right;
            mainForm.RestoreBoundsBottom = settings.RestoreBounds.Bottom;
            mainForm.MaxEvalAge = settings.MaxEvalAge;
            mainForm.AlwaysTrainConsistency = settings.AlwaysTrainConsistency;
            return mainForm;
        }
        static void SaveSettings(FormMainModel mainForm)
        {
            var settings = Properties.Settings.Default;
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
            settings.SavedFormation = mainForm.SavedFormation;
            settings.AutoPositionWithFormation = mainForm.AutoPositionWithFormation;
            settings.CurrentLanguage= mainForm.CurrentLanguage;
            settings.RestoreBounds = new System.Drawing.Rectangle(
                mainForm.RestoreBoundsLeft,
                mainForm.RestoreBoundsTop,
                mainForm.RestoreBoundsRight - mainForm.RestoreBoundsLeft,
                mainForm.RestoreBoundsBottom - mainForm.RestoreBoundsTop);
            settings.MaxEvalAge = mainForm.MaxEvalAge;
            settings.AlwaysTrainConsistency = mainForm.AlwaysTrainConsistency;
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
        public static void ChangeLanguage(ComponentResourceManager resources, CultureInfo cultureInfo, string lang, Control control)
        {
            try
            {

                resources.ApplyResources(control, control.Name, cultureInfo);
                foreach (Control subControl in control.Controls)
                    ChangeLanguage(resources, cultureInfo, lang, subControl);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}

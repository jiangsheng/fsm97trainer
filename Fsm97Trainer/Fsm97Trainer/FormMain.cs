using CsvHelper;
using CsvHelper.Configuration;
using FSM97Lib;
using Fsm97Trainer.Models;
using OpenCCNET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Fsm97Trainer
{
    public partial class FormMain : Form
    {
        public FormMainModel Model{ get; set; }
        public FormMain()
        {
            InitializeComponent();

        }        /// <summary>
                 /// Clean up any resources being used.
                 /// </summary>
                 /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Model?.Dispose();
                if (components != null)
                    components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            bindingSourceModel.DataSource = Model;
            Model.OnError += (s, ev) =>
            {
                this.Invoke(new Action(() =>
                {
                    MessageBox.Show(ev.GetException().Message);
                    toolStripStatusLabel1.Text = ev.GetException().Message;
                }));
            }; 
            Model.OnWarning += (s, ev) =>
            {
                this.Invoke(new Action(() =>
                {                    
                    toolStripStatusLabel1.Text = ev.GetException().Message;
                }));
            };
            Model.OnModalMessage += (s, ev) =>
            {
                this.Invoke(new Action(() =>
                {
                    MessageBox.Show(Model.ModalMessage);
                }));
            };
            Model.PropertyChanged += (s, ev) =>
            {
                switch (ev.PropertyName) {
                    case nameof(Model.AutoTrain):
                        case nameof(Model.AutoResetStatus):
                    case nameof(Model.ContractAutoRenew):
                    case nameof(Model.MaxPower):
                    case nameof(Model.MaxMorale):
                    case nameof(Model.MaxForm):
                    case nameof(Model.MaxEnergy):
                        this.Invoke(new Action(() =>
                        {
                            UpdateTimers();
                        }));
                        break;
                }
            };
            Model.PropertyChanging += Model_PropertyChanging;
            try
            {
                ZhConverter.Initialize();
            }
            catch (Exception ex)
            {
                this.buttonUpdateNewSpawn.Enabled = false;
            }            
            Model.OnFastTimer();
            Model.OnSlowTimer();
            this.UpdateTimers();
            if (!string.IsNullOrEmpty(Model.CurrentLanguage))
            {
                Model.ChangeLanguage(Model.CurrentLanguage, this);
            }
        }

        private void buttonExportPlayerData_Click(object sender, EventArgs e)
        {
            Model.ExportPlayerData();
        }


        private void buttonCopyPlayerData_Click(object sender, EventArgs e)
        {
            Model.CopyPlayerData();
        }
        private void buttonPastePlayerData_Click(object sender, EventArgs e)
        {
            Model.PastePlayerData();            
        }

        private void buttonImportPlayerData_Click(object sender, EventArgs e)
        {
            Model.ImportPlayerData();           
        }

        private void buttonBoostYouthPlayer_Click(object sender, EventArgs e)
        {
            Model.BoostYouthPlayers();            
        }
        private void buttonRotateByEnergy_Click(object sender, EventArgs e)
        {
            Model.Rotate(RotateMethod.Energy);
        }
        private void buttonRotateByStatistics_Click(object sender, EventArgs e)
        {
            Model.Rotate(RotateMethod.Statistics);
        }


        private void buttonImproveAllPlayersBy1_Click(object sender, EventArgs e)
        {
            Model.ImproveAllPlayersBy1();
           
        }
        void UpdateTimers()
        {
            timerUpdateFast.Enabled = Model.AutoTrain || Model.AutoResetStatus || Model.MaxEnergy
                  || Model.MaxForm || Model.MaxMorale;

            timerUpdateSlow.Enabled = Model.ContractAutoRenew || Model.MaxPower;
        }

        private void Model_PropertyChanging(object sender, PropertyChangingCancelEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Model.ConvertToGK):
                    if ((bool)e.NewValue == true)
                    {
                        var result = MessageBox.Show(Properties.Resources.WarningConvertToGK, Properties.Resources.Warning, MessageBoxButtons.YesNo);
                        if (result == DialogResult.No)
                            e.Cancel = true;
                    }
                    break;
                default:
                    break;
            }
        }
        private void timerUpdateFast_Tick(object sender, EventArgs e)
        {
            Model.OnFastTimer();
        }

        private void timerUpdateSlow_Tick(object sender, EventArgs e)
        {
            Model.OnSlowTimer();
        }

        private void buttonResetDate_Click(object sender, EventArgs e)
        {
            string targetYearText = textBoxResetDateYear.Text;
            uint targetYear = 0;
            if (!uint.TryParse(targetYearText, out targetYear)
                || targetYear < 1900
                || targetYear > 2078)
            {
                MessageBox.Show(Properties.Resources.GameDateOutOfReangePrompt);
                return;
            }
            Model.ResetDate(targetYear);
        }

        private void buttonAutoPosition_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                        Properties.Resources.PleaseSwitchToTheTrainingSchedulePageFirstContinue, Properties.Resources.Warning, MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                Model.AutoPosition();
            }
        }

        private void buttonSaveCurrentFormation_Click(object sender, EventArgs e)
        {
            Model.SaveCurrentFormation();

        }

        private void buttonForceExit_Click(object sender, EventArgs e)
        {
            Model.ForceExit();
            
        }

        private void buttonUpdateNewSpawn_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(Properties.Resources.SeasonBeginningOnly, Properties.Resources.Warning, MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                Model.UpdateNewSpawn(comboBoxRespawnCategory.Text);
            }
        }

        private void buttonLandPurchase_Click(object sender, EventArgs e)
        {
            Model.LandPurchase();
        }

        private void comboBoxLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBoxLanguage.SelectedIndex)
            {
                case 0:
                    Model.ChangeLanguage("en-US",this);
                    break;
                case 1:
                    Model.ChangeLanguage("zh-CN", this);
                    break;
            }
        }

    }
}
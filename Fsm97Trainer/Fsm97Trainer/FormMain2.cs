using Fsm97Trainer.Models;
using OpenCCNET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Fsm97Trainer
{
    public partial class FormMain2 : Form
    {
        public FormMain2()
        {
            InitializeComponent();
        }
        public FormMainModel Model { get; set; }
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

        private void FormMain2_Load(object sender, EventArgs e)
        {
            if (Model.MaxEvalAge == 0)
                Model.MaxEvalAge = 19;
            bindingSourceMain.DataSource = Model;
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
                switch (ev.PropertyName)
                {
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
                this.buttonUpdatePlayerNameForNewSpawn.Enabled = false;
            }
            Model.OnFastTimer();
            Model.OnSlowTimer();
            this.UpdateTimers();
            switch (Model.CurrentLanguage)
            {
                case "en-US":
                    comboBoxLanguages.SelectedIndex= 0;
                    Model.ChangeLanguage("en-US", this);
                    break;
                case "zh-CN":
                    comboBoxLanguages.SelectedIndex = 1;
                    Model.ChangeLanguage("zh-CN", this);
                    break;
            }
            if (Model.RestoreBoundsLeft != Model.RestoreBoundsRight)
            {
                Location = new Point(Model.RestoreBoundsLeft, Model.RestoreBoundsTop);
                Size = new Size(
                    Model.RestoreBoundsRight - Model.RestoreBoundsLeft,
                    Model.RestoreBoundsBottom - Model.RestoreBoundsTop);
            }
            // Check if the form is visible on any screen
            if (!IsOnScreen(this.Bounds))
            {
                // Optionally resize to fit
                // this.Size = Screen.PrimaryScreen.WorkingArea.Size; 
                int screenWidth = Screen.PrimaryScreen.Bounds.Width;
                int screenHeight = Screen.PrimaryScreen.Bounds.Height;
                int formWidth = this.Width;
                int formHeight = this.Height;

                this.Location = new Point(
                    (screenWidth - formWidth) / 2,
                    (screenHeight - formHeight) / 2
                );
            }
        }
        bool IsOnScreen(Rectangle bounds)
        {
            foreach (var screen in Screen.AllScreens)
            {
                if (screen.WorkingArea.IntersectsWith(bounds))
                {
                    return true;
                }
            }
            return false;
        }
        void UpdateTimers()
        {
            timerFast.Enabled = Model.AutoTrain || Model.AutoResetStatus || Model.MaxEnergy
                || Model.MaxForm || Model.MaxMorale;

            timerSlow.Enabled = Model.ContractAutoRenew || Model.MaxPower;
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

        private void timerSlow_Tick(object sender, EventArgs e)
        {
            Model.OnSlowTimer();
        }

        private void timerFast_Tick(object sender, EventArgs e)
        {

            Model.OnFastTimer();
        }
        private void buttonSaveFormation_Click(object sender, EventArgs e)
        {
            Model.SaveCurrentFormation();
        }

        private void buttonChangeToBestPosition_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                       Properties.Resources.PleaseSwitchToTheTrainingSchedulePageFirstContinue, Properties.Resources.Warning, MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                Model.AutoPosition();
            }
        }

        private void buttonAurateByEnergy_Click(object sender, EventArgs e)
        {
            Model.Rotate(RotateMethod.Energy);

        }

        private void buttonRotateByAbility_Click(object sender, EventArgs e)
        {
            Model.Rotate(RotateMethod.Statistics);

        }

        private void buttonBoostYouthPlayers_Click(object sender, EventArgs e)
        {

            Model.BoostYouthPlayers();
        }

        private void buttonBoostAllPlayersBy1_Click(object sender, EventArgs e)
        {
            Model.ImproveAllPlayersBy1();
        }

        private void buttonExportPlayerData_Click(object sender, EventArgs e)
        {
            Model.ExportPlayerData();
        }

        private void buttonImport_Click(object sender, EventArgs e)
        {
            Model.ImportPlayerData();
        }

        private void buttonCopyPlayerData_Click(object sender, EventArgs e)
        {
            Model.CopyPlayerData();

        }

        private void buttonPastePlayerData_Click(object sender, EventArgs e)
        {
            Model.PastePlayerData();

        }

        private void buttonPurchaseLand_Click(object sender, EventArgs e)
        {
            Model.LandPurchase();
        }

        private void buttonTimeTravel_Click(object sender, EventArgs e)
        {
            string targetYearText = numericUpDownTimeTravel.Text;
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

        private void buttonRestartGame_Click(object sender, EventArgs e)
        {
            Model.Restart();
        }

        private void buttonUpdatePlayerNameForNewSpawn_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(Properties.Resources.SeasonBeginningOnly, Properties.Resources.Warning, MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                Model.UpdateNewSpawn(comboBoxRespawnCategory.Text);
            }
        }

        private void buttonChangeLanguage_Click(object sender, EventArgs e)
        {
            switch (comboBoxLanguages.SelectedIndex)
            {
                case 0:
                    Model.CurrentLanguage = "en-US";
                    Model.ChangeLanguage("en-US", this);
                    break;
                case 1:
                    Model.CurrentLanguage = "zh-CN";
                    Model.ChangeLanguage("zh-CN", this);
                    break;
            }
        }

        private void FormMain2_FormClosing(object sender, FormClosingEventArgs e)
        {
            Rectangle restoreBounds =
                WindowState == FormWindowState.Normal ?
                new Rectangle(this.Location, this.Size) :
                this.RestoreBounds;

            Model.RestoreBoundsLeft = restoreBounds.Left;
            Model.RestoreBoundsTop = restoreBounds.Top;
            Model.RestoreBoundsRight = restoreBounds.Right;
            Model.RestoreBoundsBottom = restoreBounds.Bottom;
        }

        private void timerEvelProgressReport_Tick(object sender, EventArgs e)
        {
            if (Model.EvalProgress >= Model.TotalPlayerPositionsToEval)
            { 
                Invoke(new Action(() =>
                {
                    this.timerEvelProgressReport.Stop();
                }));
                
            }
            else
            {
                Invoke(new Action(() =>
                {
                    if(Model.TotalPlayerPositionsToEval>0)
                        toolStripProgressBar1.Maximum=Model.TotalPlayerPositionsToEval;
                    if(Model.EvalProgress< Model.TotalPlayerPositionsToEval)
                        toolStripProgressBar1.Value = Model.EvalProgress;
                    else
                        toolStripProgressBar1.Value = Model.TotalPlayerPositionsToEval;
                    toolStripStatusLabel1.Text = string.Format(Properties.Resources.EvaluatingPlayerPositionsProgressReport,
                        Model.EvalProgress,
                        Model.TotalPlayerPositionsToEval);
                }));
                
            }
        }
        private void buttonEvalYoungPlayers_Click(object sender, EventArgs e)
        {
            this.timerEvelProgressReport.Start();
            
            if(backgroundWorkerEval.IsBusy)
            {
                MessageBox.Show(Properties.Resources.EvaluationInProgressPleaseWait);
                return;
            }
            textBoxEvalYoungPlayers.Text =string.Empty; 
            backgroundWorkerEval.RunWorkerAsync();
        }

        private void flowLayoutPanelData_Resize(object sender, EventArgs e)
        {
            textBoxEvalYoungPlayers.Width= flowLayoutPanelData.ClientSize.Width - textBoxEvalYoungPlayers.Margin.Left - textBoxEvalYoungPlayers.Margin.Right - 20;
        }

        private void backgroundWorkerEval_DoWork(object sender, DoWorkEventArgs e)
        {
            if(!string.IsNullOrEmpty(Model.CurrentLanguage))
                Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(Model.CurrentLanguage);
            Model.EvaluateYoungPlayers();
            Model.OnEvalProgressChanged+= (s, ev) =>
            {
                backgroundWorkerEval.ReportProgress(Model.EvalProgress);
            };
        }

        private void backgroundWorkerEval_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if(timerEvelProgressReport.Enabled==false)
                timerEvelProgressReport.Start();
        }

        private void backgroundWorkerEval_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            toolStripProgressBar1.Value = 0;
            toolStripStatusLabel1.Text = Properties.Resources.EvaluationCompleted;
            textBoxEvalYoungPlayers.Text = Model.EvalYoungPlayersResult;
        }
    }
}

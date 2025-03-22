using CsvHelper;
using CsvHelper.Configuration;
using FSM97Lib;
using OpenCCNET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Fsm97Trainer
{
    public partial class FormMain : Form
    {
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
                if (MenusProcess != null)
                    MenusProcess.Dispose();
                if (components != null)
                    components.Dispose();
            }
            base.Dispose(disposing);
        }

        MenusProcess MenusProcess { get; set; }
        public LinkedList<Player> PlayerData { get; set; }
        public bool AutoTrain { get; set; }
        public bool MaxEnergy { get; set; }
        public bool ConvertToGK { get; set; }
        public bool AutoResetStatus { get; set; }
        public bool MaxForm { get; set; }
        public bool MaxMorale { get; set; }
        public bool MaxPower { get; set; }
        public bool NoAlternativeTraining { get; set; }
        public bool ContractAutoRenew { get; set; }
        public bool RemoveNegativeTraining { get; internal set; }
        public bool TrainingEffectX2 { get; internal set; }
        public bool ThrowingTrainThrowIn { get; internal set; }
        public bool ShootingTrainGreed { get; internal set; }
        public bool PassingTrainLeadership { get; internal set; }
        public Formation SavedFormation { get; internal set; }
        public bool AutoPositionWithFormation { get; internal set; }

        private void FormMain_Load(object sender, EventArgs e)
        {
            checkBoxAutoTrain.Checked = this.AutoTrain;
            checkBoxConvertToGK.Checked = this.ConvertToGK;
            checkBoxNoAlternativeTraining.Checked = this.NoAlternativeTraining;

            checkBoxNoAbsense.Checked = this.AutoResetStatus;

            checkBoxMaxEnergy.Checked = this.MaxEnergy;
            checkBoxMaxForm.Checked = this.MaxForm;
            checkBoxMaxMorale.Checked = this.MaxMorale;

            checkBoxMaxStrength.Checked = this.MaxPower;

            checkBoxContractAutoRenew.Checked = this.ContractAutoRenew;
            checkBoxAutoPositionWithCurrentFormation.Checked = this.AutoPositionWithFormation;
            ZhConverter.Initialize();
            this.checkBoxFastTimer_CheckedChanged(sender, e);
            this.checkBoxSlowTimer_CheckedChanged(sender, e);
            timerUpdateSlow_Tick(sender, e);
        }
        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.AutoTrain = checkBoxAutoTrain.Checked;
            this.ConvertToGK = checkBoxConvertToGK.Checked;
            this.NoAlternativeTraining = checkBoxNoAlternativeTraining.Checked;

            this.AutoResetStatus = checkBoxNoAbsense.Checked;

            this.MaxEnergy = checkBoxMaxEnergy.Checked;
            this.MaxForm = checkBoxMaxForm.Checked;
            this.MaxMorale = checkBoxMaxMorale.Checked;

            this.MaxPower = checkBoxMaxStrength.Checked;

            this.ContractAutoRenew = checkBoxContractAutoRenew.Checked;
            this.AutoPositionWithFormation = checkBoxAutoPositionWithCurrentFormation.Checked;
        }
        private MenusProcess GetMenusProcess()
        {
            if (MenusProcess == null)
            {
                MenusProcess = new MenusProcess();
                return MenusProcess;
            }
            else
            {
                try
                {
                    if (!MenusProcess.HasExited())
                        return MenusProcess;
                    MenusProcess = new MenusProcess();
                    return MenusProcess;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    toolStripStatusLabel1.Text = ex.Message;
                    MenusProcess = new MenusProcess();
                    return MenusProcess;
                }
            }
        }

        private void buttonExportPlayerData_Click(object sender, EventArgs e)
        {
            try
            {
                MenusProcess menusProcess = GetMenusProcess();
                var players = menusProcess.ReadPlayers(false);
                if (players.Count > 0)
                {
                    if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        CsvConfiguration config = new CsvConfiguration();
                        config.Encoding = Encoding.UTF8;
                        config.CultureInfo = new CultureInfo("zh-hans");
                        using (var writer = new StreamWriter(saveFileDialog1.FileName, false, Encoding.UTF8))
                        using (var csv = new CsvWriter(writer, config))
                        {
                            csv.Configuration.RegisterClassMap<PlayerMap>();
                            csv.WriteHeader<Player>();
                            foreach (var playerNode in players)
                            {
                                csv.WriteRecord<Player>(playerNode.Data);
                            }
                        }
                        ProcessStartInfo psi = new ProcessStartInfo();
                        psi.UseShellExecute = true;
                        psi.FileName = saveFileDialog1.FileName;
                        Process.Start(psi);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                toolStripStatusLabel1.Text = ex.Message;
            }
        }


        private void buttonCopyPlayerData_Click(object sender, EventArgs e)
        {
            try
            {
                MenusProcess menusProcess = GetMenusProcess();
                this.PlayerData = new LinkedList<Player>();
                foreach (var playerNode in menusProcess.ReadPlayers(false))
                    this.PlayerData.AddLast(playerNode.Data);
                MessageBox.Show(Strings.PlayerDataCopied);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                toolStripStatusLabel1.Text = ex.Message;
            }
        }
        private void buttonPastePlayerData_Click(object sender, EventArgs e)
        {
            if (PlayerData == null)
            {
                MessageBox.Show(Strings.PleaseCopyPlayerDataFirst);
                return;
            }

            try
            {
                MenusProcess menusProcess = GetMenusProcess();
                menusProcess.LoadPlayerData(this.PlayerData);
                MessageBox.Show(Strings.PlayerDataPasted);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                toolStripStatusLabel1.Text = ex.Message;
            }
        }



        private void buttonImportPlayerData_Click(object sender, EventArgs e)
        {
            try
            {
                var players = this.LoadFromCsv();
                if (players.Count > 0)
                {
                    MenusProcess menusProcess = GetMenusProcess();
                    menusProcess.LoadPlayerData(players);
                    MessageBox.Show(Strings.PlayerDataImported);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                toolStripStatusLabel1.Text = ex.Message;
            }
        }

        private LinkedList<Player> LoadFromCsv()
        {
            var result = new LinkedList<Player>();
            var ofd = this.openFileDialog1;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                CsvConfiguration config = new CsvConfiguration();
                config.Encoding = Encoding.UTF8;
                config.CultureInfo = new CultureInfo("zh-hans");
                using (var reader = new StreamReader(ofd.FileName, Encoding.UTF8))
                using (var csv = new CsvReader(reader, config))
                {
                    csv.Configuration.RegisterClassMap<PlayerMap>();
                    csv.ReadHeader();
                    foreach (var record in csv.GetRecords<Player>())
                    {
                        result.AddLast(record);
                    }
                }
            }
            return result;
        }
        private void buttonBoostYouthPlayer_Click(object sender, EventArgs e)
        {
            try
            {
                var result = MessageBox.Show(Strings.SeasonBeginningOnly, Strings.Warning, MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    MenusProcess menusProcess = GetMenusProcess();
                    menusProcess.BoostYouthPlayer(false);
                    MessageBox.Show(Strings.YouthPlayerDataBoosted);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                toolStripStatusLabel1.Text = ex.Message;
            }
        }



        private void buttonRotateByEnergy_Click(object sender, EventArgs e)
        {
            Rotate(RotateMethod.Energy);
        }

        private void buttonRotateByStatistics_Click(object sender, EventArgs e)
        {
            Rotate(RotateMethod.Statistics);
        }


        private void Rotate(RotateMethod rotateMethod)
        {

            try
            {
                MenusProcess menusProcess = GetMenusProcess();
                if (!menusProcess.HasExited())
                {
                    var result = MessageBox.Show(Strings.PleaseSwitchToTheTrainingSchedulePageFirstContinue,
                        Strings.Warning, MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                    {
                        if (checkBoxAutoPositionWithCurrentFormation.Checked && this.SavedFormation.IsValid())
                        {
                            menusProcess.RotatePlayer(rotateMethod, this.SavedFormation);
                        }
                        else
                            menusProcess.RotatePlayer(rotateMethod, null);

                        MessageBox.Show(Strings.PlayersRotated);
                    }
                }
                else
                {
                    MessageBox.Show(Strings.CannotFindGameProcess);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                toolStripStatusLabel1.Text = ex.Message;
            }
        }


        private void buttonImproveAllPlayersBy1_Click(object sender, EventArgs e)
        {
            try
            {
                MenusProcess menusProcess = GetMenusProcess();
                menusProcess.ImproveAllPlayersBy1();
                MessageBox.Show(Strings.AllPlayerDataBoosted);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                toolStripStatusLabel1.Text = ex.Message;
            }
        }


        private void checkBoxFastTimer_CheckedChanged(object sender, EventArgs e)
        {
            timerUpdateFast.Enabled = checkBoxAutoTrain.Checked || checkBoxNoAbsense.Checked || checkBoxMaxEnergy.Checked
                || checkBoxMaxForm.Checked || checkBoxMaxMorale.Checked;
        }

        private void checkBoxConvertToGK_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxConvertToGK.Checked)
            {
                var result = MessageBox.Show(Strings.WarningConvertToGK, Strings.Warning, MessageBoxButtons.YesNo);
                if (result == DialogResult.No)
                    checkBoxConvertToGK.Checked = !checkBoxConvertToGK.Checked;
            }
        }
        bool inFastTimer = false;
        private void timerUpdateFast_Tick(object sender, EventArgs e)
        {
            if (inFastTimer) return;
            bool convertToGK = checkBoxConvertToGK.Checked;
            bool autoResetStatus = checkBoxNoAbsense.Checked;
            bool autoTrain = checkBoxAutoTrain.Checked;
            bool maxEnergy = checkBoxMaxEnergy.Checked;
            bool maxForm = checkBoxMaxForm.Checked;
            bool maxMorale = checkBoxMaxMorale.Checked;
            bool maxPower = checkBoxMaxStrength.Checked;
            bool noAlternativeTraining = checkBoxNoAlternativeTraining.Checked;

            try
            {
                MenusProcess menusProcess = GetMenusProcess();
                menusProcess.FastUpdate(autoTrain, convertToGK, autoResetStatus, maxEnergy, maxForm, maxMorale, maxPower, noAlternativeTraining
                    );
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                toolStripStatusLabel1.Text = ex.Message;
            }
            inFastTimer = false;
        }


        private void checkBoxSlowTimer_CheckedChanged(object sender, EventArgs e)
        {
            timerUpdateSlow.Enabled = this.checkBoxContractAutoRenew.Checked || checkBoxMaxStrength.Checked;
        }
        bool inSlowTimer = false;
        private void timerUpdateSlow_Tick(object sender, EventArgs e)
        {
            if (inSlowTimer) return;
            inSlowTimer = true;
            this.ContractAutoRenew = checkBoxContractAutoRenew.Checked;
            this.MaxPower = checkBoxMaxStrength.Checked;


            try
            {
                MenusProcess menusProcess = GetMenusProcess();
                menusProcess.SlowUpdate(ContractAutoRenew, MaxPower);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                toolStripStatusLabel1.Text = ex.Message;
            }
            inSlowTimer = false;
        }

        private void buttonResetDate_Click(object sender, EventArgs e)
        {
            try
            {
                string targetYearText = textBoxResetDateYear.Text;
                uint targetYear = 0;
                if (!uint.TryParse(targetYearText, out targetYear)
                    || targetYear < 1900
                    || targetYear > 2078)
                {
                    MessageBox.Show(Strings.GameDateOutOfReangePrompt);
                    return;
                }
                MenusProcess menusProcess = GetMenusProcess();
                if (!menusProcess.HasExited())
                {
                    var result = MessageBox.Show(Strings.GameDateChangeWarning,Strings.Warning, MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                    {
                        menusProcess.ResetDate(targetYear);
                        MessageBox.Show(Strings.GameDateReset);
                    }
                }
                else
                {
                    MessageBox.Show(Strings.CannotFindGameProcess);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                toolStripStatusLabel1.Text = ex.Message;
            }
        }

        private void buttonAutoPosition_Click(object sender, EventArgs e)
        {
            try
            {
                MenusProcess menusProcess = GetMenusProcess();
                if (!menusProcess.HasExited())
                {
                    var result = MessageBox.Show(
                        Strings.PleaseSwitchToTheTrainingSchedulePageFirstContinue,Strings.Warning, MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                    {
                        if (checkBoxAutoPositionWithCurrentFormation.Checked && this.SavedFormation.IsValid())
                        {
                            menusProcess.AutoPosition(this.SavedFormation);
                        }
                        else
                            menusProcess.AutoPosition(null);
                        MessageBox.Show(Strings.PositionAutoReset);
                    }
                }
                else
                {
                    MessageBox.Show(Strings.CannotFindGameProcess);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                toolStripStatusLabel1.Text = ex.Message;
            }
        }

        private void buttonSaveCurrentFormation_Click(object sender, EventArgs e)
        {
            try
            {
                MenusProcess menusProcess = GetMenusProcess();
                if (!menusProcess.HasExited())
                {
                    Formation newFormation = new Formation();
                    menusProcess.GetCurrentFormation(newFormation);
                    if (newFormation.IsValid())
                    {
                        this.SavedFormation = newFormation;
                        StringBuilder message = new StringBuilder();
                        message.Append(Strings.FormationSaved);
                        message.AppendFormat(":{0}", newFormation.GetFormationName());
                        MessageBox.Show(message.ToString());
                    }
                    else
                        MessageBox.Show(Strings.InvalidFormationForSaving);
                }
                else
                {
                    MessageBox.Show(Strings.CannotFindGameProcess);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                toolStripStatusLabel1.Text = ex.Message;
            }
        }

        private void buttonForceExit_Click(object sender, EventArgs e)
        {
            try
            {
                MenusProcess menusProcess = GetMenusProcess();
                if (!menusProcess.HasExited())
                {
                    menusProcess.Kill();
                }
                this.MenusProcess = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                toolStripStatusLabel1.Text = ex.Message;
            }
        }

        private void buttonUpdateNewSpawn_Click(object sender, EventArgs e)
        {
            try
            {
                var respawnCategory = comboBoxRespawnCategory.Text;
                MenusProcess menusProcess = GetMenusProcess();
                if (!menusProcess.HasExited())
                {
                    var result = MessageBox.Show(Strings.SeasonBeginningOnly,Strings.Warning, MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                    {
                        menusProcess.UpdatePlayerNames(respawnCategory);
                        MessageBox.Show(Strings.PlayerNamesUpdated);
                    }
                }
                else
                {
                    MessageBox.Show(Strings.CannotFindGameProcess);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                toolStripStatusLabel1.Text = ex.Message;
            }
        }

        private void comboBoxLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBoxLanguage.SelectedIndex)
            {
                case 0:
                    ChangeLanguage("en-US");
                    break;
                case 1:
                    ChangeLanguage("zh-CN");
                    break;
            }
        }
        void ChangeLanguage(string lang)
        {
            ComponentResourceManager resources = new ComponentResourceManager(this.GetType());
            Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(lang);
            Program.ChangeLanguage(resources, Thread.CurrentThread.CurrentUICulture, lang, this);
        }

    }
}
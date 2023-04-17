using CsvHelper;
using CsvHelper.Configuration;
using FSM97Lib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
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
        public List<Player> PlayerData { get; set; }
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
                    SaveToCsv(players);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                toolStripStatusLabel1.Text = ex.Message;
            }
        }

        private static void SaveToCsv(List<PlayerNode> players)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.FileName = "players.csv";
                sfd.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
                sfd.Title = "选择球员数据保存路径(Select player data export location)";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    CsvConfiguration config = new CsvConfiguration();
                    config.Encoding = Encoding.UTF8;
                    config.CultureInfo = new CultureInfo("zh-hans");
                    using (var writer = new StreamWriter(sfd.FileName, false, Encoding.UTF8))
                    using (var csv = new CsvWriter(writer, config))
                    {
                        csv.Configuration.RegisterClassMap<PlayerMap>();
                        csv.WriteRecords(players.Select(n => n.Data).ToList());
                    }
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.UseShellExecute = true;
                    psi.FileName = sfd.FileName;
                    Process.Start(psi);
                }
            }
        }


        private void buttonCopyPlayerData_Click(object sender, EventArgs e)
        {
            try
            {
                MenusProcess menusProcess = GetMenusProcess();
                this.PlayerData = menusProcess.ReadPlayers(false).Select(n => n.Data).ToList();
                MessageBox.Show("球员数据已复制(Player data copied)");
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
                MessageBox.Show("请先复制数据！(Please copy player data first)");
                return;
            }

            try
            {
                MenusProcess menusProcess = GetMenusProcess();
                menusProcess.LoadPlayerData(this.PlayerData);
                MessageBox.Show("球员数据已粘贴(Player data pasted)");
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
                    MessageBox.Show("球员数据已导入(Player data imported)");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                toolStripStatusLabel1.Text = ex.Message;
            }
        }

        private List<Player> LoadFromCsv()
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.FileName = "players.csv";
                ofd.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
                ofd.Title = "选择球员数据保存路径(Select player data export location)";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    CsvConfiguration config = new CsvConfiguration();
                    config.Encoding = Encoding.UTF8;
                    config.CultureInfo = new CultureInfo("zh-hans");
                    using (var reader = new StreamReader(ofd.FileName, Encoding.UTF8))
                    using (var csv = new CsvReader(reader, config))
                    {
                        csv.Configuration.RegisterClassMap<PlayerMap>();
                        return csv.GetRecords<Player>().ToList();
                    }
                }
                return new List<Player>();
            }
        }

        private void buttonBoostYouthPlayer_Click(object sender, EventArgs e)
        {
            try
            {
                MenusProcess menusProcess = GetMenusProcess();
                menusProcess.BoostYouthPlayer(false);
                MessageBox.Show("年轻球员数据已增益(Youth Player data boosted)");
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
                    var result = MessageBox.Show("请切换到训练安排页。继续？\r\n"
    + "Please switch to the training schedule page first. Continue?", "警告(warning)", MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                    {
                        if (checkBoxAutoPositionWithCurrentFormation.Checked && this.SavedFormation.IsValid())
                        {
                            menusProcess.RotatePlayer(rotateMethod, this.SavedFormation);
                        }
                        else
                            menusProcess.RotatePlayer(rotateMethod, null);

                        MessageBox.Show("球员已轮换(Players rotated)");
                    }
                }
                else
                {
                    MessageBox.Show("游戏进程找不到(Cannot find game process)");
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
                MessageBox.Show("所有球员数据已增益(All Player data boosted)");
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
                var result = MessageBox.Show("更换替补球员位置为守门员会减少受伤，但是球员会抱怨甚至威胁退役，经理也会定期要求提高球队水平但是不会被解雇。继续？\r\n"
                    + "Changing subs to GK will avoid training injories mostly but players will complain and sometimes threaten to retire, and managers will want you to increase performance (but you won't be fired). Continue?", "警告(warning)", MessageBoxButtons.YesNo);
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
                TrainingEffectModifier trainingEffectModifier = menusProcess.ReadTrainingEffectModifier();
                menusProcess.FastUpdate(autoTrain, convertToGK, autoResetStatus, maxEnergy, maxForm, maxMorale, maxPower, noAlternativeTraining
                    , trainingEffectModifier);
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
                TrainingEffectModifier trainingEffectModifier = menusProcess.ReadTrainingEffectModifier();
                menusProcess.SlowUpdate(ContractAutoRenew, MaxPower, trainingEffectModifier);
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
                MenusProcess menusProcess = GetMenusProcess();
                if (!menusProcess.HasExited())
                {
                    var result = MessageBox.Show("为了避免2079年球员年龄出错，请在2079年之前重设日期。另外，如果在赛季中重设，会造成赛程错误，应该在休赛期重设日期。继续？\r\n"
    + "To avoid age bug after 2079 you should reset date before 2079. In addition, resetting date would disrupt game scheduling, you should only do it in offseason. Continue?", "警告(warning)", MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                    {
                        menusProcess.ResetDate();
                        MessageBox.Show("游戏日期已重设 (Game Date Reset)");
                    }
                }
                else
                {
                    MessageBox.Show("游戏进程找不到(Cannot find game process)");
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
                    var result = MessageBox.Show("请切换到训练安排页。继续？\r\n"
    + "Please switch to the training schedule page first. Continue?", "警告(warning)", MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                    {
                        if (checkBoxAutoPositionWithCurrentFormation.Checked && this.SavedFormation.IsValid())
                        {
                            menusProcess.AutoPosition(this.SavedFormation);
                        }
                        else
                            menusProcess.AutoPosition(null);
                        MessageBox.Show("位置已重设 (Position Auto Reset)");
                    }
                }
                else
                {
                    MessageBox.Show("游戏进程找不到(Cannot find game process)");
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
                        message.AppendFormat("阵容已保存 (Formation Saved):{0}", newFormation.GetFormationName());
                        MessageBox.Show(message.ToString());
                    }
                    else
                        MessageBox.Show("保存阵容需要场上有11名球员，包括一名守门员 (Saving formation requires 11 players on the field, including a GK.)");
                }
                else
                {
                    MessageBox.Show("游戏进程找不到(Cannot find game process)");
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
    }
}
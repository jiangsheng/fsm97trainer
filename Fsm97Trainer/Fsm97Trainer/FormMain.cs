using CsvHelper;
using CsvHelper.Configuration;
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
        public List<Player> PlayerData { get; private set; }

        private void FormMain_Load(object sender, EventArgs e)
        {

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
                    int currentTeam = MenusProcess.ReadCurrentTeamIndex();
                    return MenusProcess;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
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
            }
        }

        private static void SaveToCsv(List<PlayerNode> players)
        {
            CultureInfo ci = CultureInfo.CurrentCulture;

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.FileName = "players.csv";
                sfd.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
                sfd.Title = "选择球员数据保存路径(Select player data export location)";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    CsvConfiguration config = new CsvConfiguration();
                    config.Encoding = Encoding.UTF8;
                    config.CultureInfo = CultureInfo.CurrentCulture;
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
            }
        }
        private void buttonPastePlayerData_Click(object sender, EventArgs e)
        {
            if (PlayerData == null)
            {
                MessageBox.Show("请先复制数据！(Please copy player data first)");
            }

            try
            {
                MenusProcess menusProcess = GetMenusProcess();
                menusProcess.LoadPlayerData(this.PlayerData);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            MessageBox.Show("球员数据已粘贴(Player data pasted)");
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
                    config.CultureInfo = CultureInfo.CurrentCulture;
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
                menusProcess.RotatePlayer(rotateMethod);
                MessageBox.Show("球员已轮换(Player rotated)");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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
            try
            {
                MenusProcess menusProcess = GetMenusProcess();
                menusProcess.FastUpdate(autoTrain, convertToGK, autoResetStatus, maxEnergy, maxForm, maxMorale);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            inFastTimer = false;
        }


        private void checkBoxSlowTimer_CheckedChanged(object sender, EventArgs e)
        {
            timerUpdateSlow.Enabled = this.checkBoxContractAutoRenew.Checked;

        }
        bool inSlowTimer = false;
        private void timerUpdateSlow_Tick(object sender, EventArgs e)
        {
            if (inSlowTimer) return;
            inSlowTimer = true;
            var autoRenewContracts = checkBoxContractAutoRenew.Checked;
            try
            {
                MenusProcess menusProcess = GetMenusProcess();
                menusProcess.SlowUpdate(autoRenewContracts);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            inSlowTimer = false;
        }
    }
}
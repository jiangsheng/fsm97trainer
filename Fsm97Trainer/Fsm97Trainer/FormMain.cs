using Binarysharp.MemoryManagement;
using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Fsm97Trainer
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
            
        }
        MenusProcess menusProcess = new MenusProcess();

        public List<Player> PlayerData { get; private set; }

        private void FormMain_Load(object sender, EventArgs e)
        {
           
        }

        private void toolStripButtonExportPlayerData_Click(object sender, EventArgs e)
        {
            try
            {
                var players=this.ReadPlayers();
                if(players.Count>0)
                    SaveToCsv(players);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw;
            }
        }

        private static void SaveToCsv(List<Player> players)
        {
            CultureInfo ci=CultureInfo.CurrentCulture;
          
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.FileName = "players.csv";
                sfd.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
                sfd.Title = "选择球员数据保存路径(Select player data export location)";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    CsvConfiguration config = new CsvConfiguration(ci);

                    using (var writer = new StreamWriter(sfd.FileName,false, Encoding.UTF8))
                    using (var csv = new CsvWriter(writer, config))
                    {
                        csv.WriteRecords(players);
                    }
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.UseShellExecute = true;
                    psi.FileName = sfd.FileName;
                    Process.Start(psi);
                }
            }
        }
        List<Player> ReadPlayers()
        {
            List<Player> players = new List<Player>();
            Process[] processes = Process.GetProcessesByName("MENUS");
            if (processes.Count() > 1)
            {
                MessageBox.Show("检测到多个游戏进程 (More than one menus process found)");
                return players;
            }
            else if (processes.Count() == 0)
            {
                MessageBox.Show("未检测到游戏进程 (Menuse process not found)");
                return players;
            }

            var gameProcess = processes.First();
            var gameExeFilePath = gameProcess.MainModule.FileName;
            Encoding encoding;
            FileInfo fi = new FileInfo(gameExeFilePath);
            switch (fi.Length)
            {
                case 1378816:
                    //menusProcess.SubCountAddress = 0x614610;
                    //menusProcess.DivisionFactorAddress = 0x4f3a60;
                    menusProcess.TeamDataAddress = 0x00547102;
                    menusProcess.DateAddress = 0x00562ED8;
                    encoding = Encoding.GetEncoding(936);
                    break;
                case 1135104://English Ver
                             //menusProcess.SubCountAddress = 0x5846e8;
                             //menusProcess.DivisionFactorAddress = 0x4f5178;
                    encoding = Encoding.GetEncoding(437);
                    menusProcess.DateAddress = 0x005A4ae8;
                    menusProcess.TeamDataAddress = 0x00588D12;
                    break;
                default:
                    MessageBox.Show("不支持的游戏版本 Unsupported Game version");
                    return players;
            }
            List<Team> teams = new List<Team>();
            
            MemorySharp memorySharp = new MemorySharp(gameProcess);
            for (int i = 0; i < 348; i++)
            {
                Team team = new Team();
                int teamDataAddress = menusProcess.TeamDataAddress + i * 0x140;
                team.Name = memorySharp.ReadString
                    (new IntPtr(teamDataAddress), encoding, false, 24);
                team.FanGroupName = memorySharp.ReadString
                    (new IntPtr(teamDataAddress + 0x19), false, 16);
                team.Abbreviation = memorySharp.ReadString
                    (new IntPtr(teamDataAddress + 0x2b), encoding, false, 3);
                team.ManagerFirstName = memorySharp.ReadString
                    (new IntPtr(teamDataAddress + 0x94), encoding, false, 11);
                team.ManagerLastName = memorySharp.ReadString
                    (new IntPtr(teamDataAddress + 0xaf), encoding, false, 11);
                int currentDate = memorySharp.Read<int>(new IntPtr(menusProcess.DateAddress), false);
                int teamPlayerAddress = memorySharp.Read<int>(new IntPtr(teamDataAddress + 0x136), false);
                if (teamPlayerAddress == 0) 
                    teamPlayerAddress = memorySharp.Read<int>(new IntPtr(teamDataAddress + 0x13a), false);
                if (teamPlayerAddress != 0)
                {
                    team.Players = ReadPlayers(memorySharp, teamPlayerAddress, team, encoding, currentDate);
                    teams.Add(team);
                    players.AddRange(team.Players);
                    Debug.WriteLine(String.Format("{0} has {1} players", team.Name, team.Players.Count));
                    foreach (var player in team.Players)
                    {
                        Debug.WriteLine(String.Format("{0}, {1},{2},{3},{4},{5} ", player.LastName,player.FirstName,
                            player.Speed,player.Agility,player.Acceleration,player.Stamina));
                    }
                }
                else
                {
                    Debug.WriteLine(String.Format("{0} has no players", team.Name));
                }
            }
            return players;
        }

        private List<Player> ReadPlayers(MemorySharp memorySharp, int nodeAddress, Team team,Encoding encoding, int currentDate)
        {
            //nodeAddress=029b8530
            List<Player> result = new List<Player>();            
            //playerDataAddress =029b84b0
            int nextNodeAddress = memorySharp.Read<int>(new IntPtr(nodeAddress + 4), false);           
            //nextNodeAddress =02982f0
            do
            {
                int playerDataAddress = memorySharp.Read<int>(new IntPtr(nodeAddress), false);
                Player player = ReadPlayer(memorySharp, playerDataAddress, team, encoding, currentDate);
                result.Add(player);
                nodeAddress = nextNodeAddress;
                nextNodeAddress = memorySharp.Read<int>(new IntPtr(nodeAddress + 4), false);
            } while (nextNodeAddress != 0);
            return result;
        }

        private Player ReadPlayer(MemorySharp memorySharp, int playerDataAddress, Team team, Encoding encoding, int currentDate)
        {            
            Player player = new Player();
            player.TeamName = team.Name;
            player.TeamAbbrivation = team.Abbreviation;
            player.FirstName= memorySharp.ReadString
                        (new IntPtr(playerDataAddress+4), encoding, false, 0x18);
            player.LastName = memorySharp.ReadString
                        (new IntPtr(playerDataAddress + 0x1c), encoding, false, 0x13);
            player.Nationality = memorySharp.Read<byte>(new IntPtr(playerDataAddress + 0x2f), false);
            player.NationalityName = GetNationalityName(player.Nationality);
            player.Position = memorySharp.Read<byte>(new IntPtr(playerDataAddress + 0x30), false);
            player.Status= memorySharp.Read<byte>(new IntPtr(playerDataAddress + 0x31), false);
            byte[] bytes = memorySharp.Read<byte>(new IntPtr(playerDataAddress + 0x32), 0x1c, false);
            player.Number = bytes[0];
            player.Speed = bytes[1]; 
            player.Agility = bytes[2];
            player.Acceleration = bytes[3];
            player.Stamina = bytes[4];
            player.Strength = bytes[5];
            player.Fitness = bytes[6];
            player.Shooting = bytes[7];
            player.Passing = bytes[8];
            player.Heading = bytes[9];
            player.Control = bytes[10];
            player.Dribbling = bytes[11];
            player.Coolness = bytes[12];
            player.Awareness = bytes[13];
            player.TackleDetermination = bytes[14];
            player.TackleSkill = bytes[15];
            player.Flair = bytes[16];
            player.Kicking = bytes[17];
            player.Throwing = bytes[18];
            player.Handling = bytes[19];
            player.ThrowIn = bytes[20];
            player.Leadership = bytes[21];
            player.Consistency = bytes[22];
            player.Determination = bytes[23];
            player.Greed = bytes[24];
            player.Form = bytes[25];
            player.Moral = bytes[26];
            player.Energy = bytes[27];
            //salary
            bytes = memorySharp.Read<byte>(new IntPtr(playerDataAddress + 0x60), 0x7, false);

            player.GamesThisSeason = memorySharp.Read<byte>(new IntPtr(playerDataAddress + 0x6e), false);
            player.Goals= memorySharp.Read<byte>(new IntPtr(playerDataAddress + 0x73), false);
            player.MVP= memorySharp.Read<byte>(new IntPtr(playerDataAddress + 0x74), false);
            player.ContractWeeks = memorySharp.Read<byte>(new IntPtr(playerDataAddress + 0x75), false);
            player.PositionName = GetPositionName(player.Position);
            player.PositionRating = GetPositionRating(player, player.Position);
            int bestPosition = 0;
            int bestPositionRating = 0;
            for (int i = 0; i < 16; i++)
            {
                int testPositionRating = GetPositionRating(player, i);
                if (bestPositionRating < testPositionRating)
                {
                    bestPosition = i;
                    bestPositionRating = testPositionRating;
                }
            }
            player.BestPosition = bestPosition;
            player.BestPositionName = GetPositionName(bestPosition);
            player.BestPositionRating = bestPositionRating;

            ushort birthDate = memorySharp.Read<ushort>(new IntPtr(playerDataAddress + 0x52), false);
            DateTime currentDateTime = new DateTime(1899, 12, 31).AddDays(currentDate);
            DateTime birthday= new DateTime(1899, 12, 31).AddDays(birthDate);
            int years = currentDateTime.Year - birthday.Year;
            // Go back to the year in which the person was born in case of a leap year
            if (birthday.Date > currentDateTime.AddYears(-years))
                years--;
            player.Age= years % 256;
            return player;
        }
        //see country,.txt for coutries
        private string GetNationalityName(int nationality)
        {
            switch (nationality)
            {
                case 0x1a:
                case 0x36:
                case 0x42: 
                case 0x53: return "ENG";
                case 0x1f: return "FRA";
                case 0x21: return "GER";
                case 0x28: return "ITA";
                case 0x49: return "SPA";
                default:return "OTH";
            }
        }

        private int GetPositionRating(Player player, int position)
        {
            int playerRating = 0;
            switch (position)
            {
                case 0:
                    playerRating = player.Speed * 2 + player.Agility * 25 +
                        player.Passing * 2 + player.Control * 4 + 
                        player.Coolness * 7 + player.Awareness * 10+ 
                        player.Kicking * 8 + player.Throwing * 6 + player.Handling * 30 + 
                        player.Consistency * 6;
                    break;
                case 1:
                case 2:
                    playerRating = player.Speed * 3 +
                        player.Passing * 7 + player.Heading * 8 +
                        player.TackleDetermination * 10 + player.TackleSkill * 44 +
                        player.Coolness * 7 + player.Awareness * 15 +
                        player.Consistency * 4 + player.Determination * 2;
                    break;
                case 3:
                    playerRating = player.Speed * 3 +
                        player.Passing * 3 + player.Heading * 14 +
                        player.TackleDetermination * 10 + player.TackleSkill * 50 +
                        player.Coolness * 7 + player.Awareness * 8 +
                        player.Consistency * 2 + player.Leadership * 3;
                    break;
                case 4:
                case 5:
                    playerRating = player.Speed * 7 + player.Agility * 4 + player.Acceleration * 11 +
                        player.Passing * 12 + player.Dribbling * 26 +
                        player.TackleDetermination * 3 + player.TackleSkill * 26 +
                        player.Flair * 5 + player.Awareness * 6;
                    break;
                case 6:
                    playerRating = player.Speed * 12 + player.Acceleration * 6 +
                        player.Passing * 15 + player.Heading * 3 + player.Dribbling * 15 +
                        player.TackleDetermination * 3 + player.TackleSkill * 26 +
                        player.Awareness * 20;
                    break;
                case 7:
                    playerRating = player.Speed * 5 +
                        player.Passing * 40 + player.Heading * 5 +
                        player.TackleDetermination * 3 + player.TackleSkill * 27 +
                        player.Awareness * 20;
                    break;
                case 8:
                case 9:
                    playerRating = player.Speed * 10 + player.Acceleration * 5 +
                        player.Shooting * 3 + player.Passing * 42 + player.Control * 5 + player.Dribbling * 5 +
                        player.TackleSkill * 20 +
                        player.Flair * 5 + player.Awareness * 5;
                    break;
                case 10:
                    playerRating = player.Speed * 10 + player.Acceleration * 5 +
                        player.Shooting * 5 + player.Passing * 46 + player.Control * 5 + player.Dribbling * 5 +
                        player.TackleSkill *14 +
                        player.Flair * 5 + player.Awareness * 5;
                    break;
                case 11:
                case 12:
                    playerRating = player.Speed * 10 + player.Agility * 3 + player.Acceleration * 10 +
                        player.Shooting * 3 + player.Passing * 31 + player.Control * 3 + player.Dribbling * 27 +
                        player.TackleSkill * 3 +
                         player.Awareness * 3 + player.Flair * 7;
                    break;
                case 13:
                    playerRating = player.Speed * 12 + player.Agility * 2 + player.Acceleration * 8 +
                        player.Shooting * 4 + player.Passing * 14 + player.Heading+player.Control * 10 + player.Dribbling * 27 +
                         player.Awareness * 12 + player.Flair * 10;
                    break;
                case 14:
                    playerRating = player.Speed * 10 + player.Agility * 2 + player.Acceleration * 9 +
                        player.Shooting * 36 + player.Passing * 4 + player.Heading * 10 + player.Control * 10 + player.Dribbling * 3 +
                        +player.Coolness*3+ player.Awareness * 4+ player.Flair * 9;
                    break;
                case 15:
                    playerRating = player.Speed * 6 + player.Agility * 2 + player.Acceleration * 6 +
                        player.Shooting * 29 + player.Passing * 16 + player.Heading * 7+ player.Control * 13 + player.Dribbling * 6 +
                        +player.Coolness * 2 + player.Awareness * 3 + player.Flair * 10;
                    break;
            }
            return playerRating / 100;
        }
        private string GetPositionName(int position)
        {
            switch (position)
            {
                case 0:return "GK";
                case 1: return "RB";
                case 2: return "LB";
                case 3: return "CD";
                case 4: return "RWB";
                case 5: return "LWB";
                case 6: return "SW";
                case 7: return "DM";
                case 8: return "RM";
                case 9: return "LM";
                case 10: return "AM";
                case 11: return "RW";
                case 12: return "LW";
                case 13: return "FR";
                case 15: return "SS";
                case 14: return "FOR";
                default:return String.Empty;
            }
        }

        private void toolStripButtonCopyPlayerData_Click(object sender, EventArgs e)
        {
            try
            {
                this.PlayerData = this.ReadPlayers();
                MessageBox.Show("球员数据已复制(Player data copied)");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw;
            }
            
        }

        private void toolStripLabelPastePlayerData_Click(object sender, EventArgs e)
        {
            Process[] processes = Process.GetProcessesByName("MENUS");
            if (processes.Count() > 1)
            {
                MessageBox.Show("检测到多个游戏进程 (More than one menus process found)");
                return;
            }
            else if (processes.Count() == 0)
            {
                MessageBox.Show("未检测到游戏进程 (Menuse process not found)");
                return;
            }
            var gameProcess = processes.First();
            try
            {
                if (PlayerData == null)
                {
                    MessageBox.Show("请先复制数据！(Please copy player data first)");
                }
                LoadPlayerData(gameProcess, this.PlayerData);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw;
            }
            
            MessageBox.Show("球员数据已粘贴(Player data pasted)");
        }

        private void LoadPlayerData(Process gameProcess, List<Player> playerData)
        {            
            var gameExeFilePath = gameProcess.MainModule.FileName;
            Encoding encoding;
            FileInfo fi = new FileInfo(gameExeFilePath);
            switch (fi.Length)
            {
                case 1378816:
                    //menusProcess.SubCountAddress = 0x614610;
                    //menusProcess.DivisionFactorAddress = 0x4f3a60;
                    menusProcess.TeamDataAddress = 0x00547102;
                    menusProcess.DateAddress = 0x00562ED8;
                    encoding = Encoding.GetEncoding(936);
                    break;
                case 1135104://English Ver
                             //menusProcess.SubCountAddress = 0x5846e8;
                             //menusProcess.DivisionFactorAddress = 0x4f5178;
                    encoding = Encoding.GetEncoding(437);
                    menusProcess.DateAddress = 0x005A4ae8;
                    menusProcess.TeamDataAddress = 0x00588D12;
                    break;
                default:
                    MessageBox.Show("不支持的游戏版本 Unsupported Game version");
                    return ;
            }
            var currentPlayerData = this.ReadPlayers();
            var fromPlayerIndex = new PlayerCollectionWithIndex(currentPlayerData);
            var toPlayerIndex = new PlayerCollectionWithIndex(playerData);
            var tasks = new WritePlayerTaskCollectionWithIndex(fromPlayerIndex, toPlayerIndex);
            List<Team> teams = new List<Team>();
            

            MemorySharp memorySharp = new MemorySharp(gameProcess);
            for (int i = 0; i < 348; i++)
            {
                Team team = new Team();
                int teamDataAddress = menusProcess.TeamDataAddress + i * 0x140;
                team.Name = memorySharp.ReadString
                    (new IntPtr(teamDataAddress), encoding, false, 24);
                team.FanGroupName = memorySharp.ReadString
                    (new IntPtr(teamDataAddress + 0x19), false, 16);
                team.Abbreviation = memorySharp.ReadString
                    (new IntPtr(teamDataAddress + 0x2b), encoding, false, 3);
                team.ManagerFirstName = memorySharp.ReadString
                    (new IntPtr(teamDataAddress + 0x94), encoding, false, 11);
                team.ManagerLastName = memorySharp.ReadString
                    (new IntPtr(teamDataAddress + 0xaf), encoding, false, 11);
                int currentDate = memorySharp.Read<int>(new IntPtr(menusProcess.DateAddress), false);
                int teamPlayerAddress = memorySharp.Read<int>(new IntPtr(teamDataAddress + 0x136), false);
                if(teamPlayerAddress==0) teamPlayerAddress= memorySharp.Read<int>(new IntPtr(teamDataAddress + 0x13a), false);
                if (teamPlayerAddress != 0)
                {
                    
                    WritePlayers(memorySharp, teamPlayerAddress, team, encoding, currentDate
                        , tasks);
                }
                else
                {
                    Debug.WriteLine(String.Format("{0} has no players", team.Name));
                }    
            }
        }


        private void WritePlayers(MemorySharp memorySharp, int nodeAddress, Team team, Encoding encoding, int currentDate,
            WritePlayerTaskCollectionWithIndex tasks)
        {

            //playerDataAddress =029b84b0
            int nextNodeAddress = memorySharp.Read<int>(new IntPtr(nodeAddress + 4), false);
            //nextNodeAddress =02982f0
            do
            {
                int playerDataAddress = memorySharp.Read<int>(new IntPtr(nodeAddress), false);
                WritePlayer(memorySharp, playerDataAddress, team, encoding, currentDate, tasks);
                nodeAddress = nextNodeAddress;
                nextNodeAddress = memorySharp.Read<int>(new IntPtr(nodeAddress + 4), false);
            } while (nextNodeAddress != 0);
        }

        private void WritePlayer(MemorySharp memorySharp, int playerDataAddress, Team team, Encoding encoding,
            int currentDate,
            WritePlayerTaskCollectionWithIndex tasks)
        {
            byte[] bytes = memorySharp.Read<byte>(new IntPtr(playerDataAddress + 0x32), 0x19, false);

            Player player = new Player
            {
                FirstName = memorySharp.ReadString
                        (new IntPtr(playerDataAddress + 4), encoding, false, 0x18),
                LastName = memorySharp.ReadString
                (new IntPtr(playerDataAddress + 0x1c), encoding, false, 0x13),
                ThrowIn = bytes[20],
                Leadership = bytes[21],
                Greed = bytes[24]
            };
            var taskList = tasks.LookupByName(player.LastName, player.FirstName);
            if (taskList == null) return;
            var task = taskList.Where(t => t.From.ThrowIn == player.ThrowIn && t.From.Leadership == player.Leadership
                && t.From.Greed == player.Greed
            && (t.WritePlayerTaskAction== WritePlayerTaskAction.None)).FirstOrDefault();
            if (task != null)
            {
                taskList.Remove(task);
                return;
            }
            task = taskList.Where(t => t.From.ThrowIn == player.ThrowIn && t.From.Leadership == player.Leadership
                && t.From.Greed == player.Greed
                && (t.WritePlayerTaskAction == WritePlayerTaskAction.Update)).FirstOrDefault();
            if (task != null)
            {
                bytes[1]= (byte)task.To.Speed;
                bytes[2] = (byte)task.To.Agility;
                bytes[3] = (byte)task.To.Acceleration;
                bytes[4] = (byte)task.To.Stamina;
                bytes[5] = (byte)task.To.Strength;
                bytes[6] = (byte)task.To.Fitness;
                bytes[7] = (byte)task.To.Shooting;
                bytes[8] = (byte)task.To.Passing;
                bytes[9] = (byte)task.To.Heading;
                bytes[10] = (byte)task.To.Control;
                bytes[11] = (byte)task.To.Dribbling;
                bytes[12] = (byte)task.To.Coolness;
                bytes[13] = (byte)task.To.Awareness;
                bytes[14] = (byte)task.To.TackleDetermination;
                bytes[15] = (byte)task.To.TackleSkill;
                bytes[16] = (byte)task.To.Flair;
                bytes[17] = (byte)task.To.Kicking;
                bytes[18] = (byte)task.To.Throwing;
                bytes[19] = (byte)task.To.Handling;
                bytes[20] = (byte)task.To.ThrowIn;
                bytes[21] = (byte)task.To.Leadership;
                bytes[22] = (byte)task.To.Consistency;
                bytes[23] = (byte)task.To.Determination;
                bytes[24] = (byte)task.To.Greed;
                memorySharp.Write<byte>(new IntPtr(playerDataAddress + 0x32), bytes, false);
                taskList.Remove(task);
                return;
            }
            task = taskList.Where(t=>t.WritePlayerTaskAction == WritePlayerTaskAction.Respawn).FirstOrDefault();
            if (task != null)
            {
                bytes[1] = (byte)task.To.Speed;
                bytes[2] = (byte)task.To.Agility;
                bytes[3] = (byte)task.To.Acceleration;
                bytes[4] = (byte)task.To.Stamina;
                bytes[5] = (byte)task.To.Strength;
                bytes[6] = (byte)task.To.Fitness;
                bytes[7] = (byte)task.To.Shooting;
                bytes[8] = (byte)task.To.Passing;
                bytes[9] = (byte)task.To.Heading;
                bytes[10] = (byte)task.To.Control;
                bytes[11] = (byte)task.To.Dribbling;
                bytes[12] = (byte)task.To.Coolness;
                bytes[13] = (byte)task.To.Awareness;
                bytes[14] = (byte)task.To.TackleDetermination;
                bytes[15] = (byte)task.To.TackleSkill;
                bytes[16] = (byte)task.To.Flair;
                bytes[17] = (byte)task.To.Kicking;
                bytes[18] = (byte)task.To.Throwing;
                bytes[19] = (byte)task.To.Handling;
                bytes[20] = (byte)task.To.ThrowIn;
                bytes[21] = (byte)task.To.Leadership;
                bytes[22] = (byte)task.To.Consistency;
                bytes[23] = (byte)task.To.Determination;
                bytes[24] = (byte)task.To.Greed;
                memorySharp.Write<byte>(new IntPtr(playerDataAddress + 0x32), bytes, false);
                taskList.Remove(task);
                return;
            }
        }

        private void toolStripButtonImport_Click(object sender, EventArgs e)
        {
            Process[] processes = Process.GetProcessesByName("MENUS");
            if (processes.Count() > 1)
            {
                MessageBox.Show("检测到多个游戏进程 (More than one menus process found)");
                return;
            }
            else if (processes.Count() == 0)
            {
                MessageBox.Show("未检测到游戏进程 (Menuse process not found)");
                return;
            }
            var gameProcess = processes.First();
            
            try
            {
                var players = this.LoadFromCsv();
                if (players.Count > 0)
                {
                    LoadPlayerData(gameProcess, players);
                    MessageBox.Show("球员数据已导入(Player data imported)");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw;
            }
        }

        private List<Player> LoadFromCsv()
        {
            CultureInfo ci = CultureInfo.CurrentCulture;

            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.FileName = "players.csv";
                ofd.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
                ofd.Title = "选择球员数据保存路径(Select player data export location)";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    CsvConfiguration config = new CsvConfiguration(ci);

                    using (var reader = new StreamReader(ofd.FileName, Encoding.UTF8))
                    using (var csv = new CsvReader(reader, config))
                    {
                        return csv.GetRecords<Player>().ToList();
                    }
                }
                return new List<Player>();
            }
        }
    }
}

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
        private void buttonExportPlayerData_Click(object sender, EventArgs e)
        {
            try
            {
                var players = this.ReadPlayers();
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
                    CsvConfiguration config = new CsvConfiguration(ci);

                    using (var writer = new StreamWriter(sfd.FileName, false, Encoding.UTF8))
                    using (var csv = new CsvWriter(writer, config))
                    {
                        csv.WriteRecords(players.Select(n=>n.Data).ToList());
                    }
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.UseShellExecute = true;
                    psi.FileName = sfd.FileName;
                    Process.Start(psi);
                }
            }
        }
        PlayerNodeList ReadPlayers()
        {
            PlayerNodeList playerNodes = new PlayerNodeList();
            Process[] processes = Process.GetProcessesByName("MENUS");
            if (processes.Count() > 1)
            {
                MessageBox.Show("检测到多个游戏进程 (More than one menus process found)");
                return playerNodes;
            }
            else if (processes.Count() == 0)
            {
                MessageBox.Show("未检测到游戏进程 (Menuse process not found)");
                return playerNodes;
            }

            var gameProcess = processes.First();
            var gameExeFilePath = gameProcess.MainModule.FileName;
            
            FileInfo fi = new FileInfo(gameExeFilePath);
            switch (fi.Length)
            {
                case 1378816:
                    //menusProcess.SubCountAddress = 0x614610;
                    //menusProcess.DivisionFactorAddress = 0x4f3a60;
                    menusProcess.TeamDataAddress = 0x00547102;
                    menusProcess.DateAddress = 0x00562ED8;
                    menusProcess.CurrentTeamIndexAddress = 0x562a4c;
                    playerNodes.Encoding = Encoding.GetEncoding(936);
                    playerNodes.GameProcess = gameProcess;
                    break;
                case 1135104://English Ver
                             //menusProcess.SubCountAddress = 0x5846e8;
                             //menusProcess.DivisionFactorAddress = 0x4f5178;
                    playerNodes.Encoding = Encoding.GetEncoding(437);
                    menusProcess.DateAddress = 0x005A4ae8;
                    menusProcess.TeamDataAddress = 0x00588D12;
                    menusProcess.CurrentTeamIndexAddress = 0x5a465c;
                    playerNodes.GameProcess = gameProcess;
                    break;
                default:
                    MessageBox.Show("不支持的游戏版本 Unsupported Game version");
                    return playerNodes;
            }
            List<Team> teams = new List<Team>();
            MemorySharp memorySharp = new MemorySharp(gameProcess);
            int currentDate = memorySharp.Read<int>(new IntPtr(menusProcess.DateAddress), false);
            for (int i = 0; i < 348; i++)
            {
                Team team = new Team();
                int teamDataAddress = menusProcess.TeamDataAddress + i * 0x140;
                team.Name = memorySharp.ReadString
                    (new IntPtr(teamDataAddress), playerNodes.Encoding, false, 24);
                team.FanGroupName = memorySharp.ReadString
                    (new IntPtr(teamDataAddress + 0x19), false, 16);
                team.Abbreviation = memorySharp.ReadString
                    (new IntPtr(teamDataAddress + 0x2b), playerNodes.Encoding, false, 3);
                team.ManagerFirstName = memorySharp.ReadString
                    (new IntPtr(teamDataAddress + 0x94), playerNodes.Encoding, false, 11);
                team.ManagerLastName = memorySharp.ReadString
                    (new IntPtr(teamDataAddress + 0xaf), playerNodes.Encoding, false, 11);

                int teamPlayerAddress = memorySharp.Read<int>(new IntPtr(teamDataAddress + 0x136), false);
                //if (teamPlayerAddress == 0)
                //    teamPlayerAddress = memorySharp.Read<int>(new IntPtr(teamDataAddress + 0x13a), false);
                if (teamPlayerAddress != 0)
                {
                    team.PlayerNodes = ReadPlayers(gameProcess,memorySharp, teamPlayerAddress, team, playerNodes.Encoding, currentDate);
                    teams.Add(team);
                    playerNodes.AddRange(team.PlayerNodes);
                    Debug.WriteLine(String.Format("{0} has {1} players", team.Name, team.PlayerNodes.Count));
                    foreach (var playerNode in team.PlayerNodes)
                    {
                        var player = playerNode.Data;
                        Debug.WriteLine(String.Format("{0}, {1},{2},{3},{4},{5} ", player.LastName, player.FirstName,
                            player.Speed, player.Agility, player.Acceleration, player.Stamina));
                    }
                }
                else
                {
                    Debug.WriteLine(String.Format("{0} has no players", team.Name));
                }
            }
            return playerNodes;
        }

        private PlayerNodeList ReadPlayers(Process gameProcess, MemorySharp memorySharp, int nodeAddress, Team team, Encoding encoding, int currentDate)
        {
            PlayerNodeList result = new PlayerNodeList();
            result.Encoding = encoding;
            result.GameProcess = gameProcess;
            if (nodeAddress == 0) return result;            
            int nextNodeAddress=memorySharp.Read<int>(new IntPtr(nodeAddress + 4), false);            
            //nextNodeAddress =02982f0
            do
            {
                var resultNode= new PlayerNode();
                resultNode.NodeAddress = nodeAddress;
                resultNode.DataAddress= memorySharp.Read<int>(new IntPtr(nodeAddress), false);
                resultNode.NextNode = nextNodeAddress;//always memorySharp.Read<int>(new IntPtr(nodeAddress + 4), false);
                resultNode.PreviousNode = memorySharp.Read<int>(new IntPtr(nodeAddress + 8), false);
                resultNode.Data=ReadPlayer(memorySharp, resultNode.DataAddress, team, encoding, currentDate);
                result.Add(resultNode);
                //move next
                nodeAddress = nextNodeAddress;
                if(nodeAddress!=0)
                    nextNodeAddress = memorySharp.Read<int>(new IntPtr(nodeAddress + 4), false);
            } while (nodeAddress != 0);
            return result;
        }

        private Player ReadPlayer(MemorySharp memorySharp, int playerDataAddress, Team team, Encoding encoding, int currentDate)
        {
            Player player = new Player();
            
            player.FirstName = memorySharp.ReadString
                        (new IntPtr(playerDataAddress + 4), encoding, false, 0x18);
            player.LastName = memorySharp.ReadString
                        (new IntPtr(playerDataAddress + 0x1c), encoding, false, 0x13);
            byte[] bytes = memorySharp.Read<byte>(new IntPtr(playerDataAddress), 0x76, false);
            player.Nationality = bytes[0x2f];
            player.Position = bytes[0x30];
            player.Status = bytes[0x31];
            player.Number = bytes[0x32];
            player.Speed = bytes[0x33];
            player.Agility = bytes[0x34];
            player.Acceleration = bytes[0x35];
            player.Stamina = bytes[0x36];
            player.Strength = bytes[0x37];
            player.Fitness = bytes[0x38];
            player.Shooting = bytes[0x39];
            player.Passing = bytes[0x3A];
            player.Heading = bytes[0x3B];
            player.Control = bytes[0x3C];
            player.Dribbling = bytes[0x3d];
            player.Coolness = bytes[0x3e];
            player.Awareness = bytes[0x3f];
            player.TackleDetermination = bytes[0x40];
            player.TackleSkill = bytes[0x41];
            player.Flair = bytes[0x42];
            player.Kicking = bytes[0x43];
            player.Throwing = bytes[0x44];
            player.Handling = bytes[0x45];
            player.ThrowIn = bytes[0x46];
            player.Leadership = bytes[0x47];
            player.Consistency = bytes[0x48];
            player.Determination = bytes[0x49];
            player.Greed = bytes[0x4a];
            player.Form = bytes[0x4b];
            player.Moral = bytes[0x4c];
            player.Energy = bytes[0x4d];
            //salary
            player.Salary = BitConverter.ToDouble(bytes, 0x60);
            player.GamesThisSeason = bytes[0x6e];
            player.Goals = bytes[0x73];
            player.MVP = bytes[0x74];
            player.ContractWeeks = bytes[0x75];
            player.Team = team;
            player.UpdateBestPosition();

            ushort birthDate = memorySharp.Read<ushort>(new IntPtr(playerDataAddress + 0x52), false);
            DateTime currentDateTime = new DateTime(1899, 12, 30).AddDays(currentDate);
            DateTime birthday = new DateTime(1899, 12, 30).AddDays(birthDate);
            int years = currentDateTime.Year - birthday.Year;
            // Go back to the year in which the person was born in case of a leap year
            if (birthday.Date > currentDateTime.AddYears(-years))
                years--;
            player.Age = years % 256;
            if (currentDate < 6570)
            {
                player.Age = player.Age + 78;
            }
            return player;
        }


        private void buttonCopyPlayerData_Click(object sender, EventArgs e)
        {
            try
            {
                this.PlayerData = this.ReadPlayers().Select(n=>n.Data).ToList();
                MessageBox.Show("球员数据已复制(Player data copied)");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw;
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
                LoadPlayerData(this.PlayerData);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw;
            }

            MessageBox.Show("球员数据已粘贴(Player data pasted)");
        }

        private void LoadPlayerData(List<Player> playerData)
        {
            var currentPlayerData = this.ReadPlayers();
            if (currentPlayerData.Count() == 0) return;
            var fromPlayerIndex = new PlayerCollectionWithIndex(currentPlayerData.Select(n=>n.Data).ToList());
            var toPlayerIndex = new PlayerCollectionWithIndex(playerData);
            var tasks = new WritePlayerTaskCollectionWithIndex(fromPlayerIndex, toPlayerIndex);
            List<Team> teams = new List<Team>();

            MemorySharp memorySharp = new MemorySharp(currentPlayerData.GameProcess);
            for (int i = 0; i < 348; i++)
            {
                Team team = new Team();
                int teamDataAddress = menusProcess.TeamDataAddress + i * 0x140;
                team.Name = memorySharp.ReadString
                    (new IntPtr(teamDataAddress), currentPlayerData.Encoding, false, 24);
                team.FanGroupName = memorySharp.ReadString
                    (new IntPtr(teamDataAddress + 0x19), false, 16);
                team.Abbreviation = memorySharp.ReadString
                    (new IntPtr(teamDataAddress + 0x2b), currentPlayerData.Encoding, false, 3);
                team.ManagerFirstName = memorySharp.ReadString
                    (new IntPtr(teamDataAddress + 0x94), currentPlayerData.Encoding, false, 11);
                team.ManagerLastName = memorySharp.ReadString
                    (new IntPtr(teamDataAddress + 0xaf), currentPlayerData.Encoding, false, 11);
                int currentDate = memorySharp.Read<int>(new IntPtr(menusProcess.DateAddress), false);
                int teamPlayerAddress = memorySharp.Read<int>(new IntPtr(teamDataAddress + 0x136), false);
                if (teamPlayerAddress == 0) teamPlayerAddress = memorySharp.Read<int>(new IntPtr(teamDataAddress + 0x13a), false);
                if (teamPlayerAddress != 0)
                {

                    WritePlayers(memorySharp, teamPlayerAddress, team, currentPlayerData.Encoding, currentDate
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
                WritePlayerWithTask(memorySharp, playerDataAddress, team, encoding, currentDate, tasks);
                nodeAddress = nextNodeAddress;
                if (nodeAddress != 0)
                    nextNodeAddress = memorySharp.Read<int>(new IntPtr(nodeAddress + 4), false);
            } while (nodeAddress != 0);
        }

        private void WritePlayerWithTask(MemorySharp memorySharp, int playerDataAddress, Team team, Encoding encoding,
            int currentDate,
            WritePlayerTaskCollectionWithIndex tasks)
        {
            byte[] bytes = memorySharp.Read<byte>(new IntPtr(playerDataAddress), 0x76, false);

            Player player = ReadPlayer(memorySharp, playerDataAddress, team, encoding, currentDate);
            var taskList = tasks.LookupByName(player.LastName, player.FirstName);
            if (taskList == null) return;
            var task = taskList.Where(t => t.From.ThrowIn == player.ThrowIn && t.From.Leadership == player.Leadership
                && t.From.Greed == player.Greed
            && (t.WritePlayerTaskAction == WritePlayerTaskAction.None)).FirstOrDefault();
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
                WritePlayerWithTask(memorySharp, playerDataAddress, player, task);
                taskList.Remove(task);
                return;
            }
            task = taskList.Where(t => t.WritePlayerTaskAction == WritePlayerTaskAction.Respawn).FirstOrDefault();
            if (task != null)
            {
                WritePlayerWithTask(memorySharp, playerDataAddress, player, task);
                taskList.Remove(task);
                return;
            }
        }

        private void WritePlayerWithTask(MemorySharp memorySharp, int playerDataAddress, Player player, WritePlayerTask task)
        {
            player.Speed = task.To.Speed;
            player.Agility = task.To.Agility;
            player.Acceleration = task.To.Acceleration;
            player.Stamina = task.To.Stamina;
            player.Strength = task.To.Strength;
            player.Fitness = task.To.Fitness;
            player.Shooting = task.To.Shooting;
            player.Passing = task.To.Passing;
            player.Heading = task.To.Heading;
            player.Control = task.To.Control;
            player.Dribbling = task.To.Dribbling;
            player.Coolness = task.To.Coolness;
            player.Awareness = task.To.Awareness;
            player.TackleDetermination = task.To.TackleDetermination;
            player.TackleSkill = task.To.TackleSkill;
            player.Flair = task.To.Flair;
            player.Kicking = task.To.Kicking;
            player.Throwing = task.To.Throwing;
            player.Handling = task.To.Handling;
            player.ThrowIn = task.To.ThrowIn;
            player.Leadership = task.To.Leadership;
            player.Consistency = task.To.Consistency;
            player.Determination = task.To.Determination;
            player.Greed = task.To.Greed;
            WritePlayer(memorySharp, playerDataAddress, player);
        }

        private void buttonImportPlayerData_Click(object sender, EventArgs e)
        {
            try
            {
                var players = this.LoadFromCsv();
                if (players.Count > 0)
                {
                    LoadPlayerData(players);
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

        private void buttonBoostYouthPlayer_Click(object sender, EventArgs e)
        {
            try
            {
                BoostYouthPlayer();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw;
            }

            MessageBox.Show("年轻球员数据已增益(Youth Player data boosted)");
        }

        private void BoostYouthPlayer()
        {
            var playerNodes = ReadPlayers();
            MemorySharp memorySharp = new MemorySharp(playerNodes.GameProcess);
            foreach (var playerNode in playerNodes)
            {
                var player = playerNode.Data;
                if (player.Age >= 20) continue;
                if (player.ContractWeeks > 144 || player.ContractWeeks <= 96) continue;
                player.Speed += 25; if (player.Speed > 99) player.Speed = 99;
                player.Agility += 25; if (player.Agility > 99) player.Agility = 99;
                player.Acceleration += 25; if (player.Acceleration > 99) player.Acceleration = 99;
                player.Stamina += 25; if (player.Stamina > 99) player.Stamina = 99;
                player.Strength += 25; if (player.Strength > 99) player.Strength = 99;
                player.Fitness += 25; if (player.Fitness > 99) player.Fitness = 99;
                player.Shooting += 25; if (player.Shooting > 99) player.Shooting = 99;
                player.Passing += 25; if (player.Passing > 99) player.Passing = 99;
                player.Heading += 25; if (player.Heading > 99) player.Heading = 99;
                player.Control += 25; if (player.Control > 99) player.Control = 99;
                player.Dribbling += 25; if (player.Dribbling > 99) player.Dribbling = 99;
                player.TackleDetermination += 25; if (player.TackleDetermination > 99) player.TackleDetermination = 99;
                player.TackleSkill += 25; if (player.TackleSkill > 99) player.TackleSkill = 99;
                player.Coolness += 25; if (player.Coolness > 99) player.Coolness = 99;
                player.Awareness += 25; if (player.Awareness > 99) player.Awareness = 99;
                player.Flair += 25; if (player.Flair > 99) player.Flair = 99;
                player.Kicking += 25; if (player.Kicking > 99) player.Kicking = 99;
                player.Throwing += 25; if (player.Throwing > 99) player.Throwing = 99;
                player.Handling += 25; if (player.Handling > 99) player.Handling = 99;
                player.ThrowIn += 25; if (player.ThrowIn > 99) player.ThrowIn = 99;
                player.Leadership += 25; if (player.Leadership > 99) player.Leadership = 99;
                player.Consistency += 25; if (player.Consistency > 99) player.Consistency = 99;
                player.Determination += 25; if (player.Determination > 99) player.Determination = 99;
                player.Greed += 25; if (player.Greed > 99) player.Greed = 99;
                player.UpdateBestPosition();
                player.Position = player.BestPosition;
                WritePlayer(memorySharp, playerNode.DataAddress, player);
            }
        }

        private void WritePlayer(MemorySharp memorySharp, int playerDataAddress, Player player)
        {
            byte[] bytes = memorySharp.Read<byte>(new IntPtr(playerDataAddress), 0x76, false);
            bytes[0x2f] = (byte)player.Nationality;
            bytes[0x30] = (byte)player.Position;
            bytes[0x30] = (byte)player.Position;
            bytes[0x31] = (byte)player.Status;
            bytes[0x32] = (byte)player.Number;
            bytes[0x33] = (byte)player.Speed;
            bytes[0x34] = (byte)player.Agility;
            bytes[0x35] = (byte)player.Acceleration;
            bytes[0x36] = (byte)player.Stamina;
            bytes[0x37] = (byte)player.Strength;
            bytes[0x38] = (byte)player.Fitness;
            bytes[0x39] = (byte)player.Shooting;
            bytes[0x3A] = (byte)player.Passing;
            bytes[0x3B] = (byte)player.Heading;
            bytes[0x3C] = (byte)player.Control;
            bytes[0x3d] = (byte)player.Dribbling;
            bytes[0x3e] = (byte)player.Coolness;
            bytes[0x3f] = (byte)player.Awareness;
            bytes[0x40] = (byte)player.TackleDetermination;
            bytes[0x41] = (byte)player.TackleSkill;
            bytes[0x42] = (byte)player.Flair;
            bytes[0x43] = (byte)player.Kicking;
            bytes[0x44] = (byte)player.Throwing;
            bytes[0x45] = (byte)player.Handling;
            bytes[0x46] = (byte)player.ThrowIn;
            bytes[0x47] = (byte)player.Leadership;
            bytes[0x48] = (byte)player.Consistency;
            bytes[0x49] = (byte)player.Determination;
            bytes[0x4a] = (byte)player.Greed;
            bytes[0x4b] = (byte)player.Form;
            bytes[0x4c] = (byte)player.Moral;
            bytes[0x4d] = (byte)player.Energy;
            bytes[0x6e] = (byte)player.GamesThisSeason;
            bytes[0x73] = (byte)player.Goals;
            bytes[0x74] = (byte)player.MVP;
            bytes[0x75] = (byte)player.ContractWeeks;
            var salaryBytes=BitConverter.GetBytes(player.Salary);
            salaryBytes.CopyTo(bytes, 0x60);
            memorySharp.Write<byte>(new IntPtr(playerDataAddress), bytes, false);
        }
        
        private void buttonRotateByEnergy_Click(object sender, EventArgs e)
        {
            Rotate(RotateMethod.Energy);
        }

        private void buttonRotateByStatistics_Click(object sender, EventArgs e)
        {
            Rotate(RotateMethod.Statistics);
        }
        enum RotateMethod { Energy, Statistics }


        private void Rotate(RotateMethod rotateMethod)
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
                RotatePlayer(gameProcess, rotateMethod);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw;
            }

            MessageBox.Show("球员已轮换(Player rotated)");
        }

        private void RotatePlayer(Process gameProcess, RotateMethod rotateMethod)
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
                    menusProcess.CurrentTeamIndexAddress = 0x562a4c;
                    encoding = Encoding.GetEncoding(936);
                    break;
                case 1135104://English Ver
                             //menusProcess.SubCountAddress = 0x5846e8;
                             //menusProcess.DivisionFactorAddress = 0x4f5178;
                    encoding = Encoding.GetEncoding(437);
                    menusProcess.DateAddress = 0x005A4ae8;
                    menusProcess.TeamDataAddress = 0x00588D12;
                    menusProcess.CurrentTeamIndexAddress = 0x5a465c;
                    break;
                default:
                    MessageBox.Show("不支持的游戏版本 Unsupported Game version");
                    return;
            }
            MemorySharp memorySharp = new MemorySharp(gameProcess);
            ushort currentTeam = memorySharp.Read<ushort>(new IntPtr(menusProcess.CurrentTeamIndexAddress), false);
            int currentDate = memorySharp.Read<int>(new IntPtr(menusProcess.DateAddress), false);
            Team team = new Team();
            int teamDataAddress = menusProcess.TeamDataAddress + currentTeam * 0x140;
            team.Name = memorySharp.ReadString
                (new IntPtr(teamDataAddress), encoding, false, 24);
            team.FanGroupName = memorySharp.ReadString
                (new IntPtr(teamDataAddress + 0x19), false, 16);
            team.Abbreviation = memorySharp.ReadString
                (new IntPtr(teamDataAddress + 0x2b), encoding, false, 3);

            int teamPlayerAddress = memorySharp.Read<int>(new IntPtr(teamDataAddress + 0x136), false);
            if (teamPlayerAddress == 0)
                teamPlayerAddress = memorySharp.Read<int>(new IntPtr(teamDataAddress + 0x13a), false);
            if (teamPlayerAddress != 0)
            {
                var players = ReadPlayers(gameProcess,memorySharp, teamPlayerAddress, team, encoding, currentDate);
                var gks = players.Where(p => p.Data.Position == 0);
                var others = players.Where(p => p.Data.Position != 0);
                switch (rotateMethod)
                {
                    case RotateMethod.Energy:
                        gks = gks.OrderByDescending(p => p.Data.Energy);
                        others = others.OrderByDescending(p => p.Data.Energy);
                        break;
                    case RotateMethod.Statistics:
                        gks = gks.OrderByDescending(p => p.Data.Statistics);
                        others = others.OrderByDescending(p => p.Data.Statistics);
                        break;
                }
                List<PlayerNode> normals = new List<PlayerNode>();
                List<PlayerNode> subs = new List<PlayerNode>();
                List<PlayerNode> rest = new List<PlayerNode>();
                var gkCount = gks.Count();
                if (gkCount > 1)
                {
                    normals.Add(gks.First());
                    normals.AddRange(others.Take(10));
                    subs.AddRange(gks.Skip(1).Take(1));
                    subs.AddRange(others.Skip(10).Take(4));
                    rest.AddRange(gks.Skip(2));
                    rest.AddRange(others.Skip(14));
                }
                else if (gkCount == 1)
                {
                    normals.Add(gks.First());
                    normals.AddRange(others.Take(10));
                    subs.AddRange(others.Skip(10).Take(5));
                    rest.AddRange(others.Skip(15));
                }
                else
                {
                    normals.AddRange(others.Take(11));
                    subs.AddRange(others.Skip(11).Take(5));
                    rest.AddRange(others.Skip(16));
                }
                var newPlayers = new LinkedList<PlayerNode>();
                foreach (var playerNode in normals)
                {
                    var player = playerNode.Data;
                    player.Status = 0;
                    player.UpdateBestPosition();
                    player.Position = player.BestPosition;
                    WritePlayer(memorySharp, playerNode.DataAddress, player);
                    newPlayers.AddLast(playerNode);
                }
                foreach (var playerNode in subs)
                {
                    var player = playerNode.Data;
                    player.Status = 1;
                    if (player.Fitness < 99)
                        player.Position = 0;
                    else
                        player.Position = player.BestPosition;
                    WritePlayer(memorySharp, playerNode.DataAddress, player);
                    newPlayers.AddLast(playerNode);
                }
                foreach (var playerNode in rest)
                {
                    var player = playerNode.Data;
                    player.Status = 2;
                    if (player.Fitness < 99)
                        player.Position = 0;
                    else
                        player.Position = player.BestPosition;
                    WritePlayer(memorySharp, playerNode.DataAddress, player);
                    newPlayers.AddLast(playerNode);
                }

                if (newPlayers.Count > 0)
                {
                    var currentNode = newPlayers.First;
                    memorySharp.Write<int>(new IntPtr(teamDataAddress + 0x136), currentNode.Value.NodeAddress, false);
                    while (currentNode != null)
                    {
                        if (currentNode.Previous == null)
                        {
                            currentNode.Value.PreviousNode = 0;
                        }
                        else
                            currentNode.Value.PreviousNode = currentNode.Previous.Value.NodeAddress;

                        if (currentNode.Next== null)
                        {
                            currentNode.Value.NextNode = 0;
                        }
                        else
                            currentNode.Value.NextNode = currentNode.Next.Value.NodeAddress;

                        memorySharp.Write<int>(new IntPtr(currentNode.Value.NodeAddress + 4), currentNode.Value.NextNode, false);
                        memorySharp.Write<int>(new IntPtr(currentNode.Value.NodeAddress + 8), currentNode.Value.PreviousNode, false);
                        currentNode = currentNode.Next;
                    }
                }
            }
            else
            {
                Debug.WriteLine(String.Format("{0} has no players", team.Name));
            }
        }

        private void buttonImproveAllPlayersBy1_Click(object sender, EventArgs e)
        {
            try
            {
                ImproveAllPlayersBy1();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw;
            }

            MessageBox.Show("所有球员数据已增益(All Player data boosted)");
        }

        private void ImproveAllPlayersBy1()
        {
            var playerNodes = ReadPlayers();
            MemorySharp memorySharp = new MemorySharp(playerNodes.GameProcess);
            int increment = 1;
            if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
                increment = 99;
            foreach (var playerNode in playerNodes)
            {
                var player = playerNode.Data;
                player.Speed += increment; if (player.Speed > 99) player.Speed = 99;
                player.Agility += increment; if (player.Agility > 99) player.Agility = 99;
                player.Acceleration += increment; if (player.Acceleration > 99) player.Acceleration = 99;
                player.Stamina += increment; if (player.Stamina > 99) player.Stamina = 99;
                player.Strength += increment; if (player.Strength > 99) player.Strength = 99;
                player.Fitness += increment; if (player.Fitness > 99) player.Fitness = 99;
                player.Shooting += increment; if (player.Shooting > 99) player.Shooting = 99;
                player.Passing += increment; if (player.Passing > 99) player.Passing = 99;
                player.Heading += increment; if (player.Heading > 99) player.Heading = 99;
                player.Control += increment; if (player.Control > 99) player.Control = 99;
                player.Dribbling += increment; if (player.Dribbling > 99) player.Dribbling = 99;
                player.TackleDetermination += increment; if (player.TackleDetermination > 99) player.TackleDetermination = 99;
                player.TackleSkill += increment; if (player.TackleSkill > 99) player.TackleSkill = 99;
                player.Coolness += increment; if (player.Coolness > 99) player.Coolness = 99;
                player.Awareness += increment; if (player.Awareness > 99) player.Awareness = 99;
                player.Flair += increment; if (player.Flair > 99) player.Flair = 99;
                player.Kicking += increment; if (player.Kicking > 99) player.Kicking = 99;
                player.Throwing += increment; if (player.Throwing > 99) player.Throwing = 99;
                player.Handling += increment; if (player.Handling > 99) player.Handling = 99;
                player.ThrowIn += increment; if (player.ThrowIn > 99) player.ThrowIn = 99;
                player.Leadership += increment; if (player.Leadership > 99) player.Leadership = 99;
                player.Consistency += increment; if (player.Consistency > 99) player.Consistency = 99;
                player.Determination += increment; if (player.Determination > 99) player.Determination = 99;
                player.Greed += increment; if (player.Greed > 99) player.Greed = 99;
                player.UpdateBestPosition();
                player.Position = player.BestPosition;
                WritePlayer(memorySharp, playerNode.DataAddress, player);
            }
        }
    }
}
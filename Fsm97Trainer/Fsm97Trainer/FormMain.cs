using Binarysharp.MemoryManagement;
using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
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

        private void FormMain_Load(object sender, EventArgs e)
        {
           
        }

        private void toolStripButtonExportPlayerData_Click(object sender, EventArgs e)
        {
            try
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
                        return;
                }
                List<Team> teams = new List<Team>();
                List<Player> players = new List<Player>();
                MemorySharp memorySharp = new MemorySharp(gameProcess);
                for (int i = 0; i < 348; i++)
                {
                    Team team = new Team();
                    int teamDataAddress = menusProcess.TeamDataAddress + i * 0x140;
                    team.Name = memorySharp.ReadString
                        (new IntPtr(teamDataAddress), encoding, false, 16);
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
                    if (teamPlayerAddress != 0)
                    {
                        team.Players = ReadPlayers(memorySharp, teamPlayerAddress, team, encoding, currentDate);
                        teams.Add(team);
                        players.AddRange(team.Players);
                    }
                    else
                    {
                        
                    }
                }
                SaveToCsv(players);
            }
            catch (Exception ex)
            {
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
            player.Position = memorySharp.Read<byte>(new IntPtr(playerDataAddress + 0x30), false);
            player.Number= memorySharp.Read<byte>(new IntPtr(playerDataAddress + 0x32), false);
            player.Speed = memorySharp.Read<byte>(new IntPtr(playerDataAddress + 0x33), false);
            player.Agility = memorySharp.Read<byte>(new IntPtr(playerDataAddress + 0x34), false);
            player.Acceleration = memorySharp.Read<byte>(new IntPtr(playerDataAddress + 0x35), false);
            player.Stamina = memorySharp.Read<byte>(new IntPtr(playerDataAddress + 0x36), false);
            player.Strength = memorySharp.Read<byte>(new IntPtr(playerDataAddress + 0x37), false);
            player.Fitness = memorySharp.Read<byte>(new IntPtr(playerDataAddress + 0x38), false);
            player.Shooting = memorySharp.Read<byte>(new IntPtr(playerDataAddress + 0x39), false);
            player.Passing = memorySharp.Read<byte>(new IntPtr(playerDataAddress + 0x3a), false);
            player.Heading = memorySharp.Read<byte>(new IntPtr(playerDataAddress + 0x3b), false);
            player.Control = memorySharp.Read<byte>(new IntPtr(playerDataAddress + 0x3c), false);
            player.Dribbling = memorySharp.Read<byte>(new IntPtr(playerDataAddress + 0x3d), false);
            player.Coolness = memorySharp.Read<byte>(new IntPtr(playerDataAddress + 0x3e), false);
            player.Awareness = memorySharp.Read<byte>(new IntPtr(playerDataAddress + 0x3f), false);
            player.TackleDetermination = memorySharp.Read<byte>(new IntPtr(playerDataAddress + 0x40), false);
            player.TackleSkill = memorySharp.Read<byte>(new IntPtr(playerDataAddress + 0x41), false);
            player.Flair = memorySharp.Read<byte>(new IntPtr(playerDataAddress + 0x42), false);
            player.Kicking = memorySharp.Read<byte>(new IntPtr(playerDataAddress + 0x43), false);
            player.Throwing = memorySharp.Read<byte>(new IntPtr(playerDataAddress + 0x44), false);
            player.Handling = memorySharp.Read<byte>(new IntPtr(playerDataAddress + 0x45), false);
            player.ThrowIn = memorySharp.Read<byte>(new IntPtr(playerDataAddress + 0x46), false);
            player.Leadship = memorySharp.Read<byte>(new IntPtr(playerDataAddress + 0x47), false);
            player.Consistency = memorySharp.Read<byte>(new IntPtr(playerDataAddress + 0x48), false);

            player.Determination = memorySharp.Read<byte>(new IntPtr(playerDataAddress + 0x49), false);
            player.Greed = memorySharp.Read<byte>(new IntPtr(playerDataAddress + 0x4a), false);
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
            player.Nationality= memorySharp.Read<byte>(new IntPtr(playerDataAddress + 0x1f), false);
            player.NationalityName = GetNationalityName(player.Nationality);
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

        private string GetNationalityName(int nationality)
        {
            switch (nationality)
            {
                case 0x1a: return "ENG";
                case 0x1f: return "FRA";
                case 0x21: return "GER";
                case 0x1c: return "ITA";
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
                        player.Consistency * 2 + player.Leadship * 3;
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
    }
}

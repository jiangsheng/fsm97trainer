using FSM97Lib;
using NameParser;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenCCNET;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;
using Diacritics.Extensions;
using System.CodeDom;
using System.Windows.Forms.VisualStyles;

namespace Fsm97Trainer
{
    public class MenusProcess : IDisposable
    {
        Random random = new Random();
        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    if (Process != null)
                        Process.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~MenusProcess()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        //public int SubCountAddress { get; set; }
        //public int DivisionFactorAddress { get; set; }
        public int TeamDataAddress { get; set; }
        public int DateAddress { get; set; }
        public int CurrentTeamIndexAddress { get; internal set; }
        public int TrainingDataAddress { get; internal set; }
        public int TrainingEffectAddress { get; internal set; }
        public Encoding Encoding { get; private set; }
        Process Process { get; set; }



        public MenusProcess()
        {
            Process[] processes = Process.GetProcessesByName("MENUS");
            if (processes.Count() > 1)
            {
                for (int i = 0; i < processes.Length; i++)
                {
                    // User must also dispose of any matched Processes that are returned
                    processes[i].Dispose();
                }
                throw new InvalidOperationException("检测到多个游戏进程 (More than one menus process found)");
            }
            else if (processes.Count() == 0)
            {
                throw new InvalidOperationException("未检测到游戏进程 (Menuse process not found)");
            }
            Process = processes.First();
            var gameExeFilePath = Process.MainModule.FileName;
            FileInfo fi = new FileInfo(gameExeFilePath);
            switch (fi.Length)
            {
                case 1378816:
                    //menusProcess.SubCountAddress = 0x614610;
                    //menusProcess.DivisionFactorAddress = 0x4f3a60;
                    TeamDataAddress = 0x00547102;
                    DateAddress = 0x00562ED8;
                    CurrentTeamIndexAddress = 0x562a4c;
                    TrainingDataAddress = 0x562f50;
                    TrainingEffectAddress= 0x004e38a0;
                    Encoding = Encoding.GetEncoding(936);
                    break;
                case 1135104://English Ver
                             //menusProcess.SubCountAddress = 0x5846e8;
                             //menusProcess.DivisionFactorAddress = 0x4f5178;
                    Encoding = Encoding.GetEncoding(437);
                    DateAddress = 0x005A4ae8;
                    TeamDataAddress = 0x00588D12;
                    CurrentTeamIndexAddress = 0x5a465c;
                    TrainingDataAddress = 0x5a4b60;
                    break;
                default:
                    Process.Dispose();
                    throw new InvalidOperationException("不支持的游戏版本 Unsupported Game version");
            }
        }

        internal int ReadCurrentTeamIndex()
        {
            return NativeMethods.ReadByte(Process,CurrentTeamIndexAddress);
        }

        private void WritePlayer(PlayerNode playerNode)
        {
            int playerDataAddress = playerNode.DataAddress;
            Player player = playerNode.Data;
            byte[] bytes = NativeMethods.ReadBytes(Process,playerDataAddress, 0x76);
            bytes[0x2f] = (byte)player.Nationality;
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
            var salaryBytes = BitConverter.GetBytes(player.Salary);
            salaryBytes.CopyTo(bytes, 0x60);
            NativeMethods.WriteBytes(Process,playerDataAddress + 0x2f, bytes, 0x2f, 0x31 - 0x2f + 1);
            NativeMethods.WriteBytes(Process,playerDataAddress + 0x33, bytes, 0x33, 0x4d - 0x33 + 1);
            NativeMethods.WriteByte(Process,playerDataAddress + 0x75, (byte)player.ContractWeeks);
        }
        private void WritePlayerContractWeeks(PlayerNode playerNode)
        {
            NativeMethods.WriteByte(Process, playerNode.DataAddress + 0x75, (byte)playerNode.Data.ContractWeeks);
        }


        private void WritePlayerPosition(PlayerNode playerNode)
        {
            NativeMethods.WriteByte(Process, playerNode.DataAddress + 0x30, (byte)playerNode.Data.Position);
        }
        byte[] formMoralEnergyBuffer = new byte[3];
        private void WritePlayerFormMoralEnergy(PlayerNode playerNode)
        {
            formMoralEnergyBuffer[0] = (byte)playerNode.Data.Form;
            formMoralEnergyBuffer[1] = (byte)playerNode.Data.Moral;
            formMoralEnergyBuffer[2] = (byte)playerNode.Data.Energy;
            NativeMethods.WriteBytes(Process, playerNode.DataAddress + 0x4b, formMoralEnergyBuffer,0,3);
        }
        private void WritePlayerStrengths(PlayerNode playerNode)
        {
            formMoralEnergyBuffer[0] = (byte)playerNode.Data.Stamina;
            formMoralEnergyBuffer[1] = (byte)playerNode.Data.Strength;
            formMoralEnergyBuffer[2] = (byte)playerNode.Data.Fitness;
            NativeMethods.WriteBytes(Process, playerNode.DataAddress + 0x36, formMoralEnergyBuffer, 0, 3);
        }
        private void WritePlayerStatus(PlayerNode playerNode)
        {
            NativeMethods.WriteByte(Process, playerNode.DataAddress + 0x31, (byte)playerNode.Data.Status);
        }
        private void WritePlayerBirthDate(PlayerNode playerNode)
        {
            NativeMethods.WriteUShort(Process, playerNode.DataAddress + 0x52, playerNode.Data.BirthDateOffset);
        }
        private Player ReadPlayer(int playerDataAddress, Team team, Encoding encoding, int currentDate)
        {
            Player player = new Player();

            player.FirstName = NativeMethods.ReadString
                        (Process,playerDataAddress + 4, Encoding, 0x18);
            player.LastName = NativeMethods.ReadString(Process, playerDataAddress + 0x1c, Encoding, 0x13);
            byte[] bytes = NativeMethods.ReadBytes(Process, playerDataAddress, 0x76);
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
            player.PositionRating = player.GetPositionRating(player.Position);
            player.UpdateBestPosition();

            player.BirthDateOffset= NativeMethods.ReadUShort(Process,playerDataAddress + 0x52);
            DateTime currentDateTime = new DateTime(1899, 12, 30).AddDays(currentDate);
            DateTime birthday = new DateTime(1899, 12, 30).AddDays(player.BirthDateOffset);
            int years = currentDateTime.Year - birthday.Year;
            // Go back to the year in which the person was born in case of a leap year
            if (birthday.Date >= currentDateTime.AddYears(-years))
                years--;
            player.Age = years % 256;
            if (currentDate < 6570)
            {
                player.Age = player.Age + 78;
            }
            return player;
        }


        public PlayerNodeList ReadPlayers(bool currentTeamOnly)
        {
            PlayerNodeList playerNodes = new PlayerNodeList();
            List<Team> teams = new List<Team>();

            ushort currentTeam = NativeMethods.ReadByte(Process,CurrentTeamIndexAddress);
            int currentDate = NativeMethods.ReadInt(Process,DateAddress);
            if (currentTeamOnly)
            {
                Team team = new Team();
                int teamDataAddress = TeamDataAddress + currentTeam * 0x140;
                team.Name = NativeMethods.ReadString(Process, teamDataAddress, Encoding, 24);
                team.FanGroupName = NativeMethods.ReadString
                    (Process, teamDataAddress + 0x19, Encoding, 16);
                team.Abbreviation = NativeMethods.ReadString(Process, teamDataAddress + 0x2b, Encoding, 3);

                int teamPlayerAddress = NativeMethods.ReadInt(Process, teamDataAddress + 0x136);
                if (teamPlayerAddress == 0)
                    teamPlayerAddress = NativeMethods.ReadInt(Process, teamDataAddress + 0x13a);
                if (teamPlayerAddress != 0)
                {
                    team.PlayerNodes = ReadPlayers(teamPlayerAddress, team, Encoding, currentDate);
                    foreach (var playerNode in team.PlayerNodes)
                    {
                        playerNodes.AddLast(playerNode);
                    }
                    
                    //Debug.WriteLine(String.Format("{0} has {1} players", team.Name, team.PlayerNodes.Count));
                }
            }
            else
            {
                for (int i = 0; i < 348; i++)
                {
                    Team team = new Team();
                    int teamDataAddress = TeamDataAddress + i * 0x140;
                    team.Name = NativeMethods.ReadString(Process, teamDataAddress, Encoding, 24);
                    team.FanGroupName = NativeMethods.ReadString(Process, teamDataAddress + 0x19, Encoding, 16);
                    team.Abbreviation = NativeMethods.ReadString(Process, teamDataAddress + 0x2b, Encoding, 3);
                    team.ManagerFirstName = NativeMethods.ReadString(Process, teamDataAddress + 0x94, Encoding, 11);
                    team.ManagerLastName = NativeMethods.ReadString(Process, teamDataAddress + 0x9f, Encoding, 11);

                    int teamPlayerAddress = NativeMethods.ReadInt(Process, teamDataAddress + 0x136);
                    if (teamPlayerAddress != 0)
                    {
                        team.PlayerNodes = ReadPlayers(teamPlayerAddress, team, Encoding, currentDate);
                        teams.Add(team);
                        foreach (var playerNode in team.PlayerNodes)
                        {
                            playerNodes.AddLast(playerNode);
                        }
                        //Debug.WriteLine(String.Format("{0} has {1} players", team.Name, team.PlayerNodes.Count));
                        /*foreach (var playerNode in team.PlayerNodes)
                        {
                            var player = playerNode.Data;
                            Debug.WriteLine(String.Format("{0}, {1},{2},{3},{4},{5} ", player.LastName, player.FirstName,
                                player.Speed, player.Agility, player.Acceleration, player.Stamina));
                        }*/
                    }
                    else
                    {
                        Debug.WriteLine(String.Format("{0} ({1}) has no players", team.Name,team.Abbreviation));
                    }
                }
            }
            return playerNodes;
        }


        private PlayerNodeList ReadPlayers(int nodeAddress, Team team, Encoding encoding, int currentDate)
        {
            PlayerNodeList result = new PlayerNodeList();
            if (nodeAddress == 0) return result;
            int nextNodeAddress = NativeMethods.ReadInt(Process, nodeAddress + 4);
            //nextNodeAddress =02982f0
            do
            {
                var resultNode = new PlayerNode();
                resultNode.NodeAddress = nodeAddress;
                resultNode.DataAddress = NativeMethods.ReadInt(Process, nodeAddress);
                resultNode.NextNode = nextNodeAddress;//always memorySharp.ReadInt(nodeAddress + 4), false);
                resultNode.PreviousNode = NativeMethods.ReadInt(Process, nodeAddress + 8);
                resultNode.Data = ReadPlayer(resultNode.DataAddress, team, encoding, currentDate);
                result.AddLast(resultNode);
                //move next
                nodeAddress = nextNodeAddress;
                if (nodeAddress != 0)
                    nextNodeAddress = NativeMethods.ReadInt(Process, nodeAddress + 4);
            } while (nodeAddress != 0);
            return result;
        }

        public void LoadPlayerData(LinkedList<Player> playerData)
        {
            var currentPlayerData = this.ReadPlayers(false);
            if (currentPlayerData.Count() == 0) return;
            var respawnPlayerNodes = new ObjectWithNameCollectionWithIndex<PlayerNode>();
            var retiredPlayerIndex = new ObjectWithNameCollectionWithIndex<Player>(playerData);
            foreach (var playerNode in currentPlayerData)
            {
                var lastName = playerNode.Data.LastName;
                var firstName = playerNode.Data.FirstName;
                var namesakePlayers = retiredPlayerIndex.LookupByName(lastName, firstName);
                Player foundPlayer = null;
                if (namesakePlayers != null)
                {
                    foreach (var namesakePlayer in namesakePlayers)
                    {
                        if (Player.CompareAttributesApproximately(playerNode.Data, namesakePlayer) == 0)
                        {
                            WritePlayerWithData(playerNode, namesakePlayer);
                            foundPlayer = namesakePlayer;
                            break;
                        }
                    }
                }
                if (foundPlayer != null)
                {
                    retiredPlayerIndex.Remove(foundPlayer);
                }
                else
                {
                    respawnPlayerNodes.AddObjectWithName(playerNode);
                }
            }
            var retiredPlayerLastNames = retiredPlayerIndex.Keys;
            LinkedList<PlayerNode> unmatchedPlayerNodes = new LinkedList<PlayerNode>();
            LinkedList<Player> unmatchedPlayers= new LinkedList<Player>();
            foreach (var retiredPlayerLastName in retiredPlayerLastNames)
            {
                var retiredPlayersWithSameLastName = retiredPlayerIndex.LookupByLastName(retiredPlayerLastName);
                var respawnPlayersWithSameLastName = respawnPlayerNodes.LookupByLastName(retiredPlayerLastName);
                while(retiredPlayersWithSameLastName.Count>0 && respawnPlayersWithSameLastName.Count>0)
                {
                    var from = respawnPlayersWithSameLastName.First.Value;
                    var to = retiredPlayersWithSameLastName.First.Value;
                    WritePlayerWithData(from,to);
                    Debug.WriteLine(String.Format("Matched {0} with {1}", from.Data, to));
                    respawnPlayersWithSameLastName.Remove(from);
                    retiredPlayersWithSameLastName.Remove(to);
                }
                while (respawnPlayersWithSameLastName.Count > 0)
                {
                    var from = respawnPlayersWithSameLastName.First.Value;
                    unmatchedPlayerNodes.AddLast(from);
                    respawnPlayersWithSameLastName.Remove(from);
                }
                while (retiredPlayersWithSameLastName.Count > 0)
                {
                    var to = retiredPlayersWithSameLastName.First.Value;
                    unmatchedPlayers.AddLast(to);
                    retiredPlayersWithSameLastName.Remove(to);
                }
            }
            foreach (var playerNode in respawnPlayerNodes.FlattenValues())
            {
                unmatchedPlayerNodes.AddLast(playerNode);
            }
            
            while (unmatchedPlayerNodes.Count > 0 && unmatchedPlayers.Count > 0)
            {
                var from = unmatchedPlayerNodes.First.Value;
                var to = unmatchedPlayers.First.Value;
                WritePlayerWithData(from, to);
                Debug.WriteLine(String.Format("Matched {0} with {1}", from.Data, to));
                unmatchedPlayerNodes.Remove(from);
                unmatchedPlayers.Remove(to);
            }
        }

        private void WritePlayerWithData(PlayerNode playerNode, Player to)
        {
            int playerDataAddress = playerNode.DataAddress;
            Player player = playerNode.Data;

            player.Speed = Math.Max(player.Speed, to.Speed);
            player.Agility = Math.Max(player.Agility, to.Agility);
            player.Acceleration = Math.Max(player.Acceleration, to.Acceleration);
            player.Stamina = Math.Max(player.Stamina, to.Stamina);
            player.Strength = Math.Max(player.Strength, to.Strength);
            player.Fitness = Math.Max(player.Fitness, to.Fitness);
            player.Shooting = Math.Max(player.Shooting, to.Shooting);
            player.Passing = Math.Max(player.Passing, to.Passing);
            player.Heading = Math.Max(player.Heading, to.Heading);
            player.Control = Math.Max(player.Control, to.Control);
            player.Dribbling = Math.Max(player.Dribbling, to.Dribbling);
            player.Coolness = Math.Max(player.Coolness, to.Coolness);
            player.Awareness = Math.Max(player.Awareness, to.Awareness);
            player.TackleDetermination = Math.Max(player.TackleDetermination, to.TackleDetermination);
            player.TackleSkill = Math.Max(player.TackleSkill, to.TackleSkill);
            player.Flair = Math.Max(player.Flair, to.Flair);
            player.Kicking = Math.Max(player.Kicking, to.Kicking);
            player.Throwing = Math.Max(player.Throwing, to.Throwing);
            player.Handling = Math.Max(player.Handling, to.Handling);
            player.ThrowIn = Math.Max(player.ThrowIn, to.ThrowIn);
            player.Leadership = Math.Max(player.Leadership, to.Leadership);
            player.Consistency = Math.Max(player.Consistency, to.Consistency);
            player.Determination = Math.Max(player.Determination, to.Determination);
            player.Greed = Math.Max(player.Greed, to.Greed);
            WritePlayer(playerNode);
        }

        public void BoostYouthPlayer(bool currentTeamOnly)
        {
            try
            {
                NativeMethods.SuspendProcess(Process);
                int currentDate = NativeMethods.ReadInt(Process, DateAddress);
                DateTime currentDateTime = new DateTime(1899, 12, 30).AddDays(currentDate);
                if (currentDateTime.Month < 5 || currentDateTime.Month > 7)
                {
                    throw new InvalidOperationException("只能在赛季初修改年轻球员数据 (Can only change youth player data at the beginning of the season)");
                }
                var playerNodes = ReadPlayers(currentTeamOnly);
                var youthPlayerNodes = playerNodes.Where(p => p.Data.Age < 20 && p.Data.ContractWeeks == 144 || p.Data.ContractWeeks == 143)
                    .ToList();
                if (youthPlayerNodes.Count == 0)
                {
                    throw new InvalidOperationException("未找到匹配的年轻球员。注意只能在赛季初修改年轻球员数据 (Youth player not found. Note can only change youth player data at the beginning of the season)");
                }
                foreach (var playerNode in youthPlayerNodes)
                {
                    var player = playerNode.Data;
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
                    player.PositionRating = player.GetPositionRating(player.Position);
                    player.UpdateBestPosition();
                    player.Position = player.BestPosition;
                    WritePlayer(playerNode);
                }
            }
            finally
            {

                NativeMethods.ResumeProcess(Process);
            }
        }
        public void RotatePlayer(RotateMethod rotateMethod, Formation targetFormation)
        {
            try
            {
                NativeMethods.SuspendProcess(Process);

                var players = ReadPlayers(true);
                if (players.Count == 0)
                {
                    Debug.WriteLine("Current team has no players");
                    return;
                }
                var leftoverPlayers = new PlayerNodeList();
                foreach (var player in players)
                {
                    leftoverPlayers.AddLast(player);
                }

                List<PlayerNode> normals = new List<PlayerNode>();
                List<PlayerNode> subs = new List<PlayerNode>();
                List<PlayerNode> rest = new List<PlayerNode>();

                GetGKs(leftoverPlayers, rotateMethod, normals, subs);
                GetNormals(leftoverPlayers, rotateMethod, normals, targetFormation);
                GetSubs(leftoverPlayers, rotateMethod, normals, subs, targetFormation);
                GetRest(leftoverPlayers, rotateMethod, rest, targetFormation);
                FixPositionAndSaveChangesToGame(normals, subs, rest);
            }
            finally {
                NativeMethods.ResumeProcess(Process);
            }
        }

        private void GetGKs(PlayerNodeList leftoverPlayers, RotateMethod rotateMethod,
            List<PlayerNode> normals, List<PlayerNode> subs)
        {

            IOrderedEnumerable<PlayerNode> gkQuery;
            switch (rotateMethod)
            {
                case RotateMethod.Energy:
                    gkQuery = leftoverPlayers.OrderByDescending(p => p.Data.Energy +
                    p.Data.GetPositionRating((int)PlayerPosition.GK)).ThenBy(p => this.random.Next());
                    break;
                case RotateMethod.Statistics:
                default:
                    gkQuery = leftoverPlayers.OrderByDescending(p => p.Data.Statistics +
                    p.Data.GetPositionRating((int)PlayerPosition.GK)).ThenBy(p => this.random.Next());
                    break;
            }
            var gks = gkQuery.Take(2).ToArray();
            var mainGK = gks[0];
            var backupGK = gks[1];

            normals.Add(mainGK);
            subs.Add(backupGK);
            if (mainGK.Data.Position != (int)PlayerPosition.GK)
            {
                mainGK.Data.Position = (int)PlayerPosition.GK;
                WritePlayerPosition(mainGK);
            }
            if (backupGK.Data.Position != (int)PlayerPosition.GK)
            {
                backupGK.Data.Position = (int)PlayerPosition.GK;
                WritePlayerPosition(backupGK);
            }

            leftoverPlayers.Remove(mainGK);
            leftoverPlayers.Remove(backupGK);
        }

        private void GetNormals(PlayerNodeList leftoverPlayers, RotateMethod rotateMethod, List<PlayerNode> normals, Formation targetFormation)
        {
            if (targetFormation != null)
            {
                for (int position = (int)PlayerPosition.SS; position > 0; position--)//choose all position except gk which is chosen
                {
                    for (int requiredPlayersInPosition = targetFormation.PlayersInEachPosition[position];
                        requiredPlayersInPosition > 0; requiredPlayersInPosition--)
                    {
                        IOrderedEnumerable<PlayerNode> bestFitsQuery;
                        switch (rotateMethod)
                        {
                            case RotateMethod.Energy:
                                bestFitsQuery = leftoverPlayers.OrderByDescending(p => p.Data.Energy + p.Data.GetPositionRating(position))
                                    .ThenBy(p => p.Data.GetAveragePositionRatingInFormationExceptTargetPositionAndGK(p.Data, position, targetFormation))
                                    .ThenBy(p => this.random.Next());
                                break;
                            case RotateMethod.Statistics:
                            default:
                                bestFitsQuery = leftoverPlayers.OrderByDescending(p => p.Data.Statistics + 
                                    p.Data.GetPositionRating(position))
                                    .ThenBy(p => p.Data.GetAveragePositionRatingInFormationExceptTargetPositionAndGK(p.Data, position, targetFormation))
                                    .ThenBy(p => this.random.Next());
                                break;
                        }
                        var bestFit = bestFitsQuery.First();
                        var targetPosition = position;
                        if (bestFit.Data.Position != targetPosition)
                        {
                            bestFit.Data.Position = targetPosition;
                            WritePlayerPosition(bestFit);
                        }
                        leftoverPlayers.Remove(bestFit);
                        normals.Add(bestFit);
                    }
                }
            }
            else {
                IOrderedEnumerable<PlayerNode> query;
                switch (rotateMethod)
                {
                    case RotateMethod.Energy:
                        query = leftoverPlayers.OrderByDescending(
                            p => p.Data.Energy + p.Data.GetBestPositionRatingExceptGKInFormation(null))
                            .ThenBy(p => this.random.Next());
                        break;
                    case RotateMethod.Statistics:
                    default:
                        query = leftoverPlayers.OrderByDescending(
                            p => p.Data.Statistics + p.Data.GetBestPositionRatingExceptGKInFormation(null)).
                            ThenBy(p => this.random.Next());
                        break;
                }
                var mainTeam = query.Take(10).ToArray();
                bool lb = false;
                bool lwb = false;
                bool lm = false;
                bool lw = false;
                foreach (var mainTeamPlayer in mainTeam)
                {
                    var targetPosition = mainTeamPlayer.Data.BestPosition;
                    if (targetPosition == (int)PlayerPosition.GK)
                        targetPosition = mainTeamPlayer.Data.GetBestPositionExceptGKInFormation(null);
                    switch ((PlayerPosition)targetPosition)
                    {
                        case PlayerPosition.RB:
                            if (lb)
                            {
                                targetPosition = (int)PlayerPosition.LB;
                            }
                            else
                                targetPosition = (int)PlayerPosition.RB;
                            lb = !lb; break;
                        case PlayerPosition.RWB:
                            if (lwb)
                            {
                                targetPosition = (int)PlayerPosition.LWB;
                            }
                            else
                                targetPosition = (int)PlayerPosition.RWB;
                            lwb = !lwb; break;
                        case PlayerPosition.RM:
                            if (lm)
                            {
                                targetPosition = (int)PlayerPosition.LM;
                            }
                            else
                                targetPosition = (int)PlayerPosition.RM;
                            lm = !lm; break;
                        case PlayerPosition.RW:
                            if (lw)
                            {
                                targetPosition = (int)PlayerPosition.LW;
                            }
                            else
                                targetPosition = (int)PlayerPosition.RW;
                            lw = !lw; break;
                        default: break;
                    }
                    if (mainTeamPlayer.Data.Position != targetPosition)
                    {
                        mainTeamPlayer.Data.Position = targetPosition;
                        WritePlayerPosition(mainTeamPlayer);
                    }
                    normals.Add(mainTeamPlayer);
                    leftoverPlayers.Remove(mainTeamPlayer);
                }
            }
        }


        private void GetSubs(PlayerNodeList leftoverPlayers, RotateMethod rotateMethod, List<PlayerNode> normals, List<PlayerNode> subs, Formation targetFormation)
        {
            int subNeeded = 4;
            bool hasFrontCourt = false;
            bool hasWings = false;
            bool hasBackCourt = false;
            bool hasMiddleField = false;
            foreach (var mainTeamPlayer in normals)
            {
                var playerPosition = (PlayerPosition)mainTeamPlayer.Data.Position;
                switch (playerPosition)
                {
                    case PlayerPosition.LB:
                    case PlayerPosition.RB:
                    case PlayerPosition.CD:
                    case PlayerPosition.SW:
                        hasBackCourt = true;
                        break;
                    case PlayerPosition.RM:
                    case PlayerPosition.LM:
                    case PlayerPosition.DM:
                    case PlayerPosition.AM:
                        hasMiddleField = true;
                        break;
                    case PlayerPosition.RWB:
                    case PlayerPosition.LWB:
                    case PlayerPosition.RW:
                    case PlayerPosition.LW:
                    case PlayerPosition.FR:
                        hasWings = true;
                        break;
                    case PlayerPosition.SS:
                    case PlayerPosition.FOR:
                        hasFrontCourt = true;
                        break;
                }
            }
            if (hasFrontCourt)
            {
                PlayerPosition[] targetPositions = new PlayerPosition[] {
                    PlayerPosition.SS,PlayerPosition.FOR,
                };
                GetASub(leftoverPlayers, rotateMethod, subs, targetPositions);
                subNeeded--;
            }
            if (hasWings)
            {
                PlayerPosition[] targetPositions = new PlayerPosition[] {
                    PlayerPosition.RWB,PlayerPosition.LWB,
                    PlayerPosition.RW,PlayerPosition.LW,
                    PlayerPosition.FR,
                };
                GetASub(leftoverPlayers, rotateMethod, subs, targetPositions);
                subNeeded--;
            }
            if (hasMiddleField)
            {
                PlayerPosition[] targetPositions = new PlayerPosition[] {
                    PlayerPosition.RM,PlayerPosition.LM,
                    PlayerPosition.AM,PlayerPosition.DM,
                };
                GetASub(leftoverPlayers, rotateMethod, subs, targetPositions);
                subNeeded--;
            }
            if (hasBackCourt)
            {
                PlayerPosition[] targetPositions = new PlayerPosition[] {
                    PlayerPosition.RB,PlayerPosition.LB,
                    PlayerPosition.SW,PlayerPosition.CD,
                };
                GetASub(leftoverPlayers, rotateMethod, subs, targetPositions);
                subNeeded--;
            }
            if (subNeeded > 0)
            {
                IOrderedEnumerable<PlayerNode> subQuery;
                switch (rotateMethod)
                {
                    case RotateMethod.Energy:
                        subQuery = leftoverPlayers.OrderByDescending(p => p.Data.Energy +
                                p.Data.GetBestPositionRatingExceptGKInFormation(targetFormation)).ThenBy(p => this.random.Next());
                        break;
                    case RotateMethod.Statistics:
                    default:
                        subQuery = leftoverPlayers.OrderByDescending(p => p.Data.Statistics +
                                p.Data.GetBestPositionRatingExceptGKInFormation(targetFormation)).ThenBy(p => this.random.Next());
                        break;
                }
                var subRest = subQuery.Take(subNeeded).ToArray();
                foreach (var subTeamPlayer in subRest)
                {
                    int targetPosition;
                    if (targetFormation == null)
                        targetPosition = subTeamPlayer.Data.BestPosition;
                    else
                        targetPosition = subTeamPlayer.Data.BestFitInFormation(targetFormation);

                    if (targetPosition != subTeamPlayer.Data.Position)
                    {
                        subTeamPlayer.Data.Position = targetPosition;
                        WritePlayerPosition(subTeamPlayer);
                    }
                    leftoverPlayers.Remove(subTeamPlayer);
                    subs.Add(subTeamPlayer);
                }
            }
        }

        private void GetASub(PlayerNodeList leftoverPlayers, RotateMethod rotateMethod, List<PlayerNode> subs, PlayerPosition[] targetPositions)
        {
            IOrderedEnumerable<PlayerNode> subQuery;

            switch (rotateMethod)
            {
                case RotateMethod.Energy:
                    subQuery = leftoverPlayers.OrderByDescending(p => p.Data.Energy +
                            p.Data.GetBestPositionRating(targetPositions)).ThenBy(p => this.random.Next());
                    break;
                case RotateMethod.Statistics:
                default:
                    subQuery = leftoverPlayers.OrderByDescending(p => p.Data.Statistics +
                           p.Data.GetBestPositionRating(targetPositions)).ThenBy(p => this.random.Next());
                    break;
            }
            var subTeamPlayer = subQuery.First();
            var targetPosition = subTeamPlayer.Data.BestFitInPositions(targetPositions);
            if (targetPosition != subTeamPlayer.Data.Position)
            {
                subTeamPlayer.Data.Position = targetPosition;
                WritePlayerPosition(subTeamPlayer);
            }
            leftoverPlayers.Remove(subTeamPlayer);
            subs.Add(subTeamPlayer);
        }

        private void GetRest(PlayerNodeList leftoverPlayers, RotateMethod rotateMethod, List<PlayerNode> rest, Formation targetFormation)
        {
            foreach (var leftoverPlayer in leftoverPlayers)
            {
                int  targetPosition;
                if(targetFormation==null)
                    targetPosition = leftoverPlayer.Data.BestPosition;
                else
                    targetPosition = leftoverPlayer.Data.BestFitInFormation(targetFormation);             

                if (targetPosition != leftoverPlayer.Data.Position)
                {
                    leftoverPlayer.Data.Position = targetPosition;
                    WritePlayerPosition(leftoverPlayer);
                }
                rest.Add(leftoverPlayer);
            }

        }
        private void FixPositionAndSaveChangesToGame(List<PlayerNode> normals, List<PlayerNode> subs, List<PlayerNode> rest)
        {
            var newPlayers = new LinkedList<PlayerNode>();
            foreach (var playerNode in normals)
            {
                var player = playerNode.Data;
                if (player.Status != 0)
                {
                    player.Status = 0;
                    WritePlayerStatus(playerNode);
                }
                newPlayers.AddLast(playerNode);
            }
            foreach (var playerNode in subs)
            {
                var player = playerNode.Data;
                if (player.Status != 1)
                {
                    player.Status = 1;
                    WritePlayerStatus(playerNode);
                }
                newPlayers.AddLast(playerNode);
            }
            foreach (var playerNode in rest)
            {
                var player = playerNode.Data;
                if (player.Status != 2)
                {
                    player.Status = 2;
                    WritePlayerStatus(playerNode);
                }
                newPlayers.AddLast(playerNode);
            }
            if (newPlayers.Count > 0)
            {
                var currentNode = newPlayers.First;
                ushort currentTeam = NativeMethods.ReadByte(Process, CurrentTeamIndexAddress);
                int teamDataAddress = TeamDataAddress + currentTeam * 0x140;
                NativeMethods.WriteInt(Process, teamDataAddress + 0x136, currentNode.Value.NodeAddress);
                while (currentNode != null)
                {
                    if (currentNode.Previous == null)
                    {
                        currentNode.Value.PreviousNode = 0;
                    }
                    else
                        currentNode.Value.PreviousNode = currentNode.Previous.Value.NodeAddress;

                    if (currentNode.Next == null)
                    {
                        currentNode.Value.NextNode = 0;
                    }
                    else
                        currentNode.Value.NextNode = currentNode.Next.Value.NodeAddress;

                    NativeMethods.WriteInt(Process, currentNode.Value.NodeAddress + 4, currentNode.Value.NextNode);
                    NativeMethods.WriteInt(Process, currentNode.Value.NodeAddress + 8, currentNode.Value.PreviousNode);
                    currentNode = currentNode.Next;
                }
            }
        }

        public void ImproveAllPlayersBy1()
        {
            try
            {
                NativeMethods.SuspendProcess(Process);
                var playerNodes = ReadPlayers(false);
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
                    WritePlayer(playerNode);
                }
            }
            finally {
                NativeMethods.ResumeProcess(Process);
            }
        }


        internal void FastUpdate(bool autoTrain, bool convertToGK, bool autoResetStatus, bool maxEnergy, bool maxForm, bool maxMorale,bool maxPower, bool noAlternativeTraining,
            TrainingEffectModifier trainingEffectModifier)
        {
            try
            {
                NativeMethods.SuspendProcess(Process);
                var playerNodes = ReadPlayers(true);
                foreach (var playerNode in playerNodes)
                {
                    if (autoTrain)
                    {
                        var playerSchedule = TrainingSchedule.GetTrainingSchedule(playerNode.Data, autoResetStatus, maxEnergy, maxPower, noAlternativeTraining, trainingEffectModifier)
                            .Select(p => (byte)p).ToArray();

                        if (playerNode.Data.Fitness < 99)
                        {
                            if (playerNode.Data.Status != 0 && convertToGK)
                            {
                                playerNode.Data.Position = (byte)PlayerPosition.GK;
                                WritePlayerPosition(playerNode);
                            }
                        }
                        else
                        {
                            if (playerNode.Data.Status != 0 && convertToGK)
                            {
                                if (playerNode.Data.Position == (byte)PlayerPosition.GK && playerNode.Data.BestPosition != (byte)PlayerPosition.GK)
                                {
                                    playerNode.Data.Position = playerNode.Data.BestPosition;
                                    WritePlayerPosition(playerNode);
                                }
                            }
                        }
                        int playerScheduleAddress = TrainingDataAddress +
                            (playerNode.Data.Number - 1) * 116;
                        NativeMethods.WriteBytes(Process, playerScheduleAddress, playerSchedule, 0, 7);
                    }
                    if (autoResetStatus)
                    {
                        if (playerNode.Data.Status > 2)
                        {
                            playerNode.Data.Status = 2;
                            WritePlayerStatus(playerNode);
                        }
                    }
                    if (maxEnergy || maxForm || maxMorale)
                    {

                        if (maxEnergy)
                        {
                            playerNode.Data.Energy = 99;
                        }
                        if (maxForm)
                        {
                            playerNode.Data.Form = 99;
                        }
                        if (maxMorale)
                        {
                            playerNode.Data.Moral = 99;
                        }
                        WritePlayerFormMoralEnergy(playerNode);
                    }
                }
            }
            finally {
                NativeMethods.ResumeProcess(Process);
            }
        }


        internal void SlowUpdate(bool autoRenewContracts,bool maxPower, 
            TrainingEffectModifier trainingEffectModifier)
        {
            try
            {
                NativeMethods.SuspendProcess(Process);
                var playerNodes = ReadPlayers(true);
                foreach (var playerNode in playerNodes)
                {
                    if (autoRenewContracts)
                    {
                        if (playerNode.Data.ContractWeeks < 144)
                        {
                            playerNode.Data.ContractWeeks = 255;
                            WritePlayerContractWeeks(playerNode);
                        }
                    }
                    if (maxPower)
                    {
                        playerNode.Data.Stamina = 99;
                        playerNode.Data.Strength = 99;
                        playerNode.Data.Fitness = 99;
                        WritePlayerStrengths(playerNode);
                    }
                }
            }
            finally {
                NativeMethods.SuspendProcess(Process);
            }
        }

        internal bool HasExited()
        {
            if (Process == null) return true;
            return Process.HasExited;
        }

        internal void ResetDate(uint targetYear)
        {
            try
            {
                NativeMethods.SuspendProcess(Process);
                int currentDate = NativeMethods.ReadInt(Process, DateAddress);
                DateTime currentDateTime = new DateTime(1899, 12, 30).AddDays(currentDate);
                if (currentDateTime.Month < 5 || currentDateTime.Month > 7)
                {
                    throw new InvalidOperationException("只能在休赛季修改日期 (Can only change date in offseason)");
                }
                DateTime resetDateTime = new DateTime((int)targetYear, currentDateTime.Month, currentDateTime.Day);
                TimeSpan resetTimeSpan = currentDateTime - resetDateTime;
                int daysToSubtract = resetTimeSpan.Days;
                var playerList = this.ReadPlayers(false);
                foreach (var playerNode in playerList)
                {
                    playerNode.Data.BirthDateOffset = (ushort)(playerNode.Data.BirthDateOffset - daysToSubtract);
                    WritePlayerBirthDate(playerNode);
                }
                NativeMethods.WriteInt(Process, DateAddress, currentDate - daysToSubtract);
            }
            finally {
                NativeMethods.ResumeProcess(Process);
            }
        }

        internal void AutoPosition(Formation targetFormation)
        {
            try
            {
                NativeMethods.SuspendProcess(Process);

                var playerNodes = ReadPlayers(true);
                if (targetFormation == null)
                {
                    bool lb = false;
                    bool lwb = false;
                    bool lm = false;
                    bool lw = false;
                    foreach (var playerNode in playerNodes)
                    {
                        if (playerNode.Data.Position != playerNode.Data.BestPosition)
                        {
                            playerNode.Data.Position = playerNode.Data.BestPosition;
                        }
                        switch ((PlayerPosition)playerNode.Data.BestPosition)
                        {
                            case PlayerPosition.RB:
                                if (lb)
                                {
                                    playerNode.Data.Position = (int)PlayerPosition.LB;
                                }
                                else
                                    playerNode.Data.Position = (int)PlayerPosition.RB;
                                lb = !lb; break;
                            case PlayerPosition.RWB:
                                if (lwb)
                                {
                                    playerNode.Data.Position = (int)PlayerPosition.LWB;
                                }
                                else
                                    playerNode.Data.Position = (int)PlayerPosition.RWB;
                                lwb = !lwb; break;
                            case PlayerPosition.RM:
                                if (lm)
                                {
                                    playerNode.Data.Position = (int)PlayerPosition.LM;
                                }
                                else
                                    playerNode.Data.Position = (int)PlayerPosition.RM;
                                lm = !lm; break;
                            case PlayerPosition.RW:
                                if (lw)
                                {
                                    playerNode.Data.Position = (int)PlayerPosition.LW;
                                }
                                else
                                    playerNode.Data.Position = (int)PlayerPosition.RW;
                                lw = !lw; break;
                            default: break;
                        }
                        WritePlayerPosition(playerNode);
                    }
                }
                else
                {
                    if (playerNodes.Where(p => p.Data.Status == 0).Count() != 11)
                    {
                        throw new InvalidOperationException("当前场上需要11名球员 (Auto Position to Formation requires 11 Players on the field");
                    }
                    var leftoverPlayers = new PlayerNodeList();

                    foreach (var player in playerNodes.Where(p => p.Data.Status == 0))
                    {
                        leftoverPlayers.AddLast(player);
                    }
                    for (int position = (int)PlayerPosition.SS; position >= 0; position--)
                    {
                        for (int requiredPlayersInPosition = targetFormation.PlayersInEachPosition[position];
                            requiredPlayersInPosition > 0; requiredPlayersInPosition--)
                        {
                            PlayerNode bestPlayerForPosition = null;
                            double bestPlayerRatingForPosition = 0;
                            foreach (var leftoverPlayer in leftoverPlayers)
                            {
                                double positionRating = leftoverPlayer.Data.GetPositionRatingDouble(position);
                                if (positionRating > bestPlayerRatingForPosition)
                                {
                                    bestPlayerRatingForPosition = positionRating;
                                    bestPlayerForPosition = leftoverPlayer;
                                }
                            }
                            if (bestPlayerForPosition.Data.Position != position)
                            {
                                bestPlayerForPosition.Data.Position = position;
                                WritePlayerPosition(bestPlayerForPosition);
                            }
                            leftoverPlayers.Remove(bestPlayerForPosition);
                        }
                    }
                }
            }
            finally {
                NativeMethods.ResumeProcess(Process);
            }
        }

        internal TrainingEffectModifier ReadTrainingEffectModifier()
        {            
            if (TrainingEffectAddress != 0)
            {
                var trainingEffectBytes = NativeMethods.ReadBytes(Process, TrainingEffectAddress, 4 * 27 * ((int)TrainingScheduleType.TrainingMatch + 1));
                return TrainingScheduleEffect.DetectModifiers(trainingEffectBytes);
            }
            return new TrainingEffectModifier(); 
        }

        internal void GetCurrentFormation(Formation savedFormation)
        {
            var playerNodes = ReadPlayers(true);
            for (int i = 0; i < savedFormation.PlayersInEachPosition.Length; i++)
            {
                savedFormation.PlayersInEachPosition[i] =
                    playerNodes.Where(
                        p => p.Data.Status == 0
                        && p.Data.Position == i
                        ).Count();
            }
        }

        internal void Kill()
        {
            Process.Kill();
        }
        internal void UpdatePlayerNames(string respawnCategory)
        {
            try
            {
                NativeMethods.SuspendProcess(Process);

                var playerNodes = ReadPlayers(false);
                Dictionary<string, Dictionary<string, Player>> playerDictionary = new Dictionary<string, Dictionary<string, Player>>();
                foreach (var item in playerNodes)
                {
                    if (!playerDictionary.ContainsKey(item.Data.LastName))
                    {
                        playerDictionary.Add(item.Data.LastName, new Dictionary<string, Player>());
                    }
                    if (!playerDictionary[item.Data.LastName].ContainsKey(item.Data.FirstName))
                    {
                        playerDictionary[item.Data.LastName].Add(item.Data.FirstName, item.Data);
                    }
                }
                var newSpawns = playerNodes.Where(p => p.Data.Age < 20 && p.Data.ContractWeeks == 144)
                    .OrderByDescending(p=>p.Data.Statistics).ToList();
                if (newSpawns.Count() == 0)
                {
                    throw new InvalidOperationException("没找到重生球员，此功能必须在新赛季初执行 （Cannot find new spawn, this feature can only be run at the beginning of the season");
                }
                List<HumanName> newNames = GetNewPlayerNames(respawnCategory);
                foreach (var item in newNames)
                {
                    if (newSpawns.Count == 0) break;
                    if (playerDictionary.ContainsKey(item.Last))
                    {
                        if (playerDictionary[item.Last].ContainsKey(item.First))
                            continue;
                    }

                    var newSpawn = newSpawns.First();
                    newSpawn.Data.FirstName = item.First;
                    newSpawn.Data.LastName = item.Last;
                    newSpawn.Data.ContractWeeks = 143;//skip in the next update
                    WritePlayerNames(newSpawn);
                    WritePlayerContractWeeks(newSpawn);
                    if (!playerDictionary.ContainsKey(item.Last))
                    {
                        playerDictionary.Add(item.Last, new Dictionary<string, Player>());
                    }
                    if (!playerDictionary[item.Last].ContainsKey(item.First))
                    {
                        playerDictionary[item.Last].Add(item.First, newSpawn.Data);
                    }
                    newSpawns.Remove(newSpawn);
                }
            }
            finally
            {
                NativeMethods.ResumeProcess(Process);
            }
        }

        private void WritePlayerNames(PlayerNode playerNode)
        {
            NativeMethods.WriteString(Process, playerNode.Data.LastName,playerNode.DataAddress + 0x1c, Encoding, 0x13);
            NativeMethods.WriteString(Process, playerNode.Data.FirstName, playerNode.DataAddress + 0x4, Encoding, 0x18);
        }

        private List<HumanName> GetNewPlayerNames(string respawnCategory)
        {
            List<HumanName> result = new List<HumanName>();
            using (WebClient client = new WebClient())
            {
                int currentDateInt = NativeMethods.ReadInt(Process, DateAddress);
                DateTime currentDateTime = new DateTime(1899, 12, 30).AddDays(currentDateInt);
                var toYear = currentDateTime.AddYears(-17).Year;
                var fromYear = currentDateTime.AddYears(-17).Year;
                var language = "en";
                switch (Encoding.CodePage)
                {
                    case 936:
                        language = "zh";
                        break;
                    case 437:
                    default:
                        language = "en";
                        break;
                }
                string url;
                if (string.IsNullOrWhiteSpace(respawnCategory))
                {
                    /*PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
    PREFIX dbo: <http://dbpedia.org/ontology/>
    PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
    PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>

    SELECT ?player, ?name WHERE {
    ?player dbo:wikiPageID ?number;
    rdf:type dbo:SoccerPlayer ;
               dbo:birthDate|dbp:birthDate ?birthdate ;
               rdfs:label ?name 
     FILTER ((lang(?name)="zh")&&?birthdate>= "1970-01-01"^^xsd:date && ?birthdate<= "1970-12-31"^^xsd:date ) . 
    } 
    ORDER BY (?number)*/
                    url = string.Format(Properties.Resources.GetPlayerByYearQueryUrl, fromYear, toYear, language);
                }
                else
                {
                    /*
                     * PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
PREFIX dbo: <http://dbpedia.org/ontology/>
PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>

SELECT DISTINCT ?player , ?name
WHERE {?player dbo:wikiPageID ?number; rdf:type dbo:SoccerPlayer ;
dbo:birthDate|dbp:birthDate ?birthdate ;rdfs:label ?name ;dcterms:subject ?category
FILTER ((lang(?name)="zh")&&?birthdate>= "1960-01-01"^^xsd:date && ?birthdate<= "1997-12-31"^^xsd:date && ?category =<http://dbpedia.org/resource/Category:A.C._Milan_players> ). 
} 
ORDER BY (?number)
                     */
                    url = string.Format(Properties.Resources.GetPlayerByYearCndCategoryQueryUrl, fromYear, toYear, language,
                      Uri.EscapeDataString(respawnCategory));
                }
                string json = client.DownloadString(url);
                var queryResult = JObject.Parse(json);
                List<QueryUpdatePlayerNameResult> downloadedNames = queryResult.SelectToken("results").SelectToken("bindings")
                    .Select(jt => jt.ToObject<QueryUpdatePlayerNameResult>()).ToList();
                if (downloadedNames.Count == 0)
                {
                    //this would give people who are most famous when Wikipedia founded which is not far from when the game was released. 
                    url = string.Format(Properties.Resources.GetPlayerByYearQueryUrl, "1960", toYear, language);
                    json = client.DownloadString(url);
                    queryResult = JObject.Parse(json);
                    downloadedNames = queryResult.SelectToken("results").SelectToken("bindings")
                        .Select(jt => jt.ToObject<QueryUpdatePlayerNameResult>()).ToList();
                }
                foreach (var item in downloadedNames)
                {
                    var pageUrl = item.Player.Value.Replace("http://dbpedia.org/resource/", string.Empty);
                    var fullName = item.Name.Value;
                    fullName = ZhConverter.HantToHans(fullName);
                    if (fullName.Contains("("))
                    {
                        fullName = fullName.Substring(0, fullName.IndexOf("(")).Trim();
                    }
                    if (fullName.Contains("("))
                    {
                        fullName = fullName.Substring(0, fullName.IndexOf("(")).Trim();
                    }
                    fullName = fullName.Replace("·", " ");
                    HumanName humanName = new HumanName(fullName);
                    if (string.IsNullOrWhiteSpace(humanName.Last))
                    {
                        fullName = string.Format("{0} {1}", pageUrl.Substring(0, 1), fullName);
                        humanName = new HumanName(fullName);
                    }
                    else {
                        var pageUrlParts = pageUrl.Split('_');
                        var firstName = pageUrlParts.First().RemoveDiacritics();
                        fullName = string.Format("{0} {1}", firstName, fullName);
                        humanName = new HumanName(fullName);
                    }
                    result.Add(humanName);
                }
            }
            return result;
        }
    }
}

using Diacritics.Extensions;
using FSM97Lib;
using HtmlAgilityPack;
using NameParser;
using Newtonsoft.Json.Linq;
using OpenCCNET;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Fsm97Trainer
{
    public class MenusProcess : IDisposable
    {
        private const int totalTeamCount = 349;
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

        public int AssetAddress { get; internal set; }
        public int SeatsAddresses { get; set; }

        public Encoding Encoding { get; private set; }
        Process Process { get; set; }


        TrainingEffectModifier trainingEffectModifier;
        public MenusProcess()
        {
            Process[] processes = Process.GetProcessesByName("MENUS");
            if (processes == null || processes.Length == 0)
                processes = Process.GetProcessesByName("MENUS.EXE");//for WINE
            if (processes.Count() > 1)
            {
                for (int i = 0; i < processes.Length; i++)
                {
                    // User must also dispose of any matched Processes that are returned
                    processes[i].Dispose();
                }
                throw new InvalidOperationException(Properties.Resources.MultipleGameProcessFound);
            }
            else if (processes.Count() == 0)
            {
                throw new InvalidOperationException(Properties.Resources.CannotFindGameProcess);
            }
            Process = processes.First();
            var gameExeFilePath = Process.MainModule.FileName;
            FileInfo fi = new FileInfo(gameExeFilePath);
            switch (fi.Length)
            {
                case 1378816:
                    //menusProcess.SubCountAddress = 0x614610;
                    //menusProcess.DivisionFactorAddress = 0x4f3a60;
                    TeamDataAddress = 0x547102;
                    DateAddress = 0x562ED8;
                    CurrentTeamIndexAddress = 0x562a4c;
                    TrainingDataAddress = 0x562f50;
                    TrainingEffectAddress = 0x4e38a0;
                    AssetAddress = 0x58a549;
                    Encoding = Encoding.GetEncoding(936);
                    trainingEffectModifier = ReadTrainingEffectModifier();
                    break;
                case 1135104://English Ver 97/98 patch
                             //menusProcess.SubCountAddress = 0x5846e8;
                             //menusProcess.DivisionFactorAddress = 0x4f5178;
                    Encoding = Encoding.GetEncoding(437);
                    DateAddress = 0x5A4ae8;
                    TeamDataAddress = 0x588D12;
                    AssetAddress = 0x523f01;
                    CurrentTeamIndexAddress = 0x5a465c;
                    TrainingDataAddress = 0x5a4b60;
                    TrainingEffectAddress = 0x4e1000;
                    trainingEffectModifier = ReadTrainingEffectModifier();
                    break;
                case 1129472://English RTM
                             //menusProcess.SubCountAddress = 0x5846e8;
                             //menusProcess.DivisionFactorAddress = 0x4f5178;
                    Encoding = Encoding.GetEncoding(437);
                    DateAddress = 0x547b50;
                    TeamDataAddress = 0x52bd7a;
                    CurrentTeamIndexAddress = 0x5476C4;
                    TrainingDataAddress = 0x547BC8;
                    TrainingEffectAddress = 0x4E0DF8;
                    AssetAddress = 0x577df9;
                    trainingEffectModifier = ReadTrainingEffectModifier();
                    break;
                default:
                    Process.Dispose();
                    throw new InvalidOperationException(Properties.Resources.UnsupportedGameVersion);
            }
        }

        internal ushort ReadCurrentTeamIndex()
        {
            return NativeMethods.ReadUShort(Process, CurrentTeamIndexAddress);
        }
        public List<TeamNode> ReadTeams()
        {
            var result = new List<TeamNode>(totalTeamCount);
            for (int i = 0; i < totalTeamCount; i++)
            {
                TeamNode teamNode = new TeamNode();
                var team = new TeamModel();
                teamNode.Data = team;
                int teamDataAddress = TeamDataAddress + i * 0x140;
                teamNode.Address = teamDataAddress;
                team.Id = (ushort)i;
                team.Name = NativeMethods.ReadString(Process, teamDataAddress, Encoding, 24);
                team.FanGroupName = NativeMethods.ReadString(Process, teamDataAddress + 0x19, Encoding, 16);
                team.Abbreviation = NativeMethods.ReadString(Process, teamDataAddress + 0x2b, Encoding, 3);
                team.ManagerFirstName = NativeMethods.ReadString(Process, teamDataAddress + 0x94, Encoding, 11);
                team.ManagerLastName = NativeMethods.ReadString(Process, teamDataAddress + 0x9f, Encoding, 11);
                team.Stadium = NativeMethods.ReadString(Process, teamDataAddress + 0x32, Encoding, 16);
                team.MapName = NativeMethods.ReadString(Process, teamDataAddress + 0xBD, Encoding, 26);
                result.Add(teamNode);
            }
            return result;
        }
        public PlayerNodeList ReadPlayers(bool currentTeamOnly)
        {
            PlayerNodeList playerNodes = new PlayerNodeList();
            List<TeamNode> teams = ReadTeams();

            ushort currentTeam = NativeMethods.ReadUShort(Process, CurrentTeamIndexAddress);
            int currentDate = NativeMethods.ReadInt(Process, DateAddress);
            if (currentTeamOnly)
            {
                var teamNode = teams.FirstOrDefault(t => t.Data.Id == currentTeam);
                int teamDataAddress = teamNode.Address;
                int teamPlayerAddress = NativeMethods.ReadInt(Process, teamDataAddress + 0x136);
                if (teamPlayerAddress == 0)
                    teamPlayerAddress = NativeMethods.ReadInt(Process, teamDataAddress + 0x13a);
                if (teamPlayerAddress != 0)
                {
                    teamNode.PlayerNodes = ReadPlayers(teamPlayerAddress, teamNode, Encoding, currentDate);
                    foreach (var playerNode in teamNode.PlayerNodes)
                    {
                        playerNodes.AddLast(playerNode);
                    }

                    //Debug.WriteLine(String.Format("{0} has {1} players", team.Name, team.PlayerNodes.Count));
                }
            }
            else
            {
                for (int i = 0; i < totalTeamCount; i++)
                {
                    var teamNode = teams.FirstOrDefault(t => t.Data.Id == i);
                    int teamDataAddress = teamNode.Address;

                    int teamPlayerAddress = NativeMethods.ReadInt(Process, teamDataAddress + 0x136);
                    if (teamPlayerAddress != 0)
                    {
                        teamNode.PlayerNodes = ReadPlayers(teamPlayerAddress, teamNode, Encoding, currentDate);
                        teams.Add(teamNode);
                        foreach (var playerNode in teamNode.PlayerNodes)
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
                        Debug.WriteLine(String.Format("{0} ({1}) has no players", teamNode.Data.Name, teamNode.Data.Abbreviation));
                    }
                }
            }
            return playerNodes;
        }


        private PlayerNodeList ReadPlayers(int nodeAddress, TeamNode team, Encoding encoding, int currentDate)
        {
            PlayerNodeList result = new PlayerNodeList();
            if (nodeAddress == 0) return result;
            int nextNodeAddress = NativeMethods.ReadInt(Process, nodeAddress + 4);
            do
            {
                var resultNode = new PlayerNode();
                resultNode.NodeAddress = nodeAddress;
                resultNode.DataAddress = NativeMethods.ReadInt(Process, nodeAddress);
                resultNode.NextNode = nextNodeAddress;//always memorySharp.ReadInt(nodeAddress + 4), false);
                resultNode.PreviousNode = NativeMethods.ReadInt(Process, nodeAddress + 8);
                resultNode.ReadPlayer(Process, resultNode.DataAddress, team, encoding, currentDate);
                resultNode.TeamNode = team;
                result.AddLast(resultNode);
                //move next
                nodeAddress = nextNodeAddress;
                if (nodeAddress != 0)
                    nextNodeAddress = NativeMethods.ReadInt(Process, nodeAddress + 4);
            } while (nodeAddress != 0);
            return result;
        }

        public void LoadPlayerData(IEnumerable<PlayerModel> playerData)
        {
            var currentPlayerData = this.ReadPlayers(false);
            if (currentPlayerData.Count() == 0) return;


            var respawnPlayerNodes = new ObjectWithNameCollectionWithIndex<PlayerNode>();
            var retiredPlayerIndex = new ObjectWithNameCollectionWithIndex<PlayerModel>(playerData);
            foreach (var playerNode in currentPlayerData)
            {
                var lastName = playerNode.Data.LastName;
                var firstName = playerNode.Data.FirstName;
                var namesakePlayers = retiredPlayerIndex.LookupByName(lastName, firstName);
                PlayerModel foundPlayer = null;
                if (namesakePlayers != null)
                {
                    foreach (var namesakePlayer in namesakePlayers)
                    {
                        //twins?
                        if (playerNode.Data.BirthDateOffset == namesakePlayer.BirthDateOffset
                            && PlayerModel.CompareAttributesApproximately(playerNode.Data, namesakePlayer) == 0)
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
            var unmatchedPlayerNodes = new LinkedList<PlayerNode>();
            var unmatchedPlayers = new LinkedList<PlayerModel>();
            foreach (var retiredPlayerLastName in retiredPlayerLastNames)
            {
                var retiredPlayersWithSameLastName = retiredPlayerIndex.LookupByLastName(retiredPlayerLastName);
                var respawnPlayersWithSameLastName = respawnPlayerNodes.LookupByLastName(retiredPlayerLastName);
                while (retiredPlayersWithSameLastName.Count > 0 && respawnPlayersWithSameLastName.Count > 0)
                {
                    var from = respawnPlayersWithSameLastName.First.Value;
                    var to = retiredPlayersWithSameLastName.First.Value;
                    WritePlayerWithData(from, to);
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

        private void WritePlayerWithData(PlayerNode playerNode, PlayerModel to)
        {
            int playerDataAddress = playerNode.DataAddress;
            var player = playerNode.Data;
            if (to.Number > 0 && to.Number < 40)//game bug: no 40 causes access violation.
            {
                if (player.Number != to.Number)
                    player.Number = to.Number;
            }
            player.Nationality = to.Nationality;
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
            playerNode.WritePlayer(Process, Encoding);
        }

        public void BoostYouthPlayer(bool currentTeamOnly)
        {
            try
            {
                NativeMethods.SuspendProcess(Process);
                int currentDate = NativeMethods.ReadInt(Process, DateAddress);
                DateTime currentDateTime = PlayerModel.dateOffsetBase.AddDays(currentDate);
                if (currentDateTime.Month < 5 || currentDateTime.Month > 7)
                {
                    throw new InvalidOperationException(Properties.Resources.CanOnlyChangeAtSeasonStart);
                }
                var playerNodes = ReadPlayers(currentTeamOnly);
                var youthPlayerNodes = playerNodes.Where(p => p.Data.Age < 20 && p.Data.ContractWeeks == 144 || p.Data.ContractWeeks == 143)
                    .ToList();
                if (youthPlayerNodes.Count == 0)
                {
                    throw new InvalidOperationException(Properties.Resources.YouthPlayerNotFound);
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
                    player.Position = player.BestPosition;
                    playerNode.WritePlayer(Process, Encoding);
                }
            }
            finally
            {

                NativeMethods.ResumeProcess(Process);
            }
        }
        public void RotatePlayer(RotateMethod rotateMethod, Formation targetFormation, bool convertToGk)
        {
            try
            {
                NativeMethods.SuspendProcess(Process);

                var players = ReadPlayers(true);
                if (players.Count == 0)
                {
                    Debug.WriteLine("Current team has no player");
                    return;
                }
                var leftoverPlayers = new PlayerNodeList();
                foreach (var player in players)
                {
                    leftoverPlayers.AddLast(player);
                }

                List<PlayerNode> normals = new List<PlayerNode>(11);
                List<PlayerNode> subs = new List<PlayerNode>(5);
                List<PlayerNode> rest = new List<PlayerNode>(24);

                GetGKs(leftoverPlayers, rotateMethod, normals, subs);
                GetNormals(leftoverPlayers, rotateMethod, normals, targetFormation);
                GetSubs(leftoverPlayers, rotateMethod, normals, subs, targetFormation, convertToGk);
                GetRest(leftoverPlayers, rotateMethod, rest, targetFormation, convertToGk);
                FixPositionAndSaveChangesToGame(normals, subs, rest, convertToGk);
            }
            finally
            {
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
                    PositionRatings.GetPositionRating((int)PlayerPosition.GK, p.Data) * 2).ThenBy(p => this.random.Next());
                    break;
                case RotateMethod.Statistics:
                default:
                    gkQuery = leftoverPlayers.OrderByDescending(p => p.Data.Statistics +
                    PositionRatings.GetPositionRating((int)PlayerPosition.GK, p.Data) * 2).ThenBy(p => this.random.Next());
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
                mainGK.WritePlayerPosition(Process);
            }
            if (backupGK.Data.Position != (int)PlayerPosition.GK)
            {
                backupGK.Data.Position = (int)PlayerPosition.GK;
                backupGK.WritePlayerPosition(Process);
            }

            leftoverPlayers.Remove(mainGK);
            leftoverPlayers.Remove(backupGK);
        }

        private void GetNormals(PlayerNodeList leftoverPlayers, RotateMethod rotateMethod, List<PlayerNode> normals, Formation targetFormation)
        {
            if (targetFormation != null)
            {
                GetNormalsByFormation(leftoverPlayers, rotateMethod, normals, targetFormation);
            }
            else
            {
                GetNormalsByPlayerPreference(leftoverPlayers, rotateMethod, normals);
            }
        }

        private void GetNormalsByPlayerPreference(PlayerNodeList leftoverPlayers, RotateMethod rotateMethod, List<PlayerNode> normals)
        {
            IOrderedEnumerable<PlayerNode> query;
            switch (rotateMethod)
            {
                case RotateMethod.Energy:
                    query = leftoverPlayers.OrderByDescending(
                        p => p.Data.Energy + PositionRatings.GetBestPositionRatingExceptGKInFormation(null, p.Data) * 2)
                        .ThenBy(p => this.random.Next());
                    break;
                case RotateMethod.Statistics:
                default:
                    query = leftoverPlayers.OrderByDescending(
                        p => p.Data.Statistics + PositionRatings.GetBestPositionRatingExceptGKInFormation(null, p.Data) * 2).
                        ThenBy(p => this.random.Next());
                    break;
            }
            //worst player get prefered position first
            //better players are versatle.
            var mainTeam = query.Take(10).OrderBy(p => p.Data.BestPositionRating).ToArray();
            int[] positionLimit = new int[] {
                0,//GK
                1,1,3,//RB, LB, CD,
        1,1,1,3,//RWB, LWB, SW, DM,
        1,1,2,//RM, LM, AM,
        1,1,//RW,LW,
        1,10,1//FR, FOR,SS,Count
            };
            foreach (var mainTeamPlayer in mainTeam)
            {
                var targetPosition = PositionRatings.GetBestPositionWithinLimit(positionLimit, mainTeamPlayer.Data);
                if (mainTeamPlayer.Data.Position != targetPosition)
                {
                    mainTeamPlayer.Data.Position = targetPosition;
                    mainTeamPlayer.WritePlayerPosition(Process);
                }
                normals.Add(mainTeamPlayer);
                leftoverPlayers.Remove(mainTeamPlayer);
                positionLimit[targetPosition] = positionLimit[targetPosition] - 1;
            }
        }

        private void GetNormalsByFormation(PlayerNodeList leftoverPlayers, RotateMethod rotateMethod, List<PlayerNode> normals, Formation targetFormation)
        {
            for (int position = (int)PlayerPosition.Count - 1; position > 0; position--)//choose all position except gk which is chosen
            {
                for (int requiredPlayersInPosition = targetFormation.PlayersInEachPosition[position];
                    requiredPlayersInPosition > 0; requiredPlayersInPosition--)
                {
                    IOrderedEnumerable<PlayerNode> bestFitsQuery;
                    switch (rotateMethod)
                    {
                        case RotateMethod.Energy:
                            bestFitsQuery = leftoverPlayers.OrderByDescending(p => p.Data.Energy + p.Data.GetPositionRatingDouble(position) * 2)
                                .ThenBy(p => PositionRatings.GetAveragePositionRatingInFormationExceptTargetPositionAndGK(
                                    p.Data, position, targetFormation))
                                .ThenBy(p => this.random.Next());
                            break;
                        case RotateMethod.Statistics:
                        default:
                            bestFitsQuery = leftoverPlayers.OrderByDescending(p => p.Data.Statistics +
                                p.Data.GetPositionRatingDouble(position) * 2)
                                .ThenBy(p => PositionRatings.GetAveragePositionRatingInFormationExceptTargetPositionAndGK(
                                    p.Data, position, targetFormation))
                                .ThenBy(p => this.random.Next());
                            break;
                    }
                    var bestFit = bestFitsQuery.First();
                    var targetPosition = position;
                    if (bestFit.Data.Position != targetPosition)
                    {
                        bestFit.Data.Position = targetPosition;
                        bestFit.WritePlayerPosition(Process);
                    }
                    leftoverPlayers.Remove(bestFit);
                    normals.Add(bestFit);
                }
            }
        }

        private void GetSubs(PlayerNodeList leftoverPlayers, RotateMethod rotateMethod,
            List<PlayerNode> normals, List<PlayerNode> subs, Formation targetFormation, bool convertToGk)
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
                GetASub(leftoverPlayers, rotateMethod, subs, targetPositions, convertToGk);
                subNeeded--;
            }
            if (hasWings)
            {
                PlayerPosition[] targetPositions = new PlayerPosition[] {
                    PlayerPosition.RWB,PlayerPosition.LWB,
                    PlayerPosition.RW,PlayerPosition.LW,
                    PlayerPosition.FR,
                };
                GetASub(leftoverPlayers, rotateMethod, subs, targetPositions, convertToGk);
                subNeeded--;
            }
            if (hasMiddleField)
            {
                PlayerPosition[] targetPositions = new PlayerPosition[] {
                    PlayerPosition.RM,PlayerPosition.LM,
                    PlayerPosition.AM,PlayerPosition.DM,
                };
                GetASub(leftoverPlayers, rotateMethod, subs, targetPositions, convertToGk);
                subNeeded--;
            }
            if (hasBackCourt)
            {
                PlayerPosition[] targetPositions = new PlayerPosition[] {
                    PlayerPosition.RB,PlayerPosition.LB,
                    PlayerPosition.SW,PlayerPosition.CD,
                };
                GetASub(leftoverPlayers, rotateMethod, subs, targetPositions, convertToGk);
                subNeeded--;
            }
            if (subNeeded > 0)
            {
                IOrderedEnumerable<PlayerNode> subQuery;
                switch (rotateMethod)
                {
                    case RotateMethod.Energy:
                        subQuery = leftoverPlayers.OrderByDescending(p => p.Data.Energy +
                                PositionRatings.GetBestPositionRatingExceptGKInFormation(targetFormation, p.Data) * 2).ThenBy(p => this.random.Next());
                        break;
                    case RotateMethod.Statistics:
                    default:
                        subQuery = leftoverPlayers.OrderByDescending(p => p.Data.Statistics +
                                PositionRatings.GetBestPositionRatingExceptGKInFormation(targetFormation, p.Data) * 2).ThenBy(p => this.random.Next());
                        break;
                }
                var subRest = subQuery.Take(subNeeded).ToArray();
                foreach (var subTeamPlayer in subRest)
                {
                    int targetPosition;
                    if (targetFormation == null)
                        targetPosition = subTeamPlayer.Data.BestPosition;
                    else
                        targetPosition = PositionRatings.BestFitInFormation(targetFormation, subTeamPlayer.Data);
                    if (convertToGk)
                        targetPosition = (int)PlayerPosition.GK;

                    if (targetPosition != subTeamPlayer.Data.Position)
                    {
                        subTeamPlayer.Data.Position = targetPosition;
                        subTeamPlayer.WritePlayerPosition(Process);
                    }
                    leftoverPlayers.Remove(subTeamPlayer);
                    subs.Add(subTeamPlayer);
                }
            }
        }

        private void GetASub(PlayerNodeList leftoverPlayers, RotateMethod rotateMethod, List<PlayerNode> subs, PlayerPosition[] targetPositions, bool convertToGk)
        {
            IOrderedEnumerable<PlayerNode> subQuery;

            switch (rotateMethod)
            {
                case RotateMethod.Energy:
                    subQuery = leftoverPlayers.OrderByDescending(p => p.Data.Energy +
                            PositionRatings.GetBestPositionRating(targetPositions, p.Data) * 2).ThenBy(p => this.random.Next());
                    break;
                case RotateMethod.Statistics:
                default:
                    subQuery = leftoverPlayers.OrderByDescending(p => p.Data.Statistics +
                           PositionRatings.GetBestPositionRating(targetPositions, p.Data) * 2).ThenBy(p => this.random.Next());
                    break;
            }
            var subTeamPlayer = subQuery.First();
            var targetPosition = PositionRatings.BestFitInPositions(targetPositions, subTeamPlayer.Data);
            if (convertToGk)
                targetPosition = (int)PlayerPosition.GK;
            else if (subTeamPlayer.Data.BestPosition != subTeamPlayer.Data.Position)
            {
                subTeamPlayer.Data.Position = subTeamPlayer.Data.BestPosition;
                subTeamPlayer.WritePlayerPosition(Process);
            }
            leftoverPlayers.Remove(subTeamPlayer);
            subs.Add(subTeamPlayer);
        }

        private void GetRest(PlayerNodeList leftoverPlayers, RotateMethod rotateMethod, List<PlayerNode> rest, Formation targetFormation,
            bool convertToGk)
        {
            foreach (var leftoverPlayer in leftoverPlayers)
            {
                int targetPosition;
                if (targetFormation == null)
                    targetPosition = leftoverPlayer.Data.BestPosition;
                else
                    targetPosition = PositionRatings.BestFitInFormation(targetFormation, leftoverPlayer.Data);
                if (convertToGk)
                    targetPosition = (int)PlayerPosition.GK;
                if (targetPosition != leftoverPlayer.Data.Position)
                {
                    leftoverPlayer.Data.Position = targetPosition;
                    leftoverPlayer.WritePlayerPosition(Process);
                }
                rest.Add(leftoverPlayer);
            }

        }
        private void FixPositionAndSaveChangesToGame(List<PlayerNode> normals, List<PlayerNode> subs, List<PlayerNode> rest, bool convertToGk)
        {
            var newPlayers = new LinkedList<PlayerNode>();
            foreach (var playerNode in normals)
            {
                var player = playerNode.Data;
                if (player.Status != 0)
                {
                    player.Status = 0;
                    playerNode.WritePlayerStatus(Process);
                }
                newPlayers.AddLast(playerNode);
            }
            foreach (var playerNode in subs)
            {
                var player = playerNode.Data;
                if (player.Status != 1)
                {
                    player.Status = 1;
                    playerNode.WritePlayerStatus(Process);
                }
                newPlayers.AddLast(playerNode);
            }
            foreach (var playerNode in rest)
            {
                var player = playerNode.Data;
                if (player.Status != 2)
                {
                    player.Status = 2;
                    playerNode.WritePlayerStatus(Process);
                }
                if (convertToGk)
                    Debug.Assert(player.Position == (int)PlayerPosition.GK);
                newPlayers.AddLast(playerNode);
            }
            if (newPlayers.Count > 0)
            {
                var currentNode = newPlayers.First;
                ushort currentTeam = NativeMethods.ReadUShort(Process, CurrentTeamIndexAddress);
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
                    player.Position = player.BestPosition;
                    playerNode.WritePlayer(Process, Encoding);
                }
            }
            finally
            {
                NativeMethods.ResumeProcess(Process);
            }
        }


        internal void FastUpdate(bool autoTrain, bool convertToGK, bool autoResetStatus,
            bool maxEnergy, bool maxForm, bool maxMorale, bool maxPower, bool noAlternativeTraining
, bool alwaysTrainConsistency)
        {
            try
            {
                var trainingEffects = trainingEffectModifier.TrainingEffects;
                NativeMethods.SuspendProcess(Process);
                var playerNodes = ReadPlayers(true);
                Parallel.ForEach(playerNodes, playerNode =>
                {
                    var player = playerNode.Data;
                    if (autoTrain)
                    {
                        var playerPosition = (PlayerPosition)player.Position;
                        PlayerModelDouble playerDouble = new PlayerModelDouble(player); 
                        if (convertToGK)
                        {
                            if (playerPosition == (int)PlayerPosition.GK)
                            {
                                var BestPlayerPosition = player.BestPosition;
                                playerPosition = (PlayerPosition)BestPlayerPosition;
                            }
                        }
                        TrainingScheduleCalculationState trainingScheduleCalculationState=new TrainingScheduleCalculationState(playerDouble, 
                            convertToGK, playerPosition,
                            autoResetStatus, maxEnergy, maxPower, noAlternativeTraining, alwaysTrainConsistency, trainingEffectModifier, trainingEffects);

                        var playerSchedule = TrainingSchedule.GetTrainingSchedule(trainingScheduleCalculationState);
                        if (playerSchedule == null)
                        {
                            playerSchedule = TrainingSchedule.GenericTraining(trainingScheduleCalculationState, PlayerPosition.Count,null,null);
                        }
                        if (playerSchedule != null)
                        {
                            var playerScheduleBytes = playerSchedule.Select(p => (byte)p.TrainingScheduleType).ToArray();
                            if (player.Fitness < TrainingSchedule.attributeCap)
                            {
                                if (playerNode.Data.Status != 0 && convertToGK)
                                {
                                    playerNode.Data.Position = (byte)PlayerPosition.GK;
                                    playerNode.WritePlayerPosition(Process);
                                }
                            }
                            else
                            {
                                if (player.Status != 0 && convertToGK)
                                {
                                    if (player.Position != (byte)PlayerPosition.GK)
                                    {
                                        player.Position = (byte)PlayerPosition.GK;
                                        playerNode.WritePlayerPosition(Process);
                                    }
                                }
                            }
                            int playerScheduleAddress = TrainingDataAddress +
                                (player.Number - 1) * 116;
                            NativeMethods.WriteBytes(Process, playerScheduleAddress, playerScheduleBytes, 0, 7);
                        }
                    }
                    if (autoResetStatus)
                    {
                        if (player.Status > 2)
                        {
                            player.Status = 2;
                            playerNode.WritePlayerStatus(Process);
                        }
                    }
                    if (maxEnergy || maxForm || maxMorale)
                    {

                        if (maxEnergy)
                        {
                            player.Energy = 99;
                        }
                        if (maxForm)
                        {
                            player.Form = 99;
                        }
                        if (maxMorale)
                        {
                            player.Moral = 99;
                        }
                        playerNode.WritePlayerFormMoralEnergy(Process);
                    }
                });
            }
            finally
            {
                NativeMethods.ResumeProcess(Process);
            }
        }


        internal void SlowUpdate(bool autoRenewContracts, bool maxPower)
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
                            playerNode.WritePlayerContractWeeks(Process);
                        }
                    }
                    if (maxPower)
                    {
                        playerNode.Data.Stamina = 99;
                        playerNode.Data.Strength = 99;
                        playerNode.Data.Fitness = 99;
                        playerNode.WritePlayerStrengths(Process);
                    }
                }
            }
            finally
            {
                NativeMethods.ResumeProcess(Process);
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
                DateTime currentDateTime = PlayerModel.dateOffsetBase.AddDays(currentDate);
                if (currentDateTime.Month < 5 || currentDateTime.Month > 7)
                {
                    throw new InvalidOperationException(Properties.Resources.CanOnlyChangeDateInOffseason);
                }
                DateTime resetDateTime = new DateTime((int)targetYear, currentDateTime.Month, currentDateTime.Day);
                TimeSpan resetTimeSpan = currentDateTime - resetDateTime;
                int daysToSubtract = resetTimeSpan.Days;
                var playerList = this.ReadPlayers(false);
                foreach (var playerNode in playerList)
                {
                    playerNode.Data.BirthDateOffset = (ushort)(playerNode.Data.BirthDateOffset - daysToSubtract);
                    playerNode.WritePlayerBirthDate(Process);
                }
                NativeMethods.WriteInt(Process, DateAddress, currentDate - daysToSubtract);
            }
            finally
            {
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
                        playerNode.WritePlayerPosition(Process);
                    }
                }
                else
                {
                    if (playerNodes.Where(p => p.Data.Status == 0).Count() != 11)
                    {
                        throw new InvalidOperationException(Properties.Resources.NotEnoughPlayersForAutoPosition);
                    }
                    var leftoverPlayers = new PlayerNodeList();

                    foreach (var player in playerNodes.Where(p => p.Data.Status == 0))
                    {
                        leftoverPlayers.AddLast(player);
                    }
                    for (int position = (int)PlayerPosition.Count - 1; position >= 0; position--)
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
                                bestPlayerForPosition.WritePlayerPosition(Process);
                            }
                            leftoverPlayers.Remove(bestPlayerForPosition);
                        }
                    }
                }
            }
            finally
            {
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
        internal List<QueryUpdatePlayerNameResult> UpdatePlayerNames(string respawnCategory)
        {
            List<QueryUpdatePlayerNameResult> newNames = null;
            try
            {
                NativeMethods.SuspendProcess(Process);

                var playerNodes = ReadPlayers(false);

                var playerNodesIndex = new ObjectWithNameCollectionWithIndex<PlayerNode>(playerNodes);

                var newSpawns = playerNodes.Where(p => p.Data.Age < 20 && p.Data.ContractWeeks == 144)
                    .OrderByDescending(p => p.Data.Statistics).ToList();
                if (newSpawns.Count() == 0)
                {
                    throw new InvalidOperationException(Properties.Resources.CannotFindNewSpawn);
                }
                newNames = GetNewPlayerNames(respawnCategory, newSpawns.Count);
                foreach (var newName in newNames)
                {
                    if (newSpawns.Count == 0) break;

                    var namesakePlayers = playerNodesIndex.LookupByName(newName.HumanName.Last, newName.HumanName.First);
                    if (namesakePlayers != null && namesakePlayers.Count > 0)
                    {
                        bool found = false;
                        foreach (var namesakePlayer in namesakePlayers)
                        {
                            if (namesakePlayer.Data.BirthDay != newName.BirthDay)
                            {
                                found = true; break;
                            }
                        }
                        if (!found)
                        {
                            continue;//next newName 
                        }
                        if (newName.BirthDay.HasValue)
                        {
                            int currentDateInt = NativeMethods.ReadInt(Process, DateAddress);
                            DateTime currentDateTime = PlayerModel.dateOffsetBase.AddDays(currentDateInt);
                            namesakePlayers.First.Value.Data.BirthDateOffset = (ushort)(newName.BirthDay.Value - PlayerModel.dateOffsetBase).Days;
                            PlayerNode.UpdateAge(namesakePlayers.First.Value.Data, currentDateTime);
                            namesakePlayers.First.Value.WritePlayerBirthDate(Process);
                        }
                    }

                    var newSpawn = newSpawns.First();
                    newSpawn.Data.FirstName = newName.HumanName.First;
                    newSpawn.Data.LastName = newName.HumanName.Last;
                    newSpawn.Data.ContractWeeks = 143;//skip in the next update
                    newSpawn.WritePlayerNames(Process, Encoding);
                    newSpawn.WritePlayerContractWeeks(Process);
                    /*
                    if (!playerDictionary.ContainsKey(newName.HumanName.Last))
                    {
                        playerDictionary.Add(newName.HumanName.Last, new Dictionary<string, Player>());
                    }
                    if (!playerDictionary[newName.HumanName.Last].ContainsKey(newName.First))
                    {
                        playerDictionary[newName.HumanName.Last].Add(newName.HumanName.First, newSpawn.HumanName.Data);
                    }*/
                    newSpawns.Remove(newSpawn);
                }
                return newNames;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error updating player names: " + ex.Message);
                throw;
            }
            finally
            {
                NativeMethods.ResumeProcess(Process);
            }
            return newNames;
        }


        List<QueryUpdatePlayerNameResult> GetNewPlayerNames(string respawnCategory, int resultLimit)
        {
            List<QueryUpdatePlayerNameResult> result = new List<QueryUpdatePlayerNameResult>(resultLimit);
            int currentDateInt = NativeMethods.ReadInt(Process, DateAddress);
            DateTime currentDateTime = PlayerModel.dateOffsetBase.AddDays(currentDateInt);
            var birthYear = currentDateTime.AddYears(-17).Year;
            var language = "en";
            List<QueryUpdatePlayerNameResult> downloadedNames = null;
            switch (Encoding.CodePage)
            {
                case 936:
                    language = "zh";
                    downloadedNames = GetNewPlayerNamesWikiData(respawnCategory, birthYear, language, resultLimit);
                    break;
                case 437:
                default:
                    language = "en";
                    downloadedNames = GetNewPlayerNamesDbpedia(respawnCategory, birthYear, language, resultLimit);
                    break;
            }

            foreach (var downloadedName in downloadedNames)
            {
                var itemUrl = downloadedName.EntityId;
                var itemLabel_en = downloadedName.EnglishName.RemoveDiacritics();
                var itemLabel_zh = downloadedName.ChineseName;
                if (string.IsNullOrEmpty(itemLabel_zh))
                {
                    HumanName englishHumanName = new HumanName(itemLabel_en);
                    var firstName = englishHumanName.First;
                    var lastName = englishHumanName.Last;

                    if (string.IsNullOrWhiteSpace(lastName))
                    {
                        //this person does not have full name
                        lastName = firstName;
                    }
                    if (firstName.Length > 17)
                        firstName = firstName.Substring(0, 17);
                    if (lastName.Length > 12)
                        lastName = lastName.Substring(0, 12);
                    downloadedName.HumanName = new HumanName(string.Format("{0} {1}", firstName, lastName));
                    downloadedName.BirthDay = DateTime.Parse(downloadedName.BirthDayText);
                    result.Add(downloadedName);
                }
                else
                {
                    itemLabel_zh = ZhConverter.HantToHans(itemLabel_zh);
                    if (itemLabel_en.Contains("("))
                    {
                        itemLabel_en = itemLabel_en.Substring(0, itemLabel_en.IndexOf("(")).Trim();
                    }
                    if (itemLabel_zh.Contains("("))
                    {
                        itemLabel_zh = itemLabel_zh.Substring(0, itemLabel_zh.IndexOf("(")).Trim();
                    }
                    //Chinese wikipedia has u+00B7 as the separator for names
                    itemLabel_zh = itemLabel_zh.Replace("·", " ");
                    HumanName chineseHumanName = new HumanName(itemLabel_zh);
                    HumanName englishHumanName = new HumanName(itemLabel_en);
                    var firstName = englishHumanName.First;
                    if (firstName.Length > 17)
                        firstName = firstName.Substring(0, 17);
                    var lastName = chineseHumanName.Last;
                    if (string.IsNullOrWhiteSpace(chineseHumanName.Last))
                    {
                        //could be CJK name
                        //use full chinese name as last name
                        //use english first name as firs name
                        lastName = chineseHumanName.First;
                    }
                    if (lastName.Length > 12)
                        lastName = lastName.Substring(0, 12);
                    downloadedName.HumanName = new HumanName(string.Format("{0} {1}", firstName, lastName));
                    downloadedName.BirthDay = DateTime.Parse(downloadedName.BirthDayText);
                    result.Add(downloadedName);
                }
            }
            return result;
        }

        string GetNewPlayerNamesWikiDataQuery(string respawnCategory, int birthYear, string language, int resultLimit)
        {
            if (language != "zh") throw new ArgumentException(nameof(language));
            if (string.IsNullOrEmpty(respawnCategory))
            {
                /* SELECT ?player ?birthDate ?itemLabel_en ?itemLabel_zh WHERE {
                  ?player wdt:P31 wd:Q5;                # instance of human
                  wdt:P106 wd:Q937857;          # occupation: association football player
                  wdt:P569 ?birthDate.          # date of birth
                  FILTER(YEAR(?birthDate) = {0})       # born in 1987
                  SERVICE wikibase:label {
                       bd:serviceParam wikibase:language "en".
                      ?player rdfs:label ?itemLabel_en.
                       }
                  SERVICE wikibase:label {
                      bd:serviceParam wikibase:language "zh".
                      ?player rdfs:label ?itemLabel_zh.
                  }
                  FILTER(LANG(?itemLabel_zh) = "zh")
                  }
                  ORDER BY xsd:integer(REPLACE(STR(?player), "http://www.wikidata.org/entity/Q", ""))
                  LIMIT {1}
                 */
                return string.Format(Properties.Resources.WikiDataQueryGetPlayerNameByBirthYear,
                   birthYear, resultLimit);
            }
            else
            {
                /* 
                     SELECT ?item ?itemLabel_en ?itemLabel_zh
                     WHERE {
                      SERVICE wikibase:mwapi {
                        bd:serviceParam wikibase:endpoint "en.wikipedia.org";
                          wikibase:api "Generator";
                          mwapi:generator "categorymembers";
                          mwapi:gcmtitle "Category:{0}";
                          mwapi:gcmprop "ids";
                          mwapi:gcmlimit "{1}".
                        ?item wikibase:apiOutputItem mwapi:item.
                      }
                      ?item wdt:P569 ?birthDate
                      SERVICE wikibase:label {
                        bd:serviceParam wikibase:language "en".
                        ?item rdfs:label ?itemLabel_en.
                      } 
                      SERVICE wikibase:label {
                        bd:serviceParam wikibase:language "zh".
                        ?item rdfs:label ?itemLabel_zh.
                      }     
                       FILTER(YEAR(?birthDate) = {2}) 
                
                       FILTER(lang(?itemLabel_zh) = "zh")
                    }
                    ORDER BY xsd:integer(REPLACE(STR(?item), "http://www.wikidata.org/entity/Q", ""))

                    LIMIT {1}
                 */
                return string.Format(Properties.Resources.WikiDataQueryGetPlayerNameByBirthYearWithinCategory,
                   respawnCategory.Replace('_', ' '),
                   resultLimit, birthYear);


            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="respawnCategory"></param>
        /// <returns></returns>
        List<QueryUpdatePlayerNameResult> GetNewPlayerNamesWikiData(string respawnCategory, int birthYear, string language, int resultLimit)
        {
            List<HumanName> result = new List<HumanName>(resultLimit);
            string query = GetNewPlayerNamesWikiDataQuery(respawnCategory, birthYear, language, resultLimit);
            // Wikidata SPARQL endpoint
            var endpointUri = new Uri("https://query.wikidata.org/sparql");
            using (WebClient client = new WebClient())
            {
                client.Headers.Add("Referer", "https://query.wikidata.org/");
                client.Headers.Add("User-Agent", "github.com/jiangsheng/FSM97Trainer/1.0");
                client.Headers.Add("Accept", "application/sparql-results+json");
                client.Encoding = Encoding.UTF8;
                Debug.WriteLine(query);
                string json = client.DownloadString(string.Format("{0}?query={1}", endpointUri, Uri.EscapeDataString(query)));
                var queryResult = JObject.Parse(json);

                List<QueryUpdatePlayerNameResult> downloadedNames = queryResult.SelectToken("results").SelectToken("bindings")
                    .Select(jt => new QueryUpdatePlayerNameResult
                    {
                        EntityId = jt["newName"]?["value"]?.ToString(),
                        EnglishName = jt["itemLabel_en"]?["value"]?.ToString(),
                        ChineseName = jt["itemLabel_zh"]?["value"]?.ToString(),
                        BirthDayText = jt["birthDate"]?["value"]?.ToString()
                    }).ToList();
                return downloadedNames;
            }
        }
        string GetNewPlayerNamesDbpediaQuery(string respawnCategory, int birthYear, string language, int resultLimit)
        {
            if (string.IsNullOrWhiteSpace(respawnCategory))
            {
                /*
                SELECT DISTINCT ?item ?itemLabel_en WHERE {
                      ?player a dbo:SoccerPlayer ;
                      rdfs:label ?itemLabel;
                      owl:sameAs ?item;
                      dbo:birthDate ?birthDate.
                      FILTER (lang(?itemLabel) = "en")
                    FILTER(YEAR(?birthDate) = 1987)       # born in 1987
                    FILTER(STRSTARTS(STR(?item), "http://www.wikidata.org/entity/Q"))
                    # Extract QID string
                      BIND(STRAFTER(STR(?item), "http://www.wikidata.org/entity/Q") AS ?qid)
                     BIND(STR(?itemLabel) AS ?itemLabel_en)
                    }
                    ORDER BY xsd:integer(?qid)
                    LIMIT 50

                 */
                return string.Format(Properties.Resources.DBPediaGetPlayerByYearQuery, birthYear, resultLimit);
            }
            else
            { /*
                 SELECT DISTINCT * WHERE {
                  ?player a dbo:SoccerPlayer ;
                  rdfs:label ?itemLabel;
                  owl:sameAs ?item;
                  dbo:birthDate ?birthDate;
                dct:subject ?category.
                  FILTER (lang(?itemLabel) = "en")
                FILTER(YEAR(?birthDate) = 1970)       # born in 1987
                FILTER(STRSTARTS(STR(?item), "http://www.wikidata.org/entity/Q"))
                FILTER(STRSTARTS(STR(?category), "http://dbpedia.org/resource/Category:AC_Milan_players"))
                # Extract QID string
                  BIND(STRAFTER(STR(?item), "http://www.wikidata.org/entity/Q") AS ?qid)
                 BIND(STR(?itemLabel) AS ?itemLabel_en)
                }
                ORDER BY xsd:integer(?qid)
                LIMIT 500
               */
                return string.Format(Properties.Resources.DbpediaGetPlayerByYearAndCategoryQuery, birthYear, respawnCategory,
                    resultLimit);

            }

        }
        private List<QueryUpdatePlayerNameResult> GetNewPlayerNamesDbpedia(string respawnCategory, int birthYear, string language, int resultLimit)
        {
            string query = GetNewPlayerNamesDbpediaQuery(respawnCategory, birthYear, language, resultLimit);

            var endpointUri = new Uri("https://dbpedia.org/sparql");
            using (WebClient client = new WebClient())
            {
                client.Headers.Add("Referer", "https://dbpedia.org/sparql");
                client.Headers.Add("User-Agent", "github.com/jiangsheng/FSM97Trainer/1.0");
                client.Headers.Add("Accept", "application/sparql-results+json");
                client.Encoding = Encoding.UTF8;
                Debug.WriteLine(query);
                string json = client.DownloadString(string.Format("{0}?query={1}", endpointUri, Uri.EscapeDataString(query)));
                var queryResult = JObject.Parse(json);

                List<QueryUpdatePlayerNameResult> downloadedNames = queryResult.SelectToken("results").SelectToken("bindings")
                    .Select(jt =>
                    new QueryUpdatePlayerNameResult
                    {
                        EntityId = jt["newName"]?["value"]?.ToString(),
                        EnglishName = jt["itemLabel_en"]?["value"]?.ToString(),
                        BirthDayText = jt["birthDate"]?["value"]?.ToString()
                    }).ToList();
                return downloadedNames;
            }
        }

        internal void PurchaseAllLand()
        {
            if (AssetAddress == 0)
            {
                throw new InvalidOperationException("不支持的游戏版本 (Unsupported Game Version)!");
            }
            try
            {
                NativeMethods.SuspendProcess(Process);
                byte[] bytes = Enumerable.Repeat((byte)1, 21).ToArray();
                for (int i = 0; i < 22; i++)
                {
                    NativeMethods.WriteBytes(Process, AssetAddress + i * 40, bytes, 0, 21);
                }

            }
            finally
            {
                NativeMethods.ResumeProcess(Process);
            }
        }
        public void Restart()
        {
            var mainModulePath = this.Process.MainModule.FileName;
            if (!this.Process.HasExited)
            {
                this.Process.CloseMainWindow(); // Sends a close request
                this.Process.WaitForExit(5000); // Waits for up to 5 seconds
            }
            if (!Process.HasExited)
            {
                Process.Kill();
                Process.WaitForExit();
            }
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = mainModulePath,
                WorkingDirectory = Path.GetDirectoryName(mainModulePath)
            };
            System.Diagnostics.Process.Start(startInfo);
        }
        int evalProgress = 0;
        object evalProgressLock = new object();
        public string EvaluateYoungPlayers(PlayerPosition playerPosition, int maxEvalAge, bool autoResetStatus,
            bool maxEnergy, bool maxPower, bool noAlternativeTraining, Action<int> evaluateYoungPlayersReportProgress, Action<int> evaluateYoungPlayersReportTotalPlayerPositions,
             string playerLastname, int minRating,bool alwaysTrainConsistency, bool debugTraining)
        {
            List<PlayerModelDouble> youngPlayers = null;
            float[][] traingEffect = trainingEffectModifier.TrainingEffects;
            try
            {
                NativeMethods.SuspendProcess(Process);
                ushort currentTeam = NativeMethods.ReadUShort(Process, CurrentTeamIndexAddress);
                var playerNodes = ReadPlayers(false);

                var youngPlayersQuery = playerNodes.Where(p => p.Data.Age <= maxEvalAge);
                if (!string.IsNullOrEmpty(playerLastname))
                {
                    youngPlayersQuery = youngPlayersQuery.Where(p => p.Data.LastName == playerLastname);
                }
                youngPlayers = youngPlayersQuery.Select(node =>new PlayerModelDouble(node.Data)).ToList();

            }
            catch
            {
                return Properties.Resources.FailedToReadPlayersForTheMoment;
            }
            finally
            {
                NativeMethods.ResumeProcess(Process);
            }
            if (youngPlayers == null || youngPlayers.Count == 0)
            {
                return Properties.Resources.YouthPlayerNotFound;
            }

            Dictionary<PlayerPosition, string> targetPositions = new Dictionary<PlayerPosition, string>();
            if (playerPosition == PlayerPosition.Count)
            {
                targetPositions.Add(PlayerPosition.FOR, "FOR/SS");
                targetPositions.Add(PlayerPosition.FR, "FR");
                targetPositions.Add(PlayerPosition.RW, "LW/RW");
                targetPositions.Add(PlayerPosition.RM, "LM/RM/AM");
                targetPositions.Add(PlayerPosition.DM, "DM");
                targetPositions.Add(PlayerPosition.RWB, "LWB/RWB");
                targetPositions.Add(PlayerPosition.CD, "CD");
                targetPositions.Add(PlayerPosition.RB, "LB/RB");
                targetPositions.Add(PlayerPosition.SW, "SW");
                targetPositions.Add(PlayerPosition.GK, "GK");
            }
            else
            {
                targetPositions.Add(playerPosition, Enum.GetName(typeof(PlayerPosition), playerPosition));
            }

            var targetPositionValues = targetPositions.Keys.ToList();
            var targetPositionValueIndexes = Enumerable.Range(0, targetPositionValues.Count).ToList();

            EvaluateYoungPlayersResult[] evaluateYoungPlayersResults = new EvaluateYoungPlayersResult[targetPositionValues.Count];
            evaluateYoungPlayersReportTotalPlayerPositions(targetPositionValueIndexes.Count * youngPlayers.Count);
            if (debugTraining)
            {
                foreach (var targetPositionValueIndex in targetPositionValueIndexes)
                {
                    var position = targetPositionValues[targetPositionValueIndex];
                    EvaluateYoungPlayersResult evaluateYoungPlayersResult = new EvaluateYoungPlayersResult(position, youngPlayers,
                    autoResetStatus,
                    maxEnergy, maxPower, noAlternativeTraining,
                    trainingEffectModifier, alwaysTrainConsistency, traingEffect);
                    evaluateYoungPlayersResults[targetPositionValueIndex] =
                    evaluateYoungPlayersResult;
                    evaluateYoungPlayersResult.OnEvalPlayerPositionComplete += (s, e) =>
                    {
                        lock (evalProgressLock)
                        {
                            evalProgress++;
                            evaluateYoungPlayersReportProgress(evalProgress);
                        }
                    };
                    evaluateYoungPlayersResults[targetPositionValueIndex].Evaluate(minRating);
                }
            }
            else
            {
                Parallel.ForEach(targetPositionValueIndexes, targetPositionValueIndex =>
                {
                    var position = targetPositionValues[targetPositionValueIndex];
                    EvaluateYoungPlayersResult evaluateYoungPlayersResult = new EvaluateYoungPlayersResult(position, youngPlayers,
                    autoResetStatus,
                    maxEnergy, maxPower, noAlternativeTraining,
                    trainingEffectModifier, alwaysTrainConsistency, traingEffect);
                    evaluateYoungPlayersResults[targetPositionValueIndex] =
                    evaluateYoungPlayersResult;
                    evaluateYoungPlayersResult.OnEvalPlayerPositionComplete += (s, e) =>
                    {
                        lock (evalProgressLock)
                        {
                            evalProgress++;
                            evaluateYoungPlayersReportProgress(evalProgress);
                        }
                    };
                    evaluateYoungPlayersResults[targetPositionValueIndex].Evaluate(minRating);
                });
            }
            return GenerateHtmlOutput(targetPositions, targetPositionValues, evaluateYoungPlayersResults);
        }

        private static string GenerateHtmlOutput(Dictionary<PlayerPosition, string> targetPositions, List<PlayerPosition> targetPositionValues, EvaluateYoungPlayersResult[] evaluateYoungPlayersResults)
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            var documentNode = HtmlNode.CreateNode("<!DOCTYPE html><html><head><style>table {\r\n    border-collapse: collapse; /* Prevents double borders or gaps */\r\n  }\r\n  th, td {\r\n    padding: 3px;\r\n  }\r\n  /* Apply a vertical border to the right side of targeted cells */\r\n  .col-border {\r\n    border-right: 1px solid black;\r\n  }</style></head><body></body></html>");
            doc.DocumentNode.AppendChild(documentNode);
            var bodyNode = doc.DocumentNode.SelectSingleNode("//body");
            var rankingsNode = doc.CreateElement("div");
            rankingsNode.Attributes.Add("Id", "rankings");

            var detailsNode = doc.CreateElement("div");
            detailsNode.Attributes.Add("Id", "details");

            for (int targetPositionIndex = 0; targetPositionIndex < targetPositionValues.Count; targetPositionIndex++)
            {
                var targetPositionValue = targetPositionValues[targetPositionIndex];
                var targetPositionName = targetPositions[targetPositionValue];
                var resultForPosition = evaluateYoungPlayersResults[targetPositionIndex];
                var topPlayers = resultForPosition.Grades
                    .Where(r => r.WeeksToMax > 0)
                    .OrderBy(p => p.WeeksToMax)
                    .ThenByDescending(p => p.FinalRating)
                    .ThenByDescending(p => p.Player.Statistics)
                    .Take(20).ToList();
                if (topPlayers.Count == 0) continue;
                var averageWeeks = resultForPosition.Grades
                        .Where(r => r.WeeksToMax > 0)
                        .OrderByDescending(p => p.FinalRating)
                        .ThenBy(p => p.WeeksToMax)
                        .ThenByDescending(p => p.Player.Statistics)
                        .Average(p => p.WeeksToMax);
                var evalResultTable = doc.CreateElement("table");
                var caption = doc.CreateElement("caption");
                var textNode = doc.CreateTextNode(string.Format(Properties.Resources.EvalTopPlayersHeader, targetPositionName));
                caption.AppendChild(textNode);
                evalResultTable.AppendChild(caption);
                var thead = doc.CreateElement("thead");
                var headerRow = doc.CreateElement("tr");
                var headers = new string[] { Properties.Resources.LastName, Properties.Resources.FirstName, Properties.Resources.Age, Properties.Resources.PositionRating, Properties.Resources.Position, Properties.Resources.Nationality, Properties.Resources.WeeksToMax };
                foreach (var header in headers)
                {
                    var th = doc.CreateElement("th");
                    th.AppendChild(doc.CreateTextNode(header));
                    headerRow.AppendChild(th);
                }
                thead.AppendChild(headerRow);
                evalResultTable.AppendChild(thead);
                var tbody = doc.CreateElement("tbody");

                foreach (var topPlayer in topPlayers)
                {
                    var player = topPlayer.Player;
                    var row = doc.CreateElement("tr");
                    var cellLastName = doc.CreateElement("td");
                    cellLastName.AppendChild(doc.CreateTextNode(player.LastName));
                    row.AppendChild(cellLastName);
                    var cellFirstName = doc.CreateElement("td");
                    cellFirstName.AppendChild(doc.CreateTextNode(player.FirstName));
                    row.AppendChild(cellFirstName);
                    var cellAge = doc.CreateElement("td");
                    cellAge.AppendChild(doc.CreateTextNode(player.Age.ToString()));
                    row.AppendChild(cellAge);
                    var cellPositionRating = doc.CreateElement("td");
                    cellPositionRating.AppendChild(doc.CreateTextNode(player.PositionRating.ToString()));
                    row.AppendChild(cellPositionRating);
                    var cellPositionName = doc.CreateElement("td");
                    cellPositionName.AppendChild(doc.CreateTextNode(player.PositionName));
                    row.AppendChild(cellPositionName);
                    var cellNationality = doc.CreateElement("td");
                    if (!string.IsNullOrWhiteSpace(player.NationalityName))
                        cellNationality.AppendChild(doc.CreateTextNode(player.NationalityName));
                    row.AppendChild(cellNationality);
                    var cellWeeksToMax = doc.CreateElement("td");
                    cellWeeksToMax.AppendChild(doc.CreateTextNode(topPlayer.WeeksToMax.ToString()));
                    row.AppendChild(cellWeeksToMax);
                    tbody.AppendChild(row);
                }
                evalResultTable.AppendChild(tbody);


                var tfoot = doc.CreateElement("tfoot");
                tfoot.Attributes.Add("style", "text-align: center;");
                var footerRow = doc.CreateElement("tr");
                var footerCell = doc.CreateElement("td");
                footerCell.SetAttributeValue("colspan", headers.Length.ToString());
                footerCell.AppendChild(doc.CreateTextNode(string.Format(Properties.Resources.AverageWeeks, averageWeeks)));
                footerRow.AppendChild(footerCell);
                tfoot.AppendChild(footerRow);
                evalResultTable.AppendChild(tfoot);

                rankingsNode.AppendChild(evalResultTable);

                foreach (var topPlayer in topPlayers)
                {
                    var player = topPlayer.Player;
                    var scheduleTable = doc.CreateElement("table");
                    var scheduleCaption = doc.CreateElement("caption");
                    scheduleCaption.AppendChild(doc.CreateTextNode(string.Format(Properties.Resources.EvalTopPlayerEntry,
                        player.LastName, player.FirstName,
                        player.Age,
                        player.PositionRating,
                        player.PositionName,
                        player.NationalityName,
                        topPlayer.WeeksToMax
                        )));
                    scheduleTable.AppendChild(scheduleCaption);

                    var scheduleThead = doc.CreateElement("thead");
                    var scheduleHeaderRow = doc.CreateElement("tr");
                    var scheduleHeaders = new string[] { PlayerAttribute.Speed.ToLocalizedString(),
                                PlayerAttribute.Agility.ToLocalizedString(),
                                PlayerAttribute.Acceleration.ToLocalizedString(),
                                PlayerAttribute.Stamina.ToLocalizedString(),
                                PlayerAttribute.Strength.ToLocalizedString(),
                                PlayerAttribute.Fitness.ToLocalizedString(),
                                PlayerAttribute.Shooting.ToLocalizedString(),
                                PlayerAttribute.Passing.ToLocalizedString(),
                                PlayerAttribute.Heading.ToLocalizedString(),
                                PlayerAttribute.Control.ToLocalizedString(),
                                PlayerAttribute.Dribbling.ToLocalizedString(),
                                PlayerAttribute.TackleDetermination.ToLocalizedString(),
                                PlayerAttribute.TackleSkill.ToLocalizedString(),
                                PlayerAttribute.Coolness.ToLocalizedString(),
                                PlayerAttribute.Awareness.ToLocalizedString(),
                                PlayerAttribute.Flair.ToLocalizedString(),
                                PlayerAttribute.Kicking.ToLocalizedString(),
                                PlayerAttribute.Throwing.ToLocalizedString(),
                                PlayerAttribute.Handling.ToLocalizedString(),
                                PlayerAttribute.Leadership.ToLocalizedString(),
                                PlayerAttribute.Consistency.ToLocalizedString(),
                                PlayerAttribute.Determination.ToLocalizedString(),
                                Properties.Resources.BottleneckAttributes,
                                Properties.Resources.TrainingSchedule,
                                Properties.Resources.WeeksCount
                        };
                    var columnWithBorders = new Dictionary<int,int>();
                    columnWithBorders.Add(2, 2);
                    columnWithBorders.Add(5, 5);
                    columnWithBorders.Add(10, 10);
                    columnWithBorders.Add(12, 12);
                    columnWithBorders.Add(15, 15);
                    columnWithBorders.Add(18, 18);
                    columnWithBorders.Add(21, 21);
                    columnWithBorders.Add(22, 22);

                    int columnIndex = 0;
                    foreach (var scheduleHeader in scheduleHeaders)
                    {
                        if (scheduleHeader != null)
                        {
                            var th = doc.CreateElement("th");
                            th.AppendChild(doc.CreateTextNode(scheduleHeader));
                            if (columnWithBorders.ContainsKey(columnIndex))
                                th.AddClass("col-border");
                            scheduleHeaderRow.AppendChild(th);
                            columnIndex++;
                        }
                    }
                    scheduleThead.AppendChild(scheduleHeaderRow);
                    scheduleTable.AppendChild(scheduleThead);
                    var scheduleTbody = doc.CreateElement("tbody");

                    int[] roundsForEachTrainingScheduleType = new int[(int)TrainingScheduleType.Count];
                    foreach (var weeklyTrainingSchedule in topPlayer.WeeklyTrainingSchedules)
                    {
                        var scheduleRow = doc.CreateElement("tr");
                        var schedulePlayer = weeklyTrainingSchedule.Player;
                        columnIndex = 0;
                        for (int i = 0; i < (int)PlayerAttribute.Count; i++)
                        {
                            var cell = doc.CreateElement("td");
                            double attributeValue;
                            switch ((PlayerAttribute)i)
                            {
                                default:
                                    attributeValue = schedulePlayer.Attributes[i]; break;
                                case PlayerAttribute.Coolness:
                                    attributeValue = schedulePlayer.TackleDetermination; break;
                                case PlayerAttribute.Awareness:
                                    attributeValue = schedulePlayer.TackleSkill; break;

                                case PlayerAttribute.TackleDetermination:
                                    attributeValue = schedulePlayer.Coolness; break;
                                case PlayerAttribute.TackleSkill:
                                    attributeValue = schedulePlayer.Awareness; break;
                            }
                            switch ((PlayerAttribute)i)
                            {
                                case PlayerAttribute.ThrowIn:
                                case PlayerAttribute.Greed:
                                    break;
                                default:
                                    if (attributeValue < TrainingSchedule.attributeCap)
                                    {
                                        cell.AppendChild(doc.CreateTextNode(attributeValue.ToStringTruncated(2)));
                                    }
                                    if (columnWithBorders.ContainsKey(columnIndex))
                                        cell.AddClass("col-border");
                                    scheduleRow.AppendChild(cell);
                                    columnIndex++;
                                    break;

                            }
                            
                        }

                        var bottleneckAttributesCell = doc.CreateElement("td");
                        if (weeklyTrainingSchedule.BottleneckAttributes != null)
                        {
                            bool isFirstBottleneckAttribute = true;
                            StringBuilder stringBuilderbottleneckAttributesCellText = new StringBuilder();
                            foreach (var bottleneckAttribute in weeklyTrainingSchedule.BottleneckAttributes)
                            {
                                if (isFirstBottleneckAttribute)
                                    isFirstBottleneckAttribute = false;
                                else
                                    stringBuilderbottleneckAttributesCellText.Append(", ");
                                stringBuilderbottleneckAttributesCellText.Append(bottleneckAttribute.AttributeIndex.ToLocalizedString());
                                if (bottleneckAttribute.Repeat > 0)
                                {
                                    stringBuilderbottleneckAttributesCellText.Append("(");
                                    stringBuilderbottleneckAttributesCellText.Append(bottleneckAttribute.Repeat.ToString());
                                    stringBuilderbottleneckAttributesCellText.Append(")");
                                }
                            }
                            if (stringBuilderbottleneckAttributesCellText.Length > 0)
                            {
                                bottleneckAttributesCell.AppendChild(doc.CreateTextNode(stringBuilderbottleneckAttributesCellText.ToString()));
                            }
                        }
                        bottleneckAttributesCell.AddClass("col-border");
                        scheduleRow.AppendChild(bottleneckAttributesCell);
                        var trainingScheduleCell = doc.CreateElement("td");
                        if (weeklyTrainingSchedule.Steps != null)
                        {
                            bool firstTrainingScheduleCell = true;
                            StringBuilder trainingScheduleCellText = new StringBuilder();
                            foreach (var training in weeklyTrainingSchedule.Steps)
                            {
                                if (firstTrainingScheduleCell)
                                    firstTrainingScheduleCell = false;
                                else
                                    trainingScheduleCellText.Append(", ");
                                trainingScheduleCellText.Append(training.ToString());
                                roundsForEachTrainingScheduleType[(int)training.TrainingScheduleType] += weeklyTrainingSchedule.Weeks;
                            }
                            trainingScheduleCell.AppendChild(doc.CreateTextNode(trainingScheduleCellText.ToString()));
                        }
                        scheduleRow.AppendChild(trainingScheduleCell);

                        var weeksCountCell = doc.CreateElement("td");
                        if (weeklyTrainingSchedule.Weeks > 0)
                        {
                            weeksCountCell.AppendChild(doc.CreateTextNode(weeklyTrainingSchedule.Weeks.ToString()));
                        }
                        scheduleRow.AppendChild(weeksCountCell);
                        scheduleTbody.AppendChild(scheduleRow);
                    }
                    scheduleTable.AppendChild(scheduleTbody);
                    var scheduleFoot = doc.CreateElement("tfoot");
                    scheduleFoot.Attributes.Add("style", "text-align: center;");

                    var scheduleFootRow = doc.CreateElement("tr");
                    var scheduleFootCell = doc.CreateElement("td");
                    scheduleFootCell.SetAttributeValue("colspan", scheduleHeaders.Length.ToString());
                    StringBuilder scheduleFootCellText = new StringBuilder();
                    bool firstScheduleFootCellText = true;
                    scheduleFootCellText.Append(Properties.Resources.TotalRoundsForEachTrainingScheduleType);
                    for (int i = 0; i < (int)TrainingScheduleType.Count; i++)
                    {
                        if (roundsForEachTrainingScheduleType[i] > 0)
                        {
                            if (firstScheduleFootCellText)
                                firstScheduleFootCellText = false;
                            else
                                scheduleFootCellText.Append(", ");
                            scheduleFootCellText.Append(((TrainingScheduleType)i).ToLocalizedString());
                            scheduleFootCellText.Append(": ");
                            scheduleFootCellText.Append(roundsForEachTrainingScheduleType[i].ToString());
                        }
                    }
                    scheduleFootCell.AppendChild(doc.CreateTextNode(scheduleFootCellText.ToString()));
                    scheduleFootRow.AppendChild(scheduleFootCell);
                    scheduleFoot.AppendChild(scheduleFootRow);
                    scheduleTable.AppendChild(scheduleFoot);
                    detailsNode.AppendChild(scheduleTable);
                }
            }
            bodyNode.AppendChild(rankingsNode);
            bodyNode.AppendChild(detailsNode);

            return doc.DocumentNode.OuterHtml;
        }

        public string EvaluateYoungPlayers(PlayerPosition playerPosition, int maxEvalAge, bool autoResetStatus, bool maxEnergy, bool maxPower, bool noAlternativeTraining, Action<int> evaluateYoungPlayersReportProgress, Action<int> evaluateYoungPlayersReportTotalPlayerPositions, bool alwaysTrainConsistency, PlayerModelDouble player, bool debugTraining)
        {
            float[][] traingEffect = trainingEffectModifier.TrainingEffects;
            Dictionary<PlayerPosition, string> targetPositions = new Dictionary<PlayerPosition, string>();
            if (playerPosition == PlayerPosition.Count)
            {
                targetPositions.Add(PlayerPosition.FOR, "FOR/SS");
                targetPositions.Add(PlayerPosition.FR, "FR");
                targetPositions.Add(PlayerPosition.RW, "LW/RW");
                targetPositions.Add(PlayerPosition.RM, "LM/RM/AM");
                targetPositions.Add(PlayerPosition.DM, "DM");
                targetPositions.Add(PlayerPosition.RWB, "LWB/RWB");
                targetPositions.Add(PlayerPosition.CD, "CD");
                targetPositions.Add(PlayerPosition.RB, "LB/RB");
                targetPositions.Add(PlayerPosition.SW, "SW");
                targetPositions.Add(PlayerPosition.GK, "GK");
            }
            else
            {
                targetPositions.Add(playerPosition, Enum.GetName(typeof(PlayerPosition), playerPosition));
            }
            var targetPositionValues = targetPositions.Keys.ToList();
            var targetPositionValueIndexes = Enumerable.Range(0, targetPositionValues.Count).ToList();

            EvaluateYoungPlayersResult[] evaluateYoungPlayersResults = new EvaluateYoungPlayersResult[targetPositionValues.Count];
            evaluateYoungPlayersReportTotalPlayerPositions(targetPositionValueIndexes.Count);
            if (debugTraining)
            {
                foreach (var targetPositionValueIndex in targetPositionValueIndexes)
                {
                    var position = targetPositionValues[targetPositionValueIndex];
                    EvaluateYoungPlayersResult evaluateYoungPlayersResult = new EvaluateYoungPlayersResult(position, new List<PlayerModelDouble> { player },
                    autoResetStatus,
                    maxEnergy, maxPower, noAlternativeTraining,
                    trainingEffectModifier, alwaysTrainConsistency, traingEffect);
                    evaluateYoungPlayersResults[targetPositionValueIndex] =
                    evaluateYoungPlayersResult;
                    evaluateYoungPlayersResult.OnEvalPlayerPositionComplete += (s, e) =>
                    {
                        lock (evalProgressLock)
                        {
                            evalProgress++;
                            evaluateYoungPlayersReportProgress(evalProgress);
                        }
                    };
                    evaluateYoungPlayersResults[targetPositionValueIndex].Evaluate(0);
                }
            }
            else
            {

                Parallel.ForEach(targetPositionValueIndexes, targetPositionValueIndex =>
                {
                    var position = targetPositionValues[targetPositionValueIndex];
                    EvaluateYoungPlayersResult evaluateYoungPlayersResult = new EvaluateYoungPlayersResult(position, new List<PlayerModelDouble> { player },
                    autoResetStatus,
                    maxEnergy, maxPower, noAlternativeTraining,
                    trainingEffectModifier, alwaysTrainConsistency, traingEffect);
                    evaluateYoungPlayersResults[targetPositionValueIndex] =
                    evaluateYoungPlayersResult;
                    evaluateYoungPlayersResult.OnEvalPlayerPositionComplete += (s, e) =>
                    {
                        lock (evalProgressLock)
                        {
                            evalProgress++;
                            evaluateYoungPlayersReportProgress(evalProgress);
                        }
                    };
                    evaluateYoungPlayersResults[targetPositionValueIndex].Evaluate(0);
                });
            }
            return GenerateHtmlOutput(targetPositions, targetPositionValues, evaluateYoungPlayersResults);
        }
    }
}
 
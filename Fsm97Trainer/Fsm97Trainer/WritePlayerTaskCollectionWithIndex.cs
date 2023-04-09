using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Fsm97Trainer
{
    public class WritePlayerTaskCollectionWithIndex : Dictionary<string,
        Dictionary<string, List<WritePlayerTask>>>
    {
        public List<WritePlayerTask> LookupByName(string lastName, string fistName)
        {
            var result = new List<WritePlayerTask>();
            if (!this.ContainsKey(lastName)) return result;
            if (!this[lastName].ContainsKey(fistName)) return result;
            return this[lastName][fistName];
        }
        public List<WritePlayerTask> LookupByLastName(string lastName)
        {
            var result = new List<WritePlayerTask>();
            if (!this.ContainsKey(lastName)) return result;
            foreach (var item in this[lastName])
            {
                result.AddRange(item.Value);
            }
            return result;
        }
        public WritePlayerTaskCollectionWithIndex(PlayerCollectionWithIndex from, PlayerCollectionWithIndex to)
        {
            var respawnFromPlayers = new List<Player>();
            var respawnToPlayers = new List<Player>();
            foreach (var item in to)
            {
                var toLastNameValues = item.Value.Values.ToList();
                var respawnFromPlayersWithSameLastName = from.LookupByLastName(item.Key);
                var respawnToPlayersWithSameLastName = to.LookupByLastName(item.Key);
                foreach (var toPlayers in toLastNameValues)
                {
                    foreach (var toPlayer in toPlayers)
                    {
                        var fromPlayers = from.LookupByName(toPlayer.LastName, toPlayer.FirstName);
                        if (fromPlayers != null)
                        {
                            foreach (var fromPlayer in fromPlayers)
                            {
                                if (Player.CompareAttributes(fromPlayer, toPlayer) == 0)
                                {
                                    Debug.WriteLine(String.Format("Matched {0} with {1}", fromPlayer, toPlayer));
                                    Add(fromPlayer, toPlayer, WritePlayerTaskAction.None);
                                }
                                else if (toPlayer.Greed == fromPlayer.Greed && toPlayer.ThrowIn == fromPlayer.ThrowIn
                                && toPlayer.Leadership == fromPlayer.Leadership)
                                {
                                    Debug.WriteLine(String.Format("Matched {0} with {1}", fromPlayer, toPlayer));
                                    Add(fromPlayer, toPlayer, WritePlayerTaskAction.Update);
                                }

                                respawnFromPlayersWithSameLastName.RemoveAll(p => p.FirstName == fromPlayer.FirstName
                                    && p.Greed == fromPlayer.Greed && p.ThrowIn == fromPlayer.ThrowIn
                                    && p.Leadership == fromPlayer.Leadership);
                                respawnToPlayersWithSameLastName.RemoveAll(p => p.FirstName == fromPlayer.FirstName
                                    && p.Greed == fromPlayer.Greed && p.ThrowIn == fromPlayer.ThrowIn
                                    && p.Leadership == fromPlayer.Leadership);
                            }
                        }
                    }
                }
                respawnFromPlayersWithSameLastName = respawnFromPlayersWithSameLastName.OrderBy(p => p.Greed + p.Leadership + p.ThrowIn).ToList();
                respawnToPlayersWithSameLastName = respawnToPlayersWithSameLastName.OrderByDescending(p => p.Greed + p.Leadership + p.ThrowIn).ToList();
                int i = 0;
                for (; i < respawnFromPlayersWithSameLastName.Count && i < respawnToPlayersWithSameLastName.Count; i++)
                {
                    var fromPlayer = respawnFromPlayersWithSameLastName[i];
                    var toPlayer = respawnToPlayersWithSameLastName[i];
                    Debug.WriteLine(String.Format("Matched {0} with {1}", fromPlayer, toPlayer));
                    Add(fromPlayer, toPlayer, WritePlayerTaskAction.Respawn);
                }
                for (int j = i; j < respawnFromPlayersWithSameLastName.Count; j++)
                {
                    Debug.WriteLine(String.Format("From Player {0} cannot be matched with players with the same last name", respawnFromPlayersWithSameLastName[j]));
                    respawnFromPlayers.Add(respawnFromPlayersWithSameLastName[j]);
                }
                for (int j = i; j < respawnToPlayersWithSameLastName.Count; j++)
                {
                    Debug.WriteLine(String.Format("To Player {0} cannot be matched with players with the same last name", respawnToPlayersWithSameLastName[j]));
                    respawnToPlayers.Add(respawnToPlayersWithSameLastName[j]);
                }
            }

            for (int i = 0; i < respawnFromPlayers.Count && i < respawnToPlayers.Count; i++)
            {
                var fromPlayer = respawnFromPlayers[i];
                var toPlayer = respawnToPlayers[i];
                Debug.WriteLine(String.Format("Randomly Matched {0} with {1}", fromPlayer, toPlayer));
                Add(fromPlayer, toPlayer, WritePlayerTaskAction.Respawn);
            }

        }
        protected void Add(Player from, Player to, WritePlayerTaskAction writePlayerTaskAction)
        {
            WritePlayerTask writePlayerTask = new WritePlayerTask { From = from, To = to, WritePlayerTaskAction = writePlayerTaskAction };
            if (!this.ContainsKey(from.LastName))
                base.Add(from.LastName, new Dictionary<string, List<WritePlayerTask>>());
            if (!this[from.LastName].ContainsKey(from.FirstName))
                this[from.LastName].Add(from.FirstName, new List<WritePlayerTask>());
            this[from.LastName][from.FirstName].Add(writePlayerTask);
        }

    }
}

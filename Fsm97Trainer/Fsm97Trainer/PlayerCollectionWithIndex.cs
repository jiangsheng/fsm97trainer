using System.Collections.Generic;

namespace Fsm97Trainer
{
    public class PlayerCollectionWithIndex : Dictionary<string, Dictionary<string, NamesakePlayers>>
    {
        public PlayerCollectionWithIndex(List<Player> players)
        {
            foreach (var player in players)
            {
                if (!this.ContainsKey(player.LastName))
                {
                    this.Add(player.LastName, new Dictionary<string, NamesakePlayers>());
                }
                if (!this[player.LastName].ContainsKey(player.FirstName))
                {
                    this[player.LastName].Add(player.FirstName, new NamesakePlayers());
                }
                this[player.LastName][player.FirstName].Add(player);
            }
        }
        public NamesakePlayers LookupByName(string lastName, string fistName)
        {
            if (!this.ContainsKey(lastName)) return null;
            if (!this[lastName].ContainsKey(fistName)) return null;
            return this[lastName][fistName];
        }
        public List<Player> LookupByLastName(string lastName)
        {
            var result = new List<Player>();
            if (!this.ContainsKey(lastName)) return result;
            foreach (var item in this[lastName])
            {
                result.AddRange(item.Value);
            }
            return result;
        }
    }
}

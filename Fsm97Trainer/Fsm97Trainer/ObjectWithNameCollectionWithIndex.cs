using System;
using System.Collections.Generic;

namespace Fsm97Trainer
{
    public class ObjectWithNameCollectionWithIndex<T> : Dictionary<string, Dictionary<string, LinkedList<T>>>
        where T : IObjectWithPersonName
    {
        public ObjectWithNameCollectionWithIndex()
        {
        }
        public ObjectWithNameCollectionWithIndex(LinkedList<T> dataSource)
        {
            foreach (var player in dataSource)
            {
                AddObjectWithName(player);

            }
        }

        public void AddObjectWithName(T value)
        {
            if (!this.ContainsKey(value.LastName))
            {
                this.Add(value.LastName, new Dictionary<string, LinkedList<T>>());
            }
            if (!this[value.LastName].ContainsKey(value.FirstName))
            {
                this[value.LastName].Add(value.FirstName, new LinkedList<T>());
            }
            this[value.LastName][value.FirstName].AddLast(value);
        }

        public LinkedList<T> LookupByName(string lastName, string fistName)
        {
            if (!this.ContainsKey(lastName)) return null;
            if (!this[lastName].ContainsKey(fistName)) return null;
            return this[lastName][fistName];
        }
        public LinkedList<T> LookupByLastName(string lastName)
        {
            var result = new LinkedList<T>();
            if (!this.ContainsKey(lastName)) return result;
            foreach (var firstName in this[lastName].Keys)
            {
                foreach (var item in this[lastName][firstName])
                {
                    result.AddLast(item);
                }
            }
            return result;
        }
        public void Remove(T value)
        {
            var namesakes = LookupByName(value.LastName, value.FirstName);
            if (namesakes == null) return;
            namesakes.Remove(value);
            if (namesakes.Count == 0)
            {
                this[value.LastName].Remove(value.FirstName);
            }
            if (this[value.LastName].Count == 0)
                this.Remove(value.LastName);
        }
        public LinkedList<T> FlattenValues()
        {
            var result = new LinkedList<T>();
            foreach (var lastName in Keys)
            {
                foreach (var firstName in this[lastName].Keys)
                {
                    foreach (var item in this[lastName][firstName])
                    {
                        result.AddLast(item);
                    }
                }
            }

            return result;
        }
    }
}

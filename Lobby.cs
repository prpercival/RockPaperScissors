using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RockPaperScissors
{
    public class Lobby
    {
        public string Name { get; set; }

        public Guid Id { get; set; }

        private Dictionary<string, User> _users;

        public Dictionary<string, User> Users
        {
            get { return _users; }
            set
            {
                _users = value;
                if(value != null)
                    Size = value.Count;
            }
        }

        private int _size;

        public int Size
        {
            get { return _size; }
            set
            {
                _size = value;
                Capacity = $"{_size}/2";
            }
        }

        public string Capacity { get; private set; }
    }

    public class User
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public Choice? Choice { get; set; }
    }

    public enum Choice
    {
        Rock = 1,
        Paper = 2,
        Scissors = 3,
        Moo = 4
    }
}

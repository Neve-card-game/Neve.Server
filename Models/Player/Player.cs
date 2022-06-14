using System.Collections.Generic;

namespace NeveServer.Models
{
    public class Player : User
    {
        public bool LoginStatus { get; set; }
        public int UsingDeckId { get; set; }

        public List<Decks> PlayerDecks = new List<Decks>();

        public Player() { }

        public Player(User user)
        {
            this.id = user.id;
            this.Email = user.Email;
            this.Username = user.Username;
            this.Password = user.Password;
            this.RegTime = user.RegTime;
            this.LastLogInTime = user.LastLogInTime;
            this.Status = user.Status;
        }
    }
}

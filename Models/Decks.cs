using System.Collections.Generic;

namespace NeveServer.Models
{
    [System.Serializable]
    public class Decks
    {
        public string? DeckName;
        public string? DeckId;
        public string? DeckColor;
        public List<Card>? Deck;
        public int CardsCounts;

        public bool IsDeckOnClick = false;
        public int EditTime = 0;

        public Decks() { }

        public Decks(string deckName, string deckColor)
        {
            DeckName = deckName;
            DeckColor = deckColor;
        }

        public Decks(string deckName, string deckId, string deckColor)
        {
            DeckName = deckName;
            DeckId = deckId;
            DeckColor = deckColor;
        }

        public Decks(
            string deckName,
            string deckId,
            string deckColor,
            List<Card> deck,
            bool isDeckOnClick,
            int editTime
        ) : this(deckName, deckId, deckColor)
        {
            Deck = deck;
            CardsCounts = deck.Count;
            IsDeckOnClick = isDeckOnClick;
            EditTime = editTime;
        }
        public void DecksClear()
        {
            DeckName = null;
            DeckId = null;
            DeckColor = null;
            if(Deck != null)
                Deck.Clear();
            CardsCounts = 0;
            EditTime = 0;

            IsDeckOnClick = false;
        }
    }
}

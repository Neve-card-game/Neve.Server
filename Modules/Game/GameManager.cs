using System;
using System.Collections.Generic;

namespace NeveServer.Models
{
    public class GameManger
    {
        private delegate List<T> addition<T>();

        public GameManger() { }

        public void MatchStart(ref GameState Match)
        {
            Match = GameState.Pre_Match;
        }

        public List<Card> PreMatch(
            ref GameState Match,
            List<Card> deck1,
            List<Card> deck2,
            List<Card> AdditionResource
        )
        {
            List<Card> newDeck = AdditionList<Card>(
                () =>
                {
                    return AdditionList<Card>(deck1, deck2);
                },
                AdditionResource
            );
            Random random = new Random();
            for (int i = newDeck.Count - 1; i >= 0; --i)
            {
                Swap<Card>(ref newDeck, random.Next(0, i + 1), i);
            }
            Match = GameState.Combat_Start;
            return newDeck;
        }

        public List<Card> PreMatch(ref GameState Match, List<Card> deck1, List<Card> deck2)
        {
            List<Card> newDeck = AdditionList<Card>(deck1, deck2);
            Random random = new Random();
            for (int i = newDeck.Count - 1; i >= 0; --i)
            {
                Swap<Card>(ref newDeck, random.Next(0, i + 1), i);
            }
            Match = GameState.Combat_Start;
            return newDeck;
        }

        private void Swap<T>(ref List<T> list, int index1, int index2)
        {
            var temp = list[index1];
            list[index1] = list[index2];
            list[index2] = temp;
        }

        private List<T> AdditionList<T>(List<T> list1, List<T> list2)
        {
            foreach (var t in list2)
            {
                list1.Add(t);
            }
            return list1;
        }

        private List<T> AdditionList<T>(addition<T> list1, List<T> list2)
        {
            List<T> list = list1();
            foreach (var t in list2)
            {
                list.Add(t);
            }
            return list;
        }
    }
}

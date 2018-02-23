using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace whist_card_game
{
    class Deck
    {
        private Card[] deck;
        private int currentCard;
        private const int NUMBER_OF_CARDS = 52;
        private Random ranNum;

        public Deck()
        {
            string[] faces = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "jack", "queen", "king", "ace"};
            string[] suits = { "hearts", "spades", "diamonds", "clubs" };
            int[] points = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 };
            deck = new Card[NUMBER_OF_CARDS];
            currentCard = 0;
            ranNum = new Random();
            for (int count = 0; count < deck.Length; count++)
            {
                string face = faces[count % 13];
                string suite = suits[count / 13];
                string image = face + "_of_" + suite + ".png";
                deck[count] = new Card(face, suite, points[count % 13], image);
            }
        }

        public void Shuffle()
        {
            currentCard = 0;
            for(int first = 0; first < deck.Length; first++)
            {
                int second = ranNum.Next(NUMBER_OF_CARDS);
                Card temp = deck[first];
                deck[first] = deck[second];
                deck[second] = temp;
            }
        }

        public Card DealCard()
        {
            if (currentCard < deck.Length)
                return deck[currentCard++];
            return null;
        }
    }
}

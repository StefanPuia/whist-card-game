using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace whist_card_game
{
    class Card
    {
        private string face;
        private string suit;
        private string image;
        private int points;

        public Card(string cardFace = "", string cardSuit = "", int cardPoints = 0, string cardImage = "")
        {
            face = cardFace;
            suit = cardSuit;
            image = cardImage;
            points = cardPoints;
        }

        public override string ToString()
        {
            return face + "_of_" + suit;
        }

        public string getImage()
        {
            return image;
        }

        public int getPoints(string trump = "")
        {
            return points + (trump.Equals(suit) ? 100 : 0);
        }

        public string getSuit()
        {
            return suit;
        }
    }
}

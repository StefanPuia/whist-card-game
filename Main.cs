using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace whist_card_game
{
    public partial class form_main : Form
    {
        Panel activePanel;
        string[] players = new string[4];
        List<Card>[] hands = new List<Card>[4];
        int currentPlayer = 0;
        List<PictureBox> displayedCards = new List<PictureBox>();
        Card[] playedCards = new Card[4] { new Card("", "", 0, ""), new Card("", "", 0, ""), new Card("", "", 0, ""), new Card("", "", 0, "") };
        int round = 0;
        int[] points = new int[4] { 0, 0, 0, 0 };
        string[] trumps = { "hearts", "spades", "diamonds", "clubs" };
        int currentTrump = 0;
        int match = 0;
        int startingPlayer;
        int[,] trumpPoints = new int[4,2] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };

        public form_main()
        {
            InitializeComponent();
            activePanel = panel_start;
        }

        private void displayPanel(Panel newpanel)
        {
            activePanel.Visible = false;
            newpanel.Visible = true;
            activePanel = newpanel;
        }

        private void button_start_Click(object sender, EventArgs e)
        {
            displayPanel(panel_players);
        }

        private void button_exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button_rules_Click(object sender, EventArgs e)
        {
            displayPanel(panel_rules);
        }

        private void button_players_start_Click(object sender, EventArgs e)
        {
            players[0] = !textBox_players_name_player1.Text.Equals("")?textBox_players_name_player1.Text:"Player1";
            players[1] = !textBox_players_name_player2.Text.Equals("") ? textBox_players_name_player2.Text : "Player2";
            players[2] = !textBox_players_name_player3.Text.Equals("") ? textBox_players_name_player3.Text : "Player3";
            players[3] = !textBox_players_name_player4.Text.Equals("") ? textBox_players_name_player4.Text : "Player4";

            hideInfo();

            label_score_player1.Text = players[0];
            label_score_player2.Text = players[1];
            label_score_player3.Text = players[2];
            label_score_player4.Text = players[3];

            label_score_points_t1.Text = "0";
            label_score_points_t1.Text = "0";

            foreach (Control scorelabel in panel_score.Controls)
            {
                Regex rgx = new Regex(@"label_score_points_t._s.");
                if(rgx.IsMatch(scorelabel.Text))
                {
                    scorelabel.Text = "0";
                }
            }

            Deck deck = new Deck();
            deck.Shuffle();

            for (int i = 0; i < 4; i++)
            {
                hands[i] = new List<Card>();
                for (int j = 0; j < 13; j++)
                    hands[i].Add(deck.DealCard());
            }
            sortHands();

            Random r = new Random();
            currentPlayer = r.Next(3) + 1;
            startingPlayer = nextPlayer(1);

            match = 0;
            currentTrump = r.Next(4);
            pictureBox_game_trump.ImageLocation = "cards/ace_of_" + trumps[currentTrump] + ".png";

            displayedCards = new List<PictureBox>();
            playedCards = new Card[4] { new Card("", "", 0, ""), new Card("", "", 0, ""), new Card("", "", 0, ""), new Card("", "", 0, "") };
            round = 0;
            points = new int[4] { 0, 0, 0, 0 };


            rotate();
            displayPanel(panel_switch);
        }

        private void button_players_back_Click(object sender, EventArgs e)
        {
            displayPanel(panel_start);
        }

        private void button_rules_back_Click(object sender, EventArgs e)
        {
            displayPanel(panel_start);
        }

        private void button_game_back_Click(object sender, EventArgs e)
        {
            timer_handlewin.Stop();
            displayPanel(panel_start);
        }

        private void rotate()
        {
            if (round >= 4)
            {
                renderPlayer("bottom", currentPlayer);
                for (int i = 0; i < displayedCards.Count; i++)
                {
                    panel_game.Controls.Remove(displayedCards[i]);
                }
                int max = 0;
                int maxplayer = -1;
                for (int i = 0; i < 4; i++)
                {
                    if(playedCards[i].getPoints(trumps[currentTrump]) >= max)
                    {
                        max = playedCards[i].getPoints(trumps[currentTrump]);
                        maxplayer = i;
                    }
                }

                points[maxplayer]++;
                renderPlayers();
                currentPlayer = parsePlayer(maxplayer - 1);
                startingPlayer = nextPlayer(1);
                displayInfo(players[maxplayer] + " a castigat runda.");
                timer_handlewin.Start();
            }
            else
            {
                round++;
                currentPlayer = nextPlayer(1);

                displayPanel(panel_switch);
                label_switch.Text = players[currentPlayer] + ", este randul tau! Fa click pe ecran cand esti gata.";

                renderPlayers();
                renderCards();
            }
        }

        private void renderPlayers()
        {
            renderPlayer("bottom", currentPlayer);
            renderPlayer("right", nextPlayer(1));
            renderPlayer("top", nextPlayer(2));
            renderPlayer("left", nextPlayer(3));
        }

        private int nextPlayer(int inc)
        {
            return parsePlayer(currentPlayer + inc);
        }

        private int parsePlayer(int player)
        {
            if (player >= 0 && player <= 3)
                return player;
            if(player < 0)
                return 4 + (player % 4);
            return player % 4;
        }

        private void renderCards()
        {
            for (int i = 0; i < displayedCards.Count; i++)
            {
                panel_game.Controls.Remove(displayedCards[i]);
            }

            int cards = hands[currentPlayer].Count;
            int renderStart = 20;
            int spacing = 0;
            if (cards > 0)
            {
                int renderWidth = cards * 120 + (cards - 1) * 20;
                renderStart = (1180 - renderWidth) / 2;
                if (renderWidth > 1120)
                {
                    renderWidth = 1120;
                    renderStart = 20;
                }

                spacing = renderWidth / cards - 1;
            }

            for (int c = 0; c < cards; c++)
            {
                Card currentCard = hands[currentPlayer][c];
                PictureBox temp = new PictureBox();
                temp.Size = new Size(120, 170);
                temp.Location = new Point(renderStart + c * spacing, 621);
                temp.BackColor = Color.White;
                temp.SizeMode = PictureBoxSizeMode.StretchImage;
                temp.ImageLocation = "cards/" + currentCard.getImage();
                temp.Click += new EventHandler((sender, e) => playCard(temp));

                panel_game.Controls.Add(temp);
                displayedCards.Add(temp);
                temp.BringToFront();
            }
        }

        private void playCard(PictureBox card)
        {
            for(int i = 0; i < hands[currentPlayer].Count; i++)
            {
                string cardFilename = card.ImageLocation.Split('/')[1];
                if (hands[currentPlayer][i].getImage().Equals(cardFilename))
                {
                    if (validCard(hands[currentPlayer][i]) || currentPlayer == startingPlayer)
                    {
                        playedCards[currentPlayer] = hands[currentPlayer][i];
                        hands[currentPlayer].Remove(hands[currentPlayer][i]);
                        hideInfo();
                        rotate();
                    }
                    else
                    {
                        displayInfo("Inca mai ai carti din suita trucului. Trebuie sa le folosesti pe acelea prima data!");
                    }
                }
            }
        }

        private void panel_switch_Click(object sender, EventArgs e)
        {
            displayPanel(panel_game);
        }

        private void label_switch_Click(object sender, EventArgs e)
        {
            displayPanel(panel_game);
        }

        private void renderPlayed(PictureBox card, int player)
        {
            if (!playedCards[player].ToString().Equals("_of_"))
            {
                card.ImageLocation = "cards/" + playedCards[player].getImage();
                card.Visible = true;
            }
            else
            {
                card.Visible = false;
            }
        }

        private void displayInfo(string info)
        {
            label_game_info.Visible = true;
            label_game_info.BringToFront();
            label_game_info.Text = info;
        }

        private void hideInfo()
        {
            label_game_info.Visible = false;
        }

        private void renderPlayer(string side, int player)
        {
            Label name = panel_game.Controls["label_game_player_" + side] as Label;
            name.Text = players[player];

            Label score = panel_game.Controls["label_game_points_" + side] as Label;
            score.Text = points[player].ToString();

            PictureBox card = panel_game.Controls["pictureBox_game_played_" + side] as PictureBox;
            renderPlayed(card, player);
        }

        private int getRoundPoints(int points)
        {
            points = points - 5;
            if (points > 0)
                return points;
            return 0;
        }

        private void timer_handlewin_Tick(object sender, EventArgs e)
        {
            playedCards = new Card[4] { new Card("", "", 0, ""), new Card("", "", 0, ""), new Card("", "", 0, ""), new Card("", "", 0, "") };
            pictureBox_game_played_bottom.Visible = false;
            pictureBox_game_played_top.Visible = false;
            pictureBox_game_played_left.Visible = false;
            pictureBox_game_played_right.Visible = false;
            round = 0;

            hideInfo();
            rotate();
            timer_handlewin.Stop();

            if(hands[startingPlayer].Count <= 0)
            {
                displayPanel(panel_score);

                label_score_suit_1.Text = trumps[currentTrump];
                currentTrump = parsePlayer(currentTrump + 1);

                trumpPoints[match, 0] = getRoundPoints(points[0] + points[2]);
                trumpPoints[match, 1] = getRoundPoints(points[1] + points[3]);
                panel_score.Controls["label_score_points_t1_s" + (match + 1)].Text = trumpPoints[match, 0].ToString();
                panel_score.Controls["label_score_points_t2_s" + (match + 1)].Text = trumpPoints[match, 1].ToString();


                panel_score.Controls["label_score_points_t1"].Text = sumScore(0).ToString();
                panel_score.Controls["label_score_points_t2"].Text = sumScore(1).ToString();

                if(match >= 3)
                {
                    button_score_back.Visible = false;
                    if(sumScore(0) > sumScore(1))
                    {
                        label_score_points_t1.Text = "CASTIGATORI";
                    }
                    else
                    {
                        label_score_points_t2.Text = "CASTIGATORI";
                    }
                }
                else
                {
                    match++;
                }
            }
            else
            {
                displayPanel(panel_switch);
                label_switch.Text = players[currentPlayer] + ", este randul tau! Fa click pe ecran cand esti gata.";
            }
        }

        private int sumScore(int team)
        {
            int sum = 0;
            for (int i = 0; i < 4; i++)
            {
                sum += trumpPoints[i, team];
            }
            return sum;
        }

        private void sortHands()
        {
            for (int i = 0; i < 4; i++)
            {
                List<Card> final = new List<Card>();
                List<Card> hearts = new List<Card>();
                List<Card> spades = new List<Card>();
                List<Card> diamonds = new List<Card>();
                List<Card> clubs = new List<Card>();
                foreach (Card c in hands[i])
                {
                    switch (c.getSuit())
                    {
                        case "hearts":
                            hearts.Add(c);
                            break;

                        case "spades":
                            spades.Add(c);
                            break;

                        case "diamonds":
                            diamonds.Add(c);
                            break;

                        case "clubs":
                            clubs.Add(c);
                            break;
                    }
                }

                for (int j = 0; j < hearts.Count - 1; j++)
                {
                    for (int k = j; k < hearts.Count; k++)
                    {
                        if (hearts[j].getPoints() > hearts[k].getPoints())
                        {
                            Card t = new Card();
                            t = hearts[j];
                            hearts[j] = hearts[k];
                            hearts[k] = t;
                        }
                    }
                }

                for (int j = 0; j < spades.Count - 1; j++)
                {
                    for (int k = j; k < spades.Count; k++)
                    {
                        if (spades[j].getPoints() > spades[k].getPoints())
                        {
                            Card t = new Card();
                            t = spades[j];
                            spades[j] = spades[k];
                            spades[k] = t;
                        }
                    }
                }

                for (int j = 0; j < diamonds.Count - 1; j++)
                {
                    for (int k = j; k < diamonds.Count; k++)
                    {
                        if (diamonds[j].getPoints() > diamonds[k].getPoints())
                        {
                            Card t = new Card();
                            t = diamonds[j];
                            diamonds[j] = diamonds[k];
                            diamonds[k] = t;
                        }
                    }
                }

                for (int j = 0; j < clubs.Count - 1; j++)
                {
                    for (int k = j; k < clubs.Count; k++)
                    {
                        if (clubs[j].getPoints() > clubs[k].getPoints())
                        {
                            Card t = new Card();
                            t = clubs[j];
                            clubs[j] = clubs[k];
                            clubs[k] = t;
                        }
                    }
                }

                final.AddRange(hearts);
                final.AddRange(spades);
                final.AddRange(diamonds);
                final.AddRange(clubs);

                hands[i] = final;
            }
        }

        private bool validCard(Card card)
        {
            Card trick = playedCards[startingPlayer];
            bool foundTrick = false;
            for (int i = 0; i < hands[currentPlayer].Count; i++)
            {
                if(hands[currentPlayer][i].getSuit() == trick.getSuit())
                {
                    foundTrick = true;
                    break;
                }
            }

            if (card.getSuit() != trick.getSuit() && foundTrick)
                return false;
            return true;
        }

        private void button_score_exit_Click(object sender, EventArgs e)
        {
            displayPanel(panel_start);
        }

        private void button_score_back_Click(object sender, EventArgs e)
        {
            displayPanel(panel_switch);
            label_switch.Text = players[currentPlayer] + ", este randul tau! Fa click pe ecran cand esti gata.";

            points = new int[4] { 0, 0, 0, 0 };

            hideInfo();

            Deck deck = new Deck();
            deck.Shuffle();

            for (int i = 0; i < 4; i++)
            {
                hands[i] = new List<Card>();
                for (int j = 0; j < 13; j++)
                    hands[i].Add(deck.DealCard());
            }
            sortHands();

            Random r = new Random();
            currentPlayer = r.Next(3) + 1;
            startingPlayer = nextPlayer(1);

            match++;
            pictureBox_game_trump.ImageLocation = "cards/ace_of_" + trumps[currentTrump] + ".png";

            displayedCards = new List<PictureBox>();
            playedCards = new Card[4] { new Card("", "", 0, ""), new Card("", "", 0, ""), new Card("", "", 0, ""), new Card("", "", 0, "") };

            round = 0;
            points = new int[4] { 0, 0, 0, 0 };

            rotate();
        }
    }
}

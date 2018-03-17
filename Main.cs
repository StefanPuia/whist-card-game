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
        // initializarea variablilelor
        Card[] playedCards = new Card[4] { new Card("", "", 0, ""), new Card("", "", 0, ""), new Card("", "", 0, ""), new Card("", "", 0, "") };
        int[] points = new int[4] { 0, 0, 0, 0 };
        string[] trumps = { "hearts", "spades", "diamonds", "clubs" };
        int[,] trumpPoints = new int[4, 2] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };

        Panel activePanel;
        int startingPlayer;
        int currentPlayer = 0;
        int currentTrump = 0;
        int round = 0;
        int match = 0;

        string[] players = new string[4];
        List<Card>[] hands = new List<Card>[4];
        List<PictureBox> displayedCards = new List<PictureBox>();

        public form_main()
        {
            InitializeComponent();
            activePanel = panel_start;
        }

        // schimba panoul afisat cu cel din parametri
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
            // daca nu au fost introduse numele jucatorilor, foloseste "PlayerX"
            players[0] = !textBox_players_name_player1.Text.Equals("")?textBox_players_name_player1.Text:"Player1";
            players[1] = !textBox_players_name_player2.Text.Equals("") ? textBox_players_name_player2.Text : "Player2";
            players[2] = !textBox_players_name_player3.Text.Equals("") ? textBox_players_name_player3.Text : "Player3";
            players[3] = !textBox_players_name_player4.Text.Equals("") ? textBox_players_name_player4.Text : "Player4";

            hideInfo();

            // adauga numele jucatorilor in fiecare label
            label_score_player1.Text = players[0];
            label_score_player2.Text = players[1];
            label_score_player3.Text = players[2];
            label_score_player4.Text = players[3];

            // reseteaza scorul
            label_score_points_t1.Text = "0";
            label_score_points_t2.Text = "0";
            foreach (Control scorelabel in panel_score.Controls)
            {
                Regex rgx = new Regex(@"label_score_points_t._s.");
                if(rgx.IsMatch(scorelabel.Text))
                {
                    scorelabel.Text = "0";
                }
            }

            // creeaza un pachet si amesteca-l
            Deck deck = new Deck();
            deck.Shuffle();

            // distribuie cartile si sorteaza cartile din maini
            for (int i = 0; i < 4; i++)
            {
                hands[i] = new List<Card>();
                for (int j = 0; j < 13; j++)
                    hands[i].Add(deck.DealCard());
            }
            sortHands();

            // alege un jucator aleatoriu care sa inceapa
            Random r = new Random();
            currentPlayer = r.Next(3) + 1;
            startingPlayer = nextPlayer(1);

            // alege un atu aleatoriu
            match = 0;
            currentTrump = r.Next(4);
            pictureBox_game_trump.ImageLocation = "cards/ace_of_" + trumps[currentTrump] + ".png";

            // reseteaza cartile si punctele
            displayedCards = new List<PictureBox>();
            playedCards = new Card[4] { new Card("", "", 0, ""), new Card("", "", 0, ""), new Card("", "", 0, ""), new Card("", "", 0, "") };
            round = 0;
            points = new int[4] { 0, 0, 0, 0 };

            // incepe jocul
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
            // daca nu e ultima runda
            if (round >= 4)
            {
                // afiseaza cartea jucata de jucatorul de jos
                renderPlayer("bottom", currentPlayer);
                // sterge toate cartile din mana pentru a nu fi apasate in timpul schimbului
                for (int i = 0; i < displayedCards.Count; i++)
                {
                    panel_game.Controls.Remove(displayedCards[i]);
                }

                // determina jucatorul cu scor maxim
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

                // adauga scorul
                points[maxplayer]++;
                // muta jocul la acelasi jucator
                renderPlayers();
                currentPlayer = parsePlayer(maxplayer - 1);
                startingPlayer = nextPlayer(1);
                displayInfo(players[maxplayer] + " a castigat runda.");
                // incepe timer-ul pentru a reseta runda
                timer_handlewin.Start();
            }
            else
            {
                // muta jocul la urmatorul jucator
                round++;
                currentPlayer = nextPlayer(1);

                // afiseaza panoul de schimbare
                displayPanel(panel_switch);
                label_switch.Text = players[currentPlayer] + ", este randul tau! Fa click pe ecran cand esti gata.";

                // afiseaza cartile
                renderPlayers();
                renderCards();
            }
        }

        // afiseaza toate cartile si punctele jucatorilor
        private void renderPlayers()
        {
            renderPlayer("bottom", currentPlayer);
            renderPlayer("right", nextPlayer(1));
            renderPlayer("top", nextPlayer(2));
            renderPlayer("left", nextPlayer(3));
        }

        // creeaza un numar modulo 4 care e mai mare decat jucatorul curent
        private int nextPlayer(int inc)
        {
            return parsePlayer(currentPlayer + inc);
        }

        // creeaza un numar valid intre 0 si 3 in functie de numarul dat
        private int parsePlayer(int player)
        {
            if (player >= 0 && player <= 3)
                return player;
            if(player < 0)
                return 4 + (player % 4);
            return player % 4;
        }

        // afiseaza cartile
        private void renderCards()
        {
            // sterge cartile afisate
            for (int i = 0; i < displayedCards.Count; i++)
            {
                panel_game.Controls.Remove(displayedCards[i]);
            }

            // calculeaza spatiile pentru a pozitiona cartile in mijloc
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

            // afiseaza cartile si adauga fiecareia evenimetul de click 
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

        // functia apelata la click pe cartea din mana
        private void playCard(PictureBox card)
        {
            // incearca toate cartile din mana pentru a fi sigur ca e o carte valida
            for(int i = 0; i < hands[currentPlayer].Count; i++)
            {
                string cardFilename = card.ImageLocation.Split('/')[1];
                if (hands[currentPlayer][i].getImage().Equals(cardFilename))
                {
                    // in cazul in care cartea jucata urmareste regulile
                    if (validCard(hands[currentPlayer][i]) || currentPlayer == startingPlayer)
                    {
                        // joaca acea carte
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

        // afiseaza cartea jucata in dreptul jucatorului respectiv
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

        // afiseaza textul informativ in caseta de text din mijloc
        private void displayInfo(string info)
        {
            label_game_info.Visible = true;
            label_game_info.BringToFront();
            label_game_info.Text = info;
        }

        // ascunde caseta de text
        private void hideInfo()
        {
            label_game_info.Visible = false;
        }

        // afiseaza numele jucatorilor in functie de locatia lor la masa
        private void renderPlayer(string side, int player)
        {
            Label name = panel_game.Controls["label_game_player_" + side] as Label;
            name.Text = players[player];

            Label score = panel_game.Controls["label_game_points_" + side] as Label;
            score.Text = points[player].ToString();

            PictureBox card = panel_game.Controls["pictureBox_game_played_" + side] as PictureBox;
            renderPlayed(card, player);
        }

        // calculeaza excesul de puncte care vor fi utilizate ca punctaj final
        private int getRoundPoints(int points)
        {
            points = points - 5;
            if (points > 0)
                return points;
            return 0;
        }

        // reseteaza trucul cand ultimul jucator a jucat cartea
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

            // daca nu mai sunt carti de jucat afiseaza scorul
            if (hands[startingPlayer].Count <= 0)
            {
                displayPanel(panel_score);

                // afiseaza atuul in tabela de scor
                label_score_suit_1.Text = trumps[currentTrump];
                // schimba atuul
                currentTrump = parsePlayer(currentTrump + 1);

                // afiseaza scorul in tabela
                trumpPoints[match, 0] = getRoundPoints(points[0] + points[2]);
                trumpPoints[match, 1] = getRoundPoints(points[1] + points[3]);
                panel_score.Controls["label_score_points_t1_s" + (match + 1)].Text = trumpPoints[match, 0].ToString();
                panel_score.Controls["label_score_points_t2_s" + (match + 1)].Text = trumpPoints[match, 1].ToString();
                panel_score.Controls["label_score_points_t1"].Text = sumScore(0).ToString();
                panel_score.Controls["label_score_points_t2"].Text = sumScore(1).ToString();

                // daca una dintre echipe a ajuns la 7 puncte, anunta castigatorii
                if (sumScore(0) >= 7)
                {
                    label_score_points_t1.Text = "CASTIGATORI";
                    button_score_back.Visible = false;
                }
                else if (sumScore(1) >= 7)
                {
                    label_score_points_t2.Text = "CASTIGATORI";
                    button_score_back.Visible = false;
                }
                // daca nu, continua jocul
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

        // calculeaza scorul total al echipei
        private int sumScore(int team)
        {
            int sum = 0;
            for (int i = 0; i < 4; i++)
            {
                sum += trumpPoints[i, team];
            }
            return sum;
        }

        // sorteaza mana dupa atu si numar in ordine crescatoare
        private void sortHands()
        {
            // pentru fiecare jucator
            for (int i = 0; i < 4; i++)
            {
                List<Card> final = new List<Card>();
                List<Card> hearts = new List<Card>();
                List<Card> spades = new List<Card>();
                List<Card> diamonds = new List<Card>();
                List<Card> clubs = new List<Card>();
                // pentru fiecare carte din mana
                // adauga fiecare atu in lista sa
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

                // sorteaza fiecare lista

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

                // adauga listele in lista finala
                final.AddRange(hearts);
                final.AddRange(spades);
                final.AddRange(diamonds);
                final.AddRange(clubs);

                hands[i] = final;
            }
        }

        // verifica validitatea cartii
        private bool validCard(Card card)
        {
            // jucatorul trebuie sa joace cartea trucului, daca are vreuna
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

            // daca nu, poate juca orice carte
            if (card.getSuit() != trick.getSuit() && foundTrick)
                return false;
            return true;
        }

        private void button_score_exit_Click(object sender, EventArgs e)
        {
            displayPanel(panel_start);
        }

        // butonul de intoarcere la joc din tabela de scor
        private void button_score_back_Click(object sender, EventArgs e)
        {
            // reseteaza jocul
            displayPanel(panel_switch);
            label_switch.Text = players[currentPlayer] + ", este randul tau! Fa click pe ecran cand esti gata.";

            points = new int[4] { 0, 0, 0, 0 };

            hideInfo();

            // adauga cartile in maini
            Deck deck = new Deck();
            deck.Shuffle();

            for (int i = 0; i < 4; i++)
            {
                hands[i] = new List<Card>();
                for (int j = 0; j < 13; j++)
                    hands[i].Add(deck.DealCard());
            }
            sortHands();

            // alege un nou jucator
            Random r = new Random();
            currentPlayer = r.Next(3) + 1;
            startingPlayer = nextPlayer(1);

            // schimba atuul
            pictureBox_game_trump.ImageLocation = "cards/ace_of_" + trumps[currentTrump] + ".png";

            displayedCards = new List<PictureBox>();
            playedCards = new Card[4] { new Card("", "", 0, ""), new Card("", "", 0, ""), new Card("", "", 0, ""), new Card("", "", 0, "") };

            round = 0;
            points = new int[4] { 0, 0, 0, 0 };

            // incepe jocul
            rotate();
        }
    }
}

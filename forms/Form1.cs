using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Lab1_playingcardLibrary;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;
using System.IO;

namespace forms
{
    public enum CardLocation
    {
        Deck,
        DiscardPile,
        StandardPile,
        Hand1,
        Hand2
    }
    public enum ActiveHand
    {
        hand1,
        hand2,
        hand3,
        hand4,
    }
    public partial class Form1 : Form
    {
        string SaveFile = "SavedGame.data";

        Random random = new Random();
        private Deck _deck;
        private ActiveHand _activeHand; 
        private List<PlayingHand> _hands;
        private Transform _cardTrasnfomrBlueprint;
        public float LerpSmoothness;

        private List<Card> _cards;
        public Card FindCard(PlayingCard card) => _cards.Find(s => s.PlayingCard == card);

        private Point _deckPostion;
        public Point DeckPosition { get { return _deckPostion; } set { MoveDeck(value, 200); } }
        
        public Form1()
        {
            InitializeComponent();
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            Width = 1920;
            Height = 1080;
            BackColor = Color.Green;
            LerpSmoothness = 1f / 60f;
            DoubleBuffered = true;
            SetupGame();
            TimerMain.Start();
            _activeHand = ActiveHand.hand1;
            Shown += Form1_Shown;
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            Refresh();
        }

        private async void SetupGame()
        {
            GenerateRadioButtons();


            _hands = new List<PlayingHand>();
            for (int i = 0; i < 4; i++)
            {
                _hands.Add(new PlayingHand((Width / 2), Height / 8 * 7));
            }

            UpdateHands();

            _cards = new List<Card>();



            //_deck = new Deck(4, CardRank.Ace, CardRank.King, CardRank.Queen, CardRank.Jack);
            _deck = new Deck(2);
            _cardTrasnfomrBlueprint = new Transform(0, 0, 75, 107, 1);

            foreach (PlayingCard card in _deck.Cards) { _cards.Add(new Card(card, _cardTrasnfomrBlueprint, this)); }

            SetUpCards();

            SortCardLayers();
            RefreshAllCards();
            await MoveDeck(new Point(70, 140), 200);
            //DealCards();
        }
        

        private async Task DealCards()
        {
            List<Task> tasks = new List<Task>();
            int counter = (int)_activeHand;
            while(_deck.Cards.Count > 0) {
                tasks.Add(DealCard(200));
                counter++;
                _activeHand = (ActiveHand)(counter % 4);
                await Task.Delay(15);
            }
            await Task.WhenAll(tasks);
            return;
        }

        private void GenerateRadioButtons()
        {
            GroupBox box = new GroupBox();
            box.Location = new Point(Width/2, Height/10);
            box.Width = 200;
            box.Height = 200;

            for(int i = 0; i < 4; i++)
            {

                RadioButton button = new RadioButton();
                button.Text = (i+1).ToString();
                button.Tag = (ActiveHand)i;
                button.Location = new Point(20,40 + 20*i);
                button.CheckedChanged += Button_CheckedChanged;
                box.Controls.Add(button);
            }
            Controls.Add(box);
            box.Refresh();
        }

        private void Button_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            if (!radioButton.Checked) return;
            _activeHand = (ActiveHand)radioButton.Tag;
            UpdateHands();
        }

        private void UpdateHands()
        {
            for(int i = 0; i < _hands.Count; i++)
            {
                _hands[i].Visible = i == (int)_activeHand;
                _hands[i].UpdateHand();
            }
            _hands[(int)_activeHand].UpdateHand();
        }

        private void SortCardLayers2Deck()//sorts layers based of _deck card order. will follow the shuffle of the deck.
        {
            _cards.Sort((a, b) =>
            {
               return _deck.Cards.IndexOf(a.PlayingCard).CompareTo(_deck.Cards.IndexOf(b.PlayingCard));

            });
            foreach (Card card in _cards)
            {
                card._Image.BringToFront();
            }
        }

        private void SortCardLayers()//dont use, sorts layers based on card values. wont care about the shuffeled deck
        {
            _cards.Sort((a, b) =>
            {
                return a.PlayingCard.CompareTo(b.PlayingCard);

            });
            foreach(Card card in _cards)
            {
                card._Image.BringToFront();
            }
        }

        public void ShuffleDeck()//shuffles deck
        {
            _deck.Shuffle();
            _deck.Shuffle();
        }

        private void SetUpCards()//first time card setup
        {
            foreach (Card card in _cards)
            {
                card.UpdateImage();
                card.Paint();
                card._Image.MouseClick += MouseClickCard;
                card._Image.MouseEnter += MouseHandHover;
                card._Image.MouseLeave += MouseHandLeave;
            }
            ShuffleDeck();
        }


        private void RefreshAllCards()
        {
            foreach(Card card in _cards)
            {
                card.Paint();
            }
        }


        private async Task MoveCard(Card card, Point destination, int timeMS)
        {
            await CardMovement(card, destination, timeMS);
            return;
        }
        private async Task CardMovement(Card card, Point destination, int timeMS)
        {
            if (timeMS == 0) throw new Exception("timeMS cant be 0. Math error");

            Point StartPosition = new Point(card.transform.Xpos, card.transform.Ypos);
            float time = 0f;

            while(time < timeMS)
            {
                card.transform.Xpos = Lerp(StartPosition.X, destination.X, Math.Min(1f, time / timeMS));
                card.transform.Ypos = Lerp(StartPosition.Y, destination.Y, Math.Min(1f, time / timeMS));
                time += LerpSmoothness * 1000f;
                card.PaintFast();
                await Task.Delay((int)(LerpSmoothness * 1000));
            }
            card.transform.Xpos = destination.X;
            card.transform.Ypos= destination.Y;
            card.PaintFast();
            return;
        }
        private int Lerp(float a, float b, float f)
        {
            return (int) (a + f * (b - a));
        }
        private async Task MoveDeck(Point destination, int TimeMS)
        {
            _deckPostion = destination;
            float time = 0f;
            float jumps = TimeMS / _cards.Count;
            int jumpsI = (int)jumps;
            int i = 0;
            List<Task> tasks = new List<Task>();
            while(time < TimeMS && i < _cards.Count)
            {

                //destination = new Point((int)(Width * random.NextDouble()), (int)(Height * random.NextDouble()));
                tasks.Add(MoveCard(_cards[i], destination, TimeMS));
                time += jumps;
                i++;
                await Task.Delay(jumpsI);
            }
            await Task.WhenAll(tasks);
            return;
        }

        private async Task DealCard(int speed)//gives a card to player 1
        {
            PlayingCard pcard = _deck.DealTopCard();//gets playingcard from deck. gets removed from _deck
            Card card = FindCard(pcard);//gets Card with the containing Playingcard.

            _hands[(int)_activeHand].AddCard(pcard, card);// add playingCard and Card to _hand 1

            await MoveCard(card, _hands[(int)_activeHand].PositionCards, speed);//does animation, waits for it to complete

            card.Location = CardLocation.Hand1;// sets cards location to hand1
            card.transform.ScaleMultiplier = 1.5f;//increases card scale
            card.PlayingCard.SetFaceUp();// makes face up
            card.PaintFast();//paints card again.
            _hands[(int)_activeHand].UpdateHand();// redraws all cards and positions.

            return;
        }



        // EVENTS


        private void MouseHandLeave(object sender, EventArgs e)//mouse hover exit playing card
        {
            Card card = _cards.Find(c => c._Image == (PictureBox)sender);
            if (card.Location == CardLocation.Hand1)
            {
                card.transform.Ypos = _hands[(int)_activeHand].PositionCards.Y;
                card.PaintFast();
            }
        }

        private void MouseHandHover(object sender, EventArgs e)// mouse hover enters playing card 
        {
            Card card = _cards.Find(c => c._Image == (PictureBox)sender);
            if(card.Location == CardLocation.Hand1)
            {
                card.transform.Ypos = _hands[(int)_activeHand].PositionCards.Y - 30;
                card.PaintFast();
            }
        }

        private void button1_MouseClick(object sender, MouseEventArgs e)//reset button
        {
            SaveGame();
        }

        private async Task Reset()
        {
            foreach (Card card in _cards)
            {
                card.transform.ScaleMultiplier = 1f;
                card.Location = CardLocation.Deck;
                card.PlayingCard.SetFaceDown();
                card._Image.Visible = true;
            }
            foreach (PlayingHand hand in _hands)
            {
                int count = hand._cards.Count;
                for (int i = 0; i < count; i++)
                {
                    _deck.AddCard(hand.DealTopCard());
                }
            }
            ShuffleDeck();
            await MoveDeck(new Point(70,140), 200);
            return;
        }

        MySerializer<List<int[]>[]> _serialiser = new MySerializer<List<int[]>[]>();
        private async void SaveGame()
        {
            await SaveFileMethod();
            //need to serialise the thing here and add a way to read it back again.
        }

        private async Task SaveFileMethod()
        {
            List<int[]>[] templist = new List<int[]>[4];
            PdfHandler temp = new PdfHandler();
            for (int i = 0; i < _hands.Count; i++)
            {
                _hands[i].UpdateHand(true);
                temp.CreatePdfFromhand(_hands[i],$"Hands/Player{i + 1}_Hand.pdf");
                templist[i] = new List<int[]>();

                templist[i].AddRange(Array.ConvertAll(_hands[i].Cards.ToArray(), c => new int[2] { (int)c._rank, (int)c._suit }));
            }

            await Task.Run(() => _serialiser.WriteFile(SaveFile, templist));
            return;
        }

        private void MouseClickCard(object sender, MouseEventArgs e)//mouse clicks any card
        {
            Card card = _cards.Find(c => c._Image == (PictureBox)sender);
            if (card.Location == CardLocation.Deck) DealCard(300);
        }

        Card topCard;

        private void TimerTick(object sender, EventArgs e)
        {
            topCard?.PaintFast();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            LoadGame();
        }

        private async void LoadGame()
        {
            await Reset();

            if (!File.Exists(SaveFile)) return ;
            List<int[]>[] temp = _serialiser.ReadFile(SaveFile);
            List<Task> tasks = new List<Task>();

            for(int i = 0; i < temp.Length; i++)
            {
                foreach (int[] ii in temp[i])
                {
                    tasks.Add(MoveLoadedCard(i, ii));
                    await Task.Delay(50);
                }
                await Task.WhenAll(tasks);
            }
            return;
        }
        private async Task MoveLoadedCard(int i, int[] ii)
        {
            PlayingCard pcard = _deck.GetCard((CardRank)ii[0], (CardSuit)ii[1]);//gets playingcard from deck. gets removed from _deck
            if (pcard == null) return;

            Card card = FindCard(pcard);//gets Card with the containing Playingcard.

            _hands[i].AddCard(pcard, card);// add playingCard and Card to _hand 1
            await MoveCard(card, _hands[i].PositionCards, 200);//does animation, adds it to the wait list.

            card.Location = CardLocation.Hand1;// sets cards location to hand1
            card.transform.ScaleMultiplier = 1.5f;//increases card scale
            card.PlayingCard.SetFaceUp();// makes face up
            card.PaintFast();//paints card again.
            _hands[i].UpdateHand();// redraws all cards and positions.
            _hands[(int)_activeHand].UpdateHand();
        }

        private void resetButton(object sender, EventArgs e)
        {
            Reset();
        }

        private void DealCardsClicked(object sender, EventArgs e)
        {
            DealCards();
        }
    }
}

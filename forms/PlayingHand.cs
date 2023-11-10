using Lab1_playingcardLibrary;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace forms
{
    [Serializable]
    public class PlayingHand : Hand
    {
        public Point PositionCards;

        public List<Card> _cards;
        public PlayingHand(Point CardsLocation) : base() { 
            Visible = true;

            offset = 30;
            _cards = new List<Card>();
            PositionCards = CardsLocation;
        }

        public override PlayingCard DealTopCard()
        {
            PlayingCard temp = base.DealTopCard();
            _cards.Remove(_cards.Find(s => s.PlayingCard == temp));
            return temp;
        }

        public void AddCard(PlayingCard card, Card card2)
        {
            base.AddCard(card);
            _cards.Add(card2); 
            
        }
        public bool Visible;

        public int offset;

        public void UpdateHand(bool ForceSort)
        {
            int HandRange = offset * _cards.Count;

            int min = PositionCards.X - HandRange / 2;
            if(Visible || ForceSort) Sort();
            int i = 0;
            foreach(Card card in _cards)
            {
                if (card.Location != CardLocation.Hand1) continue;
                if(card._Image.Visible != Visible) card._Image.Visible = Visible;
                i++;
                card.transform.Xpos = min + offset * i;
                card.PaintFast();
            }
            foreach(Card card in _cards) if(card.Location == CardLocation.Hand1) card.PaintFast();
            //_cards[0]?.Paint();
        }
        public void UpdateHand()
        {
            UpdateHand(false);
        }
        public override void Sort()
        {
            base.Sort();
            sortToValues();
        }
        private void sortToValues()
        {
            _cards.Sort((a, b) =>
            {
                return a.PlayingCard.CompareTo(b.PlayingCard);

            });
            BringCardsToFront();
        }
        private void sortToLayers()
        {
            _cards.Sort((a, b) =>
            {
                int i = a._Image.Parent.Controls.GetChildIndex(a._Image);
                int j = b._Image.Parent.Controls.GetChildIndex(b._Image);
                return j.CompareTo(i);
            });
        }
        public void BringCardsToFront()
        {
            foreach (Card card in _cards)
            {
                card._Image.BringToFront();
            }
        }

        public PlayingHand(int Xpos, int Ypos) : this(new Point(Xpos, Ypos)) { }
    }
}

using Avalonia.Controls;
using Avalonia.Interactivity;
using lab8;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace lab8
{
    public class MakaoCard
    {
        public string Name { get; set; }
        public string Suit { get; set; }
        public string Rank { get; set; }
    }

    public class MakaoPlayer
    {
        public string Login { get; set; }
        public ObservableCollection<MakaoCard> Hand { get; set; } = new ObservableCollection<MakaoCard>();
    }

    public partial class MakaoWindow : Window
    {
        private List<MakaoPlayer> _players = new List<MakaoPlayer>();
        private List<MakaoCard> _deck = new List<MakaoCard>();
        private List<MakaoCard> _stack = new List<MakaoCard>();
        private int _currentPlayerIndex = 0;
        private bool _gameOver = false;

        private int _cardsToDraw = 0;
        private int _turnsToSkip = 0;
        private string _demandedSuit = "";
        private string _demandedRank = "";
        private bool _didSayMakao = false;

        public MakaoWindow(List<string> selectedLogins)
        {
            InitializeComponent();
            foreach (var login in selectedLogins) _players.Add(new MakaoPlayer { Login = login });

            this.FindControl<Button>("BtnReady").Click += Ready_Click;
            this.FindControl<Button>("BtnTakeCards").Click += TakeCards_Click;
            this.FindControl<Button>("BtnSayMakao").Click += SayMakao_Click;
            this.FindControl<ListBox>("LstCurrentHand").SelectionChanged += PlayCard_Selected;

            this.FindControl<Button>("BtnSuitSpades").Click += (s, e) => ApplySelection("suit", "♠");
            this.FindControl<Button>("BtnSuitHearts").Click += (s, e) => ApplySelection("suit", "♥");
            this.FindControl<Button>("BtnSuitDiamonds").Click += (s, e) => ApplySelection("suit", "♦");
            this.FindControl<Button>("BtnSuitClubs").Click += (s, e) => ApplySelection("suit", "♣");

            string[] ranks = { "5", "6", "7", "8", "9", "10" };
            foreach (var r in ranks)
            {
                this.FindControl<Button>($"BtnRank{r}").Click += (s, e) => ApplySelection("rank", r);
            }
            this.FindControl<Button>("BtnRankNone").Click += (s, e) => ApplySelection("rank", "");

            StartGame();
        }

        private void StartGame()
        {
            _deck = GenerateDeck();

            for (int i = 0; i < _players.Count; i++)
            {
                for (int c = 0; c < 5; c++)
                {
                    _players[i].Hand.Add(DrawFromDeck());
                }
            }

            var startCard = _deck.First(c => !"234JQKA".Contains(c.Rank));
            _deck.Remove(startCard);
            _stack.Add(startCard);

            PrepareCoverScreen();
        }

        private List<MakaoCard> GenerateDeck()
        {
            var suits = new[] { "♠", "♥", "♦", "♣" };
            var ranks = new[] { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };
            var newDeck = new List<MakaoCard>();

            foreach (var s in suits)
                foreach (var r in ranks)
                    newDeck.Add(new MakaoCard { Name = $"{r}{s}", Suit = s, Rank = r });

            return newDeck.OrderBy(x => Guid.NewGuid()).ToList();
        }

        private MakaoCard DrawFromDeck()
        {
            if (_deck.Count == 0)
            {
                var top = _stack.Last();
                _stack.Remove(top);
                _deck.AddRange(_stack.OrderBy(x => Guid.NewGuid()));
                _stack.Clear();
                _stack.Add(top);
            }
            var card = _deck.First();
            _deck.Remove(card);
            return card;
        }

        private void PrepareCoverScreen()
        {
            _didSayMakao = false;
            this.FindControl<Button>("BtnSayMakao").IsEnabled = true;

            this.FindControl<StackPanel>("PanelSuitSelection").IsVisible = false;
            this.FindControl<StackPanel>("PanelRankSelection").IsVisible = false;

            this.FindControl<ListBox>("LstCurrentHand").ItemsSource = null;

            var currentPlayer = _players[_currentPlayerIndex];
            this.FindControl<TextBlock>("TxtCoverMessage").Text = $"Teraz gra: {currentPlayer.Login}";

            this.FindControl<StackPanel>("PanelCover").IsVisible = true;
            this.FindControl<Grid>("PanelGame").IsVisible = false;
        }

        private void Ready_Click(object sender, RoutedEventArgs e)
        {
            if (_gameOver) return;
            var currentPlayer = _players[_currentPlayerIndex];

            var opps = _players.Where(p => p != currentPlayer).Select(p => $"{p.Login} ({p.Hand.Count})");
            this.FindControl<TextBlock>("TxtOpponentsInfo").Text = "Przeciwnicy: " + string.Join(" | ", opps);

            this.FindControl<TextBlock>("TxtTopCard").Text = _stack.Last().Name;
            this.FindControl<TextBlock>("TxtCurrentPlayerCards").Text = "Twoje karty:";

            string status = "";
            if (_cardsToDraw > 0) status = $"Dobierasz karty ({_cardsToDraw})";
            else if (_turnsToSkip > 0) status = $"Czekasz kolejkę ({_turnsToSkip})";
            else if (!string.IsNullOrEmpty(_demandedSuit)) status = $"ŻĄDANIE KOLORU: {_demandedSuit}";
            else if (!string.IsNullOrEmpty(_demandedRank)) status = $"ŻĄDANIE FIGURY: {_demandedRank}";

            this.FindControl<TextBlock>("TxtTableStatus").Text = status;

            var btnTake = this.FindControl<Button>("BtnTakeCards");
            if (_cardsToDraw > 0) btnTake.Content = "Poddaję się - Dobieram karty";
            else if (_turnsToSkip > 0) btnTake.Content = $"Poddaję się - Tracę kolejkę";
            else btnTake.Content = "Dobieram kartę";

            var lstHand = this.FindControl<ListBox>("LstCurrentHand");
            lstHand.ItemsSource = currentPlayer.Hand;
            lstHand.SelectedItem = null;

            this.FindControl<StackPanel>("PanelCover").IsVisible = false;
            this.FindControl<Grid>("PanelGame").IsVisible = true;
        }

        private void SayMakao_Click(object sender, RoutedEventArgs e)
        {
            _didSayMakao = true;
            this.FindControl<Button>("BtnSayMakao").IsEnabled = false;
        }

        private bool CanPlayCard(MakaoCard card, MakaoCard topCard)
        {
            if (card.Rank == "Q" || topCard.Rank == "Q") return true;

            if (_cardsToDraw > 0)
            {
                if (card.Rank == "2" || card.Rank == "3" || card.Rank == "K") return true;
                return false;
            }

            if (_turnsToSkip > 0)
            {
                if (card.Rank == "4") return true;
                return false;
            }

            if (!string.IsNullOrEmpty(_demandedSuit)) return card.Suit == _demandedSuit || card.Rank == "A";
            if (!string.IsNullOrEmpty(_demandedRank)) return card.Rank == _demandedRank || card.Rank == "J";

            return card.Suit == topCard.Suit || card.Rank == topCard.Rank;
        }

        private void PlayCard_Selected(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems == null || e.AddedItems.Count == 0 || _gameOver) return;

            if (!this.FindControl<Grid>("PanelGame").IsVisible) return;

            var card = e.AddedItems[0] as MakaoCard;
            if (card == null) return;

            var lstHand = this.FindControl<ListBox>("LstCurrentHand");
            lstHand.SelectedItem = null;

            var currentPlayer = _players[_currentPlayerIndex];
            var topCard = _stack.Last();

            if (CanPlayCard(card, topCard))
            {
                bool forgotMakao = (currentPlayer.Hand.Count == 2 && !_didSayMakao);

                currentPlayer.Hand.Remove(card);
                _stack.Add(card);

                if (currentPlayer.Hand.Count == 0 && !forgotMakao)
                {
                    EndGame(currentPlayer.Login);
                    return;
                }

                if (card.Rank != "A") _demandedSuit = "";
                if (card.Rank != "J") _demandedRank = "";

                if (card.Rank == "Q") { _cardsToDraw = 0; _turnsToSkip = 0; }
                else if (card.Rank == "2") _cardsToDraw += 2;
                else if (card.Rank == "3") _cardsToDraw += 3;
                else if (card.Rank == "K") _cardsToDraw += 5;
                else if (card.Rank == "4") _turnsToSkip += 1;

                if (forgotMakao)
                {
                    for (int i = 0; i < 5; i++) currentPlayer.Hand.Add(DrawFromDeck());
                }

                if (card.Rank == "A")
                {
                    this.FindControl<StackPanel>("PanelSuitSelection").IsVisible = true;
                    this.FindControl<ListBox>("LstCurrentHand").IsEnabled = false;
                    this.FindControl<Button>("BtnTakeCards").IsEnabled = false;
                    this.FindControl<TextBlock>("TxtTopCard").Text = card.Name;
                    return;
                }
                if (card.Rank == "J")
                {
                    this.FindControl<StackPanel>("PanelRankSelection").IsVisible = true;
                    this.FindControl<ListBox>("LstCurrentHand").IsEnabled = false;
                    this.FindControl<Button>("BtnTakeCards").IsEnabled = false;
                    this.FindControl<TextBlock>("TxtTopCard").Text = card.Name;
                    return;
                }

                AdvanceTurn();
            }
        }

        private void ApplySelection(string type, string value)
        {
            if (type == "suit") _demandedSuit = value;
            else _demandedRank = value;

            this.FindControl<ListBox>("LstCurrentHand").IsEnabled = true;
            this.FindControl<Button>("BtnTakeCards").IsEnabled = true;
            AdvanceTurn();
        }

        private void TakeCards_Click(object sender, RoutedEventArgs e)
        {
            var currentPlayer = _players[_currentPlayerIndex];

            if (_cardsToDraw > 0)
            {
                for (int i = 0; i < _cardsToDraw; i++) currentPlayer.Hand.Add(DrawFromDeck());
                _cardsToDraw = 0;
            }
            else if (_turnsToSkip > 0)
            {
                _turnsToSkip--;
            }
            else
            {
                currentPlayer.Hand.Add(DrawFromDeck());
            }

            AdvanceTurn();
        }

        private void AdvanceTurn()
        {
            _currentPlayerIndex = (_currentPlayerIndex + 1) % _players.Count;
            PrepareCoverScreen();
        }

        private void EndGame(string winnerLogin)
        {
            _gameOver = true;

            AppState.MatchHistory.Add(new MatchRecord
            {
                GameName = "Makao",
                WinnerLogin = winnerLogin,
                Date = DateTime.Now.ToString("g")
            });

            this.FindControl<StackPanel>("PanelCover").IsVisible = true;
            this.FindControl<Grid>("PanelGame").IsVisible = false;

            this.FindControl<TextBlock>("TxtCoverMessage").Text = $"Koniec gry! Wygrywa: {winnerLogin}";
            this.FindControl<Button>("BtnReady").IsVisible = false;
        }
    }
}
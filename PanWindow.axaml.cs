using Avalonia.Controls;
using Avalonia.Interactivity;
using lab8;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace lab8
{
    public class PanCard
    {
        public string Name { get; set; }
        public int Value { get; set; }
    }

    public class PanPlayer
    {
        public string Login { get; set; }
        public ObservableCollection<PanCard> Hand { get; set; } = new ObservableCollection<PanCard>();
    }

    public partial class PanWindow : Window
    {
        private List<PanPlayer> _players = new List<PanPlayer>();
        private List<PanCard> _stack = new List<PanCard>();
        private int _currentPlayerIndex = 0;
        private bool _gameOver = false;

        public PanWindow(List<string> selectedLogins)
        {
            InitializeComponent();

            foreach (var login in selectedLogins)
            {
                _players.Add(new PanPlayer { Login = login });
            }

            this.FindControl<Button>("BtnReady").Click += Ready_Click;
            this.FindControl<Button>("BtnTakeCards").Click += TakeCards_Click;
            this.FindControl<ListBox>("LstCurrentHand").SelectionChanged += PlayCard_Selected;

            StartGame();
        }

        private void StartGame()
        {
            var deck = GenerateDeck();
            int cardsPerPlayer = deck.Count / _players.Count;

            for (int i = 0; i < _players.Count; i++)
            {
                var dealtCards = deck.Skip(i * cardsPerPlayer).Take(cardsPerPlayer).OrderBy(c => c.Value);
                foreach (var card in dealtCards)
                {
                    _players[i].Hand.Add(card);
                }
            }

            int starterIndex = 0;
            for (int i = 0; i < _players.Count; i++)
            {
                var nineOfHearts = _players[i].Hand.FirstOrDefault(c => c.Name == "9♥");
                if (nineOfHearts != null)
                {
                    _players[i].Hand.Remove(nineOfHearts);
                    _stack.Add(nineOfHearts);
                    starterIndex = i;
                    break;
                }
            }

            _currentPlayerIndex = (starterIndex + 1) % _players.Count;
            PrepareCoverScreen();
        }

        private List<PanCard> GenerateDeck()
        {
            var suits = new[] { "♠", "♥", "♦", "♣" };
            var ranks = new[] { "9", "10", "J", "Q", "K", "A" };
            var newDeck = new List<PanCard>();

            foreach (var suit in suits)
            {
                foreach (var rank in ranks)
                {
                    int val = rank switch
                    {
                        "9" => 9,
                        "10" => 10,
                        "J" => 11,
                        "Q" => 12,
                        "K" => 13,
                        "A" => 14,
                        _ => 0
                    };
                    newDeck.Add(new PanCard { Name = $"{rank}{suit}", Value = val });
                }
            }
            return newDeck.OrderBy(x => Guid.NewGuid()).ToList();
        }

        private void PrepareCoverScreen()
        {
            var currentPlayer = _players[_currentPlayerIndex];

            this.FindControl<TextBlock>("TxtCoverMessage").Text = $"Teraz gra: {currentPlayer.Login}";

            this.FindControl<StackPanel>("PanelCover").IsVisible = true;
            this.FindControl<Grid>("PanelGame").IsVisible = false;
        }

        private void Ready_Click(object sender, RoutedEventArgs e)
        {
            if (_gameOver) return;

            var currentPlayer = _players[_currentPlayerIndex];

            var opponentsInfo = _players
                .Where(p => p != currentPlayer)
                .Select(p => $"{p.Login} ({p.Hand.Count} kart)")
                .ToList();

            this.FindControl<TextBlock>("TxtOpponentsInfo").Text = "Przeciwnicy: " + string.Join(" | ", opponentsInfo);

            this.FindControl<TextBlock>("TxtTopCard").Text = _stack.Last().Name;
            this.FindControl<TextBlock>("TxtStackCount").Text = $"Karty na stosie: {_stack.Count}";
            this.FindControl<TextBlock>("TxtCurrentPlayerCards").Text = "Twoje karty:";

            var lstHand = this.FindControl<ListBox>("LstCurrentHand");
            lstHand.ItemsSource = currentPlayer.Hand;
            lstHand.SelectedItem = null;

            this.FindControl<StackPanel>("PanelCover").IsVisible = false;
            this.FindControl<Grid>("PanelGame").IsVisible = true;
        }

        private void AdvanceTurn()
        {
            _currentPlayerIndex = (_currentPlayerIndex + 1) % _players.Count;
            PrepareCoverScreen();
        }

        private void PlayCard_Selected(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems == null || e.AddedItems.Count == 0 || _gameOver) return;

            var card = e.AddedItems[0] as PanCard;
            if (card == null) return;

            var lstHand = this.FindControl<ListBox>("LstCurrentHand");

            lstHand.SelectedItem = null;

            var currentPlayer = _players[_currentPlayerIndex];

            if (card.Value >= _stack.Last().Value)
            {
                currentPlayer.Hand.Remove(card);
                _stack.Add(card);

                if (currentPlayer.Hand.Count == 0)
                {
                    EndGame(currentPlayer.Login);
                    return;
                }

                AdvanceTurn();
            }
        }

        private void TakeCards_Click(object sender, RoutedEventArgs e)
        {
            if (_gameOver) return;

            var currentPlayer = _players[_currentPlayerIndex];

            int cardsToTakeCount = Math.Min(3, _stack.Count - 1);
            if (cardsToTakeCount > 0)
            {
                var cardsToTake = new List<PanCard>();

                for (int i = 0; i < cardsToTakeCount; i++)
                {
                    cardsToTake.Add(_stack.Last());
                    _stack.RemoveAt(_stack.Count - 1);
                }

                var tempHand = currentPlayer.Hand.Concat(cardsToTake).OrderBy(c => c.Value).ToList();
                currentPlayer.Hand.Clear();
                foreach (var c in tempHand) currentPlayer.Hand.Add(c);
            }

            AdvanceTurn();
        }

        private void EndGame(string winnerLogin)
        {
            _gameOver = true;

            AppState.MatchHistory.Add(new MatchRecord
            {
                GameName = "Pan",
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
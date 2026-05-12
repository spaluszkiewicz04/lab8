using Avalonia.Controls;
using Avalonia.Interactivity;
using lab8;
using System;
using System.Collections.Generic;
using System.Linq;

namespace lab8
{
    public class Card
    {
        public string Name { get; set; }
        public int Value { get; set; }
        public bool IsAce { get; set; }
    }

    public partial class BlackjackWindow : Window
    {
        private List<Card> _deck = new List<Card>();
        private List<Card> _playerCards = new List<Card>();
        private List<Card> _dealerCards = new List<Card>();
        private Random _rnd = new Random();
        private bool _gameOver = false;

        public BlackjackWindow()
        {
            InitializeComponent();

            this.FindControl<Button>("BtnHit").Click += Hit_Click;
            this.FindControl<Button>("BtnStand").Click += Stand_Click;
            this.FindControl<Button>("BtnNewGame").Click += NewGame_Click;

            var cmbPlayers = this.FindControl<ComboBox>("CmbPlayers");
            var selectionPanel = this.FindControl<StackPanel>("PlayerSelectionPanel");

            if (AppState.Players.Count == 0)
            {
                selectionPanel.IsVisible = false;
            }
            else
            {
                cmbPlayers.ItemsSource = AppState.Players;
                cmbPlayers.SelectedIndex = 0;
            }

            StartNewGame();
        }

        private void StartNewGame()
        {
            _gameOver = false;
            _deck = GenerateDeck();
            _playerCards.Clear();
            _dealerCards.Clear();

            this.FindControl<TextBlock>("TxtGameResult").Text = "";
            this.FindControl<Button>("BtnHit").IsEnabled = true;
            this.FindControl<Button>("BtnStand").IsEnabled = true;

            _playerCards.Add(DrawCard());
            _playerCards.Add(DrawCard());
            _dealerCards.Add(DrawCard());
            _dealerCards.Add(DrawCard());

            UpdateUI();
            CheckInitialBlackjack();
        }

        private List<Card> GenerateDeck()
        {
            var suits = new[] { "♠", "♥", "♦", "♣" };
            var ranks = new[] { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };
            var newDeck = new List<Card>();

            foreach (var suit in suits)
            {
                foreach (var rank in ranks)
                {
                    int val = 0;
                    bool isAce = false;

                    if (rank == "A") { val = 11; isAce = true; }
                    else if (rank == "J" || rank == "Q" || rank == "K") { val = 10; }
                    else { val = int.Parse(rank); }

                    newDeck.Add(new Card { Name = $"{rank}{suit}", Value = val, IsAce = isAce });
                }
            }
            return newDeck.OrderBy(x => _rnd.Next()).ToList();
        }

        private Card DrawCard()
        {
            var card = _deck.First();
            _deck.RemoveAt(0);
            return card;
        }

        private int CalculateScore(List<Card> hand)
        {
            int score = hand.Sum(c => c.Value);
            int aces = hand.Count(c => c.IsAce);

            while (score > 21 && aces > 0)
            {
                score -= 10;
                aces--;
            }
            return score;
        }

        private void UpdateUI()
        {
            this.FindControl<TextBlock>("TxtPlayerCards").Text = string.Join("  ", _playerCards.Select(c => c.Name));
            this.FindControl<TextBlock>("TxtPlayerScore").Text = $"Punkty: {CalculateScore(_playerCards)}";

            this.FindControl<TextBlock>("TxtDealerCards").Text = string.Join("  ", _dealerCards.Select(c => c.Name));
            this.FindControl<TextBlock>("TxtDealerScore").Text = $"Punkty: {CalculateScore(_dealerCards)}";
        }

        private void Hit_Click(object sender, RoutedEventArgs e)
        {
            if (_gameOver) return;

            _playerCards.Add(DrawCard());
            UpdateUI();

            if (CalculateScore(_playerCards) > 21)
            {
                EndGame("Krupier wygrywa");
            }
        }

        private void Stand_Click(object sender, RoutedEventArgs e)
        {
            if (_gameOver) return;

            while (CalculateScore(_dealerCards) < 17)
            {
                _dealerCards.Add(DrawCard());
            }

            UpdateUI();

            int playerScore = CalculateScore(_playerCards);
            int dealerScore = CalculateScore(_dealerCards);

            if (dealerScore > 21)
                EndGame("Wygrywasz");
            else if (playerScore > dealerScore)
                EndGame("Wygrywasz");
            else if (playerScore < dealerScore)
                EndGame("Krupier wygrywa");
            else
                EndGame("Remis");
        }

        private void CheckInitialBlackjack()
        {
            if (CalculateScore(_playerCards) == 21)
            {
                EndGame("Wygrywasz");
            }
        }

        private void EndGame(string resultMessage)
        {
            _gameOver = true;
            this.FindControl<TextBlock>("TxtGameResult").Text = resultMessage;
            this.FindControl<Button>("BtnHit").IsEnabled = false;
            this.FindControl<Button>("BtnStand").IsEnabled = false;

            string winner = "Krupier";
            if (resultMessage.Contains("Wygrywasz"))
            {
                var cmbPlayers = this.FindControl<ComboBox>("CmbPlayers");

                if (AppState.Players.Count > 0 && cmbPlayers.SelectedItem is Player selectedPlayer)
                {
                    winner = selectedPlayer.Login;
                }
                else
                {
                    winner = "Gość";
                }
            }
            else if (resultMessage.Contains("Remis"))
            {
                winner = "Remis";
            }

            AppState.MatchHistory.Add(new MatchRecord
            {
                GameName = "Blackjack",
                WinnerLogin = winner,
                Date = DateTime.Now.ToString("g")
            });
        }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            StartNewGame();
        }
    }
}
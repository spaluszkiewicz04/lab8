using Avalonia.Controls;
using Avalonia.Interactivity;
using lab8;
using System;

namespace lab8
{
    public partial class PlayerWindow : Window
    {
        public PlayerWindow()
        {
            InitializeComponent();
            this.FindControl<Button>("BtnSave").Click += SavePlayer;
        }

        private void SavePlayer(object sender, RoutedEventArgs e)
        {
            var loginInput = this.FindControl<TextBox>("TxtLogin").Text;
            if (!string.IsNullOrWhiteSpace(loginInput))
            {
                var newPlayer = new Player
                {
                    Login = loginInput,
                    Id = AppState.Players.Count + 1
                };
                AppState.Players.Add(newPlayer);

                AppState.SavePlayers();

                this.FindControl<TextBlock>("TxtMessage").Text = $"Dodano gracza: {loginInput}!";
                this.FindControl<TextBox>("TxtLogin").Text = "";
            }
        }
    }
}
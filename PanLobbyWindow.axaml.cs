using Avalonia.Controls;
using Avalonia.Interactivity;
using lab8;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace lab8
{
    public class PlayerSelectionItem
    {
        public Player Player { get; set; }
        public bool IsSelected { get; set; }
    }

    public partial class PanLobbyWindow : Window
    {
        private ObservableCollection<PlayerSelectionItem> _selectablePlayers = new();

        public PanLobbyWindow()
        {
            InitializeComponent();

            foreach (var player in AppState.Players)
            {
                _selectablePlayers.Add(new PlayerSelectionItem { Player = player, IsSelected = false });
            }

            this.FindControl<ListBox>("LstPlayers").ItemsSource = _selectablePlayers;
            this.FindControl<Button>("BtnStartGame").Click += StartGame_Click;
        }

        private void StartGame_Click(object sender, RoutedEventArgs e)
        {
            var selectedLogins = _selectablePlayers
                .Where(p => p.IsSelected)
                .Select(p => p.Player.Login)
                .ToList();

            var txtError = this.FindControl<TextBlock>("TxtError");

            if (selectedLogins.Count < 2 || selectedLogins.Count > 4)
            {
                txtError.Text = "Wybierz od 2 do 4 graczy";
                return;
            }

            var panGameWindow = new PanWindow(selectedLogins);
            panGameWindow.Show();

            this.Close();
        }
    }
}
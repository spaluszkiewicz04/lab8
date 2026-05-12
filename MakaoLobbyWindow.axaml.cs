using Avalonia.Controls;
using Avalonia.Interactivity;
using lab8;
using System.Collections.ObjectModel;
using System.Linq;

namespace lab8
{
    public partial class MakaoLobbyWindow : Window
    {
        private ObservableCollection<PlayerSelectionItem> _selectablePlayers = new();

        public MakaoLobbyWindow()
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

            if (selectedLogins.Count < 2 || selectedLogins.Count > 4)
            {
                this.FindControl<TextBlock>("TxtError").Text = "Wybierz od 2 do 4 graczy";
                return;
            }

            var gameWindow = new MakaoWindow(selectedLogins);
            gameWindow.Show();

            this.Close();
        }
    }
}
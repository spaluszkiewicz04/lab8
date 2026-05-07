using Avalonia.Controls;
using Avalonia.Interactivity;

namespace lab8
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.FindControl<Button>("BtnAddPlayer").Click += OpenAddPlayer;
            this.FindControl<Button>("BtnHistory").Click += OpenHistory;

            this.FindControl<Button>("BtnGame1").Click += (s, e) => { /* Gra 1 */ };
            this.FindControl<Button>("BtnGame2").Click += (s, e) => { /* Gra 2 */ };
            this.FindControl<Button>("BtnGame3").Click += (s, e) => { /* Gra 3 */ };
        }

        private void OpenAddPlayer(object sender, RoutedEventArgs e)
        {
            var playerWindow = new PlayerWindow();
            playerWindow.ShowDialog(this);
        }

        private void OpenHistory(object sender, RoutedEventArgs e)
        {
            //var historyWindow = new HistoryWindow();
            //historyWindow.Show();
        }
    }
}
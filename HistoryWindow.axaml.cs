using Avalonia.Controls;
using System;

namespace lab8
{
    public partial class HistoryWindow : Window
    {
        public HistoryWindow()
        {
            InitializeComponent();

            var listBox = this.FindControl<ListBox>("LstHistory");
            listBox.ItemsSource = AppState.MatchHistory;
        }
    }
}
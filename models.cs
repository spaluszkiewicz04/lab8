using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;

namespace lab8
{
    public class Player
    {
        public string Login { get; set; }
        public int Id { get; set; }
    }

    public class PlayerSelectionItem
    {
        public Player Player { get; set; }
        public bool IsSelected { get; set; }
    }

    public class MatchRecord
    {
        public string GameName { get; set; }
        public string WinnerLogin { get; set; }
        public string Date { get; set; }
    }

    public static class AppState
    {
        public static ObservableCollection<Player> Players { get; set; } = new ObservableCollection<Player>();
        public static ObservableCollection<MatchRecord> MatchHistory { get; set; } = new ObservableCollection<MatchRecord>();

        private const string PlayersFilePath = "players.json";

        public static void SavePlayers()
        {
            try
            {
                var json = JsonSerializer.Serialize(Players);
                File.WriteAllText(PlayersFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Błąd");
            }
        }

        public static void LoadPlayers()
        {
            try
            {
                if (File.Exists(PlayersFilePath))
                {
                    var json = File.ReadAllText(PlayersFilePath);
                    var loadedPlayers = JsonSerializer.Deserialize<ObservableCollection<Player>>(json);

                    if (loadedPlayers != null)
                    {
                        Players.Clear();
                        foreach (var p in loadedPlayers)
                        {
                            Players.Add(p);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Błąd");
            }
        }
    }
}
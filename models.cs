using System.Collections.Generic;

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
        public static List<Player> Players { get; set; } = new List<Player>();
        public static List<MatchRecord> MatchHistory { get; set; } = new List<MatchRecord>();
    }
}
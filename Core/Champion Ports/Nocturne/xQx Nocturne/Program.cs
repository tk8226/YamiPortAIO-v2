using LeagueSharp;

using EloBuddy; 
 using LeagueSharp.Common; 
 namespace Nocturne
{
    internal class Program
    {
        public static string ChampionName => "Einstein Exory";
        private static void Main(string[] args)
        {
            if (ObjectManager.Player.ChampionName == "Nocturne")
            {
                Nocturne.Init();
            }
        }
    }
}

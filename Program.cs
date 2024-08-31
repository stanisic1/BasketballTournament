using BasketballTournament;
using BasketballTournament.Model;
using System.Text.Json;

class Program 
{
    static void Main(string[] args)
    {
        var groups = JsonSerializer.Deserialize<Dictionary<string, List<OlympicTeam>>>(DataStorage.GroupData);

       // var exhibitions = JsonSerializer.Deserialize<Dictionary<string, List<ExhibitionMatch>>>(DataStorage.ExhibitionData);

        var tournament = new Tournament(groups);

        tournament.SimulateGroupStage();
        tournament.PrintGroupRankings();
        tournament.RankAndSetupHats();

    }  

}


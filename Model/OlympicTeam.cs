using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasketballTournament.Model
{
    public class OlympicTeam
    {
        public string Team { get; set; }
        public string ISOCode { get; set; }
        public int FIBARanking { get; set; }
        public string GroupKey { get; set; }
        public int Points { get; set; } = 0;
        public int Wins { get; set; } = 0;
        public int Losses { get; set; } = 0;
        public int ScoredPoints { get; set; } = 0;
        public int ConcededPoints { get; set; } = 0;
       
        public double Form { get; set; } = 1.0;

        public int PointDifference => ScoredPoints - ConcededPoints;

        public void UpdateStats(int scored, int conceded, bool won, int pointsAwarded)
        {
            ScoredPoints += scored;
            ConcededPoints += conceded;

            if (pointsAwarded == 0)
            {
                Losses++;
            }
            else if (won)
            {
                Wins++;
                Points += pointsAwarded; 
            }
            else
            {
                Losses++;
                Points += pointsAwarded; 
            }
        }

        public static int CompareTeamsByMutualResults(OlympicTeam team1, OlympicTeam team2, Dictionary<string, List<Match>> groupMatches)
        {
            var match = groupMatches.Values.SelectMany(g => g)
                .FirstOrDefault(m => (m.Team1.ISOCode == team1.ISOCode && m.Team2.ISOCode == team2.ISOCode) ||
                                     (m.Team1.ISOCode == team2.ISOCode && m.Team2.ISOCode == team1.ISOCode));
            if (match != null)
            {
                if (match.Team1Score > match.Team2Score)
                    return team1.ISOCode == match.Team1.ISOCode ? 1 : -1;
                if (match.Team1Score < match.Team2Score)
                    return team1.ISOCode == match.Team1.ISOCode ? -1 : 1;
            }
            return 0;
        }

        public static int CalculatePointDifferenceForTiedTeams(List<OlympicTeam> teams, Dictionary<string, List<Match>> groupMatches)
        {
            int totalPointDifference = 0;
            foreach (var team1 in teams)
            {
                foreach (var team2 in teams)
                {
                    if (team1.ISOCode != team2.ISOCode)
                    {
                        var match = groupMatches.Values.SelectMany(g => g)
                            .FirstOrDefault(m => (m.Team1.ISOCode == team1.ISOCode && m.Team2.ISOCode == team2.ISOCode) ||
                                                 (m.Team1.ISOCode == team2.ISOCode && m.Team2.ISOCode == team1.ISOCode));
                        if (match != null)
                        {
                            if (team1.ISOCode == match.Team1.ISOCode)
                                totalPointDifference += match.Team1Score - match.Team2Score;
                            else
                                totalPointDifference += match.Team2Score - match.Team1Score;
                        }
                    }
                }
            }
            return totalPointDifference;
        }

        public static List<OlympicTeam> RankTeams(List<OlympicTeam> teams, Dictionary<string, List<Match>> groupMatches)
        {
            var sortedTeams = teams.OrderByDescending(t => t.Points).ToList();
            var rankedTeams = new List<OlympicTeam>();

            while (sortedTeams.Any())
            {
                // Get teams with the same points
                var currentPoints = sortedTeams.First().Points;
                var currentGroup = sortedTeams.TakeWhile(t => t.Points == currentPoints).ToList();
                sortedTeams = sortedTeams.Skip(currentGroup.Count).ToList();

                if (currentGroup.Count == 1)
                {
                    // Only one team in this group, just add it
                    rankedTeams.Add(currentGroup.First());
                }
                else
                {
                    // Multiple teams tied, apply tie-breaking criteria
                    var teamComparison = currentGroup
                        .Select(team => new
                        {
                            Team = team,
                            MutualResults = currentGroup
                                .Where(other => other != team)
                                .Select(other => CompareTeamsByMutualResults(team, other, groupMatches))
                                .Sum()
                        })
                        .OrderByDescending(t => t.MutualResults)
                        .ThenByDescending(t => t.Team.PointDifference)
                        .ThenByDescending(t => t.Team.ScoredPoints)
                        .Select(t => t.Team)
                        .ToList();

                    rankedTeams.AddRange(teamComparison);
                }
            }
            return rankedTeams;
        }

    }
}

﻿using BasketballTournament.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BasketballTournament
{
    public class Tournament
    {
        public Dictionary<string, List<OlympicTeam>> Groups { get; set; }
        private Dictionary<string, List<ExhibitionMatch>> Exhibitions { get; set; }
        private List<Model.Match> Matches { get; set; } = new List<Model.Match>();

        public Tournament(Dictionary<string, List<OlympicTeam>> groups, Dictionary<string, List<ExhibitionMatch>> exhibitions)
        {
            Groups = groups;
            Exhibitions = exhibitions;
        }

        public void InitializeTeamForms()
        {
            foreach (var group in Groups)
            {
                foreach (var team in group.Value)
                {
                    if (Exhibitions.TryGetValue(team.ISOCode, out var exhibitionMatches))
                    {
                        team.UpdateForm(exhibitionMatches, new List<Model.Match>());
                    }
                }
            }
        }

        public void SimulateGroupStage()
        {
            Console.WriteLine("Grupna faza:");
            foreach (var group in Groups)
            {
                var teams = group.Value;
                for (int i = 0; i < teams.Count; i++)
                {
                    for (int j = i + 1; j < teams.Count; j++)
                    {
                        var match = Model.Match.Simulate(teams[i], teams[j]);
                        match.GroupKey = group.Key; 
                        Matches.Add(match); 
                        match.PrintResult(group.Key);
                    }
                }
                UpdateTeamFormsAfterMatch();
            }
            Console.WriteLine();
        }

        public void UpdateTeamFormsAfterMatch()
        {
            foreach (var group in Groups)
            {
                foreach (var team in group.Value)
                {
                    var tournamentMatches = Matches.Where(m => m.Team1.ISOCode == team.ISOCode || m.Team2.ISOCode == team.ISOCode).ToList();
                    team.UpdateForm(Exhibitions.GetValueOrDefault(team.ISOCode, new List<ExhibitionMatch>()), tournamentMatches);
                }
            }
        }

        public void PrintGroupRankings()
        {
            foreach (var group in Groups)
            {
                var teams = group.Value;
                var groupMatches = new Dictionary<string, List<Model.Match>>();

                foreach (var team in teams)
                {
                    groupMatches[team.ISOCode] = new List<Model.Match>();
                }

                foreach (var match in GetAllMatchesForGroup(group.Key))
                {
                    if (groupMatches.ContainsKey(match.Team1.ISOCode))
                        groupMatches[match.Team1.ISOCode].Add(match);
                    if (groupMatches.ContainsKey(match.Team2.ISOCode))
                        groupMatches[match.Team2.ISOCode].Add(match);
                }

                var rankedTeams = OlympicTeam.RankTeams(teams, groupMatches);
                Console.WriteLine($"Grupa {group.Key} Konacan plasman:");
                Console.WriteLine();
                Console.WriteLine("    Tim - pobede/porazi/bodovi/postignuti koševi/primljeni koševi/koš razlika");
                for (int i = 0; i < rankedTeams.Count; i++)
                {
                    var team = rankedTeams[i];
                    Console.WriteLine($"{i + 1}. {team.Team} - {team.Wins} /{team.Losses} /{team.Points} /{team.ScoredPoints} /{team.ConcededPoints} /{team.PointDifference}");
                   
                }
                Console.WriteLine();
            }
        }

        public void RankAndSetupHats()
        {
            var topTeams = new List<OlympicTeam>();
            var secondTeams = new List<OlympicTeam>();
            var thirdTeams = new List<OlympicTeam>();

            foreach (var group in Groups)
            {
                var teams = group.Value;

                var groupMatches = GetAllMatchesForGroup(group.Key)
                    .GroupBy(m => m.Team1.ISOCode)
                    .ToDictionary(g => g.Key, g => g.ToList());

                var rankedTeamsByGroups = OlympicTeam.RankTeams(teams, groupMatches);

                if (rankedTeamsByGroups.Count > 2)
                {
                    topTeams.Add(rankedTeamsByGroups[0]); 
                    secondTeams.Add(rankedTeamsByGroups[1]);  
                    thirdTeams.Add(rankedTeamsByGroups[2]); 
                }
            }

            topTeams = topTeams.OrderByDescending(t => t.Points)
                                .ThenByDescending(t => t.PointDifference)
                                .ThenByDescending(t => t.ScoredPoints)
                                .ToList();

            secondTeams = secondTeams.OrderByDescending(t => t.Points)
                                      .ThenByDescending(t => t.PointDifference)
                                      .ThenByDescending(t => t.ScoredPoints)
                                      .ToList();

            thirdTeams = thirdTeams.OrderByDescending(t => t.Points)
                                    .ThenByDescending(t => t.PointDifference)
                                    .ThenByDescending(t => t.ScoredPoints)
                                    .ToList();

            var allTeams = topTeams
                .Concat(secondTeams)
                .Concat(thirdTeams)
                .ToList();

            if (allTeams.Count < 9)
            {
                Console.WriteLine("Nema dovoljno timova da se predje u eliminacionu fazu.");
                return;
            }

            var rankedTeams = allTeams.Take(9).ToList();
            var excludedTeam = rankedTeams.Last();
            rankedTeams = rankedTeams.Take(8).ToList();

        
            var hatD = rankedTeams.Take(2).ToList();  
            var hatE = rankedTeams.Skip(2).Take(2).ToList();  
            var hatF = rankedTeams.Skip(4).Take(2).ToList();  
            var hatG = rankedTeams.Skip(6).Take(2).ToList();  

            PrintHatTeams("Sesir D", hatD);
            PrintHatTeams("Sesir E", hatE);
            PrintHatTeams("Sesir F", hatF);
            PrintHatTeams("Sesir G", hatG);

          
            GenerateKnockoutMatches(hatD, hatE, hatF, hatG);
        }

        private IEnumerable<Model.Match> GetAllMatchesForGroup(string groupKey)
        {
            if (Matches == null || !Matches.Any())
            {
                throw new InvalidOperationException("Nema podataka za utakmice.");
            }

            return Matches.Where(m => m.GroupKey == groupKey);
        }

        private void PrintHatTeams(string hatName, List<OlympicTeam> teams)
        {
            Console.WriteLine($"{hatName}:");
            foreach (var team in teams)
            {
                Console.WriteLine($" - {team.Team}");
            }
        }

      

        private void GenerateKnockoutMatches(List<OlympicTeam> hatD, List<OlympicTeam> hatE, List<OlympicTeam> hatF, List<OlympicTeam> hatG)
        {
            var random = new Random();
            var quarterFinals = new List<Model.Match>();

            var possibleMatchesD_G = new List<(OlympicTeam, OlympicTeam)>();
            var possibleMatchesE_F = new List<(OlympicTeam, OlympicTeam)>();

            foreach (var teamD in hatD)
            {
                foreach (var teamG in hatG)
                {
                    if (!HaveTeamsPlayedInGroupStage(teamD, teamG))
                    {
                        possibleMatchesD_G.Add((teamD, teamG));
                    }
                }
            }


            foreach (var teamE in hatE)
            {
                foreach (var teamF in hatF)
                {
                    if (!HaveTeamsPlayedInGroupStage(teamE, teamF))
                    {
                        possibleMatchesE_F.Add((teamE, teamF));
                    }
                }
            }

           
            var mandatoryMatches = new List<(OlympicTeam, OlympicTeam)>();
            var hatDTeams = new List<OlympicTeam>(hatD);
            var hatGTeams = new List<OlympicTeam>(hatG);

            foreach (var teamD in hatDTeams)
            {
                var validOpponents = possibleMatchesD_G.Where(m => m.Item1 == teamD || m.Item2 == teamD).ToList();
                if (validOpponents.Count == 1)
                {
                    var matchPair = validOpponents[0];
                    if (matchPair.Item1 == teamD) hatGTeams.Remove(matchPair.Item2);
                    else hatDTeams.Remove(matchPair.Item1);

                    mandatoryMatches.Add(matchPair);
                    possibleMatchesD_G.Remove(matchPair);
                }
            }

            
            var hatETeams = new List<OlympicTeam>(hatE);
            var hatFTeams = new List<OlympicTeam>(hatF);

            foreach (var teamE in hatETeams)
            {
                var validOpponents = possibleMatchesE_F.Where(m => m.Item1 == teamE || m.Item2 == teamE).ToList();
                if (validOpponents.Count == 1)
                {
                    var matchPair = validOpponents[0];
                    if (matchPair.Item1 == teamE) hatFTeams.Remove(matchPair.Item2);
                    else hatETeams.Remove(matchPair.Item1);

                    mandatoryMatches.Add(matchPair);
                    possibleMatchesE_F.Remove(matchPair);
                }
            }

            foreach (var matchPair in mandatoryMatches)
            {
                quarterFinals.Add(Model.Match.Simulate(matchPair.Item1, matchPair.Item2));
            }

           
            hatDTeams = hatDTeams.Except(mandatoryMatches.Select(m => m.Item1)).ToList();
            hatGTeams = hatGTeams.Except(mandatoryMatches.Select(m => m.Item2)).ToList();
            hatETeams = hatETeams.Except(mandatoryMatches.Select(m => m.Item1)).ToList();
            hatFTeams = hatFTeams.Except(mandatoryMatches.Select(m => m.Item2)).ToList();

           
            while (possibleMatchesD_G.Count > 0 && hatDTeams.Count > 0 && hatGTeams.Count > 0)
            {
                var matchPair = SelectValidMatch(possibleMatchesD_G);
                if (matchPair != default && hatDTeams.Contains(matchPair.Item1) && hatGTeams.Contains(matchPair.Item2))
                {
                    quarterFinals.Add(Model.Match.Simulate(matchPair.Item1, matchPair.Item2));
                    hatDTeams.Remove(matchPair.Item1);
                    hatGTeams.Remove(matchPair.Item2);
                }
            }

            while (possibleMatchesE_F.Count > 0 && hatETeams.Count > 0 && hatFTeams.Count > 0)
            {
                var matchPair = SelectValidMatch(possibleMatchesE_F);
                if (matchPair != default && hatETeams.Contains(matchPair.Item1) && hatFTeams.Contains(matchPair.Item2))
                {
                    quarterFinals.Add(Model.Match.Simulate(matchPair.Item1, matchPair.Item2));
                    hatETeams.Remove(matchPair.Item1);
                    hatFTeams.Remove(matchPair.Item2);
                }
            }


            Console.WriteLine("\nCetvrtfinale:");
            foreach (var match in quarterFinals)
            {
                match.PrintKnockoutResult();
            }

           
            if (quarterFinals.Count < 4)
            {
                Console.WriteLine("Nema dovoljno timova da se kreiraju cetvrtfinala.");
                return;
            }

           
            var hatH = quarterFinals.Where(match => hatD.Contains(match.Team1) || hatG.Contains(match.Team1)).Select(match => match.WinningTeam).ToList();
            var hatI = quarterFinals.Where(match => hatE.Contains(match.Team1) || hatF.Contains(match.Team1)).Select(match => match.WinningTeam).ToList();

            
            var semiFinals = GenerateSemiFinals(hatH, hatI);

          
            Console.WriteLine("\nPolufinale:");
            foreach (var match in semiFinals)
            {
                match.PrintKnockoutResult();
            }

            GenerateFinalAndThirdPlaceMatches(semiFinals);
        }

        private (OlympicTeam, OlympicTeam) SelectValidMatch(List<(OlympicTeam, OlympicTeam)> matches)
        {
            var random = new Random();

            if (matches.Count == 0) return default;
            var matchIndex = random.Next(matches.Count);
            var matchPair = matches[matchIndex];
            matches.RemoveAt(matchIndex);
            return matchPair;
        }


        private bool HaveTeamsPlayedInGroupStage(OlympicTeam team1, OlympicTeam team2)
        {
            return Matches.Any(match =>
                (match.Team1.ISOCode == team1.ISOCode && match.Team2.ISOCode == team2.ISOCode) ||
                (match.Team1.ISOCode == team2.ISOCode && match.Team2.ISOCode == team1.ISOCode)
            );
        }

        private List<Model.Match> GenerateSemiFinals(List<OlympicTeam> hatH, List<OlympicTeam> hatI)
        {
            var random = new Random();
            var semiFinals = new List<Model.Match>();

            while (hatH.Any() && hatI.Any())
            {
                var teamH = hatH[random.Next(hatH.Count)];
                hatH.Remove(teamH);

                var teamI = hatI[random.Next(hatI.Count)];
                hatI.Remove(teamI);

                semiFinals.Add(Model.Match.Simulate(teamH, teamI));
            }

            return semiFinals;
        }

        private void GenerateFinalAndThirdPlaceMatches(List<Model.Match> semiFinals)
        {
            var finalMatch = Model.Match.Simulate(semiFinals[0].WinningTeam, semiFinals[1].WinningTeam);

            var thirdPlaceMatch = Model.Match.Simulate(semiFinals[0].LosingTeam, semiFinals[1].LosingTeam);

           
            Console.WriteLine("\nUtakmica za trece mesto:");
            thirdPlaceMatch.PrintKnockoutResult();

            Console.WriteLine("\nFinale:");
            finalMatch.PrintKnockoutResult();

            var goldMedalist = finalMatch.WinningTeam;
            var silverMedalist = finalMatch.LosingTeam;
            var bronzeMedalist = thirdPlaceMatch.WinningTeam;

            Console.WriteLine("\nMedalje:");
            Console.WriteLine($"1. {goldMedalist.Team}");
            Console.WriteLine($"2. {silverMedalist.Team}");
            Console.WriteLine($"3. {bronzeMedalist.Team}");
        }

    }
}

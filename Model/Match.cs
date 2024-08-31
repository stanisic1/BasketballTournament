using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasketballTournament.Model
{
    public class Match
    {
        public string GroupKey { get; set; }
        public OlympicTeam Team1 { get; set; }
        public OlympicTeam Team2 { get; set; }
        public int Team1Score { get; set; }
        public int Team2Score { get; set; }
        public OlympicTeam WinningTeam => Team1Score > Team2Score ? Team1 : Team2;
        public OlympicTeam LosingTeam => Team1Score < Team2Score ? Team1 : Team2;

        public static Match Simulate(OlympicTeam team1, OlympicTeam team2)
        {
            Random rand = new Random();

            double team1Probability = CalculateWinProbability(team1, team2);
            double team2Probability = 1.0 - team1Probability;

            double surrenderProbability = 0.03; 

            int baseScoreMin = 60;
            int baseScoreMax = 120;
            int team1Score = rand.Next(baseScoreMin, baseScoreMax);
            int team2Score = rand.Next(baseScoreMin, baseScoreMax);

       
            int rankingDifference = team2.FIBARanking - team1.FIBARanking;
            double rankingImpactFactor = Math.Abs(rankingDifference) / 40.0; 


            if (rand.NextDouble() < surrenderProbability)
            {           
                team1Score = 0;
                team2Score = 20; 
                team1.UpdateStats(0, 20, false, 0); 
                team2.UpdateStats(20, 0, true, 2); 
            }
            else if (rand.NextDouble() < surrenderProbability)
            {
               
                team2Score = 0;
                team1Score = 20; 
                team1.UpdateStats(20, 0, true, 2);  
                team2.UpdateStats(0, 20, false, 0); 
            }
            else
            {        
                if (team1Probability > team2Probability)
                {                 
                    team1Score += (int)(team1Score * rankingImpactFactor * team1Probability);
                    team2Score -= (int)(team2Score * rankingImpactFactor * team2Probability);
                }
                else
                {
                    team2Score += (int)(team2Score * rankingImpactFactor * team2Probability);
                    team1Score -= (int)(team1Score * rankingImpactFactor * team1Probability);
                }

              
                team1Score = Math.Max(baseScoreMin, Math.Min(baseScoreMax, team1Score));
                team2Score = Math.Max(baseScoreMin, Math.Min(baseScoreMax, team2Score));

               
                if (team1Score == team2Score)
                {
                    if (rand.NextDouble() < 0.5)
                        team1Score += 1;
                    else
                        team2Score += 1;
                }

                var match = new Match
                {
                    Team1 = team1,
                    Team2 = team2,
                    Team1Score = team1Score,
                    Team2Score = team2Score,
                };

               
                team1.UpdateStats(match.Team1Score, match.Team2Score, match.Team1Score > match.Team2Score, match.Team1Score > match.Team2Score ? 2 : 1);
                team2.UpdateStats(match.Team2Score, match.Team1Score, match.Team2Score > match.Team1Score, match.Team2Score > match.Team1Score ? 2 : 1);

                return match;
            }

            
            return new Match
            {
                Team1 = team1,
                Team2 = team2,
                Team1Score = team1Score,
                Team2Score = team2Score,
            };
        }


        private static double CalculateWinProbability(OlympicTeam team1, OlympicTeam team2)
        {
            double rankingDifference = team2.FIBARanking - team1.FIBARanking;

            double baseProbability = 0.5; 
            double probabilityAdjustment = Math.Clamp(rankingDifference / 100.0, -0.25, 0.25); 

            return baseProbability + probabilityAdjustment;
        }

        public void PrintResult(string groupName)
        {
            Console.WriteLine($"Grupa {groupName}: {Team1.Team} - {Team2.Team} ({Team1Score}:{Team2Score})");
        }

        public void PrintKnockoutResult()
        {
            Console.WriteLine($" {Team1.Team} - {Team2.Team} ({Team1Score}:{Team2Score})");
        }
    }
}

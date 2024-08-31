using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BasketballTournament.Model
{
    public class ExhibitionMatch
    {
        public string Team1 { get; set; }
        public string Team2 { get; set; }
        public string Result { get; set; }

        public static int GetScoreDifferenceForTeam(ExhibitionMatch match, string teamCode)
        {
            var scores = match.Result.Split('-').Select(int.Parse).ToArray();
            bool isTeam1 = match.Team1 == teamCode;

            int teamScore = isTeam1 ? scores[1] : scores[0];
            int opponentScore = isTeam1 ? scores[0] : scores[1];
          
            return teamScore - opponentScore; 
        }
    }
}

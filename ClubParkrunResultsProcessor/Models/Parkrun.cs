using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClubParkrunResultsProcessor.Models
{
    public class Parkrun
    {
        public string Title { get; set; }
        public string ParticpantsDescription { get; set; }
        public string LinkToParkrunResults { get; set; }
        public List<Result> Results { get; set; } = new List<Result>();
    }
}

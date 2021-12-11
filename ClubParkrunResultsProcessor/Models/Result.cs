using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClubParkrunResultsProcessor.Models
{
    public class Result
    {
        public int Position { get; set; }
        public int GenderPosition { get; set; }
        public string Name { get; set; }
        public string LinkToRunner{ get; set; }
        public string Club { get; set; }
        public string Time { get; set; }
    }
}

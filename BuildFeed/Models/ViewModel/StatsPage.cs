using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BuildFeed.Models.ViewModel
{
    public class StatsPage
    {
        public IEnumerable<MonthCount> AdditionsByMonth { get; set; }
        public IEnumerable<MonthCount> CompilesByMonth { get; set; }
        public IEnumerable<Tuple<string, int>> BuildsByLab { get; set; }
    }

    public struct MonthCount
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public double Count { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlusNine.Logic.Models
{
    public class ActivityCalendarEntry
    {
        public int Count { get; set; }
        public required string Date { get; set; }
        public int Level { get; set; }
    }

}

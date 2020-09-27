using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadarReader.Models
{
   public class TimeSlot
   {
      public string Start { get; set; }

      public string End { get; set; }

      [JsonIgnore]
      public DateTime StartDateTime { get; set; }

      [JsonIgnore]
      public DateTime EndDateTime { get; set; }

      public void Initialize()
      {
         var start = this.Start.Split(':');
         var end = this.End.Split(':');
         this.StartDateTime = new DateTime(1, 1, 1, Convert.ToInt32(start[0]), Convert.ToInt32(start[1]), 0);
         this.EndDateTime = new DateTime(1, 1, 1, Convert.ToInt32(end[0]), Convert.ToInt32(end[1]), 0);
      }
   }
}

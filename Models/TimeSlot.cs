using Newtonsoft.Json;
using System;

namespace RadarReader.Models
{
   public class TimeSlot
   {
      [JsonIgnore]
      public int StartNo { get; private set; }

      [JsonIgnore]
      public int EndNo { get; private set; }

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
         this.StartNo = Convert.ToInt32(this.StartDateTime.ToString("hhmm"));
         this.EndNo = Convert.ToInt32(this.EndDateTime.ToString("hhmm"));
      }

      //public bool IsWithinPeriod()
      //{
      //   var now = Convert.ToInt32(DateTime.Now.ToString("hhmm"));
      //   if (now <= EndNo && now >= StartNo) return true;
      //   else return false;
      //}
   }
}

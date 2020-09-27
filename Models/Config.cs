using Newtonsoft.Json;
using RadarReader.ViewModels;
using System.Collections.Generic;

namespace RadarReader.Models
{
   public class Config
   {
      public List<Radar> Radars { get; set; }

      public List<TimeSlot> TimeSlots { get; set; }
   }
}

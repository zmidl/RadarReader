using RadarReader.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;

namespace RadarReader.ViewModels
{
   public class MainWindowViewmodel:ViewModel
   {
      private readonly Timer t; 

      private List<DateTime> targets = new List<DateTime>();

      public Config Config { get; set; } = new Config();

      public object SelectedItems { get; set; }

      public RelayCommand Start => new RelayCommand((selectedItems) =>
      {
         foreach (var item in ((System.Collections.IList)selectedItems).Cast<Radar>())
         {
            item.Initialize();
            item.On();
         }
      });

      public RelayCommand Stop => new RelayCommand(() =>
      {
         this.Config.Radars.Where(o => o.IsStart == true).ToList().ForEach(o => o.Off());
      });


      public MainWindowViewmodel()
      {
         this.Config = Helper.Readjson<Config>();
         t = new Timer(this.DoWork, null, Timeout.InfiniteTimeSpan, TimeSpan.FromMinutes(1));
      }

      private void DoWork(object param)
      {

      }
   }
}

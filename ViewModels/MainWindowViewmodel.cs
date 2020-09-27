using RadarReader.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;

namespace RadarReader.ViewModels
{
   public class MainWindowViewmodel : ViewModel
   {
      public string Title { get; set; }
      private object selectedItems;
      public DateTime current;

      private readonly Timer t;

      private List<DateTime> targets = new List<DateTime>();

      public Config Config { get; set; } = new Config();

      public object SelectedItems { get; set; }

      public RelayCommand AutoExecute => new RelayCommand((selectedItems) =>
      {
         this.selectedItems = selectedItems;
         t.Change(TimeSpan.Zero, TimeSpan.FromMinutes(1));
         if (this.targets?.Count > 0) this.current = this.targets[0];
      });

      public RelayCommand CancelExecute => new RelayCommand(() =>
      {
         t.Change(Timeout.Infinite, Timeout.Infinite);
         this.StopExecute.Execute(null);
      });

      public RelayCommand StartExecute => new RelayCommand((selectedItems) => this.Start(selectedItems));

      public RelayCommand StopExecute => new RelayCommand(() =>
      {
         this.Config.Radars.Where(o => o.IsStart == true).ToList().ForEach(o => o.Off());
      });

      public MainWindowViewmodel()
      {
         this.Config = Helper.Readjson<Config>();
         this.Config.TimeSlots.ForEach(o => o.Initialize());
         foreach (var item in this.Config.TimeSlots)
         {
            this.targets.Add(item.StartDateTime);
            this.targets.Add(item.EndDateTime);
         }
         t = new Timer(this.DoWork, this.selectedItems, Timeout.InfiniteTimeSpan, TimeSpan.FromMinutes(1));
      }

      private void Start(object selectedItems)
      {
         foreach (var item in ((System.Collections.IList)selectedItems).Cast<Radar>())
         {
            item.Initialize();
            item.On();
         }
      }

      private bool Switch()
      {
         var currerntIndex = this.targets.IndexOf(this.current);
         if (++currerntIndex > this.targets.Count - 1) currerntIndex = 0;
         this.current = this.targets[currerntIndex];
         return currerntIndex % 2 == 0 ? true : false;
      }

      private void DoWork(object selectedItems)
      {
         if (DateTime.Now.ToString("hhmm") == this.current.ToString("hhmm"))
         {
            if (this.Switch()) this.Start(selectedItems);
            else this.StopExecute.Execute(null);
         }

         //if (DateTime.Now.ToString("hhmm") == this.current.ToString("hhmm"))
         //{
         //   App.Current.Dispatcher.Invoke(() => 
         //   {
         //      if (this.Switch()) this.Title +=$"{ DateTime.Now:hhmm}--开";
         //      else this.Title += $"{ DateTime.Now:hhmm}--关";
         //      this.RaiseProperty(this.Title);
         //   });

         //}
      }
   }
}

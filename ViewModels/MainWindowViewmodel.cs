using RadarReader.Models;
using System;
using System.Linq;
using System.Threading;
using Xceed.Wpf.Toolkit;

namespace RadarReader.ViewModels
{
   public class MainWindowViewmodel : ViewModel
   {
      public string Title { get; set; } = string.Empty;

      private object selectedItems;

      private bool isUnattendedRunning;

      private bool isUnattended = false;

      /// <summary>
      /// 自动采集标帜
      /// </summary>
      public bool IsUnattended
      {
         get => this.isUnattended;
         set
         {
            this.isUnattended = value;
            this.RaiseProperty(nameof(IsUnattended));
         }
      }

      private bool isManual;
      /// <summary>
      /// 手动采集标帜
      /// </summary>
      public bool IsManual
      {
         get => this.isManual;
         set
         {
            this.isManual = value;
            this.RaiseProperty(nameof(this.IsManual));
         }
      }

      /// <summary>
      /// 下一个自动采集时间
      /// </summary>
      public TimeSlot nextTimeSlot;

      /// <summary>
      /// 时段计时器
      /// </summary>
      private readonly Timer timer;

      ///// <summary>
      ///// 所有自动采集的时间集合
      ///// </summary>
      //private readonly List<DateTime> timeSlots = new List<DateTime>();

      public Config Config { get; set; } = new Config();

      /// <summary>
      /// 自动采集
      /// </summary>
      public RelayCommand AutoExecute => new RelayCommand((selectedItems) =>
      {
         if (((System.Collections.IList)selectedItems).Cast<Radar>().Count() > 0)
         {
            this.selectedItems = selectedItems;
            this.CalcTimeSlot();
            this.isUnattendedRunning = false;
            this.IsUnattended = true;
            timer.Change(TimeSpan.Zero, TimeSpan.FromMinutes(1));
         }
         else MessageBox.Show("至少选择1个雷达");
      });

      /// <summary>
      /// 取消自动采集
      /// </summary>
      public RelayCommand CancelExecute => new RelayCommand(() =>
      {
         this.IsUnattended = false;
         timer.Change(Timeout.Infinite, Timeout.Infinite);
         this.StopExecute.Execute(null);
      });

      /// <summary>
      /// 手动采集
      /// </summary>
      public RelayCommand StartExecute => new RelayCommand((selectedItems) =>
      {
         if (((System.Collections.IList)selectedItems).Cast<Radar>().Count() > 0)
         {
            this.IsManual = true;
            this.Start(selectedItems);
         }
         else MessageBox.Show("至少选择1个雷达");
      });


      /// <summary>
      /// 手动停止
      /// </summary>
      public RelayCommand StopExecute => new RelayCommand(() =>
      {
         this.IsManual = false;
         this.Config.Radars.Where(o => o.IsStart == "已连接").ToList().ForEach(o => o.Off());
      });

      public MainWindowViewmodel()
      {
         this.Config = Helper.Readjson<Config>();
         this.Config.TimeSlots.ForEach(o => o.Initialize());
         timer = new Timer(this.DoWork, this.selectedItems, Timeout.InfiniteTimeSpan, TimeSpan.FromMinutes(1));
      }

      /// <summary>
      /// 查找下一个时间段
      /// </summary>
      /// <returns></returns>
      private void CalcTimeSlot()
      {
         var now = Convert.ToInt32(DateTime.Now.ToString("hhmm"));
         if (now > this.Config.TimeSlots.Last().StartNo) this.nextTimeSlot = this.Config.TimeSlots.First();
         else
         {
            for (int i = this.Config.TimeSlots.Count - 1; i >= 0; i--)
            {
               if (now < this.Config.TimeSlots[i].StartNo) this.nextTimeSlot = this.Config.TimeSlots[i];
               else break;
            }
         }
      }

      /// <summary>
      /// 自动推进下一个时间段
      /// </summary>
      private void PushTimeSlot()
      {
         if (this.nextTimeSlot == this.Config.TimeSlots.Last()) this.nextTimeSlot = this.Config.TimeSlots.First();
         else this.nextTimeSlot = this.Config.TimeSlots[this.Config.TimeSlots.IndexOf(this.nextTimeSlot) + 1];
      }

      /// <summary>
      /// 启动选中的雷达1~N
      /// </summary>
      /// <param name="selectedItems"></param>
      private void Start(object selectedItems)
      {
         foreach (var item in ((System.Collections.IList)selectedItems).Cast<Radar>())
         {
            item.Initialize(this.Config.Threshold);
            item.On();
         }
      }

      /// <summary>
      /// 计时器回调函数
      /// </summary>
      /// <param name="selectedItems"></param>
      private void DoWork(object selectedItems)
      {
         var now = Convert.ToInt32(DateTime.Now.ToString("hhmm"));

         if (this.isUnattendedRunning == false)
         {
            if (now == this.nextTimeSlot.StartNo)
            {
               this.isUnattendedRunning = true;
               App.Current.Dispatcher.Invoke(() => this.Start(this.selectedItems));
            }
         }
         else
         {
            if (now == this.nextTimeSlot.EndNo)
            {
               this.isUnattendedRunning = false;
               App.Current.Dispatcher.Invoke(() => this.StopExecute.Execute(null));
               this.PushTimeSlot();
            }
         }
      }
   }
}

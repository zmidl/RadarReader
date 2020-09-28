using Newtonsoft.Json;
using RadarReader.Models;
using System;
using System.Collections.Generic;
using System.Net;

namespace RadarReader.ViewModels
{
   public class Radar : Notify
   {
      public string Ip { get; set; }

      public int Port { get; set; }

      [JsonIgnore]
      public int ReceivedPockage { get; set; }

      [JsonIgnore]
      public bool IsStart { get; private set; }

      private int threshold = 0;

      private readonly List<string> columnsName = new List<string> { nameof(RowModel.ID), nameof(RowModel.Length), nameof(RowModel.SpeedY), nameof(RowModel.SpeedX), nameof(RowModel.PointY), nameof(RowModel.PointY),"Time" };

      public event EventHandler<List<string>> Received;

      [JsonIgnore]
      public List<string> Data { get; set; } = new List<string>();

      private SyncTcpClient client;

      public Radar()
      {
         //this.client = new SyncTcpClient(new IPEndPoint(IPAddress.Parse(ipAddress), port));
         //this.client.Received += Client_Received;
      }

      public void Initialize()
      {
         this.client = new SyncTcpClient(new IPEndPoint(IPAddress.Parse(this.Ip), this.Port));
         this.client.Received += Client_Received;
      }

      private void Client_Received(object sender, NotifyEventArgs e)
      {
         this.Data.AddRange(this.Analysis(e.Message as List<byte[]>));
         if (++this.threshold >= 250)
         {
            //App.Current.Dispatcher.InvokeAsync(()=>Helper.SaveCSV(this.columnsName, this.Data, $"C:\\Users\\赵敏\\Desktop\\bb", $"aa{DateTime.Now:yyyy_MM_dd_hh_mm}.csv"));
            Helper.SaveCSV(this.columnsName, this.Data, $"{Environment.CurrentDirectory}\\{this.Ip}_{this.Port}", $"{DateTime.Now:MM_dd hh_mm}.csv");
            this.threshold = 0;
            this.Data.Clear();
         }
      }

      private IEnumerable<string> Analysis(List<byte[]> source)
      {
         foreach (var item in source) yield return new RowModel(item).ToString();
      }

      public void On()
      {
         this.client.Connect();
         this.IsStart = true;
      }

      public void Off()
      {
         this.client.Disconnect();
         this.IsStart = false;
      }

      public void Send(byte[] data = null)
      {
         if (data == null) data = new byte[] { 0xFF };
         this.client.Send(data);
      }
   }

}

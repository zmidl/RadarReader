using Newtonsoft.Json;
using RadarReader.Models;
using System;
using System.Collections.Generic;
using System.Net;

namespace RadarReader.ViewModels
{
   public class Radar : Notify
   {
      /// <summary>
      /// 雷达IP地址
      /// </summary>
      public string Ip { get; set; }

      /// <summary>
      /// 雷达端口
      /// </summary>
      public int Port { get; set; }

      /// <summary>
      /// 已接收帧
      /// </summary>
      [JsonIgnore]
      public int ReceivedPockage { get; set; }

      /// <summary>
      /// 是否连接
      /// </summary>
      [JsonIgnore]
      public string IsStart { get; private set; } = "未连接";

      /// <summary>
      /// 缓存容量（决定是否保存当前缓存数据并清空）
      /// </summary>
      private int threshold = 0;

      private readonly List<string> columnsName = new List<string> { nameof(RowModel.ID), nameof(RowModel.Length), nameof(RowModel.SpeedY), nameof(RowModel.SpeedX), nameof(RowModel.PointY), nameof(RowModel.PointY),"Time" };

      [JsonIgnore]
      public List<string> Data { get; set; } = new List<string>();

      private SyncTcpClient client;

      public Radar() { }

      /// <summary>
      /// 初始化雷达对象
      /// </summary>
      public void Initialize()
      {
         this.client = new SyncTcpClient(new IPEndPoint(IPAddress.Parse(this.Ip), this.Port));
         this.client.Received += Client_Received;
      }

      /// <summary>
      /// 接收到数据包
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void Client_Received(object sender, NotifyEventArgs e)
      {
         this.ReceivedPockage++;
         this.RaiseProperty(nameof(this.ReceivedPockage));
         this.Data.AddRange(this.Analysis(e.Message as List<byte[]>));
         if (++this.threshold >= 300)
         {
            Helper.SaveCSV(this.columnsName, this.Data, $"{Environment.CurrentDirectory}\\{this.Ip}_{this.Port}", $"{DateTime.Now:MM_dd hh_mm}.csv");
            this.threshold = 0;
            this.Data.Clear();
         }
      }

      /// <summary>
      /// 数据包解析为多条有效信息
      /// </summary>
      /// <param name="source">数据源</param>
      /// <returns></returns>
      private IEnumerable<string> Analysis(List<byte[]> source)
      {
         foreach (var item in source) yield return new RowModel(item).ToString();
      }

      /// <summary>
      /// 开启雷达数据采集
      /// </summary>
      public void On()
      {
         this.client.Connect();
         this.IsStart = "已连接";
         this.RaiseProperty(nameof(this.IsStart));
      }

      /// <summary>
      /// 关闭雷达数据采集
      /// </summary>
      public void Off()
      {
         this.client.Disconnect();
         this.IsStart = "已断开";
         this.RaiseProperty(nameof(this.IsStart));
      }

      /// <summary>
      /// 发送
      /// </summary>
      /// <param name="data"></param>
      public void Send(byte[] data = null)
      {
         if (data == null) data = new byte[] { 0xFF };
         this.client.Send(data);
      }
   }

}

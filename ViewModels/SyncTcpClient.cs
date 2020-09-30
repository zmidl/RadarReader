using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace RadarReader.ViewModels
{
   public class SyncTcpClient
   {
      private int survivalDegree;
      private int health;
      private readonly int WAEWOULDBLOCK = 10035;
      private bool isAutoReconnect;
      private bool isData;
      private readonly byte[] zeroData = new byte[0];
      private readonly byte[] head = new byte[] { 0xCA, 0xCB, 0xCC, 0xCD };
      private readonly byte[] tail = new byte[] { 0XEA, 0XEB, 0XEC, 0XED, 0X61, 0X67, 0X5C, 0X14, 0X89, 0XC6 };
      private int processingLength = 1;
      private readonly List<byte> processingCache = new List<byte>();
      private readonly List<byte[]> outputData = new List<byte[]>();
      private int processingStep = 0;
      private int Tag;
      private TcpClient tcpClient;

      private BinaryReader binaryReader;

      private BinaryWriter binaryWriter;

      private readonly IPEndPoint serverIpEndPoint;

      private bool isInvokeDisconnection;

      private bool isTaskCancel;

      private readonly int receivingLength;

      public bool IsConnected { get; set; }

      //private readonly List<byte> bytesPool = new List<byte>();

      //private readonly Task processingTask;

      //private bool isTaskCancel;

      public event EventHandler<NotifyEventArgs> ErrorThrown;

      public event EventHandler<NotifyEventArgs> Connected;

      public event EventHandler<NotifyEventArgs> Disconnected;

      public event EventHandler<NotifyEventArgs> Received;

      public event EventHandler<NotifyEventArgs> Sent;

      public SyncTcpClient(IPEndPoint ipEndPoint, int receivingLength = 1024, bool isAutoReconnect = true, int survivalDegree = 10)
      {


         this.tcpClient = new TcpClient();

         this.tcpClient.Client.DontFragment = true;

         this.serverIpEndPoint = ipEndPoint;

         this.receivingLength = receivingLength;

         this.isAutoReconnect = isAutoReconnect;

         this.survivalDegree = survivalDegree;
      }

      public void Connect()
      {
         //if (this.tcpClient == null) this.tcpClient = new TcpClient();

         try
         {
            this.tcpClient?.BeginConnect(this.serverIpEndPoint.Address, this.serverIpEndPoint.Port, this.AsyncConnect, this.tcpClient);
         }
         catch (Exception ex)
         {
            if (this.isAutoReconnect)
            {
               this.tcpClient = new TcpClient();
               this.tcpClient?.BeginConnect(this.serverIpEndPoint.Address, this.serverIpEndPoint.Port, this.AsyncConnect, this.tcpClient);
            }
         }
      }

      private void AsyncConnect(IAsyncResult asyncResult)
      {
         try
         {
            if (tcpClient.Connected)
            {
               this.isInvokeDisconnection = false;
               var stream = this.tcpClient.GetStream();
               this.binaryReader = new BinaryReader(stream);
               this.binaryWriter = new BinaryWriter(stream);
               Thread ReceiveBytesThread = new Thread(new ThreadStart(this.Receive)) { IsBackground = true };
               ReceiveBytesThread.Start();
               this.Connected?.Invoke(this, null);
               this.Send(new byte[] { 0xFF });
            }
            else { }
         }
         catch (ArgumentNullException argumentNullException) { throw argumentNullException; }
         catch (ArgumentException argumentException) { throw argumentException; }
         catch (IndexOutOfRangeException indexOutOfRangeException) { throw indexOutOfRangeException; }
      }

      public bool Disconnect()
      {
         bool result = false;
         try
         {
            this.isInvokeDisconnection = true;
            result = true;
         }
         catch
         {

         }
         return result;
      }

      private void ResetCache()
      {
         this.processingStep = 0;
         this.processingLength = 1;
         this.processingCache.Clear();
         this.outputData.Clear();
         this.binaryReader.BaseStream.Flush();
      }

      private void Receive()
      {
         try
         {
            while (this.isInvokeDisconnection == false)
            {
               byte[] readBytes = this.binaryReader.ReadBytes(this.processingLength);
               if (readBytes.Length == 0)
               {
                  this.OnDisconnect("传感器设置断开连接");
                  this.isInvokeDisconnection = true;
               }
               else
               {
                  this.processingCache.AddRange(readBytes);
                  if (this.processingStep == 0)
                  {
                     if (this.processingCache[0] == this.head[0])
                     {
                        if (this.processingCache.Count == this.head.Length)
                        {
                           if (Enumerable.SequenceEqual(readBytes, this.processingCache.Skip(1)))
                           {
                              this.processingStep = 1;
                              this.processingCache.Clear();
                              this.processingLength = 3;
                           }
                           else
                           {
                              this.processingCache.RemoveAt(0);
                              this.processingLength = 1;
                           }
                        }
                        else
                        {
                           if (this.processingCache.Count < this.head.Length) this.processingLength = this.head.Length - this.processingCache.Count;
                           else
                           {
                              this.processingCache.Clear();
                              this.processingLength = 1;
                           }
                        }
                     }
                     else
                     {
                        this.processingCache.RemoveAt(0);
                        this.processingLength = 4;
                     }
                  }
                  else if (this.processingStep == 1)
                  {
                     if (readBytes.Length == 3)
                     {
                        if (readBytes[0] == 0x05 || readBytes[0] == 0x06) this.processingLength =8;
                        else
                        {
                           if (Enumerable.SequenceEqual(readBytes, this.tail.Take(3)))
                           {
                              this.processingStep = 2;
                              this.processingLength = 7;
                           }
                        }
                     }
                     else
                     {
                        this.processingLength = 3;
                        this.outputData.Add(readBytes);
                     }
                  }
                  else
                  {
                     if (Enumerable.SequenceEqual(readBytes, this.tail.Skip(3))) this.OnReceived(this.outputData);
                     ResetCache();
                  }
               }
            }
            this.binaryReader?.Close();
            this.binaryWriter?.Close();
            this.tcpClient?.Close();
         }
         catch (Exception exception)
         {
            this.isInvokeDisconnection = true;
            switch (exception)
            {
               case var type when type is IOException:
                  {
                     this.OnDisconnect(exception.Message);
                     break;
                  }
               case var type when type is ArgumentException:
                  {
                     this.OnDisconnect(exception.Message);
                     break;
                  }
               case var type when type is ArgumentOutOfRangeException:
                  {
                     this.OnDisconnect(exception.Message);
                     break;
                  }
               case var type when type is ObjectDisposedException:
                  {
                     this.OnDisconnect(exception.Message);
                     break;
                  }
               default: { this.OnError(exception.Message); break; }
            }
         }
         finally { }
      }

      //private void Receive2()
      //{
      //   try
      //   {
      //      while (this.isInvokeDisconnection == false)
      //      {
      //         byte[] readBytes = this.binaryReader.ReadBytes(5000);
      //         if (readBytes.Length == 0)
      //         {
      //            this.OnDisconnect("传感器设置断开连接");
      //            this.isInvokeDisconnection = true;
      //         }
      //         else
      //         {
      //            this.OnReceived(readBytes);
      //            this.isInvokeDisconnection = true;
      //         }
      //      }
      //      this.binaryReader?.Close();
      //      this.binaryWriter?.Close();
      //      this.tcpClient?.Close();
      //   }
      //   catch (Exception exception)
      //   {
      //      this.isInvokeDisconnection = true;
      //      switch (exception)
      //      {
      //         case var type when type is IOException:
      //            {
      //               this.OnDisconnect(exception.Message);
      //               break;
      //            }
      //         case var type when type is ArgumentException:
      //            {
      //               this.OnDisconnect(exception.Message);
      //               break;
      //            }
      //         case var type when type is ArgumentOutOfRangeException:
      //            {
      //               this.OnDisconnect(exception.Message);
      //               break;
      //            }
      //         case var type when type is ObjectDisposedException:
      //            {
      //               this.OnDisconnect(exception.Message);
      //               break;
      //            }
      //         default: { this.OnError(exception.Message); break; }
      //      }
      //   }
      //   finally { }
      //}

      public void Send(byte[] byteArray)
      {
         if (this.isInvokeDisconnection == false)
         {
            try
            {
               this.binaryWriter.Write(byteArray);
               this.binaryWriter.Flush();
            }
            catch (Exception exception)
            {
               switch (exception)
               {
                  case var type when type is IOException:
                     {
                        this.OnDisconnect(exception.Message);
                        break;
                     }
                  case var type when type is ArgumentNullException:
                     {
                        this.OnDisconnect(exception.Message);
                        break;
                     }

                  case var type when type is ObjectDisposedException:
                     {
                        this.OnDisconnect(exception.Message);
                        break;
                     }
                  default: { this.OnError(exception.Message); break; }
               }
            }
         }
      }

      public void Clear()
      {
         this.ResetCache();
      }

      private bool HealthCountdown() => this.health-- <= 0 ? true : false;

      private void RecoveryHealth() => this.health = this.survivalDegree;

      private void BeginSurvive()
      {
         Task.Factory.StartNew(() =>
         {
            while (true)
            {
               this.binaryWriter.BaseStream.BeginWrite(this.zeroData, 0, this.zeroData.Length, new AsyncCallback(this.EndSurvive), this.tcpClient);
               Task.Delay(TimeSpan.FromSeconds(5)).Wait();
            }
         });
      }

      private void EndSurvive(IAsyncResult asyncResult)
      {
         var isSurvival = false;
         try { isSurvival = true; }
         catch (Exception ex)
         {
            if (ex.HResult == this.WAEWOULDBLOCK) isSurvival = true;
         }

         if (isSurvival) this.RecoveryHealth();
      }

      public IPEndPoint GetIp() => this.serverIpEndPoint;

      public virtual void OnSent(object parameter) => this.Sent?.Invoke(this, new NotifyEventArgs(this.tcpClient, parameter));

      public virtual void OnReceived(object parameter) => this.Received?.Invoke(this, new NotifyEventArgs(this.tcpClient, parameter));

      public virtual void OnError(object parameter) => this.ErrorThrown?.Invoke(this, new NotifyEventArgs(this.tcpClient, parameter));

      public virtual void OnConnect(object parameter) => this.Connected?.Invoke(this, new NotifyEventArgs(this.tcpClient, parameter));

      public virtual void OnDisconnect(object parameter) => this.Disconnected?.Invoke(this, new NotifyEventArgs(this.tcpClient, parameter));
   }

   public class NotifyEventArgs : EventArgs
   {
      public TcpClient TcpClient { get; set; }
      public object Message { get; set; }
      public NotifyEventArgs(TcpClient TcpClient, object message)
      {
         this.TcpClient = TcpClient;
         this.Message = message;
      }
   }
}

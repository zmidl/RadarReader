using System;
using System.Linq;
using System.Text;

namespace RadarReader.Models
{
   public class RowModel
   {

      public string ID { get; set; }

      public string Length { get; set; }

      public string SpeedX { get; set; }

      public string SpeedY { get; set; }

      public string PointX { get; set; }

      public string PointY { get; set; }

      public RowModel(string id, string length, string sx, string sy, string px, string py)
      {
         this.ID = id;
         this.Length = length;
         this.SpeedX = sx;
         this.SpeedY = sy;
         this.PointX = px;
         this.PointY = py;
      }

      public RowModel(byte[] data)
      {
         StringBuilder sb = new StringBuilder(8);
         foreach (var item in data.Skip(data.Length - 8))
         {
            sb.Append(Convert.ToString(item, 2).PadLeft(8, '0'));
         }
         var bin = sb.ToString();
         var xPointBin = bin.Substring(50, 13);
         var yPointBin = bin.Substring(37, 13);
         var xSpeedBin = bin.Substring(26, 11);
         var ySpeedBin = bin.Substring(15, 11);
         var lengthBin = bin.Substring(8, 7);
         var idBin = bin.Substring(0, 8);

         this.ID = Convert.ToInt32(idBin, 2).ToString();
         this.Length = (Convert.ToInt32(lengthBin, 2) * 0.2).ToString("0.0");
         this.PointX = ((Convert.ToInt32(xPointBin, 2) - 4096) * 0.128).ToString("0.000");
         this.PointY = ((Convert.ToInt32(yPointBin, 2) - 4096) * 0.128).ToString("0.000");
         this.SpeedX = ((Convert.ToInt32(xSpeedBin, 2) - 1024) * 0.36).ToString("0.00");
         this.SpeedY = ((Convert.ToInt32(ySpeedBin, 2) - 1024) * 0.36).ToString("0.00");
         //Console.WriteLine($"id:{this.ID}--length:{this.Length}--pointX:{this.PointX}--pointY:{this.PointY}--speedX:{this.SpeedX}--speedY:{this.SpeedY}");
      }

      public override string ToString() => $"{this.ID},{this.Length},{this.SpeedY},{this.SpeedX},{this.PointY},{this.PointX},{DateTime.Now.ToString("hh:mm")}";

   }
}

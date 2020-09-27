using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Documents;

namespace RadarReader
{
   public class Helper
   {
      public static string PrintHexString(byte[] data, string separator = null)
      {
         var result = new StringBuilder();
         if (separator == null) separator = string.Empty;
         for (int i = 0; i < data.Length; i++)
         {
            if (i == data.Length - 1) separator = string.Empty;
            result.AppendFormat("{0:x2}{1}", data[i], separator);
         }
         return result.ToString();
      }

      public static string Serialize<T>(T t) => JsonConvert.SerializeObject(t);

      public static T Deserialize<T>(string json) => JsonConvert.DeserializeObject<T>(json);

      /// <summary>
      /// 将DataTable中数据写入到CSV文件中
      /// </summary>
      /// <param name="dt">提供保存数据的DataTable</param>
      /// <param name="fileName">CSV的文件路径</param>
      public static void SaveCSV(List<string> columns, List<string> source, string fullPath,string fileName)
      {
         if (!Directory.Exists(fullPath))Directory.CreateDirectory(fullPath);
         //FileInfo fileInfo = new FileInfo($"{fullPath}\\{fileName}");
         //if (!fileInfo.Exists) fileInfo.Create();
         //var bbb = $"{fullPath}\\{fileName}";
         using (FileStream fileStream = new FileStream($"{fullPath}\\{fileName}", FileMode.OpenOrCreate, FileAccess.ReadWrite))
         {
            using (StreamWriter sw = new StreamWriter(fileStream, Encoding.UTF8))
            {
               sw.WriteLine(string.Join(",", columns));
               for (int i = 0; i < source.Count; i++) sw.WriteLine(source[i]);
            }
         }
      }

      public static T Readjson<T>()
      {
         string fileName = $"{Environment.CurrentDirectory}\\Config.json";
         return Deserialize<T>(File.ReadAllText(fileName, Encoding.Default));
      }
   }
}

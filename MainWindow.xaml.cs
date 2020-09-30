using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RadarReader
{
   /// <summary>
   /// MainWindow.xaml 的交互逻辑
   /// </summary>
   public partial class MainWindow : Window
   {
      public MainWindow()
      {
         InitializeComponent();
      }
   }

   public class UnattendedToVisible : IValueConverter
   {
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         return System.Convert.ToBoolean(value) ? Visibility.Visible : Visibility.Hidden;
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
         throw new NotImplementedException();
      }
   }

   public class BoolToReverse : IValueConverter
   {
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         return !System.Convert.ToBoolean(value);
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
         throw new NotImplementedException();
      }
   }
}

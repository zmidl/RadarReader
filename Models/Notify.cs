using System.ComponentModel;
using System.Linq;

namespace RadarReader.Models
{
   public abstract class Notify : INotifyPropertyChanged
   {
      public event PropertyChangedEventHandler PropertyChanged;

      protected void RaiseProperty(object property)
      {
         var name = this.GetType().GetProperties().FirstOrDefault(o => o.Equals(property))?.Name;
         this.RaiseProperty(name);
      }

      protected void RaiseProperty(string propertyName)
      {
         this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
      }

      protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
      {
         this.PropertyChanged?.Invoke(this, e);
      }
   }
}

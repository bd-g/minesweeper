using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Minesweeper.Utils {
  public class CustomNotifyPropertyChanged: INotifyPropertyChanged
  {
    public virtual event PropertyChangedEventHandler PropertyChanged;
 
    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

  }
}

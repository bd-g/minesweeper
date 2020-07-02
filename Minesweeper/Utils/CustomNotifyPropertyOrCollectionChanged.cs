using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Minesweeper.Utils {
  public class CustomNotifyPropertyOrCollectionChanged : INotifyPropertyChanged, INotifyCollectionChanged
  {
    public virtual event PropertyChangedEventHandler PropertyChanged;
    public virtual event NotifyCollectionChangedEventHandler CollectionChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
      CollectionChanged?.Invoke(this, e);
    }
  }
}

using Minesweeper.Utils;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Collections.Specialized;

//TODO - decide how to implement selecting a tile. keep in mind checking for losing/winning, and chain reactions off of 
// stuff, like labeling how many mines are nearby
namespace Minesweeper.GamePlay {
    #region Classes
  public class GameTileModel {
    private readonly Guid guid;
    public GameTileModel(int row, int col, bool IsMine = false, bool IsSelected = false, bool IsFlagged = false) {
      this.Row = row;
      this.Col = col;
      this.IsMine = IsMine;
      this.IsSelected = IsSelected;
      this.IsFlagged = IsFlagged;
      guid = Guid.NewGuid();
    }
    public int Row { get; }
    public int Col { get; }
    public bool IsMine { get; }
    public bool IsSelected { get; }
    public bool IsFlagged { get; }

    public Guid TileIdentifier { get { return guid;  } }

    public override string ToString()
    {
      return String.Format("GameTileModel {{ IsMine : %s, IsFlagged : %s, IsSelected: %s }}", IsMine.ToString(), IsFlagged.ToString(), IsSelected.ToString());
    }
  }
  #endregion
}

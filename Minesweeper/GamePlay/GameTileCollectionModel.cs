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
  public class GameTileCollectionModel : CustomNotifyPropertyOrCollectionChanged {
    #region Fields
    private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
    private List<List<GameTileModel>> prTileList;
    #endregion

    #region Constructors
    public GameTileCollectionModel(int boardHeight = 10, int boardWidth = 10, int numMines = 10)
    {
      prTileList = new List<List<GameTileModel>>();
      for (int i = 0; i < boardHeight; i++) {
        prTileList.Add(new List<GameTileModel>());
        for (int j = 0; j < boardWidth; j++) {
          prTileList[i].Add(null);
        }
      }

      List<int> tileNumbers = Enumerable.Range(0, (boardHeight * boardWidth - 1)).ToList();
      Random rand = new Random();

      for (int i = 0; i < numMines; i++) {
        int index = rand.Next(0, tileNumbers.Count - 1);
        int randNumber = tileNumbers[index];
        prTileList[(randNumber / boardWidth)][(randNumber % boardWidth)] = new GameTileModel(true, false, false);
        tileNumbers.RemoveAt(index);
      }

      for (int i = 0; i < boardHeight; i++) {
        for (int j = 0; j < boardWidth; j++) {
          if (prTileList[i][j] == null) {
            prTileList[i][j] = new GameTileModel();
          }
        }
      }      
    }
    #endregion

    #region Properties
    public List<List<GameTileModel>> TileArray { get { return prTileList; } }
    #endregion

    #region Public Methods
    /// <summary>
    /// Method that enables changes to a game tile, whether it is being selected or flagged
    /// </summary>
    /// <param name="row">Row in game board of tile to be changed</param>
    /// <param name="col">Row in game board of tile to be changed</param>
    /// <param name="isSwitchingFlaggedStatus">Boolean indicating whether the flag status should change</param>
    /// <param name="isMineBeingSelected">Boolean indicating whether the tile has been selected by the user to reveal</param>
    public void AlterGameTile(Guid identifier, bool isSwitchingFlaggedStatus, bool isMineBeingSelected) 
    {
      GameTileModel oldTile = null;
      int row = -1;
      int col = -1;
      for (int i = 0; i < prTileList.Count; i++) {
        for (int j = 0; j < prTileList[i].Count; j++) {
          if (prTileList[i][j].TileIdentifier.Equals(identifier)) {
            oldTile = prTileList[i][j];
            row = i;
            col = j;
            break;
          }
        }
        if (oldTile != null) {
          break;
        }
      }

      if (oldTile == null) {
        throw new ArgumentNullException("Could not find specified tile");
      }

      bool newTileIsMine = oldTile.IsMine;
      bool newTileIsFlagged = isSwitchingFlaggedStatus ^ oldTile.IsFlagged;
      bool newTileIsSelected = oldTile.IsSelected || isMineBeingSelected;

      if ((oldTile.IsFlagged != newTileIsFlagged) || (oldTile.IsSelected != newTileIsSelected)) {
        var newTile = new GameTileModel(newTileIsMine, newTileIsSelected, newTileIsFlagged);
        prTileList[row][col] = newTile;
        int index = (prTileList[0].Count * row) + col;
        logger.Trace("Gametile altered at [%d,%d], index %d\nOld Tile - %s\nNewTile - %s", row, col, index, oldTile.ToString(), newTile.ToString());
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, prTileList[row][col], oldTile, index));
      }     
    }
    #endregion
  }

  #region Helper Classes
  public class GameTileModel {
    private readonly Guid guid;
    public GameTileModel(bool IsMine = false, bool IsSelected = false, bool IsFlagged = false) {
      this.IsMine = IsMine;
      this.IsSelected = IsSelected;
      this.IsFlagged = IsFlagged;
      guid = Guid.NewGuid();
    }
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Minesweeper.GamePlay
{
  [Obsolete("Previous attempt at GameTileCollection", true)]
  public class GameTileCollectionModel
  {
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
        prTileList[(randNumber / boardWidth)][(randNumber % boardWidth)] = new GameTileModel(0, 0, true, false, false);
        tileNumbers.RemoveAt(index);
      }

      for (int i = 0; i < boardHeight; i++) {
        for (int j = 0; j < boardWidth; j++) {
          if (prTileList[i][j] == null) {
            prTileList[i][j] = new GameTileModel(0, 0);
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
        var newTile = new GameTileModel(0, 0, newTileIsMine, newTileIsSelected, newTileIsFlagged);
        prTileList[row][col] = newTile;
        int index = (prTileList[0].Count * row) + col;
        logger.Trace("Gametile altered at [%d,%d], index %d\nOld Tile - %s\nNewTile - %s", row, col, index, oldTile.ToString(), newTile.ToString());
      }
    }
    #endregion
  }
}

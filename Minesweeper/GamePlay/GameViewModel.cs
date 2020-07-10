using Minesweeper.GamePlay;
using Minesweeper.Utils;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Minesweeper.GamePlay {
  #region Outer Enums
  public enum ViewState
  {
    Menu,
    Game
  }

  public enum GameDifficulty
  {
    Beginner,
    Intermediate,
    Expert
  }

  public enum GameStatus
  {
    NotStarted,
    InProgress,
    Won,
    Lost
  }
  #endregion
  public class GameViewModel : CustomNotifyPropertyChanged {

    #region Fields
    private ViewState currentState;
    private GameDifficulty currentDifficulty;
    private GameStatus gameStatus;
    private bool isSettingFlag;
    private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
    #endregion

    #region Constructors
    public GameViewModel() 
    {
      this.currentState = ViewState.Menu;
      this.isSettingFlag = false;
      this.currentDifficulty = GameDifficulty.Beginner;
      this.gameStatus = GameStatus.NotStarted;
      this.StartGameCommand = new DelegateCommand<object>(this.StartGame, this.CanStartGame);
      this.EndGameCommand = new DelegateCommand<object>(this.EndGame, this.CanEndGame);
      this.SetFlagCommand = new DelegateCommand<object>(this.SetFlag, this.CanSetFlag);
      this.SelectTileCommand = new DelegateCommand<object>(this.SelectTile, this.CanSelectTile);
    }
    #endregion

    #region Properties
    public ICommand StartGameCommand { get; private set; }
    public ICommand EndGameCommand { get; private set; }
    public ICommand SetFlagCommand { get; private set; }
    public ICommand SelectTileCommand { get; private set; }
    public ViewState CurrentState { 
      get 
      { 
        return currentState;
      } 
      set 
      {
        currentState = value;
        OnPropertyChanged();
      }
    }
    public GameStatus CurrentGameStatus
    {
      get
      {
        return gameStatus;
      }
      set
      {
        gameStatus = value;
        OnPropertyChanged();
      }
    }
    public GameDifficulty CurrentDifficulty
    {
      get
      {
        return currentDifficulty;
      }
      set
      {
        currentDifficulty = value;
        OnPropertyChanged();
      }
    }
    public int NumberOfMines
    {
      get
      {
        if (CurrentDifficulty == GameDifficulty.Beginner) {
          return 10;
        } else if (CurrentDifficulty == GameDifficulty.Intermediate) {
          return 40;
        } else {
          return 99;
        }
      }
    }
    public int NumberOfMinesRemainingToFlag
    {
      get
      {
        int numFlagsPlaced = 0;
        foreach (var gameTile in GameBoardCollection) {
          if (gameTile.IsFlagged && !gameTile.IsSelected) {
            numFlagsPlaced++;
          }
        }
        return (NumberOfMines - numFlagsPlaced);
      }
    }
    public ObservableCollection<GameTileModel> GameBoardCollection { get; private set; }
    public bool IsSettingFlag {
      get {
        return isSettingFlag;
      }
      set {
        isSettingFlag = value;
        OnPropertyChanged();
      }
    }
    #endregion

    #region Private Methods
    private void StartGame(object arg)
    {
      int boardHeight, boardWidth;
      if (CurrentDifficulty == GameDifficulty.Beginner) {
        boardHeight = 10;
        boardWidth = 10;
      } else if (CurrentDifficulty == GameDifficulty.Intermediate) {
        boardHeight = 16;
        boardWidth = 16;
      } else {
        boardHeight = 16;
        boardWidth = 30;
      }
      GameBoardCollection = new ObservableCollection<GameTileModel>();
      for (int i = 0; i < boardHeight; i++) {
        for (int j = 0; j < boardWidth; j++) {
          GameBoardCollection.Add(new GameTileModel(i, j));
        }
      }

      List<int> tileNumbers = Enumerable.Range(0, (boardHeight * boardWidth - 1)).ToList();
      Random rand = new Random();

      for (int i = 0; i < NumberOfMines; i++) {
        int index = rand.Next(0, tileNumbers.Count - 1);
        int randNumber = tileNumbers[index];
        int row = randNumber / boardWidth;
        int col = randNumber % boardWidth;
        GameBoardCollection[randNumber] = new GameTileModel(row, col, true, false, false);
        tileNumbers.RemoveAt(index);
      }

      for (int i = 0; i < boardHeight; i++) {
        for (int j = 0; j < boardWidth; j++) {
          int index = (i * boardWidth) + j;
          int numMineNeighbors = GetNumMineNeighbors(i ,j, boardWidth, boardHeight);
          GameBoardCollection[index].NumMineNeighbors = numMineNeighbors;
        }
      }
      CurrentGameStatus = GameStatus.InProgress;
      CurrentState = ViewState.Game;
    }
    private void EndGame(object arg)
    {
      ResetDefaults();
      CurrentState = ViewState.Menu;
    }
    private void SetFlag(object arg)
    {
      IsSettingFlag = !IsSettingFlag;
    }
    private void SelectTile(object arg)
    {
      GameTileModel oldTile = (GameTileModel)arg;

      bool newTileIsMine = oldTile.IsMine;
      bool newTileIsFlagged = IsSettingFlag ^ oldTile.IsFlagged;
      bool newTileIsSelected = oldTile.IsSelected || !IsSettingFlag;

      if ((oldTile.IsFlagged != newTileIsFlagged) || (oldTile.IsSelected != newTileIsSelected)) {
        int index = GameBoardCollection.IndexOf(oldTile);
        var newTile = new GameTileModel(oldTile.Row, oldTile.Col, newTileIsMine, newTileIsSelected, newTileIsFlagged, oldTile.NumMineNeighbors);
        GameBoardCollection[index] = newTile;
        OnPropertyChanged("NumberOfMinesRemainingToFlag");
        logger.Trace("Gametile altered at position [%d,%d], index %d\nOld Tile - %s\nNewTile - %s", 
          oldTile.Row, oldTile.Col, index, oldTile.ToString(), newTile.ToString());
        if (newTile.NumMineNeighbors == 0 && !IsSettingFlag) {
          SelectNeighborsWithNoNeighboringMines(newTile);
        }
        if (CheckForGameWin()) {
          CurrentGameStatus = GameStatus.Won;
        } else if (newTile.IsMine && newTile.IsSelected) {
          RevealAllTiles();
          CurrentGameStatus = GameStatus.Lost;
        }
      }
    }
    private bool CanStartGame(object arg) { return true; }
    private bool CanEndGame(object arg) { return true; }
    private bool CanSetFlag(object arg) { return CurrentState == ViewState.Game && CurrentGameStatus == GameStatus.InProgress; }
    private bool CanSelectTile(object arg) { return CurrentGameStatus == GameStatus.InProgress; }
    private void ResetDefaults()
    {
      IsSettingFlag = false;
      CurrentDifficulty = GameDifficulty.Beginner;
      CurrentGameStatus = GameStatus.NotStarted;
      GameBoardCollection = null;
    }
    private int GetNumMineNeighbors(int row, int col, int boardWidth, int boardHeight)
    {
      int numMineNeighbors = 0;

      if (row - 1 >= 0 && col - 1 >= 0) {
        int upperLeftIndex = ((row - 1) * boardWidth) + col - 1;
        if (GameBoardCollection[upperLeftIndex].IsMine) {
          numMineNeighbors++;
        }
      }
      if (row - 1 >= 0) {
        int upperIndex = ((row - 1) * boardWidth) + col;
        if (GameBoardCollection[upperIndex].IsMine) {
          numMineNeighbors++;
        }
      }
      if (row - 1 >= 0 && col + 1 < boardWidth) {
        int upperRightIndex = ((row - 1) * boardWidth) + col + 1;
        if (GameBoardCollection[upperRightIndex].IsMine) {
          numMineNeighbors++;
        }
      }
      if (col - 1 >= 0) {
        int leftIndex = (row * boardWidth) + col - 1;
        if (GameBoardCollection[leftIndex].IsMine) {
          numMineNeighbors++;
        }
      }
      if (col + 1 < boardWidth) {
        int rightIndex = (row * boardWidth) + col + 1;
        if (GameBoardCollection[rightIndex].IsMine) {
          numMineNeighbors++;
        }
      }
      if (row + 1 < boardHeight && col - 1 >= 0) {
        int lowerLeftIndex = ((row + 1) * boardWidth) + col - 1;
        if (GameBoardCollection[lowerLeftIndex].IsMine) {
          numMineNeighbors++;
        }
      }
      if (row + 1 < boardHeight) {
        int lowerIndex = ((row + 1) * boardWidth) + col;
        if (GameBoardCollection[lowerIndex].IsMine) {
          numMineNeighbors++;
        }
      }
      if (row + 1 < boardHeight && col + 1 < boardWidth) {
        int lowerRightIndex = ((row + 1) * boardWidth) + col + 1;
        if (GameBoardCollection[lowerRightIndex].IsMine) {
          numMineNeighbors++;
        }
      }

      return numMineNeighbors;
    }
    private void SelectNeighborsWithNoNeighboringMines(GameTileModel newestSelectedTile)
    {
      int row = newestSelectedTile.Row;
      int col = newestSelectedTile.Col;
      int boardWidth = 0;
      int boardHeight = 0;
      switch (CurrentDifficulty) {
        case GameDifficulty.Beginner:
          boardWidth = 10;
          boardHeight = 10;
          break;
        case GameDifficulty.Intermediate:
          boardWidth = 16;
          boardHeight = 16;
          break;
        case GameDifficulty.Expert:
          boardWidth = 30;
          boardHeight = 16;
          break;
      }

      if (row - 1 >= 0 && col - 1 >= 0) {
        int upperLeftIndex = ((row - 1) * boardWidth) + col - 1;
        SelectTile(GameBoardCollection[upperLeftIndex]);
      }
      if (row - 1 >= 0) {
        int upperIndex = ((row - 1) * boardWidth) + col;
        SelectTile(GameBoardCollection[upperIndex]);
      }
      if (row - 1 >= 0 && col + 1 < boardWidth) {
        int upperRightIndex = ((row - 1) * boardWidth) + col + 1;
        SelectTile(GameBoardCollection[upperRightIndex]);
      }
      if (col - 1 >= 0) {
        int leftIndex = (row * boardWidth) + col - 1;
        SelectTile(GameBoardCollection[leftIndex]);
      }
      if (col + 1 < boardWidth) {
        int rightIndex = (row * boardWidth) + col + 1;
        SelectTile(GameBoardCollection[rightIndex]);
      }
      if (row + 1 < boardHeight && col - 1 >= 0) {
        int lowerLeftIndex = ((row + 1) * boardWidth) + col - 1;
        SelectTile(GameBoardCollection[lowerLeftIndex]);
      }
      if (row + 1 < boardHeight) {
        int lowerIndex = ((row + 1) * boardWidth) + col;
        SelectTile(GameBoardCollection[lowerIndex]);
      }
      if (row + 1 < boardHeight && col + 1 < boardWidth) {
        int lowerRightIndex = ((row + 1) * boardWidth) + col + 1;
        SelectTile(GameBoardCollection[lowerRightIndex]);
      }
    }
    private bool CheckForGameWin()
    {
      foreach (GameTileModel tile in GameBoardCollection) {
        if (!tile.IsMine && !tile.IsSelected) {
          return false;
        }
      }
      return true;
    }
    private void RevealAllTiles()
    {
      for(int i = 0; i < GameBoardCollection.Count; i++) {
        if (!GameBoardCollection[i].IsSelected) {
          GameTileModel oldTile = GameBoardCollection[i];
          GameBoardCollection[i] = new GameTileModel(oldTile.Row, oldTile.Col, oldTile.IsMine, true, oldTile.IsFlagged, oldTile.NumMineNeighbors);
        }
      }
    }
    #endregion
  }
}

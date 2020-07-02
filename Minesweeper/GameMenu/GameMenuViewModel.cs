﻿using Minesweeper.GamePlay;
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

namespace Minesweeper.GameMenu {
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
  #endregion
  class GameMenuViewModel : CustomNotifyPropertyChanged {

    #region Fields
    private ObservableCollection<GameTileModel> prGameTileCollection;
    private ViewState currentState;
    private GameDifficulty currentDifficulty;
    private bool isSettingFlag;
    private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
    #endregion

    /*Beginner has a total of ten mines and the board size is either 8 × 8, 9 × 9, or 10 × 10.
     * Intermediate has 40 mines and also varies in size between 13 × 15 and 16 × 16. 
     * Expert has 99 mines and is always 16 × 30 (or 30 × 16)     * 
     */
    #region Constructors
    public GameMenuViewModel() 
    {
      this.currentState = ViewState.Menu;
      this.isSettingFlag = false;
      this.currentDifficulty = GameDifficulty.Beginner;
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
      get  { 
        return currentState;
      } 
      set {
        currentState = value;
        OnPropertyChanged();
      }
    }

    public ObservableCollection<GameTileModel> GameBoardCollection {
      get {
        return prGameTileCollection;
      }
    }

    public GameDifficulty CurrentDifficulty {
      get {
        return currentDifficulty;
      }
      set {
        currentDifficulty = value;
        OnPropertyChanged();
      }
    } 

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
      int boardHeight, boardWidth, numMines;
      if (CurrentDifficulty == GameDifficulty.Beginner) {
        boardHeight = 10;
        boardWidth = 10;
        numMines = 10;
      } else if (CurrentDifficulty == GameDifficulty.Intermediate) {
        boardHeight = 16;
        boardWidth = 16;
        numMines = 40;
      } else {
        boardHeight = 16;
        boardWidth = 30;
        numMines = 99;
      }
      prGameTileCollection = new ObservableCollection<GameTileModel>();
      for (int i = 0; i < boardHeight; i++) {
        for (int j = 0; j < boardWidth; j++) {
          prGameTileCollection.Add(null);
        }
      }

      List<int> tileNumbers = Enumerable.Range(0, (boardHeight * boardWidth - 1)).ToList();
      Random rand = new Random();

      for (int i = 0; i < numMines; i++) {
        int index = rand.Next(0, tileNumbers.Count - 1);
        int randNumber = tileNumbers[index];
        int row = randNumber / boardWidth;
        int col = randNumber % boardWidth;
        prGameTileCollection[randNumber] = new GameTileModel(row, col, true, false, false);
        tileNumbers.RemoveAt(index);
      }

      for (int i = 0; i < boardHeight; i++) {
        for (int j = 0; j < boardWidth; j++) {
          int index = (i * boardWidth) + j;
          if (prGameTileCollection[index] == null) {
            prGameTileCollection[index] = new GameTileModel(i, j);
          }
        }
      }
      CurrentState = ViewState.Game;
      ResetDefaults();
    }

    private void EndGame(object arg)
    {
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
        int index = prGameTileCollection.IndexOf(oldTile);
        var newTile = new GameTileModel(oldTile.Row, oldTile.Col, newTileIsMine, newTileIsSelected, newTileIsFlagged);
        prGameTileCollection[index] = newTile;
        logger.Trace("Gametile altered at position [%d,%d], index %d\nOld Tile - %s\nNewTile - %s", 
          oldTile.Row, oldTile.Col, index, oldTile.ToString(), newTile.ToString());
      }
    }
    private bool CanStartGame(object arg) { return true; }

    private bool CanEndGame(object arg) { return true; }

    private bool CanSetFlag(object arg) { return CurrentState == ViewState.Game; }

    private bool CanSelectTile(object arg) { return true; }
    private void ResetDefaults()
    {
      IsSettingFlag = false;
      CurrentDifficulty = GameDifficulty.Beginner;
    }
    #endregion
  }
}

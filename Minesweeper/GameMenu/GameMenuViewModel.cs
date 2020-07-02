using Minesweeper.GamePlay;
using Minesweeper.Utils;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
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
  class GameMenuViewModel : CustomNotifyPropertyOrCollectionChanged {

    #region Fields
    private GameTileCollectionModel gameTileCollectionModel;
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
      this.gameTileCollectionModel = new GameTileCollectionModel();
      gameTileCollectionModel.CollectionChanged += tileModel_CollectionChanged;
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

    public List<List<GameTileModel>> GameBoardArray {
      get {
        return gameTileCollectionModel.TileArray;
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
      GameTileModel selectedTile = (GameTileModel)arg;
      bool isSwitchingFlagStatus = IsSettingFlag;
      bool isMineBeingSelected = !IsSettingFlag;
      gameTileCollectionModel.AlterGameTile(selectedTile.TileIdentifier, isSwitchingFlagStatus, isMineBeingSelected);
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

    private void tileModel_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (e.Action == NotifyCollectionChangedAction.Replace) {
        Console.WriteLine(GameBoardArray);
        OnPropertyChanged("GameBoardArray");
      } else {
        logger.Warn(String.Format("Collection changed action type not implemented : {0}", e.Action.ToString()));
        throw new NotImplementedException(String.Format("Collection changed action type not implemented : {0}", e.Action.ToString()));
      }   
    }
    #endregion
  }
}

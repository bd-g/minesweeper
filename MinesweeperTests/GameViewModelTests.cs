using System;
using System.Collections.ObjectModel;
using Minesweeper.GamePlay;
using NUnit.Framework;

namespace MinesweeperTests {
  [TestFixture]
  public class GameViewModelTests {
    #region Fields
    private GameViewModel gameViewModel;
    #endregion

    [SetUp]
    public void Setup()
    {
      gameViewModel = new GameViewModel();
    }

    [TestCase(GameDifficulty.Beginner)]
    [TestCase(GameDifficulty.Intermediate)]
    [TestCase(GameDifficulty.Expert)]
    public void GameTileCollectionCheckMineCount(GameDifficulty gameDifficulty) {
      int numMines = 0;
      int numExpectedMines;
      switch(gameDifficulty) {
        case GameDifficulty.Beginner:
          numExpectedMines = 10;
          break;
        case GameDifficulty.Intermediate:
          numExpectedMines = 40;
          break;
        case GameDifficulty.Expert:
          numExpectedMines = 99;
          break;
        default:
          throw new ArgumentException(String.Format("Unexpected game difficulty encountered - {0}", gameDifficulty));
        }
      gameViewModel.CurrentDifficulty = gameDifficulty;
      gameViewModel.StartGameCommand.Execute(null);

      Assert.AreEqual(gameViewModel.CurrentState, ViewState.Game);
      Assert.AreEqual(gameViewModel.CurrentGameStatus, GameStatus.InProgress);

      ObservableCollection<GameTileModel> gameTileCollection = gameViewModel.GameBoardCollection;
      foreach (GameTileModel singleTile in gameTileCollection) {
        if (singleTile.IsMine) {
          numMines++;
        }
      }

      Assert.AreEqual(numExpectedMines, numMines, "Unexpected number of mines were found.");
    }
  }
}
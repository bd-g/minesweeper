using Minesweeper.GamePlay;
using NUnit.Framework;

namespace MinesweeperTests {
  [TestFixture]
  public class GameTileCollectionModelTests {
    [SetUp]
    public void Setup() {
    }

    [TestCase(10, 10, 10)]
    [TestCase(15, 11, 15)]
    [TestCase(16, 20, 18)]
    [TestCase(16, 19, 22)]
    [TestCase(24, 40, 52)]
    [TestCase(9, 9, 13)]
    [TestCase(10, 12, 0)]
    public void GameTileCollectionCheckMineCount(int height, int width, int numMinesToCreate) {
      int numMines = 0;
      var gameTileCollection = new GameTileCollectionModel(height, width, numMinesToCreate);
      var gameTileArray = gameTileCollection.TileArray;
      for (int i = 0; i < gameTileArray.Count; i++) {
        for (int j = 0; j < gameTileArray[0].Count; j++) {
          if (gameTileArray[i][j].IsMine) {
            numMines++;
          }
        }
      }

      Assert.AreEqual(numMinesToCreate, numMines, "Unexpected number of mines were found.");
    }
  }
}
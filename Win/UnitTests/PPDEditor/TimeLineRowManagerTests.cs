using PPDEditor;
using Xunit;

namespace UnitTests.PPDEditor
{
    /// <summary>
    /// 時間軸の行（ボタンごとの行の順番と表示）のテストです。
    /// </summary>
    public class TimeLineRowManagerTests
    {
        [Fact]
        public void Normal_ShowsEveryRowInOrder()
        {
            var manager = new TimeLineRowManager();

            Assert.Equal([0, 1, 2, 3, 4, 5, 6, 7, 8, 9], manager.OrderedVisibleRows);
            Assert.Equal(10, manager.OrderedVisibleRowsCount);
            Assert.Equal([0, 1, 2, 3, 4, 5, 6, 7, 8, 9], manager.InverseRowOrders);
            Assert.Equal(3, manager.GetTypeFromRowIndex(3));
            Assert.Equal(3, manager.GetRowIndexFromType(3));
            Assert.Equal(-1, manager.GetTypeFromRowIndex(10));
            Assert.Equal(-1, manager.GetRowIndexFromType(10));
        }

        [Fact]
        public void Limited_UsesOrderAndVisibility()
        {
            var manager = new TimeLineRowManager
            {
                RowOrders = [9, 8, 7, 6, 5, 4, 3, 2, 1, 0],
                RowVisibilities = [true, false, true, false, true, true, true, true, true, true],
            };
            // 限定しないうちは、ふつうの行のまま
            Assert.Equal(10, manager.OrderedVisibleRowsCount);

            manager.IsLimited = true;

            Assert.Equal([9, 8, 7, 6, 5, 4, 2, 0], manager.OrderedVisibleRows);
            Assert.Equal(8, manager.OrderedVisibleRowsCount);
            Assert.False(manager.Visibilities[1]);
            Assert.Equal(9, manager.InverseRowOrders[0]);
            Assert.Equal(2, manager.GetTypeFromRowIndex(6));
            // 隠れた行は数えない
            Assert.Equal(6, manager.GetRowIndexFromType(2));
            Assert.Equal(7, manager.GetRowIndexFromType(1));

            // y 座標から行（上の目盛りの下から 1 行ずつ）
            var top = PPDEditorSkin.Skin.TimeLineHeight;
            var height = PPDEditorSkin.Skin.TimeLineRowHeight;
            Assert.Equal(-1, manager.GetRowIndex(top - 1));
            Assert.Equal(9, manager.GetRowIndex(top));
            Assert.Equal(8, manager.GetRowIndex(top + height));
            Assert.Equal(11, manager.GetRowIndex(top + height * 8));
            Assert.Equal(8, manager.GetNormalRowIndex(top + height * 8));

            // 元に戻すと、ふつうの行になり、限定の設定も初めに戻る
            manager.IsLimited = false;
            manager.Restore();
            Assert.Equal(10, manager.OrderedVisibleRowsCount);
            Assert.Equal([0, 1, 2, 3, 4, 5, 6, 7, 8, 9], manager.RowOrders);
            Assert.All(manager.RowVisibilities, Assert.True);
        }
    }
}

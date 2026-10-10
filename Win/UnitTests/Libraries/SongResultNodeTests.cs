using PPDFramework;
using System.Linq;
using System.Reflection;
using UnitTests.FlowScript;
using Xunit;

namespace UnitTests.Libraries
{
    /// <summary>
    /// 遊んだ結果を読むノード（PPD.Song.Result.Value）のテストです。
    /// </summary>
    public class SongResultNodeTests
    {
        static NodeHarness Node(ResultInfo value)
        {
            var harness = new NodeHarness(typeof(FlowScriptEnginePPD.FlowSourceObjects.Song.Result.ValueFlowSourceObject));
            if (value != null)
            {
                harness.Set("Value", value);
            }
            return harness;
        }

        [Fact]
        public void Value_ReadsEveryResultField()
        {
            var result = new ResultInfo();
            var number = 1;
            // 結果は内部でだけ設定するので、名前で入れる
            foreach (var property in typeof(ResultInfo).GetProperties().Where(p => p.GetSetMethod(true) != null))
            {
                if (property.PropertyType == typeof(int))
                {
                    property.SetValue(result, number++);
                }
                else if (property.PropertyType.IsEnum)
                {
                    var values = System.Enum.GetValues(property.PropertyType);
                    property.SetValue(result, values.GetValue(values.Length - 1));
                }
                else if (property.PropertyType == typeof(float))
                {
                    property.SetValue(result, 1.5f);
                }
            }
            var node = Node(result);
            var checkedCount = 0;

            foreach (var output in node.Node.GetType().GetProperties().Where(p => p.Name is not ("Value" or "Name") && p.GetGetMethod() != null && p.DeclaringType == node.Node.GetType()))
            {
                var source = typeof(ResultInfo).GetProperty(output.Name == "ResultType" ? "ResultEvaluate" : output.Name == "PlayDateTime" ? "Date" : output.Name);
                if (source == null)
                {
                    continue;
                }
                Assert.Equal(source.GetValue(result), output.GetValue(node.Node));
                checkedCount++;
            }

            Assert.True(checkedCount >= 10, $"確かめた値: {checkedCount}");
            Assert.Equal(true, new NodeHarness(typeof(FlowScriptEnginePPD.FlowSourceObjects.Song.Result.IsTypeFlowSourceObject)).Set("Target", result).Get("Value"));
        }

        [Fact]
        public void Value_WithoutResult_IsDefault()
        {
            var node = Node(null);

            Assert.Equal(0, node.Get("Score"));
            Assert.Equal(PPDFrameworkCore.Difficulty.Other, node.Get("Difficulty"));
            Assert.Equal(ResultEvaluateType.Mistake, node.Get("ResultType"));
            Assert.Equal(0, node.Get("MaxCombo"));
        }
    }
}

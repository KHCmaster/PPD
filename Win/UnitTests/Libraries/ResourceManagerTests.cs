using PPDFramework.Mod;
using PPDFramework.Resource;
using System;
using System.Collections.Generic;
using Vortice.Mathematics;
using Xunit;

namespace UnitTests.Libraries
{
    /// <summary>
    /// リソースマネージャー（親をたどって探す、消す、破棄）と、Mod の設定の保存の形のテストです。
    /// </summary>
    public class ResourceManagerTests
    {
        sealed class TestResource : ResourceBase
        {
            public int DisposeCount { get; private set; }

            protected override void DisposeResource()
            {
                DisposeCount++;
                base.DisposeResource();
            }
        }

        sealed class OtherResource : ResourceBase;

        [Fact]
        public void Find_LooksAtParentsFirst()
        {
            var parentResource = new TestResource();
            var childResource = new TestResource();
            using var parent = new ResourceManager();
            parent.Add("shared", parentResource);
            using var child = new ResourceManager([new(parent, false)]);
            child.Add("shared", childResource);
            child.Add("own", new TestResource());

            // 同じ名前なら親のもの。名前の大文字小文字は区別しない
            Assert.Same(parentResource, child.GetResource<TestResource>("SHARED"));
            Assert.True(child.HasResource<TestResource>("own"));
            Assert.False(child.HasResource<TestResource>("missing"));
            Assert.Null(child.GetResource<TestResource>("missing"));

            // 先に足したものを残す。上書きもできる
            var other = new TestResource();
            Assert.Same(other, child.Add("own", other));
            Assert.NotSame(other, child.GetResource<TestResource>("own"));
            child.AddAndOverride("own", other);
            Assert.Same(other, child.GetResource<TestResource>("own"));
        }

        [Fact]
        public void WrongType_IsNotFound()
        {
            using var manager = new ResourceManager();
            manager.Add("a", new OtherResource());

            Assert.False(manager.HasResource<TestResource>("a"));
            Assert.Null(manager.GetResource<TestResource>("a"));
            Assert.True(manager.HasResource<OtherResource>("a"));
        }

        [Fact]
        public void Remove_OnlyOwnResource()
        {
            var parentResource = new TestResource();
            var ownResource = new TestResource();
            using var parent = new ResourceManager();
            parent.Add("shared", parentResource);
            using var child = new ResourceManager([new(parent, false)]);
            child.Add("own", ownResource);

            child.RemoveResource("own");
            Assert.Equal(1, ownResource.DisposeCount);
            Assert.False(child.HasResource<TestResource>("own"));

            // 親のものは消さない（親にはまだ残っているので、壊さない）
            child.RemoveResource("shared");
            Assert.Equal(0, parentResource.DisposeCount);
            Assert.Same(parentResource, parent.GetResource<TestResource>("shared"));
        }

        [Fact]
        public void Dispose_DisposesOwnedParentsOnly()
        {
            var ownedResource = new TestResource();
            var sharedResource = new TestResource();
            var mine = new TestResource();
            var owned = new ResourceManager();
            owned.Add("a", ownedResource);
            using var shared = new ResourceManager();
            shared.Add("b", sharedResource);
            var manager = new ResourceManager([new(owned, true), new(shared, false)]);
            manager.Add("c", mine);

            manager.Dispose();

            Assert.Equal((1, 1, 0), (mine.DisposeCount, ownedResource.DisposeCount, sharedResource.DisposeCount));
        }

        [Fact]
        public void ModSettings_RoundTripEveryType()
        {
            var key = "test" + Guid.NewGuid().ToString("N");
            var values = new Dictionary<string, object>
            {
                ["int"] = 3,
                ["float"] = 1.5f,
                ["double"] = 2.25,
                ["string"] = "文字",
                ["bool"] = true,
                ["color"] = new Color4(1, 0, 0, 1),
            };

            ModSettingDatabase.Setting.SetSetting(key, values);
            var loaded = ModSettingDatabase.Setting.GetSetting(key);

            Assert.Equal(values.Keys, loaded.Keys);
            Assert.Equal((3, 1.5f, 2.25, "文字", true), ((int)loaded["int"], (float)loaded["float"], (double)loaded["double"], (string)loaded["string"], (bool)loaded["bool"]));
            Assert.Equal(new Color4(1, 0, 0, 1), loaded["color"]);
            Assert.Empty(ModSettingDatabase.Setting.GetSetting("missing" + key));
            // 空のキーには保存しない。保存できない型は例外
            ModSettingDatabase.Setting.SetSetting("", values);
            Assert.Throws<Exception>(() => ModSettingDatabase.Setting.SetSetting(key, new Dictionary<string, object> { ["x"] = new object() }));
        }
    }
}

using PPDFramework;
using PPDFramework.Scene;
using System;
using System.Collections.Generic;

namespace PPDTest
{
    class TestSceneManager(SceneManager sceneManager, PPDDevice device)
    {
        readonly List<Type> sceneTypes = [];
        int currentSceneIndex;
        readonly PPDDevice device = device;
        readonly SceneManager sceneManager = sceneManager;

        public void Add(Type type)
        {
            sceneTypes.Add(type);
        }

        public void Previous(ISceneBase caller)
        {
            currentSceneIndex--;
            if (currentSceneIndex < 0)
            {
                currentSceneIndex = sceneTypes.Count - 1;
            }
            ChangeScene(caller);
        }

        public void Next(ISceneBase caller)
        {
            currentSceneIndex++;
            if (currentSceneIndex >= sceneTypes.Count)
            {
                currentSceneIndex = 0;
            }
            ChangeScene(caller);
        }

        public TestSceneBase Initialize()
        {
            return CreateScene();
        }

        /// <summary>
        /// 最初に表示するシーンを指定します。
        /// </summary>
        /// <param name="scene">シーンの番号（0 から）、またはクラス名（大文字と小文字は区別しない。末尾の Scene は省略できる）</param>
        /// <returns>見つかったかどうか。</returns>
        public bool SetInitialScene(string scene)
        {
            if (int.TryParse(scene, out var index))
            {
                if (index < 0 || index >= sceneTypes.Count)
                {
                    return false;
                }
                currentSceneIndex = index;
                return true;
            }
            index = sceneTypes.FindIndex(t =>
                string.Equals(t.Name, scene, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(t.Name, scene + "Scene", StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                return false;
            }
            currentSceneIndex = index;
            return true;
        }

        /// <summary>
        /// シーンの一覧を「番号: クラス名」の形で取得します。
        /// </summary>
        public IEnumerable<string> GetSceneNames()
        {
            for (var i = 0; i < sceneTypes.Count; i++)
            {
                yield return string.Format("{0}: {1}", i, sceneTypes[i].Name);
            }
        }

        private void ChangeScene(ISceneBase caller)
        {
            var scene = CreateScene();
            sceneManager.PrepareNextScene(caller, scene, null, null);
        }

        private TestSceneBase CreateScene()
        {
            return (TestSceneBase)Activator.CreateInstance(sceneTypes[currentSceneIndex], this, device);
        }
    }
}

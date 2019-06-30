using PPDFramework;
using PPDFramework.Scene;
using System;
using System.Collections.Generic;

namespace PPDTest
{
    class TestSceneManager
    {
        List<Type> sceneTypes;
        int currentSceneIndex;
        PPDDevice device;
        SceneManager sceneManager;

        public TestSceneManager(SceneManager sceneManager, PPDDevice device)
        {
            this.device = device;
            this.sceneManager = sceneManager;
            sceneTypes = new List<Type>();
        }

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

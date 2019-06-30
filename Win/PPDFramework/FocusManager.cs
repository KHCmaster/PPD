using PPDFramework.Scene;
using System.Collections.Generic;

namespace PPDFramework
{
    /// <summary>
    /// フォーカスマネージャーです
    /// </summary>
    public class FocusManager
    {
        List<IFocusable> stack;
        ISceneBase baseScene;

        /// <summary>
        /// コンストラクタです
        /// </summary>
        public FocusManager()
        {
            InnerStruct(null);
        }

        /// <summary>
        /// コンストラクタです
        /// </summary>
        /// <param name="baseScene">フォーカスマネージャを管理するシーン</param>
        public FocusManager(ISceneBase baseScene)
        {
            InnerStruct(baseScene);
        }

        private void InnerStruct(ISceneBase baseScene)
        {
            stack = new List<IFocusable>();
            this.baseScene = baseScene;
        }

        /// <summary>
        /// フォーカスします
        /// </summary>
        /// <param name="focusObject"></param>
        public void Focus(IFocusable focusObject)
        {
            if (lastFocusObject != null && lastFocusObject.Focused)
            {
                lastFocusObject.Focused = false;
            }
            lastFocusObject = CurrentFocusObject;
            stack.Add(focusObject);
            focusObject.FocusManager = this;
            focusObject.Focused = true;
            focusObject.OverFocused = true;
            if (lastFocusObject != null)
            {
                lastFocusObject.Focused = false;
            }
        }

        /// <summary>
        /// 現在フォーカスが当たっているコントロールからフォーカスを除きます
        /// </summary>
        public void RemoveFocus()
        {
            if (CurrentFocusObject != null)
            {
                lastFocusObject = stack[stack.Count - 1];
                stack.RemoveAt(stack.Count - 1);
                lastFocusObject.OverFocused = false;
                lastFocusObject.Focused = false;
                lastFocusObject.FocusManager = null;
                if (CurrentFocusObject != null)
                {
                    CurrentFocusObject.Focused = true;
                }
            }
        }

        /// <summary>
        /// 現在のフォーカスが当たっているオブジェクトです
        /// </summary>
        public IFocusable CurrentFocusObject
        {
            get
            {
                if (stack.Count == 0) return null;
                return stack[stack.Count - 1];
            }
        }

        IFocusable lastFocusObject;
        /// <summary>
        /// 最後にフォーカスが当たっていたオブジェクトです
        /// </summary>
        public IFocusable LastFocusObject
        {
            get
            {
                return lastFocusObject;
            }
            set
            {
                lastFocusObject = value;
            }
        }

        /// <summary>
        /// フォーカスマネージャーを管理するシーンを取得します
        /// </summary>
        public ISceneBase BaseScene
        {
            get
            {
                return baseScene;
            }
        }

        /// <summary>
        /// 入力を処理します
        /// </summary>
        /// <param name="inputInfo"></param>
        public void ProcessInput(InputInfoBase inputInfo)
        {
            IFocusable temp = CurrentFocusObject;
            int current = stack.Count;
            for (int i = stack.Count - 1; i >= 0; i--)
            {
                if (i >= stack.Count)
                {
                    continue;
                }

                IFocusable focusable = stack[i];
                if (focusable.HandleOverFocusInput || focusable == temp)
                {
                    var handled = focusable.ProcessInput(inputInfo);
                    if (handled)
                    {
                        break;
                    }
                }
            }
        }
    }
}

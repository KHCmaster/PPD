using PPDFramework;
using PPDFramework.Scene;
using PPDShareComponent;
using System;
using System.Collections.Generic;

namespace PPD
{
    class HomeScene(PPDDevice device) : SceneBase(device)
    {
        public event Action<int> LoadProgressed;

        PictureObject back;
        HomeBottomMenu hbm;
        HomeTopHeader header;
        FocusManager focusManager;

        SettingPanel sp;
        MoviePanel mp;
        GameListPanel glp;
        FeedPanel fp;

        StartPanel startPanel;

        HomePanelBase currentPanel;
        SortedList<HomeBottomMenu.Mode, HomePanelBase> panelList;

        long startTime;
        GameLoader currentGame;

        public override bool Load()
        {
            base.Load();
            OnLoadProgressed(0);
            back = new PictureObject(device, ResourceManager, Utility.Path.Combine("background.png"));
            OnLoadProgressed(1);
            hbm = new HomeBottomMenu(device, ResourceManager, Sound);
            OnLoadProgressed(10);
            header = new HomeTopHeader(device, ResourceManager, hbm);
            OnLoadProgressed(20);
            focusManager = new FocusManager(this);
            sp = new SettingPanel(device, ResourceManager, GameHost as MyGame, Sound);
            mp = new MoviePanel(device, GameHost as MyGame, ResourceManager, Sound);
            glp = new GameListPanel(device, ResourceManager, Sound);
            fp = new FeedPanel(device, GameHost, ResourceManager, Sound);
            sp.LoadProgressed += Panel_LoadProgressed;
            mp.LoadProgressed += Panel_LoadProgressed;
            glp.LoadProgressed += Panel_LoadProgressed;
            fp.LoadProgressed += Panel_LoadProgressed;

            glp.GameStarted += Glp_GameStarted;
            hbm.ModeChanged += Hbm_ModeChanged;

            panelList = new SortedList<HomeBottomMenu.Mode, HomePanelBase>
            {
                {HomeBottomMenu.Mode.Feed,fp },                {HomeBottomMenu.Mode.Game,glp },                {HomeBottomMenu.Mode.Movie,mp },                {HomeBottomMenu.Mode.Setting,sp }            };

            sp.Load();
            mp.Load();
            glp.Load();
            fp.Load();

            foreach (HomePanelBase panel in panelList.Values)
            {
                if (panel != currentPanel)
                {
                    panel.Alpha = 0;
                }
            }
            if (!PPDGeneralSetting.Setting.IsFirstExecution)
            {
                focusManager.Focus(hbm);
                focusManager.Focus(fp);
                currentPanel = fp;
                this.AddChild(hbm);
                this.AddChild(header);
                this.AddChild(sp);
                this.AddChild(mp);
                this.AddChild(glp);
                this.AddChild(fp);
            }
            else
            {
                startPanel = new StartPanel(device, GameHost as MyGame, ResourceManager);
                startPanel.SettingFinished += StartPanel_SettingFinished;
                this.AddChild(startPanel);
            }
            this.AddChild(back);

            ChangeControllerConfig();
            OnLoadProgressed(100);

            return true;
        }

        void Panel_LoadProgressed(int obj)
        {
            int sum = 0;
            foreach (HomePanelBase panel in panelList.Values)
            {
                sum += panel.CurrentProgress;
            }

            OnLoadProgressed(20 + 70 * sum / (panelList.Count * 100));
        }

        private void OnLoadProgressed(int val)
        {
            LoadProgressed?.Invoke(val);
        }

        private void ChangeControllerConfig()
        {
            if (GameHost is MyGame myGame && myGame.Input.JoyStickCount > 0)
            {
                string configName = PPDGeneralSetting.Setting[String.Format("{0}_config", myGame.Input.JoyStickNames[myGame.Input.CurrentJoyStickIndex])];
                myGame.KeyConfigManager.ChangeSettingFromName(configName);
            }
        }

        private void SaveControllerConfig()
        {
            if (GameHost is MyGame myGame && myGame.Input.JoyStickCount > 0)
            {
                PPDGeneralSetting.Setting[String.Format("{0}_config", myGame.Input.JoyStickNames[myGame.Input.CurrentJoyStickIndex])] = myGame.KeyConfigManager.CurrentConfig.Name;
            }
        }

        void Glp_GameStarted(object sender, GameEventArgs e)
        {
            mp.StopMovie();
            if (GameHost is MyGame myGame)
            {
                myGame.StartGame(e.GameLoader);
            }

            currentGame = e.GameLoader;
            startTime = GameHost.GameTimer.ElapsedTime;
        }

        public void ComeHome()
        {
            UpdatePlayingTime();
        }

        private void UpdatePlayingTime()
        {
            if (currentGame != null)
            {
                long elapsedTime = (GameHost.GameTimer.ElapsedTime - startTime) / 1000;
                var value = GameListPanel.GetPlayingSecond(currentGame);
                value += (int)elapsedTime;
                glp.UpdatePlayingTime(currentGame, value);
                PPDGeneralSetting.Setting[currentGame.GetFullName()] = value.ToString();
                currentGame = null;
            }
        }

        void StartPanel_SettingFinished(object sender, EventArgs e)
        {
            this.ClearChildren();
            focusManager.Focus(hbm);
            focusManager.Focus(fp);
            currentPanel = fp;
            this.AddChild(hbm);
            this.AddChild(header);
            this.AddChild(sp);
            this.AddChild(mp);
            this.AddChild(glp);
            this.AddChild(fp);
            this.AddChild(back);

            PPDGeneralSetting.Setting.IsFirstExecution = false;
            (GameHost as MyGame).KeyConfigManager.Write("keyconfig.ini");
            (GameHost as MyGame).Input.AssignMode = false;
        }

        void Hbm_ModeChanged(object sender, EventArgs e)
        {
            if (currentPanel != null)
            {
                while (focusManager.CurrentFocusObject != hbm)
                {
                    focusManager.RemoveFocus();
                }
            }
            currentPanel = panelList[hbm.CurrentMode];
            focusManager.Focus(currentPanel);
        }

        public override void Update(InputInfoBase inputInfo, MouseInfo mouseInfo)
        {
            inputInfo = new MenuInputInfo(inputInfo);
            focusManager.ProcessInput(inputInfo);

            foreach (HomePanelBase panel in panelList.Values)
            {
                panel.Alpha = panel != currentPanel ? AnimationUtility.DecreaseAlpha(panel.Alpha) : AnimationUtility.IncreaseAlpha(panel.Alpha);
            }
            UpdateMouseInfo(mouseInfo);
            Update();
        }

        protected override void DisposeResource()
        {
            UpdatePlayingTime();
            SaveControllerConfig();
            base.DisposeResource();
        }
    }
}

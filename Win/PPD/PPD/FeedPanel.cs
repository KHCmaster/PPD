using PPDFramework;
using PPDFrameworkCore;
using PPDShareComponent;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Xml;

namespace PPD
{
    class FeedPanel : HomePanelBase
    {
        IGameHost gameHost;
        PPDFramework.Resource.ResourceManager resourceManager;
        ISound sound;

        PictureObject left;
        PictureObject leftSelection;
        SpriteObject feedSprite;
        TextureString errorString;
        FeedListLoader loader;

        private bool initialized;
        int currentIndex;

        public FeedPanel(PPDDevice device, IGameHost gameHost, PPDFramework.Resource.ResourceManager resourceManager, ISound sound) : base(device)
        {
            this.gameHost = gameHost;
            this.resourceManager = resourceManager;
            this.sound = sound;
        }

        public override void Load()
        {
            OnLoadProgressed(0);
            this.AddChild((left = new PictureObject(device, resourceManager, Utility.Path.Combine("leftmenu.png"))));
            OnLoadProgressed(20);
            left.AddChild((leftSelection = new PictureObject(device, resourceManager, Utility.Path.Combine("right.png"), true)
            {
                Position = new SharpDX.Vector2(30, 0),
                Scale = new SharpDX.Vector2(0.5f, 0.5f)
            }));
            OnLoadProgressed(40);
            this.AddChild((errorString = new TextureString(device, Utility.Language["ErrorInFeedLoading"], 20, true, PPDColors.White)
            {
                Position = new SharpDX.Vector2(400, 220),
                Alpha = 0
            }));
            OnLoadProgressed(60);
            this.AddChild((feedSprite = new SpriteObject(device)
            {
                Position = new SharpDX.Vector2(200, 0),
                Clip = new ClipInfo(gameHost)
                {
                    PositionX = 200,
                    PositionY = 0,
                    Width = 600,
                    Height = 450
                }
            }));

            loader = new FeedListLoader();
            OnLoadProgressed(80);

            Inputed += FeedPanel_Inputed;
            GotFocused += FeedPanel_GotFocused;
            OnLoadProgressed(100);
        }

        void FeedPanel_GotFocused(IFocusable sender, FocusEventArgs args)
        {
            if (!initialized)
            {
                loader.GetList();
                initialized = true;
            }
        }

        void FeedPanel_Inputed(IFocusable sender, InputEventArgs args)
        {
            if (initialized)
            {
                if (!loader.Error)
                {
                    if (args.InputInfo.IsPressed(ButtonType.Down))
                    {
                        currentIndex++;
                        if (currentIndex >= feedSprite.ChildrenCount)
                        {
                            if (feedSprite.ChildrenCount > 0 && (feedSprite[feedSprite.ChildrenCount - 1] as FeedComponent).Finished && currentIndex < loader.List.Length)
                            {
                                feedSprite.AddChild(new FeedComponent(device, resourceManager, loader.List[currentIndex]) { Position = new SharpDX.Vector2(0, 450 * currentIndex) });
                                sound.Play(PPDSetting.DefaultSounds[0], -1000);
                            }
                            else
                            {
                                currentIndex--;
                            }
                        }
                        else
                        {
                            sound.Play(PPDSetting.DefaultSounds[0], -1000);
                        }
                    }
                    else if (args.InputInfo.IsPressed(ButtonType.Up))
                    {
                        currentIndex--;
                        if (currentIndex < 0)
                        {
                            currentIndex = 0;
                        }
                        else
                        {
                            sound.Play(PPDSetting.DefaultSounds[0], -1000);
                        }
                    }
                }
            }
        }

        protected override void UpdateImpl()
        {
            if (initialized)
            {
                if (loader.Finished)
                {
                    loader.Finished = false;
                    if (!loader.Error)
                    {
                        int iter = 0;
                        foreach (Feed feed in loader.List)
                        {
                            left.AddChild(new TextureString(device, feed.Date.ToShortDateString(), 14, PPDColors.White)
                            {
                                Position = new SharpDX.Vector2(40, 10 + 20 * iter)
                            });
                            iter++;
                        }
                        feedSprite.AddChild(new FeedComponent(device, resourceManager, loader.List[0]));
                    }
                }

                feedSprite.Position = new SharpDX.Vector2(feedSprite.Position.X, AnimationUtility.GetAnimationValue(feedSprite.Position.Y, -currentIndex * 450));
                leftSelection.Position = new SharpDX.Vector2(leftSelection.Position.X, AnimationUtility.GetAnimationValue(leftSelection.Position.Y, currentIndex * 20 + 17));
                errorString.Alpha = loader.Error ? AnimationUtility.IncreaseAlpha(errorString.Alpha) : AnimationUtility.DecreaseAlpha(errorString.Alpha);
            }
        }

        class FeedComponent : GameComponent
        {
            PPDFramework.Resource.ResourceManager resourceManager;
            Feed feed;

            EffectObject loading;

            private bool error;
            private bool generated;

            public bool Finished
            {
                get;
                private set;
            }

            public FeedComponent(PPDDevice device, PPDFramework.Resource.ResourceManager resourceManager, Feed feed) : base(device)
            {
                this.resourceManager = resourceManager;
                this.feed = feed;

                var thread = ThreadManager.Instance.GetThread(Load);
                thread.Start();
                loading = new EffectObject(device, resourceManager, Utility.Path.Combine("loading.etd"))
                {
                    Position = new SharpDX.Vector2(300, 225),
                    PlayType = Effect2D.EffectManager.PlayType.Loop
                };
                loading.Play();
                this.AddChild(loading);

                Alpha = 0;
            }

            private void Load()
            {
                try
                {
                    string targetLanguage = "";
                    switch (PPDSetting.Setting.LangISO)
                    {
                        case "jp":
                            targetLanguage = "ja";
                            break;
                        default:
                            targetLanguage = "en";
                            break;
                    }

                    var webReq = (HttpWebRequest)HttpWebRequest.Create(String.Format(@"http://projectdxxx.me/feed/{0}", feed.Path));
                    var res = (HttpWebResponse)webReq.GetResponse();

                    var sb = new StringBuilder();
                    bool shouldRead = false;
                    using (XmlReader reader = XmlReader.Create(res.GetResponseStream(), new XmlReaderSettings { IgnoreWhitespace = true }))
                    {
                        while (reader.Read())
                        {
                            if (reader.IsStartElement())
                            {
                                switch (reader.LocalName)
                                {
                                    case "Feed":
                                        if (reader.GetAttribute("Language") == targetLanguage)
                                        {
                                            feed.Title = reader.GetAttribute("Title");
                                            feed.Author = reader.GetAttribute("Author");
                                            sb.Append(reader.ReadString().Trim());
                                            reader.MoveToElement();
                                            if (reader.LocalName == "br")
                                            {
                                                sb.Append("\n");
                                            }
                                            shouldRead = true;
                                        }
                                        else
                                        {
                                            shouldRead = false;
                                        }
                                        break;
                                }
                            }
                            else
                            {
                                if (shouldRead)
                                {
                                    sb.Append(reader.ReadString().Trim());
                                    reader.MoveToElement();
                                    if (reader.LocalName == "br")
                                    {
                                        sb.Append("\n");
                                    }
                                }
                            }
                        }
                    }

                    res.Close();
                    feed.Text = sb.ToString();
                    Generate();
                }
                catch
                {
                    error = true;
                }
                Finished = true;
            }

            private void Generate()
            {
                this.AddChild(new TextureString(device, feed.Title, 20, PPDColors.White)
                {
                    Position = new SharpDX.Vector2(10, 10)
                });
                this.AddChild(new TextureString(device, feed.Date.ToShortDateString(), 12, PPDColors.White)
                {
                    Position = new SharpDX.Vector2(10, 40)
                });
                this.AddChild(new TextureString(device, feed.Author, 12, PPDColors.White)
                {
                    Position = new SharpDX.Vector2(100, 40)
                });
                this.AddChild(new RectangleComponent(device, resourceManager, PPDColors.White) { Position = new SharpDX.Vector2(10, 35), RectangleWidth = 600, RectangleHeight = 1 });

                float y = 70;
                foreach (string str in feed.Text.Split('\n'))
                {
                    var textureString = new TextureString(device, str, 14, 500, 800, true, PPDColors.White)
                    {
                        Position = new SharpDX.Vector2(30, y)
                    };
                    textureString.Update();
                    this.AddChild(textureString);
                    y += textureString.MultiLineHeight;
                }
            }

            protected override void UpdateImpl()
            {
                if (!generated)
                {
                    Generate();
                    generated = true;
                }
                this.Alpha = AnimationUtility.IncreaseAlpha(this.Alpha);
                loading.Alpha = 1 - this.Alpha;
            }

            protected override bool OnCanUpdate()
            {
                return !error && Finished;
            }
        }


        class FeedListLoader
        {
            List<Feed> feedList;

            public Feed[] List
            {
                get
                {
                    return feedList.ToArray();
                }
            }

            public bool Loading
            {
                get;
                private set;
            }

            public bool Error
            {
                get;
                private set;
            }

            public bool Finished
            {
                get;
                set;
            }

            public FeedListLoader()
            {
                feedList = new List<Feed>();
            }

            public void GetList()
            {
                var thread = ThreadManager.Instance.GetThread(InnerGetList);
                Loading = true;
                thread.Start();
            }

            private void InnerGetList()
            {
                try
                {
                    var webReq = (HttpWebRequest)HttpWebRequest.Create(@"http://projectdxxx.me/feed/FeedList.xml");
                    var res = (HttpWebResponse)webReq.GetResponse();

                    using (XmlReader reader = XmlReader.Create(res.GetResponseStream(), new XmlReaderSettings()))
                    {
                        while (reader.Read())
                        {
                            if (reader.IsStartElement())
                            {
                                switch (reader.LocalName)
                                {
                                    case "FeedList":
                                        ReadFeed(reader.ReadSubtree());
                                        break;
                                }
                            }
                        }
                    }

                    res.Close();
                    feedList.Sort(Compare);
                    while (feedList.Count > 10)
                    {
                        feedList.RemoveAt(feedList.Count - 1);
                    }
                }
                catch
                {
                    Error = true;
                }

                Loading = false;
                Finished = true;
            }

            private int Compare(Feed x, Feed y)
            {
                return -x.Date.CompareTo(y.Date);
            }

            private void ReadFeed(XmlReader reader)
            {
                while (reader.Read())
                {
                    if (reader.IsStartElement())
                    {
                        switch (reader.LocalName)
                        {
                            case "Feed":
                                var path = reader.GetAttribute("Path");
                                var date = new DateTime(int.Parse(reader.GetAttribute("Year")), int.Parse(reader.GetAttribute("Month")), int.Parse(reader.GetAttribute("Day")));
                                feedList.Add(new Feed { Path = path, Date = date });
                                break;
                        }
                    }
                }
                reader.Close();
            }
        }

        class Feed
        {
            public string Path
            {
                get;
                set;
            }

            public DateTime Date
            {
                get;
                set;
            }

            public string Title
            {
                get;
                set;
            }

            public string Author
            {
                get;
                set;
            }

            public string Text
            {
                get;
                set;
            }
        }
    }
}

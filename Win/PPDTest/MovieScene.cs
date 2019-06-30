using PPDFramework;
using System.Linq;

namespace PPDTest
{
    class MovieScene : TestSceneBase
    {
        protected override string Title
        {
            get
            {
                return "動画表示のテスト";
            }
        }

        public MovieScene(TestSceneManager testSceneManager, PPDDevice device) : base(testSceneManager, device)
        {

        }

        protected override void OnInitialize()
        {
            var songInfo = SongInformation.All.FirstOrDefault(s => s.IsPPDSong);
            if (songInfo != null)
            {
                var movie = GameHost.GetMovie(songInfo);
                movie.Initialize();
                contentSprite.AddChild((GameComponent)movie);
                movie.Play();
            }
        }
    }
}

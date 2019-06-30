using PPDFramework;

namespace PPDMulti
{
    public class GameInformation : GameInformationBase
    {
        public override string GameDescription
        {
            get { return Utility.Language["GameDescription"]; }
        }

        public override string GameIconPath
        {
            get { return "ppdmulti.png"; }
        }

        public override string GameName
        {
            get { return "Project -Project Dxxx- MultiPlayer"; }
        }
    }
}

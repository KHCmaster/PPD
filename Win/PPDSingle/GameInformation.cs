using PPDFramework;

namespace PPDSingle
{
    public class GameInformation : GameInformationBase
    {
        public override string GameDescription
        {
            get { return Utility.Language["GameDescription"]; }
        }

        public override string GameIconPath
        {
            get { return "ppd.png"; }
        }

        public override string GameName
        {
            get { return "Project -Project Dxxx-"; }
        }
    }
}

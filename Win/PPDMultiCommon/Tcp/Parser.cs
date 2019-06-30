using MessagePack;
using PPDMultiCommon.Data;
using PPDMultiCommon.Model;
using System;

namespace PPDMultiCommon.Tcp
{
    class Parser
    {
        public static NetworkData Parse(byte[] bytes, out byte[] rest)
        {
            rest = null;
            var networkData = MessagePackSerializer.Deserialize<NetworkData>(bytes);
            switch (networkData.MethodType)
            {
                case MethodType.AddUser:
                    networkData = ParseAndGetRest<AddUserNetworkData>(bytes, out rest);
                    break;
                case MethodType.AddMessage:
                    networkData = ParseAndGetRest<AddMessageNetworkData>(bytes, out rest);
                    break;
                case MethodType.AddPrivateMessage:
                    networkData = ParseAndGetRest<AddPrivateMessageNetworkData>(bytes, out rest);
                    break;
                case MethodType.ChangeUserState:
                    networkData = ParseAndGetRest<ChangeUserStateNetworkData>(bytes, out rest);
                    break;
                case MethodType.HasSong:
                    networkData = ParseAndGetRest<HasSongNetworkData>(bytes, out rest);
                    break;
                case MethodType.MainGameLoaded:
                    networkData = ParseAndGetRest<MainGameLoadedNetworkData>(bytes, out rest);
                    break;
                case MethodType.ChangeScore:
                    networkData = ParseAndGetRest<ChangeScoreNetworkData>(bytes, out rest);
                    break;
                case MethodType.ChangeLife:
                    networkData = ParseAndGetRest<ChangeLifeNetworkData>(bytes, out rest);
                    break;
                case MethodType.ChangeEvaluate:
                    networkData = ParseAndGetRest<ChangeEvaluateNetworkData>(bytes, out rest);
                    break;
                case MethodType.SendResult:
                    networkData = ParseAndGetRest<SendResultNetworkData>(bytes, out rest);
                    break;
                case MethodType.AddEffect:
                    networkData = ParseAndGetRest<AddEffectNetworkData>(bytes, out rest);
                    break;
                case MethodType.AddEffectToPlayer:
                    networkData = ParseAndGetRest<AddEffectToPlayerNetworkData>(bytes, out rest);
                    break;
                case MethodType.ChangeGameRule:
                    networkData = ParseAndGetRest<ChangeGameRuleNetworkData>(bytes, out rest);
                    break;
                case MethodType.ChangeSong:
                    networkData = ParseAndGetRest<ChangeSongNetworkData>(bytes, out rest);
                    break;
                case MethodType.Ping:
                    networkData = ParseAndGetRest<PingNetworkData>(bytes, out rest);
                    break;
                case MethodType.JustGoToMenu:
                    networkData = ParseAndGetRest<JustGoToMenuNetworkData>(bytes, out rest);
                    break;
                case MethodType.ForceStart:
                    networkData = ParseAndGetRest<ForceStartNetworkData>(bytes, out rest);
                    break;
                case MethodType.SendScoreList:
                    networkData = ParseAndGetRest<SendScoreListNetworkData>(bytes, out rest);
                    break;
                case MethodType.KickUser:
                    networkData = ParseAndGetRest<KickUserNetworkData>(bytes, out rest);
                    break;
                case MethodType.ChangeLeader:
                    networkData = ParseAndGetRest<ChangeLeaderNetworkData>(bytes, out rest);
                    break;
                case MethodType.DeleteUser:
                    networkData = ParseAndGetRest<DeleteUserNetworkData>(bytes, out rest);
                    break;
                case MethodType.CloseConnect:
                    networkData = ParseAndGetRest<CloseConnectNetworkData>(bytes, out rest);
                    break;
                case MethodType.GoToPlay:
                    networkData = ParseAndGetRest<GoToPlayNetworkData>(bytes, out rest);
                    break;
                case MethodType.SendServerInfo:
                    networkData = ParseAndGetRest<SendServerInfoNetworkData>(bytes, out rest);
                    break;
                case MethodType.PingUser:
                    networkData = ParseAndGetRest<PingUserNetworkData>(bytes, out rest);
                    break;
                case MethodType.GoToPlayPrepare:
                    networkData = ParseAndGetRest<GoToPlayPrepareNetworkData>(bytes, out rest);
                    break;
                case MethodType.PlayMainGame:
                    networkData = ParseAndGetRest<PlayMainGameNetworkData>(bytes, out rest);
                    break;
                case MethodType.GoToMenu:
                    networkData = ParseAndGetRest<GoToMenuNetworkData>(bytes, out rest);
                    break;
                default:
                    networkData = null;
                    break;
            }
            return networkData;
        }

        private static NetworkData ParseAndGetRest<T>(byte[] bytes, out byte[] rest) where T : NetworkData
        {
            var networkData = MessagePackSerializer.Deserialize<T>(bytes);
            var serialized = MessagePackSerializer.Serialize(networkData);
            rest = new byte[bytes.Length - serialized.Length];
            Array.Copy(bytes, serialized.Length, rest, 0, rest.Length);
            return networkData;
        }
    }
}

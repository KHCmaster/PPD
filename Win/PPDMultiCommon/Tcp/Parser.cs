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
            networkData = networkData.MethodType switch
            {
                MethodType.AddUser => ParseAndGetRest<AddUserNetworkData>(bytes, out rest),
                MethodType.AddMessage => ParseAndGetRest<AddMessageNetworkData>(bytes, out rest),
                MethodType.AddPrivateMessage => ParseAndGetRest<AddPrivateMessageNetworkData>(bytes, out rest),
                MethodType.ChangeUserState => ParseAndGetRest<ChangeUserStateNetworkData>(bytes, out rest),
                MethodType.HasSong => ParseAndGetRest<HasSongNetworkData>(bytes, out rest),
                MethodType.MainGameLoaded => ParseAndGetRest<MainGameLoadedNetworkData>(bytes, out rest),
                MethodType.ChangeScore => ParseAndGetRest<ChangeScoreNetworkData>(bytes, out rest),
                MethodType.ChangeLife => ParseAndGetRest<ChangeLifeNetworkData>(bytes, out rest),
                MethodType.ChangeEvaluate => ParseAndGetRest<ChangeEvaluateNetworkData>(bytes, out rest),
                MethodType.SendResult => ParseAndGetRest<SendResultNetworkData>(bytes, out rest),
                MethodType.AddEffect => ParseAndGetRest<AddEffectNetworkData>(bytes, out rest),
                MethodType.AddEffectToPlayer => ParseAndGetRest<AddEffectToPlayerNetworkData>(bytes, out rest),
                MethodType.ChangeGameRule => ParseAndGetRest<ChangeGameRuleNetworkData>(bytes, out rest),
                MethodType.ChangeSong => ParseAndGetRest<ChangeSongNetworkData>(bytes, out rest),
                MethodType.Ping => ParseAndGetRest<PingNetworkData>(bytes, out rest),
                MethodType.JustGoToMenu => ParseAndGetRest<JustGoToMenuNetworkData>(bytes, out rest),
                MethodType.ForceStart => ParseAndGetRest<ForceStartNetworkData>(bytes, out rest),
                MethodType.SendScoreList => ParseAndGetRest<SendScoreListNetworkData>(bytes, out rest),
                MethodType.KickUser => ParseAndGetRest<KickUserNetworkData>(bytes, out rest),
                MethodType.ChangeLeader => ParseAndGetRest<ChangeLeaderNetworkData>(bytes, out rest),
                MethodType.DeleteUser => ParseAndGetRest<DeleteUserNetworkData>(bytes, out rest),
                MethodType.CloseConnect => ParseAndGetRest<CloseConnectNetworkData>(bytes, out rest),
                MethodType.GoToPlay => ParseAndGetRest<GoToPlayNetworkData>(bytes, out rest),
                MethodType.SendServerInfo => ParseAndGetRest<SendServerInfoNetworkData>(bytes, out rest),
                MethodType.PingUser => ParseAndGetRest<PingUserNetworkData>(bytes, out rest),
                MethodType.GoToPlayPrepare => ParseAndGetRest<GoToPlayPrepareNetworkData>(bytes, out rest),
                MethodType.PlayMainGame => ParseAndGetRest<PlayMainGameNetworkData>(bytes, out rest),
                MethodType.GoToMenu => ParseAndGetRest<GoToMenuNetworkData>(bytes, out rest),
                _ => SkipUnknown(bytes, out rest),
            };
            return networkData;
        }

        // 知らない種類のデータは 1 つ分を読み飛ばす（元は残りがなくなり、読む側が例外で止まっていた）
        private static NetworkData SkipUnknown(byte[] bytes, out byte[] rest)
        {
            var reader = new MessagePackReader(bytes);
            reader.Skip();
            rest = bytes[(int)reader.Consumed..];
            return null;
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

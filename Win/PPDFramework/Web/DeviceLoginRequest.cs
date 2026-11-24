namespace PPDFramework.Web
{
    /// <summary>
    /// 始めたブラウザ経由のログインです（<see cref="WebManager.StartDeviceLogin"/>）。
    /// </summary>
    /// <param name="DeviceCode">承認を確かめるときに送る秘密のコード。</param>
    /// <param name="UserCode">ユーザーに見せて、ブラウザで承認してもらうコード。</param>
    /// <param name="VerificationUrl">ブラウザで開く承認のページ（コード入り）。</param>
    /// <param name="Interval">承認を確かめる間隔（秒）。</param>
    /// <param name="ExpiresIn">承認の期限（秒）。</param>
    public record DeviceLoginRequest(string DeviceCode, string UserCode, string VerificationUrl, int Interval, int ExpiresIn);
}

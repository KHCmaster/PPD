using System.Runtime.CompilerServices;
using System.Text;

// 全ての exe に Directory.Build.targets からリンクされる。Main より前に、.NET Framework と同じ動作にするための設定を行う。
namespace PPDShared
{
    internal static class ProcessInitializer
    {
        [ModuleInitializer]
        internal static void Initialize()
        {
            // .NET では Shift_JIS（932）などのコードページが既定では使えないので登録する。
            // FlowScript の EncodingType、BMS の読み込み、ANSI コードページ（GetACP）、zip のファイル名などで使う
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#if PPD_WINFORMS
            // WinForms の既定フォントを .NET Framework と同じにする（.NET では Segoe UI 9pt になり、画面の配置が変わる）。
            // .NET Framework の Control.DefaultFont は SystemFonts.DefaultFont（日本語の Windows では MS UI Gothic 9pt）
            var font = System.Drawing.SystemFonts.DefaultFont;
            if (font != null)
            {
                System.Windows.Forms.Application.SetDefaultFont(font);
            }
#endif
        }
    }
}

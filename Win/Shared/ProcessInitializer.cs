using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
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
            // 見つからない dll を、アプリのフォルダーからも探す。
            // .NET では、deps.json に書かれていない dll は、それを使う dll と同じフォルダーしか探さないので、
            // PPD が skins から読み込む dll（PPDSingle など）や Mod の dll が、アプリのフォルダーの dll（SharpZipLib など）を見つけられない。
            // .NET Framework では、アプリのフォルダーも探していた
            AssemblyLoadContext.Default.Resolving += (context, name) => ResolveFromDirectory(context, name, AppContext.BaseDirectory);
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

        /// <summary>
        /// フォルダーに、名前の dll があれば読み込みます。
        /// </summary>
        /// <returns>見つからないときは null（ほかの方法で探す）。</returns>
        internal static Assembly ResolveFromDirectory(AssemblyLoadContext context, AssemblyName name, string directory)
        {
            if (String.IsNullOrEmpty(name.Name))
            {
                return null;
            }
            var path = Path.Combine(directory, name.Name + ".dll");
            return File.Exists(path) ? context.LoadFromAssemblyPath(path) : null;
        }
    }
}

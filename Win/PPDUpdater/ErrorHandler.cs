using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;

namespace PPDUpdater
{
    public class ErrorHandler
    {

        public void Initialize()
        {
            Thread.GetDomain().UnhandledException += Program_UnhandledException;
        }

        /// <summary>
        /// UI のスレッドで起きた例外も処理します（以前の WinForms の Application.ThreadException と同じく、処理を続ける）。
        /// </summary>
        public void Attach(Application application)
        {
            application.DispatcherUnhandledException += (sender, e) =>
            {
                ProcessError(e.Exception);
                e.Handled = true;
            };
        }

        void Program_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ProcessError(e.ExceptionObject as Exception);
        }

        public void ProcessError(Exception e)
        {
            try
            {
                OnProcessError(e);
            }
            finally
            {
                WriteError(e);
            }
        }

        protected virtual void OnProcessError(Exception e)
        {
            MessageBox.Show("Fatal Error Occurred\n");
        }

        static void WriteError(Exception e)
        {
            try
            {
                using StreamWriter writer = new("error.log", true);
                writer.WriteLine("------------------------------------------------------------------");
                writer.WriteLine(String.Format("{0}", DateTime.Now));
                WriteAssembly(Assembly.GetEntryAssembly(), writer);
                if (e != null)
                {
                    WriteErrorImpl(e, writer);
                }
                else
                {
                    writer.WriteLine("Exception is Null.");
                }
                writer.WriteLine("------------------------------------------------------------------");
            }
            catch
            {

            }
        }

        static void WriteAssembly(Assembly assembly, StreamWriter writer)
        {
            writer.WriteLine("FullName:{0}", assembly.FullName);
            WriteBinaryInfo(assembly.Location, writer);
            foreach (AssemblyName name in assembly.GetReferencedAssemblies())
            {
                writer.WriteLine("RefAssembly:{0}", name.FullName);
            }
        }

        static void WriteBinaryInfo(string filePath, StreamWriter writer)
        {
            try
            {
                writer.WriteLine("Assembly:{0}", filePath);
                writer.WriteLine("Name:{0}", Path.GetFileName(filePath));
                var fileVersionInfo = FileVersionInfo.GetVersionInfo(filePath);
                writer.WriteLine("FileVersion:{0}", fileVersionInfo.FileVersion);
                writer.WriteLine("ProductVersion:{0}", fileVersionInfo.ProductVersion);
            }
            catch
            {
            }
        }

        static void WriteErrorImpl(Exception e, StreamWriter writer)
        {
            writer.WriteLine(e.Data);
            writer.WriteLine(e.HelpLink);
            writer.WriteLine(e.Message);
            writer.WriteLine(e.Source);
            writer.WriteLine(e.StackTrace);
            writer.WriteLine(e.TargetSite);
            if (e.InnerException != null)
            {
                WriteErrorImpl(e.InnerException, writer);
            }
        }
    }
}

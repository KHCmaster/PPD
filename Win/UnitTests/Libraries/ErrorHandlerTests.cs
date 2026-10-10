using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace UnitTests.Libraries
{
    /// <summary>
    /// 落ちたときにエラーを error.log に書く処理（共通の ErrorHandler と、アップデーターの ErrorHandler）のテストです。
    /// </summary>
    /// <remarks>
    /// メッセージボックスを出すところは、テスト用の派生クラスで置き換える。error.log は今のフォルダー（一時フォルダー）に書く。
    /// </remarks>
    public class ErrorHandlerTests
    {
        sealed class TestErrorHandler : ErrorHandle.ErrorHandler
        {
            public List<Exception> Shown { get; } = [];

            protected override void OnProcessError(Exception e) => Shown.Add(e);
        }

        sealed class TestUpdaterErrorHandler : global::PPDUpdater.ErrorHandler
        {
            public List<Exception> Shown { get; } = [];

            protected override void OnProcessError(Exception e) => Shown.Add(e);
        }

        static Exception Thrown()
        {
            try
            {
                try
                {
                    throw new InvalidOperationException("内側のエラー");
                }
                catch (Exception inner)
                {
                    throw new ApplicationException("外側のエラー", inner);
                }
            }
            catch (Exception e)
            {
                return e;
            }
        }

        [Fact]
        public void ErrorHandler_ShowsAndAppendsToLog()
        {
            using var folder = new TempFolder(setCurrentDirectory: true);
            var handler = new TestErrorHandler();
            var error = Thrown();

            handler.ProcessError(error);
            handler.ProcessError(null);

            Assert.Equal([error, null], handler.Shown);
            var log = File.ReadAllText(folder.Combine("error.log"));
            // 内側のエラーまで書き、書き足していく
            Assert.Contains("外側のエラー", log);
            Assert.Contains("内側のエラー", log);
            Assert.Contains("Exception is Null.", log);
            Assert.Contains("FullName:", log);
        }

        [Fact]
        public void UpdaterErrorHandler_ShowsAndAppendsToLog()
        {
            using var folder = new TempFolder(setCurrentDirectory: true);
            var handler = new TestUpdaterErrorHandler();
            var error = Thrown();

            handler.ProcessError(error);
            handler.ProcessError(null);

            Assert.Equal([error, null], handler.Shown);
            var log = File.ReadAllText(folder.Combine("error.log"));
            Assert.Contains("外側のエラー", log);
            Assert.Contains("内側のエラー", log);
            Assert.Contains("Exception is Null.", log);
        }
    }
}

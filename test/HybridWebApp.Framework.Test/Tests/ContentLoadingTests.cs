using HybridWebApp.Toolkit;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace HybridWebApp.Framework.Test.Tests
{
    [TestClass]
    public class ContentLoadingTests
    {
        [TestMethod]
        public async Task Valid_Css_Loads_Without_Throwing_An_Exception()
        {
            Exception thrownException = null;

            var action = new Func<Task>(async () =>
            {
                var webView = new WebView();
                var interpreter = new Interpreter(new BrowserWrapper(webView));

                var wait = true;

                webView.NavigationCompleted += async (s, a) =>
                {
                    try
                    {
                        var css = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///TestData/app.css"));
                        var cssString = await Windows.Storage.FileIO.ReadTextAsync(css);

                        await interpreter.LoadFrameworkAsync();
                        await interpreter.LoadCssAsync(cssString);
                    }
                    catch (Exception e)
                    {
                        thrownException = e;
                    }

                    wait = false;
                };

                await Task.Factory.StartNew(async () =>
                {
                    await DispatcherHelper.Dispatch(() =>
                    {
                        webView.NavigateToString("<html><body><div id='abc'></div></body></html>");
                    });
                });

                while (wait)
                {
                    await Task.Delay(1000);
                }
            });

            var runner = new AsyncUITestRunner(action);
            await runner.RunAsync();

            Assert.IsNull(thrownException);
        }

        [TestMethod]
        public async Task Valid_Css_Loads_And_Is_The_Same_As_The_Contents_Of_The_Css_File()
        {
            var actualCss = "";
            var expectedCss = "";

            Exception thrownException = null;

            var action = new Func<Task>(async () =>
            {
                var webView = new WebView();
                var interpreter = new Interpreter(new BrowserWrapper(webView));

                var wait = true;

                webView.NavigationCompleted += async (s, a) =>
                {
                    try
                    {
                        var css = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///TestData/app.css"));
                        expectedCss = await Windows.Storage.FileIO.ReadTextAsync(css);

                        await interpreter.LoadFrameworkAsync();
                        await interpreter.LoadCssAsync(expectedCss);

                        actualCss = await interpreter.EvalAsync("document.getElementsByTagName('style')[0].innerHTML");
                    }
                    catch (Exception e)
                    {
                        thrownException = e;
                    }

                    wait = false;
                };

                await Task.Factory.StartNew(async () =>
                {
                    await DispatcherHelper.Dispatch(() =>
                    {
                        webView.NavigateToString("<html><body><div id='abc'></div></body></html>");
                    });
                });

                while (wait)
                {
                    await Task.Delay(1000);
                }
            });

            var runner = new AsyncUITestRunner(action);
            await runner.RunAsync();

            Assert.IsNull(thrownException);
            Assert.IsTrue(string.Equals(expectedCss.Replace("\r", string.Empty).Replace("\n", string.Empty), actualCss, StringComparison.OrdinalIgnoreCase));
        }
    }
}

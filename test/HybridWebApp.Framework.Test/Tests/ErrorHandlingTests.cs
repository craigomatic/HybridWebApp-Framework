using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using HybridWebApp.Toolkit;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System.Threading;

namespace HybridWebApp.Framework.Test.Tests
{
    [TestClass]
    public class ErrorHandlingTests
    {
        [TestMethod]
        public async Task Calling_A_JS_Function_After_Document_Loads_Doesnt_Throw_An_Exception()
        {
            Exception thrownException = null;

            var action = new Func<Task>(async () =>
            {
                var webView = new WebView();
                var interpreter = new Interpreter(new BrowserWrapper(webView));

                var wait = true;

                webView.NavigationCompleted += (s, a) =>
                {
                    try
                    {
                        interpreter.Load("function abc() { }");
                        interpreter.Eval("abc();");
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
        public async Task Calling_A_Global_JS_Function_That_Doesnt_Exist_Gives_A_Friendly_Exception_Message()
        {
            Exception thrownException = null;

            var action = new Func<Task>(async () =>
            {
                var webView = new WebView();
                var interpreter = new Interpreter(new BrowserWrapper(webView));

                var wait = true;

                webView.NavigationCompleted += (s, a) =>
                {
                    try
                    {
                        interpreter.Eval("thisDoesntExist();");
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
                        webView.Navigate(new Uri("http://www.example.org"));
                    });
                });

                while (wait)
                {
                    await Task.Delay(1000);
                }
            });

            var runner = new AsyncUITestRunner(action);
            await runner.RunAsync();

            Assert.IsTrue(thrownException is FunctionNotFoundException);
        }

        [TestMethod]
        public async Task Calling_A_JS_Function_On_An_Object_That_Doesnt_Exist_Gives_A_Friendly_Exception_Message()
        {
            Exception thrownException = null;
            
            var action = new Func<Task>(async() =>
            {
                var webView = new WebView();
                var interpreter = new Interpreter(new BrowserWrapper(webView));

                var wait = true;

                webView.NavigationCompleted += (s, a) =>
                {
                    interpreter.Load("app = {};");

                    try
                    {
                        interpreter.Eval("app.thisDoesntExist();");
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
                        webView.Navigate(new Uri("http://www.example.org"));
                    });                    
                });

                while (wait)
                {
                    await Task.Delay(1000);
                }
            });

            var runner = new AsyncUITestRunner(action);
            await runner.RunAsync();

            Assert.IsTrue(thrownException is FunctionNotFoundException);
        }
    }
}

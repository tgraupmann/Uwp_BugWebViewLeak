using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Uwp_BugWebViewLeak
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private bool _mWaitForExit = true;
        private bool _mSendData = false;

        public MainPage()
        {
            this.InitializeComponent();
        }

        private void Current_Closed(object sender, Windows.UI.Core.CoreWindowEventArgs e)
        {
            _mWaitForExit = false;
        }

        private void WebView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            _mSendData = true;
        }

        private async Task SendData()
        {
            while (_mWaitForExit)
            {
                try
                {
                    if (_mSendData)
                    {
                        string data = string.Empty;
                        for (int i = 0; i <= 1000; ++i)
                        {
                            data += Guid.NewGuid().ToString();
                        }

                        string command = string.Format("sendFunction('{0}');", data);
                        string result = await _mWebView.InvokeScriptAsync("eval", new string[]
                        {
                            command
                        });
                        result = null;
                        GC.Collect();
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("SendData exception {0}", ex);
                }

                await Task.Delay(1000 / 30);
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Window.Current.Closed += Current_Closed;
            _ = SendData();
        }
    }
}

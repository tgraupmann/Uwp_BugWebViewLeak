using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Uwp_BugWebViewLeak
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private bool _mWaitForExit = true;
        private bool _mRecycle = true;
        private bool _mSendData = false;

        public MainPage()
        {
            this.InitializeComponent();
        }

        private void Current_Closed(object sender, Windows.UI.Core.CoreWindowEventArgs e)
        {
            _mWaitForExit = false;
        }

        private void WebView_NavigationCompleted(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs args)
        {
            _mSendData = true;
        }

        private async Task SendData()
        {
            WebView2 webView = null;
            while (_mWaitForExit)
            {
                try
                {
                    if (_mRecycle)
                    {
                        _mRecycle = false;
                        _mSendData = false;
                        _mGrid.Children.Clear();
                        if (webView != null)
                        {
                            _mGrid.Children.Remove(webView);
                            webView = null;
                            GC.Collect();
                            await Task.Delay(2000); // Wait to see WebView disappear
                        }
                        webView = new WebView2();
                        webView.Background = new SolidColorBrush(Colors.Transparent);
                        //webView.Source = new Uri("ms-appx-web:///Assets/parent.html"); //not working yet
                        webView.Source = new Uri("https://tagenigma.com/Uwp_BugWebViewLeak/parent.html");
                        webView.NavigationCompleted += WebView_NavigationCompleted;
                        _mGrid.Children.Add(webView);
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("SendData exception {0}", ex);
                }

                try
                {
                    if (_mSendData && webView != null)
                    {
                        string data = string.Empty;
                        for (int i = 0; i <= 1000; ++i)
                        {
                            data += Guid.NewGuid().ToString();
                        }

                        string command = string.Format("sendFunction('{0}');", data);
                        string result = await webView.ExecuteScriptAsync(command);
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _mRecycle = true;
        }
    }
}

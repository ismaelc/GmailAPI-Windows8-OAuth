using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Authentication.Web;
using Windows.Security.Credentials;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace OAuth2Gmail
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        /// <summary>
        /// A credential locker to securely store our Access Token 
        /// http://msdn.microsoft.com/en-us/library/windows/apps/br227089.aspx
        /// </summary>
        private PasswordVault _vault;
        private const string RESOURCE_NAME = "GoogleAccessToken";
        private const string USER = "user";

        // we use a property instead of normal field because of how PasswordVault works
        private string _accessToken
        {
            get
            {
                try
                {
                    var creds = _vault.FindAllByResource(RESOURCE_NAME).FirstOrDefault();
                    if (creds != null)
                    {
                        return _vault.Retrieve(RESOURCE_NAME, USER).Password;
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception)
                {
                    // if no access token found, the FindAllByResource method throws System.Exception: Element not found
                    return null;
                }
            }
            set
            {
                _vault.Add(new PasswordCredential(RESOURCE_NAME, USER, value));
            }
        }

        private string _logText;

        //TODO: Enable the Gmail API in your Google Developer Console to get your credentials/keys
        //LINK: https://developers.google.com/gmail/api/quickstart/quickstart-cs#step_1_enable_the_gmail_api

        private string _consumerKey = "<client ID>";
        private string _consumerSecretKey = "<client secret>";
        private string _callbackUrl = "https://71a8c891.ngrok.com/"; // Preferably a publicly accessible domain

        /// <summary>
        /// In this sample, we want to get readonly permissions 
        /// https://developers.google.com/gmail/api/auth/scopes
        /// </summary>
        private string _scope = "https://www.googleapis.com/auth/gmail.readonly"; // "email profile"; //
        private string _authorizationCode;

        public MainPage()
        {
            this.InitializeComponent();

            _vault = new PasswordVault();

            sendHttpRequestButton.Click += sendHttpRequestButton_Click;
            clearLogButton.Click += clearLogButton_Click;
            clearAccessTokenButton.Click += clearAccessToken_Click;
            getAccessTokenButton.Click += getAccessTokenButton_Click;
        }

        async void getAccessTokenButton_Click(object sender, RoutedEventArgs e)
        {
            await checkAndGetAccessToken();
        }

        void clearAccessToken_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var creds = _vault.FindAllByResource(RESOURCE_NAME).FirstOrDefault();
                if (creds != null)
                {
                    _vault.Remove(creds);
                    sendHttpRequestButton.IsEnabled = false;
                }
            }
            catch (Exception)
            {
                //
            }
        }

        void clearLogButton_Click(object sender, RoutedEventArgs e)
        {
            _logText = "";
            logTextBox.Text = "";
        }

        async void sendHttpRequestButton_Click(object sender, RoutedEventArgs e)
        {
            var apiUrl = linkedInApiUrl.Text;
            var url = apiUrl;

            if (!string.IsNullOrEmpty(apiQuery.Text))
            {
                url += "&" + apiQuery.Text;
            }

            using (var httpClient = new HttpClient())
            {
                httpClient.MaxResponseContentBufferSize = int.MaxValue;
                httpClient.DefaultRequestHeaders.ExpectContinue = false;
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + _accessToken);

                var httpRequestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(url)
                };


                var response = await httpClient.SendAsync(httpRequestMessage);

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    log(jsonString);
                }
                else
                {
                    log(response.ToString());
                }
            }
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            await checkAndGetAccessToken();
        }

        private async Task checkAndGetAccessToken()
        {
            // If we don't have an access token, we will try to get one
            if (string.IsNullOrEmpty(_accessToken))
            {
                await getAuthorizeCode();
                await getAccessToken();
            }
            sendHttpRequestButton.IsEnabled = true;
            log("Access Token is found, ready to send Gmail API request...");
        }

        private async Task getAuthorizeCode()
        {
            var url = "https://accounts.google.com/o/oauth2/auth?"
                                            + "&response_type=code"
                                            + "&client_id=" + _consumerKey
                                            + "&redirect_uri=" + Uri.EscapeDataString(_callbackUrl)
                                            + "&scope=" + Uri.EscapeDataString(_scope)
                                            + "&state=/profile"
                                            + "&access_type=online"
                                            + "&approval_prompt=auto";
                                            
            
            log(url);
            var startUri = new Uri(url);
            var endUri = new Uri(_callbackUrl);

            WebAuthenticationResult war = await WebAuthenticationBroker.AuthenticateAsync(
                                                        WebAuthenticationOptions.None,
                                                        startUri,
                                                        endUri);
            switch (war.ResponseStatus)
            {
                case WebAuthenticationStatus.Success:
                    {
                        // grab access_token and oauth_verifier
                        var response = war.ResponseData;
                        IDictionary<string, string> keyDictionary = new Dictionary<string, string>();
                        var qSplit = response.Split('?');
                        foreach (var kvp in qSplit[qSplit.Length - 1].Split('&'))
                        {
                            var kvpSplit = kvp.Split('=');
                            if (kvpSplit.Length == 2)
                            {
                                keyDictionary.Add(kvpSplit[0], kvpSplit[1]);
                            }
                        }

                        _authorizationCode = keyDictionary["code"];
                        break;
                    }
                case WebAuthenticationStatus.UserCancel:
                    {
                        log("HTTP Error returned by AuthenticateAsync() : " + war.ResponseErrorDetail.ToString());
                        break;
                    }
                default:
                case WebAuthenticationStatus.ErrorHttp:
                    log("Error returned by AuthenticateAsync() : " + war.ResponseStatus.ToString());
                    break;
            }
        }

        private async Task getAccessToken()
        {
            var httpContent = new HttpRequestMessage(HttpMethod.Post, "https://accounts.google.com/o/oauth2/token");

            var values = new List<KeyValuePair<string, string>>();
            values.Add(new KeyValuePair<string, string>("grant_type", "authorization_code"));
            values.Add(new KeyValuePair<string, string>("code", _authorizationCode));
            values.Add(new KeyValuePair<string, string>("redirect_uri", _callbackUrl));
            values.Add(new KeyValuePair<string, string>("client_id", _consumerKey));
            values.Add(new KeyValuePair<string, string>("client_secret", _consumerSecretKey));

            httpContent.Content = new FormUrlEncodedContent(values);

            using (var httpClient = new HttpClient())
            {

                var response = await httpClient.SendAsync(httpContent);
                //var response = await httpClient.GetAsync(httpRequestMessage);
                var jsonString = await response.Content.ReadAsStringAsync();

                var json = JsonObject.Parse(jsonString);
                _accessToken = json.GetNamedString("access_token");
                log("Getting New Access Token"); 
            }
        }

        private void log(string text)
        {
            _logText += string.Format("\r\n{0}:\t{1}\r\n", DateTime.Now.ToLocalTime(), text);
            logTextBox.Text = _logText;
        }
    }
}

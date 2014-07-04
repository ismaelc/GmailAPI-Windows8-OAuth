Gmail API Windows 8 Sample Code
=========

<img src="https://nuspyq.bn1.livefilestore.com/y2pj2KUjhZgXc6RO3ZG4Ppo9SbteA9_KdzHcvu61YdulBCM1OLC7mgI6-WD-4xIeVOy3PuG1AgQ55XglSMDJCD8ZNWk7CtDm8zlQrU4PltlFsI/Capture.PNG?psid=1" width="800px" />

This Windows 8 (C#/XAML) code demonstrates:

  - Getting an access token for the Gmail API using OAuth 2.0
  - Calling Gmail API (Get messages IDs)

You can view a video of the app in action [here](https://www.youtube.com/watch?v=1mJaKalv-5s)


Using this code
------------
This C#/XAML sample code is a modified version of [LinkedIn OAuth](http://code.msdn.microsoft.com/windowsapps/LinkedIn-OAuth-20-Example-408dd568).  It's re-targeted for Windows 8.1 using Microsoft Visual Studio Express 2013 for Windows.

To use this code, you need to set up your Gmail API credentials from the Google Developer Console. Check out [these instructions](https://developers.google.com/gmail/api/quickstart/quickstart-cs#step_1_enable_the_gmail_api) on how to set up your account, and get your Client ID and Client Secret keys.

Once you have your keys, open `MainPage.xaml.cs` and insert the keys as below:

        //TODO: Enable the Gmail API in your Google Developer Console to get your credentials/keys
        //LINK: https://developers.google.com/gmail/api/quickstart/quickstart-cs#step_1_enable_the_gmail_api

        private string _consumerKey = "<client ID>";
        private string _consumerSecretKey = "<client secret>";
        private string _callbackUrl = "https://google.com//"; // Preferably a publicly accessible domain

That's it.  Hit Run (F5).  You can view a video of the app running in a simulator [here](https://www.youtube.com/watch?v=1mJaKalv-5s).


using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace FlashcardAppMobile
{
    public partial class App : Application
    {
        public static readonly string writingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new MainPage());
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}

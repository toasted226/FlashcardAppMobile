using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace FlashcardAppMobile
{
    public partial class App : Application
    {
        public static readonly string userId = "toast";
        public static readonly string writingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        public static DatabaseInfo databaseInfo = new DatabaseInfo();
        public static bool databaseInitialised = false;

        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new MainPage());
            Startup();
        }

        public async void Startup()
        {
            databaseInfo.OnDatabaseInitialisationFinished += DatabaseInfo_OnDatabaseInitialisationFinished;
            await databaseInfo.InitialiseAsync();
        }

        public void DatabaseInfo_OnDatabaseInitialisationFinished(bool succeeded)
        {
            if (succeeded)
            {
                databaseInitialised = true;
            }
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

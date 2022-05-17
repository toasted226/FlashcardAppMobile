using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace FlashcardAppMobile
{
    public partial class MainPage : ContentPage
    {
        ObservableCollection<FlashcardSet> flashcardSets = new ObservableCollection<FlashcardSet>();
        public ObservableCollection<FlashcardSet> FlashcardSets { get { return flashcardSets; } }

        public MainPage()
        {
            InitializeComponent();

            flashcardSetView.ItemsSource = flashcardSets;

            Startup();
        }

        void Startup()
        {
            App.databaseInfo.OnDatabaseInitialisationFinished += DatabaseInfo_OnDatabaseInitialisationFinished;
            App.databaseInfo.OnDatabaseQueryItems += DatabaseInfo_OnDatabaseQueryItems;
            App.databaseInfo.OnDatabaseAddToContainerFinished += DatabaseInfo_OnDatabaseAddToContainerFinished;
            UpdateFlashcardSets();
        }

        private void DatabaseInfo_OnDatabaseAddToContainerFinished()
        {
            DownloadButton.IsEnabled = true;
            UploadButton.IsEnabled = true;
        }

        private void DatabaseInfo_OnDatabaseQueryItems(List<UserFlashcardInfo> userFlashcardInfos)
        {
            UserFlashcardInfo userFlashcardInfo = userFlashcardInfos[0];
            Regex regex = new Regex("\"(.*?)\"");

            foreach (UserFlashcardSet userFlashcardSet in userFlashcardInfo.UserFlashcardSets) 
            {
                string information = userFlashcardSet.InfoLine;

                var matches = regex.Matches(information);
                string setName = matches[0].Value.Replace("\"", "");
                string setDescription = matches[1].Value.Replace("\"", "");
                int version = int.Parse(matches[2].Value.Replace("\"", ""));
                string filename = setName.Trim() + ".txt";

                string path = Path.Combine(App.writingPath, filename);

                Console.WriteLine(path);

                if (!File.Exists(path))
                {
                    using (StreamWriter sw = File.CreateText(path))
                    {
                        sw.WriteLine(information);
                    }

                    FlashcardSet flashcardSet = new FlashcardSet(setName, setDescription);
                    flashcardSet.Write(userFlashcardSet.Flashcards);
                    Console.WriteLine($"Wrote {setName} to local storage");
                }
                else
                {
                    Console.WriteLine($"Found an existing flashcard set with the name {setName} on local storage\nComparing version numbers...");
                    //compare version numbers, replace if newer
                    FlashcardSet flashcardSet = new FlashcardSet(setName, setDescription);
                    if (version > flashcardSet.GetVersionNumber())
                    {
                        Console.WriteLine($"Version number of {setName} on local storage is older than the version number on the server. Replacing...");
                        flashcardSet.Write(userFlashcardSet.Flashcards, version);
                    }
                }
            }

            UpdateFlashcardSets();
            DownloadButton.IsEnabled = true;
            UploadButton.IsEnabled = true;
        }

        public void DatabaseInfo_OnDatabaseInitialisationFinished()
        {
            DownloadButton.IsEnabled = true;
            UploadButton.IsEnabled = true;
        }

        void CheckIsFlashcardSetsEmpty() 
        {
            if (flashcardSets.Count == 0)
            {
                flashcardSetView.IsVisible = false;
                viewFrame.IsVisible = false;
                noFlashcardSetsLabel.IsVisible = true;
            }
            else
            {
                flashcardSetView.IsVisible = true;
                viewFrame.IsVisible = true;
                noFlashcardSetsLabel.IsVisible = false;
            }
        }

        private void flashcardSetView_Refreshing(object sender, EventArgs e)
        {
            UpdateFlashcardSets();
        }

        private async void CreateNewFlashcardSet_Clicked(object sender, EventArgs e)
        {
            var createFlashcardSetPage = new CreateFlashcardSetPage();
            await Navigation.PushAsync(createFlashcardSetPage, true);
            createFlashcardSetPage.mainPage = this;
        }

        public void UpdateFlashcardSets() 
        {
            flashcardSets.Clear();
            flashcardSetView.SelectedItem = null;
            
            var files = Directory.EnumerateFiles(App.writingPath);
            
            if (files.Count() > 0) 
            {
                foreach (var file in files)
                {
                    FlashcardSet flashcardSet = new FlashcardSet();
                    flashcardSet.ReadName(file);
                    flashcardSets.Add(flashcardSet);
                }
            }

            CheckIsFlashcardSetsEmpty();
        }

        private async void Edit_Clicked(object sender, EventArgs e)
        {
            FlashcardSet selectedSet = (FlashcardSet)flashcardSetView.SelectedItem;

            if (selectedSet != null)
            {
                await Navigation.PushAsync(new EditFlashcardSetPage(this, selectedSet), true);
            }
            else 
            {
                await DisplayAlert("Error", "Please select a flashcard set to edit.", "OK");
            }
        }

        private async void Test_Clicked(object sender, EventArgs e)
        {
            FlashcardSet flashcardSet = (FlashcardSet)flashcardSetView.SelectedItem;

            if (flashcardSet != null)
            {
                Flashcard[] flashcards = flashcardSet.GetFlashcards();

                if (flashcards.Length > 0)
                {
                    await Navigation.PushAsync(new TestSettingsPage(flashcardSet), true);
                }
                else
                {
                    bool addFlashcards = await DisplayAlert
                    (
                        "Error", 
                        "There are no flashcards in this set. Would you like to add flashcards now?", 
                        "Yes", 
                        "No"
                    );

                    if (addFlashcards)
                    {
                        await Navigation.PushAsync(new EditFlashcardSetPage(this, flashcardSet), true);
                    }
                }
            }
            else
            {
                await DisplayAlert("Error", "Please select a flashcard set.", "OK");
            }
        }

        public async void CreateDatabaseItem() 
        {
            List<UserFlashcardSet> userFlashcardSets = new List<UserFlashcardSet>();

            if (flashcardSets.Count > 0)
            {
                Console.WriteLine("Found flashcard sets");
                foreach (var flashcardSet in flashcardSets)
                {
                    string infoLine = $"[\"{flashcardSet.SetName}\", \"{flashcardSet.SetDescription}\", \"{flashcardSet.GetVersionNumber()}\"]";
                    Flashcard[] flashcards = flashcardSet.GetFlashcards();
                    userFlashcardSets.Add(new UserFlashcardSet(infoLine, flashcards));
                }
            }

            UserFlashcardInfo userFlashcardInfo = new UserFlashcardInfo
            {
                Id = App.userId,
                UserId = App.userId,
                UserFlashcardSets = userFlashcardSets.ToArray()
            };

            await App.databaseInfo.AddItemToContainerAsync(userFlashcardInfo);
        }

        private void UploadButton_Clicked(object sender, EventArgs e)
        {
            CreateDatabaseItem();
            UploadButton.IsEnabled = false;
            DownloadButton.IsEnabled = false;
        }

        private async void DownloadButton_Clicked(object sender, EventArgs e)
        {
            await App.databaseInfo.QueryItemsAsync(App.userId);
            UploadButton.IsEnabled = false;
            DownloadButton.IsEnabled = false;
        }
    }
}

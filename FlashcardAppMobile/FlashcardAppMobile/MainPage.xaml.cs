using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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
            UpdateFlashcardSets();
        }

        void CheckIsFlashcardSetsEmpty() 
        {
            if (flashcardSets.Count == 0)
            {
                flashcardSetView.IsVisible = false;
                noFlashcardSetsLabel.IsVisible = true;
            }
            else
            {
                flashcardSetView.IsVisible = true;
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
    }
}

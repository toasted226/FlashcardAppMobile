using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace FlashcardAppMobile
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EditFlashcardSetPage : ContentPage
    {
        ObservableCollection<Flashcard> flashcards = new ObservableCollection<Flashcard>();
        public ObservableCollection<Flashcard> Flashcards { get { return flashcards; } }

        public MainPage mainPage;
        public FlashcardSet flashcardSet;

        private bool changesMade = false;

        public EditFlashcardSetPage()
        {
            NavigationPage.SetHasBackButton(this, false);
            InitializeComponent();

            flashcardsView.ItemsSource = flashcards;

            Startup();
        }

        public EditFlashcardSetPage(MainPage mainPage, FlashcardSet flashcardSet)
        {
            NavigationPage.SetHasBackButton(this, false);
            InitializeComponent();

            flashcardsView.ItemsSource = flashcards;
            this.mainPage = mainPage;
            this.flashcardSet = flashcardSet;
            setNameDisplay.Text = flashcardSet.SetName;

            Startup();
        }
        protected override bool OnBackButtonPressed()
        {
            if (changesMade)
            {
                PromptConfirmExit();
                return true;
            }

            return base.OnBackButtonPressed();
        }

        private async void PromptConfirmExit()
        {
            var canExit = await DisplayAlert
            (
                "Unsaved changes",
                "Are you sure you want to exit without saving changes?",
                "Yes",
                "No"
            );

            if (canExit)
            {
                await Navigation.PopToRootAsync(true);
            }
        }

        private void Startup()
        {
            LoadFlashcardsDisplay();
        }

        private async void LoadFlashcardsDisplay()
        {
            flashcards.Clear();

            if (flashcardSet != null)
            {
                Flashcard[] loadedFlashcards = flashcardSet.GetFlashcards(flashcardSet);

                if (loadedFlashcards.Length > 0)
                {
                    foreach (Flashcard flashcard in loadedFlashcards)
                    {
                        flashcards.Add(flashcard);
                    }
                }
                else
                {
                    noFlashcardsWarning.IsVisible = true;
                }
            }
            else 
            {
                await DisplayAlert("Error", "Flashcard set not found", "OK");
            }
        }

        private void UpdateFlashcardDisplay() 
        {
            if (flashcards.Count > 0)
            {
                noFlashcardsWarning.IsVisible = false;
            }
            else 
            {
                noFlashcardsWarning.IsVisible = true;
            }
        }

        private async void DeleteFlashcardSet_Clicked(object sender, EventArgs e)
        {
            var canDelete = await DisplayAlert
            (
                "Delete Flashcard Set", 
                "Are you sure you want to delete " + flashcardSet.SetName + "?", 
                "Delete", 
                "Cancel"
            );

            if (canDelete)
            {
                string filePath = flashcardSet.GetFilePath();
                File.Delete(filePath);
                mainPage.UpdateFlashcardSets();
                await Navigation.PopToRootAsync(true);
            }
        }

        private void AddFlashcard_Clicked(object sender, EventArgs e)
        {
            string word = wordEntry.Text;
            string translation = translationEntry.Text;

            if (IsValid(word, wordWarning, AddFlashcard) && IsValid(translation, translationWarning, AddFlashcard))
            {
                Flashcard flashcard = new Flashcard(word.Trim(), translation.Trim());
                flashcards.Add(flashcard);
                wordEntry.Text = "";
                translationEntry.Text = "";
                wordWarning.IsVisible = false;
                translationWarning.IsVisible = false;
                UpdateFlashcardDisplay();
                changesMade = true;
                SaveChanges.IsEnabled = true;
            }
        }

        private bool IsValid(string s, Label warningLabel, Button acceptButton) 
        {
            bool isValid;

            if (s != null && s != "")
            {
                isValid = true;
                acceptButton.IsEnabled = true;
                warningLabel.IsVisible = false;

                //char[] forbiddenCharacters = { '"', '[', ']', '/', '\\', '.', ':', '?', '<', '>', '|', '*' };

                if (s.Contains("\"") || s.Contains("[") || s.Contains("]") || s.Contains(".") || s.Contains("/") || s.Contains("\\") || s.Contains("?") || s.Contains("*") || s.Contains("|") || s.Contains(":") || s.Contains("<") || s.Contains(">"))
                {
                    isValid = false;
                    acceptButton.IsEnabled = false;
                    warningLabel.IsVisible = true;
                    warningLabel.Text = "Text entered contains a forbidden character.";
                }
            }
            else
            {
                isValid = false;
                acceptButton.IsEnabled = false;
                warningLabel.IsVisible = true;
                warningLabel.Text = "This field cannot be left blank.";
            }

            return isValid;
        }

        private void wordEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            IsValid(wordEntry.Text, wordWarning, AddFlashcard);
        }

        private void translationEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            IsValid(translationEntry.Text, translationWarning, AddFlashcard);
        }

        private void RemoveSelected_Clicked(object sender, EventArgs e)
        {
            Flashcard selectedFlashcard = (Flashcard)flashcardsView.SelectedItem;

            if (selectedFlashcard != null)
            {
                flashcards.Remove(selectedFlashcard);
                UpdateFlashcardDisplay();
                changesMade = true;
                SaveChanges.IsEnabled = true;
            }
        }

        private async void SaveChanges_Clicked(object sender, EventArgs e)
        {
            if (changesMade)
            {
                Flashcard[] finalFlashcards = flashcards.ToArray();

                flashcardSet.Write(finalFlashcards);
                mainPage.UpdateFlashcardSets();
                await DisplayAlert("Success", "Flashcard set updated.", "OK");
                await Navigation.PopToRootAsync(true);
            }
        }
    }
}
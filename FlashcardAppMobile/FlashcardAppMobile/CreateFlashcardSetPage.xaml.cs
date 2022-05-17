using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace FlashcardAppMobile
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CreateFlashcardSetPage : ContentPage
    {
        public MainPage mainPage;
        
        public CreateFlashcardSetPage()
        {
            InitializeComponent();
        }

        private async void Cancel_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopToRootAsync(true);
        }

        private void Accept_Clicked(object sender, EventArgs e)
        {
            string setname = flashcardSetNameEntry.Text;
            string setdescription = flashcardSetDescriptionEntry.Text;
            int version = 0;

            if (setname == null || setname == "")
            {
                nameWarning.IsVisible = true;
                nameWarning.Text = "This field cannot be left blank.";
                Accept.IsEnabled = false;
            }
            else
            {
                string information = $"[\"{setname.Trim()}\", \"{setdescription.Trim()}\", \"{version}\"]";
                string filename = setname.Trim() + ".txt";
                string path = Path.Combine(App.writingPath, filename);

                Debug.WriteLine(path);

                if (!File.Exists(path))
                {
                    using (StreamWriter sw = File.CreateText(path))
                    {
                        sw.WriteLine(information);
                    }

                    DisplayAlert("Success", "Flashcard set created successfully.", "OK");
                    mainPage.UpdateFlashcardSets();
                    Navigation.PopToRootAsync(true);
                }
                else
                {
                    DisplayAlert("Error", "A flashcard set with that name already exists.", "OK");
                }
            }
        }

        private void flashcardSetNameEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            string s = flashcardSetNameEntry.Text;

            if (s.Length > 0)
            {
                Accept.IsEnabled = true;
                nameWarning.IsVisible = false;

                //char[] forbiddenCharacters = { '"', '[', ']', '/', '\\', '.', ':', '?', '<', '>', '|', '*' };

                if (s.Contains("\"") || s.Contains("[") || s.Contains("]") || s.Contains(".") || s.Contains("/") || s.Contains("\\") || s.Contains("?") || s.Contains("*") || s.Contains("|") || s.Contains(":") || s.Contains("<") || s.Contains(">"))
                {
                    nameWarning.IsVisible = true;
                    nameWarning.Text = "Set name contains a forbidden character.";
                    Accept.IsEnabled = false;
                }
            }
            else
            {
                Accept.IsEnabled = false;
                nameWarning.Text = "This field cannot be left blank.";
                nameWarning.IsVisible = true;
            }
        }
    }        
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace FlashcardAppMobile
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TestSettingsPage : ContentPage
    {
        public FlashcardSet flashcardSet;

        public TestSettingsPage()
        {
            InitializeComponent();
        }

        public TestSettingsPage(FlashcardSet flashcardSet)
        {
            InitializeComponent();
            this.flashcardSet = flashcardSet;
            
            Startup();
        }

        void Startup() 
        {
            testType.Items.Add("Random");
            testType.Items.Add($"From {flashcardSet.SetName}");
            testType.Items.Add($"To {flashcardSet.SetName}");

            testType.SelectedIndex = 0;
        }

        private async void StartTest_Clicked(object sender, EventArgs e)
        {
            TestSettings settings = new TestSettings();

            if (testType.SelectedIndex == 0)
            {
                settings.testType = TestSettings.TestType.Random;
            }
            else if (testType.SelectedIndex == 1)
            {
                settings.testType = TestSettings.TestType.FromSet;
            }
            else if (testType.SelectedIndex == 2)
            {
                settings.testType = TestSettings.TestType.ToSet;
            }

            settings.randomiseOrder = randomiseOrder.On;
            settings.repeatMistakes = repeatMistakes.On;
            settings.caseSensitive = caseSensitive.On;
            settings.showCorrectAnswers = showCorrectAnswers.On;
            settings.randomiseQuestionTranslation = randomiseQuestionTranslation.On;

            await Navigation.PushAsync(new TestFlashcardsPage(flashcardSet, settings), true);
        }
    }
}
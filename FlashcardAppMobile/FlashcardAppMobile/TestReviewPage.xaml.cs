using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace FlashcardAppMobile
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TestReviewPage : ContentPage
    {
        public TestSettings settings;
        
        public ObservableCollection<FlashcardStatistic> flashcardStatistics = new ObservableCollection<FlashcardStatistic>();
        public ObservableCollection<FlashcardStatistic> FlashcardStatistics { get { return flashcardStatistics; } }

        public int correctAnswers;
        public int totalFlashcards;

        public TestReviewPage()
        {
            NavigationPage.SetHasNavigationBar(this, false);
            InitializeComponent();
        }

        public TestReviewPage(FlashcardStatistic[] flashcardStatistics, int correctAnswers, int totalFlashcards, TestSettings settings)
        {
            NavigationPage.SetHasNavigationBar(this, false);
            InitializeComponent();

            this.correctAnswers = correctAnswers;
            this.totalFlashcards = totalFlashcards;
            this.settings = settings;

            //assign flashcard statistics array to the observablecollection
            foreach (FlashcardStatistic flashcardStatistic in flashcardStatistics)
            {
                if (settings.showCorrectAnswers)
                {
                    this.flashcardStatistics.Add(flashcardStatistic);
                }
                else 
                {
                    if (flashcardStatistic.wrongAnswers > 0) 
                    {
                        this.flashcardStatistics.Add(flashcardStatistic);
                    }
                }
            }

            Startup();
        }

        private void Startup() 
        {
            correctAnswersLabel.Text = $"{correctAnswers} of {totalFlashcards}";

            if (correctAnswers == totalFlashcards)
            {
                correctAnswersLabel.TextColor = new Color(0, 255, 0);
            }

            //order flashcardstatistics by the number of wrong answers
            flashcardStatistics = new ObservableCollection<FlashcardStatistic>(flashcardStatistics.OrderByDescending(flashcardStatistic => flashcardStatistic.wrongAnswers));

            resultsView.ItemsSource = flashcardStatistics;
        }

        private async void HomeButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopToRootAsync(true);
        }
    }
}
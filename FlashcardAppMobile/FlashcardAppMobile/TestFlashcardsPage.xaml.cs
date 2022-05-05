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
    public partial class TestFlashcardsPage : ContentPage
    {
        public TestSettings settings;

        public FlashcardSet flashcardSet;
        public Flashcard[] flashcards;

        public List<Flashcard> incorrectFlashcards = new List<Flashcard>();
        public List<FlashcardStatistic> flashcardStatistics = new List<FlashcardStatistic>();

        private int currentFlashcard;
        private string correctAnswer;

        private bool mainSetFinished;
        private bool isFinished;

        private int correctAnswers;
        private int totalFlashcards;

        public TestFlashcardsPage()
        {
            InitializeComponent();
        }

        public TestFlashcardsPage(FlashcardSet flashcardSet, TestSettings settings)
        {
            InitializeComponent();
            this.settings = settings;
            this.flashcardSet = flashcardSet;

            Startup();
        }

        private void Startup() 
        {
            flashcards = flashcardSet.GetFlashcards();
            if (settings.randomiseOrder)
            {
                flashcards = flashcards.OrderBy(x => Guid.NewGuid()).ToArray();
            }
            totalFlashcards = flashcards.Length;

            StartTest();
        }

        private void CreateFlashcardStatistic() 
        {
            FlashcardStatistic flashcardStatistic = new FlashcardStatistic();
            flashcardStatistic.flashcardValue = flashcards[currentFlashcard].Value;
            flashcardStatistic.wrongAnswers = 0;

            bool foundExisting = false;
            foreach (var stat in flashcardStatistics)
            {
                if (stat.flashcardValue == flashcardStatistic.flashcardValue)
                {
                    foundExisting = true;
                }
            }

            if (!foundExisting)
            {
                flashcardStatistics.Add(flashcardStatistic);
            }
        }

        private void AssignQuestionAndAnswer() 
        {
            CreateFlashcardStatistic();

            if (settings.testType == TestSettings.TestType.Random)
            {
                int ran = new Random().Next(0, 2);
                if (ran == 0)
                {
                    flashcardLabel.Text = flashcards[currentFlashcard].Word;
                    correctAnswer = flashcards[currentFlashcard].Translation;
                }
                else
                {
                    flashcardLabel.Text = flashcards[currentFlashcard].Translation;
                    correctAnswer = flashcards[currentFlashcard].Word;
                }
            }
            
            if (settings.testType == TestSettings.TestType.FromSet)
            {
                flashcardLabel.Text = flashcards[currentFlashcard].Word;
                correctAnswer = flashcards[currentFlashcard].Translation;
            }
            else if (settings.testType == TestSettings.TestType.ToSet)
            {
                flashcardLabel.Text = flashcards[currentFlashcard].Translation;
                correctAnswer = flashcards[currentFlashcard].Word;
            }
        }

        private void NextFlashcard() 
        {
            currentFlashcard++;
            if (currentFlashcard >= flashcards.Length)
            {
                FinishTest();
            }
            else
            {
                progressLabel.Text = $"{currentFlashcard + 1} / {flashcards.Length}";
                AssignQuestionAndAnswer();
                correctAnswerLabel.Text = "";
                answerEntry.Text = "";
                warningLabel.IsVisible = false;
            }
        }

        private void StartTest() 
        {
            progressLabel.Text = $"{currentFlashcard + 1} / {flashcards.Length}";

            AssignQuestionAndAnswer();

            correctAnswerLabel.Text = "";
        }

        private void FinishTest()
        {
            correctAnswer = "";
            correctAnswerLabel.Text = "";
            answerEntry.Text = "";
            warningLabel.IsVisible = false;
            currentFlashcard = 0;
            mainSetFinished = true;

            if (incorrectFlashcards.Count == 0)
            {
                flashcardLabel.Text = "Test finished!";
                isFinished = true;
                SeeResults.IsEnabled = true;
            }
            else
            {
                if (settings.repeatMistakes)
                {
                    flashcards = incorrectFlashcards.ToArray();
                    incorrectFlashcards.Clear();
                    StartTest();
                }
                else
                {
                    flashcardLabel.Text = "Test finished!";
                    isFinished = true;
                    SeeResults.IsEnabled = true;
                }
            }
        }

        private void NextWord_Clicked(object sender, EventArgs e)
        {
            if (answerEntry.Text != null && answerEntry.Text.Trim() != "")
            {
                if (!isFinished)
                {
                    CheckAnswer();
                    NextFlashcard();
                }
            }
            else 
            {
                warningLabel.IsVisible = true;
            }
        }

        private bool CheckAnswer()
        {
            string answer = answerEntry.Text.Trim();

            if (!settings.caseSensitive)
            {
                if (answer.ToLower() == correctAnswer.ToLower())
                {
                    correctAnswers = !mainSetFinished ? correctAnswers + 1 : correctAnswers;
                    return true;
                }
            }
            else
            {
                if (answer == correctAnswer)
                {
                    correctAnswers = !mainSetFinished ? correctAnswers + 1 : correctAnswers;
                    return true;
                }
            }

            if (!incorrectFlashcards.Contains(flashcards[currentFlashcard]))
            {
                incorrectFlashcards.Add(flashcards[currentFlashcard]);

                //find flashcard in statistics and increment wrong answers
                foreach (var flashcardStatistic in flashcardStatistics)
                {
                    if (flashcardStatistic.flashcardValue == flashcards[currentFlashcard].Value)
                    {
                        flashcardStatistic.wrongAnswers++;
                    }
                }
            }

            return false;
        }

        private void CheckAnswerButton_Clicked(object sender, EventArgs e)
        {
            if (!isFinished)
            {
                if (answerEntry.Text != null && answerEntry.Text.Trim() != "")
                {
                    correctAnswerLabel.Text = correctAnswer;

                    if (CheckAnswer())
                    {
                        correctAnswerLabel.TextColor = new Color(0, 255, 0);
                    }
                    else
                    {
                        correctAnswerLabel.TextColor = new Color(255, 0, 0);
                    }
                }
                else 
                {
                    warningLabel.IsVisible = true;
                }
            }
        }

        private void answerEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (answerEntry.Text != "")
            {
                warningLabel.IsVisible = false;
            }
            else 
            {
                warningLabel.IsVisible = true;
            }
        }

        private async void SeeResults_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new TestReviewPage
                (
                    flashcardStatistics.ToArray(), 
                    correctAnswers, 
                    totalFlashcards,
                    settings
                ));
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FlashcardAppMobile
{
    public class FlashcardSet
    {
        public string SetName { get; set; }
        public string SetDescription { get; set; }

        public FlashcardSet()
        {
        }

        public FlashcardSet(string setName, string setDescription)
        {
            SetName = setName;
            SetDescription = setDescription;
        }

        public void ReadName(string filePath)
        {
            string infoline = File.ReadLines(filePath).First();

            Regex regex = new Regex("\"(.*?)\"");

            var matches = regex.Matches(infoline);

            SetName = matches[0].Value.Replace("\"", "");
            SetDescription = matches[1].Value.Replace("\"", "");
        }

        public void Write(Flashcard[] flashcards)
        {
            string filePath = GetFilePath();
            string information = $"[\"{SetName}\", \"{SetDescription}\"]";

            using (StreamWriter sw = File.CreateText(filePath))
            {
                sw.WriteLine(information);

                foreach (var flashcard in flashcards)
                {
                    sw.WriteLine(flashcard.Value);
                }
            }
        }

        public string GetFilePath(FlashcardSet set) 
        {
            return Path.Combine(App.writingPath, set.SetName + ".txt");
        }
        
        public string GetFilePath()
        {
            return Path.Combine(App.writingPath, SetName + ".txt");
        }        

        public Flashcard[] GetFlashcards(FlashcardSet set)
        {
            string filePath = GetFilePath(set);
            List<Flashcard> flashcards = new List<Flashcard>();

            string[] lines = File.ReadAllLines(filePath);

            foreach (string line in lines)
            {
                if (line.StartsWith("["))
                {
                    continue;
                }

                string[] parts = line.Split('|');

                if (parts[0].Trim() != "")
                {
                    Flashcard flashcard = new Flashcard(parts[0].Trim(), parts[1].Trim());

                    flashcards.Add(flashcard);
                }
            }

            return flashcards.ToArray();
        }

        public Flashcard[] GetFlashcards()
        {
            string filePath = GetFilePath(this);
            List<Flashcard> flashcards = new List<Flashcard>();

            string[] lines = File.ReadAllLines(filePath);

            foreach (string line in lines)
            {
                if (line.StartsWith("["))
                {
                    continue;
                }

                string[] parts = line.Split('|');

                if (parts[0].Trim() != "")
                {
                    Flashcard flashcard = new Flashcard(parts[0].Trim(), parts[1].Trim());

                    flashcards.Add(flashcard);
                }
            }

            return flashcards.ToArray();
        }
    }
}

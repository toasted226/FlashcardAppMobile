using System;
using System.Collections.Generic;
using System.Text;

namespace FlashcardAppMobile
{
    public class Flashcard
    {
        public string Word { get; set; }
        public string Translation { get; set; }
        public string Value { get; set; }

        public Flashcard()
        {
        }
        
        public Flashcard(string word, string translation)
        {
            Word = word;
            Translation = translation;
            Value = word + " | " + translation;
        }
    }
}

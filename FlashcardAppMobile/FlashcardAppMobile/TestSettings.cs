using System;
using System.Collections.Generic;
using System.Text;

namespace FlashcardAppMobile
{
    public class TestSettings
    {
        public enum TestType { Random, FromSet, ToSet };
        public TestType testType;
        public bool randomiseOrder;
        public bool repeatMistakes;
        public bool caseSensitive;

        public bool showCorrectAnswers;

        public bool randomiseQuestionTranslation;
    }
}

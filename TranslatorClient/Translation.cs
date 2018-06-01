using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TranslatorClient
{
    class Translation
    {
        public string ch { get; set; }
        public string en { get; set; }
        public string ru { get; set; }

        public void ChangeLine(String language, String text)
        {
            switch (language)
            {
                case "ch":
                    ch = text;
                    break;

                case "en":
                    en = text;
                    break;

                case "ru":
                    ru = text;
                    break;

                default:
                    Console.WriteLine("While changing: {0} has wrong language {1}, where en is {2}", text, language, en);
                    break;
            }
        }

        public void ClearLine(String language)
        {
            switch (language)
            {
                case "ch":
                    ch = "";
                    break;

                case "en":
                    en = "";
                    break;

                case "ru":
                    ru = "";
                    break;

                default:
                    Console.WriteLine("While removing: wrong language {0}, where en is {1}", language, en);
                    break;
            }
        }

        public bool Contains(String text)
        {
            if (ch.Equals(text)) return true;
            if (en.Equals(text)) return true;
            if (ru.Equals(text)) return true;
            return false;
        }

        public String GetTranslation(String language)
        {
            String text = "";

            if (!Contains(language))
            {
                ChangeLine(language, "No text!!!");
                Console.WriteLine("Language {0} was created for {1} line", language);
            }

            switch (language)
            {
                case "ch":
                    text = ch;
                    break;

                case "en":
                    text = en;
                    break;

                case "ru":
                    text = ru;
                    break;

                default:
                    Console.WriteLine("While getting: wrong language {0}, where en is {1}", language, en);
                    break;
            }

            return text;
        }

    }
}

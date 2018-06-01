using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TranslatorClient
{
    class TranslationsHolder
    {
        public List<Translation> translations { get; set; }

        public TranslationsHolder(List<Translation> translations)
        {
            translations = new List<Translation>();
            this.translations = translations;
        }

        public TranslationsHolder()
        {
            translations = new List<Translation>();
        }

        public void AddTranslation(Translation translation)
        {
            translations.Add(translation);
        }

    }
}

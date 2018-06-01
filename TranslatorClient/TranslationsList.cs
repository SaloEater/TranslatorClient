using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TranslatorClient
{
    class TranslationsList
    {
        public List<TranslationsHolder> translations { get; set; }

        public TranslationsList()
        {
            translations = new List<TranslationsHolder>();
        }

        public List<Translation> FindByChinese(string ch)
        {
            List<Translation> translations = new List<Translation>();
            foreach(TranslationsHolder tH in this.translations)
            {
                foreach(Translation t in tH.translations)
                {
                    if (t.ch.Equals(ch)) translations.Add(t);
                }
            }
            if (translations.Count == 0) Console.WriteLine("{0} hasn't lines", ch);
            return translations;
        }

        internal void Add(TranslationsHolder tH)
        {
            foreach(TranslationsHolder _tH in translations)
            {
                if(_tH.translations[0].ch.Equals(tH.translations[0]))
                {
                    _tH.translations.Add(tH.translations[tH.translations.Count - 1]);
                    break;
                }
            }
            translations.Add(tH);
        }
    }
}

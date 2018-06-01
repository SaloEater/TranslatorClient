using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TranslatorClient
{
    class FileUtils
    {
        public TranslationsList ReadNonFormattedFile(string filename)
        {
            string file = File.ReadAllText(filename);
            List<Translation> translationList = JsonConvert.DeserializeObject<List<Translation>>(file);
            TranslationsList translations = ToFormatted(translationList);

            return translations;
        }

        private TranslationsList ToFormatted(List<Translation> translationList)
        {
            TranslationsList translations = new TranslationsList();
            foreach (Translation translation in translationList)
            {
                TranslationsHolder translationsHolder = new TranslationsHolder();
                translationsHolder.AddTranslation(translation);
                translations.translations.Add(translationsHolder);
            }
            return translations;
        }

        internal Dictionary<string, int> DetectVersions()
        {
            Dictionary<string, int> versions;
            string filename = "versions.txt";
            if (File.Exists(filename))
            {
                string str_versions = File.ReadAllText(filename);
                versions = JsonConvert.DeserializeObject<Dictionary<string, int>>(str_versions);
            } else
            {
                versions = new Dictionary<string, int>();
                foreach(string file in LoadWorkfileNames())
                {
                    versions.Add(file, 0);
                }
            }
            return versions;
        }

        internal string LoadPreviousIP()
        {
            string ip = "";
            string filename = "previousIP.txt";
            if (File.Exists(filename))
            {
                ip = File.ReadAllText(filename);
            }
            return ip;
        }

        internal void SaveVersions(Dictionary<string, int> versions)
        {
            string filename = "versions.txt";
            File.WriteAllText(filename, JsonConvert.SerializeObject(versions));
        }

        private List<string> LoadWorkfileNames()
        {
            List<string> workfiles;
            string filename = "workfiles.txt";
            if (File.Exists(filename))
            {
                string str_versions = File.ReadAllText(filename);
                workfiles = JsonConvert.DeserializeObject<List<string>>(str_versions);
                foreach(string file in workfiles.ToArray())
                {
                    if (!File.Exists(file)) workfiles.Remove(file);
                }
            }
            else
            {
                workfiles = new List<string>();
            }
            return workfiles;
        }

        public void SaveWorkfileNames(List<string> workfiles)
        {
            string filename = "workfiles.txt";  
            foreach (string file in workfiles.ToArray())
            {
                if (!File.Exists(file)) workfiles.Remove(file);
            }
            File.WriteAllText(filename, JsonConvert.SerializeObject(workfiles));
        }

        public TranslationsList ReadFormattedFile(string filename)
        {
            filename = MakeWorkFile(filename);
            if (!File.Exists(filename))
            {
                File.Create(filename);
                filename = UnWorkFile(filename);
                TranslationsList translations = ReadNonFormattedFile(filename);
                WriteFormattedFile(filename, translations);
                return translations;
            }else
            {
                string file = File.ReadAllText(filename);
                return StringToFormatted(file);
            }
        }

        public void WriteNonFormattedFile(string filename, TranslationsList translations)
        {

        }

        public void WriteFormattedFile(string filename, TranslationsList translations)
        {
            filename = MakeWorkFile(filename);
            File.WriteAllText(filename, JsonConvert.SerializeObject(translations).Replace("},", "},\n"));
        }

        internal string MakeWorkFile(string filename)
        {
            return filename.Replace(".tsv", "_work.tsv");
        }

        internal string UnWorkFile(string filename)
        {
            return filename.Replace("_work.tsv", ".tsv");
        }

        internal static byte[] GetFileBytes(string filename)
        {
            return File.ReadAllBytes(filename);
        }

        public bool CompareFileWithServer(string filename, Client client)
        {
            int versionBuffer;
            if (!File.Exists(filename))
            {
                File.Create(filename);
                try
                {
                    TranslationsList tL = StringToFormatted(client.GetFile(filename));
                    WriteFormattedFile(filename, tL);
                }
                catch (NullReferenceException ex)
                {
                    return false;
                }                
            } else
            {
                //if (fileInfo.Length == client.GetServersideFileSize(filename)) return false;
                try
                {
                    if (client.versions[filename] != (versionBuffer = client.AskVersion(filename)))
                    {
                        AddToFormattedFile(filename, StringToFormatted(client.GetFile(filename)));
                        client.versions[filename] = versionBuffer;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (NullReferenceException ex)
                {
                    return false;
                }              
            }
            return true;
        }

        private void AddToFormattedFile(string filename, TranslationsList translationsList)
        {
            TranslationsList original = ReadFormattedFile(filename);
            original = PutTwoInOne(original, translationsList);
            WriteFormattedFile(filename, original);
        }

        internal void SaveIP(string ip)
        {
            string filename = "previousIP.txt";
            File.WriteAllText(filename, ip);
        }

        private TranslationsList PutTwoInOne(TranslationsList original, TranslationsList translationsList)
        {
            foreach (TranslationsHolder tH in original.translations)
            {
                foreach (TranslationsHolder tH2 in translationsList.translations)
                {
                    if (tH.translations[0].ch.Equals(tH2.translations[0].ch))
                    {
                        foreach (Translation t2 in tH2.translations)
                        {
                            if (t2 == null) continue;
                            bool found = false;
                            foreach (Translation t in tH.translations)
                            {
                                if (t == null) continue;
                                if (t.ru == t2.ru)
                                {
                                    found = true;
                                    break;
                                }
                            }
                            if (!found)
                            {
                                tH.translations.Add(t2);
                            }
                        }
                    }
                }
            }
            return original;
        }

        public TranslationsList StringToFormatted(string v)
        {
            return JsonConvert.DeserializeObject<TranslationsList>(v);
        }

        public String FormattedToString(TranslationsList v)
        {
            return JsonConvert.SerializeObject(v);
        }

        internal void Remove(string filename)
        {
            File.Delete(filename);
            File.Delete(MakeWorkFile(filename));
        }
    }
}

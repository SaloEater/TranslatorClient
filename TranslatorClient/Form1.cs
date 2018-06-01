using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TranslatorClient
{
    public partial class Form1 : Form
    {
        Client client = null;
        FileUtils fileUtils;
        TranslationsList chosenTranslationList;
        UIConstants uIConstants;
        Dictionary<string, TranslationsList> changedTranslations;
        bool googleTranslatedString;
        int translationsOnPage,
            translationsPerVariant;

        public Form1()
        {
            InitializeComponent();
            googleTranslatedString = false;
            changedTranslations = new Dictionary<string, TranslationsList>();
            fileUtils = new FileUtils();
            translationsOnPage = 10;
            translationsPerVariant = 50;
            uIConstants = new UIConstants(panelTranslationString, richTextBoxStringOrigin, buttonStringOrigin, richTextBoxUserWriteOrigin);

            LoadFilenames();
            LoadIP();

            this.FormClosed += new FormClosedEventHandler(Form1_FormClosed);
            this.richTextBoxIP.KeyPress += new KeyPressEventHandler(CheckIPKeys);
            this.richTextBoxVariant.KeyPress += new KeyPressEventHandler(CheckVariantKeys);
            this.richTextBoxUserTranslation.KeyPress += new KeyPressEventHandler(CheckUTKeys);
        }

        private void CheckUTKeys(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                buttonSendTranslation.PerformClick();
            }
        }

        private void CheckVariantKeys(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                buttonShowVariant.PerformClick();
            }
        }

        private void CheckIPKeys(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                buttonApplyIP.PerformClick();
            }
        }

        private void LoadIP()
        {
            richTextBoxIP.Text = fileUtils.LoadPreviousIP();
        }

        private void LoadFilenames()
        {
            if (client != null)
            {
                foreach (string file in client.versions.Keys)
                {
                    if (!listBoxFiles.Items.Contains(file)) listBoxFiles.Items.Add(file);
                    if (!changedTranslations.Keys.Contains(file)) changedTranslations.Add(file, new TranslationsList());
                }
            } else
            {
                foreach (string file in fileUtils.DetectVersions().Keys)
                {
                    if (!listBoxFiles.Items.Contains(file)) listBoxFiles.Items.Add(file);
                    if (!changedTranslations.Keys.Contains(file)) changedTranslations.Add(file, new TranslationsList());
                }
            }

            listBoxFiles.Refresh();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void buttonLoadFilesList_Click(object sender, EventArgs e)
        {
            if (client == null)
            {
                MessageBox.Show("Сначала нужно сохранить ip-адрес");
                return;
            }
            try
            {
                client.versions = client.GetFileIdsFromServer();
            } catch(NullReferenceException ex)
            {
                return;
            } catch(Exception ex)
            {
                return;
            }
            listBoxFiles.Items.Clear();
            LoadFilenames();
        }

        private void buttonOpenFile_Click(object sender, EventArgs e)
        {
            if (listBoxFiles.SelectedIndex == -1)
            {
                MessageBox.Show("Сначала выберите файл", "Открытие файла");
                return;
            }

            if (tabPageFile.Text != "null") fileUtils.WriteFormattedFile(tabPageFile.Text, chosenTranslationList);

            labelTranslationNumber.Text = "-1";

            string filename = (string)listBoxFiles.Items[listBoxFiles.SelectedIndex];
            try
            {
                if (fileUtils.CompareFileWithServer(filename, client)) Console.WriteLine("File {0} downloaded from server", filename);
            } catch
            {
                Console.WriteLine("Can't connect to server");
            }
            chosenTranslationList = fileUtils.ReadFormattedFile(filename);

            buttonTranslationPageForward.Visible = true;
            buttonTranslationPageForward.Enabled = true;
            labelTranslationPage.Text = "0";

            UpdateTranslationsOnScreen();
            tabPageFile.Text = filename;
        }

        private void UpdateTranslationsOnScreen()
        {
            ResetPanel(panelTranslations);

            int panelY = uIConstants.panelTranslationStringLocation.Y;
            List<TranslationsHolder> translations = chosenTranslationList.translations;
            for(int i = 0; i < translationsOnPage; i++ )
            {
                int index = Convert.ToInt32(labelTranslationPage.Text) * translationsOnPage + i;
                if (chosenTranslationList.translations.Count <= index)
                {
                    break;
                }
                Panel panel = CreateTranslationString(translations[index].translations[0].en, index);
                panel.Location = new Point(uIConstants.panelTranslationStringLocation.X, panelY);
                UpdatePanelColor(panel, index);
                panelTranslations.Controls.Add(panel);
                panelY += uIConstants.panelTranslationStringSize.Height * 9 / 8;
            }

            /*int index = 0;
            foreach (TranslationsHolder tH in translationsList.translations)
            {
                Panel panel = CreateTranslationString(tH.translations[0].en, index);
                panel.Location = new Point(uIConstants.panelTranslationStringLocation.X, panelY);
                UpdatePanelColor(panel, index);
                panelTranslations.Controls.Add(panel);
                panelY += uIConstants.panelTranslationStringSize.Height * 5 / 4;
                index++;
            }*/
            panelTranslations.Refresh();
        }

        private void UpdatePanelColor(Panel panel, int index)
        {
            if(chosenTranslationList.translations[index].translations.Count > 1)
            {
                panel.BackColor = Color.LightSeaGreen;
            } else
            {
                panel.BackColor = Color.Red;
            }
        }

        private void ResetPanel(Panel panel)
        {
            //Console.Write("Очищаю список: ");
            foreach (Control c in panel.Controls)
            {
                //Console.Write("{0}, ",c.Name);
                c.Dispose();
            }
            panel.Controls.Clear();
            panel.Refresh();
            //Console.WriteLine("Список очищен");
        }

        private Panel CreateTranslationString(string englishTest, int id)
        {
            Panel panel = new Panel();

            panel.Name += "_"+id;
            panel.Size = uIConstants.panelTranslationStringSize;

            RichTextBox richTextBox = new RichTextBox();
            richTextBox.Size = uIConstants.richTextBoxStringOriginSize;
            richTextBox.Location = uIConstants.richTextBoxStringOriginLocation;
            richTextBox.Text = englishTest;

            panel.Controls.Add(richTextBox);

            Button button = new Button();
            button.Size = uIConstants.buttonStringOriginSize;
            button.Location = uIConstants.buttonStringOriginLocation;
            button.Click += buttonString_Click;
            button.Text = uIConstants.buttonStringOriginText;
            button.BackColor = uIConstants.buttonStringOriginBackColor;
            button.Font = uIConstants.buttonStringOriginFont;

            panel.Controls.Add(button);

            panel.Refresh();

            //Console.WriteLine("Added {0} panel", id);

            return panel;
        }

        private void buttonString_Click(object sender, EventArgs e)
        {
            string str_translationIndex = ((Button)sender).Parent.Name.Split('_')[1];
            int translationIndex = Convert.ToInt32(str_translationIndex);
            
            int _id = translationIndex % translationsOnPage;            
                
            HaveToCreateNameForThisFunction(_id, chosenTranslationList.translations[translationIndex], translationIndex);

        }

        private void HaveToCreateNameForThisFunction(int newIndex, TranslationsHolder assignedTH, int assignedIndex)
        {
            int oldTranslationIndex = Convert.ToInt32(labelTranslationNumber.Text);

            if (oldTranslationIndex != -1)
            {
                UpdatePanelColor((Panel)panelTranslations.Controls[oldTranslationIndex % translationsOnPage], oldTranslationIndex);
            }
            UpdateTranslationView(assignedTH, assignedIndex);

            panelTranslations.Controls[newIndex].BackColor = Color.Blue;
        }

        private void UpdateTranslationView(TranslationsHolder translationsHolder, int id)
        {
            Translation firstTranslation = translationsHolder.translations[0];
            richTextBoxChineseText.Text = firstTranslation.ch;
            richTextBoxEnglishText.Text = firstTranslation.en;
            richTextBoxUserTranslation.Text = "";          
            labelTranslationNumber.Text = id.ToString();
            UpdateUserTranslations(id);
        }

        private void buttonSendTranslation_Click(object sender, EventArgs e)
        {
            if (chosenTranslationList == null)
            {
                MessageBox.Show("Сначала загрузите файл", "К первому без перевода");
                return;
            }
            if (labelTranslationNumber.Text == "-1")
            {
                MessageBox.Show("Сначала нужно выбрать строку", "Добавление перевода");
                return;
            }
            if(googleTranslatedString)
            {
                DialogResult dialogResult = MessageBox.Show("Вы только что использовали встроенный перевод, вы уверены, что предложение переведено корректно?", "Добавление перевода", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.No)
                {
                    return;
                }
            }
            int id = Convert.ToInt32(labelTranslationNumber.Text);
            Translation translation = new Translation();
            translation.ch = richTextBoxChineseText.Text;
            translation.en = richTextBoxEnglishText.Text;
            translation.ru = richTextBoxUserTranslation.Text;
            if(translation.ru.Equals(""))
            {
                return;
            }
            if(translation.ru.Replace(" ", "").Equals(""))
            {
                return;
            }
            if (translation.ru.Length > translation.en.Length)
            {
                DialogResult dialogResult = MessageBox.Show("Количество символов в русском переводе длиннее, чем в ангийском. Все равно добавить?", "Добавление перевода", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.No)
                {
                    return;
                }
            }
            foreach (Translation t in chosenTranslationList.translations[id].translations)
            {
                if (t.ru != null && translation.ru == t.ru) return;
            }
            chosenTranslationList.translations[id].translations.Add(translation);

            changedTranslations[tabPageFile.Text].Add(chosenTranslationList.translations[id]);
            UpdatePanelColor((Panel) panelTranslations.Controls[id%translationsOnPage], id);
            UpdateUserTranslations(id);
        }

        private void UpdateUserTranslations(int id)
        {
            ResetPanel(panelPreparedTranslations);

            int index = 0;
            foreach (Translation t in chosenTranslationList.translations[id].translations)
            {
                if (t.ru == null) continue;
                RichTextBox richTextBox = new RichTextBox();
                richTextBox.Text = t.ru;
                richTextBox.Size = uIConstants.richTextBoxUserWriteOriginSize;
                richTextBox.Location = new Point(uIConstants.richTextBoxUserWriteOriginLocation.X, index* uIConstants.richTextBoxUserWriteOriginSize.Height * 5 / 4);
                richTextBox.ReadOnly = true;
                panelPreparedTranslations.Controls.Add(richTextBox);
                index++;
            }

            panelPreparedTranslations.Refresh();
            panelTranslation.Refresh();
        }

        private void SaveFileNames()
        {
            List<string> files = new List<string>();
            foreach (string s in listBoxFiles.Items)
                files.Add(s);
            fileUtils.SaveWorkfileNames(files);
        }

        private void buttonSendChanges_Click(object sender, EventArgs e)
        {
            if (client == null)
            {
                MessageBox.Show("Сначала нужно сохранить ip-адрес");
                return;
            }
            foreach (string file in changedTranslations.Keys)
            {
                try
                {
                    client.SendFile(file, changedTranslations[file]);
                }
                catch (NullReferenceException ex)
                {
                    return;
                }
            }
            MessageBox.Show("Все изменения были отправлены", "Отправление текущих изменений");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (client == null)
            {
                MessageBox.Show("Сначала нужно сохранить ip-адрес");
                return;
            }
            if (tabPageFile.Text != "null")
                fileUtils.WriteFormattedFile(tabPageFile.Text, chosenTranslationList);
            foreach(string file in client.versions.Keys)
            {
                try
                {
                    client.SendFile(file, fileUtils.ReadFormattedFile(file));
                }
                catch (NullReferenceException ex)
                {
                    return;
                }
            }
            MessageBox.Show("Все файлы были отправлены", "Отправление файлов");
        }

        private void buttonRemoveFile_Click(object sender, EventArgs e)
        {
            if (client == null)
            {
                MessageBox.Show("Сначала нужно сохранить ip-адрес", "Удаление файла");
                return;
            }
            if (listBoxFiles.SelectedIndex == -1 )
            {
                MessageBox.Show("Сначала выберите файл", "Удаление файла");
                return;
            }
            string filename = (string)listBoxFiles.Items[listBoxFiles.SelectedIndex];
            if (!File.Exists(filename))
            {
                MessageBox.Show("Файл не существует", "Удаление файла");
                return;
            }
            fileUtils.Remove(filename);
            MessageBox.Show("Файл удален с компьютера", "Удаление файла");
            listBoxFiles.Items.Remove(filename);
            try
            {
                client.versions = client.GetFileIdsFromServer();
            }
            catch (NullReferenceException ex)
            {
                return;
            }
            LoadFilenames();
        }

        private void buttonFirstEmpty_Click(object sender, EventArgs e)
        {
            if(chosenTranslationList == null)
            {
                MessageBox.Show("Сначала загрузите файл", "К первому без перевода");
                return;
            }

            int id = 0;
            foreach (TranslationsHolder tH in chosenTranslationList.translations)
            {
                if(tH.translations.Count <= 1)
                {
                    ShowPageBasedOnIndex(id);
                    break;
                }
                id++;
            }
        }

        private void ShowPageBasedOnIndex(int id)
        {
            TranslationsHolder tH = chosenTranslationList.translations[id];
            double page = id / translationsOnPage;
            labelTranslationPage.Text = "" + Math.Floor(page);
            UpdateTranslationsOnScreen();
            HaveToCreateNameForThisFunction(id%translationsOnPage, tH, id);
            CheckPagingButtons();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (chosenTranslationList == null)
            {
                MessageBox.Show("Сначала загрузите файл", "К первому без перевода");
                return;
            }
            if (labelTranslationNumber.Text == "-1")
            {
                MessageBox.Show("Сначала нужно выбрать строку", "Перевод строки");
                return;
            }
            try
            {
                string translated = TranslateString(richTextBoxEnglishText.Text, "en", "ru");
                AutoClosingMessageBox.Show("Текст переводится", "Google Translate", 1000);
                richTextBoxUserTranslation.Text = translated;
                googleTranslatedString = true;
            }
            catch(Exception)
            {
                MessageBox.Show("Не удалось подключиться к серверам переводчика", "Google Translate");
            }
            
        }

        private string TranslateString(String text, String sourceLang, String targetLang)
        {
            string url = "https://translate.googleapis.com/translate_a/single?client=gtx&sl="
            + sourceLang + "&tl=" + targetLang + "&dt=t&q=" + text;
            WebRequest request = WebRequest.Create(url);
            WebResponse response = request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            string translatedText = responseFromServer.Split('\"')[1];
            Console.WriteLine("Translate \"" + text + "\" as \"" + translatedText + "\"");
            reader.Close();
            response.Close();
            return translatedText;
        }

        private void buttonApplyIP_Click(object sender, EventArgs e)
        {
            string ip = richTextBoxIP.Text;
            if (ip == "")
            {
                MessageBox.Show("Сначала нужно ввести правильный айпи адрес", "Запуск клиента");
                return;
            }
            IPAddress address;
            bool validip = IPAddress.TryParse(ip, out address);
            if (!validip)
            {
                richTextBoxIP.Text = ip.Replace(" ", "").Replace("\n", "");
                MessageBox.Show("Введенный неправильный айпи адрес. Попробуйте еще раз, возможно, программа смогла исправить введенное значение", "Запуск клиента");
                return;
            }
            if (client == null)
            {
                client = new Client(address, 80);
                client.versions = fileUtils.DetectVersions();
                LoadFilenames();
                AutoClosingMessageBox.Show(client.TryConnect() ? "Сервер работает" : "Сервер не работает", "Подключение к серверу", 1000);
            }
            else
            {
                client.ChangeIP(address);
                AutoClosingMessageBox.Show(client.TryConnect() ? "Сервер работает" : "Сервер не работает", "Подключение к серверу", 1000);
            }
        }

        private void richTextBoxUserTranslation_TextChanged(object sender, EventArgs e)
        {
            googleTranslatedString = false;
        }

        private void buttonTranslationPageBackward_Click(object sender, EventArgs e)
        {
            int currentPage = (Convert.ToInt32(labelTranslationPage.Text) - 1);
            labelTranslationPage.Text = "" + currentPage;
            CheckPagingButtons();
            UpdateTranslationsOnScreen();
        }

        private void buttonTranslationPageForward_Click(object sender, EventArgs e)
        {
            int currentPage = Convert.ToInt32(labelTranslationPage.Text) + 1;
            labelTranslationPage.Text = "" + currentPage;
            CheckPagingButtons();
            UpdateTranslationsOnScreen();
        }

        private void CheckPagingButtons()
        {
            int variantNumber = ((Convert.ToInt32(labelTranslationPage.Text) * 10) / translationsPerVariant + 1);
            richTextBoxVariant.Text = "" + variantNumber;
            if (Convert.ToInt32(labelTranslationPage.Text) <= 0)
            {
                buttonTranslationPageBackward.Visible = false;
                buttonTranslationPageBackward.Enabled = false;
            } else
            {
                buttonTranslationPageBackward.Visible = true;
                buttonTranslationPageBackward.Enabled = true;
            }

            if (Convert.ToInt32(labelTranslationPage.Text) >= chosenTranslationList.translations.Count / translationsOnPage )
            {
                buttonTranslationPageForward.Visible = false;
                buttonTranslationPageForward.Enabled = false;
            }
            else
            {
                buttonTranslationPageForward.Visible = true;
                buttonTranslationPageForward.Enabled = true;
            }
        }

        private void buttonShowVariant_Click(object sender, EventArgs e)
        {
            if (chosenTranslationList == null)
            {
                MessageBox.Show("Сначала загрузите файл", "К первому без перевода");
                return;
            }
            int variantPage = 1;
            try
            {
                variantPage = Convert.ToInt32(richTextBoxVariant.Text);
            } catch(Exception)
            {
                return;
            }
            if (variantPage < 1)
            {
                MessageBox.Show("Вариант должен быть больше 0", "Открыть вариант");
                return;
            }
            if ((variantPage - 1) * translationsPerVariant  > chosenTranslationList.translations.Count)
            {
                MessageBox.Show("Значение должно быть меньше "+ (chosenTranslationList.translations.Count/translationsPerVariant + 1), "Открыть вариант");
                return;
            }
            variantPage--;
            labelTranslationPage.Text = "" + variantPage * translationsPerVariant / translationsOnPage;
            UpdateTranslationsOnScreen();
            CheckPagingButtons();
        }

        private void buttonLoadFromServer_Click(object sender, EventArgs e)
        {

        }

        private void richTextBoxIP_TextChanged(object sender, EventArgs e)
        {

        }

        void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            fileUtils.WriteFormattedFile(tabPageFile.Text, chosenTranslationList);
            SaveFileNames();
            if(client != null)
            {
                client.SaveIP();
            }
        }
    }
}

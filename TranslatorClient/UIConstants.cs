using System;
using System.Drawing;
using System.Windows.Forms;

namespace TranslatorClient
{
    internal class UIConstants
    {
        public Size panelTranslationStringSize;
        public Point panelTranslationStringLocation;
        public Size richTextBoxStringOriginSize;
        public Point richTextBoxStringOriginLocation;
        public Size buttonStringOriginSize;
        public Point buttonStringOriginLocation;
        public String buttonStringOriginText;
        public Color buttonStringOriginBackColor;
        public Font buttonStringOriginFont;

        public Size richTextBoxUserWriteOriginSize;
        public Point richTextBoxUserWriteOriginLocation;

        public UIConstants(Panel panelTranslationString, RichTextBox richTextBoxStringOrigin, Button buttonStringOrigin, RichTextBox richTextBoxUserWriteOrigin)
        {
            panelTranslationStringSize = panelTranslationString.Size;
            panelTranslationStringLocation = panelTranslationString.Location;

            richTextBoxStringOriginSize = richTextBoxStringOrigin.Size;
            richTextBoxStringOriginLocation = richTextBoxStringOrigin.Location;

            buttonStringOriginSize = buttonStringOrigin.Size;
            buttonStringOriginLocation = buttonStringOrigin.Location;
            buttonStringOriginText = buttonStringOrigin.Text;
            buttonStringOriginBackColor = buttonStringOrigin.BackColor;
            buttonStringOriginFont = buttonStringOrigin.Font;

            richTextBoxUserWriteOriginSize = richTextBoxUserWriteOrigin.Size;
            richTextBoxUserWriteOriginLocation = richTextBoxUserWriteOrigin.Location;
        }
    }
}
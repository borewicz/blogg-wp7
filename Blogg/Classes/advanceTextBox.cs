using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Blogg
{
    public class EDAdvancedTextbox : TextBox
    {
        private string _defaultText = string.Empty;
        public string DefaultText
        {
            get
            {
                return _defaultText;
            }
            set
            {
                _defaultText = value;
                SetDefaultText();
            }
        }

        public EDAdvancedTextbox()
        {
            this.GotFocus += (sender, e) =>
            { if (this.Text.Equals(DefaultText)) { this.Text = string.Empty; } };
            this.LostFocus += (sender, e) => { SetDefaultText(); };
        }

        private void SetDefaultText()
        {
            if (this.Text.Trim().Length == 0)
                this.Text = DefaultText;
        }
    }
}

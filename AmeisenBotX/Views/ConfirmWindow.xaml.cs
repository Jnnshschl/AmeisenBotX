using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AmeisenBotX.Views
{
    /// <summary>
    /// Interaktionslogik für ConfirmWindow.xaml
    /// </summary>
    public partial class ConfirmWindow : Window
    {
        public ConfirmWindow(string title, string message, string btnOkayText = "Okay", string btnCancelText = "Cancel")
        {
            InitializeComponent();

            messageTitle.Text = title;
            messageLabel.Text = message;

            buttonOkay.Content = btnOkayText;
            buttonCancel.Content = btnCancelText;
        }

        public bool OkayPressed { get; private set; }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
            => DragMove();

        private void ButtonOkay_Click(object sender, RoutedEventArgs e)
        {
            OkayPressed = true;
            Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            OkayPressed = false;
            Close();
        }
    }
}

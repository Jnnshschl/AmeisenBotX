using System.Windows;
using System.Windows.Input;

namespace AmeisenBotX.Views
{
    public partial class ConfirmWindow : Window
    {
        public ConfirmWindow(string title, string message, string btnOkayText = "✔️ Okay", string btnCancelText = "❌ Cancel")
        {
            InitializeComponent();

            messageTitle.Content = title;
            messageLabel.Text = message;

            if (message.Length > 64)
            {
                messageLabel.FontSize = 12;
            }

            buttonOkay.Content = btnOkayText;
            buttonCancel.Content = btnCancelText;
        }

        public bool OkayPressed { get; private set; }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            OkayPressed = false;
            Close();
        }

        private void ButtonOkay_Click(object sender, RoutedEventArgs e)
        {
            OkayPressed = true;
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Point pointToWindow = Mouse.GetPosition(this);
            Point pointToScreen = PointToScreen(pointToWindow);

            System.Windows.Media.Matrix transform = PresentationSource.FromVisual(this).CompositionTarget.TransformFromDevice;
            Point mouse = transform.Transform(pointToScreen);
            Left = mouse.X - (Width / 2);
            Top = mouse.Y - (Height / 2);
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
                    => DragMove();
    }
}
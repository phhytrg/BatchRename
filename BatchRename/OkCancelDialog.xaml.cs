using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TextBox = System.Windows.Controls.TextBox;
using Window = System.Windows.Window;

namespace BatchRename
{
    /// <summary>
    /// Interaction logic for OkCancelDialog.xaml
    /// </summary>
    public partial class OkCancelDialog : Window
    {
        private bool IsClosed = false;
        List<TextBox> textBoxes = new List<TextBox>();
        public List<string> Parameters { get; set; } = new List<string>();

        private Regex _invalidCharacters = new Regex("[\\~#%&*{}/:<>?|\"-]");
        public OkCancelDialog(Point position)
        {
            InitializeComponent();

            Top = position.Y + 30 + Application.Current.MainWindow.Top + 40;
            Left = position.X + Application.Current.MainWindow.Left; ;
        }
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            foreach(var textBox in textBoxes)
            {
                if (_invalidCharacters.IsMatch(textBox.Text))
                {
                    MessageBox.Show("Filename must not contain " + _invalidCharacters.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.None, MessageBoxOptions.DefaultDesktopOnly);
                    return;
                }

                for(int i = 0; i < textBox.Text.Length; i++)
                {
                    if(textBox.Text[i] < 32)
                    {
                        MessageBox.Show("Filename contains invalid character ", "Error", MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.None, MessageBoxOptions.DefaultDesktopOnly);
                        return;
                    }
                }

                if (textBox.Text != "")
                {
                    Parameters.Add(textBox.Text.ToString());
                }
                else
                {
                    MessageBox.Show("You need to fill all fields", "Error", MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.None, MessageBoxOptions.DefaultDesktopOnly);
                    return;
                }
            }
            IsClosed = true;
            DialogResult = true;
        }

        private void cancelRenamePlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            IsClosed = true;
            DialogResult = false;
        }
        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);
            if (!IsClosed)
            {
                DialogResult = false;
            }
        }
        public void GenerateInputField(List<string> fields)
        {
            var stackPanel = new StackPanel() { Orientation = Orientation.Vertical};
            foreach (var field in fields)
            {
                TextBlock textBlock = new TextBlock();
                textBlock.Text = field;
                textBlock.Height = 24;
                textBlock.Margin = new Thickness(8, 0, 0, 0);
                stackPanel.Children.Add(textBlock);

                TextBox textBox = new TextBox(); 
                Style style = new Style(typeof(TextBox), textBox.Style);
                Trigger trigger = new Trigger()
                {
                    Property = TextBox.TextProperty,
                    Value = ""
                };
                trigger.Setters.Add(new Setter()
                {
                    Property = Control.BorderBrushProperty,
                    Value = Brushes.Red
                });
                style.Triggers.Add(trigger);
                textBox.Style = style;  

                stackPanel.Children.Add(textBox);
                textBox.Margin = new Thickness(8, 0, 8, 0);
                textBox.Height = 28;
                textBox.VerticalContentAlignment = VerticalAlignment.Center;

                textBoxes.Add(textBox);
            }
            DockPanel.SetDock(stackPanel, Dock.Top);
            this.dockPanel.Children.Add(stackPanel);
        }
    }
}

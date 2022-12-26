using BatchRename.Models;
using BatchRename.Viewmodels;
using Contract;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using MessageBox = System.Windows.Forms.MessageBox;
using Path = System.IO.Path;
using TextBox = System.Windows.Controls.TextBox;
using Window = System.Windows.Window;

namespace BatchRename
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public DataViewModel DataViewModel { get; set; } = new DataViewModel();
        public SettingsViewModel SettingsViewModel { get; set; } = new SettingsViewModel();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDllRules();
        }

        private void addItemsButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.itemTypeComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please choose item type (files or folders)", "Error");
                return;
            }
            if (this.itemTypeComboBox.SelectedItem.ToString() == "File")
            {

                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Multiselect = true;
                openFileDialog.Filter = "All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == true)
                {
                    string[] files = openFileDialog.FileNames;
                    bool isExist = false;

                    foreach (string file in files)
                    {
                        string currentName = Path.GetFileName(file);
                        string directoryPath = Path.GetDirectoryName(file)!;

                        foreach(Item item in DataViewModel.Items)
                        {
                            if(item.Name == currentName && item.Path == directoryPath)
                            {
                                isExist = true;
                                break;
                            }
                        }

                        if (!isExist){
                            DataViewModel.Items.Add(new Item() { Name = currentName, Path = directoryPath });
                        }
                    }
                }
            }
            else if (itemTypeComboBox.SelectedItem.ToString() == "Folder")
            {
                CommonOpenFileDialog dialog = new CommonOpenFileDialog();
                dialog.Multiselect = true;
                dialog.IsFolderPicker = true;
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    var folders = dialog.FileNames;
                    folders.ToList().ForEach(folder =>
                    {
                        string currentName = Path.GetFileName(folder);
                        string directoryPath = Path.GetDirectoryName(folder)!;
                        bool isExist = false;

                        foreach (Item item in DataViewModel.Items)
                        {
                            if (item.Name == currentName && item.Path == directoryPath)
                            {
                                isExist = true;
                                break;
                            }
                        }

                        if (!isExist)
                        {
                            DataViewModel.Items.Add(new Item() { Name = currentName, Path = directoryPath });
                        }
                    });
                }
            }
        }

        private void LoadDllRules()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var dlls = new DirectoryInfo(baseDir).GetFiles("Rules/*.dll");

            foreach (var dll in dlls)
            {
                var assembly = Assembly.LoadFile(dll.FullName);
                var types = assembly.GetTypes();

                foreach(var type in types)
                {
                    if (type.IsClass)
                    {
                        if (typeof(IRule).IsAssignableFrom(type))
                        {
                            IRule rule = (IRule)Activator.CreateInstance(type);
                            RuleFactory.Register(rule);
                        }
                    }
                }
            }

            DataViewModel.AvailableRules = new ObservableCollection<string>(RuleFactory.Instance().GetRules());
        }

        private void resetRulesButton_Click(object sender, RoutedEventArgs e)
        {
            LoadDllRules();
        }
        private void addRuleButton_Click(object sender, RoutedEventArgs e)
        {
            if (rulesComboBox.SelectedItem == null)
            {
                return;
            }

            var prototype = rulesComboBox.SelectedItem.ToString();
            IRule rule = RuleFactory.Instance().Builder(prototype);

            if(rule == null)
            {
                return;
            }

            if (rule.HasParameter)
            {
                IRuleWithParameters pRule = (IRuleWithParameters)rule;
                Point relativePoint = Mouse.GetPosition(Application.Current.MainWindow);
                var window = new OkCancelDialog(relativePoint);
                window.Owner = this;
                window.GenerateInputField(pRule.Keys.ToList());

                if (window.ShowDialog() == true)
                {
                    pRule.Values = window.Parameters;
                    DataViewModel.ActiveRule.Add(rule);
                }
            }
            else
            {
                DataViewModel.ActiveRule.Add(rule);
            }
            //DataViewModel.ActiveRule.Add(RuleFactory.Instance().Parse(rule)!);
        }

        private void previewButton_Click(object sender, RoutedEventArgs e)
        {
            foreach(var item in DataViewModel.Items)
            {
                string tmp = item.Name!;
                foreach (var rule in DataViewModel.ActiveRule)
                {
                    tmp = rule.Rename(tmp);

                }
                item.NewName = tmp;
            }
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in DataViewModel.Items)
            {
                string tmp = item.Name!;
                foreach (var rule in DataViewModel.ActiveRule)
                {
                    tmp = rule.Rename(tmp);

                }
                item.NewName = tmp;
            }

            foreach (var item in DataViewModel.Items)
            {
                File.Move(item.Path + @"\" + item.Name, item.Path + @"\" + item.NewName);
                item.Name = item.NewName;
            }
        }

        private void Reset()
        {
            foreach(var item in DataViewModel.Items)
            {
                item.NewName = "";
            }
        }
    }
}

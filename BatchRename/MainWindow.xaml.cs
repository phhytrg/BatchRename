using BatchRename.Models;
using BatchRename.Viewmodels;
using Contract;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.TextFormatting;
using System.Windows.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using Path = System.IO.Path;
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
            AutoSave();
            DataViewModel.BatchType = Constants.RENAME_ORIGINAL;
            DataViewModel.PresetSaveType = Constants.JSON_FILE;
        }

        #region Item Region
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

                    foreach (string file in files)
                    {
                        bool isExist = false;
                        string currentName = Path.GetFileName(file);
                        string directoryPath = Path.GetDirectoryName(file)!;

                        foreach (Item item in DataViewModel.Items)
                        {
                            if (item.Name == currentName && item.Directory == directoryPath)
                            {
                                isExist = true;
                                break;
                            }
                        }

                        if (!isExist)
                        {
                            DataViewModel.Items.Add(new Item() { Name = currentName, Directory = directoryPath });
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
                            if (item.Name == currentName && item.Directory == directoryPath)
                            {
                                isExist = true;
                                break;
                            }
                        }

                        if (!isExist)
                        {
                            DataViewModel.Items.Add(new Item() { Name = currentName, Directory = directoryPath });
                        }
                    });
                }
            }
        }
        private void removeAllItemsButton_Click(object sender, RoutedEventArgs e)
        {
            DataViewModel.Items.Clear();
        }

        #endregion

        #region rules region
        private void LoadDllRules()
        {
            RuleFactory.Instance().UnRegisterAll();
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var dlls = new DirectoryInfo(baseDir).GetFiles("Rules/*.dll");

            foreach (var dll in dlls)
            {
                var assembly = Assembly.LoadFile(dll.FullName);
                var types = assembly.GetTypes();

                foreach (var type in types)
                {
                    if (type.IsClass)
                    {
                        if (typeof(IRule).IsAssignableFrom(type))
                        {
                            IRule rule = (IRule)Activator.CreateInstance(type)!;
                            RuleFactory.Register(rule);
                        }
                    }
                }
            }

            DataViewModel.AvailableRules = new ObservableCollection<string>(RuleFactory.Instance().GetRulesType());
        }

        private void resetRulesButton_Click(object sender, RoutedEventArgs e)
        {
            DataViewModel.ActiveRule.Clear();
        }
        private void addRuleButton_Click(object sender, RoutedEventArgs e)
        {
            if (rulesComboBox.SelectedItem == null)
            {
                return;
            }

            var prototype = rulesComboBox.SelectedItem.ToString();

            foreach (var r in DataViewModel.ActiveRule)
            {
                if (r.RuleType.Equals(prototype))
                {
                    MessageBox.Show(prototype + " is available!!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            IRule rule = RuleFactory.Instance().Builder(prototype!);

            if (rule == null)
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
                    if (pRule.Errors != "")
                    {
                        MessageBox.Show(pRule.Errors, "Errors", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    DataViewModel.ActiveRule.Add(rule);
                }
            }
            else
            {
                DataViewModel.ActiveRule.Add(rule);
            }
            //DataViewModel.ActiveRule.Add(RuleFactory.Instance().Parse(rule)!);
        }
        private void importRuleButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();

            dialog.Filter = "DLL Rule (*.dll)|*.dll";
            dialog.Multiselect = true;

            if (dialog.ShowDialog() == false)
            {
                return;
            }

            var dlls = dialog.FileNames;
            var msg = "";
            foreach (var dll in dlls)
            {
                var assembly = Assembly.LoadFile(dll);
                var types = assembly.GetTypes();

                foreach (var type in types)
                {
                    if (type.IsClass)
                    {
                        if (typeof(IRule).IsAssignableFrom(type))
                        {
                            IRule rule = (IRule)Activator.CreateInstance(type)!;
                            if (!RuleFactory.Register(rule))
                            {
                                msg += $"{rule.RuleType} has already been added!\n";
                            }
                            else
                            {
                                msg += $"Rule added: {rule.RuleType}\n";
                            }
                        }
                    }
                }
            }

            if (msg != "")
            {
                MessageBox.Show(msg, "Message");
            }
            else
            {
                MessageBox.Show("No rule has been added!!!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

        }
        #endregion

        #region Apply Rules region

        private void previewButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyRules();
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyRules();
            foreach (var item in DataViewModel.Items)
            {
                FileAttributes attr = File.GetAttributes(item.Directory + "\\" + item.Name);
                if (attr.HasFlag(FileAttributes.Directory))
                {
                    if (DataViewModel.BatchType == Constants.RENAME_ORIGINAL)
                    {
                        if (Directory.Exists(item.Directory + @"\" + item.NewName))
                        {
                            item.Errors += "Directory already exist.";
                            continue;
                        }
                        Directory.Move(item.Directory + @"\" + item.Name, item.Directory + @"\" + item.NewName);
                    }
                    else if (DataViewModel.BatchType == Constants.MOVE_TO_FOLDER)
                    {
                        if (Directory.Exists(item.NewDirectory + @"\" + item.NewName))
                        {
                            item.Errors += "Directory already exist.";
                            continue;
                        }
                        CopyFilesRecursively(item.Directory + @"\" + item.Name, item.NewDirectory + @"\" + item.NewName);
                        item.Directory = item.NewDirectory;
                    }
                }
                else
                {
                    if (DataViewModel.BatchType == Constants.RENAME_ORIGINAL)
                    {
                        if (File.Exists(item.Directory + @"\" + item.NewName))
                        {
                            item.Errors += "File already exist.";
                            continue;
                        }
                        File.Move(item.Directory + @"\" + item.Name, item.Directory + @"\" + item.NewName);
                    }
                    else if (DataViewModel.BatchType == Constants.MOVE_TO_FOLDER)
                    {
                        if (File.Exists(item.NewDirectory + @"\" + item.NewName))
                        {
                            item.Errors += "File already exist.";
                            continue;
                        }
                        File.Copy(item.Directory + @"\" + item.Name, item.NewDirectory + @"\" + item.NewName);
                        item.Directory = item.NewDirectory;
                    }
                }
                item.Name = item.NewName;
            }
            Reset();
        }

        private void ApplyRules()
        {
            List<IRule> rules = new List<IRule>();

            foreach (var rule in DataViewModel.ActiveRule)
            {
                rules.Add((IRule)rule.Clone());
            }
            foreach (var item in DataViewModel.Items)
            {
                string tmp = item.Name!;
                foreach (var rule in rules)
                {
                    tmp = rule.Rename(tmp);
                }

                if (tmp.Length > 255)
                {
                    item.Errors += "Exceed maximum file length (255).";
                }
                else
                {
                    item.NewName = tmp;
                }
            }

            if (DataViewModel.BatchType == Constants.MOVE_TO_FOLDER)
            {
                CommonOpenFileDialog dialog = new CommonOpenFileDialog();
                dialog.IsFolderPicker = true;
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    var baseDirectory = dialog.FileName;
                    foreach (var item in DataViewModel.Items)
                    {
                        var filename = Path.GetFileName(item.NewName);
                        item.NewDirectory = baseDirectory;
                    }
                }
            }
        }
        private void CopyFilesRecursively(string sourcePath, string targetPath)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
            }
        }
        #endregion

        #region Preset
        private void Reset()
        {
            foreach (var item in DataViewModel.Items)
            {
                item.NewName = "";
                item.NewDirectory = "";
            }
        }

        private bool NewJsonSavedPath()
        {
            var dialog = new SaveFileDialog();
            dialog.Filter = "Json (*.json)|*.json";
            if (dialog.ShowDialog() == false)
            {
                return false; ;
            }
            DataViewModel.PresetPath = dialog.FileName;
            return true;
        }

        private bool NewTxtSavedPath()
        {
            var dialog = new SaveFileDialog();
            dialog.Filter = "Text (*.txt)|*.txt";
            if (dialog.ShowDialog() == false)
            {
                return false;
            }
            DataViewModel.PresetPath = dialog.FileName;
            return true;
        }
        private async Task SavePreset()
        {
            if (DataViewModel.ActiveRule.Count <= 0 && DataViewModel.PresetPath == "")
            {
                MessageBox.Show("No rule to save or PresetPath is empty");
                return;
            }

            DataViewModel.Status = "Saving Preset...";
            if (DataViewModel.PresetSaveType == Constants.JSON_FILE)
            {
                SavePresetToJson();
            }
            else if (DataViewModel.PresetSaveType == Constants.TEXT_FILE)
            {
                SavePresetToTxt();
            }

            DataViewModel.Status = $"Preset is Saved, path: {DataViewModel.PresetPath}";
            await Task.Factory.StartNew(() =>
            {
                Task.Delay(30000);
                DataViewModel.Status = "";
            });
        }

        private async void SavePresetToJson()
        {
            await Task.Factory.StartNew(() =>
            {
                using StreamWriter file = new(DataViewModel.PresetPath);
                string json = JsonConvert.SerializeObject(DataViewModel.ActiveRule);
                file.WriteAsync(json);
            });
        }

        private void SavePresetToTxt()
        {
            using StreamWriter file = new(DataViewModel.PresetPath);
            foreach (var rule in DataViewModel.ActiveRule)
            {
                string line = $"{rule.RuleType} {rule.ToString()}";
                file.WriteLineAsync(line);
            }
        }

        private void LoadPresetFromTextFile(string path)
        {
            var lines = File.ReadAllLines(path);
            string errors = "";
            var rules = new List<IRule>();
            foreach (var line in lines)
            {
                var rule = RuleFactory.Instance().Parse(line);
                if (rule == null)
                {
                    errors += "Line: " + line + "\n";
                    continue;
                }
                rules.Add(rule);
            }
            DataViewModel.ActiveRule = new ObservableCollection<IRule>(rules);
        }

        private void LoadPresetFromJson(string path)
        {
            string errors = "";
            StreamReader reader = new StreamReader(path);
            string json = reader.ReadToEnd();

            List<RuleJObj> jRules = JsonConvert.DeserializeObject<List<RuleJObj>>(json, new RuleObjectConverter())!;

            if (jRules == null)
            {
                return;
            }

            var rules = new List<IRule>();
            foreach (var jRule in jRules)
            {
                if (jRule == null)
                {
                    continue;
                }
                var rule = RuleFactory.Instance().ParseRuleFromJObj(jRule);
                if (rule == null)
                {
                    errors += $"{jRule.RuleType} is not exist!!\n";
                    continue;
                }
                else
                {
                    rules.Add(rule);
                }
            }
            if (rules.Count > 0)
            {
                DataViewModel.ActiveRule.Clear();
                rules.ForEach(rule =>
                {
                    DataViewModel.ActiveRule.Add(rule);
                });
            }

            if (errors != "")
            {
                MessageBox.Show("Invalid syntax!!\n" + errors, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void openPresetButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Json files (*.json)|*.json|Text files (*.txt)|*.txt";
            if (dialog.ShowDialog() == false)
            {
                return;
            }
            var extension = Path.GetExtension(dialog.FileName);
            if (extension == ".json")
            {
                LoadPresetFromJson(dialog.FileName);
            }
            else if (extension == ".txt")
            {
                LoadPresetFromTextFile(dialog.FileName);
            }
        }

        private async void savePresetButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataViewModel.ActiveRule.Count <= 0)
            {
                MessageBox.Show("No rule to save");
                return;
            }

            if (DataViewModel.PresetPath == "")
            {
                if (DataViewModel.PresetSaveType == Constants.JSON_FILE)
                {
                    if (!NewJsonSavedPath())
                    {
                        return;
                    }
                }
                else if (DataViewModel.PresetSaveType == Constants.TEXT_FILE)
                {
                    if (!NewTxtSavedPath()) { return; }
                }
            }

            if (DataViewModel.PresetSaveType == Constants.JSON_FILE && Path.GetExtension(DataViewModel.PresetPath) == ".txt")
            {
                if (!NewJsonSavedPath()) { return; }
            }

            if (DataViewModel.PresetSaveType == Constants.TEXT_FILE && Path.GetExtension(DataViewModel.PresetPath) == ".json")
            {
                if (!NewTxtSavedPath()) { return; }
            }
            await SavePreset();
        }

        private async void saveAsPresetButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataViewModel.ActiveRule.Count <= 0)
            {
                MessageBox.Show("No rule to save");
                return;
            }

            if (DataViewModel.PresetSaveType == Constants.JSON_FILE && Path.GetExtension(DataViewModel.PresetPath) == ".txt")
            {
                if (!NewJsonSavedPath()) { return; }
            }

            if (DataViewModel.PresetSaveType == Constants.TEXT_FILE && Path.GetExtension(DataViewModel.PresetPath) == ".json")
            {
                if (!NewTxtSavedPath()) { return; }
            }

            await SavePreset();
        }
        #endregion

        #region Project
        private async Task SaveProject(string path)
        {
            DataViewModel.Status = "Saving project...";
            var json = JsonConvert.SerializeObject(DataViewModel, new JsonSerializerSettings()
            {
                ContractResolver = new IgnorePropertiesResolver(new[] { "AvailableRules", "Status" })
            }).ToString();
            await Task.Factory.StartNew(() =>
            {
                File.WriteAllTextAsync(path, json);
            });
            DataViewModel.Status = $"Project is Saved, Path = {path}";
            await Task.Factory.StartNew(() =>
            {
                Thread.Sleep(30000);
                DataViewModel.Status = "";
            });
        }

        private void openProjectButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Project (*.proj)|*.proj";

            if (dialog.ShowDialog() == false)
            {
                return;
            }

            var json = File.ReadAllText(dialog.FileName);
            if (json == null)
            {
                MessageBox.Show("Unknown error!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var newDataViewModel = JsonConvert.DeserializeObject<DataViewModel>(json, new JsonSerializerSettings()
            {
                ContractResolver = new IgnorePropertiesResolver(new[] { "ActiveRule" })
            })!;
            if (newDataViewModel == null)
            {
                MessageBox.Show("Invalid file!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            DataViewModel = newDataViewModel;
        }

        private async void saveProjectButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataViewModel.Items.Count == 0 && DataViewModel.ActiveRule.Count == 0)
            {
                MessageBox.Show("Nothing to save!!", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (DataViewModel.ProjectPath == "")
            {
                var dialog = new SaveFileDialog();
                dialog.Filter = "Project (*.proj)|*.proj";
                if (dialog.ShowDialog() == false)
                {
                    return;
                }
                DataViewModel.ProjectPath = dialog.FileName;
            }

            var path = DataViewModel.ProjectPath;

            await SaveProject(path);
        }

        private async void saveAsProjectButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataViewModel.Items.Count == 0 && DataViewModel.ActiveRule.Count == 0)
            {
                MessageBox.Show("Nothing to save!!", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new SaveFileDialog();
            dialog.Filter = "Project (*.project)|*.project";

            if (dialog.ShowDialog() == false)
            {
                return;
            }

            DataViewModel.ProjectPath = dialog.FileName;
            var path = DataViewModel.ProjectPath;

            await SaveProject(path);
        }
        #endregion

        #region Rule Moving

        private CancellationTokenSource? ts = null;
        private async void toFirstButton_Click(object sender, RoutedEventArgs e)
        {
            if (rulesChosenListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("You must choose rule before starting moving!!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int count = rulesChosenListView.SelectedItems.Count;

            for (int i = count - 1; i >= 0; i--)
            {
                int index = DataViewModel.ActiveRule.IndexOf((IRule)rulesChosenListView.SelectedItems[i]!);
                var rule = DataViewModel.ActiveRule[index];
                DataViewModel.ActiveRule.RemoveAt(index);
                DataViewModel.ActiveRule.Insert(0, rule);
            }

            for (int i = 0; i < count; i++)
            {
                rulesChosenListView.SelectedItems.Add(DataViewModel.ActiveRule[i]);
            }

            using (var _ts = new CancellationTokenSource())
            {
                // previous task (if any) cancellation
                if (null != ts)
                    ts.Cancel();

                // let cancel from outside
                ts = _ts;
                try
                {
                    await RemoveSeletecItems(ts.Token);
                }
                catch (OperationCanceledException)
                {
                    //Eat exception!
                };
            }
        }

        private async void upButton_Click(object sender, RoutedEventArgs e)
        {
            if (rulesChosenListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("You must choose rule before starting moving!!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            List<int> selectedItemIndexes = new List<int>();

            for (int i = 0; i < rulesChosenListView.SelectedItems.Count; i++)
            {
                int index = DataViewModel.ActiveRule.IndexOf((IRule)rulesChosenListView.SelectedItems[i]!);
                selectedItemIndexes.Add(index);
            }

            rulesChosenListView.SelectedItems.Clear();
            selectedItemIndexes = selectedItemIndexes.OrderBy(o => o).ToList();
            List<int> finalPosition = new List<int>();

            foreach (var index in selectedItemIndexes)
            {
                if (index == 0)
                {
                    finalPosition.Add(index);
                    continue;
                }

                int toPosition = index - 1;

                if (finalPosition.Contains(toPosition))
                {
                    finalPosition.Add(index);
                    continue;
                }
                else
                {
                    var rule = DataViewModel.ActiveRule[index];
                    DataViewModel.ActiveRule.RemoveAt(index);
                    DataViewModel.ActiveRule.Insert(toPosition, rule);
                    finalPosition.Add(toPosition);
                }
            }

            foreach (var postion in finalPosition)
            {
                rulesChosenListView.SelectedItems.Add(DataViewModel.ActiveRule[postion]);
            }

            using (var _ts = new CancellationTokenSource())
            {
                // previous task (if any) cancellation
                if (null != ts)
                    ts.Cancel();

                // let cancel from outside
                ts = _ts;
                try
                {
                    await RemoveSeletecItems(ts.Token);
                }
                catch (OperationCanceledException)
                {
                    //Eat exception!
                };
            }
        }

        private async void downButton_Click(object sender, RoutedEventArgs e)
        {
            if (rulesChosenListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("You must choose rule before starting moving!!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            List<int> selectedItemIndexes = new List<int>();

            for (int i = 0; i < rulesChosenListView.SelectedItems.Count; i++)
            {
                int index = DataViewModel.ActiveRule.IndexOf((IRule)rulesChosenListView.SelectedItems[i]!);
                selectedItemIndexes.Add(index);
            }

            rulesChosenListView.SelectedItems.Clear();
            selectedItemIndexes = selectedItemIndexes.OrderByDescending(o => o).ToList();
            List<int> finalPosition = new List<int>();

            foreach (var index in selectedItemIndexes)
            {
                if (index == DataViewModel.ActiveRule.Count - 1)
                {
                    finalPosition.Add(index);
                    continue;
                }

                int toPosition = index + 1;

                if (finalPosition.Contains(toPosition))
                {
                    finalPosition.Add(index);
                    continue;
                }
                else
                {
                    var rule = DataViewModel.ActiveRule[index];
                    DataViewModel.ActiveRule.RemoveAt(index);
                    DataViewModel.ActiveRule.Insert(toPosition, rule);
                    finalPosition.Add(toPosition);
                }
            }

            foreach (var postion in finalPosition)
            {
                rulesChosenListView.SelectedItems.Add(DataViewModel.ActiveRule[postion]);
            }

            using (var _ts = new CancellationTokenSource())
            {
                // previous task (if any) cancellation
                if (null != ts)
                    ts.Cancel();

                // let cancel from outside
                ts = _ts;
                try
                {
                    await RemoveSeletecItems(ts.Token);
                }
                catch (OperationCanceledException)
                {
                    //Eat exception!
                };
            }
        }

        private async void toLastButton_Click(object sender, RoutedEventArgs e)
        {
            if (rulesChosenListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("You must choose rule before starting moving!!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int count = rulesChosenListView.SelectedItems.Count;

            for (int i = count - 1; i >= 0; i--)
            {
                int index = DataViewModel.ActiveRule.IndexOf((IRule)rulesChosenListView.SelectedItems[i]!);
                var rule = DataViewModel.ActiveRule[index];
                DataViewModel.ActiveRule.RemoveAt(index);
                DataViewModel.ActiveRule.Add(rule);
            }

            for (int i = 0; i < count; i++)
            {
                rulesChosenListView.SelectedItems.Add(DataViewModel.ActiveRule[DataViewModel.ActiveRule.Count - 1 - i]);
            }

            using (var _ts = new CancellationTokenSource())
            {
                // previous task (if any) cancellation
                if (null != ts)
                    ts.Cancel();

                // let cancel from outside
                ts = _ts;
                try
                {
                    await RemoveSeletecItems(ts.Token);
                }
                catch (OperationCanceledException)
                {
                    //Eat exception!
                };
            }
        }

        private async Task RemoveSeletecItems(CancellationToken ct)
        {
            await Task.Delay(5000, ct).ContinueWith(t =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    rulesChosenListView.SelectedItems.Clear();
                });
            }, TaskContinuationOptions.OnlyOnRanToCompletion);
        }
        #endregion

        private void AutoSave()
        {
            var path = DataViewModel.ProjectPath != "" ? DataViewModel.ProjectPath : "autosave.proj";
            var dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += async (sender, e) =>
            {
                await SaveProject(path);

            };
            dispatcherTimer.Interval = new TimeSpan(0, 0, 60);
            dispatcherTimer.Start();
        }

        #region Drag and drop items
        private void DragAndDropItems(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (files.Length <= 0)
                {
                    return;
                }

                foreach (string file in files)
                {
                    string currentName = Path.GetFileName(file);
                    string directoryPath = Path.GetDirectoryName(file)!;
                    bool isExist = false;

                    foreach (Item item in DataViewModel.Items)
                    {
                        if (item.Name == currentName && item.Directory == directoryPath)
                        {
                            isExist = true;
                            break;
                        }
                    }

                    if (!isExist)
                    {
                        DataViewModel.Items.Add(new Item() { Name = currentName, Directory = directoryPath });
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// Change rule's parameter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var rule = ((ListViewItem)sender).Content as IRule;

            if (rule == null || rule.HasParameter == false)
            {
                return;
            }

            if (rule.HasParameter)
            {
                IRuleWithParameters pRule = (IRuleWithParameters)rule.Clone();
                Point relativePoint = Mouse.GetPosition(Application.Current.MainWindow);
                var window = new OkCancelDialog(relativePoint);
                window.Owner = this;
                window.GenerateInputField(pRule.Keys.ToList());

                if (window.ShowDialog() == true)
                {
                    pRule.Values = window.Parameters;
                    if (pRule.Errors != "")
                    {
                        MessageBox.Show(pRule.Errors, "Errors", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    int index = DataViewModel.ActiveRule.IndexOf(rule);
                    DataViewModel.ActiveRule.Remove(rule);
                    DataViewModel.ActiveRule.Insert(index, pRule);
                }
            }
        }

        private async void newProjectButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataViewModel.ActiveRule.Count >= 0 || DataViewModel.Items.Count >= 0)
            {
                var result = MessageBox.Show("Do you want to save current project", "Batch Rename", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    if (DataViewModel.ProjectPath == "")
                    {
                        var dialog = new SaveFileDialog();
                        dialog.Filter = "Project (*.proj)|*.proj";
                        if (dialog.ShowDialog() == false)
                        {
                            return;
                        }
                        DataViewModel.ProjectPath = dialog.FileName;
                    }

                    var path = DataViewModel.ProjectPath;

                    await SaveProject(path);
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    return;
                }
            }
            DataViewModel = new DataViewModel();
        }

        private void toLastItemsButton_Click(object sender, RoutedEventArgs e)
        {
            if (itemsListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("You must choose rule before starting moving!!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int count = itemsListView.SelectedItems.Count;

            for (int i = count - 1; i >= 0; i--)
            {
                int index = DataViewModel.Items.IndexOf((Item)itemsListView.SelectedItems[i]!);
                var item = DataViewModel.Items[index];
                DataViewModel.Items.RemoveAt(index);
                DataViewModel.Items.Add(item);
            }
        }

        private void downItemsButton_Click(object sender, RoutedEventArgs e)
        {
            if (itemsListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("You must choose rule before starting moving!!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            List<int> selectedItemIndexes = new List<int>();

            for (int i = 0; i < itemsListView.SelectedItems.Count; i++)
            {
                int index = DataViewModel.Items.IndexOf((Item)itemsListView.SelectedItems[i]!);
                selectedItemIndexes.Add(index);
            }

            itemsListView.SelectedItems.Clear();
            selectedItemIndexes = selectedItemIndexes.OrderByDescending(o => o).ToList();
            List<int> finalPosition = new List<int>();

            foreach (var index in selectedItemIndexes)
            {
                if (index == DataViewModel.Items.Count - 1)
                {
                    finalPosition.Add(index);
                    continue;
                }

                int toPosition = index + 1;

                if (finalPosition.Contains(toPosition))
                {
                    finalPosition.Add(index);
                    continue;
                }
                else
                {
                    var item = DataViewModel.Items[index];
                    DataViewModel.Items.RemoveAt(index);
                    DataViewModel.Items.Insert(toPosition, item);
                    finalPosition.Add(toPosition);
                }
            }
        }

        private void upITemsButton_Click(object sender, RoutedEventArgs e)
        {
            if (itemsListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("You must choose rule before starting moving!!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            List<int> selectedItemIndexes = new List<int>();

            for (int i = 0; i < itemsListView.SelectedItems.Count; i++)
            {
                int index = DataViewModel.Items.IndexOf((Item)itemsListView.SelectedItems[i]!);
                selectedItemIndexes.Add(index);
            }

            itemsListView.SelectedItems.Clear();
            selectedItemIndexes = selectedItemIndexes.OrderBy(o => o).ToList();
            List<int> finalPosition = new List<int>();

            foreach (var index in selectedItemIndexes)
            {
                if (index == 0)
                {
                    finalPosition.Add(index);
                    continue;
                }

                int toPosition = index - 1;

                if (finalPosition.Contains(toPosition))
                {
                    finalPosition.Add(index);
                    continue;
                }
                else
                {
                    var item = DataViewModel.Items[index];
                    DataViewModel.Items.RemoveAt(index);
                    DataViewModel.Items.Insert(toPosition, item);
                    finalPosition.Add(toPosition);
                }
            }
        }

        private void toFirstItemsButton_Click(object sender, RoutedEventArgs e)
        {
            if (itemsListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("You must choose rule before starting moving!!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int count = itemsListView.SelectedItems.Count;

            for (int i = count - 1; i >= 0; i--)
            {
                int index = DataViewModel.Items.IndexOf((Item)itemsListView.SelectedItems[i]!);
                var items = DataViewModel.Items[index];
                DataViewModel.Items.RemoveAt(index);
                DataViewModel.Items.Insert(0, items);
            }
        }

        private void deleteRuleContextMenu_Click(object sender, RoutedEventArgs e)
        {
            List<int> indexes = new List<int>();
            for (int i = 0; i < rulesChosenListView.SelectedItems.Count; i++)
            {
                var rule = (IRule)rulesChosenListView.SelectedItems[i]!;
                indexes.Add(DataViewModel.ActiveRule.IndexOf(rule));
            }
            indexes.Sort();
            indexes.Reverse();
            indexes.ForEach(i =>
            {
                DataViewModel.ActiveRule.RemoveAt(i);
            });
        }
        private void editRuleParamsContextMenu_Click(object sender, RoutedEventArgs e)
        {
            var rule = (IRule)rulesChosenListView.SelectedItems[0];

            if (rule == null || rule.HasParameter == false)
            {
                return;
            }

            if (rule.HasParameter)
            {
                IRuleWithParameters pRule = (IRuleWithParameters)rule.Clone();
                Point relativePoint = Mouse.GetPosition(Application.Current.MainWindow);
                var window = new OkCancelDialog(relativePoint);
                window.Owner = this;
                window.GenerateInputField(pRule.Keys.ToList());

                if (window.ShowDialog() == true)
                {
                    pRule.Values = window.Parameters;
                    if (pRule.Errors != "")
                    {
                        MessageBox.Show(pRule.Errors, "Errors", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    int index = DataViewModel.ActiveRule.IndexOf(rule);
                    DataViewModel.ActiveRule.Remove(rule);
                    DataViewModel.ActiveRule.Insert(index, pRule);
                }
            }
        }

        private void refreshRuleButton_Click(object sender, RoutedEventArgs e)
        {
            LoadDllRules();
        }

        private void addFromDirButton_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;

            if(dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                var baseDirectory = dialog.FileName; 
                var fileArray = Directory.GetFiles(baseDirectory);
                string msg = "";
                foreach(var file in fileArray)
                {
                    var name = Path.GetFileName(file);
                    var dir = Path.GetDirectoryName(file);
                    bool isExist = false;

                    foreach(var item in DataViewModel.Items)
                    {
                        if(item.Name == name && dir == item.Directory)
                        {
                            isExist = true;
                            break;
                        }
                    }

                    if (isExist)
                    {
                        msg += $"{file} is exist\n";
                        continue;
                    }

                    DataViewModel.Items.Add(new Item() { Name = name, Directory = dir });
                }
            }
        }

        private void deleteItemContextMenu_Click(object sender, RoutedEventArgs e)
        {
            List<int> indexes = new List<int>();
            for (int i = 0; i < itemsListView.SelectedItems.Count; i++)
            {
                var item = (Item)itemsListView.SelectedItems[i]!;
                indexes.Add(DataViewModel.Items.IndexOf(item));
            }
            indexes.Sort();
            indexes.Reverse();
            indexes.ForEach(i =>
            {
                DataViewModel.Items.RemoveAt(i);
            });
        }
    }
}
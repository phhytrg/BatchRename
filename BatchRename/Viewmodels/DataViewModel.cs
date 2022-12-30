
using BatchRename.Models;
using Contract;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatchRename.Viewmodels
{
    public static class Constants
    {
        public static int JSON_FILE = 0;
        public static int TEXT_FILE = 1;

        public static int RENAME_ORIGINAL = 0;
        public static int MOVE_TO_FOLDER = 1;
    }

    public class DataViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public ObservableCollection<Item> Items { get; set; } = new ObservableCollection<Item>();
        public ObservableCollection<string> AvailableRules { get; set; } = new ObservableCollection<string>();
        [JsonProperty(ItemConverterType = typeof(IRuleConverter))]
        public ObservableCollection<IRule> ActiveRule { get; set; } = new ObservableCollection<IRule>();
        public string PresetPath { get; set; } = "";
        public string ProjectPath { get; set; } = "";
        public string Status { get; set; } = "";
        public int PresetSaveType { get; set; }
        public int BatchType { get; set; }
    }
}


using BatchRename.Models;
using Contract;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatchRename.Viewmodels
{
    public class DataViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public ObservableCollection<Item> Items { get; set; } = new ObservableCollection<Item>();
        public ObservableCollection<string> AvailableRules { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<IRule> ActiveRule { get; set; } = new ObservableCollection<IRule>();
    }
}

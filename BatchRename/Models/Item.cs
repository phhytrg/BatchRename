using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatchRename.Models
{
    public class Item: INotifyPropertyChanged
    {
        public string? Name { get; set; }
        public string? Path { get; set; }
        public string? NewName { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}

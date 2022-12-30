using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatchRename.Viewmodels
{

    public class SettingsViewModel: INotifyPropertyChanged
    {
        public List<string> ItemsType { get; set; } = new List<string> { "File", "Folder" };

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}

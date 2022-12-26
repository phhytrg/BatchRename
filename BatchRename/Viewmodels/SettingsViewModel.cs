using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatchRename.Viewmodels
{
    public class SettingsViewModel
    {
        public List<string> ItemsType { get; set; } = new List<string> { "File", "Folder" };
    }
}

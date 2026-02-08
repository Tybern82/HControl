using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HControl.Game {

    public class ImageFolderListItem {

        public bool IsEnabled { get; set; } = true;
        public string Folder { get; }
        public bool AddSubfolders { get; set; } = false;

        public ImageFolderListItem(string folder, bool addSubfolders = false) {
            this.Folder = folder;
            this.AddSubfolders = addSubfolders;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace Binner {
    public class ImageLocation : INotifyPropertyChanged {
        private string _name;
        private string _path;
        public string Name {
            get => _name;
            set {
                if (_name != value) {
                    _name = value;
                    NotifyPropertyChanged(nameof(Name));
                }
            }
        }
        public string Path {
            get => _path;
            set {
                if (_path != value) {
                    AutoName(_path, value);
                    _path = value;
                    NotifyPropertyChanged(nameof(Path));
                }
            }
        }

        public bool IsSource { get; set; }

        public bool Empty() {
            return !(Path.Any() || Name.Any());
        }

        public List<string> Validate() {
            var errorList = new List<string>();

            if (Empty())
                errorList.Add("Istnieje lokalizacja bez nazwy ani ścieżki.");
            else if (Path.Length == 0)                
                errorList.Add($"Lokalizacji o nazwie {Name} brak ścieżki.");
            else if (Name.Length == 0)
                errorList.Add($"Lokalizacji ze ścieżką {Path} brak nazwy.");

            if (Path.Length != 0 && !Directory.Exists(Path))
                errorList.Add($"Katalog o ścieżce \"{Path}\" dla lokalizacji {Name} nie istnieje.");
           
            return errorList;
        }

        private void AutoName(string currPath, string newPath) {
            if (Name.Length == 0 || Name == DirName(currPath))
                Name = DirName(newPath);

            string DirName(string path) => string.IsNullOrWhiteSpace(path) ? "" : new DirectoryInfo(path).Name;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class ImageLocationList : ObservableCollection<ImageLocation> {
        public void SetEmptyRows(int amount, bool removeUncompliant = true) {
            var toAdd = amount;
            for (var i = Count - 1; i >= 0 && this[i].Empty(); i--)
                toAdd--;
            while (toAdd-- > 0)
                Add(new ImageLocation() { Name = "", Path = "", IsSource = false });

            if (removeUncompliant)
                this.Take(Count - amount).Where(loc => loc.Empty()).ToList().ForEach(loc => Remove(loc));
        }

        public List<string> Validate() {
            var errorList = new List<string>();

            if (!this.Any(dir => dir.IsSource && !dir.Empty()))
                errorList.Add("Nie został wybrany żaden źródłowy katalog.");
            
            if (this.Count() < 2)
                errorList.Add("Muszą zostać wybrane co najmniej dwa katalogi.");

            errorList.AddRange(this.SelectMany(dir => dir.Validate()));

            var dupeNames = this.Where(dir1 => this.Any(dir2 => IndexOf(dir1) < IndexOf(dir2) && dir1.Name == dir2.Name)).Select(dir => dir.Name).ToList();
            if (dupeNames.Any())
                errorList.Add($"Wśród wpisów katalogów znajdują się wpisy o identycznych nazwach: {string.Join(", ", dupeNames)}.");

            var dupeSources = (from dir1 in this
                               from dir2 in this
                               where IndexOf(dir1) < IndexOf(dir2)
                                  && Directory.Exists(dir1.Path) && Directory.Exists(dir2.Path)
                                  && dir1.IsSource && dir2.IsSource 
                                  && PathsContainCommonSetOfFiles(dir1.Path, dir2.Path)
                               select $"{(dir1.Name.Length != 0 ? dir1.Name : dir1.Path)} & {(dir2.Name.Length != 0 ? dir2.Name : dir2.Path)}").ToList();
            if (dupeSources.Any())
                errorList.Add($"Poniższe lokalizacje prowadzą do [częściowo] tych samych plików:\n{string.Join("\n", dupeSources)}");

            return errorList;
        }

        private static bool PathsContainCommonSetOfFiles(string dir1, string dir2)
        {
            return SamePath(dir1, dir2) || IsPathParent(dir1, dir2) || IsPathParent(dir2, dir1);

            bool IsPathParent(string parent, string child) => IsPathParentImpl(parent, new DirectoryInfo(child));
            bool IsPathParentImpl(string parent, DirectoryInfo child) => child.Parent != null && (SamePath(parent, child.Parent.FullName) || IsPathParentImpl(parent, child.Parent));
            bool SamePath(string path1, string path2) => string.Compare(NormalizedPath(path1), NormalizedPath(path2), StringComparison.InvariantCultureIgnoreCase) == 0;
            string NormalizedPath(string path) => Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
    }
}

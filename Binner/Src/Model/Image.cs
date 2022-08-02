using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Binner
{
    public class Image
    {
        public string Path { get; set; }
        public BitmapImage Bitmap { get; set; }

        public void MoveTo(string destinationPath) {
            try {
                var selfFilename = System.IO.Path.GetFileName(Path) ?? throw new Exception($"Przenoszony obraz nie ma ustawionej ścieżki.");
                destinationPath = System.IO.Path.Combine(destinationPath, selfFilename);
                File.Move(Path, destinationPath);
            } catch (Exception Ex) {
                var exceptionMessage = $"Wystąpił błąd podczas kopiowania o ścieżce '{Path}' do '{destinationPath}'.";

                try {
                    LoadBitmap();
                } catch (Exception ReloadEx) {
                    Bitmap = null;
                    exceptionMessage += $" Podczas próby przeładowania obrazu także wystąpił błąd, \"{ComposeExceptionMsg(ReloadEx)}\"."; 
                }

                throw new Exception(exceptionMessage, Ex);
            }

            Path = destinationPath;

            string ComposeExceptionMsg(Exception E) => E.Message + " " + (E.InnerException == null ? ComposeExceptionMsg(E.InnerException) : "");
        }

        public void LoadBitmap() {
            Bitmap = new BitmapImage();
            try {
                Bitmap.BeginInit();
                Bitmap.CacheOption = BitmapCacheOption.OnLoad;
                Bitmap.UriSource = new Uri(Path);
                Bitmap.EndInit();
            } catch (Exception Ex) {
                Bitmap = null;
                throw new Exception($"Wystąpił błąd podczas przeładowywania obrazu o ścieżce '{Path}'.", Ex);
            }          
        }
    }

    public class ImageList : List<Image>, INotifyPropertyChanged {
        private int _currentIndex = 0;
        public Image Current => this.Any() ? this[_currentIndex] : null;
        public int CurrentPosition => _currentIndex + 1;

        public void Previous() {
            Current.Bitmap = null;
            _currentIndex = _currentIndex == 0 ? Count - 1 : _currentIndex - 1;
            LoadCurrent();
        }

        public void Next() {
            Current.Bitmap = null;
            _currentIndex = IsLast() ? 0 : _currentIndex + 1;
            LoadCurrent();
        }

        public bool IsLast() {
            return _currentIndex + 1 == Count;
        }

        public void Load(string srcPath) {
            var emptyBefore = !this.Any();
            try {
                AddRange(Directory.EnumerateFiles(srcPath, "*.*", SearchOption.AllDirectories)
                    .Where(s => new[] { ".png", ".jpg", ".jpeg", ".bmp" }.Any(s.EndsWith))
                    .Select(p => new Image() { Path = p }));
            } catch (Exception Ex) {
                throw new Exception($"Wystąpił błąd podczas ładowania obrazów ze ścieżki '{srcPath}'.", Ex);
            }

            NotifyPropertyChanged(nameof(Count));
            if (emptyBefore && this.Any())
                LoadCurrent();
        }

        public new void Clear() {
            _currentIndex = 0;
            base.Clear();
            NotifyPropertyChanged(nameof(Count));
            NotifyPropertyChanged(nameof(Current));
            NotifyPropertyChanged(nameof(CurrentPosition));
        }

        private void LoadCurrent() {
            Current.LoadBitmap();
            NotifyPropertyChanged(nameof(Current));
            NotifyPropertyChanged(nameof(CurrentPosition));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Binner {
    public partial class MainWindow : Window {
        public ImageLocationList ImageLocations { get; set; }
        public ImageList Images { get; set; }

        public MainWindow() {
            InitializeComponent();
            DataContext = this;
            InitializeModel();
        }

        private void InitializeModel() {
            Images = new ImageList();
            ImageLocations = new ImageLocationList();
            ImageLocations.SetEmptyRows(1);
        }

        private void MainWindow_KeyUp(object sender, KeyEventArgs e) {
            if (WindowTabs.SelectedIndex == TabConfig)
                return;

            try {
                if (e.Key == Key.Left)
                    Images.Previous();
                else if (e.Key == Key.Right)
                    GoToNextImage();
            } catch (Exception Ex) {
                ShowException("Wystąpił błąd podczas zmiany obrazu.", Ex);
            }
        }

        private void ImageLocation_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) {
            SetEmptyLocationRows(sender as TextBox, false);
        }

        private void ImageLocation_LostFocus(object sender, RoutedEventArgs e) {
            SetEmptyLocationRows(sender as TextBox, true);
        }

        private void RemoveLocationButton_Click(object sender, RoutedEventArgs e) {
            var location = GetSenderDataContext<ImageLocation>(sender);
            if (ImageLocations.IndexOf(location) != ImageLocations.Count - 1)
                ImageLocations.Remove(location);
        }

        private void ChoosePathButton_Click(object sender, RoutedEventArgs e) {
            try {
                var location = GetSenderDataContext<ImageLocation>(sender);

                if (CommonFileDialog.IsPlatformSupported) {
                    var dialog = new CommonOpenFileDialog() { IsFolderPicker = true, InitialDirectory = location.Path };
                    if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                        location.Path = dialog.FileName;
                } else {
                    var dialog = new System.Windows.Forms.FolderBrowserDialog() { SelectedPath = location.Path };
                    dialog.ShowDialog();
                    location.Path = dialog.SelectedPath;
                }
            } catch (Exception Ex) {
                ShowException("Wystąpił błąd podczas wybierania ścieżki.", Ex);
            }
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e) {
            var (success, failMessage) = SwitchToSortingTab();
            if (!success) {
                ImageLocations.SetEmptyRows(1);
                MessageBox.Show(failMessage, "Wystąpiły błędy podczas ładowania", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ConfigButton_Click(object sender, RoutedEventArgs e) {
            var dialogRes = MessageBox.Show("Powrót spowoduje opróżnienie listy obrazów. Kontynuować?", "Powrót", 
                                            MessageBoxButton.OKCancel, MessageBoxImage.Exclamation, MessageBoxResult.Cancel);
            if (dialogRes != MessageBoxResult.OK)
                return;
            SwitchToConfigTab();
        }

        private void TargetButton_Click(object sender, RoutedEventArgs e) {
            Images.Current.MoveTo(GetSenderDataContext<ImageLocation>(e.OriginalSource).Path);
            GoToNextImage();
        }

        private void GoToNextImage() {
            if (Images.IsLast())
                MessageBox.Show("Koniec obrazów do przetworzenia, powrót na początek listy.", "Koniec", MessageBoxButton.OK, MessageBoxImage.Information);

            try {
                Images.Next();
            }
            catch (Exception Ex) {
                ShowException("Wystąpił błąd podczas przechodzenia do kolejnego obrazu.", Ex);
            }
        }

        private (bool success, string failMessage) SwitchToSortingTab() {
            try {
                var failMsg = PrepareDataForLoading();
                if (failMsg.Any())
                    return (false, failMsg);

                failMsg = LoadImages();
                if (failMsg.Any())
                    return (false, failMsg);
            } catch (Exception Ex) {
                return (false, "Wyjątek: " + ComposeExceptionMsg(Ex));
            }

            WindowTabs.SelectedIndex = TabSorting;
            return (true, "");

            string PrepareDataForLoading() {
                ImageLocations.SetEmptyRows(0);
                var validationErrors = ImageLocations.Validate();
                return validationErrors.Any() ? $"Błędy w danych wejściowych:\n\n{string.Join("\n\n", validationErrors)}" : "";
            }

            string LoadImages() {
                ImageLocations.Where(loc => loc.IsSource).ToList().ForEach(loc => Images.Load(loc.Path));
                return Images.Any() ? "" : "Nie znaleziono żadnego obrazu wśród wybranych katalogach źródłowych.";
            }
        }

        private void SwitchToConfigTab() {
            Images.Clear();
            ImageLocations.SetEmptyRows(1);
            WindowTabs.SelectedIndex = TabConfig;
        }

        private void SetEmptyLocationRows(TextBox activeControl, bool removeUncompliant) {
            activeControl?.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
            ImageLocations.SetEmptyRows(1, removeUncompliant);
        }

        private void ShowException(string Preamble, Exception Ex) {            
            MessageBox.Show(Preamble + " " + ComposeExceptionMsg(Ex), "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        private static string ComposeExceptionMsg(Exception E) => E.Message + " " + (E.InnerException == null ? ComposeExceptionMsg(E.InnerException) : "");

        private const int TabConfig = 0;
        private const int TabSorting = 1;
        private static TDataContext GetSenderDataContext<TDataContext>(object control) {
            if (!(control is FrameworkElement frameworkElement))
                throw new ArgumentException($"Otrzymano argument typu {control.GetType()}, a oczekiwano FrameworkElement lub pochodnego.", paramName: nameof(control));
            if (!(frameworkElement.DataContext is TDataContext dataCtx))
                throw new ArgumentException($"DataContext argumentu jest typu {frameworkElement.DataContext.GetType()}, a oczekiwano {typeof(TDataContext).Name}.", paramName: nameof(control));
            return dataCtx;
        }
    }
}

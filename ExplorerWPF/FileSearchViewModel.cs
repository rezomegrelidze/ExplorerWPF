using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ExplorerWPF.Annotations;

namespace ExplorerWPF
{
    public class FileSearchViewModel : INotifyPropertyChanged
    {
        private string _selectedFolder;
        private string _searchText;
        private ObservableCollection<string> _searchResults;
        private double _progressBarValue;
        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public FileSearchViewModel()
        {
            SearchResults = new ObservableCollection<string>();
        }

        public string SelectedFolder
        {
            get => _selectedFolder;
            set
            {
                _selectedFolder = value;
                OnPropertyChanged();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> SearchResults
        {
            get => _searchResults;
            set
            {
                _searchResults = value;
                OnPropertyChanged();
            }
        }

        public double ProgressBarValue
        {
            get => _progressBarValue;
            set
            {
                _progressBarValue = value;
                OnPropertyChanged();
            }
        }

        public ICommand Search => new RelayCommand(async () =>
        {
            ProgressBarValue = 0;
            try
            {
                var searchResults = Task.Run(() => GetSearchResults(SearchText, SelectedFolder));
                foreach (var result in await searchResults)
                {
                    SearchResults.Add(result);
                    ProgressBarValue += 0.1;
                }

                ProgressBarValue = 100;
                
            }
            catch (ArgumentNullException ex)
            {
                MessageBox.Show("Please fill all the fields to start search");
            }
            catch(Exception ex)
            {
            }
        });

        private IEnumerable<string> GetSearchResults([NotNull] string searchText, [NotNull] string selectedFolder)
        {
            if (searchText == null) throw new ArgumentNullException(nameof(searchText));
            if (selectedFolder == null) throw new ArgumentNullException(nameof(selectedFolder));

            foreach (var file in Directory.GetFiles(selectedFolder)
                .Where(file => file.Contains(searchText)))
                yield return file;

            var subfolders = Directory.GetDirectories(selectedFolder);

            foreach (var folder in subfolders)
            foreach (var file in GetSearchResults(searchText, folder))
                yield return file;
        }

        public ICommand SelectFolderDialog => new RelayCommand(() =>
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.SelectedPath = SelectedFolder;
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                SelectedFolder = dialog.SelectedPath;
            }
        });

    }
}
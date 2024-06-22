using Prism.Commands;
using Prism.Mvvm;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using WinTool.Model;
using WinTool.View;

namespace WinTool.ViewModel
{
    public class SearchTextViewModel : BindableBase
    {
        private string? _folderPath;
        private string? _searchedText;
        private string? _filesFilter;
        private bool _matchCase;
        private bool _isRecursive;
        private bool _useRegex;
        private Window? _window;
        private SearchTextResultView? _resultView;

        public string? FolderPath
        {
            get => _folderPath;
            set => SetProperty(ref _folderPath, value);
        }

        public string? SearchedText
        {
            get => _searchedText;
            set => SetProperty(ref _searchedText, value);
        }

        public string? FilesFilter
        {
            get => _filesFilter;
            set => SetProperty(ref _filesFilter, value);
        }

        public bool MatchCase
        {
            get => _matchCase;
            set => SetProperty(ref _matchCase, value);
        }

        public bool IsRecursive
        {
            get => _isRecursive;
            set => SetProperty(ref _isRecursive, value);
        }

        public bool UseRegex
        {
            get => _useRegex;
            set => SetProperty(ref _useRegex, value);
        }

        public DelegateCommand SearchCommand { get; }
        public DelegateCommand<Window> WindowLoadedCommand { get; }
        public DelegateCommand CloseWindowCommand { get; }

        public SearchTextViewModel(string folderPath)
        {
            FolderPath = folderPath;

            SearchCommand = new DelegateCommand(ShowSearchResult);
            WindowLoadedCommand = new DelegateCommand<Window>(w => _window = w);
            CloseWindowCommand = new DelegateCommand(() => _window?.Close());
        }

        public void ShowSearchResult()
        {
            _window!.Topmost = false;
            var results = Search();
            SearchTextResultViewModel vm = new(results, SearchedText ?? string.Empty);

            _resultView?.Close();
            _resultView = new(vm);
            _resultView.Show();
            _resultView.Activate();
        }

        public List<TextOccurrence> Search()
        {
            if (SearchedText is null or [])
                return [];

            RegexOptions regexOptions = MatchCase ? RegexOptions.None : RegexOptions.IgnoreCase;
            SearchOption searchOption = IsRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            string regexSearchedText = UseRegex ? SearchedText : Regex.Escape(SearchedText);

            List<TextOccurrence> textOccurrences = [];
            IEnumerable<string> allFiles = Directory.GetFiles(FolderPath!, "*", searchOption);

            if (FilesFilter is not (null or []))
                allFiles = allFiles.Where(f => Regex.IsMatch(f, FilesFilter));

            foreach (string file in allFiles)
            {
                var lines = File.ReadAllLines(file);

                for (int i = 0; i < lines.Length; i++)
                {
                    foreach (Match match in Regex.Matches(lines[i], regexSearchedText, regexOptions))
                    {
                        textOccurrences.Add(new TextOccurrence(file, i, match.Index, match.Value));
                    }
                }
            }

            return textOccurrences;
        }
    }
}

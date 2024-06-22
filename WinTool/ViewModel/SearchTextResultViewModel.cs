using Prism.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using WinTool.Model;

namespace WinTool.ViewModel
{
    public class SearchTextResultViewModel : BindableBase
    {
        private string? _title;
        private ObservableCollection<TextOccurrenceViewModel> _occurrences = [];

        public string? Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public ObservableCollection<TextOccurrenceViewModel> Occurrences
        {
            get => _occurrences;
            set => SetProperty(ref _occurrences, value);
        }

        public SearchTextResultViewModel(List<TextOccurrence> occurrences, string searchedText)
        {
            Title = string.Format(Resources.Localizations.Resources.SearchResult, searchedText, occurrences.Count);

            foreach (var occurrence in occurrences)
            {
                Occurrences.Add(new TextOccurrenceViewModel(occurrence));
            }
        }
    }
}

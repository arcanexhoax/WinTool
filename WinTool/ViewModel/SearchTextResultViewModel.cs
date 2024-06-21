using Prism.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using WinTool.Model;

namespace WinTool.ViewModel
{
    public class SearchTextResultViewModel : BindableBase
    {
        private ObservableCollection<TextOccurrenceViewModel> _occurrences = [];

        public ObservableCollection<TextOccurrenceViewModel> Occurrences
        {
            get => _occurrences;
            set => SetProperty(ref _occurrences, value);
        }

        public SearchTextResultViewModel(List<TextOccurrence> occurrences)
        {
            foreach (var occurrence in occurrences)
            {
                Occurrences.Add(new TextOccurrenceViewModel(occurrence));
            }
        }
    }
}

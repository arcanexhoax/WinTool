using Prism.Mvvm;
using WinTool.Model;

namespace WinTool.ViewModel
{
    public class TextOccurrenceViewModel : BindableBase
    {
        private string? _filePath;
        private string? _location;
        private string? _value;

        public string? FilePath
        {
            get => _filePath;
            set => SetProperty(ref _filePath, value);
        }

        public string? Location
        {
            get => _location;
            set => SetProperty(ref _location, value);
        }

        public string? Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        public TextOccurrenceViewModel(TextOccurrence occurrence)
        {
            FilePath = occurrence.FilePath;
            Location = $"({occurrence.Line}, {occurrence.Letter})";
            Value = occurrence.Text;
        }
    }
}

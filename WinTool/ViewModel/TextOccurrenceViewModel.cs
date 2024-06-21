using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinTool.Model;
using WinTool.Modules;

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

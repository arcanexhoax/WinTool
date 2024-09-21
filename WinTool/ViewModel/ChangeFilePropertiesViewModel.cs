using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using WinTool.Utils;
using Resource = WinTool.Resources.Localizations.Resources;

namespace WinTool.ViewModel
{
    public class ChangeFilePropertiesViewModel : BindableBase
    {
        private string? _fileName;
        private string? _title;
        private string? _performers;
        private string? _album;
        private string? _genres;
        private string? _lyrics;
        private uint _year;
        private bool _mediaTagsSupported;
        private DateTime _creationTime;
        private DateTime _changeTime;
        private Window? _window;

        public string? FileName
        {
            get => _fileName;
            set => SetProperty(ref _fileName, value);
        }

        public string? Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string? Performers
        {
            get => _performers;
            set => SetProperty(ref _performers, value);
        }

        public string? Album
        {
            get => _album;
            set => SetProperty(ref _album, value);
        }

        public string? Genres
        {
            get => _genres;
            set => SetProperty(ref _genres, value);
        }

        public string? Lyrics
        {
            get => _lyrics;
            set => SetProperty(ref _lyrics, value);
        }

        public uint Year
        {
            get => _year;
            set => SetProperty(ref _year, value);
        }

        public bool MediaTagsSupported
        {
            get => _mediaTagsSupported;
            set => SetProperty(ref _mediaTagsSupported, value);
        }

        public DateTime CreationTime
        {
            get => _creationTime;
            set => SetProperty(ref _creationTime, value);
        }

        public DateTime ChangeTime
        {
            get => _changeTime;
            set => SetProperty(ref _changeTime, value);
        }

        public DelegateCommand<Window> WindowLoadedCommand { get; }
        public DelegateCommand SaveCommand { get; }
        public DelegateCommand CloseWindowCommand { get; }

        public ChangeFilePropertiesViewModel(string filePath, TagLib.File? tfile = null)
        {
            FileName = Path.GetFileName(filePath);
            CreationTime = File.GetCreationTime(filePath);
            ChangeTime = File.GetLastWriteTime(filePath);

            if (tfile != null)
            {
                MediaTagsSupported = true;
                Title = tfile.Tag.Title;
                Performers = string.Join(", ", tfile.Tag.Performers);
                Album = tfile.Tag.Album;
                Genres = string.Join(", ", tfile.Tag.Genres);
                Lyrics = tfile.Tag.Lyrics;
                Year = tfile.Tag.Year;
            }

            SaveCommand = new DelegateCommand(() =>
            {
                try
                {
                    if (MediaTagsSupported)
                    {
                        tfile!.Tag.Title = Title;
                        tfile.Tag.Performers = [Performers];
                        tfile.Tag.Album = Album;
                        tfile.Tag.Genres = [Genres];
                        tfile.Tag.Lyrics = Lyrics;
                        tfile.Tag.Year = Year;
                        tfile.Save();
                        tfile.Dispose();
                    }

                    File.SetCreationTime(filePath, CreationTime);
                    File.SetLastWriteTime(filePath, ChangeTime);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Save file properties error: " + ex.Message);
                    MessageBoxHelper.ShowError(string.Format(Resource.SaveFilePropertiesError, filePath, ex.Message));
                }

                _window?.Close();
            });
            WindowLoadedCommand = new DelegateCommand<Window>(w => _window = w);
            CloseWindowCommand = new DelegateCommand(() => _window?.Close());
        }
    }
}

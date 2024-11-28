using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using WinTool.Utils;
using Resource = WinTool.Resources.Localizations.Resources;

namespace WinTool.ViewModel
{
    public class ChangeFilePropertiesViewModel : ObservableObject
    {
        private Window? _window;

        public string? FileName
        {
            get; set => SetProperty(ref field, value);
        }

        public string? Title
        {
            get; set => SetProperty(ref field, value);
        }

        public string? Performers
        {
            get; set => SetProperty(ref field, value);
        }

        public string? Album
        {
            get; set => SetProperty(ref field, value);
        }

        public string? Genres
        {
            get; set => SetProperty(ref field, value);
        }

        public string? Lyrics
        {
            get; set => SetProperty(ref field, value);
        }

        public uint Year
        {
            get; set => SetProperty(ref field, value);
        }

        public bool MediaTagsSupported
        {
            get; set => SetProperty(ref field, value);
        }

        public DateTime CreationTime
        {
            get; set => SetProperty(ref field, value);
        }

        public DateTime ChangeTime
        {
            get; set => SetProperty(ref field, value);
        }

        public RelayCommand<Window> WindowLoadedCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand CloseWindowCommand { get; }

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

            SaveCommand = new RelayCommand(() =>
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
            WindowLoadedCommand = new RelayCommand<Window>(w => _window = w);
            CloseWindowCommand = new RelayCommand(() => _window?.Close());
        }
    }
}

﻿using Prism.Commands;
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
        private string? _musicTitle;
        private string? _musicPerformers;
        private bool _mediaTagsSupported;
        private DateTime _creationTime;
        private DateTime _changeTime;
        private Window? _window;

        public string? FileName
        {
            get => _fileName;
            set => SetProperty(ref _fileName, value);
        }

        public string? MusicTitle
        {
            get => _musicTitle;
            set => SetProperty(ref _musicTitle, value);
        }

        public string? MusicPerformers
        {
            get => _musicPerformers;
            set => SetProperty(ref _musicPerformers, value);
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
                MusicTitle = tfile.Tag.Title;
                MusicPerformers = string.Join(", ", tfile.Tag.Performers);
            }

            SaveCommand = new DelegateCommand(() =>
            {
                try
                {
                    if (MediaTagsSupported)
                    {
                        tfile!.Tag.Title = MusicTitle;
                        tfile.Tag.Performers = [MusicPerformers];
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

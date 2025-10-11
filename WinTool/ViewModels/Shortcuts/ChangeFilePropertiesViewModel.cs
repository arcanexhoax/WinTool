using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.IO;
using WinTool.Models;
using WinTool.ViewModel;

namespace WinTool.ViewModels.Shortcuts;

public class ChangeFilePropertiesViewModel : ObservableObject, IDialogViewModel<ChangeFilePropertiesInput, ChangeFilePropertiesOutput>
{
    private Action<Result<ChangeFilePropertiesOutput>>? _onResult;

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

    public RelayCommand SaveCommand { get; }
    public RelayCommand CloseWindowCommand { get; }

    public ChangeFilePropertiesViewModel()
    {
        SaveCommand = new RelayCommand(() =>
        {
            _onResult?.Invoke(new Result<ChangeFilePropertiesOutput>(true, new ChangeFilePropertiesOutput( 
                Title,
                Performers?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
                Album,
                Genres?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
                Lyrics,
                Year,
                CreationTime,
                ChangeTime)));
        });
        CloseWindowCommand = new RelayCommand(() => _onResult?.Invoke(new Result<ChangeFilePropertiesOutput>(false)));
    }

    public void OnShow(ChangeFilePropertiesInput input, Action<Result<ChangeFilePropertiesOutput>> onResult)
    {
        FileName = Path.GetFileName(input.FilePath);
        CreationTime = File.GetCreationTime(input.FilePath);
        ChangeTime = File.GetLastWriteTime(input.FilePath);

        if (input.MediaTagsSupported)
        {
            MediaTagsSupported = true;
            Title = input.Title;
            Performers = string.Join(", ", input.Performers ?? []);
            Album = input.Album;
            Genres = string.Join(", ", input.Genres ?? []);
            Lyrics = input.Lyrics;
            Year = input.Year;
        }

        _onResult = onResult;
    }

    public void OnClose() => _onResult = null;
}

﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using WinTool.Options;
using WinTool.View;

namespace WinTool.ViewModel;

public class ShortcutViewModel : ObservableObject
{
    protected readonly WritableOptions<ShortcutsOptions> _shortcutsOptions;
    protected readonly EditShortcutViewModel _editShortcutViewModel;

    public string Shortcut
    {
        get; set => SetProperty(ref field, value);
    }

    public string Description { get; }

    public RelayCommand EditShortcutCommand { get; }

    public ShortcutViewModel(
        Func<ShortcutOptions> optionsFactory,
        WritableOptions<ShortcutsOptions> shortcutsOptions,
        EditShortcutViewModel editShortcutViewModel,
        string description)
    {
        _shortcutsOptions = shortcutsOptions;
        _editShortcutViewModel = editShortcutViewModel;

        Shortcut = optionsFactory().Shortcut;
        Description = description;

        // TODO move fast file creation shortcut view into shortcut view
        EditShortcutCommand = new RelayCommand(() =>
        {
            var window = new EditShortcutWindow(_editShortcutViewModel);
            window.ShowDialog(Shortcut);
        });
    }
}

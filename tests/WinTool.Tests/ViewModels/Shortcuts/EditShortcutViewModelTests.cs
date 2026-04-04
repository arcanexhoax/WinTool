using GlobalKeyInterceptor;
using Microsoft.Extensions.Options;
using WinTool.Models;
using WinTool.Options;
using WinTool.Properties;
using WinTool.ViewModels.Shortcuts;

namespace WinTool.Tests.ViewModels.Shortcuts;

public class EditShortcutViewModelTests
{
    [Fact]
    public void OnShow_SetsCurrentShortcutAndEditingFlag()
    {
        var shortcutContext = new ShortcutContext();
        var viewModel = CreateViewModel(shortcutContext: shortcutContext);
        var shortcut = ParseShortcut("Ctrl + E");

        viewModel.OnShow(new EditShortcutInput(shortcut, ShortcutNames.CreateFile), _ => { });

        Assert.True(shortcutContext.IsEditing);
        Assert.Equal(shortcut, viewModel.Shortcut);
    }

    [Fact]
    public void OnClose_ClearsEditingFlag()
    {
        var shortcutContext = new ShortcutContext();
        var viewModel = CreateViewModel(shortcutContext: shortcutContext);

        viewModel.OnShow(new EditShortcutInput(ParseShortcut("Ctrl + E"), ShortcutNames.CreateFile), _ => { });
        viewModel.OnClose();

        Assert.False(shortcutContext.IsEditing);
    }

    [Fact]
    public void SaveCommand_WithoutModifier_CannotExecute()
    {
        var viewModel = CreateViewModel();

        viewModel.OnShow(new EditShortcutInput(null, ShortcutNames.CreateFile), _ => { });
        viewModel.Shortcut = ParseShortcut("A");

        Assert.False(viewModel.SaveCommand.CanExecute(null));
        Assert.False(viewModel.IsErrorShown);
    }

    [Fact]
    public void SaveCommand_DuplicateShortcut_ShowsErrorAndCannotExecute()
    {
        var options = new ShortcutsOptions
        {
            Shortcuts = new Dictionary<string, string?>
            {
                [ShortcutNames.CreateFile] = "Ctrl + E",
                [ShortcutNames.FastFileCreation] = "Ctrl + Shift + E"
            }
        };
        var viewModel = CreateViewModel(options);

        viewModel.OnShow(new EditShortcutInput(ParseShortcut("Ctrl + P"), ShortcutNames.RunFileWithArgs), _ => { });
        viewModel.Shortcut = ParseShortcut("Ctrl + E");

        Assert.False(viewModel.SaveCommand.CanExecute(null));
        Assert.True(viewModel.IsErrorShown);
    }

    [Fact]
    public void SaveCommand_UniqueShortcut_ReturnsShortcut()
    {
        Result<Shortcut>? result = null;
        var viewModel = CreateViewModel();
        var newShortcut = ParseShortcut("Ctrl + P");

        viewModel.OnShow(new EditShortcutInput(ParseShortcut("Ctrl + E"), ShortcutNames.CreateFile), output => result = output);
        viewModel.Shortcut = newShortcut;

        Assert.True(viewModel.SaveCommand.CanExecute(null));

        viewModel.SaveCommand.Execute(null);

        Assert.NotNull(result);
        Assert.True(result!.Success);
        Assert.Equal(newShortcut, result.Data);
    }

    private static EditShortcutViewModel CreateViewModel(ShortcutsOptions? options = null, ShortcutContext? shortcutContext = null)
    {
        return new EditShortcutViewModel(
            new StaticOptionsMonitor<ShortcutsOptions>(options ?? new ShortcutsOptions()),
            shortcutContext ?? new ShortcutContext());
    }

    private static Shortcut ParseShortcut(string text, KeyState state = KeyState.Down)
    {
        return Shortcut.Parse(text, state);
    }

    private sealed class StaticOptionsMonitor<T>(T currentValue) : IOptionsMonitor<T>
    {
        public T CurrentValue => currentValue;

        public T Get(string? name)
        {
            return currentValue;
        }

        public IDisposable OnChange(Action<T, string?> listener)
        {
            return EmptyDisposable.Instance;
        }
    }

    private sealed class EmptyDisposable : IDisposable
    {
        public static EmptyDisposable Instance { get; } = new();

        public void Dispose()
        {
        }
    }
}
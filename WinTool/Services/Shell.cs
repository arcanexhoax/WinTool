using SHDocVw;
using Shell32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UIAutomationClient;
using WinTool.Native;

namespace WinTool.Services;

public class Shell(StaThreadService staThreadService)
{
    private const int UIA_ClassNamePropertyId = 30012;
    private const int UIA_AutomationIdPropertyId = 30011;
    private const int UIA_ValuePatternId = 10002;

    private readonly StaThreadService _staThreadService = staThreadService;

    public bool IsActive => IsWindowExplorer(NativeMethods.GetForegroundWindow());

    public bool IsWindowExplorer(nint handle)
    {
        try
        {
            var className = NativeMethods.GetClassName(handle);
            return className is "CabinetWClass" or "ExploreWClass";
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Failed to check if window is Explorer: {ex.Message}");
            return false;
        }
    }

    public string? GetActiveExplorerPath()
    {
        return _staThreadService.Invoke(GetActiveExplorerPathInternal);
    }

    public List<string> GetSelectedItemsPaths()
    {
        return _staThreadService.Invoke(GetSelectedItemsPathsInternal);
    }

    private string? GetActiveExplorerPathInternal()
    {
        IntPtr handle = NativeMethods.GetForegroundWindow();
        InternetExplorer? window = GetActiveShellWindow(handle);

        if (window is null)
            return null;

        return new Uri(window.LocationURL).LocalPath;
    }

    private List<string> GetSelectedItemsPathsInternal()
    {
        List<string> selectedItemsPaths = [];

        IntPtr handle = NativeMethods.GetForegroundWindow();
        InternetExplorer? window = GetActiveShellWindow(handle);

        if (window is null)
            return selectedItemsPaths;

        FolderItems folderItems = ((IShellFolderViewDual2)window.Document).SelectedItems();

        foreach (var folderItem in folderItems)
        {
            string selectedItemPath = ((FolderItem)folderItem).Path;

            if (!string.IsNullOrEmpty(selectedItemPath))
                selectedItemsPaths.Add(selectedItemPath);
        }

        return selectedItemsPaths;
    }

    private InternetExplorer? GetActiveShellWindow(IntPtr handle)
    {
        // check if current Windows version is 11
        if (Environment.OSVersion.Version.Build >= 22000)
            return GetActiveShellWindow11(handle);
        else
            return GetActiveShellWindow10(handle);
    }

    private InternetExplorer? GetActiveShellWindow10(IntPtr handle)
    {
        ShellWindows shellWindows = new();

        foreach (InternetExplorer window in shellWindows)
        {
            if (window.HWND == handle.ToInt32())
                return window;
        }

        return null;
    }

    private InternetExplorer? GetActiveShellWindow11(IntPtr handle)
    {
        ShellWindows shellWindows = new();
        List<InternetExplorer> tabs = [];

        // The explorer window in Windows 11 supports multiple tabs, these tabs will have the same window handle
        foreach (InternetExplorer window in shellWindows)
        {
            if (window.HWND == handle.ToInt32() && window.LocationURL is not (null or []))
                tabs.Add(window);
        }

        if (tabs.Count == 0)
            return null;
        if (tabs.Count == 1)
            return tabs[0];

        // Try to get the active tab URL from the address bar. The current limitations:
        // 1. It doesn't work with special folders like Desktop/Downloads/Pictures etc
        // 2. It doesn't work if user has edited the address bar or switching tab
        // 3. If there are multiple explorer windows with same folder opened, we may use wrong tab
        var activeTabUrl = GetActiveShellUrl11(handle);

        if (activeTabUrl is not null)
        {
            var activeTab = tabs.FirstOrDefault(t => t.LocationURL.Equals(activeTabUrl, StringComparison.InvariantCultureIgnoreCase));

            if (activeTab is not null)
                return activeTab;
        }

        // Fallback method: try to match the tab URL with window title.
        // The title has format "Folder - File Explorer" or "Folder and X more tabs - File Explorer"
        // This way has almost the same limitations as above (it works if user has edited the address bar) 
        // And it doesn't work with folders that have different display name ("Local Disk (C:)", localized "Program Files" etc)
        string? windowTitle = NativeMethods.GetWindowText(handle);

        if (windowTitle is null or [])
        {
            Trace.WriteLine("Failed to get window title of current Explorer window");
            return null;
        }

        return tabs.MaxBy(t =>
        {
            if (!Uri.TryCreate(t.LocationURL, UriKind.RelativeOrAbsolute, out var uri))
                return 0;

            var dirName = Path.GetFileName(uri.LocalPath);
            return windowTitle.StartsWith(dirName) ? dirName.Length : 0;
        });
    }

    private string? GetActiveShellUrl11(nint handle)
    {
        if (!IsWindowExplorer(handle))
            return null;

        var automation = new CUIAutomation();
        var root = automation.ElementFromHandle(handle);

        if (root == null)
            return null;

        var automationCond = automation.CreatePropertyCondition(UIA_AutomationIdPropertyId, "TextBox");
        var classCond = automation.CreatePropertyCondition(UIA_ClassNamePropertyId, "TextBox");
        var combinedCond = automation.CreateAndCondition(automationCond, classCond);
        var textBoxes = root.FindAll(TreeScope.TreeScope_Descendants, combinedCond);

        // We receive two text boxes: address bar and search box. We want the address bar, which is the first one.
        var t1 = textBoxes?.GetElement(0).GetCurrentPattern(UIA_ValuePatternId) as IUIAutomationValuePattern;
        var url = t1?.CurrentValue;

        // URL of address bar could be invalid if user has typed in it, if it's a special folder or after switching tab
        if (url is null or [] || !Path.IsPathFullyQualified(url))
            return null;

        return url;
    }
}

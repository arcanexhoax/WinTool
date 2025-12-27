using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

namespace WinTool.Native.Shell;

[ComImport]
[Guid("B63EA76D-1F85-456F-A19C-48159EFA858B")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IShellItemArray
{
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void BindToHandler([In, MarshalAs(UnmanagedType.Interface)] nint pbc, [In] ref Guid rbhid, [In] ref Guid riid, out nint ppvOut);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void GetPropertyStore([In] int flags, [In] ref Guid riid, out nint ppv);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void GetPropertyDescriptionList([In] ref PropertyKey keyType, [In] ref Guid riid, out nint ppv);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void GetAttributes([In] SIATTRIBFLAGS dwAttribFlags, [In] uint sfgaoMask, out uint psfgaoAttribs);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    int GetCount();

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    IShellItem GetItemAt(int dwIndex);

    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
    void EnumItems([MarshalAs(UnmanagedType.Interface)] out nint ppenumShellItems);
}

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("43826d1e-e718-42ee-bc55-a1e261c37bfe")]
public interface IShellItem
{
    void BindToHandler(
        nint pbc,
        [MarshalAs(UnmanagedType.LPStruct)] Guid bhid,
        [MarshalAs(UnmanagedType.LPStruct)] Guid riid,
        out nint ppv);

    void GetParent(out IShellItem ppsi);

    [return: MarshalAs(UnmanagedType.LPWStr)]
    string GetDisplayName(SIGDN sigdnName);

    void GetAttributes(uint sfgaoMask, out uint psfgaoAttribs);

    void Compare(IShellItem psi, uint hint, out int piOrder);
}

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("000214E2-0000-0000-C000-000000000046")]
public interface IShellBrowser
{
    void GetWindow(out nint phwnd);

    void ContextSensitiveHelp(bool fEnterMode);

    void InsertMenusSB(nint hmenuShared, nint lpMenuWidths);

    void SetMenuSB(nint hmenuShared, nint holeMenuRes, nint hwndActiveObject);

    void RemoveMenusSB(nint hmenuShared);

    void SetStatusTextSB(nint pszStatusText);

    void EnableModelessSB(bool fEnable);

    void TranslateAcceleratorSB(nint pmsg, ushort wID);

    void BrowseObject(nint pidl, uint wFlags);

    void GetViewStateStream(uint grfMode, nint ppStrm);

    void GetControlWindow(uint id, out nint lpIntPtr);

    void SendControlMsg(uint id, uint uMsg, uint wParam, uint lParam, nint pret);

    [return: MarshalAs(UnmanagedType.IUnknown)]
    object QueryActiveShellView();

    void OnViewWindowActive(IShellView ppshv);

    void SetToolbarItems(nint lpButtons, uint nButtons, uint uFlags);
}

[ComImport]
[Guid("000214E3-0000-0000-C000-000000000046")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[SuppressUnmanagedCodeSecurity]
public interface IShellView
{
}

[Guid("6D5140C1-7436-11CE-8034-00AA006009FA")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IServiceProvider
{
    [return: MarshalAs(UnmanagedType.IUnknown)]
    object QueryService([MarshalAs(UnmanagedType.LPStruct)] Guid service, [MarshalAs(UnmanagedType.LPStruct)] Guid riid);
}

[ComImport]
[Guid("cde725b0-ccc9-4519-917e-325d72fab4ce")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[SuppressUnmanagedCodeSecurity]
public interface IFolderView
{
    void GetCurrentViewMode([Out] out uint pViewMode);

    void SetCurrentViewMode([In] uint viewMode);

    void GetFolder([In, MarshalAs(UnmanagedType.LPStruct)] Guid riid, [Out, MarshalAs(UnmanagedType.Interface)] out object ppv);

    void Item([In] int iItemIndex, [Out] out nint ppidl);

    void ItemCount([In] uint uFlags, [Out] out int pcItems);

    void Items([In] uint uFlags, [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid, [Out, MarshalAs(UnmanagedType.Interface)] out object ppv);

    void GetSelectionMarkedItem([Out] out int piItem);

    void GetFocusedItem([Out] out int piItem);

    void GetItemPosition([In] nint pidl, [Out] out Point ppt);

    void GetSpacing([In, Out] ref Point ppt);

    void GetDefaultSpacing([Out] out Point ppt);

    void GetAutoArrange();

    void SelectItem([In] int iItem, [In] uint dwFlags);

    void SelectAndPositionItems([In] uint cidl, [In] nint apidl, [In] nint apt, [In] uint dwFlags);
}

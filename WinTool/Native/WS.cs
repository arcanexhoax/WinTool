using System;

namespace WinTool.Native;

/// <summary>
/// Windows styles attributes. https://learn.microsoft.com/en-us/windows/win32/winmsg/window-styles
/// </summary>
[Flags]
public enum WS : long
{
    /// <summary> The window is an overlapped window. An overlapped window has a title bar and a border. Same as the <see cref="TILED"/> style. </summary>
    OVERLAPPED = 0x00000000,

    /// <summary> The window is a pop-up window. This style cannot be used with the <see cref="CHILD"/> style. </summary>
    POPUP = 0x80000000,

    /// <summary> 
    /// The window is a child window. 
    /// A window with this style cannot have a menu bar. 
    /// This style cannot be used with the <see cref="POPUP"/> style. 
    /// </summary>
    CHILD = 0x40000000,

    /// <summary> The window is initially minimized. Same as the <see cref="ICONIC"/> style. </summary>
    MINIMIZE = 0x20000000,

    /// <summary> The window is initially visible. This style can be turned on and off by using the ShowWindow or SetWindowPos function. </summary>
    VISIBLE = 0x10000000,

    /// <summary> 
    /// The window is initially disabled. A disabled window cannot receive input from the user. 
    /// To change this after a window has been created, use the EnableWindow function. 
    /// </summary>
    DISABLED = 0x08000000,

    /// <summary> 
    /// Clips child windows relative to each other; that is, when a particular child window receives a WM_PAINT message, 
    /// the <see cref="CLIPSIBLINGS"/> style clips all other overlapping child windows out of the region of the child window to be updated. 
    /// If <see cref="CLIPSIBLINGS"/> is not specified and child windows overlap, it is possible, when drawing within the client area of a child window, 
    /// to draw within the client area of a neighboring child window. 
    /// </summary>
    CLIPSIBLINGS = 0x04000000,

    /// <summary> 
    /// Excludes the area occupied by child windows when drawing occurs within the parent window. 
    /// This style is used when creating the parent window. 
    /// </summary>
    CLIPCHILDREN = 0x02000000,

    /// <summary> The window is initially maximized. </summary>
    MAXIMIZE = 0x01000000,

    /// <summary> The window has a thin-line border </summary>
    BORDER = 0x00800000,

    /// <summary> The window has a border of a style typically used with dialog boxes. A window with this style cannot have a title bar. </summary>
    DLGFRAME = 0x00400000,

    /// <summary> The window has a vertical scroll bar. </summary>
    VSCROLL = 0x00200000,

    /// <summary> The window has a horizontal scroll bar. </summary>
    HSCROLL = 0x00100000,

    /// <summary> The window has a window menu on its title bar. The <see cref="CAPTION"/> style must also be specified. </summary>
    SYSMENU = 0x00080000,

    /// <summary> The window has a sizing border. Same as the <see cref="SIZEBOX"/> style. </summary>
    THICKFRAME = 0x00040000,

    /// <summary> 
    /// The window is the first control of a group of controls. 
    /// The group consists of this first control and all controls defined after it, up to the next control with the <see cref="GROUP"/> style. 
    /// The first control in each group usually has the <see cref="TABSTOP"/> style so that the user can move from group to group. 
    /// The user can subsequently change the keyboard focus from one control in the group to the next control in the group by using the direction keys. 
    /// You can turn this style on and off to change dialog box navigation. To change this style after a window has been created, use the SetWindowLong function. 
    /// </summary>
    GROUP = 0x00020000,

    /// <summary> 
    /// The window is a control that can receive the keyboard focus when the user presses the TAB key.
    /// Pressing the TAB key changes the keyboard focus to the next control with the <see cref="TABSTOP"/> style. 
    /// You can turn this style on and off to change dialog box navigation.
    /// To change this style after a window has been created, use the SetWindowLong function. 
    /// For user-created windows and modeless dialogs to work with tab stops, alter the message loop to call the IsDialogMessage function. 
    /// </summary>
    TABSTOP = 0x00010000,


    /// <summary> 
    /// The window has a maximize button. Cannot be combined with the WS_EX_CONTEXTHELP style. 
    /// <see cref="SYSMENU"/> style must also be specified. 
    /// </summary>
    MINIMIZEBOX = GROUP,

    /// <summary> 
    /// The window has a minimize button. Cannot be combined with the WS_EX_CONTEXTHELP style. 
    /// The <see cref="SYSMENU"/> style must also be specified. 
    /// </summary>
    MAXIMIZEBOX = TABSTOP,


    /// <summary> The window has a title bar (includes the <see cref="BORDER"/> style). </summary>
    CAPTION = BORDER | DLGFRAME,

    /// <summary> 
    /// The window is an overlapped window. 
    /// An overlapped window has a title bar and a border. Same as the <see cref="OVERLAPPED"/> style. 
    /// </summary>
    TILED = OVERLAPPED,

    /// <summary> The window is initially minimized. Same as the <see cref="MINIMIZE"/> style. </summary>
    ICONIC = MINIMIZE,

    /// <summary> The window has a sizing border. Same as the <see cref="THICKFRAME"/> style. </summary>
    SIZEBOX = THICKFRAME,


    /// <summary> The window is an overlapped window. Same as the <see cref="TILEDWINDOW"/> style. </summary>
    OVERLAPPEDWINDOW = OVERLAPPED | CAPTION | SYSMENU | THICKFRAME | MINIMIZEBOX | MAXIMIZEBOX,

    /// <summary> The window is an overlapped window. Same as the <see cref="OVERLAPPEDWINDOW"/> style. </summary>
    TILEDWINDOW = OVERLAPPEDWINDOW,


    /// <summary> 
    /// The window is a pop-up window.
    /// The <see cref="CAPTION"/> and <see cref="POPUPWINDOW"/> styles must be combined to make the window menu visible. 
    /// </summary>
    /// 
    POPUPWINDOW = POPUP | BORDER | SYSMENU,

    /// <summary> Same as the <see cref="CHILD"/> style. </summary>
    CHILDWINDOW = CHILD,
}

/// <summary>
/// Windows extended styles attributes. https://learn.microsoft.com/en-us/windows/win32/winmsg/extended-window-styles
/// </summary>
[Flags]
public enum WS_EX : long
{
    /// <summary> The window accepts drag-drop files. </summary>
    ACCEPTFILES = 0x00000010L,

    /// <summary> Forces a top-level window onto the taskbar when the window is visible. </summary>
    APPWINDOW = 0x00040000L,

    /// <summary> The window has a border with a sunken edge. </summary>
    CLIENTEDGE = 0x00000200L,

    /// <summary> 
    /// Paints all descendants of a window in bottom-to-top painting order using double-buffering.
    /// Bottom-to-top painting order allows a descendent window to have translucency(alpha) and transparency(color-key) effects, 
    /// but only if the descendent window also has the <see cref="TRANSPARENT"/> bit set.
    /// Double-buffering allows the window and its descendents to be painted without flicker.
    /// This cannot be used if the window has a class style of CS_OWNDC, CS_CLASSDC, or CS_PARENTDC.
    /// Windows 2000: This style is not supported. </summary>
    COMPOSITED = 0x02000000L,

    /// <summary> 
    /// The title bar of the window includes a question mark. 
    /// When the user clicks the question mark, the cursor changes to a question mark with a pointer. 
    /// If the user then clicks a child window, the child receives a WM_HELP message. 
    /// The child window should pass the message to the parent window procedure, which should call the WinHelp function using the HELP_WM_HELP command.
    /// The Help application displays a pop-up window that typically contains help for the child window.
    /// <see cref="CONTEXTHELP"/> cannot be used with the <see cref="WS.MAXIMIZEBOX"/> or <see cref="WS.MINIMIZEBOX"/> styles. 
    /// </summary>
    CONTEXTHELP = 0x00000400L,

    /// <summary> 
    /// The window itself contains child windows that should take part in dialog box navigation.
    /// If this style is specified, the dialog manager recurses into children of this window when performing navigation operations such as 
    /// handling the TAB key, an arrow key, or a keyboard mnemonic. 
    /// </summary>
    CONTROLPARENT = 0x00010000L,

    /// <summary> 
    /// The window has a double border; the window can, optionally, 
    /// be created with a title bar by specifying the <see cref="WS.CAPTION"/> style in the dwStyle parameter.
    /// </summary>
    DLGMODALFRAME = 0x00000001L,

    /// <summary> 
    /// The window is a layered window.
    /// This style cannot be used if the window has a class style of either CS_OWNDC or CS_CLASSDC.
    /// Windows 8: The <see cref="LAYERED"/> style is supported for top-level windows and child windows.
    /// Previous Windows versions support <see cref="LAYERED"/> only for top-level windows. 
    /// </summary>
    LAYERED = 0x00080000L,

    /// <summary> 
    /// If the shell language is Hebrew, Arabic, or another language that supports reading order alignment, 
    /// the horizontal origin of the window is on the right edge. 
    /// Increasing horizontal values advance to the left. 
    /// </summary>
    LAYOUTRTL = 0x00400000L,

    /// <summary> The window has generic left-aligned properties. This is the default. </summary>
    LEFT = 0x00000000L,

    /// <summary> 
    /// If the shell language is Hebrew, Arabic, or another language that supports reading order alignment, 
    /// the vertical scroll bar (if present) is to the left of the client area.
    /// For other languages, the style is ignored. 
    /// </summary>
    LEFTSCROLLBAR = 0x00004000L,

    /// <summary> The window text is displayed using left-to-right reading-order properties. This is the default. </summary>
    LTRREADING = LEFT,

    /// <summary> The window is a MDI child window. </summary>
    MDICHILD = 0x00000040L,

    /// <summary> 
    /// A top-level window created with this style does not become the foreground window when the user clicks it. 
    /// The system does not bring this window to the foreground when the user minimizes or closes the foreground window.
    /// The window should not be activated through programmatic access or via keyboard navigation by accessible technology, such as Narrator.
    /// To activate the window, use the SetActiveWindow or SetForegroundWindow function. The window does not appear on the taskbar by default. 
    /// To force the window to appear on the taskbar, use the <see cref="APPWINDOW"/> style. 
    /// </summary>
    NOACTIVATE = 0x08000000L,

    /// <summary> The window does not pass its window layout to its child windows. </summary>
    NOINHERITLAYOUT = 0x00100000L,

    /// <summary> The child window created with this style does not send the WM_PARENTNOTIFY message to its parent window when it is created or destroyed. </summary>
    NOPARENTNOTIFY = 0x00000004L,

    /// <summary> 
    /// The window does not render to a redirection surface.
    /// This is for windows that do not have visible content or that use mechanisms other than surfaces to provide their visual.
    ///</summary>
    NOREDIRECTIONBITMAP = 0x00200000L,

    /// <summary> The window is an overlapped window. </summary>
    OVERLAPPEDWINDOW = WINDOWEDGE | CLIENTEDGE,

    /// <summary> The window is palette window, which is a modeless dialog box that presents an array of commands. </summary>
    PALETTEWINDOW = WINDOWEDGE | TOOLWINDOW | TOPMOST,

    /// <summary> 
    /// The window has generic "right-aligned" properties.
    /// This depends on the window class. 
    /// This style has an effect only if the shell language is Hebrew, Arabic, or another language that supports reading-order alignment; 
    /// otherwise, the style is ignored.
    /// Using the <see cref="RIGHT"/> style for static or edit controls has the same effect as using the SS_RIGHT or ES_RIGHT style, respectively. 
    /// Using this style with button controls has the same effect as using BS_RIGHT and BS_RIGHTBUTTON styles. 
    /// </summary>
    RIGHT = 0x00001000L,

    /// <summary> The vertical scroll bar (if present) is to the right of the client area. This is the default. </summary>
    RIGHTSCROLLBAR = LEFT,

    /// <summary> 
    /// If the shell language is Hebrew, Arabic, or another language that supports reading-order alignment, 
    /// the window text is displayed using right-to-left reading-order properties.
    /// For other languages, the style is ignored. 
    /// </summary>
    RTLREADING = 0x00002000L,

    /// <summary> The window has a three-dimensional border style intended to be used for items that do not accept user input. </summary>
    STATICEDGE = 0x00020000L,

    /// <summary> 
    /// The window is intended to be used as a floating toolbar.
    /// A tool window has a title bar that is shorter than a normal title bar, and the window title is drawn using a smaller font.
    /// A tool window does not appear in the taskbar or in the dialog that appears when the user presses ALT + TAB.
    /// If a tool window has a system menu, its icon is not displayed on the title bar. 
    /// However, you can display the system menu by right-clicking or by typing ALT + SPACE.
    /// </summary>
    TOOLWINDOW = 0x00000080L,

    /// <summary>
    /// The window should be placed above all non-topmost windows and should stay above them, even when the window is deactivated.
    /// To add or remove this style, use the SetWindowPos function.
    /// </summary>
    TOPMOST = 0x00000008L,

    /// <summary> The window should not be painted until siblings beneath the window (that were created by the same thread) have been painted.
    /// The window appears transparent because the bits of underlying sibling windows have already been painted.
    /// To achieve transparency without these restrictions, use the SetWindowRgn function. 
    /// </summary>
    TRANSPARENT = 0x00000020L,

    /// <summary> The window has a border with a raised edge. </summary>
    WINDOWEDGE = 0x00000100L,
}
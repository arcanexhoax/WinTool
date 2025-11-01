using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinTool.Native;

[Flags]
public enum DWMWINDOWATTRIBUTE
{
    /// <summary>
    /// Is non-client rendering enabled/disabled
    /// </summary>
    DWMWA_NCRENDERING_ENABLED = 1,

    /// <summary>
    /// DWMNCRENDERINGPOLICY - Non-client rendering policy
    /// </summary>
    DWMWA_NCRENDERING_POLICY = 2,

    /// <summary>
    /// Potentially enable/forcibly disable transitions
    /// </summary>
    DWMWA_TRANSITIONS_FORCEDISABLED = 3,

    /// <summary>
    /// Enables content rendered in the non-client area to be visible on the frame drawn by DWM.
    /// </summary>
    DWMWA_ALLOW_NCPAINT = 4,

    /// <summary>
    /// Retrieves the bounds of the caption button area in the window-relative space.
    /// </summary>
    DWMWA_CAPTION_BUTTON_BOUNDS = 5,

    /// <summary>
    /// Is non-client content RTL mirrored
    /// </summary>
    DWMWA_NONCLIENT_RTL_LAYOUT = 6,

    /// <summary>
    /// Forces the window to display an iconic thumbnail or peek representation (a static bitmap), even if a live or snapshot representation of the window is available.
    /// </summary>
    DWMWA_FORCE_ICONIC_REPRESENTATION = 7,

    /// <summary>
    /// Designates how Flip3D will treat the window.
    /// </summary>
    DWMWA_FLIP3D_POLICY = 8,

    /// <summary>
    /// Gets the extended frame bounds rectangle in screen space
    /// </summary>
    DWMWA_EXTENDED_FRAME_BOUNDS = 9,

    /// <summary>
    /// Indicates an available bitmap when there is no better thumbnail representation.
    /// </summary>
    DWMWA_HAS_ICONIC_BITMAP = 10,

    /// <summary>
    /// Don't invoke Peek on the window.
    /// </summary>
    DWMWA_DISALLOW_PEEK = 11,

    /// <summary>
    /// LivePreview exclusion information
    /// </summary>
    DWMWA_EXCLUDED_FROM_PEEK = 12,

    /// <summary>
    /// Cloaks the window such that it is not visible to the user.
    /// </summary>
    DWMWA_CLOAK = 13,

    /// <summary>
    /// If the window is cloaked, provides one of the following values explaining why.
    /// </summary>
    DWMWA_CLOAKED = 14,

    /// <summary>
    /// Freeze the window's thumbnail image with its current visuals. Do no further live updates on the thumbnail image to match the window's contents.
    /// </summary>
    DWMWA_FREEZE_REPRESENTATION = 15,

    /// <summary>
    /// BOOL, Updates the window only when desktop composition runs for other reasons
    /// </summary>
    DWMWA_PASSIVE_UPDATE_MODE = 16,

    /// <summary>
    /// BOOL, Allows the use of host backdrop brushes for the window.
    /// </summary>
    DWMWA_USE_HOSTBACKDROPBRUSH = 17,

    /// <summary>
    /// Allows a window to either use the accent color, or dark, according to the user Color Mode preferences.
    /// </summary>
    DMWA_USE_IMMERSIVE_DARK_MODE_OLD = 19,

    /// <summary>
    /// Allows a window to either use the accent color, or dark, according to the user Color Mode preferences.
    /// </summary>
    DWMWA_USE_IMMERSIVE_DARK_MODE = 20,

    /// <summary>
    /// Controls the policy that rounds top-level window corners.
    /// <para>Windows 11 and above.</para>
    /// </summary>
    DWMWA_WINDOW_CORNER_PREFERENCE = 33,

    /// <summary>
    /// The color of the thin border around a top-level window.
    /// </summary>
    DWMWA_BORDER_COLOR = 34,

    /// <summary>
    /// The color of the caption.
    /// <para>Windows 11 and above.</para>
    /// </summary>
    DWMWA_CAPTION_COLOR = 35,

    /// <summary>
    /// The color of the caption text.
    /// <para>Windows 11 and above.</para>
    /// </summary>
    DWMWA_TEXT_COLOR = 36,

    /// <summary>
    /// Width of the visible border around a thick frame window.
    /// <para>Windows 11 and above.</para>
    /// </summary>
    DWMWA_VISIBLE_FRAME_BORDER_THICKNESS = 37,

    /// <summary>
    /// Allows to enter a value from 0 to 4 deciding on the imposed backdrop effect.
    /// </summary>
    DWMWA_SYSTEMBACKDROP_TYPE = 38,

    /// <summary>
    /// Indicates whether the window should use the Mica effect.
    /// <para>Windows 11 and above.</para>
    /// </summary>
    DWMWA_MICA_EFFECT = 1029,
}

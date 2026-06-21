using NLog;
using System;
using UIAutomationClient;
using WinTool.Native;

namespace WinTool.Utils;

public class CaretHelper
{
    private const int TextPatternId = 10014;
    private const int TextPattern2Id = 10024;

    private static readonly Logger s_logger = LogManager.GetCurrentClassLogger();
    private static readonly CUIAutomation s_automation = new();

    public static RECT? GetCaretRect()
    {
        var focusedElement = s_automation.GetFocusedElement();

        var caretRect = GetCaretRectByTextPattern2(focusedElement);

        if (IsRectValid(caretRect))
        {
            s_logger.Debug("Used TextPattern2 to get caret rect.");
            return caretRect;
        }

        if (NativeMethods.GetGuiThreadInfo() is not { } info)
            return null;

        caretRect = GetCaretRectByGuiThreadInfo(info);

        if (IsRectValid(caretRect))
        {
            s_logger.Debug("Used GuiThreadInfo to get caret rect.");
            return caretRect;
        }

        caretRect = GetCaretRectByAccessible(info.hwndFocus);

        if (IsRectValid(caretRect))
        {
            s_logger.Debug("Used AccessibleCaretRect to get caret rect.");
            return caretRect;
        }

        caretRect = GetCaretRectByTextPattern(focusedElement);

        if (IsRectValid(caretRect))
        {
            s_logger.Debug("Used TextPattern to get caret rect.");
            return caretRect;
        }

        return null;
    }

    public static RECT? GetCaretRectByTextPattern2(IUIAutomationElement element)
    {
        try
        {
            if (element?.GetCurrentPattern(TextPattern2Id) is not IUIAutomationTextPattern2 textPattern2)
                return null;
            
            var caretRange = textPattern2.GetCaretRange(out int isFound);

            if (isFound == 0)
                return null;

            return GetRectOfTextRange(caretRange);
        }
        catch
        {
            return null;
        }
    }

    public static RECT? GetCaretRectByTextPattern(IUIAutomationElement element)
    {
        try
        {
            if (element?.GetCurrentPattern(TextPatternId) is not IUIAutomationTextPattern textPattern)
                return null;

            var selection = textPattern.GetSelection();

            if (selection == null || selection.Length == 0)
                return null;

            var selectionRange = selection.GetElement(0);

            return GetRectOfTextRange(selectionRange);
        }
        catch
        {
            return null;
        }
    }

    private static RECT? GetRectOfTextRange(IUIAutomationTextRange? textRange)
    {
        if (textRange == null)
            return null;

        var boundingRectangle = textRange.GetBoundingRectangles();

        if ((boundingRectangle == null || boundingRectangle.Length < 4)
            && textRange.CompareEndpoints(
                TextPatternRangeEndpoint.TextPatternRangeEndpoint_Start,
                textRange,
                TextPatternRangeEndpoint.TextPatternRangeEndpoint_End) == 0)
        {
            var characterRange = textRange.Clone();
            characterRange.ExpandToEnclosingUnit(TextUnit.TextUnit_Character);
            boundingRectangle = characterRange.GetBoundingRectangles();
        }

        if (boundingRectangle == null || boundingRectangle.Length < 4)
            return null;

        int left = Convert.ToInt32(boundingRectangle.GetValue(0));
        int top = Convert.ToInt32(boundingRectangle.GetValue(1));
        int width = Convert.ToInt32(boundingRectangle.GetValue(2));
        int height = Convert.ToInt32(boundingRectangle.GetValue(3));

        return BuildRect(left, top, width, height);
    }

    private static RECT? GetCaretRectByGuiThreadInfo(GUITHREADINFO info)
    {
        if (info.hwndCaret == nint.Zero)
            return null;

        var topLeft = new POINT
        {
            X = info.rcCaret.left,
            Y = info.rcCaret.top
        };

        if (!NativeMethods.ClientToScreen(info.hwndCaret, ref topLeft))
            return null;

        return BuildRect(
            topLeft.X,
            topLeft.Y,
            info.rcCaret.right - info.rcCaret.left,
            info.rcCaret.bottom - info.rcCaret.top);
    }

    private static RECT? GetCaretRectByAccessible(nint hwnd)
    {
        var guid = typeof(IAccessible).GUID;
        object? accessibleObject = null;
        var retVal = NativeMethods.AccessibleObjectFromWindow(hwnd, NativeMethods.OBJID_CARET, ref guid, ref accessibleObject);

        if (retVal != 0 || accessibleObject is not IAccessible accessible)
            return null;

        accessible.accLocation(out int left, out int top, out int width, out int height, NativeMethods.CHILDID_SELF);

        return BuildRect(left, top, width, height);
    }

    private static bool IsRectValid(RECT? rect)
    {
        return rect is { bottom: > 0, right: > 0, left: > 0, top: > 0 };
    }

    private static RECT BuildRect(int left, int top, int width, int height)
    {
        return new RECT()
        {
            bottom = top + height,
            left = left,
            right = left + width,
            top = top
        };
    }
}

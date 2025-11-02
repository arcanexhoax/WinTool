using System;
using System.Diagnostics;
using UIAutomationClient;
using WinTool.Native;

namespace WinTool.Utils;

public class CarretHelper
{
    private const int TextPatternId = 10014;
    private const int TextPattern2Id = 10024;

    private static CUIAutomation s_automation = new();

    public static RECT? GetCaretRect()
    {
        var focusedElement = s_automation.GetFocusedElement();

        var caretRect = GetCaretRectByTextPattern2(focusedElement);

        if (IsRectValid(caretRect))
        {
            Debug.WriteLine("Used TextPattern2 to get caret rect.");
            return caretRect;
        }

        if (NativeMethods.GetGuiThreadInfo() is not { } info)
            return null;

        caretRect = GetCaretRectByAccessible(info.hwndFocus);

        if (IsRectValid(caretRect))
        {
            Debug.WriteLine("Used AccessibleCaretRect to get caret rect.");
            return caretRect;
        }

        caretRect = GetCaretRectByTextPattern(focusedElement);

        if (IsRectValid(caretRect))
        {
            Debug.WriteLine("Used TextPattern to get caret rect.");
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

        if (boundingRectangle == null || boundingRectangle.Length < 4)
            return null;

        int left = Convert.ToInt32(boundingRectangle.GetValue(0));
        int top = Convert.ToInt32(boundingRectangle.GetValue(1));
        int width = Convert.ToInt32(boundingRectangle.GetValue(2));
        int height = Convert.ToInt32(boundingRectangle.GetValue(3));

        return BuildRect(left, top, width, height);
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

    private static RECT GetCaretRectByWinApi(nint hwnd)
    {
        uint idAttach = 0;
        uint curThreadId = 0;
        POINT caretPoint;

        try
        {
            idAttach = NativeMethods.GetWindowThreadProcessId(hwnd, out uint id);
            curThreadId = NativeMethods.GetCurrentThreadId();

            // To attach to current thread
            var sa = NativeMethods.AttachThreadInput(idAttach, curThreadId, true);
            var caretPos = NativeMethods.GetCaretPos(out caretPoint);
            NativeMethods.ClientToScreen(hwnd, ref caretPoint);
        }
        finally
        {
            // To dettach from current thread
            NativeMethods.AttachThreadInput(idAttach, curThreadId, false);
        }

        return BuildRect(caretPoint.X, caretPoint.Y, 1, 20);
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

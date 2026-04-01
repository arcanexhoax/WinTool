# XAML Style

Use [src/WinTool/Settings.XamlStyler](../src/WinTool/Settings.XamlStyler) as the source of truth for XAML formatting in this repository.

When editing XAML:

- keep attribute ordering, wrapping, and line breaks aligned with that file
- preserve the existing indentation and element layout conventions
- avoid hand-formatting that conflicts with the XamlStyler settings
- if a XamlStyler format command is available in the editor, run it before finishing the change

If formatting is not available in the current environment, make the XAML changes manually but keep them consistent with the settings file.
namespace AuditFiles.Core.Rules;

/// <summary>
/// Well-known SharePoint Online / OneDrive restrictions used as default audit thresholds, based on
/// Microsoft's "Invalid file names and file types" and "restrictions and limitations" documentation.
/// Microsoft periodically revises these lists; treat the defaults here as a starting point and adjust
/// them (via <see cref="Models.ScanOptions"/>, or by editing this file) for your tenant before relying
/// on the results for a real migration decision.
/// </summary>
public static class SharePointLimits
{
    /// <summary>
    /// Characters that SharePoint does not allow anywhere in a file or folder name.
    /// </summary>
    public static readonly char[] InvalidNameCharacters =
    {
        '"', '*', ':', '<', '>', '?', '/', '\\', '|', '#', '%',
    };

    /// <summary>
    /// Names that are reserved outright (Windows device names) or have special meaning to SharePoint,
    /// compared case-insensitively.
    /// </summary>
    public static readonly HashSet<string> ReservedNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "CON", "PRN", "AUX", "NUL",
        "COM0", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
        "LPT0", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9",
        "_vti_", "desktop.ini", "forms",
    };

    /// <summary>
    /// Name prefixes that are reserved, compared case-insensitively (e.g. Office lock files such as
    /// "~$budget.xlsx").
    /// </summary>
    public static readonly string[] ReservedNamePrefixes = { "~$" };

    /// <summary>
    /// File extensions (including the leading dot) that SharePoint blocks from being uploaded.
    /// </summary>
    public static readonly HashSet<string> BlockedFileExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".ade", ".adp", ".app", ".asa", ".ashx", ".asmx", ".asp", ".bas", ".bat",
        ".cdx", ".cer", ".cla", ".class", ".cmd", ".cnt", ".com", ".config", ".cpl",
        ".crt", ".csh", ".dll", ".exe", ".fxp", ".gadget", ".grp", ".hlp", ".hpj",
        ".hta", ".htr", ".htw", ".ida", ".idc", ".idq", ".ins", ".isp", ".its",
        ".jse", ".ksh", ".lnk", ".mad", ".maf", ".mag", ".mam", ".maq", ".mar",
        ".mas", ".mat", ".mau", ".mav", ".maw", ".mda", ".mdb", ".mde", ".mdt",
        ".mdw", ".mdz", ".msc", ".msh", ".msh1", ".msh1xml", ".msh2", ".msh2xml",
        ".mshxml", ".msi", ".msp", ".mst", ".ops", ".pcd", ".pif", ".prf", ".prg",
        ".printerexport", ".ps1", ".ps1xml", ".ps2", ".ps2xml", ".psc1", ".psc2",
        ".psd1", ".psdm1", ".pst", ".reg", ".rep", ".rgs", ".scf", ".scr", ".sct",
        ".shb", ".shs", ".sys", ".theme", ".tmp", ".url", ".vb", ".vbe", ".vbp",
        ".vbs", ".vsmacros", ".vss", ".vst", ".vsw", ".ws", ".wsc", ".wsf", ".wsh",
        ".xbap", ".xnk",
    };
}

namespace AuditFiles.Core.Rules;

/// <summary>
/// SharePoint Online / OneDrive restrictions used as default audit thresholds, based on Microsoft's
/// published guidance:
/// - "Restrictions and limitations in OneDrive and SharePoint"
///   (support.microsoft.com/office/64883a5d-228e-48f5-b3d2-eb39e07630fa)
/// - "Types of files that cannot be added to a list or library"
///   (support.microsoft.com/office/30be234d-e551-4c2a-8de8-f8546ffbf5b3)
/// - "SharePoint limits" (Service Descriptions,
///   learn.microsoft.com/office365/servicedescriptions/sharepoint-online-service-description/sharepoint-online-limits)
/// Microsoft periodically revises these lists (e.g. '#' and '%' used to be blocked by default and are
/// now allowed unless a tenant admin disables "special characters" support); treat the defaults here
/// as a starting point and adjust them (via <see cref="Models.ScanOptions"/>, or by editing this file)
/// for your tenant before relying on the results for a real migration decision.
/// </summary>
public static class SharePointLimits
{
    /// <summary>
    /// Characters that SharePoint does not allow anywhere in a file or folder name. '#' and '%' are
    /// deliberately not included: they are supported by default in OneDrive and SharePoint in
    /// Microsoft 365 (only blocked if a tenant admin explicitly disables special-character support).
    /// </summary>
    public static readonly char[] InvalidNameCharacters =
    {
        '"', '*', ':', '<', '>', '?', '/', '\\', '|', '{', '}',
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
        "_vti_", "desktop.ini", ".lock",
    };

    /// <summary>
    /// Name prefixes that are reserved, compared case-insensitively: Office lock files such as
    /// "~$budget.xlsx". Folders have an additional, broader rule - see
    /// <see cref="Rules.InvalidNameRule"/> - they cannot start with "~" at all, not just "~$".
    /// </summary>
    public static readonly string[] ReservedNamePrefixes = { "~$" };

    /// <summary>
    /// File extensions (including the leading dot) that SharePoint and OneDrive block from being
    /// uploaded by default, per "Types of files that cannot be added to a list or library".
    /// </summary>
    public static readonly HashSet<string> BlockedFileExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".ade", ".adp", ".app", ".asa", ".asp", ".bas", ".bat", ".cdx", ".cer",
        ".chm", ".class", ".cmd", ".com", ".cpl", ".crt", ".csh", ".dll", ".exe",
        ".fxp", ".hlp", ".hta", ".htr", ".htw", ".ida", ".idc", ".idq", ".ins",
        ".isp", ".its", ".jse", ".ksh", ".lnk", ".mad", ".maf", ".mag", ".mam",
        ".maq", ".mar", ".mas", ".mat", ".mau", ".mav", ".maw", ".mda", ".mdb",
        ".mde", ".mdt", ".mdw", ".mdz", ".msc", ".msi", ".msp", ".mst", ".ops",
        ".pcd", ".pif", ".prf", ".prg", ".printer", ".pst", ".reg", ".scf",
        ".scr", ".sct", ".shb", ".shs", ".shtm", ".shtml", ".stm", ".url",
        ".vb", ".vbe", ".vbs", ".wsc", ".wsf", ".wsh",
    };
}

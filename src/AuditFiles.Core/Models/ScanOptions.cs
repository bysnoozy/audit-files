namespace AuditFiles.Core.Models;

public sealed class ScanOptions
{
    public required string RootPath { get; init; }

    /// <summary>
    /// Maximum length, in characters, of a full SharePoint URL (site + library + folder path + file
    /// name). Microsoft's documented limit is 400 characters; verify against current Microsoft 365
    /// documentation before relying on this value for a real migration decision.
    /// </summary>
    public int MaxFullUrlLength { get; init; } = 400;

    /// <summary>
    /// Number of characters expected to be consumed by the destination site URL, document library
    /// name, and any additional prefix before the folder structure being scanned. Subtracted from
    /// <see cref="MaxFullUrlLength"/> to compute the budget available to the scanned relative path.
    /// Adjust this to match the real destination site once it is known.
    /// </summary>
    public int ReservedUrlPrefixLength { get; init; } = 100;

    /// <summary>
    /// Maximum length, in characters, of a single file or folder name.
    /// </summary>
    public int MaxNameLength { get; init; } = 400;

    /// <summary>
    /// Maximum file size, in bytes. Defaults to 250 GB, the SharePoint Online / OneDrive upload
    /// limit at the time of writing.
    /// </summary>
    public long MaxFileSizeInBytes { get; init; } = 250L * 1024 * 1024 * 1024;

    /// <summary>
    /// Folder depth beyond which a warning is raised, since deep nesting is a common cause of
    /// path-length failures. This is a configurable heuristic, not a hard SharePoint limit.
    /// </summary>
    public int MaxFolderDepth { get; init; } = 50;

    public bool IncludeBlockedFileTypeCheck { get; init; } = true;

    /// <summary>
    /// Additional file extensions (including the leading dot, e.g. ".xyz") to treat as blocked,
    /// on top of the built-in <see cref="Rules.SharePointLimits.BlockedFileExtensions"/> list.
    /// </summary>
    public IReadOnlySet<string>? ExtraBlockedExtensions { get; init; }

    public int EffectiveMaxRelativePathLength => Math.Max(0, MaxFullUrlLength - ReservedUrlPrefixLength);
}

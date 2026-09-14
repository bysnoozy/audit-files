namespace AuditFiles.Core.Models;

public enum AuditIssueType
{
    PathTooLong,
    InvalidCharacterInName,
    NameStartsOrEndsWithSpace,
    NameEndsWithPeriod,
    NameTooLong,
    ReservedName,
    BlockedFileType,
    FileTooLarge,
    FolderTooDeep,
    DuplicateNameDifferingByCase,
}

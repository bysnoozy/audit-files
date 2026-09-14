using AuditFiles.Core.Models;
using AuditFiles.Core.Rules;
using Xunit;

namespace AuditFiles.Core.Tests;

public class BlockedFileTypeRuleTests
{
    private readonly BlockedFileTypeRule _rule = new();
    private readonly ScanOptions _options = new() { RootPath = "C:\\root" };

    [Fact]
    public void Evaluate_BlockedExtension_ReturnsIssue()
    {
        var entry = MakeEntry("installer.exe");

        var issues = _rule.Evaluate(entry, _options).ToList();

        var issue = Assert.Single(issues);
        Assert.Equal(AuditIssueType.BlockedFileType, issue.Type);
    }

    [Fact]
    public void Evaluate_AllowedExtension_ReturnsNoIssue()
    {
        var entry = MakeEntry("report.docx");

        var issues = _rule.Evaluate(entry, _options).ToList();

        Assert.Empty(issues);
    }

    [Fact]
    public void Evaluate_Folder_ReturnsNoIssue()
    {
        var entry = new ScanEntry
        {
            FullPath = "installer.exe",
            RelativePath = "installer.exe",
            Name = "installer.exe",
            Kind = ScanEntryKind.Folder,
            SizeInBytes = 0,
            Depth = 1,
            LastModifiedUtc = DateTime.UtcNow,
        };

        var issues = _rule.Evaluate(entry, _options).ToList();

        Assert.Empty(issues);
    }

    private static ScanEntry MakeEntry(string name) => new()
    {
        FullPath = name,
        RelativePath = name,
        Name = name,
        Kind = ScanEntryKind.File,
        SizeInBytes = 0,
        Depth = 1,
        LastModifiedUtc = DateTime.UtcNow,
    };
}

using AuditFiles.Core.Models;
using AuditFiles.Core.Rules;
using Xunit;

namespace AuditFiles.Core.Tests;

public class FileSizeRuleTests
{
    private readonly FileSizeRule _rule = new();

    [Fact]
    public void Evaluate_FileOverLimit_ReturnsIssue()
    {
        var options = new ScanOptions { RootPath = "C:\\root", MaxFileSizeInBytes = 1000 };
        var entry = MakeEntry(1001);

        var issues = _rule.Evaluate(entry, options).ToList();

        var issue = Assert.Single(issues);
        Assert.Equal(AuditIssueType.FileTooLarge, issue.Type);
    }

    [Fact]
    public void Evaluate_FileWithinLimit_ReturnsNoIssue()
    {
        var options = new ScanOptions { RootPath = "C:\\root", MaxFileSizeInBytes = 1000 };
        var entry = MakeEntry(1000);

        var issues = _rule.Evaluate(entry, options).ToList();

        Assert.Empty(issues);
    }

    private static ScanEntry MakeEntry(long size) => new()
    {
        FullPath = "file.bin",
        RelativePath = "file.bin",
        Name = "file.bin",
        Kind = ScanEntryKind.File,
        SizeInBytes = size,
        Depth = 1,
        LastModifiedUtc = DateTime.UtcNow,
    };
}

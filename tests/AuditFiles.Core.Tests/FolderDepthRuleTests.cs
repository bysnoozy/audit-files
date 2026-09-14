using AuditFiles.Core.Models;
using AuditFiles.Core.Rules;
using Xunit;

namespace AuditFiles.Core.Tests;

public class FolderDepthRuleTests
{
    private readonly FolderDepthRule _rule = new();

    [Fact]
    public void Evaluate_FolderBeyondMaxDepth_ReturnsIssue()
    {
        var options = new ScanOptions { RootPath = "C:\\root", MaxFolderDepth = 5 };
        var entry = MakeEntry(6);

        var issues = _rule.Evaluate(entry, options).ToList();

        var issue = Assert.Single(issues);
        Assert.Equal(AuditIssueType.FolderTooDeep, issue.Type);
    }

    [Fact]
    public void Evaluate_FolderWithinMaxDepth_ReturnsNoIssue()
    {
        var options = new ScanOptions { RootPath = "C:\\root", MaxFolderDepth = 5 };
        var entry = MakeEntry(5);

        var issues = _rule.Evaluate(entry, options).ToList();

        Assert.Empty(issues);
    }

    private static ScanEntry MakeEntry(int depth) => new()
    {
        FullPath = "sub",
        RelativePath = "sub",
        Name = "sub",
        Kind = ScanEntryKind.Folder,
        SizeInBytes = 0,
        Depth = depth,
        LastModifiedUtc = DateTime.UtcNow,
    };
}

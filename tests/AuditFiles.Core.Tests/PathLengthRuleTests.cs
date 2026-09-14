using AuditFiles.Core.Models;
using AuditFiles.Core.Rules;
using Xunit;

namespace AuditFiles.Core.Tests;

public class PathLengthRuleTests
{
    private readonly PathLengthRule _rule = new();

    [Fact]
    public void Evaluate_PathWithinBudget_ReturnsNoIssue()
    {
        var options = new ScanOptions { RootPath = "C:\\root", MaxFullUrlLength = 400, ReservedUrlPrefixLength = 100 };
        var entry = MakeEntry(new string('a', 50));

        var issues = _rule.Evaluate(entry, options).ToList();

        Assert.Empty(issues);
    }

    [Fact]
    public void Evaluate_PathExceedsBudget_ReturnsIssue()
    {
        var options = new ScanOptions { RootPath = "C:\\root", MaxFullUrlLength = 400, ReservedUrlPrefixLength = 100 };
        var entry = MakeEntry(new string('a', 350));

        var issues = _rule.Evaluate(entry, options).ToList();

        var issue = Assert.Single(issues);
        Assert.Equal(AuditIssueType.PathTooLong, issue.Type);
    }

    private static ScanEntry MakeEntry(string relativePath) => new()
    {
        FullPath = relativePath,
        RelativePath = relativePath,
        Name = relativePath,
        Kind = ScanEntryKind.File,
        SizeInBytes = 0,
        Depth = 1,
        LastModifiedUtc = DateTime.UtcNow,
    };
}

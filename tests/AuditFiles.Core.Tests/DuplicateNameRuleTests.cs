using AuditFiles.Core.Models;
using AuditFiles.Core.Rules;
using Xunit;

namespace AuditFiles.Core.Tests;

public class DuplicateNameRuleTests
{
    [Fact]
    public void Evaluate_NamesDifferingOnlyByCase_ReturnsIssue()
    {
        var siblings = new[]
        {
            MakeEntry("Report.docx"),
            MakeEntry("report.docx"),
        };

        var issues = DuplicateNameRule.Evaluate(siblings).ToList();

        var issue = Assert.Single(issues);
        Assert.Equal(AuditIssueType.DuplicateNameDifferingByCase, issue.Type);
        Assert.Equal("report.docx", issue.RelativePath);
    }

    [Fact]
    public void Evaluate_DistinctNames_ReturnsNoIssue()
    {
        var siblings = new[]
        {
            MakeEntry("Report.docx"),
            MakeEntry("Summary.docx"),
        };

        var issues = DuplicateNameRule.Evaluate(siblings).ToList();

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

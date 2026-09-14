using AuditFiles.Core.Models;
using AuditFiles.Core.Rules;
using Xunit;

namespace AuditFiles.Core.Tests;

public class InvalidNameRuleTests
{
    private readonly InvalidNameRule _rule = new();
    private readonly ScanOptions _options = new() { RootPath = "C:\\root", MaxNameLength = 400 };

    [Fact]
    public void Evaluate_NameEndsWithSpace_ReturnsIssue()
    {
        var entry = MakeEntry("Draft ");

        var issues = _rule.Evaluate(entry, _options).ToList();

        Assert.Contains(issues, i => i.Type == AuditIssueType.NameStartsOrEndsWithSpace);
    }

    [Fact]
    public void Evaluate_NameEndsWithPeriod_ReturnsIssue()
    {
        var entry = MakeEntry("Version 1.");

        var issues = _rule.Evaluate(entry, _options).ToList();

        Assert.Contains(issues, i => i.Type == AuditIssueType.NameEndsWithPeriod);
    }

    [Theory]
    [InlineData("CON")]
    [InlineData("con.txt")]
    [InlineData("LPT1")]
    [InlineData("~$budget.xlsx")]
    public void Evaluate_ReservedName_ReturnsIssue(string name)
    {
        var entry = MakeEntry(name);

        var issues = _rule.Evaluate(entry, _options).ToList();

        Assert.Contains(issues, i => i.Type == AuditIssueType.ReservedName);
    }

    [Fact]
    public void Evaluate_NameTooLong_ReturnsIssue()
    {
        var entry = MakeEntry(new string('a', 401));

        var issues = _rule.Evaluate(entry, _options).ToList();

        Assert.Contains(issues, i => i.Type == AuditIssueType.NameTooLong);
    }

    [Fact]
    public void Evaluate_NormalName_ReturnsNoIssue()
    {
        var entry = MakeEntry("Budget 2024.xlsx");

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

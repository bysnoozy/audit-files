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

    [Fact]
    public void Evaluate_ConsecutivePeriodsInName_ReturnsIssue()
    {
        var entry = MakeEntry("Version..finale.docx");

        var issues = _rule.Evaluate(entry, _options).ToList();

        Assert.Contains(issues, i => i.Type == AuditIssueType.ConsecutivePeriodsInName);
    }

    [Theory]
    [InlineData("CON")]
    [InlineData("con.txt")]
    [InlineData("LPT1")]
    [InlineData("~$budget.xlsx")]
    [InlineData(".lock")]
    public void Evaluate_ReservedName_ReturnsIssue(string name)
    {
        var entry = MakeEntry(name);

        var issues = _rule.Evaluate(entry, _options).ToList();

        Assert.Contains(issues, i => i.Type == AuditIssueType.ReservedName);
    }

    [Fact]
    public void Evaluate_FolderStartingWithTilde_ReturnsReservedNameIssue()
    {
        var entry = MakeEntry("~backup", ScanEntryKind.Folder);

        var issues = _rule.Evaluate(entry, _options).ToList();

        Assert.Contains(issues, i => i.Type == AuditIssueType.ReservedName);
    }

    [Fact]
    public void Evaluate_FileStartingWithTilde_WithoutDollarSign_ReturnsNoReservedNameIssue()
    {
        // The "starts with ~" rule is folder-specific; a file merely starting with "~" (not "~$")
        // is not itself a reserved Office lock-file name.
        var entry = MakeEntry("~backup.zip", ScanEntryKind.File);

        var issues = _rule.Evaluate(entry, _options).ToList();

        Assert.DoesNotContain(issues, i => i.Type == AuditIssueType.ReservedName);
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

    private static ScanEntry MakeEntry(string name, ScanEntryKind kind = ScanEntryKind.File) => new()
    {
        FullPath = name,
        RelativePath = name,
        Name = name,
        Kind = kind,
        SizeInBytes = 0,
        Depth = 1,
        LastModifiedUtc = DateTime.UtcNow,
    };
}

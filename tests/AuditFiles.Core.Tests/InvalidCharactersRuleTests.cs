using AuditFiles.Core.Models;
using AuditFiles.Core.Rules;
using Xunit;

namespace AuditFiles.Core.Tests;

public class InvalidCharactersRuleTests
{
    private readonly InvalidCharactersRule _rule = new();
    private readonly ScanOptions _options = new() { RootPath = "C:\\root" };

    [Theory]
    [InlineData("plan:v2.pptx")]
    [InlineData("quote\"final.docx")]
    [InlineData("wildcard*.xlsx")]
    [InlineData("path/like.txt")]
    [InlineData("back\\slash.txt")]
    [InlineData("pipe|delimited.csv")]
    [InlineData("less<than.txt")]
    [InlineData("greater>than.txt")]
    [InlineData("question?.txt")]
    [InlineData("curly{brace}.txt")]
    public void Evaluate_NameWithInvalidCharacter_ReturnsIssue(string name)
    {
        var entry = MakeEntry(name);

        var issues = _rule.Evaluate(entry, _options).ToList();

        var issue = Assert.Single(issues);
        Assert.Equal(AuditIssueType.InvalidCharacterInName, issue.Type);
    }

    [Theory]
    [InlineData("report#2024.docx")]
    [InlineData("100% final.xlsx")]
    [InlineData("Annual Report 2024.docx")]
    public void Evaluate_ValidName_ReturnsNoIssue(string name)
    {
        // '#' and '%' are supported by default in OneDrive/SharePoint in Microsoft 365 (only
        // blocked if a tenant admin explicitly disables special-character support), so they must
        // not be flagged.
        var entry = MakeEntry(name);

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

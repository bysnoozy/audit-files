using AuditFiles.Core.Models;
using AuditFiles.Core.Rules;
using Xunit;

namespace AuditFiles.Core.Tests;

public class InvalidCharactersRuleTests
{
    private readonly InvalidCharactersRule _rule = new();
    private readonly ScanOptions _options = new() { RootPath = "C:\\root" };

    [Theory]
    [InlineData("report#2024.docx")]
    [InlineData("100% final.xlsx")]
    [InlineData("plan:v2.pptx")]
    public void Evaluate_NameWithInvalidCharacter_ReturnsIssue(string name)
    {
        var entry = MakeEntry(name);

        var issues = _rule.Evaluate(entry, _options).ToList();

        var issue = Assert.Single(issues);
        Assert.Equal(AuditIssueType.InvalidCharacterInName, issue.Type);
    }

    [Fact]
    public void Evaluate_ValidName_ReturnsNoIssue()
    {
        var entry = MakeEntry("Annual Report 2024.docx");

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

using AuditFiles.Core.Models;
using AuditFiles.Core.Scanning;
using Xunit;

namespace AuditFiles.Core.Tests;

public class FileSystemScannerTests : IDisposable
{
    private readonly string _tempRoot;

    public FileSystemScannerTests()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "AuditFilesTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempRoot);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
        {
            Directory.Delete(_tempRoot, recursive: true);
        }
    }

    [Fact]
    public void Scan_CountsFilesAndFolders()
    {
        Directory.CreateDirectory(Path.Combine(_tempRoot, "SubFolder"));
        File.WriteAllText(Path.Combine(_tempRoot, "a.txt"), "hello");
        File.WriteAllText(Path.Combine(_tempRoot, "SubFolder", "b.txt"), "world");

        var scanner = new FileSystemScanner();
        var options = new ScanOptions { RootPath = _tempRoot };

        var result = scanner.Scan(options);

        Assert.Equal(2, result.Volumetry.TotalFiles);
        Assert.Equal(1, result.Volumetry.TotalFolders);
        Assert.Equal(10, result.Volumetry.TotalSizeInBytes);
    }

    [Fact]
    public void Scan_FlagsBlockedFileTypeAndInvalidCharacters()
    {
        File.WriteAllText(Path.Combine(_tempRoot, "setup.exe"), "binary");
        File.WriteAllText(Path.Combine(_tempRoot, "100%done.txt"), "content");

        var scanner = new FileSystemScanner();
        var options = new ScanOptions { RootPath = _tempRoot };

        var result = scanner.Scan(options);

        Assert.Contains(result.Issues, i => i.Type == AuditIssueType.BlockedFileType && i.RelativePath == "setup.exe");
        Assert.Contains(result.Issues, i => i.Type == AuditIssueType.InvalidCharacterInName && i.RelativePath == "100%done.txt");
    }

    [Fact]
    public void Scan_ThrowsWhenRootDoesNotExist()
    {
        var scanner = new FileSystemScanner();
        var options = new ScanOptions { RootPath = Path.Combine(_tempRoot, "does-not-exist") };

        Assert.Throws<DirectoryNotFoundException>(() => scanner.Scan(options));
    }

    [Fact]
    public void Scan_TracksLargestFilesAndExtensionBreakdown()
    {
        File.WriteAllText(Path.Combine(_tempRoot, "small.txt"), new string('a', 10));
        File.WriteAllText(Path.Combine(_tempRoot, "big.txt"), new string('a', 100));

        var scanner = new FileSystemScanner();
        var options = new ScanOptions { RootPath = _tempRoot };

        var result = scanner.Scan(options);

        Assert.Equal("big.txt", result.Volumetry.LargestFiles.First().RelativePath);

        var txtStats = result.Volumetry.SizeByExtension.Single(e => e.Extension == ".txt");
        Assert.Equal(2, txtStats.FileCount);
        Assert.Equal(110, txtStats.TotalSizeInBytes);
    }
}

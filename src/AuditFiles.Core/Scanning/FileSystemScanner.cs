using System.Diagnostics;
using AuditFiles.Core.Models;
using AuditFiles.Core.Rules;

namespace AuditFiles.Core.Scanning;

/// <summary>
/// Walks a directory tree, evaluates every audit rule against each file and folder found, and
/// aggregates volumetry statistics (file/folder counts, total size, size by extension, largest
/// files) along the way.
/// </summary>
public sealed class FileSystemScanner
{
    private const int LargestFilesTracked = 25;
    private const int ProgressReportIntervalMs = 250;

    private readonly IReadOnlyList<IAuditRule> _rules;

    public FileSystemScanner(IReadOnlyList<IAuditRule>? rules = null)
    {
        _rules = rules ?? new IAuditRule[]
        {
            new PathLengthRule(),
            new InvalidCharactersRule(),
            new InvalidNameRule(),
            new BlockedFileTypeRule(),
            new FileSizeRule(),
            new FolderDepthRule(),
        };
    }

    public ScanResult Scan(ScanOptions options, IProgress<ScanProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        var root = new DirectoryInfo(options.RootPath);
        if (!root.Exists)
        {
            throw new DirectoryNotFoundException($"Root path not found: {options.RootPath}");
        }

        var startedUtc = DateTime.UtcNow;
        var issues = new List<AuditIssue>();
        var errors = new List<string>();
        var extensionStats = new Dictionary<string, (long Count, long Size)>(StringComparer.OrdinalIgnoreCase);
        var largestFiles = new List<ScanEntry>(LargestFilesTracked + 1);

        long totalFiles = 0;
        long totalFolders = 0;
        long totalSize = 0;
        var maxDepth = 0;
        DateTime? oldest = null;
        DateTime? newest = null;

        var stopwatch = Stopwatch.StartNew();
        long lastReportMs = 0;

        var stack = new Stack<(DirectoryInfo Directory, string RelativePath, int Depth)>();
        stack.Push((root, string.Empty, 0));

        while (stack.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var (directory, relativePath, depth) = stack.Pop();

            FileSystemInfo[] children;
            try
            {
                children = directory.GetFileSystemInfos();
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
            {
                errors.Add($"Impossible de lire '{directory.FullName}' : {ex.Message}");
                continue;
            }

            var childEntries = new List<ScanEntry>(children.Length);

            foreach (var child in children)
            {
                var childRelativePath = relativePath.Length == 0 ? child.Name : $"{relativePath}/{child.Name}";
                var isDirectory = child is DirectoryInfo;

                var entry = new ScanEntry
                {
                    FullPath = child.FullName,
                    RelativePath = childRelativePath,
                    Name = child.Name,
                    Kind = isDirectory ? ScanEntryKind.Folder : ScanEntryKind.File,
                    SizeInBytes = child is FileInfo file ? file.Length : 0,
                    Depth = depth + 1,
                    LastModifiedUtc = child.LastWriteTimeUtc,
                    ParentRelativePath = relativePath.Length == 0 ? null : relativePath,
                };

                childEntries.Add(entry);

                foreach (var rule in _rules)
                {
                    issues.AddRange(rule.Evaluate(entry, options));
                }

                if (entry.Kind == ScanEntryKind.Folder)
                {
                    totalFolders++;
                    maxDepth = Math.Max(maxDepth, entry.Depth);
                    stack.Push(((DirectoryInfo)child, entry.RelativePath, entry.Depth));
                }
                else
                {
                    totalFiles++;
                    totalSize += entry.SizeInBytes;

                    var rawExtension = Path.GetExtension(entry.Name);
                    var extension = string.IsNullOrEmpty(rawExtension) ? "(sans extension)" : rawExtension.ToLowerInvariant();

                    extensionStats.TryGetValue(extension, out var stats);
                    extensionStats[extension] = (stats.Count + 1, stats.Size + entry.SizeInBytes);

                    if (oldest is null || entry.LastModifiedUtc < oldest)
                    {
                        oldest = entry.LastModifiedUtc;
                    }

                    if (newest is null || entry.LastModifiedUtc > newest)
                    {
                        newest = entry.LastModifiedUtc;
                    }

                    InsertLargestFile(largestFiles, entry);
                }

                if (stopwatch.ElapsedMilliseconds - lastReportMs >= ProgressReportIntervalMs)
                {
                    lastReportMs = stopwatch.ElapsedMilliseconds;
                    progress?.Report(new ScanProgress
                    {
                        FilesScanned = totalFiles,
                        FoldersScanned = totalFolders,
                        CurrentPath = entry.RelativePath,
                    });
                }
            }

            issues.AddRange(DuplicateNameRule.Evaluate(childEntries));
        }

        progress?.Report(new ScanProgress
        {
            FilesScanned = totalFiles,
            FoldersScanned = totalFolders,
            CurrentPath = null,
        });

        var volumetry = new VolumetrySummary
        {
            TotalFiles = totalFiles,
            TotalFolders = totalFolders,
            TotalSizeInBytes = totalSize,
            MaxDepthEncountered = maxDepth,
            OldestFileModifiedUtc = oldest,
            NewestFileModifiedUtc = newest,
            SizeByExtension = extensionStats
                .Select(kvp => new ExtensionVolumetry(kvp.Key, kvp.Value.Count, kvp.Value.Size))
                .OrderByDescending(e => e.TotalSizeInBytes)
                .ToList(),
            LargestFiles = largestFiles,
        };

        return new ScanResult
        {
            RootPath = options.RootPath,
            Issues = issues,
            Volumetry = volumetry,
            ScanStartedUtc = startedUtc,
            ScanCompletedUtc = DateTime.UtcNow,
            Errors = errors,
        };
    }

    private static void InsertLargestFile(List<ScanEntry> largestFiles, ScanEntry entry)
    {
        if (largestFiles.Count < LargestFilesTracked)
        {
            largestFiles.Add(entry);
            largestFiles.Sort((a, b) => b.SizeInBytes.CompareTo(a.SizeInBytes));
            return;
        }

        if (entry.SizeInBytes <= largestFiles[^1].SizeInBytes)
        {
            return;
        }

        largestFiles[^1] = entry;
        largestFiles.Sort((a, b) => b.SizeInBytes.CompareTo(a.SizeInBytes));
    }
}

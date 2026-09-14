using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AuditFiles.Core.Models;
using AuditFiles.Core.Scanning;

namespace AuditFiles.App.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly FileSystemScanner _scanner = new();
    private CancellationTokenSource? _cancellationTokenSource;

    private string _rootPath = string.Empty;
    private string _statusText = "Sélectionnez un dossier à auditer.";
    private string _progressText = string.Empty;
    private bool _isScanning;
    private string _totalFiles = "-";
    private string _totalFolders = "-";
    private string _totalSize = "-";
    private string _issueCount = "-";
    private ScanResult? _lastResult;

    public MainViewModel()
    {
        StartScanCommand = new RelayCommand(async _ => await StartScanAsync(), _ => !IsScanning && !string.IsNullOrWhiteSpace(RootPath));
        CancelScanCommand = new RelayCommand(_ => Cancel(), _ => IsScanning);
    }

    public ObservableCollection<AuditIssue> Issues { get; } = new();

    public RelayCommand StartScanCommand { get; }

    public RelayCommand CancelScanCommand { get; }

    public string RootPath
    {
        get => _rootPath;
        set
        {
            if (SetField(ref _rootPath, value))
            {
                StartScanCommand.RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Settable from the view's code-behind after an export dialog completes.
    /// </summary>
    public string StatusText
    {
        get => _statusText;
        internal set => SetField(ref _statusText, value);
    }

    public string ProgressText
    {
        get => _progressText;
        private set => SetField(ref _progressText, value);
    }

    public bool IsScanning
    {
        get => _isScanning;
        private set
        {
            if (SetField(ref _isScanning, value))
            {
                StartScanCommand.RaiseCanExecuteChanged();
                CancelScanCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string TotalFiles
    {
        get => _totalFiles;
        private set => SetField(ref _totalFiles, value);
    }

    public string TotalFolders
    {
        get => _totalFolders;
        private set => SetField(ref _totalFolders, value);
    }

    public string TotalSize
    {
        get => _totalSize;
        private set => SetField(ref _totalSize, value);
    }

    public string IssueCount
    {
        get => _issueCount;
        private set => SetField(ref _issueCount, value);
    }

    /// <summary>
    /// Read by the view's code-behind to feed the CSV/HTML export dialogs.
    /// </summary>
    public ScanResult? LastResult
    {
        get => _lastResult;
        private set
        {
            if (SetField(ref _lastResult, value))
            {
                OnPropertyChanged(nameof(CanExport));
            }
        }
    }

    public bool CanExport => LastResult is not null;

    private async Task StartScanAsync()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        IsScanning = true;
        StatusText = "Analyse en cours...";
        Issues.Clear();
        LastResult = null;

        var options = new ScanOptions { RootPath = RootPath };
        var progress = new Progress<ScanProgress>(p =>
        {
            ProgressText = $"{p.FilesScanned:N0} fichiers, {p.FoldersScanned:N0} dossiers analysés";
        });

        try
        {
            var result = await Task.Run(
                () => _scanner.Scan(options, progress, _cancellationTokenSource.Token),
                _cancellationTokenSource.Token);

            foreach (var issue in result.Issues)
            {
                Issues.Add(issue);
            }

            TotalFiles = result.Volumetry.TotalFiles.ToString("N0");
            TotalFolders = result.Volumetry.TotalFolders.ToString("N0");
            TotalSize = FormatBytes(result.Volumetry.TotalSizeInBytes);
            IssueCount = result.Issues.Count.ToString("N0");

            StatusText = result.Errors.Count > 0
                ? $"Analyse terminée avec {result.Errors.Count} erreur(s) de lecture."
                : "Analyse terminée.";

            LastResult = result;
        }
        catch (OperationCanceledException)
        {
            StatusText = "Analyse annulée.";
        }
        catch (Exception ex)
        {
            StatusText = $"Erreur : {ex.Message}";
        }
        finally
        {
            IsScanning = false;
            _cancellationTokenSource = null;
        }
    }

    private void Cancel()
    {
        _cancellationTokenSource?.Cancel();
    }

    private static string FormatBytes(long bytes)
    {
        string[] units = { "o", "Ko", "Mo", "Go", "To" };
        double size = bytes;
        var unitIndex = 0;
        while (size >= 1024 && unitIndex < units.Length - 1)
        {
            size /= 1024;
            unitIndex++;
        }

        return $"{size:F2} {units[unitIndex]}";
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName!);
        return true;
    }
}

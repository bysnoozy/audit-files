using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AuditFiles.Core.Models;
using AuditFiles.Core.Reporting;
using AuditFiles.Core.Scanning;
using Microsoft.Win32;

namespace AuditFiles.App.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly FileSystemScanner _scanner = new();
    private CancellationTokenSource? _cancellationTokenSource;
    private ScanResult? _lastResult;

    private string _rootPath = string.Empty;
    private string _statusText = "Sélectionnez un dossier à auditer.";
    private string _progressText = string.Empty;
    private bool _isScanning;
    private string _totalFiles = "-";
    private string _totalFolders = "-";
    private string _totalSize = "-";
    private string _issueCount = "-";

    public MainViewModel()
    {
        BrowseCommand = new RelayCommand(_ => Browse());
        StartScanCommand = new RelayCommand(async _ => await StartScanAsync(), _ => !IsScanning && !string.IsNullOrWhiteSpace(RootPath));
        CancelScanCommand = new RelayCommand(_ => Cancel(), _ => IsScanning);
        ExportCsvCommand = new RelayCommand(_ => ExportCsv(), _ => _lastResult is not null);
        ExportHtmlCommand = new RelayCommand(_ => ExportHtml(), _ => _lastResult is not null);
    }

    public ObservableCollection<AuditIssue> Issues { get; } = new();

    public RelayCommand BrowseCommand { get; }

    public RelayCommand StartScanCommand { get; }

    public RelayCommand CancelScanCommand { get; }

    public RelayCommand ExportCsvCommand { get; }

    public RelayCommand ExportHtmlCommand { get; }

    public string RootPath
    {
        get => _rootPath;
        set => SetField(ref _rootPath, value);
    }

    public string StatusText
    {
        get => _statusText;
        private set => SetField(ref _statusText, value);
    }

    public string ProgressText
    {
        get => _progressText;
        private set => SetField(ref _progressText, value);
    }

    public bool IsScanning
    {
        get => _isScanning;
        private set => SetField(ref _isScanning, value);
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

    private void Browse()
    {
        var dialog = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = "Sélectionnez le dossier racine à auditer",
            UseDescriptionForTitle = true,
        };

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            RootPath = dialog.SelectedPath;
        }
    }

    private async Task StartScanAsync()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        IsScanning = true;
        StatusText = "Analyse en cours...";
        Issues.Clear();
        _lastResult = null;

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

            _lastResult = result;

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

    private void ExportCsv()
    {
        if (_lastResult is null)
        {
            return;
        }

        var dialog = new SaveFileDialog
        {
            Filter = "Fichier CSV (*.csv)|*.csv",
            FileName = "audit-anomalies.csv",
        };

        if (dialog.ShowDialog() == true)
        {
            CsvReportWriter.WriteIssues(_lastResult, dialog.FileName);
            StatusText = $"Export CSV enregistré : {dialog.FileName}";
        }
    }

    private void ExportHtml()
    {
        if (_lastResult is null)
        {
            return;
        }

        var dialog = new SaveFileDialog
        {
            Filter = "Rapport HTML (*.html)|*.html",
            FileName = "rapport-audit.html",
        };

        if (dialog.ShowDialog() == true)
        {
            HtmlReportWriter.WriteReport(_lastResult, dialog.FileName);
            StatusText = $"Rapport HTML enregistré : {dialog.FileName}";
        }
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

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

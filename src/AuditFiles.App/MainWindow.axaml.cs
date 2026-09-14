using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using AuditFiles.App.ViewModels;
using AuditFiles.Core.Reporting;

namespace AuditFiles.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private MainViewModel ViewModel => (MainViewModel)DataContext!;

    private async void Browse_Click(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null)
        {
            return;
        }

        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Sélectionnez le dossier racine à auditer",
            AllowMultiple = false,
        });

        if (folders.Count > 0)
        {
            ViewModel.RootPath = folders[0].Path.LocalPath;
        }
    }

    private async void ExportCsv_Click(object? sender, RoutedEventArgs e)
    {
        var result = ViewModel.LastResult;
        if (result is null)
        {
            return;
        }

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null)
        {
            return;
        }

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Exporter les anomalies en CSV",
            SuggestedFileName = "audit-anomalies.csv",
            FileTypeChoices = new[] { new FilePickerFileType("Fichier CSV") { Patterns = new[] { "*.csv" } } },
        });

        if (file is not null)
        {
            CsvReportWriter.WriteIssues(result, file.Path.LocalPath);
            ViewModel.StatusText = $"Export CSV enregistré : {file.Path.LocalPath}";
        }
    }

    private async void ExportHtml_Click(object? sender, RoutedEventArgs e)
    {
        var result = ViewModel.LastResult;
        if (result is null)
        {
            return;
        }

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null)
        {
            return;
        }

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Exporter le rapport HTML",
            SuggestedFileName = "rapport-audit.html",
            FileTypeChoices = new[] { new FilePickerFileType("Rapport HTML") { Patterns = new[] { "*.html" } } },
        });

        if (file is not null)
        {
            HtmlReportWriter.WriteReport(result, file.Path.LocalPath);
            ViewModel.StatusText = $"Rapport HTML enregistré : {file.Path.LocalPath}";
        }
    }

    private async void ExportPdf_Click(object? sender, RoutedEventArgs e)
    {
        var result = ViewModel.LastResult;
        if (result is null)
        {
            return;
        }

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null)
        {
            return;
        }

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Exporter le rapport PDF client",
            SuggestedFileName = "rapport-audit-client.pdf",
            FileTypeChoices = new[] { new FilePickerFileType("Rapport PDF") { Patterns = new[] { "*.pdf" } } },
        });

        if (file is null)
        {
            return;
        }

        try
        {
            PdfReportWriter.WriteClientReport(result, file.Path.LocalPath);
            ViewModel.StatusText = $"Rapport PDF client enregistré : {file.Path.LocalPath}";
        }
        catch (InvalidOperationException ex)
        {
            ViewModel.StatusText = $"Impossible de générer le PDF : {ex.Message}";
        }
    }
}

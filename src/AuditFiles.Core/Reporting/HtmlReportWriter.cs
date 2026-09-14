using System.Text;
using AuditFiles.Core.Models;

namespace AuditFiles.Core.Reporting;

public static class HtmlReportWriter
{
    public static void WriteReport(ScanResult result, string filePath)
    {
        File.WriteAllText(filePath, BuildHtml(result), Encoding.UTF8);
    }

    public static string BuildHtml(ScanResult result)
    {
        var sb = new StringBuilder();
        var duration = result.ScanCompletedUtc - result.ScanStartedUtc;
        var issuesByType = result.Issues
            .GroupBy(i => i.Type)
            .OrderByDescending(g => g.Count())
            .ToList();

        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"fr\">");
        sb.AppendLine("<head>");
        sb.AppendLine("<meta charset=\"utf-8\">");
        sb.AppendLine("<title>Rapport d'audit de migration SharePoint</title>");
        sb.AppendLine("<style>");
        sb.AppendLine(CssStyles);
        sb.AppendLine("</style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("<h1>Rapport d'audit de migration SharePoint</h1>");
        sb.AppendLine($"<p class=\"meta\">Racine analysée : <code>{Encode(result.RootPath)}</code><br>");
        sb.AppendLine($"Analyse terminée le {result.ScanCompletedUtc:yyyy-MM-dd HH:mm} UTC " +
            $"(durée : {duration.Hours:00}h{duration.Minutes:00}m{duration.Seconds:00}s)</p>");

        sb.AppendLine("<h2>Volumétrie</h2>");
        sb.AppendLine("<div class=\"cards\">");
        AppendCard(sb, "Fichiers", result.Volumetry.TotalFiles.ToString("N0"));
        AppendCard(sb, "Dossiers", result.Volumetry.TotalFolders.ToString("N0"));
        AppendCard(sb, "Taille totale", FormatBytes(result.Volumetry.TotalSizeInBytes));
        AppendCard(sb, "Profondeur max.", result.Volumetry.MaxDepthEncountered.ToString());
        AppendCard(sb, "Anomalies détectées", result.Issues.Count.ToString("N0"));
        sb.AppendLine("</div>");

        sb.AppendLine("<h2>Répartition par type de fichier</h2>");
        sb.AppendLine("<table><thead><tr><th>Extension</th><th>Fichiers</th><th>Taille</th></tr></thead><tbody>");
        foreach (var ext in result.Volumetry.SizeByExtension.Take(20))
        {
            sb.AppendLine($"<tr><td>{Encode(ext.Extension)}</td><td>{ext.FileCount:N0}</td><td>{FormatBytes(ext.TotalSizeInBytes)}</td></tr>");
        }

        sb.AppendLine("</tbody></table>");

        sb.AppendLine("<h2>Fichiers les plus volumineux</h2>");
        sb.AppendLine("<table><thead><tr><th>Chemin</th><th>Taille</th></tr></thead><tbody>");
        foreach (var file in result.Volumetry.LargestFiles)
        {
            sb.AppendLine($"<tr><td>{Encode(file.RelativePath)}</td><td>{FormatBytes(file.SizeInBytes)}</td></tr>");
        }

        sb.AppendLine("</tbody></table>");

        sb.AppendLine("<h2>Anomalies par type</h2>");
        sb.AppendLine("<table><thead><tr><th>Type</th><th>Occurrences</th></tr></thead><tbody>");
        foreach (var group in issuesByType)
        {
            sb.AppendLine($"<tr><td>{Encode(group.Key.ToString())}</td><td>{group.Count():N0}</td></tr>");
        }

        sb.AppendLine("</tbody></table>");

        sb.AppendLine("<h2>Détail des anomalies</h2>");
        sb.AppendLine("<table><thead><tr><th>Sévérité</th><th>Type</th><th>Chemin</th><th>Description</th></tr></thead><tbody>");
        foreach (var issue in result.Issues.OrderBy(i => i.RelativePath, StringComparer.OrdinalIgnoreCase))
        {
            var severityClass = issue.Severity == AuditSeverity.Blocking ? "blocking" : "warning";
            sb.AppendLine($"<tr class=\"{severityClass}\"><td>{Encode(issue.Severity.ToString())}</td>" +
                $"<td>{Encode(issue.Type.ToString())}</td><td>{Encode(issue.RelativePath)}</td>" +
                $"<td>{Encode(issue.Description)}</td></tr>");
        }

        sb.AppendLine("</tbody></table>");

        if (result.Errors.Count > 0)
        {
            sb.AppendLine("<h2>Erreurs de lecture</h2>");
            sb.AppendLine("<ul>");
            foreach (var error in result.Errors)
            {
                sb.AppendLine($"<li>{Encode(error)}</li>");
            }

            sb.AppendLine("</ul>");
        }

        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    private static void AppendCard(StringBuilder sb, string label, string value)
    {
        sb.AppendLine("<div class=\"card\">");
        sb.AppendLine($"<div class=\"card-value\">{Encode(value)}</div>");
        sb.AppendLine($"<div class=\"card-label\">{Encode(label)}</div>");
        sb.AppendLine("</div>");
    }

    private static string Encode(string value) => System.Net.WebUtility.HtmlEncode(value);

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

    private const string CssStyles = """
        body { font-family: Segoe UI, Arial, sans-serif; margin: 2rem; color: #1b1b1f; }
        h1 { margin-bottom: 0.25rem; }
        .meta { color: #555; margin-top: 0; }
        .cards { display: flex; flex-wrap: wrap; gap: 1rem; margin: 1rem 0 2rem; }
        .card { background: #f3f2f1; border-radius: 8px; padding: 1rem 1.5rem; min-width: 140px; }
        .card-value { font-size: 1.6rem; font-weight: 600; }
        .card-label { color: #555; font-size: 0.85rem; }
        table { border-collapse: collapse; width: 100%; margin-bottom: 2rem; }
        th, td { border: 1px solid #ddd; padding: 0.4rem 0.6rem; text-align: left; font-size: 0.9rem; }
        th { background: #f3f2f1; }
        tr.blocking td:first-child { color: #a80000; font-weight: 600; }
        tr.warning td:first-child { color: #8a6d00; font-weight: 600; }
        """;
}

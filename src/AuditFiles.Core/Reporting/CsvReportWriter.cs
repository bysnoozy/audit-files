using System.Text;
using AuditFiles.Core.Models;

namespace AuditFiles.Core.Reporting;

public static class CsvReportWriter
{
    public static void WriteIssues(ScanResult result, string filePath)
    {
        using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
        writer.WriteLine("Severite,TypeAnomalie,Chemin,Description");

        foreach (var issue in result.Issues.OrderBy(i => i.RelativePath, StringComparer.OrdinalIgnoreCase))
        {
            writer.WriteLine(string.Join(',',
                Escape(issue.Severity.ToString()),
                Escape(issue.Type.ToString()),
                Escape(issue.RelativePath),
                Escape(issue.Description)));
        }
    }

    private static string Escape(string value)
    {
        if (value.IndexOfAny(new[] { ',', '"', '\n', '\r' }) < 0)
        {
            return value;
        }

        return $"\"{value.Replace("\"", "\"\"")}\"";
    }
}

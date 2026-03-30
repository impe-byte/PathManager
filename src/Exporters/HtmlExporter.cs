using System.IO;
using System.Text;
using PathManager.Core;
using PathManager.Localization;

namespace PathManager.Exporters
{
    public class HtmlExporter : IReportExporter
    {
        public void Export(ScanReport report, string outputPath)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang=\"en\">");
            sb.AppendLine("<head>");
            sb.AppendLine("  <meta charset=\"UTF-8\">");
            sb.AppendLine("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            sb.AppendLine(string.Format("  <title>{0} - Report</title>", Strings.Get("Title")));
            sb.AppendLine("  <style>");
            sb.AppendLine("    :root { --primary: #0078D7; --bg: #f5f5f5; --card: #ffffff; --text: #333; --border: #e0e0e0; --danger: #d13438; }");
            sb.AppendLine("    body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background: var(--bg); color: var(--text); padding: 20px; margin: 0; }");
            sb.AppendLine("    .container { max-width: 1200px; margin: auto; }");
            sb.AppendLine("    .header { background: var(--card); padding: 20px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); margin-bottom: 20px; }");
            sb.AppendLine("    .header h1 { margin: 0 0 10px 0; color: var(--primary); }");
            sb.AppendLine("    .stats { display: flex; gap: 20px; flex-wrap: wrap; }");
            sb.AppendLine("    .stat-card { background: var(--bg); padding: 15px; border-radius: 6px; flex: 1; min-width: 150px; border: 1px solid var(--border); }");
            sb.AppendLine("    .stat-val { font-size: 1.5em; font-weight: bold; color: var(--primary); }");
            sb.AppendLine("    table { width: 100%; background: var(--card); border-collapse: collapse; box-shadow: 0 2px 4px rgba(0,0,0,0.1); border-radius: 8px; overflow: hidden; }");
            sb.AppendLine("    th, td { padding: 12px 15px; text-align: left; border-bottom: 1px solid var(--border); word-break: break-all; }");
            sb.AppendLine("    th { background: var(--primary); color: white; cursor: pointer; user-select: none; }");
            sb.AppendLine("    th:hover { background: #005a9e; }");
            sb.AppendLine("    tr:last-child td { border-bottom: none; }");
            sb.AppendLine("    tr:hover { background: #f9f9f9; }");
            sb.AppendLine("    .excess { color: var(--danger); font-weight: bold; width: 10%; }");
            sb.AppendLine("    .total { width: 10%; font-weight: bold; }");
            sb.AppendLine("    .search { margin-bottom: 15px; width: 100%; padding: 10px; border: 1px solid var(--border); border-radius: 4px; font-size: 1em; box-sizing: border-box; }");
            sb.AppendLine("  </style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("  <div class=\"container\">");
            sb.AppendLine("    <div class=\"header\">");
            sb.AppendLine(string.Format("      <h1>{0}</h1>", Strings.Get("Title")));
            sb.AppendLine(string.Format("      <p><strong>{0}</strong> {1}</p>", Strings.Get("StatsRoot"), System.Security.SecurityElement.Escape(report.RootPath)));
            sb.AppendLine("      <div class=\"stats\">");
            sb.AppendLine(string.Format("        <div class=\"stat-card\"><div>{0}</div><div class=\"stat-val\">{1}</div></div>", Strings.Get("StatsFolders"), report.TotalFolders));
            sb.AppendLine(string.Format("        <div class=\"stat-card\"><div>{0}</div><div class=\"stat-val\">{1}</div></div>", Strings.Get("StatsFiles"), report.TotalFiles));
            sb.AppendLine(string.Format("        <div class=\"stat-card\"><div>{0}</div><div class=\"stat-val\">{1}</div></div>", Strings.Get("StatsSize"), Strings.FormatSize(report.TotalSizeBytes)));
            sb.AppendLine(string.Format("        <div class=\"stat-card\"><div>{0}</div><div class=\"stat-val\">{1}</div></div>", Strings.Get("StatsThreshold"), report.ThresholdLimit));
            sb.AppendLine(string.Format("        <div class=\"stat-card\"><div>{0}</div><div class=\"stat-val excess\">{1}</div></div>", Strings.Get("StatsViolations"), report.BadPaths.Count));
            sb.AppendLine("      </div>");
            sb.AppendLine("    </div>");
            
            sb.AppendLine("    <div>");
            sb.AppendLine("      <input type=\"text\" id=\"searchInput\" class=\"search\" placeholder=\"Search paths...\" onkeyup=\"filterTable()\">");
            sb.AppendLine("      <table id=\"reportTable\">");
            sb.AppendLine("        <thead>");
            sb.AppendLine(string.Format("          <tr><th onclick=\"sortTable(0)\">{0} &#9650;&#9660;</th><th onclick=\"sortTable(1)\">{1} &#9650;&#9660;</th><th onclick=\"sortTable(2)\">{2} &#9650;&#9660;</th></tr>", Strings.Get("ColExcess"), Strings.Get("ColTotal"), Strings.Get("ColRelative")));
            sb.AppendLine("        </thead>");
            sb.AppendLine("        <tbody>");

            foreach (var bp in report.BadPaths)
            {
                sb.AppendLine(string.Format("          <tr><td class=\"excess\">+{0}</td><td class=\"total\">{1}</td><td>{2}</td></tr>", bp.ExcessChars, bp.CharCount, System.Security.SecurityElement.Escape(bp.RelativePath)));
            }

            sb.AppendLine("        </tbody>");
            sb.AppendLine("      </table>");
            sb.AppendLine("    </div>");
            sb.AppendLine("  </div>");
            
            sb.AppendLine("  <script>");
            sb.AppendLine("    function filterTable() {");
            sb.AppendLine("      var input = document.getElementById('searchInput'), filter = input.value.toUpperCase();");
            sb.AppendLine("      var tr = document.getElementById('reportTable').getElementsByTagName('tr');");
            sb.AppendLine("      for (var i = 1; i < tr.length; i++) {");
            sb.AppendLine("        var td = tr[i].getElementsByTagName('td')[2];");
            sb.AppendLine("        if (td) {");
            sb.AppendLine("          var txtValue = td.textContent || td.innerText;");
            sb.AppendLine("          tr[i].style.display = txtValue.toUpperCase().indexOf(filter) > -1 ? '' : 'none';");
            sb.AppendLine("        }");
            sb.AppendLine("      }");
            sb.AppendLine("    }");
            sb.AppendLine("    function sortTable(n) {");
            sb.AppendLine("      var table = document.getElementById('reportTable');");
            sb.AppendLine("      var rows = table.rows; var switching = true; var dir = 'asc'; var switchcount = 0;");
            sb.AppendLine("      while (switching) {");
            sb.AppendLine("        switching = false;");
            sb.AppendLine("        var b = table.rows;");
            sb.AppendLine("        for (var i = 1; i < (b.length - 1); i++) {");
            sb.AppendLine("          var shouldSwitch = false;");
            sb.AppendLine("          var x = b[i].getElementsByTagName('td')[n];");
            sb.AppendLine("          var y = b[i + 1].getElementsByTagName('td')[n];");
            sb.AppendLine("          var xContent = n < 2 ? parseInt(x.innerText.replace('+', '')) : x.innerText.toLowerCase();");
            sb.AppendLine("          var yContent = n < 2 ? parseInt(y.innerText.replace('+', '')) : y.innerText.toLowerCase();");
            sb.AppendLine("          if (dir == 'asc') { if (xContent > yContent) { shouldSwitch = true; break; } }");
            sb.AppendLine("          else if (dir == 'desc') { if (xContent < yContent) { shouldSwitch = true; break; } }");
            sb.AppendLine("        }");
            sb.AppendLine("        if (shouldSwitch) {");
            sb.AppendLine("          b[i].parentNode.insertBefore(b[i + 1], b[i]);");
            sb.AppendLine("          switching = true; switchcount++;");
            sb.AppendLine("        } else {");
            sb.AppendLine("          if (switchcount == 0 && dir == 'asc') { dir = 'desc'; switching = true; }");
            sb.AppendLine("        }");
            sb.AppendLine("      }");
            sb.AppendLine("    }");
            sb.AppendLine("  </script>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");
            
            File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
        }
    }
}

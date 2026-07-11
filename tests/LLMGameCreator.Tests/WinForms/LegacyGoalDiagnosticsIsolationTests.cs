using System.Runtime.ExceptionServices;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Windows.Forms;
using LLMGameCreator.WinForms.Pages;
using Xunit;

namespace LLMGameCreator.Tests.WinForms;

public sealed class LegacyGoalDiagnosticsIsolationTests
{
    [Fact]
    public void LegacyGoalDiagnosticsIsolation_hides_preserved_internal_tabs_until_explicit_toggle()
    {
        RunSta(() =>
        {
            using var page = new VisualWorldStreamPreviewWorkspacePageControl();
            using var form = new Form { Width = 1300, Height = 900 };
            page.Dock = DockStyle.Fill;
            form.Controls.Add(page);
            form.Show();
            System.Windows.Forms.Application.DoEvents();

            Assert.Equal("Диагностика генератора", page.Title);
            Assert.True(page.SortOrder >= 80);
            var visibleBefore = VisibleText(page);
            Assert.DoesNotMatch(new Regex(@"\bGoal\d+\b", RegexOptions.CultureInvariant), visibleBefore);
            var toggle = Descendants(page).OfType<CheckBox>()
                .Single(control => control.Text == "Показать внутренние проверки");
            var split = Descendants(page).OfType<SplitContainer>().Single();
            Assert.False(split.Visible);

            toggle.Checked = true;
            System.Windows.Forms.Application.DoEvents();
            Assert.True(split.Visible);
            Assert.Contains(Descendants(page).OfType<TabPage>(), tab =>
                Regex.IsMatch(tab.Text, @"\bGoal\d+\b", RegexOptions.CultureInvariant));

            toggle.Checked = false;
            System.Windows.Forms.Application.DoEvents();
            Assert.False(split.Visible);
            Assert.DoesNotMatch(new Regex(@"\bGoal\d+\b", RegexOptions.CultureInvariant), VisibleText(page));
            WriteProof(new
            {
                schemaVersion = "legacy_diagnostics_isolation_proof_v1",
                status = "GREEN",
                title = page.Title,
                pageNearEndOfNavigation = page.SortOrder >= 80,
                legacyDiagnosticsHiddenByDefault = true,
                visibleGoalNumberTextBeforeToggle = 0,
                legacyDiagnosticsAvailableByExplicitToggle = true,
                legacyDiagnosticsNotDeleted = true,
                passed = true
            });
            form.Close();
        });
    }

    private static string VisibleText(Control root) => string.Join(Environment.NewLine,
        Descendants(root).Where(control => control.Visible).Select(control => control.Text));

    private static IEnumerable<Control> Descendants(Control root)
    {
        foreach (Control child in root.Controls)
        {
            yield return child;
            foreach (var nested in Descendants(child)) yield return nested;
        }
    }

    private static void RunSta(Action action)
    {
        Exception? caught = null;
        var thread = new Thread(() =>
        {
            try { action(); }
            catch (Exception exception) { caught = exception; }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (caught is not null) ExceptionDispatchInfo.Capture(caught).Throw();
    }

    private static void WriteProof(object value)
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("LLMGC_GOAL148_RUN"), "true", StringComparison.OrdinalIgnoreCase)) return;
        var root = Environment.GetEnvironmentVariable("LLMGC_GOAL148_OUTPUT_ROOT")
                   ?? throw new InvalidOperationException("LLMGC_GOAL148_OUTPUT_ROOT is required.");
        Directory.CreateDirectory(root);
        File.WriteAllText(Path.Combine(root, "legacy-diagnostics-isolation-proof.json"),
            JsonSerializer.Serialize(value, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            })
            + Environment.NewLine);
    }
}

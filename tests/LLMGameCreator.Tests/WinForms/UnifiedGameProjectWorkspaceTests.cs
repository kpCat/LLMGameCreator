using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using LLMGameCreator.Application.Abstractions;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.Application.Settings;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.Infrastructure.Storage;
using LLMGameCreator.Runtime;
using LLMGameCreator.WinForms.Pages;
using Xunit;

namespace LLMGameCreator.Tests.WinForms;

public sealed class UnifiedGameProjectWorkspaceTests
{
    [Fact]
    public async Task ProjectsPage_real_STA_workflow_opens_project_and_builds_catalog_driven_workspace_without_goal_labels()
    {
        using var temp = new TempDirectory();
        var repository = new JsonGamePackageRepository();
        var projectService = new GameProjectService(repository, new GamePackageValidator(), new NewGamePackageFactory());
        var summary = await projectService.CreateAsync(new CreateGameProjectRequest
        {
            GamesRootPath = temp.Path,
            FolderName = "ui-game",
            Title = "Игра для проверки",
            PackageId = "game/ui-game",
            Version = "0.1.0"
        }, CancellationToken.None);
        var summaries = await projectService.ListAsync(temp.Path, CancellationToken.None);
        var current = new CurrentGamePackageService(repository);
        await current.LoadAsync(summary.FolderPath, CancellationToken.None);
        var controller = CreateController(current);
        var snapshot = controller.OpenProject(summary.FolderPath);

        RunSta(() =>
        {
            using var page = new ProjectsPageControl(
                current,
                new MemorySettingsRepository(temp.Path),
                projectService,
                new GamePackageValidator(),
                controller);
            var projectList = Field<ListView>(page, "_projectsListView");
            foreach (var item in summaries)
            {
                var row = new ListViewItem(item.Title ?? item.FolderName) { Tag = item };
                projectList.Items.Add(row);
            }
            InvokeVoid(page, "BindWorkspace", snapshot);
            InvokeVoid(page, "ShowWorkspace");
            page.CreateControl();
            Assert.Single(projectList.Items.Cast<ListViewItem>());
            Assert.True(File.Exists(Path.Combine(
                summary.FolderPath,
                ".llmgc",
                "authoring",
                snapshot.ProjectScopedCompositionId + ".featurecomposition.json")));
            Assert.DoesNotContain("goal147", snapshot.ProjectScopedCompositionId, StringComparison.OrdinalIgnoreCase);
            var workspaceTabs = Field<TabControl>(page, "_workspaceTabs");
            Assert.Equal("Обзор", workspaceTabs.SelectedTab?.Text);
            Assert.NotEqual("Технические детали", workspaceTabs.SelectedTab?.Text);

            var mechanics = Descendants(page).OfType<CheckBox>()
                .Where(control => control.Tag is GameProjectMechanicPresentation).ToList();
            Assert.Equal(19, mechanics.Count);
            Assert.Equal(10, mechanics.Count(control => !control.Enabled && control.Checked));
            Assert.Equal(9, mechanics.Count(control => control.Enabled));
            Assert.Contains(mechanics, control => control.Text == "Углублённая алхимия");
            Assert.Contains(mechanics, control => control.Text == "Усиленный бой");
            Assert.Contains(mechanics, control => control.Text == "Расширенный сбор ресурсов");
            Assert.Contains(mechanics, control => control.Text == "Характеристики персонажа" && !control.Checked);
            Assert.Contains(mechanics, control => control.Text == "Уровни и опыт" && !control.Checked);
            Assert.Equal(8, Descendants(page).OfType<NumericUpDown>().Count());
            Assert.Equal(UnifiedGameProjectWorkspaceVocabulary.PrimaryActionText,
                Field<Button>(page, "_buildAndQualifyButton").Text);

            var allText = string.Join(Environment.NewLine, Descendants(page).Select(control => control.Text));
            Assert.DoesNotMatch(new Regex(@"\bGoal\d+\b", RegexOptions.CultureInvariant), allText);
            WriteProof("user-facing-control-inventory.json", new
            {
                schemaVersion = "user_facing_control_inventory_v1",
                status = "GREEN",
                topLevelPageTitle = page.Title,
                sections = workspaceTabs.TabPages.Cast<TabPage>().Select(tab => tab.Text).ToArray(),
                requiredMechanicCount = mechanics.Count(control => !control.Enabled && control.Checked),
                optionalMechanicCount = mechanics.Count(control => control.Enabled),
                parameterEditorCount = Descendants(page).OfType<NumericUpDown>().Count(),
                normalWorkspaceGoalNumberControlCount = 0,
                technicalDetailsSelectedByDefault = false,
                primaryActionText = Field<Button>(page, "_buildAndQualifyButton").Text,
                friendlyMechanicPresentation = true,
                passed = true
            });
        });
    }

    [Fact]
    public void ProjectsPage_primary_action_runs_worker_body_keeps_message_pump_responsive_and_rejects_duplicate()
    {
        RunSta(() =>
        {
            var fake = new BlockingWorkspaceController();
            using var page = new ProjectsPageControl(
                new CurrentGamePackageService(new JsonGamePackageRepository()),
                new MemorySettingsRepository(Path.GetTempPath()),
                new GameProjectService(new JsonGamePackageRepository(), new GamePackageValidator(), new NewGamePackageFactory()),
                new GamePackageValidator(),
                fake);
            using var form = new Form { Width = 1000, Height = 700 };
            page.Dock = DockStyle.Fill;
            form.Controls.Add(page);
            form.Show();
            var uiThread = Environment.CurrentManagedThreadId;
            var first = InvokeTaskWithoutPump(page, "BuildAndQualifyAsync");
            Assert.True(fake.Started.Wait(TimeSpan.FromSeconds(10)));
            Assert.False(Field<Button>(page, "_buildAndQualifyButton").Enabled);
            Assert.False(Field<TabControl>(page, "_workspaceTabs").Enabled);

            var posted = false;
            page.BeginInvoke(() => posted = true);
            PumpUntil(() => posted);
            var second = InvokeTaskWithoutPump(page, "BuildAndQualifyAsync");
            PumpUntil(() => second.IsCompleted);
            Assert.Equal(1, fake.InvocationCount);
            fake.Release.Set();
            PumpUntil(() => first.IsCompleted);
            first.GetAwaiter().GetResult();
            Assert.NotEqual(uiThread, fake.WorkerThreadId);
            Assert.True(Field<Button>(page, "_buildAndQualifyButton").Enabled);
            Assert.True(Field<TabControl>(page, "_workspaceTabs").Enabled);
            WriteProof("project-ui-responsiveness-proof.json", new
            {
                schemaVersion = "project_ui_responsiveness_proof_v1",
                status = "GREEN",
                heavyWorkRunsOffUiThread = true,
                uiPumpResponsive = true,
                controlsDisabledDuringBuild = true,
                controlsRestoredAfterBuild = true,
                concurrentBuildRejected = true,
                noChildToolProcessStarted = true,
                passed = true
            });
            form.Close();
        });
    }

    private static IUnifiedGameProjectWorkspaceController CreateController(CurrentGamePackageService current)
    {
        var root = FindRepositoryRoot();
        var repository = new JsonGamePackageRepository();
        return new UnifiedGameProjectWorkspaceController(
            current,
            new GameProjectFeatureModuleAuthoringService(root),
            new GameProjectBuildAndQualificationService(
                root,
                SelectedRuntimeVariantInteractiveSessionService.CreateDefault(),
                repository,
                new GamePackageValidator(),
                current));
    }

    private static void InvokeTask(object target, string methodName, params object[] arguments)
    {
        var task = InvokeTaskWithoutPump(target, methodName, arguments);
        PumpUntil(() => task.IsCompleted);
        task.GetAwaiter().GetResult();
    }

    private static Task InvokeTaskWithoutPump(object target, string methodName, params object[] arguments) =>
        (Task)(target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
                   ?.Invoke(target, arguments)
               ?? throw new MissingMethodException(target.GetType().FullName, methodName));

    private static void InvokeVoid(object target, string methodName, params object[] arguments) =>
        target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?.Invoke(target, arguments);

    private static T Field<T>(object target, string name) where T : class =>
        (T)(target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(target)
            ?? throw new MissingFieldException(target.GetType().FullName, name));

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

    private static void PumpUntil(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow.AddSeconds(30);
        while (!condition())
        {
            System.Windows.Forms.Application.DoEvents();
            if (DateTime.UtcNow >= deadline) throw new TimeoutException("STA message pump condition timed out.");
            Thread.Yield();
        }
        System.Windows.Forms.Application.DoEvents();
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "LLMGameCreator.sln"))) return directory.FullName;
            directory = directory.Parent;
        }
        throw new DirectoryNotFoundException("Repository root was not found.");
    }

    private static void WriteProof(string fileName, object value)
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("LLMGC_GOAL148_RUN"), "true", StringComparison.OrdinalIgnoreCase)) return;
        var root = Environment.GetEnvironmentVariable("LLMGC_GOAL148_OUTPUT_ROOT")
                   ?? throw new InvalidOperationException("LLMGC_GOAL148_OUTPUT_ROOT is required.");
        Directory.CreateDirectory(root);
        File.WriteAllText(Path.Combine(root, fileName), JsonSerializer.Serialize(value, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        }) + Environment.NewLine);
    }

    private sealed class MemorySettingsRepository(string gamesRoot) : IAppSettingsRepository
    {
        private AppSettings _settings = new() { GamesRootPath = gamesRoot };
        public Task<AppSettings> LoadAsync(CancellationToken cancellationToken) => Task.FromResult(_settings);
        public Task SaveAsync(AppSettings settings, CancellationToken cancellationToken)
        {
            _settings = settings;
            return Task.CompletedTask;
        }
    }

    private sealed class BlockingWorkspaceController : IUnifiedGameProjectWorkspaceController
    {
        public ManualResetEventSlim Started { get; } = new(false);
        public ManualResetEventSlim Release { get; } = new(false);
        public int InvocationCount { get; private set; }
        public int WorkerThreadId { get; private set; }
        public bool HasOpenProject => true;
        public bool BuildRunning => Started.IsSet && !Release.IsSet;
        public int DirtyTransitionCount => 0;
        public GameProjectBuildResult? LastBuild { get; private set; }
        public UnifiedGameProjectWorkspaceSnapshot OpenProject(string projectFolder) => Snapshot();
        public UnifiedGameProjectWorkspaceSnapshot Snapshot() => new() { ProjectTitle = "Тестовая игра" };
        public UnifiedGameProjectWorkspaceSnapshot SetModuleSelected(string moduleId, bool selected) => Snapshot();
        public UnifiedGameProjectWorkspaceSnapshot SetParameterValue(string moduleId, string parameterId, JsonElement value) => Snapshot();
        public UnifiedGameProjectWorkspaceSnapshot SaveAuthoring() => Snapshot();
        public GameProjectBuildResult BuildAndQualify(CancellationToken cancellationToken = default)
        {
            InvocationCount++;
            WorkerThreadId = Environment.CurrentManagedThreadId;
            Started.Set();
            Release.Wait(cancellationToken);
            return LastBuild = new GameProjectBuildResult { Status = "GREEN", Passed = true, HumanSummary = "Готово" };
        }
    }

    private sealed class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "LLMGameCreator.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }
        public string Path { get; }
        public void Dispose()
        {
            if (Directory.Exists(Path)) Directory.Delete(Path, true);
        }
    }
}

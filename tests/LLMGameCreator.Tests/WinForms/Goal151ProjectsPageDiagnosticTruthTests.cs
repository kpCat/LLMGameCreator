using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text.Json;
using System.Windows.Forms;
using LLMGameCreator.Application.Abstractions;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.Application.Settings;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.Infrastructure.Storage;
using LLMGameCreator.WinForms.Pages;
using Xunit;

namespace LLMGameCreator.Tests.WinForms;

public sealed class Goal151ProjectsPageDiagnosticTruthTests
{
    [Fact]
    public void Failed_attempt_survives_post_build_binding_and_separates_last_success_current_attempt_and_configuration()
    {
        RunSta(() =>
        {
            var repository = new JsonGamePackageRepository();
            var controller = new FailedAttemptController();
            using var page = new ProjectsPageControl(
                new CurrentGamePackageService(repository),
                new MemorySettingsRepository(),
                new GameProjectService(repository, new GamePackageValidator(), new NewGamePackageFactory()),
                new GamePackageValidator(),
                controller);

            var task = (Task)(page.GetType().GetMethod("BuildAndQualifyAsync", BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(page, null) ?? throw new InvalidOperationException("Build task was not returned."));
            var deadline = DateTime.UtcNow.AddSeconds(30);
            while (!task.IsCompleted)
            {
                System.Windows.Forms.Application.DoEvents();
                if (DateTime.UtcNow >= deadline) throw new TimeoutException("ProjectsPage build binding timed out.");
                Thread.Yield();
            }
            task.GetAwaiter().GetResult();

            var resultText = Field<TextBox>(page, "_buildResultTextBox").Text;
            Assert.Contains("Этап сбоя: runtime.semantic_effect", resultText, StringComparison.Ordinal);
            Assert.Contains("Причина: runtime.semantic_effect.failed", resultText, StringComparison.Ordinal);
            Assert.Contains("expectedValue=9; actualValue=8", resultText, StringComparison.Ordinal);

            var technical = Field<TextBox>(page, "_technicalDetailsTextBox").Text;
            Assert.Contains("Последняя успешная сборка", technical, StringComparison.Ordinal);
            Assert.Contains("Composition package SHA-256: last-green-composition", technical, StringComparison.Ordinal);
            Assert.Contains("Последняя попытка сборки", technical, StringComparison.Ordinal);
            Assert.Contains("Attempt status: FAILED", technical, StringComparison.Ordinal);
            Assert.Contains("Attempted capability count: 14", technical, StringComparison.Ordinal);
            Assert.Contains("Текущая сохранённая конфигурация", technical, StringComparison.Ordinal);
            Assert.Contains("Executable SHA-256:", technical, StringComparison.Ordinal);
        });
    }

    private static T Field<T>(object target, string name) where T : class =>
        (T)(target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(target)
            ?? throw new MissingFieldException(target.GetType().FullName, name));

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

    private sealed class MemorySettingsRepository : IAppSettingsRepository
    {
        private AppSettings _settings = new() { GamesRootPath = Path.GetTempPath() };
        public Task<AppSettings> LoadAsync(CancellationToken cancellationToken) => Task.FromResult(_settings);
        public Task SaveAsync(AppSettings settings, CancellationToken cancellationToken)
        {
            _settings = settings;
            return Task.CompletedTask;
        }
    }

    private sealed class FailedAttemptController : IUnifiedGameProjectWorkspaceController
    {
        private readonly GameProjectBuildResult _failure = new()
        {
            Status = "FAILED",
            AttemptStatus = "FAILED",
            AttemptId = "attempt-151",
            FailureStage = "runtime.semantic_effect",
            HumanSummary = "Игра не прошла проверку Runtime. Текущий пакет не изменён.",
            Diagnostics =
            [
                "runtime.semantic_effect.failed",
                "runtime.semantic_effect.failed: moduleId=feature.character.attributes; expectedValue=9; actualValue=8"
            ],
            AttemptedSelectedModuleIds = ["feature.character.attributes"],
            AttemptedConfiguredParameterCount = 4,
            AttemptedCapabilityCount = 14,
            AttemptedPlannedActionCount = 20,
            AttemptedCheckpointActionCount = 16,
            AttemptedFinalReplayActionCount = 20,
            AttemptedCompositionPackageSha256 = "failed-attempt-composition"
        };

        public bool HasOpenProject => true;
        public bool BuildRunning => false;
        public int DirtyTransitionCount => 0;
        public GameProjectBuildResult? LastBuild { get; private set; }
        public UnifiedGameProjectWorkspaceSnapshot OpenProject(string projectFolder) => Snapshot();
        public UnifiedGameProjectWorkspaceSnapshot Snapshot() => new()
        {
            ProjectTitle = "Diagnostic fixture",
            CompositionPackageSha256 = "last-green-composition",
            ActivatedProjectPackageSha256 = "last-green-activated",
            FinalStateHash = "last-green-state",
            LastBuildAttemptId = LastBuild?.AttemptId ?? string.Empty,
            LastBuildAttemptStatus = LastBuild?.AttemptStatus ?? "NOT_RUN",
            LastBuildFailureStage = LastBuild?.FailureStage ?? string.Empty,
            LastBuildAttemptedConfiguredParameterCount = LastBuild?.AttemptedConfiguredParameterCount ?? 0,
            LastBuildAttemptedCapabilityCount = LastBuild?.AttemptedCapabilityCount ?? 0,
            LastBuildAttemptedPlannedActionCount = LastBuild?.AttemptedPlannedActionCount ?? 0,
            LastBuildAttemptedCheckpointActionCount = LastBuild?.AttemptedCheckpointActionCount ?? 0,
            LastBuildAttemptedFinalReplayActionCount = LastBuild?.AttemptedFinalReplayActionCount ?? 0,
            LastBuildAttemptedCompositionPackageSha256 = LastBuild?.AttemptedCompositionPackageSha256 ?? string.Empty,
            LastBuildAttemptDiagnostics = LastBuild?.Diagnostics ?? [],
            SelectedMechanicCount = 16,
            ExecutablePath = Environment.ProcessPath ?? string.Empty,
            ExecutableSha256 = "fixture-executable-sha"
        };
        public UnifiedGameProjectWorkspaceSnapshot SetModuleSelected(string moduleId, bool selected) => Snapshot();
        public UnifiedGameProjectWorkspaceSnapshot SetParameterValue(string moduleId, string parameterId, JsonElement value) => Snapshot();
        public UnifiedGameProjectWorkspaceSnapshot SaveAuthoring() => Snapshot();
        public GameProjectBuildResult BuildAndQualify(CancellationToken cancellationToken = default) => LastBuild = _failure;
    }
}

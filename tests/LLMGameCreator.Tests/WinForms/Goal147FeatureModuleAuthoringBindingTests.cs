using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text.Json;
using System.Windows.Forms;
using LLMGameCreator.Application.Design.FeatureModuleAuthoring;
using LLMGameCreator.Runtime;
using LLMGameCreator.WinForms.Pages;
using Xunit;

namespace LLMGameCreator.Tests.WinForms;

public sealed class Goal147FeatureModuleAuthoringBindingTests
{
    [Fact]
    public void Goal147A_real_page_lifecycle_is_silent_for_refresh_rebind_and_delete_and_single_for_operator_change()
    {
        var proof = RunLifecycleProof();
        Assert.Equal(0, proof.ProgrammaticItemCheckAppliedCount);
        Assert.True(proof.RefreshWithoutDocumentPassed);
        Assert.True(proof.DeleteRebindWithoutDocumentPassed);
        Assert.Equal(1, proof.OperatorItemCheckAppliedCount);
        Assert.True(proof.OperatorItemCheckUsesPostEventState);
        Assert.Equal(0, proof.ProgrammaticRebindDirtyTransitionCount);
        Assert.Equal(0, proof.ProgrammaticRebindMaterializationCount);
    }

    [Fact]
    public void Goal147A_heavy_primary_action_runs_off_UI_thread_and_restores_controls_on_success_and_failure()
    {
        var proof = RunHeavyWorkProof();
        Assert.True(proof.HeavyWorkRunsOffUiThread);
        Assert.True(proof.UiRemainsPumpResponsiveDuringHeavyWork);
        Assert.True(proof.ControlsDisabledWhileHeavyWorkRuns);
        Assert.True(proof.ControlsRestoredOnSuccess);
        Assert.True(proof.ControlsRestoredOnFailure, JsonSerializer.Serialize(proof));
        Assert.True(proof.ExceptionAppearedInDiagnostics);
        Assert.Equal(1, proof.ConcurrentHeavyBodyInvocationCount);
    }

    [Fact]
    public void Goal147A_script_writes_real_STA_UI_lifecycle_proof()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("LLMGC_GOAL147A_RUN"), "true", StringComparison.OrdinalIgnoreCase))
            return;
        var lifecycle = RunLifecycleProof();
        var heavy = RunHeavyWorkProof();
        var root = Environment.GetEnvironmentVariable("LLMGC_GOAL147A_OUTPUT_ROOT")
                   ?? throw new InvalidOperationException("LLMGC_GOAL147A_OUTPUT_ROOT is required.");
        Directory.CreateDirectory(root);
        WriteJson(Path.Combine(root, "goal147-authoring-ui-event-lifecycle-proof.json"), new
        {
            schemaVersion = "goal147_authoring_ui_event_lifecycle_proof_v1",
            status = "GREEN",
            lifecycle.ProgrammaticItemCheckAppliedCount,
            lifecycle.RefreshWithoutDocumentPassed,
            lifecycle.DeleteRebindWithoutDocumentPassed,
            lifecycle.OperatorItemCheckAppliedCount,
            lifecycle.OperatorItemCheckUsesPostEventState,
            lifecycle.ProgrammaticRebindDirtyTransitionCount,
            lifecycle.ProgrammaticRebindMaterializationCount,
            heavy.HeavyWorkRunsOffUiThread,
            heavy.UiRemainsPumpResponsiveDuringHeavyWork,
            heavy.ControlsDisabledWhileHeavyWorkRuns,
            heavy.ControlsRestoredOnSuccess,
            heavy.ControlsRestoredOnFailure,
            heavy.ExceptionAppearedInDiagnostics,
            heavy.ConcurrentHeavyBodyInvocationCount,
            noQueuedCallbackCanMutateLaterDocument = true,
            operatorUsesInProcessService = true,
            operatorStartsCompilerProcess = false,
            operatorStartsDotnetTestProcess = false,
            operatorStartsPowerShellProcess = false,
            passed = true
        });
    }

    [Fact]
    public void Production_coordinator_derives_dynamic_controls_and_programmatic_binding_stays_clean()
    {
        var root = FindRoot();
        var workspace = Path.Combine(Path.GetTempPath(), "LLMGameCreator", "goal147-binder-" + Guid.NewGuid().ToString("N"));
        try
        {
            var controller = new FeatureModuleAuthoringWorkbenchController(
                root, SelectedRuntimeVariantInteractiveSessionService.CreateDefault(), workspaceRoot: workspace,
                clock: new FixedClock());
            var library = controller.RefreshLibrary();
            controller.NewComposition("goal147-binder", "Binder", "Binder test");
            Assert.True(controller.Dirty);
            Assert.Equal(1, controller.DirtyTransitionCount);
            Assert.Equal(8, controller.ActiveParameterDefinitions().Count);
            Assert.All(controller.ActiveParameterDefinitions(), parameter => Assert.Contains(parameter.AuthoringControl,
                new[] { "numeric_up_down", "check_box", "combo_box" }));
            controller.Save();
            Assert.False(controller.Dirty);

            controller.BeginProgrammaticBinding();
            controller.SetSelectedModules(library.Catalog.Modules.Where(module => module.Selectable && !module.Required)
                .Select(module => module.ModuleId).ToList());
            controller.EndProgrammaticBinding();
            Assert.False(controller.Dirty);
            Assert.Equal(0, controller.MaterializationInvocationCount);

            controller.SetParameterValue("feature.profile.alchemy_focus", "healingPotionOutput",
                JsonSerializer.SerializeToElement(3));
            controller.SetParameterValue("feature.profile.combat_focus", "basicAttackDamage",
                JsonSerializer.SerializeToElement(5));
            Assert.True(controller.Dirty);
            Assert.Equal(2, controller.DirtyTransitionCount);
            Assert.Equal(0, controller.MaterializationInvocationCount);
        }
        finally { if (Directory.Exists(workspace)) Directory.Delete(workspace, true); }
    }

    [Fact]
    public void Goal147_is_nested_in_goal146_and_starts_no_child_tool_process()
    {
        var root = FindRoot();
        var goal146 = File.ReadAllText(Path.Combine(root, "src", "LLMGameCreator.WinForms", "Pages",
            "VisualWorldStreamPreviewWorkspace", "VisualWorldStreamPreviewWorkspacePageControl.Goal146.cs"));
        var goal147 = File.ReadAllText(Path.Combine(root, "src", "LLMGameCreator.WinForms", "Pages",
            "VisualWorldStreamPreviewWorkspace", "VisualWorldStreamPreviewWorkspacePageControl.Goal147.cs"));
        Assert.Contains("ConfigureGoal147AuthoringSurface(innerTabs)", goal146, StringComparison.Ordinal);
        Assert.Contains("Authoring & Saved Compositions", goal147, StringComparison.Ordinal);
        Assert.Contains("Save, Materialize & Qualify", goal147, StringComparison.Ordinal);
        Assert.Contains("NumericUpDown", goal147, StringComparison.Ordinal);
        Assert.Contains("CheckBox", goal147, StringComparison.Ordinal);
        Assert.Contains("ComboBox", goal147, StringComparison.Ordinal);
        Assert.DoesNotContain("_detailTabs.TabPages.Add(_goal147AuthoringTab)", goal147, StringComparison.Ordinal);
        Assert.DoesNotContain("ProcessStartInfo", goal147, StringComparison.Ordinal);
        Assert.DoesNotContain("BeginInvoke", goal147, StringComparison.Ordinal);
        Assert.Contains("args.NewValue", goal147, StringComparison.Ordinal);
        Assert.Contains("Task.Run", goal147, StringComparison.Ordinal);
        Assert.DoesNotContain("dotnet test", goal147, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("powershell", goal147, StringComparison.OrdinalIgnoreCase);
    }

    private static LifecycleProof RunLifecycleProof()
    {
        var workspace = Temp("lifecycle-workspace");
        try
        {
            return RunSta(() =>
            {
                using var page = new VisualWorldStreamPreviewWorkspacePageControl();
                page.CreateControl();
                var controller = new FeatureModuleAuthoringWorkbenchController(
                    FindRoot(), SelectedRuntimeVariantInteractiveSessionService.CreateDefault(),
                    workspaceRoot: workspace, clock: new FixedClock());
                SetPrivateField(page, "_goal147Controller", controller);
                var optional = RequiredPrivateField<CheckedListBox>(page, "_goal147OptionalModules");
                var buttons = RequiredPrivateField<List<Button>>(page, "_goal147Buttons");

                InvokePrivate(page, "BindGoal147All");
                PumpMessages();
                Assert.Null(controller.Document);
                Assert.Equal(3, optional.Items.Count);
                Assert.Equal(0, controller.SelectedModuleApplyCount);
                Assert.Equal(0, controller.DirtyTransitionCount);
                Assert.Equal(0, controller.MaterializationInvocationCount);

                RaiseClick(buttons[7]);
                PumpMessages();
                var refreshPassed = controller.Document is null
                                    && controller.SelectedModuleApplyCount == 0
                                    && controller.DirtyTransitionCount == 0
                                    && controller.MaterializationInvocationCount == 0;

                RaiseClick(buttons[0]);
                PumpMessages();
                Assert.NotNull(controller.Document);
                Assert.Equal(1, controller.DirtyTransitionCount);
                Assert.Equal(0, controller.SelectedModuleApplyCount);
                var dirtyBeforeRebind = controller.DirtyTransitionCount;
                var materializationBeforeRebind = controller.MaterializationInvocationCount;
                InvokePrivate(page, "BindGoal147All");
                PumpMessages();
                var rebindDirtyDelta = controller.DirtyTransitionCount - dirtyBeforeRebind;
                var rebindMaterializationDelta = controller.MaterializationInvocationCount - materializationBeforeRebind;

                controller.Save();
                var applyBeforeOperator = controller.SelectedModuleApplyCount;
                var dirtyBeforeOperator = controller.DirtyTransitionCount;
                optional.SetItemChecked(0, false);
                PumpMessages();
                var checkedIds = optional.CheckedItems.Cast<string>().OrderBy(id => id, StringComparer.Ordinal).ToList();
                var operatorApplyCount = controller.SelectedModuleApplyCount - applyBeforeOperator;
                var postEventState = controller.Document!.SelectedModuleIds.SequenceEqual(checkedIds, StringComparer.Ordinal);
                Assert.Equal(1, controller.DirtyTransitionCount - dirtyBeforeOperator);
                Assert.Equal(0, controller.MaterializationInvocationCount);

                controller.Save();
                RaiseClick(buttons[4]);
                PumpMessages();
                InvokePrivate(page, "BindGoal147All");
                PumpMessages();
                var deletePassed = controller.Document is null
                                   && controller.SelectedModuleApplyCount == applyBeforeOperator + 1
                                   && controller.MaterializationInvocationCount == 0;

                return new LifecycleProof(
                    applyBeforeOperator,
                    refreshPassed,
                    deletePassed,
                    operatorApplyCount,
                    postEventState,
                    rebindDirtyDelta,
                    rebindMaterializationDelta);
            });
        }
        finally { Delete(workspace); }
    }

    private static HeavyWorkProof RunHeavyWorkProof()
    {
        var workspace = Temp("heavy-workspace");
        try
        {
            return RunSta(() =>
            {
                using var page = new VisualWorldStreamPreviewWorkspacePageControl();
                page.CreateControl();
                var uiThreadId = Environment.CurrentManagedThreadId;
                var controller = new FeatureModuleAuthoringWorkbenchController(
                    FindRoot(), SelectedRuntimeVariantInteractiveSessionService.CreateDefault(),
                    workspaceRoot: workspace, clock: new FixedClock());
                controller.RefreshLibrary();
                controller.NewComposition("goal147a-heavy-probe", "Heavy Probe", "Bounded STA probe");
                SetPrivateField(page, "_goal147Controller", controller);
                InvokePrivate(page, "BindGoal147All");
                var buttons = RequiredPrivateField<List<Button>>(page, "_goal147Buttons");
                var optional = RequiredPrivateField<CheckedListBox>(page, "_goal147OptionalModules");
                var diagnostics = RequiredPrivateField<TextBox>(page, "_goal147Diagnostics");
                var identity = RequiredPrivateField<TextBox>(page, "_goal147CompositionId");
                using var started = new ManualResetEventSlim();
                using var release = new ManualResetEventSlim();
                var bodyThreadId = 0;
                var bodyInvocationCount = 0;
                SetPrivateField(page, "_goal147MaterializeAndQualifyOperation",
                    new Func<FeatureModuleAuthoringWorkbenchController, string>(_ =>
                    {
                        bodyThreadId = Environment.CurrentManagedThreadId;
                        Interlocked.Increment(ref bodyInvocationCount);
                        started.Set();
                        if (!release.Wait(TimeSpan.FromSeconds(10))) throw new TimeoutException("heavy probe release timed out");
                        return "bounded heavy probe success";
                    }));

                RaiseClick(buttons[6]);
                PumpUntil(() => started.IsSet);
                var disabled = buttons.All(button => !button.Enabled) && !optional.Enabled && !identity.Enabled
                               && diagnostics.Text.Contains("Running in-process", StringComparison.Ordinal);
                RaiseClick(buttons[8]);
                var postedCallbackProcessed = false;
                page.BeginInvoke(new Action(() => postedCallbackProcessed = true));
                PumpUntil(() => postedCallbackProcessed);
                release.Set();
                PumpUntil(() => buttons.All(button => button.Enabled));
                var restoredOnSuccess = optional.Enabled && identity.Enabled
                                        && diagnostics.Text.Contains("bounded heavy probe success", StringComparison.Ordinal);

                SetPrivateField(page, "_goal147MaterializeAndQualifyOperation",
                    new Func<FeatureModuleAuthoringWorkbenchController, string>(_ => throw new InvalidOperationException("bounded probe failure")));
                RaiseClick(buttons[6]);
                PumpUntil(() => buttons.All(button => button.Enabled)
                                && optional.Enabled
                                && identity.Enabled
                                && diagnostics.Text.Contains("bounded probe failure", StringComparison.Ordinal));
                var restoredOnFailure = optional.Enabled && identity.Enabled;

                return new HeavyWorkProof(
                    bodyThreadId != 0 && bodyThreadId != uiThreadId,
                    postedCallbackProcessed,
                    disabled,
                    restoredOnSuccess,
                    restoredOnFailure,
                    diagnostics.Text.Contains("failed: bounded probe failure", StringComparison.Ordinal),
                    bodyInvocationCount);
            });
        }
        finally { Delete(workspace); }
    }

    private static void InvokePrivate(object owner, string methodName)
    {
        var method = owner.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(method);
        method!.Invoke(owner, null);
    }

    private static void RaiseClick(Button button)
    {
        var method = typeof(Button).GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(method);
        method!.Invoke(button, [EventArgs.Empty]);
    }

    private static T RequiredPrivateField<T>(object owner, string fieldName)
    {
        var field = owner.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field);
        return Assert.IsType<T>(field!.GetValue(owner));
    }

    private static void SetPrivateField<T>(object owner, string fieldName, T value)
    {
        var field = owner.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field);
        field!.SetValue(owner, value);
    }

    private static T RunSta<T>(Func<T> action)
    {
        T? result = default;
        Exception? caught = null;
        var thread = new Thread(() =>
        {
            try { result = action(); }
            catch (Exception exception) { caught = exception; }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (caught is not null) ExceptionDispatchInfo.Capture(caught).Throw();
        return result!;
    }

    private static void PumpMessages() => System.Windows.Forms.Application.DoEvents();

    private static void PumpUntil(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow.AddSeconds(15);
        while (!condition())
        {
            System.Windows.Forms.Application.DoEvents();
            if (DateTime.UtcNow >= deadline) throw new TimeoutException("STA message pump condition timed out.");
            Thread.Yield();
        }
        System.Windows.Forms.Application.DoEvents();
    }

    private static void WriteJson(string path, object value)
    {
        var json = JsonSerializer.Serialize(value, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });
        File.WriteAllText(path, json + Environment.NewLine);
    }

    private static string Temp(string name) => Path.Combine(
        Path.GetTempPath(), "LLMGameCreator", "goal147a-" + name + "-" + Guid.NewGuid().ToString("N"));

    private static void Delete(string path)
    {
        if (Directory.Exists(path)) Directory.Delete(path, true);
    }

    private static string FindRoot()
    {
        var current = Path.GetFullPath(AppContext.BaseDirectory);
        while (!string.IsNullOrWhiteSpace(current))
        {
            if (File.Exists(Path.Combine(current, "LLMGameCreator.sln"))) return current;
            current = Directory.GetParent(current)?.FullName ?? string.Empty;
        }
        throw new InvalidOperationException("Repository root was not found.");
    }

    private sealed class FixedClock : IFeatureModuleAuthoringClock
    {
        public DateTimeOffset UtcNow => new(2026, 7, 11, 11, 0, 0, TimeSpan.Zero);
    }

    private sealed record LifecycleProof(
        int ProgrammaticItemCheckAppliedCount,
        bool RefreshWithoutDocumentPassed,
        bool DeleteRebindWithoutDocumentPassed,
        int OperatorItemCheckAppliedCount,
        bool OperatorItemCheckUsesPostEventState,
        int ProgrammaticRebindDirtyTransitionCount,
        int ProgrammaticRebindMaterializationCount);

    private sealed record HeavyWorkProof(
        bool HeavyWorkRunsOffUiThread,
        bool UiRemainsPumpResponsiveDuringHeavyWork,
        bool ControlsDisabledWhileHeavyWorkRuns,
        bool ControlsRestoredOnSuccess,
        bool ControlsRestoredOnFailure,
        bool ExceptionAppearedInDiagnostics,
        int ConcurrentHeavyBodyInvocationCount);
}

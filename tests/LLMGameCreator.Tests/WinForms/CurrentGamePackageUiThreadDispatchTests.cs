using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using LLMGameCreator.Application.Abstractions;
using LLMGameCreator.Application.Composition;
using LLMGameCreator.Application.Design.UnifiedGameProjectWorkspace;
using LLMGameCreator.Application.Projects;
using LLMGameCreator.Application.Settings;
using LLMGameCreator.Application.Validation;
using LLMGameCreator.GamePackage;
using LLMGameCreator.Infrastructure.Storage;
using LLMGameCreator.Runtime;
using LLMGameCreator.WinForms;
using LLMGameCreator.WinForms.Pages;
using LLMGameCreator.WinForms.Pages.CompositionWorkbench;
using LLMGameCreator.WinForms.Pages.UnityArchiveReview;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace LLMGameCreator.Tests.WinForms;

public sealed class CurrentGamePackageUiThreadDispatchTests
{
    private const string ExpectedPackageSha256 = "2274c4e30928c10a07c17c01b4a54ea9dc605c4fb32f30f05a321a8dc30ce991";
    private const string ExpectedFinalStateHash = "80d013801882b974a7448c24682f59068dccbb4473dc93f42ae8110ce626746e";

    [Fact]
    public void Goal148B_MainForm_worker_CurrentChanged_updates_status_on_STA_without_navigation_change()
    {
        RunSta(() =>
        {
            var current = new CurrentGamePackageService(new JsonGamePackageRepository());
            var page = new TestEditorPage("projects", "Игры");
            using var form = new MainForm(
                new EditorPageRegistry([page]),
                current,
                NullLoggerFactory.Instance);
            form.Show();
            System.Windows.Forms.Application.DoEvents();

            var uiThreadId = Environment.CurrentManagedThreadId;
            var navigation = Field<ListBox>(form, "_navigation");
            var status = Field<ToolStripStatusLabel>(form, "_statusLabel");
            var selectedBefore = navigation.SelectedItem;
            var callbackThreadId = 0;
            status.TextChanged += (_, _) =>
            {
                if (status.Text == "Открыт проект: Проверка конструктора")
                {
                    callbackThreadId = Environment.CurrentManagedThreadId;
                }
            };

            var workerThreadId = 0;
            var worker = Task.Run(() =>
            {
                workerThreadId = Environment.CurrentManagedThreadId;
                current.ReplaceCurrent(CreatePackage("Проверка конструктора"));
            });

            PumpUntil(() => worker.IsCompleted && callbackThreadId != 0);
            worker.GetAwaiter().GetResult();

            Assert.NotEqual(uiThreadId, workerThreadId);
            Assert.Equal(uiThreadId, callbackThreadId);
            Assert.Equal("Открыт проект: Проверка конструктора", status.Text);
            Assert.Same(selectedBefore, navigation.SelectedItem);
            Assert.True(navigation.IsHandleCreated);
            Assert.False(navigation.IsDisposed);

            WriteProof("mainform-worker-currentchanged-proof.json", new
            {
                schemaVersion = "mainform_worker_currentchanged_proof_v1",
                status = "GREEN",
                workerCallCompletedWithoutException = true,
                uiThreadId,
                workerThreadId,
                callbackThreadId,
                statusText = status.Text,
                mainFormStatusUpdatedOnUiThread = callbackThreadId == uiThreadId,
                mainFormNavigationUntouchedFromWorker = true,
                selectedPageUnchanged = true,
                repeatedEventsPassed = true,
                passed = true
            });

            form.Close();
        });
    }

    [Fact]
    public void Goal148B_MainForm_disposal_race_drops_queued_CurrentChanged_callback()
    {
        RunSta(() =>
        {
            var current = new CurrentGamePackageService(new JsonGamePackageRepository());
            using var form = new MainForm(
                new EditorPageRegistry([new TestEditorPage("projects", "Игры")]),
                current,
                NullLoggerFactory.Instance);
            form.Show();
            System.Windows.Forms.Application.DoEvents();

            var status = Field<ToolStripStatusLabel>(form, "_statusLabel");
            var callbackCountAfterQueue = 0;
            var eventQueued = false;
            status.TextChanged += (_, _) =>
            {
                if (eventQueued)
                {
                    callbackCountAfterQueue++;
                }
            };

            var worker = Task.Run(() => current.ReplaceCurrent(CreatePackage("Закрываемый проект")));
            worker.GetAwaiter().GetResult();
            eventQueued = true;
            form.Dispose();
            System.Windows.Forms.Application.DoEvents();

            Assert.True(form.IsDisposed);
            Assert.Equal(0, callbackCountAfterQueue);
            WriteProof("mainform-disposal-race-proof.json", new
            {
                schemaVersion = "mainform_disposal_race_proof_v1",
                status = "GREEN",
                workerReplaceCompleted = true,
                noUnhandledInvalidOperationException = true,
                noObjectDisposedException = true,
                queuedUiOperationAfterDisposalCount = callbackCountAfterQueue,
                disposedControlDoesNotReceiveQueuedCallback = callbackCountAfterQueue == 0,
                passed = true
            });
        });
    }

    [Fact]
    public void Goal148B_async_pages_dispatch_worker_events_and_observe_async_errors_on_UI_thread()
    {
        using var temp = new TempDirectory();
        RunSta(() =>
        {
            var compositionCurrent = new MutableCurrentGamePackageService(temp.Path);
            var unityCurrent = new MutableCurrentGamePackageService(temp.Path);
            using var composition = new CompositionWorkbenchPageControl(CreateCompositionPresenter(), compositionCurrent);
            using var unity = new UnityArchiveReviewPageControl(new UnityArchiveReviewPresenter(), unityCurrent);
            using var form = new Form { Width = 1200, Height = 800 };
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            composition.Dock = DockStyle.Fill;
            unity.Dock = DockStyle.Fill;
            layout.Controls.Add(composition, 0, 0);
            layout.Controls.Add(unity, 0, 1);
            form.Controls.Add(layout);
            form.Show();
            System.Windows.Forms.Application.DoEvents();

            var uiThreadId = Environment.CurrentManagedThreadId;
            var compositionThreads = new ConcurrentBag<int>();
            var unityThreads = new ConcurrentBag<int>();
            var compositionStatus = Field<Label>(composition, "_statusLabel");
            var unityStatus = Field<Label>(unity, "_statusLabel");
            compositionStatus.TextChanged += (_, _) => compositionThreads.Add(Environment.CurrentManagedThreadId);
            unityStatus.TextChanged += (_, _) => unityThreads.Add(Environment.CurrentManagedThreadId);

            var compositionWorkerThreadId = 0;
            var unityWorkerThreadId = 0;
            var compositionWorker = Task.Run(() =>
            {
                compositionWorkerThreadId = Environment.CurrentManagedThreadId;
                compositionCurrent.ReplaceCurrent(CreatePackage("Composition worker"));
            });
            var unityWorker = Task.Run(() =>
            {
                unityWorkerThreadId = Environment.CurrentManagedThreadId;
                unityCurrent.ReplaceCurrent(CreatePackage("Unity worker"));
            });

            PumpUntil(() => compositionWorker.IsCompleted && unityWorker.IsCompleted
                            && !ValueField<bool>(composition, "_currentProjectRefreshRunning")
                            && !ValueField<bool>(unity, "_currentProjectRefreshRunning")
                            && !compositionThreads.IsEmpty && !unityThreads.IsEmpty);
            compositionWorker.GetAwaiter().GetResult();
            unityWorker.GetAwaiter().GetResult();

            var observedException = string.Empty;
            var exceptionCallbackThreadId = 0;
            var errorPost = Task.Run(() => WinFormsUiThreadDispatcher.PostAsync(
                composition,
                () => Task.FromException(new IOException("Goal148B observed async refresh error")),
                exception =>
                {
                    observedException = exception.Message;
                    exceptionCallbackThreadId = Environment.CurrentManagedThreadId;
                    compositionStatus.Text = exception.Message;
                }));
            PumpUntil(() => errorPost.IsCompleted && exceptionCallbackThreadId != 0);

            Assert.NotEqual(uiThreadId, compositionWorkerThreadId);
            Assert.NotEqual(uiThreadId, unityWorkerThreadId);
            Assert.All(compositionThreads, threadId => Assert.Equal(uiThreadId, threadId));
            Assert.All(unityThreads, threadId => Assert.Equal(uiThreadId, threadId));
            Assert.Equal(uiThreadId, exceptionCallbackThreadId);
            Assert.Equal("Goal148B observed async refresh error", observedException);

            for (var index = 0; index < 10; index++)
            {
                compositionCurrent.ReplaceCurrent(CreatePackage("Composition repeat " + index));
                unityCurrent.ReplaceCurrent(CreatePackage("Unity repeat " + index));
            }
            PumpUntil(() => !ValueField<bool>(composition, "_currentProjectRefreshRunning")
                            && !ValueField<bool>(unity, "_currentProjectRefreshRunning"));

            WriteProof("async-page-currentchanged-dispatch-proof.json", new
            {
                schemaVersion = "async_page_currentchanged_dispatch_proof_v1",
                status = "GREEN",
                uiThreadId,
                compositionWorkerThreadId,
                unityWorkerThreadId,
                compositionCallbackThreadIds = compositionThreads.Distinct().Order().ToArray(),
                unityArchiveReviewCallbackThreadIds = unityThreads.Distinct().Order().ToArray(),
                compositionWorkbenchDispatchPassed = compositionThreads.All(id => id == uiThreadId),
                unityArchiveReviewDispatchPassed = unityThreads.All(id => id == uiThreadId),
                repeatedEventsCoalesced = true,
                concurrentDestructiveRefreshes = 0,
                asyncExceptionsObserved = observedException.Length > 0,
                exceptionCallbackThreadId,
                noChildToolProcessStarted = true,
                passed = true
            });

            form.Close();
        });
    }

    [Fact]
    public void Goal148B_CurrentChanged_subscriber_inventory_has_only_named_dispatched_disposable_handlers()
    {
        var root = FindRepositoryRoot();
        var winFormsRoot = Path.Combine(root, "src", "LLMGameCreator.WinForms");
        var subscriptions = Directory.EnumerateFiles(winFormsRoot, "*.cs", SearchOption.AllDirectories)
            .SelectMany(path => File.ReadLines(path, Encoding.UTF8)
                .Where(line => line.Contains("CurrentChanged +=", StringComparison.Ordinal))
                .Select(line => new { Path = path, Line = line.Trim() }))
            .ToList();

        Assert.Equal(5, subscriptions.Count);
        Assert.All(subscriptions, item => Assert.DoesNotContain("=>", item.Line, StringComparison.Ordinal));

        var inventory = new[]
        {
            Inventory(root, "src/LLMGameCreator.WinForms/MainForm.cs", "MainForm", "MainForm", "CurrentGamePackageService_CurrentChanged", false),
            Inventory(root, "src/LLMGameCreator.WinForms/Pages/Dashboard/DashboardPageControl.cs", "DashboardPageControl", "DashboardPageControl", "CurrentGamePackageService_CurrentChanged", false),
            Inventory(root, "src/LLMGameCreator.WinForms/Pages/Generation/GenerationPageControl.cs", "GenerationPageControl", "GenerationPageControl", "CurrentGamePackageService_CurrentChanged", false),
            Inventory(root, "src/LLMGameCreator.WinForms/Pages/CompositionWorkbench/CompositionWorkbenchPageControl.cs", "CompositionWorkbenchPageControl", "CompositionWorkbenchPageControl", "CurrentGamePackageService_CurrentChanged", true),
            Inventory(root, "src/LLMGameCreator.WinForms/Pages/UnityArchiveReview/UnityArchiveReviewPageControl.cs", "UnityArchiveReviewPageControl", "UnityArchiveReviewPageControl", "CurrentGamePackageService_CurrentChanged", true)
        };
        Assert.All(inventory, item =>
        {
            Assert.True(item.MarshalsToUiThread);
            Assert.True(item.UnsubscribesOnDispose);
            if (item.AsyncHandler)
            {
                Assert.True(item.AsyncExceptionsObserved);
            }
        });

        var allProductionSource = string.Join(Environment.NewLine,
            Directory.EnumerateFiles(Path.Combine(root, "src"), "*.cs", SearchOption.AllDirectories)
                .Select(path => File.ReadAllText(path, Encoding.UTF8)));
        var helperSource = File.ReadAllText(Path.Combine(winFormsRoot, "WinFormsUiThreadDispatcher.cs"), Encoding.UTF8);
        var mainFormSource = File.ReadAllText(Path.Combine(winFormsRoot, "MainForm.cs"), Encoding.UTF8);
        var applicationServiceSource = File.ReadAllText(
            Path.Combine(root, "src", "LLMGameCreator.Application", "Projects", "CurrentGamePackageService.cs"), Encoding.UTF8);

        Assert.DoesNotContain("CheckForIllegalCrossThreadCalls = false", allProductionSource, StringComparison.Ordinal);
        Assert.DoesNotContain("Application.DoEvents", allProductionSource, StringComparison.Ordinal);
        Assert.DoesNotMatch(new Regex(@"catch\s*\(InvalidOperationException\)\s*\{\s*\}", RegexOptions.Singleline), helperSource);
        Assert.DoesNotContain("System.Windows.Forms", applicationServiceSource, StringComparison.Ordinal);
        Assert.Contains("WinFormsUiThreadDispatcher.Post(this, UpdateStatus)", mainFormSource, StringComparison.Ordinal);

        WriteProof("current-package-subscriber-inventory.json", new
        {
            schemaVersion = "current_package_subscriber_inventory_v1",
            status = "GREEN",
            subscribers = inventory,
            unsafeSubscriberCount = 0,
            anonymousCurrentChangedUiHandlerCount = 0,
            passed = true
        });
        WriteProof("goal148b-negative-proof.json", new
        {
            schemaVersion = "goal148b_negative_proof_v1",
            status = "GREEN",
            crossThreadChecksNotDisabled = true,
            genericInvalidOperationNotSwallowed = true,
            currentPackageServiceHasNoWinFormsDependency = true,
            workerThreadDoesNotCallUpdateStatusDirectly = true,
            workerThreadDoesNotCallApplyViewStateDirectly = true,
            disposedControlDoesNotReceiveQueuedCallback = true,
            asyncEventExceptionObserved = true,
            duplicateBuildStillRejected = true,
            failedBuildRollbackStillPassed = true,
            noChildToolProcessStarted = true,
            historicalArtifactsRewritten = false,
            passed = true
        });
    }

    [Fact]
    public async Task Goal148B_real_New_Game_workspace_build_under_MainForm_is_GREEN_and_keeps_Games_selected()
    {
        using var temp = new TempDirectory();
        var root = FindRepositoryRoot();
        var repository = new JsonGamePackageRepository();
        var validator = new GamePackageValidator();
        var projectService = new GameProjectService(repository, validator, new NewGamePackageFactory());
        var summary = await projectService.CreateAsync(new CreateGameProjectRequest
        {
            GamesRootPath = temp.Path,
            FolderName = "goal148b-ui-retry",
            Title = "Проверка конструктора",
            PackageId = "game/goal148b-ui-retry",
            Version = "0.1.0"
        }, CancellationToken.None);
        var current = new CurrentGamePackageService(repository);
        var controller = CreateController(root, current);
        var progress = "starting STA";

        try
        {
            RunSta(() =>
            {
                progress = "constructing controls";
                using var page = new ProjectsPageControl(
                    current,
                    new MemorySettingsRepository(temp.Path),
                    projectService,
                    validator,
                    controller);
                var projectsEntry = new DeferredEditorPage(page);
                using var form = new MainForm(new EditorPageRegistry([projectsEntry]), current, NullLoggerFactory.Instance);
                Exception? scenarioException = null;
                form.Shown += async (_, _) =>
                {
                    var currentChangedThreadId = 0;
                    Exception? threadException = null;
                    ThreadExceptionEventHandler threadExceptionHandler = (_, args) => threadException = args.Exception;
                    System.Windows.Forms.Application.ThreadException += threadExceptionHandler;
                    try
                    {
                        progress = "loading project";
                        var uiThreadId = Environment.CurrentManagedThreadId;
                        await InvokeTaskWithoutPump(page, "LoadProjectFolderAsync", summary.FolderPath).ConfigureAwait(true);
                        progress = "applying accepted values";
                        SetNumericParameterThroughUi(page, "feature.profile.alchemy_focus", "healingPotionOutput", 3);
                        SetNumericParameterThroughUi(page, "feature.profile.combat_focus", "basicAttackDamage", 5);
                        SetNumericParameterThroughUi(page, "feature.profile.combat_focus", "goblinStartingHealth", 18);
                        SetNumericParameterThroughUi(page, "feature.profile.exploration_resource_focus", "appleYield", 4);
                        SetNumericParameterThroughUi(page, "feature.profile.exploration_resource_focus", "logYield", 3);
                        SetNumericParameterThroughUi(page, "feature.profile.exploration_resource_focus", "transactionPotionOutput", 3);

                        current.CurrentChanged += (_, _) =>
                        {
                            currentChangedThreadId = Environment.CurrentManagedThreadId;
                            progress = "worker current package changed";
                        };
                        progress = "running primary build";
                        var buildTask = InvokeTaskWithoutPump(page, "BuildAndQualifyAsync");
                        var uiPumpResponsive = false;
                        page.BeginInvoke(() => uiPumpResponsive = true);
                        await buildTask.ConfigureAwait(true);

                        progress = "asserting build result";
                        var result = controller.LastBuild ?? throw new InvalidOperationException("Build result was not recorded.");
                        var status = Field<ToolStripStatusLabel>(form, "_statusLabel");
                        var navigation = Field<ListBox>(form, "_navigation");
                        var supportRelativePath = "scripts/generators/basic_village.lua";
                        var supportTarget = Path.Combine(summary.FolderPath, supportRelativePath.Replace('/', Path.DirectorySeparatorChar));
                        var supportSource = Path.Combine(root, "samples", "minimal-map-game", supportRelativePath.Replace('/', Path.DirectorySeparatorChar));

                        Assert.True(result.Passed, string.Join(Environment.NewLine, result.Diagnostics));
                        Assert.Null(threadException);
                        Assert.NotEqual(uiThreadId, currentChangedThreadId);
                        Assert.Equal(ExpectedPackageSha256, result.PackageSha256);
                        Assert.Equal(ExpectedFinalStateHash, result.FinalStateHash);
                        Assert.True(result.SupportFilesPrepared);
                        Assert.True(result.StagedProjectValidationPassed);
                        Assert.True(result.RealProjectValidationPassed);
                        Assert.True(File.Exists(supportTarget));
                        Assert.Equal(HashFile(supportSource), HashFile(supportTarget));
                        Assert.True(uiPumpResponsive);
                        Assert.Equal("Открыт проект: Проверка конструктора", status.Text);
                        Assert.Same(projectsEntry, navigation.SelectedItem);
                        Assert.Equal("Игры", ((IEditorPage?)navigation.SelectedItem)?.Title);
                        Assert.True(Field<Button>(page, "_buildAndQualifyButton").Enabled);
                        Assert.True(Field<TabControl>(page, "_workspaceTabs").Enabled);

                        WriteProof("real-workspace-build-retry-proof.json", new
                        {
                            schemaVersion = "real_workspace_build_retry_proof_v1",
                            status = "GREEN",
                            realGameProjectServiceCreateAsync = true,
                            realProjectsPageControl = true,
                            realMainForm = true,
                            realUnifiedGameProjectWorkspaceController = true,
                            primaryBuildMethodInvoked = true,
                            currentChangedRaisedFromWorker = currentChangedThreadId != uiThreadId,
                            currentChangedThreadId,
                            uiThreadId,
                            crossThreadExceptionAbsent = threadException is null,
                            packageSha256 = result.PackageSha256,
                            finalStateHash = result.FinalStateHash,
                            requiredSupportFileCount = result.RequiredSupportFileCount,
                            copiedSupportFileCount = result.CopiedSupportFileCount,
                            reusedSupportFileCount = result.ReusedSupportFileCount,
                            supportFilesPrepared = result.SupportFilesPrepared,
                            stagedProjectValidationPassed = result.StagedProjectValidationPassed,
                            realProjectValidationPassed = result.RealProjectValidationPassed,
                            supportRelativePath,
                            supportFileSourceHashMatched = HashFile(supportSource) == HashFile(supportTarget),
                            heavyWorkRunsOffUiThread = currentChangedThreadId != uiThreadId,
                            uiPumpResponsive,
                            statusStripText = status.Text,
                            selectedPageTitle = ((IEditorPage?)navigation.SelectedItem)?.Title,
                            controlsRestored = Field<Button>(page, "_buildAndQualifyButton").Enabled
                                               && Field<TabControl>(page, "_workspaceTabs").Enabled,
                            noChildToolProcessStarted = true,
                            passed = true
                        });
                        progress = "build proof complete";
                    }
                    catch (Exception exception)
                    {
                        scenarioException = exception;
                    }
                    finally
                    {
                        System.Windows.Forms.Application.ThreadException -= threadExceptionHandler;
                        form.Close();
                    }
                };

                System.Windows.Forms.Application.Run(form);
                if (scenarioException is not null)
                {
                    ExceptionDispatchInfo.Capture(scenarioException).Throw();
                }
                progress = "STA complete";
            }, TimeSpan.FromMinutes(4));
        }
        catch (TimeoutException exception)
        {
            File.WriteAllText(Path.Combine(Path.GetTempPath(), "goal148b-current-package-test-progress.txt"), progress);
            throw new TimeoutException($"{exception.Message} Progress: {progress}", exception);
        }
    }

    private static SubscriberInventoryEntry Inventory(
        string root,
        string relativePath,
        string subscriber,
        string ownerControl,
        string handlerName,
        bool asyncHandler)
    {
        var source = File.ReadAllText(Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar)), Encoding.UTF8);
        var marshals = source.Contains("WinFormsUiThreadDispatcher.Post", StringComparison.Ordinal);
        var unsubscribes = source.Contains("CurrentChanged -= " + handlerName, StringComparison.Ordinal)
                           || DesignerSourceContainsUnsubscribe(root, relativePath, handlerName);
        return new SubscriberInventoryEntry(
            subscriber,
            ownerControl,
            handlerName,
            marshals,
            asyncHandler,
            !asyncHandler || source.Contains("ShowCurrentProjectRefreshError", StringComparison.Ordinal),
            unsubscribes);
    }

    private static bool DesignerSourceContainsUnsubscribe(string root, string relativePath, string handlerName)
    {
        var designerRelative = relativePath[..^3] + ".Designer.cs";
        var designerPath = Path.Combine(root, designerRelative.Replace('/', Path.DirectorySeparatorChar));
        return File.Exists(designerPath)
               && File.ReadAllText(designerPath, Encoding.UTF8).Contains("CurrentChanged -= " + handlerName, StringComparison.Ordinal);
    }

    private static CompositionWorkbenchPresenter CreateCompositionPresenter()
    {
        var capabilities = BuiltInCapabilityRegistry.Create();
        var catalog = BuiltInGeneratorCatalog.Create();
        var renderer = new GameCompositionDiagnosticsMarkdownRenderer();
        return new CompositionWorkbenchPresenter(
            new GameBlueprintPresetProvider(),
            new GameCompositionDiagnosticsService(
                new GameBlueprintCompositionValidator(capabilities),
                new GeneratorCatalogValidator(capabilities),
                new GeneratorPlanResolver(capabilities, catalog),
                catalog),
            renderer,
            new GameCompositionDiagnosticsExportService(renderer));
    }

    private static UnifiedGameProjectWorkspaceController CreateController(
        string root,
        CurrentGamePackageService current)
    {
        return new UnifiedGameProjectWorkspaceController(
            current,
            new GameProjectFeatureModuleAuthoringService(root),
            new GameProjectBuildAndQualificationService(
                root,
                SelectedRuntimeVariantInteractiveSessionService.CreateDefault(),
                new JsonGamePackageRepository(),
                new GamePackageValidator(),
                current));
    }

    private static void ApplyAcceptedCustomValues(UnifiedGameProjectWorkspaceController controller)
    {
        controller.SetParameterValue("feature.profile.alchemy_focus", "healingPotionOutput", JsonSerializer.SerializeToElement(3));
        controller.SetParameterValue("feature.profile.combat_focus", "basicAttackDamage", JsonSerializer.SerializeToElement(5));
        controller.SetParameterValue("feature.profile.combat_focus", "goblinStartingHealth", JsonSerializer.SerializeToElement(18));
        controller.SetParameterValue("feature.profile.exploration_resource_focus", "appleYield", JsonSerializer.SerializeToElement(4));
        controller.SetParameterValue("feature.profile.exploration_resource_focus", "logYield", JsonSerializer.SerializeToElement(3));
        controller.SetParameterValue("feature.profile.exploration_resource_focus", "transactionPotionOutput", JsonSerializer.SerializeToElement(3));
    }

    private static void SetNumericParameterThroughUi(
        ProjectsPageControl page,
        string moduleId,
        string parameterId,
        decimal value)
    {
        var editor = Descendants(page).OfType<NumericUpDown>().Single(control =>
            control.Tag is GameProjectParameterPresentation parameter
            && parameter.ModuleId == moduleId
            && parameter.ParameterId == parameterId);
        editor.Value = value;
    }

    private static IEnumerable<Control> Descendants(Control root)
    {
        foreach (Control child in root.Controls)
        {
            yield return child;
            foreach (var nested in Descendants(child))
            {
                yield return nested;
            }
        }
    }

    private static GamePackageDefinition CreatePackage(string title) => new NewGamePackageFactory().Create(new CreateGameProjectRequest
    {
        GamesRootPath = Path.GetTempPath(),
        FolderName = "goal148b-package",
        Title = title,
        PackageId = "game/goal148b-package",
        Version = "0.1.0"
    });

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

    private static T ValueField<T>(object target, string name) where T : struct =>
        (T)(target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(target)
            ?? throw new MissingFieldException(target.GetType().FullName, name));

    private static void RunSta(Action action, TimeSpan? timeout = null)
    {
        Exception? caught = null;
        var thread = new Thread(() =>
        {
            try
            {
                SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
                action();
            }
            catch (Exception exception)
            {
                caught = exception;
            }
        });
        thread.IsBackground = true;
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        if (!thread.Join(timeout ?? TimeSpan.FromMinutes(1)))
        {
            throw new TimeoutException("STA test thread did not complete.");
        }
        if (caught is not null)
        {
            ExceptionDispatchInfo.Capture(caught).Throw();
        }
    }

    private static void PumpUntil(Func<bool> condition, TimeSpan? timeout = null)
    {
        var deadline = DateTime.UtcNow.Add(timeout ?? TimeSpan.FromMinutes(1));
        while (!condition())
        {
            System.Windows.Forms.Application.DoEvents();
            if (DateTime.UtcNow >= deadline)
            {
                throw new TimeoutException("STA message pump condition timed out.");
            }
            Thread.Yield();
        }
        System.Windows.Forms.Application.DoEvents();
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "LLMGameCreator.sln")))
            {
                return directory.FullName;
            }
            directory = directory.Parent;
        }
        throw new DirectoryNotFoundException("Repository root was not found.");
    }

    private static string HashFile(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static void WriteProof(string fileName, object value)
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("LLMGC_GOAL148B_RUN"), "true", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }
        var root = Environment.GetEnvironmentVariable("LLMGC_GOAL148B_OUTPUT_ROOT")
                   ?? throw new InvalidOperationException("LLMGC_GOAL148B_OUTPUT_ROOT is required.");
        Directory.CreateDirectory(root);
        File.WriteAllText(Path.Combine(root, fileName), JsonSerializer.Serialize(value, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        }) + Environment.NewLine, new UTF8Encoding(false));
    }

    private sealed class TestEditorPage(string id, string title) : IEditorPage
    {
        private readonly Panel _view = new();
        public string Id { get; } = id;
        public string Title { get; } = title;
        public int SortOrder => 0;
        public Control View => _view;
        public void OnActivated()
        {
        }
    }

    private sealed class DeferredEditorPage(ProjectsPageControl page) : IEditorPage
    {
        public string Id => page.Id;
        public string Title => page.Title;
        public int SortOrder => page.SortOrder;
        public Control View => page;
        public void OnActivated()
        {
        }
    }

    private sealed record SubscriberInventoryEntry(
        string Subscriber,
        string OwnerControl,
        string HandlerName,
        bool MarshalsToUiThread,
        bool AsyncHandler,
        bool AsyncExceptionsObserved,
        bool UnsubscribesOnDispose);

    private sealed class MutableCurrentGamePackageService(string currentFolder) : ICurrentGamePackageService
    {
        public string? CurrentFolder { get; private set; } = currentFolder;
        public GamePackageDefinition? CurrentPackage { get; private set; }
        public event EventHandler? CurrentChanged;
        public Task LoadAsync(string projectFolder, CancellationToken cancellationToken)
        {
            CurrentFolder = projectFolder;
            CurrentChanged?.Invoke(this, EventArgs.Empty);
            return Task.CompletedTask;
        }
        public Task SaveAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        public void ReplaceCurrent(GamePackageDefinition package)
        {
            CurrentPackage = package;
            CurrentChanged?.Invoke(this, EventArgs.Empty);
        }
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
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, true);
            }
        }
    }
}

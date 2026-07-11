using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Windows.Forms;
using LLMGameCreator.Application.Design.ProductLineInteractiveSessionMatrix;
using LLMGameCreator.WinForms.Pages;
using Xunit;

namespace LLMGameCreator.Tests.WinForms;

public sealed class Goal145CandidateSelectorBindingTests
{
    [Fact]
    public void Programmatic_binding_is_silent_and_operator_commit_is_single_and_stable()
    {
        RunSta(() =>
        {
            using var page = new VisualWorldStreamPreviewWorkspacePageControl();
            var controller = RequiredPrivateField<ProductLineInteractiveSessionSelectionController>(
                page,
                "_goal145Controller");
            var candidates = RequiredPrivateField<ComboBox>(page, "_goal145Candidates");
            var committedEventCount = 0;
            var currentCallbackDepth = 0;
            var maximumCallbackDepth = 0;
            candidates.SelectionChangeCommitted += (_, _) =>
            {
                currentCallbackDepth++;
                maximumCallbackDepth = Math.Max(maximumCallbackDepth, currentCallbackDepth);
                committedEventCount++;
                currentCallbackDepth--;
            };

            controller.LoadCandidateMatrix(ProjectRoot());
            InvokePrivate(page, "BindGoal145VariantSessions");

            Assert.Equal(0, committedEventCount);
            Assert.Equal("minimal-map-game-exploration-resource-focus", controller.SelectedCandidateId);
            Assert.Equal(controller.SelectedCandidateId, candidates.SelectedValue);

            candidates.SelectedValue = "minimal-map-game-combat-focus";
            Assert.Equal(0, committedEventCount);
            Assert.Equal("minimal-map-game-exploration-resource-focus", controller.SelectedCandidateId);
            InvokePrivate(page, "BindGoal145VariantSessions");

            Assert.Equal(0, committedEventCount);
            Assert.Equal(controller.SelectedCandidateId, candidates.SelectedValue);

            candidates.SelectedValue = "minimal-map-game-combat-focus";
            RaiseSelectionChangeCommitted(candidates);

            Assert.Equal(1, committedEventCount);
            Assert.Equal(1, maximumCallbackDepth);
            Assert.Equal("minimal-map-game-combat-focus", controller.SelectedCandidateId);
            Assert.Equal(controller.SelectedCandidateId, candidates.SelectedValue);
            Assert.Equal(1, RequiredPrivateField<int>(page, "_goal145OperatorCommitSelectionCount"));
            Assert.Equal(1, RequiredPrivateField<int>(page, "_goal145MaximumSelectionCallbackDepth"));

            var session = controller.StartSelected();
            InvokePrivate(page, "BindGoal145VariantSessions");
            controller.ExecuteSelectedAction("start_runtime");
            InvokePrivate(page, "BindGoal145VariantSessions");
            controller.SaveCheckpoint();
            InvokePrivate(page, "BindGoal145VariantSessions");
            controller.ReloadCheckpoint();
            InvokePrivate(page, "BindGoal145VariantSessions");
            controller.ReplayVerify();
            InvokePrivate(page, "BindGoal145VariantSessions");
            controller.LoadCandidateMatrix(ProjectRoot());
            InvokePrivate(page, "BindGoal145VariantSessions");

            Assert.Equal(1, committedEventCount);
            Assert.Equal("minimal-map-game-combat-focus", controller.SelectedCandidateId);
            Assert.Equal(controller.SelectedCandidateId, candidates.SelectedValue);
            Assert.Equal(session.PackageSha256, controller.Session!.PackageSha256);
        });
    }

    private static void RaiseSelectionChangeCommitted(ComboBox comboBox)
    {
        var method = typeof(ComboBox).GetMethod(
            "OnSelectionChangeCommitted",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(method);
        method!.Invoke(comboBox, [EventArgs.Empty]);
    }

    private static void InvokePrivate(object owner, string methodName)
    {
        var method = owner.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(method);
        method!.Invoke(owner, null);
    }

    private static T RequiredPrivateField<T>(object owner, string fieldName)
    {
        var field = owner.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field);
        var value = field!.GetValue(owner);
        Assert.NotNull(value);
        return Assert.IsType<T>(value);
    }

    private static void RunSta(Action action)
    {
        Exception? caught = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                caught = ex;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (caught is not null) ExceptionDispatchInfo.Capture(caught).Throw();
    }

    private static string ProjectRoot()
    {
        var current = Path.GetFullPath(AppContext.BaseDirectory);
        while (!string.IsNullOrWhiteSpace(current))
        {
            if (File.Exists(Path.Combine(current, "LLMGameCreator.sln"))) return current;
            current = Directory.GetParent(current)?.FullName ?? string.Empty;
        }

        throw new InvalidOperationException("Repository root was not found.");
    }
}

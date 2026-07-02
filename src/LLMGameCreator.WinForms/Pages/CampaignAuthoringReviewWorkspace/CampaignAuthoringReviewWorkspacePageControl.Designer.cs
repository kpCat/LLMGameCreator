#nullable disable

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LLMGameCreator.WinForms.Pages
{
    partial class CampaignAuthoringReviewWorkspacePageControl
    {
        private IContainer components;
        private Label _statusLabel;
        private TabControl _tabs;
        private TabPage _rowsTabPage;
        private TabPage _schemaTabPage;
        private TabPage _diagnosticsTabPage;
        private TabPage _provenanceTabPage;
        private TabPage _actionPlanTabPage;
        private TabPage _qualityTabPage;
        private TabPage _editLoopTabPage;
        private TabPage _playableRefreshTabPage;
        private TabPage _reviewPackageTabPage;
        private TabPage _playSessionTabPage;
        private TabPage _spineQualityTabPage;
        private TabPage _runtimePreviewBridgeTabPage;
        private TabPage _runtimePreviewPlaythroughTabPage;
        private CampaignRowSelectorControl _rowSelectorControl;
        private CampaignSchemaGroupControl _schemaGroupControl;
        private CampaignDiagnosticsControl _diagnosticsControl;
        private CampaignProvenanceControl _provenanceControl;
        private CampaignActionPlanControl _actionPlanControl;
        private CampaignQualityGateControl _qualityGateControl;
        private CampaignEditValidateApplyLoopControl _editLoopControl;
        private CampaignPlayableRefreshControl _playableRefreshControl;
        private CampaignReviewPackageControl _reviewPackageControl;
        private CampaignReviewPackagePlaySessionControl _playSessionControl;
        private CampaignEditDrivenSpineQualityControl _spineQualityControl;
        private CampaignGamePackageRuntimePreviewBridgeControl _runtimePreviewBridgeControl;
        private CampaignGamePackageRuntimePreviewPlaythroughControl _runtimePreviewPlaythroughControl;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new Container();
            this._statusLabel = new Label();
            this._tabs = new TabControl();
            this._rowsTabPage = new TabPage();
            this._schemaTabPage = new TabPage();
            this._diagnosticsTabPage = new TabPage();
            this._provenanceTabPage = new TabPage();
            this._actionPlanTabPage = new TabPage();
            this._qualityTabPage = new TabPage();
            this._editLoopTabPage = new TabPage();
            this._playableRefreshTabPage = new TabPage();
            this._reviewPackageTabPage = new TabPage();
            this._playSessionTabPage = new TabPage();
            this._spineQualityTabPage = new TabPage();
            this._runtimePreviewBridgeTabPage = new TabPage();
            this._runtimePreviewPlaythroughTabPage = new TabPage();
            this._rowSelectorControl = new CampaignRowSelectorControl();
            this._schemaGroupControl = new CampaignSchemaGroupControl();
            this._diagnosticsControl = new CampaignDiagnosticsControl();
            this._provenanceControl = new CampaignProvenanceControl();
            this._actionPlanControl = new CampaignActionPlanControl();
            this._qualityGateControl = new CampaignQualityGateControl();
            this._editLoopControl = new CampaignEditValidateApplyLoopControl();
            this._playableRefreshControl = new CampaignPlayableRefreshControl();
            this._reviewPackageControl = new CampaignReviewPackageControl();
            this._playSessionControl = new CampaignReviewPackagePlaySessionControl();
            this._spineQualityControl = new CampaignEditDrivenSpineQualityControl();
            this._runtimePreviewBridgeControl = new CampaignGamePackageRuntimePreviewBridgeControl();
            this._runtimePreviewPlaythroughControl = new CampaignGamePackageRuntimePreviewPlaythroughControl();
            this._tabs.SuspendLayout();
            this._rowsTabPage.SuspendLayout();
            this._schemaTabPage.SuspendLayout();
            this._diagnosticsTabPage.SuspendLayout();
            this._provenanceTabPage.SuspendLayout();
            this._actionPlanTabPage.SuspendLayout();
            this._qualityTabPage.SuspendLayout();
            this._editLoopTabPage.SuspendLayout();
            this._playableRefreshTabPage.SuspendLayout();
            this._reviewPackageTabPage.SuspendLayout();
            this._playSessionTabPage.SuspendLayout();
            this._spineQualityTabPage.SuspendLayout();
            this._runtimePreviewBridgeTabPage.SuspendLayout();
            this._runtimePreviewPlaythroughTabPage.SuspendLayout();
            this.SuspendLayout();
            //
            // _statusLabel
            //
            this._statusLabel.Dock = DockStyle.Top;
            this._statusLabel.Location = new Point(0, 0);
            this._statusLabel.Name = "_statusLabel";
            this._statusLabel.Padding = new Padding(8, 6, 8, 6);
            this._statusLabel.Size = new Size(1100, 34);
            this._statusLabel.TabIndex = 0;
            this._statusLabel.Text = "Campaign authoring review workspace";
            //
            // _tabs
            //
            this._tabs.Controls.Add(this._rowsTabPage);
            this._tabs.Controls.Add(this._schemaTabPage);
            this._tabs.Controls.Add(this._diagnosticsTabPage);
            this._tabs.Controls.Add(this._provenanceTabPage);
            this._tabs.Controls.Add(this._actionPlanTabPage);
            this._tabs.Controls.Add(this._qualityTabPage);
            this._tabs.Controls.Add(this._editLoopTabPage);
            this._tabs.Controls.Add(this._playableRefreshTabPage);
            this._tabs.Controls.Add(this._reviewPackageTabPage);
            this._tabs.Controls.Add(this._playSessionTabPage);
            this._tabs.Controls.Add(this._spineQualityTabPage);
            this._tabs.Controls.Add(this._runtimePreviewBridgeTabPage);
            this._tabs.Controls.Add(this._runtimePreviewPlaythroughTabPage);
            this._tabs.Dock = DockStyle.Fill;
            this._tabs.Location = new Point(0, 34);
            this._tabs.Name = "_tabs";
            this._tabs.SelectedIndex = 0;
            this._tabs.Size = new Size(1100, 686);
            this._tabs.TabIndex = 1;
            //
            // _rowsTabPage
            //
            this._rowsTabPage.Controls.Add(this._rowSelectorControl);
            this._rowsTabPage.Location = new Point(4, 24);
            this._rowsTabPage.Name = "_rowsTabPage";
            this._rowsTabPage.Padding = new Padding(3);
            this._rowsTabPage.Size = new Size(1092, 658);
            this._rowsTabPage.TabIndex = 0;
            this._rowsTabPage.Text = "Rows";
            this._rowsTabPage.UseVisualStyleBackColor = true;
            //
            // _schemaTabPage
            //
            this._schemaTabPage.Controls.Add(this._schemaGroupControl);
            this._schemaTabPage.Location = new Point(4, 24);
            this._schemaTabPage.Name = "_schemaTabPage";
            this._schemaTabPage.Padding = new Padding(3);
            this._schemaTabPage.Size = new Size(1092, 658);
            this._schemaTabPage.TabIndex = 1;
            this._schemaTabPage.Text = "Schema";
            this._schemaTabPage.UseVisualStyleBackColor = true;
            //
            // _diagnosticsTabPage
            //
            this._diagnosticsTabPage.Controls.Add(this._diagnosticsControl);
            this._diagnosticsTabPage.Location = new Point(4, 24);
            this._diagnosticsTabPage.Name = "_diagnosticsTabPage";
            this._diagnosticsTabPage.Size = new Size(1092, 658);
            this._diagnosticsTabPage.TabIndex = 2;
            this._diagnosticsTabPage.Text = "Diagnostics";
            this._diagnosticsTabPage.UseVisualStyleBackColor = true;
            //
            // _provenanceTabPage
            //
            this._provenanceTabPage.Controls.Add(this._provenanceControl);
            this._provenanceTabPage.Location = new Point(4, 24);
            this._provenanceTabPage.Name = "_provenanceTabPage";
            this._provenanceTabPage.Size = new Size(1092, 658);
            this._provenanceTabPage.TabIndex = 3;
            this._provenanceTabPage.Text = "Provenance";
            this._provenanceTabPage.UseVisualStyleBackColor = true;
            //
            // _actionPlanTabPage
            //
            this._actionPlanTabPage.Controls.Add(this._actionPlanControl);
            this._actionPlanTabPage.Location = new Point(4, 24);
            this._actionPlanTabPage.Name = "_actionPlanTabPage";
            this._actionPlanTabPage.Size = new Size(1092, 658);
            this._actionPlanTabPage.TabIndex = 4;
            this._actionPlanTabPage.Text = "Action Plan";
            this._actionPlanTabPage.UseVisualStyleBackColor = true;
            //
            // _qualityTabPage
            //
            this._qualityTabPage.Controls.Add(this._qualityGateControl);
            this._qualityTabPage.Location = new Point(4, 24);
            this._qualityTabPage.Name = "_qualityTabPage";
            this._qualityTabPage.Size = new Size(1092, 658);
            this._qualityTabPage.TabIndex = 5;
            this._qualityTabPage.Text = "Quality";
            this._qualityTabPage.UseVisualStyleBackColor = true;
            //
            // _editLoopTabPage
            //
            this._editLoopTabPage.Controls.Add(this._editLoopControl);
            this._editLoopTabPage.Location = new Point(4, 24);
            this._editLoopTabPage.Name = "_editLoopTabPage";
            this._editLoopTabPage.Size = new Size(1092, 658);
            this._editLoopTabPage.TabIndex = 6;
            this._editLoopTabPage.Text = "Edit Loop";
            this._editLoopTabPage.UseVisualStyleBackColor = true;
            //
            // _playableRefreshTabPage
            //
            this._playableRefreshTabPage.Controls.Add(this._playableRefreshControl);
            this._playableRefreshTabPage.Location = new Point(4, 24);
            this._playableRefreshTabPage.Name = "_playableRefreshTabPage";
            this._playableRefreshTabPage.Size = new Size(1092, 658);
            this._playableRefreshTabPage.TabIndex = 7;
            this._playableRefreshTabPage.Text = "Playable Refresh";
            this._playableRefreshTabPage.UseVisualStyleBackColor = true;
            //
            // _reviewPackageTabPage
            //
            this._reviewPackageTabPage.Controls.Add(this._reviewPackageControl);
            this._reviewPackageTabPage.Location = new Point(4, 24);
            this._reviewPackageTabPage.Name = "_reviewPackageTabPage";
            this._reviewPackageTabPage.Size = new Size(1092, 658);
            this._reviewPackageTabPage.TabIndex = 8;
            this._reviewPackageTabPage.Text = "Review Package";
            this._reviewPackageTabPage.UseVisualStyleBackColor = true;
            //
            // _playSessionTabPage
            //
            this._playSessionTabPage.Controls.Add(this._playSessionControl);
            this._playSessionTabPage.Location = new Point(4, 24);
            this._playSessionTabPage.Name = "_playSessionTabPage";
            this._playSessionTabPage.Size = new Size(1092, 658);
            this._playSessionTabPage.TabIndex = 9;
            this._playSessionTabPage.Text = "Play Session";
            this._playSessionTabPage.UseVisualStyleBackColor = true;
            //
            // _spineQualityTabPage
            //
            this._spineQualityTabPage.Controls.Add(this._spineQualityControl);
            this._spineQualityTabPage.Location = new Point(4, 24);
            this._spineQualityTabPage.Name = "_spineQualityTabPage";
            this._spineQualityTabPage.Size = new Size(1092, 658);
            this._spineQualityTabPage.TabIndex = 10;
            this._spineQualityTabPage.Text = "Spine Quality";
            this._spineQualityTabPage.UseVisualStyleBackColor = true;
            //
            // _runtimePreviewBridgeTabPage
            //
            this._runtimePreviewBridgeTabPage.Controls.Add(this._runtimePreviewBridgeControl);
            this._runtimePreviewBridgeTabPage.Location = new Point(4, 24);
            this._runtimePreviewBridgeTabPage.Name = "_runtimePreviewBridgeTabPage";
            this._runtimePreviewBridgeTabPage.Size = new Size(1092, 658);
            this._runtimePreviewBridgeTabPage.TabIndex = 11;
            this._runtimePreviewBridgeTabPage.Text = "Runtime Bridge";
            this._runtimePreviewBridgeTabPage.UseVisualStyleBackColor = true;
            //
            // _runtimePreviewPlaythroughTabPage
            //
            this._runtimePreviewPlaythroughTabPage.Controls.Add(this._runtimePreviewPlaythroughControl);
            this._runtimePreviewPlaythroughTabPage.Location = new Point(4, 24);
            this._runtimePreviewPlaythroughTabPage.Name = "_runtimePreviewPlaythroughTabPage";
            this._runtimePreviewPlaythroughTabPage.Size = new Size(1092, 658);
            this._runtimePreviewPlaythroughTabPage.TabIndex = 12;
            this._runtimePreviewPlaythroughTabPage.Text = "Preview Playthrough";
            this._runtimePreviewPlaythroughTabPage.UseVisualStyleBackColor = true;
            //
            // child controls
            //
            this._rowSelectorControl.Dock = DockStyle.Fill;
            this._schemaGroupControl.Dock = DockStyle.Fill;
            this._diagnosticsControl.Dock = DockStyle.Fill;
            this._provenanceControl.Dock = DockStyle.Fill;
            this._actionPlanControl.Dock = DockStyle.Fill;
            this._qualityGateControl.Dock = DockStyle.Fill;
            this._editLoopControl.Dock = DockStyle.Fill;
            this._playableRefreshControl.Dock = DockStyle.Fill;
            this._reviewPackageControl.Dock = DockStyle.Fill;
            this._playSessionControl.Dock = DockStyle.Fill;
            this._spineQualityControl.Dock = DockStyle.Fill;
            this._runtimePreviewBridgeControl.Dock = DockStyle.Fill;
            this._runtimePreviewPlaythroughControl.Dock = DockStyle.Fill;
            //
            // CampaignAuthoringReviewWorkspacePageControl
            //
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(this._tabs);
            this.Controls.Add(this._statusLabel);
            this.Name = "CampaignAuthoringReviewWorkspacePageControl";
            this.Size = new Size(1100, 720);
            this._tabs.ResumeLayout(false);
            this._rowsTabPage.ResumeLayout(false);
            this._schemaTabPage.ResumeLayout(false);
            this._diagnosticsTabPage.ResumeLayout(false);
            this._provenanceTabPage.ResumeLayout(false);
            this._actionPlanTabPage.ResumeLayout(false);
            this._qualityTabPage.ResumeLayout(false);
            this._editLoopTabPage.ResumeLayout(false);
            this._playableRefreshTabPage.ResumeLayout(false);
            this._reviewPackageTabPage.ResumeLayout(false);
            this._playSessionTabPage.ResumeLayout(false);
            this._spineQualityTabPage.ResumeLayout(false);
            this._runtimePreviewBridgeTabPage.ResumeLayout(false);
            this._runtimePreviewPlaythroughTabPage.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}

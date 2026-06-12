#nullable disable

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LLMGameCreator.WinForms.Pages
{
    partial class GeneratorLibraryPageControl
    {
        private IContainer components;
        private TabControl _tabs;
        private TabPage _importTabPage;
        private TabPage _modulesTabPage;
        private TabPage _capabilitiesTabPage;
        private TabPage _integrityTabPage;
        private TabPage _issuesTabPage;
        private GeneratorLibraryImportTabControl _importTab;
        private GeneratorLibraryModulesTabControl _modulesTab;
        private GeneratorLibraryCapabilitiesTabControl _capabilitiesTab;
        private GeneratorLibraryIntegrityTabControl _integrityTab;
        private GeneratorLibraryIssuesTabControl _issuesTab;

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
            this._tabs = new TabControl();
            this._importTabPage = new TabPage();
            this._modulesTabPage = new TabPage();
            this._capabilitiesTabPage = new TabPage();
            this._integrityTabPage = new TabPage();
            this._issuesTabPage = new TabPage();
            this._importTab = new GeneratorLibraryImportTabControl();
            this._modulesTab = new GeneratorLibraryModulesTabControl();
            this._capabilitiesTab = new GeneratorLibraryCapabilitiesTabControl();
            this._integrityTab = new GeneratorLibraryIntegrityTabControl();
            this._issuesTab = new GeneratorLibraryIssuesTabControl();
            this._tabs.SuspendLayout();
            this._importTabPage.SuspendLayout();
            this._modulesTabPage.SuspendLayout();
            this._capabilitiesTabPage.SuspendLayout();
            this._integrityTabPage.SuspendLayout();
            this._issuesTabPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // _tabs
            // 
            this._tabs.Controls.Add(this._importTabPage);
            this._tabs.Controls.Add(this._modulesTabPage);
            this._tabs.Controls.Add(this._capabilitiesTabPage);
            this._tabs.Controls.Add(this._integrityTabPage);
            this._tabs.Controls.Add(this._issuesTabPage);
            this._tabs.Dock = DockStyle.Fill;
            this._tabs.Location = new Point(0, 0);
            this._tabs.Name = "_tabs";
            this._tabs.SelectedIndex = 0;
            this._tabs.Size = new Size(800, 450);
            this._tabs.TabIndex = 0;
            // 
            // _importTabPage
            // 
            this._importTabPage.Controls.Add(this._importTab);
            this._importTabPage.Location = new Point(4, 24);
            this._importTabPage.Name = "_importTabPage";
            this._importTabPage.Padding = new Padding(3);
            this._importTabPage.Size = new Size(792, 422);
            this._importTabPage.TabIndex = 0;
            this._importTabPage.Text = "Import";
            this._importTabPage.UseVisualStyleBackColor = true;
            // 
            // _modulesTabPage
            // 
            this._modulesTabPage.Controls.Add(this._modulesTab);
            this._modulesTabPage.Location = new Point(4, 24);
            this._modulesTabPage.Name = "_modulesTabPage";
            this._modulesTabPage.Padding = new Padding(3);
            this._modulesTabPage.Size = new Size(792, 422);
            this._modulesTabPage.TabIndex = 1;
            this._modulesTabPage.Text = "Modules";
            this._modulesTabPage.UseVisualStyleBackColor = true;
            // 
            // _capabilitiesTabPage
            // 
            this._capabilitiesTabPage.Controls.Add(this._capabilitiesTab);
            this._capabilitiesTabPage.Location = new Point(4, 24);
            this._capabilitiesTabPage.Name = "_capabilitiesTabPage";
            this._capabilitiesTabPage.Padding = new Padding(3);
            this._capabilitiesTabPage.Size = new Size(792, 422);
            this._capabilitiesTabPage.TabIndex = 2;
            this._capabilitiesTabPage.Text = "Capabilities";
            this._capabilitiesTabPage.UseVisualStyleBackColor = true;
            // 
            // _integrityTabPage
            // 
            this._integrityTabPage.Controls.Add(this._integrityTab);
            this._integrityTabPage.Location = new Point(4, 24);
            this._integrityTabPage.Name = "_integrityTabPage";
            this._integrityTabPage.Padding = new Padding(3);
            this._integrityTabPage.Size = new Size(792, 422);
            this._integrityTabPage.TabIndex = 3;
            this._integrityTabPage.Text = "Integrity";
            this._integrityTabPage.UseVisualStyleBackColor = true;
            // 
            // _issuesTabPage
            // 
            this._issuesTabPage.Controls.Add(this._issuesTab);
            this._issuesTabPage.Location = new Point(4, 24);
            this._issuesTabPage.Name = "_issuesTabPage";
            this._issuesTabPage.Padding = new Padding(3);
            this._issuesTabPage.Size = new Size(792, 422);
            this._issuesTabPage.TabIndex = 4;
            this._issuesTabPage.Text = "Import Issues";
            this._issuesTabPage.UseVisualStyleBackColor = true;
            // 
            // child controls
            // 
            this._importTab.Dock = DockStyle.Fill;
            this._importTab.Location = new Point(3, 3);
            this._importTab.Name = "_importTab";
            this._importTab.Size = new Size(786, 416);
            this._importTab.TabIndex = 0;
            this._modulesTab.Dock = DockStyle.Fill;
            this._modulesTab.Location = new Point(3, 3);
            this._modulesTab.Name = "_modulesTab";
            this._modulesTab.Size = new Size(786, 416);
            this._modulesTab.TabIndex = 0;
            this._capabilitiesTab.Dock = DockStyle.Fill;
            this._capabilitiesTab.Location = new Point(3, 3);
            this._capabilitiesTab.Name = "_capabilitiesTab";
            this._capabilitiesTab.Size = new Size(786, 416);
            this._capabilitiesTab.TabIndex = 0;
            this._integrityTab.Dock = DockStyle.Fill;
            this._integrityTab.Location = new Point(3, 3);
            this._integrityTab.Name = "_integrityTab";
            this._integrityTab.Size = new Size(786, 416);
            this._integrityTab.TabIndex = 0;
            this._issuesTab.Dock = DockStyle.Fill;
            this._issuesTab.Location = new Point(3, 3);
            this._issuesTab.Name = "_issuesTab";
            this._issuesTab.Size = new Size(786, 416);
            this._issuesTab.TabIndex = 0;
            // 
            // GeneratorLibraryPageControl
            // 
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(this._tabs);
            this.Name = "GeneratorLibraryPageControl";
            this.Size = new Size(800, 450);
            this._tabs.ResumeLayout(false);
            this._importTabPage.ResumeLayout(false);
            this._modulesTabPage.ResumeLayout(false);
            this._capabilitiesTabPage.ResumeLayout(false);
            this._integrityTabPage.ResumeLayout(false);
            this._issuesTabPage.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}

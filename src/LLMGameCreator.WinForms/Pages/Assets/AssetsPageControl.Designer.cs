#nullable disable

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LLMGameCreator.WinForms.Pages
{
    partial class AssetsPageControl
    {
        private IContainer components;
        private ListView _listView;
        private ColumnHeader _idColumnHeader;
        private ColumnHeader _typeColumnHeader;
        private ColumnHeader _pathColumnHeader;
        private ColumnHeader _contractColumnHeader;

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
            this._listView = new ListView();
            this._idColumnHeader = new ColumnHeader();
            this._typeColumnHeader = new ColumnHeader();
            this._pathColumnHeader = new ColumnHeader();
            this._contractColumnHeader = new ColumnHeader();
            this.SuspendLayout();
            // 
            // _listView
            // 
            this._listView.Columns.AddRange(new ColumnHeader[]
            {
                this._idColumnHeader,
                this._typeColumnHeader,
                this._pathColumnHeader,
                this._contractColumnHeader
            });
            this._listView.Dock = DockStyle.Fill;
            this._listView.FullRowSelect = true;
            this._listView.Location = new Point(0, 0);
            this._listView.Name = "_listView";
            this._listView.Size = new Size(800, 450);
            this._listView.TabIndex = 0;
            this._listView.UseCompatibleStateImageBehavior = false;
            this._listView.View = View.Details;
            // 
            // _idColumnHeader
            // 
            this._idColumnHeader.Text = "Id";
            this._idColumnHeader.Width = 280;
            // 
            // _typeColumnHeader
            // 
            this._typeColumnHeader.Text = "Type";
            this._typeColumnHeader.Width = 160;
            // 
            // _pathColumnHeader
            // 
            this._pathColumnHeader.Text = "Path";
            this._pathColumnHeader.Width = 380;
            // 
            // _contractColumnHeader
            // 
            this._contractColumnHeader.Text = "Contract";
            this._contractColumnHeader.Width = 180;
            // 
            // AssetsPageControl
            // 
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Controls.Add(this._listView);
            this.Name = "AssetsPageControl";
            this.Size = new Size(800, 450);
            this.ResumeLayout(false);
        }
    }
}

namespace LLMGameCreator.WinForms.Pages;
partial class GeneratedCampaignSavePickerDialog
{
    private ListBox _list = null!; private Button _continue = null!; private Button _migrate = null!;
    private void InitializeComponent(){_list=new ListBox{Dock=DockStyle.Fill};_continue=new Button{Text="Продолжить",AutoSize=true};_migrate=new Button{Text="Перенести и продолжить",AutoSize=true};var buttons=new FlowLayoutPanel{Dock=DockStyle.Bottom,AutoSize=true};buttons.Controls.AddRange([_continue,_migrate,new Button{Text="Отмена",DialogResult=DialogResult.Cancel,AutoSize=true}]);Controls.Add(_list);Controls.Add(buttons);ClientSize=new Size(620,360);Text="Сохранения кампании";_continue.Click+=ContinueClick;_migrate.Click+=MigrateClick;}
}

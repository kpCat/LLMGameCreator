namespace LLMGameCreator.Application.Projects;

public sealed class CreateGameProjectRequest
{
    public string GamesRootPath { get; set; } = string.Empty;
    public string FolderName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string PackageId { get; set; } = string.Empty;
    public string Version { get; set; } = "0.1.0";
}

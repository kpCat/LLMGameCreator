namespace LLMGameCreator.Application.Design.Atlas;

public static class AtlasDiagnosticSeverity
{
    public const string Info = "info";
    public const string Warning = "warning";
    public const string Error = "error";
}

public static class AtlasDiagnosticCodes
{
    public const string LoadedFile = "atlas.loaded_file";
    public const string DiscoveredExample = "atlas.discovered_example";
    public const string MissingRoot = "atlas.missing_root";
    public const string MissingKnownFile = "atlas.missing_known_file";
    public const string ExamplesRootNotFound = "atlas.examples_root_not_found";
    public const string InvalidJson = "atlas.invalid_json";
    public const string ReadFailed = "atlas.read_failed";
    public const string MissingIdentity = "atlas.missing_identity";
    public const string MissingTitle = "atlas.missing_title";
    public const string MissingPurpose = "atlas.missing_purpose";
    public const string DuplicateId = "atlas.duplicate_id";
    public const string ExampleWithoutSteps = "atlas.example_without_steps";
    public const string ExampleUnknownProfileReference = "atlas.example_unknown_profile_reference";
    public const string ReferenceUnknown = "atlas.reference_unknown";
    public const string PathOutsideAtlasRoot = "atlas.path_outside_atlas_root";
}

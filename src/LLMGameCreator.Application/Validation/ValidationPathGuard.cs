namespace LLMGameCreator.Application.Validation;

internal static class ValidationPathGuard
{
    public static bool ContainsPathTraversal(string path)
    {
        return path
            .Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries)
            .Any(segment => segment == "..");
    }

    public static bool TryResolveInsideProject(string projectFolder, string relativePath, out string resolvedPath)
    {
        resolvedPath = string.Empty;

        try
        {
            var projectRoot = Path.GetFullPath(projectFolder);
            var projectRootWithSeparator = EnsureTrailingSeparator(projectRoot);
            var candidate = Path.GetFullPath(Path.Combine(projectRoot, relativePath));

            if (!candidate.StartsWith(projectRootWithSeparator, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(candidate, projectRoot, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            resolvedPath = candidate;
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (NotSupportedException)
        {
            return false;
        }
        catch (PathTooLongException)
        {
            return false;
        }
    }

    private static string EnsureTrailingSeparator(string path)
    {
        if (path.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal) ||
            path.EndsWith(Path.AltDirectorySeparatorChar.ToString(), StringComparison.Ordinal))
        {
            return path;
        }

        return path + Path.DirectorySeparatorChar;
    }
}

using System.Text.Json;
using LLMGameCreator.Application.Design;
using Microsoft.Data.Sqlite;

namespace LLMGameCreator.Infrastructure.Storage;

public sealed class SqliteDesignDatabase : IDesignDatabaseInitializer, IDesignKnowledgeRepository, IGeneratorLibraryRegistry
{
    private string? _databasePath;

    public async Task InitializeAsync(string databasePath, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(databasePath))
        {
            throw new ArgumentException("Database path must not be empty.", nameof(databasePath));
        }

        _databasePath = Path.GetFullPath(databasePath);
        Directory.CreateDirectory(Path.GetDirectoryName(_databasePath) ?? ".");

        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        var currentVersion = await GetUserVersionAsync(connection, cancellationToken).ConfigureAwait(false);
        if (currentVersion > SqliteDesignSchema.Version)
        {
            throw new InvalidOperationException($"Unsupported design DB schema version {currentVersion}.");
        }

        await using var transaction = await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        foreach (var statement in SqliteDesignSchema.Statements)
        {
            await ExecuteAsync(connection, transaction, statement, cancellationToken).ConfigureAwait(false);
        }

        await ExecuteAsync(connection, transaction, "INSERT OR REPLACE INTO design_metadata(key, value) VALUES ('schema_version', '1');", cancellationToken).ConfigureAwait(false);
        await ExecuteAsync(connection, transaction, "INSERT OR IGNORE INTO design_metadata(key, value) VALUES ('initialized_utc', $value);", cancellationToken, ("$value", DateTimeOffset.UtcNow.ToString("O"))).ConfigureAwait(false);
        await ExecuteAsync(connection, transaction, $"PRAGMA user_version = {SqliteDesignSchema.Version};", cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<DesignDatabaseInfo> GetInfoAsync(CancellationToken cancellationToken)
    {
        EnsureInitialized();
        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        var version = await GetUserVersionAsync(connection, cancellationToken).ConfigureAwait(false);
        var initialized = await GetMetadataValueAsync(connection, "initialized_utc", cancellationToken).ConfigureAwait(false);
        var initializedUtc = DateTimeOffset.TryParse(initialized, out var parsed) ? parsed : DateTimeOffset.MinValue;
        return new DesignDatabaseInfo(_databasePath!, version, initializedUtc);
    }

    public async Task UpsertKnowledgeItemAsync(DesignKnowledgeItem item, CancellationToken cancellationToken)
    {
        const string sql = """
        INSERT INTO knowledge_items(id, kind, title, body, source, confidence, status, metadata_json, created_utc, updated_utc)
        VALUES ($id, $kind, $title, $body, $source, $confidence, $status, $metadata_json, $created_utc, $updated_utc)
        ON CONFLICT(id) DO UPDATE SET
            kind = excluded.kind,
            title = excluded.title,
            body = excluded.body,
            source = excluded.source,
            confidence = excluded.confidence,
            status = excluded.status,
            metadata_json = excluded.metadata_json,
            created_utc = excluded.created_utc,
            updated_utc = excluded.updated_utc;
        """;
        await ExecuteInitializedAsync(sql, cancellationToken,
            ("$id", item.Id), ("$kind", item.Kind), ("$title", item.Title), ("$body", item.Body), ("$source", item.Source),
            ("$confidence", item.Confidence), ("$status", item.Status), ("$metadata_json", item.MetadataJson),
            ("$created_utc", item.CreatedUtc.ToString("O")), ("$updated_utc", item.UpdatedUtc.ToString("O"))).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<DesignKnowledgeItem>> ListKnowledgeItemsAsync(CancellationToken cancellationToken)
    {
        var rows = await QueryAsync("SELECT * FROM knowledge_items ORDER BY kind, title, id;", ReadKnowledgeItem, cancellationToken).ConfigureAwait(false);
        return rows;
    }

    public async Task UpsertDecisionAsync(DesignDecision decision, CancellationToken cancellationToken)
    {
        const string sql = """
        INSERT INTO design_decisions(id, question, chosen_answer, alternatives_json, reason, status, metadata_json, created_utc, updated_utc)
        VALUES ($id, $question, $chosen_answer, $alternatives_json, $reason, $status, $metadata_json, $created_utc, $updated_utc)
        ON CONFLICT(id) DO UPDATE SET
            question = excluded.question,
            chosen_answer = excluded.chosen_answer,
            alternatives_json = excluded.alternatives_json,
            reason = excluded.reason,
            status = excluded.status,
            metadata_json = excluded.metadata_json,
            created_utc = excluded.created_utc,
            updated_utc = excluded.updated_utc;
        """;
        await ExecuteInitializedAsync(sql, cancellationToken,
            ("$id", decision.Id), ("$question", decision.Question), ("$chosen_answer", decision.ChosenAnswer),
            ("$alternatives_json", decision.AlternativesJson), ("$reason", decision.Reason), ("$status", decision.Status),
            ("$metadata_json", decision.MetadataJson), ("$created_utc", decision.CreatedUtc.ToString("O")), ("$updated_utc", decision.UpdatedUtc.ToString("O"))).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<DesignDecision>> ListDecisionsAsync(CancellationToken cancellationToken)
    {
        return await QueryAsync("SELECT * FROM design_decisions ORDER BY updated_utc DESC, id;", ReadDecision, cancellationToken).ConfigureAwait(false);
    }

    public async Task UpsertConstraintAsync(DesignConstraint constraint, CancellationToken cancellationToken)
    {
        const string sql = """
        INSERT INTO design_constraints(id, scope, rule, severity, status, metadata_json)
        VALUES ($id, $scope, $rule, $severity, $status, $metadata_json)
        ON CONFLICT(id) DO UPDATE SET
            scope = excluded.scope,
            rule = excluded.rule,
            severity = excluded.severity,
            status = excluded.status,
            metadata_json = excluded.metadata_json;
        """;
        await ExecuteInitializedAsync(sql, cancellationToken,
            ("$id", constraint.Id), ("$scope", constraint.Scope), ("$rule", constraint.Rule),
            ("$severity", constraint.Severity), ("$status", constraint.Status), ("$metadata_json", constraint.MetadataJson)).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<DesignConstraint>> ListConstraintsAsync(CancellationToken cancellationToken)
    {
        return await QueryAsync("SELECT * FROM design_constraints ORDER BY scope, severity, id;", ReadConstraint, cancellationToken).ConfigureAwait(false);
    }

    public async Task SaveImportedLibraryAsync(GeneratorLibraryImportData data, CancellationToken cancellationToken)
    {
        EnsureInitialized();
        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        foreach (var capability in data.Capabilities)
        {
            await ExecuteAsync(connection, transaction, """
            INSERT INTO capability_modules(id, category, title, purpose, source_manifest_path, runtime_targets_json, turn_modes_json, combat_modes_json, ui_modes_json, world_scales_json, metadata_json, imported_utc)
            VALUES ($id, $category, $title, $purpose, $source_manifest_path, $runtime_targets_json, $turn_modes_json, $combat_modes_json, $ui_modes_json, $world_scales_json, $metadata_json, $imported_utc)
            ON CONFLICT(id) DO UPDATE SET
                category = excluded.category,
                title = excluded.title,
                purpose = excluded.purpose,
                source_manifest_path = excluded.source_manifest_path,
                runtime_targets_json = excluded.runtime_targets_json,
                turn_modes_json = excluded.turn_modes_json,
                combat_modes_json = excluded.combat_modes_json,
                ui_modes_json = excluded.ui_modes_json,
                world_scales_json = excluded.world_scales_json,
                metadata_json = excluded.metadata_json,
                imported_utc = excluded.imported_utc;
            """, cancellationToken,
            ("$id", capability.Id), ("$category", capability.Category), ("$title", capability.Title), ("$purpose", capability.Purpose),
            ("$source_manifest_path", capability.SourceManifestPath), ("$runtime_targets_json", capability.RuntimeTargetsJson),
            ("$turn_modes_json", capability.TurnModesJson), ("$combat_modes_json", capability.CombatModesJson),
            ("$ui_modes_json", capability.UiModesJson), ("$world_scales_json", capability.WorldScalesJson),
            ("$metadata_json", capability.MetadataJson), ("$imported_utc", capability.ImportedUtc.ToString("O"))).ConfigureAwait(false);
        }

        foreach (var module in data.Modules)
        {
            await ExecuteAsync(connection, transaction, """
            INSERT INTO generator_modules(id, batch_id, path, category, capabilities_json, dependencies_json, runtime_targets_json, turn_modes_json, combat_modes_json, source_manifest_path, metadata_json, imported_utc)
            VALUES ($id, $batch_id, $path, $category, $capabilities_json, $dependencies_json, $runtime_targets_json, $turn_modes_json, $combat_modes_json, $source_manifest_path, $metadata_json, $imported_utc)
            ON CONFLICT(id) DO UPDATE SET
                batch_id = excluded.batch_id,
                path = excluded.path,
                category = excluded.category,
                capabilities_json = excluded.capabilities_json,
                dependencies_json = excluded.dependencies_json,
                runtime_targets_json = excluded.runtime_targets_json,
                turn_modes_json = excluded.turn_modes_json,
                combat_modes_json = excluded.combat_modes_json,
                source_manifest_path = excluded.source_manifest_path,
                metadata_json = excluded.metadata_json,
                imported_utc = excluded.imported_utc;
            """, cancellationToken,
            ("$id", module.Id), ("$batch_id", module.BatchId), ("$path", module.Path), ("$category", module.Category),
            ("$capabilities_json", module.CapabilitiesJson), ("$dependencies_json", module.DependenciesJson),
            ("$runtime_targets_json", module.RuntimeTargetsJson), ("$turn_modes_json", module.TurnModesJson),
            ("$combat_modes_json", module.CombatModesJson), ("$source_manifest_path", module.SourceManifestPath),
            ("$metadata_json", module.MetadataJson), ("$imported_utc", module.ImportedUtc.ToString("O"))).ConfigureAwait(false);
        }

        foreach (var file in data.Files)
        {
            await ExecuteAsync(connection, transaction, """
            INSERT INTO generator_module_files(id, batch_id, relative_path, file_kind, source_manifest_path)
            VALUES ($id, $batch_id, $relative_path, $file_kind, $source_manifest_path)
            ON CONFLICT(id) DO UPDATE SET
                batch_id = excluded.batch_id,
                relative_path = excluded.relative_path,
                file_kind = excluded.file_kind,
                source_manifest_path = excluded.source_manifest_path;
            """, cancellationToken,
            ("$id", file.Id), ("$batch_id", file.BatchId), ("$relative_path", file.RelativePath),
            ("$file_kind", file.FileKind), ("$source_manifest_path", file.SourceManifestPath)).ConfigureAwait(false);
        }

        foreach (var issue in data.Issues)
        {
            await ExecuteAsync(connection, transaction, """
            INSERT INTO import_issues(id, import_id, severity, code, message, target, metadata_json)
            VALUES ($id, $import_id, $severity, $code, $message, $target, $metadata_json)
            ON CONFLICT(id) DO UPDATE SET
                import_id = excluded.import_id,
                severity = excluded.severity,
                code = excluded.code,
                message = excluded.message,
                target = excluded.target,
                metadata_json = excluded.metadata_json;
            """, cancellationToken,
            ("$id", issue.Id), ("$import_id", issue.ImportId), ("$severity", issue.Severity), ("$code", issue.Code),
            ("$message", issue.Message), ("$target", issue.Target), ("$metadata_json", issue.MetadataJson)).ConfigureAwait(false);
        }

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<CapabilityModuleRecord>> ListCapabilitiesAsync(CancellationToken cancellationToken)
    {
        return await QueryAsync("SELECT * FROM capability_modules ORDER BY category, id;", ReadCapability, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<GeneratorModuleRecord>> ListModulesAsync(CancellationToken cancellationToken)
    {
        return await QueryAsync("SELECT * FROM generator_modules ORDER BY batch_id, id;", ReadModule, cancellationToken).ConfigureAwait(false);
    }

    public async Task<GeneratorModuleRecord?> GetModuleByIdAsync(string moduleId, CancellationToken cancellationToken)
    {
        var rows = await QueryAsync("SELECT * FROM generator_modules WHERE id = $id LIMIT 1;", ReadModule, cancellationToken, ("$id", moduleId)).ConfigureAwait(false);
        return rows.FirstOrDefault();
    }

    public async Task<IReadOnlyList<GeneratorModuleRecord>> ListModulesByCapabilityAsync(string capabilityId, CancellationToken cancellationToken)
    {
        var modules = await ListModulesAsync(cancellationToken).ConfigureAwait(false);
        return modules
            .Where(module => JsonSerializer.Deserialize<List<string>>(module.CapabilitiesJson)?.Contains(capabilityId, StringComparer.OrdinalIgnoreCase) == true)
            .OrderBy(module => module.Id, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public async Task<IReadOnlyList<GeneratorLibraryImportIssue>> ListImportIssuesAsync(CancellationToken cancellationToken)
    {
        return await QueryAsync("SELECT * FROM import_issues ORDER BY severity, code, target;", ReadIssue, cancellationToken).ConfigureAwait(false);
    }

    private SqliteConnection CreateConnection()
    {
        EnsureInitialized();
        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = _databasePath!,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Pooling = false
        };
        return new SqliteConnection(builder.ToString());
    }

    private void EnsureInitialized()
    {
        if (string.IsNullOrWhiteSpace(_databasePath))
        {
            throw new InvalidOperationException("Design database has not been initialized.");
        }
    }

    private async Task ExecuteInitializedAsync(string sql, CancellationToken cancellationToken, params (string Name, object? Value)[] parameters)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await ExecuteAsync(connection, null, sql, cancellationToken, parameters).ConfigureAwait(false);
    }

    private static async Task ExecuteAsync(SqliteConnection connection, System.Data.Common.DbTransaction? transaction, string sql, CancellationToken cancellationToken, params (string Name, object? Value)[] parameters)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Transaction = (SqliteTransaction?)transaction;
        AddParameters(command, parameters);
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<List<T>> QueryAsync<T>(string sql, Func<SqliteDataReader, T> read, CancellationToken cancellationToken, params (string Name, object? Value)[] parameters)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        AddParameters(command, parameters);
        var rows = new List<T>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            rows.Add(read(reader));
        }

        return rows;
    }

    private static void AddParameters(SqliteCommand command, IEnumerable<(string Name, object? Value)> parameters)
    {
        foreach (var parameter in parameters)
        {
            command.Parameters.AddWithValue(parameter.Name, parameter.Value ?? DBNull.Value);
        }
    }

    private static async Task<int> GetUserVersionAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA user_version;";
        var value = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return Convert.ToInt32(value);
    }

    private static async Task<string?> GetMetadataValueAsync(SqliteConnection connection, string key, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT value FROM design_metadata WHERE key = $key;";
        command.Parameters.AddWithValue("$key", key);
        return await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false) as string;
    }

    private static DesignKnowledgeItem ReadKnowledgeItem(SqliteDataReader reader)
    {
        return new DesignKnowledgeItem(
            reader.GetString(reader.GetOrdinal("id")),
            reader.GetString(reader.GetOrdinal("kind")),
            reader.GetString(reader.GetOrdinal("title")),
            reader.GetString(reader.GetOrdinal("body")),
            reader.GetString(reader.GetOrdinal("source")),
            reader.GetDouble(reader.GetOrdinal("confidence")),
            reader.GetString(reader.GetOrdinal("status")),
            reader.GetString(reader.GetOrdinal("metadata_json")),
            DateTimeOffset.Parse(reader.GetString(reader.GetOrdinal("created_utc"))),
            DateTimeOffset.Parse(reader.GetString(reader.GetOrdinal("updated_utc"))));
    }

    private static DesignDecision ReadDecision(SqliteDataReader reader)
    {
        return new DesignDecision(
            reader.GetString(reader.GetOrdinal("id")),
            reader.GetString(reader.GetOrdinal("question")),
            reader.GetString(reader.GetOrdinal("chosen_answer")),
            reader.GetString(reader.GetOrdinal("alternatives_json")),
            reader.GetString(reader.GetOrdinal("reason")),
            reader.GetString(reader.GetOrdinal("status")),
            reader.GetString(reader.GetOrdinal("metadata_json")),
            DateTimeOffset.Parse(reader.GetString(reader.GetOrdinal("created_utc"))),
            DateTimeOffset.Parse(reader.GetString(reader.GetOrdinal("updated_utc"))));
    }

    private static DesignConstraint ReadConstraint(SqliteDataReader reader)
    {
        return new DesignConstraint(
            reader.GetString(reader.GetOrdinal("id")),
            reader.GetString(reader.GetOrdinal("scope")),
            reader.GetString(reader.GetOrdinal("rule")),
            reader.GetString(reader.GetOrdinal("severity")),
            reader.GetString(reader.GetOrdinal("status")),
            reader.GetString(reader.GetOrdinal("metadata_json")));
    }

    private static CapabilityModuleRecord ReadCapability(SqliteDataReader reader)
    {
        return new CapabilityModuleRecord(
            reader.GetString(reader.GetOrdinal("id")),
            reader.GetString(reader.GetOrdinal("category")),
            reader.GetString(reader.GetOrdinal("title")),
            reader.GetString(reader.GetOrdinal("purpose")),
            reader.GetString(reader.GetOrdinal("source_manifest_path")),
            reader.GetString(reader.GetOrdinal("runtime_targets_json")),
            reader.GetString(reader.GetOrdinal("turn_modes_json")),
            reader.GetString(reader.GetOrdinal("combat_modes_json")),
            reader.GetString(reader.GetOrdinal("ui_modes_json")),
            reader.GetString(reader.GetOrdinal("world_scales_json")),
            reader.GetString(reader.GetOrdinal("metadata_json")),
            DateTimeOffset.Parse(reader.GetString(reader.GetOrdinal("imported_utc"))));
    }

    private static GeneratorModuleRecord ReadModule(SqliteDataReader reader)
    {
        return new GeneratorModuleRecord(
            reader.GetString(reader.GetOrdinal("id")),
            reader.GetString(reader.GetOrdinal("batch_id")),
            reader.GetString(reader.GetOrdinal("path")),
            reader.GetString(reader.GetOrdinal("category")),
            reader.GetString(reader.GetOrdinal("capabilities_json")),
            reader.GetString(reader.GetOrdinal("dependencies_json")),
            reader.GetString(reader.GetOrdinal("runtime_targets_json")),
            reader.GetString(reader.GetOrdinal("turn_modes_json")),
            reader.GetString(reader.GetOrdinal("combat_modes_json")),
            reader.GetString(reader.GetOrdinal("source_manifest_path")),
            reader.GetString(reader.GetOrdinal("metadata_json")),
            DateTimeOffset.Parse(reader.GetString(reader.GetOrdinal("imported_utc"))));
    }

    private static GeneratorLibraryImportIssue ReadIssue(SqliteDataReader reader)
    {
        return new GeneratorLibraryImportIssue(
            reader.GetString(reader.GetOrdinal("id")),
            reader.GetString(reader.GetOrdinal("import_id")),
            reader.GetString(reader.GetOrdinal("severity")),
            reader.GetString(reader.GetOrdinal("code")),
            reader.GetString(reader.GetOrdinal("message")),
            reader.GetString(reader.GetOrdinal("target")),
            reader.GetString(reader.GetOrdinal("metadata_json")));
    }
}

namespace LLMGameCreator.Infrastructure.Storage;

internal static class SqliteDesignSchema
{
    public const int Version = 1;

    public static readonly string[] Statements =
    {
        """
        CREATE TABLE IF NOT EXISTS design_metadata (
            key TEXT PRIMARY KEY,
            value TEXT NOT NULL
        );
        """,
        """
        CREATE TABLE IF NOT EXISTS knowledge_items (
            id TEXT PRIMARY KEY,
            kind TEXT NOT NULL,
            title TEXT NOT NULL,
            body TEXT NOT NULL,
            source TEXT NOT NULL,
            confidence REAL NOT NULL,
            status TEXT NOT NULL,
            metadata_json TEXT NOT NULL,
            created_utc TEXT NOT NULL,
            updated_utc TEXT NOT NULL
        );
        """,
        """
        CREATE TABLE IF NOT EXISTS knowledge_relations (
            id TEXT PRIMARY KEY,
            from_id TEXT NOT NULL,
            to_id TEXT NOT NULL,
            relation_kind TEXT NOT NULL,
            metadata_json TEXT NOT NULL
        );
        """,
        """
        CREATE TABLE IF NOT EXISTS design_decisions (
            id TEXT PRIMARY KEY,
            question TEXT NOT NULL,
            chosen_answer TEXT NOT NULL,
            alternatives_json TEXT NOT NULL,
            reason TEXT NOT NULL,
            status TEXT NOT NULL,
            metadata_json TEXT NOT NULL,
            created_utc TEXT NOT NULL,
            updated_utc TEXT NOT NULL
        );
        """,
        """
        CREATE TABLE IF NOT EXISTS design_constraints (
            id TEXT PRIMARY KEY,
            scope TEXT NOT NULL,
            rule TEXT NOT NULL,
            severity TEXT NOT NULL,
            status TEXT NOT NULL,
            metadata_json TEXT NOT NULL
        );
        """,
        """
        CREATE TABLE IF NOT EXISTS capability_modules (
            id TEXT PRIMARY KEY,
            category TEXT NOT NULL,
            title TEXT NOT NULL,
            purpose TEXT NOT NULL,
            source_manifest_path TEXT NOT NULL,
            runtime_targets_json TEXT NOT NULL,
            turn_modes_json TEXT NOT NULL,
            combat_modes_json TEXT NOT NULL,
            ui_modes_json TEXT NOT NULL,
            world_scales_json TEXT NOT NULL,
            metadata_json TEXT NOT NULL,
            imported_utc TEXT NOT NULL
        );
        """,
        """
        CREATE TABLE IF NOT EXISTS generator_modules (
            id TEXT PRIMARY KEY,
            batch_id TEXT NOT NULL,
            path TEXT NOT NULL,
            category TEXT NOT NULL,
            capabilities_json TEXT NOT NULL,
            dependencies_json TEXT NOT NULL,
            runtime_targets_json TEXT NOT NULL,
            turn_modes_json TEXT NOT NULL,
            combat_modes_json TEXT NOT NULL,
            source_manifest_path TEXT NOT NULL,
            metadata_json TEXT NOT NULL,
            imported_utc TEXT NOT NULL
        );
        """,
        """
        CREATE TABLE IF NOT EXISTS generator_module_files (
            id TEXT PRIMARY KEY,
            batch_id TEXT NOT NULL,
            relative_path TEXT NOT NULL,
            file_kind TEXT NOT NULL,
            source_manifest_path TEXT NOT NULL
        );
        """,
        """
        CREATE TABLE IF NOT EXISTS generator_configs (
            id TEXT PRIMARY KEY,
            module_id TEXT NOT NULL,
            name TEXT NOT NULL,
            config_json TEXT NOT NULL,
            status TEXT NOT NULL,
            metadata_json TEXT NOT NULL
        );
        """,
        """
        CREATE TABLE IF NOT EXISTS generator_plans (
            id TEXT PRIMARY KEY,
            title TEXT NOT NULL,
            goal TEXT NOT NULL,
            status TEXT NOT NULL,
            metadata_json TEXT NOT NULL,
            created_utc TEXT NOT NULL,
            updated_utc TEXT NOT NULL
        );
        """,
        """
        CREATE TABLE IF NOT EXISTS generator_plan_steps (
            id TEXT PRIMARY KEY,
            plan_id TEXT NOT NULL,
            step_order INTEGER NOT NULL,
            module_id TEXT NOT NULL,
            config_json TEXT NOT NULL,
            depends_on_json TEXT NOT NULL,
            status TEXT NOT NULL
        );
        """,
        """
        CREATE TABLE IF NOT EXISTS generated_artifacts (
            id TEXT PRIMARY KEY,
            kind TEXT NOT NULL,
            path TEXT NOT NULL,
            json TEXT NOT NULL,
            generated_by TEXT NOT NULL,
            validation_state TEXT NOT NULL,
            metadata_json TEXT NOT NULL
        );
        """,
        """
        CREATE TABLE IF NOT EXISTS validation_results (
            id TEXT PRIMARY KEY,
            artifact_id TEXT NOT NULL,
            severity TEXT NOT NULL,
            code TEXT NOT NULL,
            message TEXT NOT NULL,
            target TEXT NOT NULL,
            metadata_json TEXT NOT NULL
        );
        """,
        """
        CREATE TABLE IF NOT EXISTS prompt_context_packs (
            id TEXT PRIMARY KEY,
            purpose TEXT NOT NULL,
            included_knowledge_ids_json TEXT NOT NULL,
            included_module_ids_json TEXT NOT NULL,
            token_budget INTEGER NOT NULL,
            metadata_json TEXT NOT NULL
        );
        """,
        """
        CREATE TABLE IF NOT EXISTS import_issues (
            id TEXT PRIMARY KEY,
            import_id TEXT NOT NULL,
            severity TEXT NOT NULL,
            code TEXT NOT NULL,
            message TEXT NOT NULL,
            target TEXT NOT NULL,
            metadata_json TEXT NOT NULL
        );
        """,
        "CREATE INDEX IF NOT EXISTS ix_generator_modules_id ON generator_modules(id);",
        "CREATE INDEX IF NOT EXISTS ix_capability_modules_id_category ON capability_modules(id, category);",
        "CREATE INDEX IF NOT EXISTS ix_generator_modules_batch_id ON generator_modules(batch_id);",
        "CREATE INDEX IF NOT EXISTS ix_generator_modules_source_manifest_path ON generator_modules(source_manifest_path);",
        "CREATE INDEX IF NOT EXISTS ix_generator_module_files_source_manifest_path ON generator_module_files(source_manifest_path);",
        "CREATE INDEX IF NOT EXISTS ix_import_issues_severity_code ON import_issues(severity, code);",
        "CREATE INDEX IF NOT EXISTS ix_knowledge_items_kind_status ON knowledge_items(kind, status);"
    };
}

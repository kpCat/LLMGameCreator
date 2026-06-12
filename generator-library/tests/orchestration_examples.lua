local examples = {}

examples.valid_dependency_ordering = {
  module_id = "generation/dependency_sort/v1",
  function_name = "generate",
  input = {
    plan_id = "examples/orchestration/dependency_order",
    modules = {
      {
        id = "generation/artifact_manifest/v1",
        depends_on = {}
      },
      {
        id = "generation/pipeline_runner_plan/v1",
        depends_on = {
          "generation/artifact_manifest/v1"
        }
      },
      {
        id = "generation/context_pack_plan/v1",
        depends_on = {
          "generation/artifact_manifest/v1"
        }
      }
    }
  },
  expected = {
    json_serializable = true,
    ordered_ids = {
      "generation/artifact_manifest/v1",
      "generation/pipeline_runner_plan/v1",
      "generation/context_pack_plan/v1"
    }
  }
}

examples.cyclic_dependency_example = {
  module_id = "generation/dependency_sort/v1",
  function_name = "generate",
  input = {
    plan_id = "examples/orchestration/cycle",
    steps = {
      {
        id = "step/a",
        depends_on = {
          "step/b"
        }
      },
      {
        id = "step/b",
        depends_on = {
          "step/a"
        }
      }
    }
  },
  expected_diagnostic_codes = {
    "cyclic_dependency"
  }
}

examples.artifact_manifest_with_validation_refs = {
  module_id = "generation/artifact_manifest/v1",
  function_name = "generate",
  input = {
    manifest_id = "examples/artifacts/main",
    producer_ids = {
      "generation/dependency_sort/v1",
      "validation/module_contract_validation/v1"
    },
    validation_result_index = {
      "validation/results/module_contracts"
    },
    artifacts = {
      {
        id = "artifact/manifests/orchestration",
        kind = "manifest",
        logical_path = "generator-library/manifests/orchestration.manifest.json",
        produced_by = "generation/dependency_sort/v1",
        validation_state = "valid",
        validation_result_refs = {
          "validation/results/module_contracts"
        },
        depends_on_artifacts = {},
        metadata = {
          batch = "020"
        }
      },
      {
        id = "artifact/docs/orchestration",
        kind = "doc",
        logical_path = "generator-library/docs/lua/generator_orchestration.md",
        produced_by = "generation/dependency_sort/v1",
        validation_state = "not_validated",
        validation_result_refs = {},
        depends_on_artifacts = {
          "artifact/manifests/orchestration"
        }
      }
    }
  },
  expected = {
    json_serializable = true,
    artifact_count = 2
  }
}

examples.pipeline_runner_plan_with_ordered_steps = {
  module_id = "generation/pipeline_runner_plan/v1",
  function_name = "generate",
  input = {
    plan_id = "examples/pipeline/plan_only",
    selected_module_ids = {
      "world/world_blueprint/v1",
      "validation/world_validation/v1",
      "generation/artifact_manifest/v1"
    },
    validation_checkpoint_ids = {
      "checkpoint/world",
      "checkpoint/artifacts"
    },
    expected_artifacts = {
      "artifact/world/blueprint",
      "artifact/validation/world"
    },
    steps = {
      {
        id = "step/world_blueprint",
        module_id = "world/world_blueprint/v1",
        config_ref = "config/world/default",
        expected_artifacts = {
          "artifact/world/blueprint"
        },
        validation_checkpoints = {
          "checkpoint/world"
        },
        depends_on_steps = {},
        dry_run = true,
        failure_policy = "stop_on_error"
      },
      {
        id = "step/world_validation",
        module_id = "validation/world_validation/v1",
        config_ref = "config/validation/world",
        expected_artifacts = {
          "artifact/validation/world"
        },
        validation_checkpoints = {
          "checkpoint/artifacts"
        },
        depends_on_steps = {
          "step/world_blueprint"
        },
        dry_run = true,
        failure_policy = "collect_all_diagnostics"
      }
    }
  },
  expected = {
    json_serializable = true,
    does_not_run_steps = true
  }
}

examples.invalid_pipeline_unsafe_flag = {
  module_id = "generation/pipeline_runner_plan/v1",
  function_name = "generate",
  input = {
    selected_module_ids = {
      "world/world_blueprint/v1"
    },
    steps = {
      {
        id = "step/unsafe",
        module_id = "world/world_blueprint/v1",
        run_now = true
      }
    }
  },
  expected_diagnostic_codes = {
    "unsafe_execution_flag"
  }
}

examples.context_pack_plan = {
  module_id = "generation/context_pack_plan/v1",
  function_name = "generate",
  input = {
    context_pack_id = "examples/context/game_design",
    purpose = "compact_generator_planning",
    token_budget = {
      max_input_tokens = 12000,
      max_output_tokens = 2000,
      reserved_tokens = 1000,
      target_tokens = 9000
    },
    available_module_ids = {
      "generation/dependency_sort/v1",
      "generation/artifact_manifest/v1",
      "validation/module_contract_validation/v1"
    },
    included_knowledge_ids = {
      "knowledge/design_profile",
      "knowledge/generator_plan"
    },
    included_module_ids = {
      "generation/dependency_sort/v1",
      "generation/artifact_manifest/v1"
    },
    included_artifact_ids = {
      "artifact/manifests/orchestration"
    },
    exclusions = {
      "knowledge/large_transcript"
    },
    hints = {
      compression = "compact",
      summarization = "metadata_only",
      priority = "ids_and_contracts"
    }
  },
  expected = {
    json_serializable = true,
    does_not_call_model = true,
    does_not_read_files = true
  }
}

return examples

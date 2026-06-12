local examples = {}

function examples.generate_frontier_town(modules)
  local citizen_needs = modules.citizen_needs
  local building_catalog = modules.building_catalog
  local job_system_config = modules.job_system_config
  local service_coverage = modules.service_coverage

  local needs = citizen_needs.generate({
    tick_mode = "simulation_tick",
    citizen_profile_id = "citizen/frontier_worker",
    needs = {
      {
        id = "need/food",
        category = "survival",
        priority = 100,
        weight = 1,
        decay_per_tick = 0.02,
        thresholds = { low = 0.2, warning = 0.45, satisfied = 0.85 },
        satisfaction_sources = { "service/market", "building/farm" }
      },
      {
        id = "need/rest",
        category = "housing",
        priority = 80,
        weight = 0.8,
        decay_per_tick = 0.015,
        thresholds = { low = 0.25, warning = 0.5, satisfied = 0.9 },
        satisfaction_sources = { "building/house" }
      }
    }
  })

  local buildings = building_catalog.generate({
    catalog_id = "building_catalog/frontier",
    buildings = {
      {
        id = "building/house",
        category = "housing",
        footprint = { width = 2, height = 2 },
        build_costs = { wood = 12, stone = 4 },
        zone_tags = { "residential" },
        hooks = { housing_capacity = 4, need_sources = { "need/rest" } }
      },
      {
        id = "building/farm",
        category = "production",
        footprint = { width = 3, height = 3 },
        build_costs = { wood = 10, tools = 2 },
        zone_tags = { "food", "rural" },
        hooks = { job_ids = { "job/farmer" }, output_items = { "item/food" } }
      }
    }
  })

  local jobs = job_system_config.generate({
    turn_mode = "mixed",
    jobs = {
      {
        id = "job/farmer",
        workplace_building_id = "building/farm",
        worker_capacity = 6,
        required_tags = { "food" },
        required_skills = { "farming" },
        shift = { mode = "day", start_tick = 0, duration_ticks = 10 },
        economy_hooks = { output_item = "item/food", output_per_tick = 2 }
      }
    }
  })

  local services = service_coverage.generate({
    coverage_model = "radius_metadata",
    services = {
      {
        id = "service/market",
        provider_building_categories = { "service" },
        radius = 12,
        capacity = 80,
        coverage_target_tags = { "residential" },
        quality = 0.75,
        priority = 70,
        need_ids = { "need/food" }
      }
    }
  })

  return {
    ok = needs.ok and buildings.ok and jobs.ok and services.ok,
    data = {
      needs = needs.data,
      buildings = buildings.data,
      jobs = jobs.data,
      services = services.data
    },
    diagnostics = {
      needs = needs.diagnostics,
      buildings = buildings.diagnostics,
      jobs = jobs.diagnostics,
      services = services.diagnostics
    },
    artifacts = {}
  }
end

function examples.invalid_need_config(modules)
  return modules.citizen_needs.generate({
    needs = {
      {
        id = "Need Bad",
        category = "survival",
        weight = -1,
        decay_per_tick = -0.5,
        thresholds = { low = 0.8, warning = 0.4, satisfied = 1.2 },
        satisfaction_sources = "service/market"
      }
    }
  })
end

function examples.invalid_building_config(modules)
  return modules.building_catalog.generate({
    buildings = {
      {
        id = "building/broken",
        category = "unknown",
        footprint = { width = 0, height = -1 },
        build_costs = { wood = -3 },
        zone_tags = "residential"
      }
    }
  })
end

return examples

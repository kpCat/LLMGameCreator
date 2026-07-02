# Goal 083 — Visual / Adult Layer Context Integration

Эта задача не реализует NSFW-генерацию и не добавляет медиа.

Она интегрирует уже добавленные visual/adult docs в официальный контекст проекта:
- `CONTEXT_INDEX.md`
- `FULL_GENERATOR_GOAL_QUEUE.md`
- current-state docs
- debt register
- deterministic `.llmgc` evidence

Главное требование: adult/NSFW слой трактуется как rating-gated metadata / asset-policy extension внутри общей visual/media pipeline, а не отдельный генератор и не runtime/provider behavior.

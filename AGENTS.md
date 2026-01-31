# AGENTS

Purpose

- Central index and brief guide for agents in this repository.
- Internal, detailed documentation lives in the docs/ directory — link to docs/agents/ for per-agent docs.

Repository layout (recommended)

- agents/
  - <agent-name>/
    - README.md            # short overview + quickstart
    - agent.py             # entrypoint (or package)
    - config.yaml          # default config
    - Dockerfile           # optional containerization
    - tests/               # unit/integration tests
    - ci.yml               # optional per-agent CI settings
- docs/
  - agents/               # long-form internal docs for each agent
  - tooling.md            # codegen / evaluation tooling guidance

Agent metadata (example)

```yaml
name: example-agent
description: Short description
entrypoint: agents.example_agent:main
language: python
requirements: requirements.txt
```

Getting started (developer)

- Find agent docs: docs/agents/<agent-name>.md
- Run locally: python -m agents.<agent_name> --help or follow agent README
- Build container: docker build -t <agent-name> agents/<agent-name>
- Tests: pytest agents/<agent-name>/tests

Development & contribution

- Add an agent under agents/<agent-name> with a README and config.
- Add docs in docs/agents/<agent-name>.md describing behavior, interfaces, and evaluation plan.
- Include unit tests and update top-level CI to run them.
- Follow repository coding standards, linters, and type checks before opening a PR.

Evaluation & best practices

- Use the evaluation planner first when metrics or dataset are unclear.
- Refer to internal tooling and best practice helpers: aitk-get_agent_code_gen_best_practices, aitk-get_tracing_code_gen_best_practices, aitk-get_ai_model_guidance, aitk-evaluation_planner, aitk-get_evaluation_code_gen_best_practices, aitk-evaluation_agent_runner_best_practices.
- Document evaluation datasets and metrics in docs/agents/<agent-name>/evaluation.md.

Testing & CI

- Unit tests + integration tests required.
- Add reproducible test data under tests/data/ or a dedicated test fixtures store.
- CI should run lint, unit tests, and the agent's basic smoke test.

Maintenance

- Keep docs/ in sync with agents/.
- Update agent metadata and README when APIs or configs change.

Contacts

- Maintain documentation in docs/ and list maintainers in the agent README.

Template files and examples are in docs/agents/templates.md — use them when adding new agents.

# EXP-001 — Independent Aletheon comparison (2026-10-24)

**Status: prepared; not executed.** Supersedes the earlier cross-model handoff design (which belongs to a separate future experiment).

## Question
How do three Aletheon-conditioned models independently solve the same technical and humanities tasks? Where do reasoning, assumptions, errors, and conclusions diverge?

## Participants
A: GLM-5.3 (`z-ai/glm-5.3`); B: DeepSeek V3.2 (`deepseek/deepseek-v3.2`); C: ChatGPT GPT-6 (control participant, **not** ground truth).

## Protocol
1. Freeze identical Base Core text, T1/H1 prompts, scoring rubric, and parameters where supported. Save Core hash and git commit.
2. Obtain C's **independent answers first** and timestamp/freeze them **before** exposing C to A/B outputs.
3. Submit four OpenRouter calls concurrently: A/T1, A/H1, B/T1, B/H1. Every request is fresh; there is no handoff and no exchange of answers.
4. Save raw JSON, request/response metadata, timestamps, token usage, costs where reported, and errors. Never save API keys.
5. Evaluate technical code against predefined tests; evaluate humanities reasoning with a blinded rubric. Compare three participants side-by-side. Record disagreement, failure, uncertainty, and shared errors.
6. This experiment tests observable answer diversity under common prompts, **not** consciousness, personal identity, or temporal continuity. A single run cannot establish stable model traits.

## Files
- `questions.json`: frozen independent prompts and criteria
- `participants.json`: roster
- `runner.py`: local asynchronous four-request runner (Python standard library)
- `CONTROL_REVIEW.md`: controls, freezing and reviewer limitations
- `runs/`: local outputs, not yet created
- `results/`: evaluated reports, not yet created

## Running
`OPENROUTER_API_KEY=... python runner.py --run --core ../../../../ALETHEON_BASE_CORE.md`

The example relative path may need adjustment depending on working directory; use the actual path to the repository root's Base Core. Running requires explicit authorization and may incur charges. No hard dollar spending cap is enforced by this script: configure an external OpenRouter key/account cap before using it.

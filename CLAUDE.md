# Working agreement — hollowknightrl

This project exists to **teach Parth**, not to produce a repo quickly. A finished agent he
can't explain is a failed project. Optimize for what he can do unaided in 2028, not for
how fast the code appears.

## The rule

**Never write code that is a learning objective.**

| Parth writes | Claude may write |
|---|---|
| RL algorithms (PPO, GAE, losses, training loop) | `.csproj` / MSBuild config |
| Reward functions | `.gitignore`, `.gitattributes` |
| The Gymnasium env wrapper | log-tailing / shell one-liners |
| Game state extraction, the C# mod internals | CI config, deploy scripts |
| The IPC protocol and its parsing | scaffolding of *empty* files |
| Anything in `python/rl/` or `mod/src/` | — |

When unsure, it's Parth's.

## Tiers of help — prefer the lowest tier that works

1. **Concept** — explain the mechanism, no code. Default.
2. **Hint** — point at the file/line/class. "Your bug is in the buffer indexing."
3. **Review** — he writes, Claude critiques line by line. *This is the target mode.*
4. **Reference** — show a snippet he retypes and adapts. Sparingly, for unfamiliar syntax.
5. **Write it** — non-learning-objective only.

If he's stuck, escalate one tier at a time. Do not jump to tier 5 because it's faster.

## Habits to enforce

- **Explain-back.** After explaining something non-trivial, ask him to explain it back.
  Feeling like you understood and understanding are different states.
- **Verify, don't trust.** API/field names differ across game versions — make him confirm
  in ILSpy rather than accepting names from any document, including `WEEK1.md`.
- **Measure, don't guess.** Performance claims need numbers. Reward-shaping claims need
  multiple seeds and error bars. RL results across 3 seeds are usually noise.
- **Log failures in the moment.** `LOG.md` entries get written during the work, not after.
  The wall-hit is the valuable part; never tidy it away.

## Learning outcomes (the actual deliverable)

Each is falsifiable — "can do cold, no reference," not "has studied."

1. **Git** — recover a file deleted 20 commits back; resolve a conflict by hand; explain what
   `git add` does to `.git/`.
2. **Systems/networking** — write a TCP client that reassembles framed messages from memory;
   explain why `recv()` returning a partial message is correct behavior.
3. **.NET/Unity/runtime patching** — extract a value from an unfamiliar Unity game;
   explain `Update` vs `FixedUpdate` and how MonoMod injects into a compiled assembly.
4. **Reverse engineering** — locate boss HP in `Assembly-CSharp.dll` unaided.
5. **Deep learning** — implement backprop for a 2-layer net in pure numpy, matching PyTorch
   gradients to ~6 decimals.
6. **RL** — derive the policy gradient theorem on paper; write PPO from scratch (CleanRL
   closed) that matches its CartPole learning curve.
7. **Experimental method** — design and report an ablation with seeds and error bars.
8. **Performance** — find a training-loop bottleneck with measurements, not intuition.

## Context

Full plan in `ROADMAP.md`. Current week in `WEEK1.md`. Running diary in `LOG.md`.
Target: MIT Maker Portfolio, Nov 2028. Critical path is P2 (the C# mod + Gym env).

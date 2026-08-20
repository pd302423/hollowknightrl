# Prior art — what's already been done

**Researched 2026-08-20.** Re-run this every ~6 months. The field moves; SilksongRL was
created after this project was first conceived.

Purpose: know before spending 200 hours whether the thing already exists.

---

## Verdict in one line

**"RL agent beats a Hollow Knight boss" is thoroughly done — at least five times.**
**"A learned agent completes a metroidvania" has never been done by anyone.**

---

## 1. Hollow Knight / Silksong RL — the crowded part

| Project | Stars | Last push | Status | Achieved |
|---|---|---|---|---|
| [ailec0623/DQN_HollowKnight](https://github.com/ailec0623/DQN_HollowKnight) | **348** | Jul 2021 | dead 5 yrs | DQN vs Hornet; HP-bar mod for reward |
| [AdityaJain1030/HKRL](https://github.com/AdityaJain1030/HKRL) | 18 | Oct 2023 | dead 3 yrs | Mod + gym env, websocket. **No results reported.** 4 scenes, long TODO list |
| [jimmie-jams/SilksongRL](https://github.com/jimmie-jams/SilksongRL) | 28 | Feb 2026 | dormant 6 mo | BepInEx mod + socket + PPO. **Lace 1 defeated** — video + checkpoint. **22 commits total, all in a 3-week burst Jan 22–Feb 10 2026.** Lace 2 + Savage Beastfly still pending |
| [lucasmr19/Hollow-knight-AI](https://github.com/lucasmr19/Hollow-knight-AI) | 3 | Jan 2025 | dead | YOLO object detection + RL |
| Stanford CS224R project | — | 2025 | course project | Compared DQN / Dueling / Double / Discrete SAC on HK bosses from pixels; DQN won |

**SilksongRL is the most useful calibration point**, but not for the reason it first appears.
Same architecture this project planned (C# Unity mod → socket → Python PPO), and it has a real
published boss kill. But the *result* is modest — one boss of three, dormant since February.

**The timeline is the finding.** The whole stack — mod, state extraction, input injection,
socket server, PPO loop — plus a boss defeated, in **22 commits over ~3 weeks**. By someone who
already knew C#, Unity, BepInEx, sockets and PPO. That was execution time, not learning time.

Two conclusions:
1. **The stack is not hard once you can program.** The prerequisites are the mountain, not the project.
2. **The published bar is low.** One boss, per-boss policy, no determinism, no reported metrics.
   Multi-task across 20 bosses with lockstep and error bars is a different category of artifact.

Code split is 80% C# / 20% Python — independent confirmation that **the mod is the project**.

### What none of them have

- **Lockstep determinism.** Every one runs asynchronously. Nobody can reproduce a run exactly.
- **Multi-task policy.** All train one policy per boss. Nobody has one policy for many bosses.
- **Reported metrics.** Almost none publish win rates, seeds, or error bars.
- **Maintenance.** Three of five are abandoned.
- **Navigation.** All are arena-only. Nobody moves between rooms.

---

## 2. Long-horizon game completion by pure RL — the wall

**Pokémon Red is the closest comparable**, and it's *easier* than Hollow Knight: turn-based
combat, no execution skill, no platforming, smaller action space.

| Work | Result |
|---|---|
| [PWhiddy/PokemonRedExperiments](https://github.com/PWhiddy/PokemonRedExperiments) | 40,000+ hours simulated. Famous. Did **not** finish the game. |
| [arXiv 2502.19920](https://arxiv.org/abs/2502.19920) (Feb 2025) | "Completes an initial segment of the game up to completing Cerulean City." Simplified env. Notes agents **exploit reward signals**. |
| [PokeRL, arXiv 2604.10812](https://arxiv.org/html/2604.10812v1) (2026) | PPO, 1.07M params. Three early tasks only: exit house 65%, reach grass 60%, win rival battle 50%. Requires **manual human triggers** during dialogue. |

> ⚠️ **Media overstates this constantly.** Headlines say "AI beats Pokémon Red." The papers
> say "reached Cerulean City." Always read the abstract, never the article.

**Years of community effort, and pure RL is stuck around the second gym of an easier game.**
That is the single most important data point for calibrating the full-game ambition.

Supporting theory: [HAC Explore](https://arxiv.org/pdf/2108.05872) — a strong
hierarchical+exploration method — handles tasks needing "over 1,000 primitive actions."
A Hollow Knight any% run is **~54,000 decisions** at 15 Hz. Fifty times past the frontier.

**Conclusion: pure RL will not complete Hollow Knight. Do not attempt it.**

---

## 3. LLM/VLM agents — they *can* finish long games

| Agent | Result |
|---|---|
| Gemini 2.5 Pro | **Completed Pokémon Blue**, May 2025 (custom harness with extra tools) |
| Claude Opus 4.7 | **Completed Pokémon Red**, May 2026 |

So the long-horizon problem *is* solvable — by language-model agents reasoning over goals,
not by RL policies.

**But Pokémon requires zero execution skill.** There is no dodging, no frame-timing, no
platforming. An LLM at ~1 action/second is fine there and hopeless in a Hollow Knight boss
fight at 15 Hz.

Relevant benchmarks: PokeGym (visually-driven long-horizon VLM benchmark, Apr 2026),
[OmniGameArena](https://arxiv.org/pdf/2606.09826) (UE5, VLM game agents),
[GamingAgent](https://github.com/lmgame-org/GamingAgent) (ICLR 2026).

---

## 4. The hybrid architecture — validated, but not here

[**Hierarchical Control in Multi-Agent Games: LLM-based Planning and RL Execution**
(arXiv 2606.20014, 2026)](https://arxiv.org/abs/2606.20014)

A pretrained LLM acts as strategic controller selecting among specialised RL skill policies;
RL handles reactive low-level execution. Evaluated on 2v2 King of the Hill: matches
hand-crafted behaviour trees, **significantly outperforms flat RL**.

Also: HiPER (high-level planner + low-level executor, Hierarchical Advantage Estimation).

**The architecture works. Nobody has applied it to a metroidvania, or to a game needing
frame-level execution, or to a full-game completion.**

---

## 5. The gaps — ranked by how defensible they are

### 🥇 Full-game completion via LLM planner + RL execution
Completely unclaimed. LLM chooses goals; RL policies fight and traverse. High risk, genuinely
novel if it lands. **This is the swing.**

### 🥈 Multi-task policy across many bosses
Every existing project trains one policy per boss. One policy handling 15–25 Hall of Gods
bosses — with Radiant clears — is unclaimed and much lower risk than the full game.

### 🥉 An environment that's actually reproducible
No existing HK RL project has lockstep determinism. Three of five are abandoned. A maintained,
documented, deterministic Gymnasium environment with published baselines is a real
contribution — and the one people would actually *use*.

### ❌ Single-boss RL
**Done. Repeatedly. Do not present this as the result.** It's a checkpoint on the way to the
above, and nothing more.

---

## What this means for the plan

1. **False Knight is a milestone, not a headline.** Frame it as "validating the environment,"
   never as the achievement.
2. **Build lockstep.** It's the cheapest genuine differentiator and every competitor lacks it.
3. **Go multi-task early.** One policy, many bosses — that's the defensible middle result if
   the full game doesn't land.
4. **Read SilksongRL's code before P3.** Closest active work, same architecture. Learn from it
   and cite it.
5. **Re-run this document every 6 months.** SilksongRL didn't exist a year ago.

---

*Next review: Feb 2027*

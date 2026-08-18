# Hollow Knight RL — Project Roadmap

**Owner:** Parth Dalal
**Started:** August 2026
**Target:** MIT Maker Portfolio, application cycle Nov 2028 (or Nov 2029)
**Stretch:** Silksong agent, 2029+

---

## 0. Verdict: is this a good idea?

**Yes — with one scope correction.**

### Why it's a strong project

- It is genuinely hard across **three different disciplines**: reverse engineering / game modding (C#, Unity, IPC), systems engineering (throughput, determinism, parallelism), and machine learning (RL algorithms, reward design, evaluation). Most high-school AI projects are "I fine-tuned a model on a Kaggle dataset." This is not that.
- It produces **video**. An agent that visibly goes from flailing into spikes to perfect-parrying False Knight is the single most legible artifact you can put in a maker portfolio. Admissions readers are not going to read your loss curves. They will watch the clip.
- The hardest part is **not the ML** — it's the environment. That's actually good news, because environment engineering is a skill you can grind reliably, whereas "invent a new RL algorithm" is a coin flip.
- It has a **reusable public artifact**: a working `HollowKnightEnv` Gymnasium environment that other people can use. If strangers on GitHub use your code, that is external validation you cannot manufacture.

### The scope correction

> "Train an AI which can complete the whole Hollow Knight game."

A single end-to-end RL policy that beats Hollow Knight from a fresh save is an **open research problem**, not a two-year solo project. Here's why, concretely:

| Property | Atari (solved) | Montezuma's Revenge (hard) | Hollow Knight |
|---|---|---|---|
| Episode length | ~5–20 min | ~10 min | **20–40 hours** |
| Reward density | Dense | Extremely sparse | Extremely sparse + gated |
| Backtracking required | No | Some | **Constant** |
| Ability gating | No | Keys | Dash / claw / wings / dream nail / etc. |
| Map size | 1 screen | ~24 rooms | **~160 rooms** |
| Non-determinism | Low | Low | Boss RNG, physics |

Hollow Knight is Montezuma's Revenge with a hundred times the horizon and combat that requires frame-level execution. Nobody has solved this class of problem end-to-end without either human demonstrations or a hierarchical decomposition.

**The honest, achievable version:**

> A **hierarchical agent** for Hollow Knight: learned low-level combat policies (one per boss, or one multi-task policy), learned/planned navigation over a room graph, and a high-level route executor. Demonstrated on the full Hall of Gods boss roster, and on a routed any% completion run.

This is still extremely impressive, it is deliverable, and — critically — **it is true**. Overclaiming on an application is a worse failure mode than a smaller scope. Frame it as: *"I built a system that learns to beat Hollow Knight's bosses at superhuman consistency and can execute a full-game route. Here is exactly what is learned and what is scripted."*

---

## 1. The five insights that determine whether this project succeeds

Read this section twice. Almost everything else is execution detail.

### 1.1 Do NOT build a screen-scraping + keyboard-simulation bot

The naive architecture is: screenshot the window → CNN → simulate keypresses. This is what most tutorials do, and it caps your project at "toy demo." Problems: you can't read HP reliably, you can't reset quickly, you can't speed up time, you can't synchronize the agent's step with the game's frame, and OS-level input injection is flaky in Unity.

### 1.2 Write a C# mod instead. This is the whole project.

Hollow Knight is a Unity game with a **mature, actively maintained modding community** (the Hollow Knight Modding API, installed via Scarab, built on MonoMod). This means you can load your own C# code into the game process and get:

- **Direct state access** — player position/velocity, HP, soul, i-frames, boss HP, boss hitboxes, and *the boss's current PlayMaker FSM state name*. That last one is enormous: Hollow Knight bosses are finite state machines, so you can literally read `"Jump Antic"` or `"Slam"` as a feature. You get a perfect, noise-free observation for free.
- **Direct input injection** into the game's own input layer — no `SendInput`, no virtual gamepad, no OS flakiness, frame-exact.
- **Instant reset** — teleport the knight, restore HP, reset boss HP and FSM, skip the death animation entirely. Reset cost drops from ~5 seconds to ~0.1 seconds.
- **Lockstep execution** (see 1.3).
- **Time acceleration** via `Time.timeScale` + matched `Time.fixedDeltaTime`.

**Skill required:** basic C#. You can learn enough C# for this in 3–4 weeks. It is the highest-leverage 3–4 weeks in the entire project.

### 1.3 Make the environment synchronous (lockstep)

Have the mod **block the game's update loop** until your Python agent sends an action, and have Python block until the mod sends the next observation. The game becomes a normal, deterministic, turn-based Gymnasium environment. No frame drops, no jitter, no "did my action land on the right frame?", fully reproducible runs, and you can pause a training run under a debugger.

Without lockstep you will spend months chasing heisenbugs. With it, `env.step()` just works.

### 1.4 Use Godhome / Hall of Gods as your training arena

The Godmaster DLC (free) added the Hall of Gods: statues for ~40+ bosses, each an isolated arena with instant retry, plus difficulty tiers:

| Tier | Meaning | Use |
|---|---|---|
| Attuned | Base boss | Initial learning |
| Ascended | More HP, faster, more aggressive | Skill refinement |
| Radiant | **One hit and you die** | Final curriculum rung / the money shot |

An agent that beats **Radiant** bosses is doing something no casual human player can do. This is your headline result, and it's a natural curriculum, not something you have to invent.

### 1.5 Your bottleneck is wall-clock game time, not GPU

This is the single most misunderstood thing about game RL. Your GPU will be **idle**. You are not compute-bound, you are *simulation-bound*. Optimizing means:

- More parallel game instances (the game is cheap: low settings, 640×360, vsync off)
- Higher `timeScale`
- Faster resets
- More sample-efficient algorithms (DreamerV3, BBF, EfficientZero-style) instead of vanilla PPO

Budget for a machine that runs 8–12 game instances, not for an H100.

---

## 2. System architecture

```
┌──────────────────────────────────────────────────────────────┐
│  Learner process (Python, PyTorch)                           │
│    PPO / Recurrent PPO / DreamerV3                           │
│    replay buffer · optimizer · checkpoints · W&B logging     │
└───────────────▲──────────────────────────┬───────────────────┘
                │ rollouts                 │ policy weights
┌───────────────┴──────────────────────────▼───────────────────┐
│  VecEnv (N workers)                                          │
│    HollowKnightEnv  (gymnasium.Env)                          │
│    obs normalization · action repeat · reward shaping        │
└───────────────▲──────────────────────────┬───────────────────┘
                │ observation (lockstep)   │ action
┌───────────────┴──────────────────────────▼───────────────────┐
│  IPC bridge  (TCP loopback or named pipe, msgpack/protobuf)  │
└───────────────▲──────────────────────────┬───────────────────┘
                │                          │
┌───────────────┴──────────────────────────▼───────────────────┐
│  hkrl-mod  (C#, Hollow Knight Modding API / MonoMod)         │
│    • blocks Update() until action arrives   [LOCKSTEP]       │
│    • serializes state: player, boss, FSM, hitboxes, HP       │
│    • injects inputs into InputHandler                        │
│    • FastReset(): restore HP, reset boss FSM, skip death     │
│    • timeScale control                                       │
└───────────────┬──────────────────────────────────────────────┘
                │
        ┌───────▼────────┐
        │ Hollow Knight  │   ×N instances, 640×360, low, vsync off
        └────────────────┘
```

### Observation space

**v1 — structured state (~80–150 floats).** Start here. Massively more sample-efficient.

```
player:  x, y, vx, vy, hp, soul, facing, grounded, dashing, i-frames,
         attack_cooldown, can_dash, can_jump
boss:    x, y, vx, vy, hp, facing, fsm_state (one-hot over ~30 states),
         time_in_state, phase
derived: dx, dy, distance, boss_hitbox_rects[k], my_hitbox
history: last 4–8 frames stacked
```

**v2 — pixels (84×84 or 64×64, grayscale, 4-frame stack).** Add this later for the "it learns from vision like a human" story and for cross-boss generalization. It costs ~10× the samples.

**v3 — both.** Best final result; also gives you a clean ablation for the writeup.

### Action space

`MultiDiscrete` is better than a flat discrete space here:

```
move    : {none, left, right}                  (3)
vertical: {none, up, down}                     (3)
jump    : {none, press, hold}                  (3)
attack  : {none, nail}                         (2)
dash    : {none, dash}                         (2)
spell   : {none, cast}                         (2)   # focus/heal + spells
```
→ 216 combos factored into 6 heads. PPO handles factored action spaces natively.

Run at **action repeat 4** (15 decisions/sec at 60 fps). Human pro players operate around 8–15 meaningful inputs/sec, so this is fair and it cuts your horizon 4×.

### Reward function (start simple, resist the urge to over-shape)

```
r = + 1.0  × (boss_hp_lost this step / boss_max_hp)
    - 1.0  × (player_hp_lost this step / player_max_hp)
    - 0.001                                  # time penalty
    + 5.0  × win
    - 1.0  × death
```

Add potential-based shaping only if it stalls. Log an **unshaped** evaluation metric (win rate, avg boss HP remaining) separately from training reward — otherwise you'll fool yourself.

---

## 3. Learning path — what to learn, in order

Do **not** try to learn everything before starting. Each stage below ends with something that runs.

### Stage A — Foundations (Months 0–2, ~Aug–Sep 2026)

| Topic | Resource | Deliverable |
|---|---|---|
| Python fluency, numpy | any | comfortable |
| Git / GitHub | Pro Git ch. 1–3 | repo with real history |
| PyTorch basics | official 60-min blitz + a small CNN on CIFAR-10 | training loop written from scratch |
| Linux / CLI / SSH | — | can drive a remote box |
| Probability + linear algebra | 3Blue1Brown, Khan | comfortable with expectation, gradients |

**Gate:** you can write a training loop in PyTorch without copying one.

### Stage B — RL fundamentals (Months 2–5, ~Oct–Dec 2026)

| Topic | Resource |
|---|---|
| MDPs, value functions, Bellman | Sutton & Barto ch. 3–6 |
| Policy gradients, actor-critic | OpenAI **Spinning Up in Deep RL** (do the exercises) |
| PPO in detail | Spinning Up + *"The 37 Implementation Details of PPO"* (ICLR blog) |
| Reference implementations | **CleanRL** — single-file, readable. Read `ppo.py` line by line. |
| API conventions | Gymnasium docs, `VectorEnv`, wrappers |
| Experiment tracking | Weights & Biases |

**Implement from scratch, in this order:**
1. Tabular Q-learning → FrozenLake
2. DQN → CartPole
3. PPO → CartPole → LunarLander
4. PPO → Atari Pong and Breakout (this teaches you frame stacking, action repeat, reward clipping — all directly reused)

**Gate:** *your own* PPO solves Pong. Not Stable-Baselines3's. Yours.

### Stage C — Environment engineering (Months 4–8, overlaps B) ⚠️ **the real project**

| Topic | Resource |
|---|---|
| C# basics | Microsoft C# tour — 2–3 weeks is enough |
| Unity concepts | GameObject, Component, `Update`/`FixedUpdate`, coroutines, `Time.timeScale` |
| HK Modding API | hk-modding docs, Scarab installer, the modding Discord |
| MonoMod / Harmony hooks | how to patch a method at runtime |
| PlayMaker FSMs | how HK bosses are actually implemented |
| Inspecting the game | dnSpy / ILSpy on `Assembly-CSharp.dll` |
| IPC | TCP sockets, msgpack or protobuf |
| Screen capture (for v2) | `dxcam` / `windows-capture` (Windows), 200+ fps |

**Gate:** `env = HollowKnightEnv("FalseKnight"); obs, _ = env.reset(); obs, r, term, trunc, info = env.step(a)` works, deterministically, at >200 steps/sec with `timeScale=4`.

### Stage D — Advanced RL (Months 10+, as needed)

- Recurrent PPO / LSTM policies (partial observability)
- **DreamerV3** (Hafner et al.) — world models, strong sample efficiency from pixels
- **BBF / EfficientZero** — the Atari-100k sample-efficiency literature; directly relevant because your samples are expensive
- Multi-task / goal-conditioned RL (one policy, many bosses)
- Hierarchical RL: options framework, feudal networks
- Imitation / behavior cloning from your own recorded play (a *huge* accelerator — record yourself, pretrain, then fine-tune with RL)
- Domain randomization (for pixel-based generalization)

---

## 4. Timeline

Assumes ~10–15 hrs/week during school, more in summer. Slip factor: assume everything takes **1.5–2×** your estimate.

| Phase | Window | Goal | Shippable artifact |
|---|---|---|---|
| **P0** | Aug–Sep 2026 | Foundations | GitHub repo, PyTorch CIFAR classifier, build log started |
| **P1** | Oct–Dec 2026 | RL fundamentals | Your own PPO beats Pong; blog post: "PPO from scratch" |
| **P2** | Nov 2026–Feb 2027 | **Env engineering** | `hkrl-mod` (C#) + `hollowknight-gym` (Python), **open-sourced** |
| **P3** | Mar–Jun 2027 | **False Knight** | >95% win rate (Attuned). Video. Ablation study. Writeup. |
| **P4** | Jul–Aug 2027 | Harden + generalize | Beat 5 bosses. Ascended tier. Pixel-based version working. |
| **P5** | Sep 2027–Feb 2028 | **Multi-boss** | One multi-task policy, 15–25 Hall of Gods bosses. Radiant tier on 3+. |
| **P6** | Mar–Jul 2028 | **Navigation + hierarchy** | Room graph, movement skills, beat Crossroads→Greenpath→Hornet from a real save |
| **P7** | Aug–Oct 2028 | **Full route + portfolio** | Any%-routed run attempt; 5-min video; technical report; project site |
| **P8** | Nov 2028 | **Submit** | — |
| **P9** | 2029+ | Silksong | Transfer the env architecture |

### Buffer strategy (important)

Build the portfolio around what is **guaranteed done by mid-2028** (multi-boss agent, Radiant clears, the open-source env). Treat the full-game run as **upside**. If you make the full run the load-bearing deliverable and it slips, you have nothing. If you make the boss agent the deliverable and the full run lands, you look like a genius.

### Ship something every 6–8 weeks

The #1 killer of two-year solo projects is not difficulty — it's motivation decay. Every 6–8 weeks, ship a video, a blog post, a release, or a demo. Momentum is a resource you have to manage deliberately.

---

## 5. Compute: how much, what kind, what it costs

### 5.1 Throughput math

```
60 fps · action repeat 4                     = 15 agent steps/sec  @ 1× timeScale
                                             = 45 agent steps/sec  @ 3× timeScale
× 8 parallel instances                       = 360 steps/sec
× 0.7 (reset + IPC + stall overhead)         ≈ 250 steps/sec sustained
                                             ≈ 900,000 steps/hour
```

### 5.2 Sample budgets

| Task | Obs type | Algorithm | Est. steps | Wall-clock @ 900k/hr |
|---|---|---|---|---|
| False Knight | structured | PPO | 2–5 M | **3–6 hrs** |
| False Knight | pixels | PPO | 20–50 M | 22–55 hrs |
| False Knight | pixels | DreamerV3 | 3–8 M | 4–9 hrs* |
| Hornet / mid-tier boss | structured | PPO | 10–20 M | 11–22 hrs |
| Radiant tier boss | structured | PPO + curriculum | 30–80 M | 33–90 hrs |
| Multi-task, 20 bosses | structured | PPO multi-task | 300–600 M | 330–660 hrs |
| Navigation skills | structured | PPO + BC | 50–150 M | 55–165 hrs |

\* DreamerV3 is *sample*-efficient but *compute*-heavy — it becomes GPU-bound, so wall-clock won't improve as much as step count suggests.

**Multiply every number above by 20–50×** for the real total, because you will re-run experiments constantly (bad reward, bad hyperparameters, bug in reset, etc.). That's normal and it's where the learning happens.

### 5.3 Total project budget

| Phase | Instance-hours |
|---|---|
| P3 False Knight (all experiments) | 300 – 800 |
| P4–P5 Multi-boss | 4,000 – 15,000 |
| P6 Navigation | 1,500 – 4,000 |
| P7 Full run | 1,000 – 3,000 |
| **Total** | **~7,000 – 23,000 instance-hours** |

At 8 instances running 8 hrs/night, 250 nights/year = **16,000 instance-hours/year**. So one good desktop, run overnight, covers this comfortably over two years. **You do not need a datacenter.**

### 5.4 Hardware

**Tier 0 — start today, $0.** Whatever machine runs Hollow Knight. Do P0–P2 and the first False Knight runs on it. A GTX 1650 / integrated graphics is fine for structured-state PPO. Do not buy anything yet.

**Tier 1 — the training rig (buy around P3/P4, ~mid-2027).** This is the right purchase point: you'll know what you actually need, and hardware gets cheaper.

| Part | Spec | Why |
|---|---|---|
| CPU | 12–16 cores (Ryzen 9 7900X / 9900X or equivalent) | ~1 core per game instance |
| RAM | 64 GB | ~4 GB per instance + learner |
| GPU | **Used RTX 3090 (24 GB)** — best value. Or 4070 Ti Super / 5070 Ti (16 GB). | VRAM matters more than speed; 24 GB unlocks DreamerV3 and pixel work |
| Storage | 2 TB NVMe | game instances + replay buffers + checkpoints |
| Budget | **~$1,800 – 2,800** | |

Do **not** buy an H100 or a 4090. Your GPU sits at 15% utilization for most of this project. Cores and VRAM > raw TFLOPs.

**Tier 2 — scale-out (optional, P5+).** Hollow Knight has a **native Linux build**. That means you can run headless instances (`xvfb` + software or virtual GL) on cheap cloud boxes and run a distributed setup (IMPALA / SEED-RL style: many actors, one learner).

| Option | Cost | Notes |
|---|---|---|
| AWS `g4dn.xlarge` spot | ~$0.16–0.25/hr | ~4 instances each; 10 of them ≈ $2/hr for 40 instances |
| Vast.ai / RunPod | ~$0.15–0.40/hr | cheaper, less reliable, fine for actors |
| Second used desktop | ~$600 one-time | often better than cloud for a 2-year project |

Realistic cloud spend if you use spot instances for burst runs only: **$300–1,500 total across the project**. You need one Steam license per concurrent instance region — check Steam/Team Cherry terms before large-scale cloud deployment, and keep it to personal research use.

### 5.5 Cost-saving tricks (in order of impact)

1. **Structured state before pixels.** 10× fewer samples.
2. **Fast reset in the mod.** Cuts wasted time 30–50% on short episodes.
3. **`timeScale = 3–4`.** Free 3–4× throughput. Validate that physics and hitboxes stay faithful — scale `Time.fixedDeltaTime` to match, and verify a scripted input sequence produces identical outcomes at 1× and 4×.
4. **Behavior cloning pretrain.** Record 5–10 hours of your own play through the same mod. Pretrain the policy. This can cut RL steps by 50–80%.
5. **Curriculum**, not brute force: Attuned → Ascended → Radiant, and start the agent mid-fight at hard moments rather than always from the top.
6. **Frame skip 4** (already in the plan).
7. Kill bad runs at 20 minutes. Most failed runs are visibly failing early.

---

## 6. Risks and mitigations

| Risk | Severity | Mitigation |
|---|---|---|
| **Motivation decay over 2 years** | 🔴 Highest | Ship every 6–8 weeks. Public build log. Post clips. |
| Scope creep → nothing finished | 🔴 High | Portfolio built on P5 (multi-boss), not P7 (full run) |
| Game update breaks the mod | 🟡 Med | Pin the game version. Disable auto-update in Steam. Back up the install. |
| Time acceleration corrupts physics | 🟡 Med | Determinism test suite: same input seq @1× and @4× must match |
| Reward hacking (agent stalls forever) | 🟡 Med | Time penalty + episode timeout + always log unshaped win rate |
| Overfitting to one boss's RNG seed | 🟡 Med | Randomize seeds; hold out evaluation seeds |
| Full-game run never works | 🟢 Low impact | Already de-risked by the buffer strategy |
| School / other commitments | 🟡 Med | Front-load hard engineering into summers |

---

## 7. Portfolio strategy (this is not an afterthought)

The MIT Maker Portfolio (submitted via SlideRoom; **verify current item/length limits on the MIT admissions site — they change**) is reviewed by people who make things. They care about **process** at least as much as outcome.

**Start doing these on day one, not in month 24:**

1. **Dated build log.** One entry per week. Include the failures — especially the failures. "Week 31: three days lost because `timeScale` desynced hitboxes from animations. Fix: also scale `fixedDeltaTime`." That paragraph is worth more than a perfect result.
2. **Record everything.** Screen-record training from the very first run. The montage of an agent going from walking-into-spikes → surviving 10 seconds → flawless Radiant clear is your best single asset. You cannot recreate early-failure footage later.
3. **Open-source the environment.** `hollowknight-gym` being usable by strangers is third-party proof your work is real. Write a real README.
4. **Write a technical report.** 8–15 pages, arXiv-style: method, ablations, results tables, limitations. This also makes you competitive for ISEF / Regeneron STS, which is a parallel path worth taking.
5. **Be precise about what's learned vs. scripted.** A clear diagram labeling "learned policy" vs. "planned route" vs. "hardcoded" reads as intellectual honesty. Vagueness reads as hiding something.
6. **A short project site** with embedded videos, so a reviewer gets it in 30 seconds.

**One line to have ready:** *"I built the environment that made the ML possible."* That's the sentence that separates you from everyone who ran someone else's training script.

---

## 8. Prior art to study (do this early — it's not cheating, it's literature review)

- Search GitHub for existing Hollow Knight RL projects (there are DQN-based Hornet agents, e.g. `ailec0623/DQN_HollowKnight`). Read their code, note what they got wrong (usually: screen scraping, slow resets, no lockstep). Cite them in your writeup.
- **CleanRL** — reference implementations
- **Spinning Up in Deep RL** — OpenAI
- **DreamerV3** (Hafner et al., 2023) — world models
- **BBF** (Schwarzer et al., 2023) — Atari-100k sample efficiency
- **Go-Explore** (Ecoffet et al.) — exploration for Montezuma-class problems; relevant to full-game
- **AlphaStar / OpenAI Five** — for how hierarchical + imitation-pretrained agents are actually built at scale
- The **Hollow Knight modding Discord** — this community will unblock you faster than anything else. Join it in month 1.

---

## 9. What to do this week

Two tracks, in parallel. Both are small.

**Track 1 — prove the environment is possible (do this FIRST).**
1. Install Scarab (the Hollow Knight mod installer) and any existing debug mod. Confirm mods load.
2. Get a C# "hello world" mod loading into the game.
3. Make it print the knight's HP and position to the console every frame.
4. Make it open a TCP socket and send that state to a Python script.

If you get to step 4, the project is real. If you get stuck, ask the modding Discord.

**Track 2 — start the RL ladder.**
1. `pip install gymnasium torch`
2. Get CleanRL's `ppo.py` running on CartPole.
3. Then close it and write your own from scratch.

**Track 3 — infrastructure (30 minutes).**
1. `git init`, push to GitHub, make it public.
2. Create `LOG.md`. Write entry #1 today.

---

## 10. Success criteria by phase

| Phase | Definition of done |
|---|---|
| P2 | Gymnasium env, deterministic, >200 steps/sec, reset <200 ms, open-sourced with README |
| P3 | False Knight (Attuned) ≥95% win rate over 200 eval episodes, from structured state |
| P4 | Same agent architecture beats 5 distinct bosses; pixel version beats False Knight ≥80% |
| P5 | One multi-task policy, ≥15 bosses ≥90%; ≥3 bosses cleared on **Radiant** |
| P6 | Agent navigates a 20+ room stretch of the real map from a save file, unassisted |
| P7 | Documented full-game route executed; every component labeled learned/planned/scripted |

---

*Last updated: 2026-08-04*

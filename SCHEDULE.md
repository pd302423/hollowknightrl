# Schedule

**Rewritten 2026-08-20** — re-costed against real hours (school-day hour + vacation sprints)
and a prior-art search. `ROADMAP.md` is the *what and why*. This file is the *when*.

---

## Budget

| | |
|---|---|
| School days, Sep 2026 → Oct 2028 | ~650 × 1 hr = **650 hrs** |
| Vacation days (~9 weeks/yr) | ~120 × 5 hrs = **600 hrs** |
| **Total** | **~1,250 hrs** (up to ~1,475 at 6 hrs/day in vacations) |

Estimated work for the full-game path: **~1,320 hrs.** It fits, with no slack.
Vacations are not a bonus here — they are load-bearing. Miss two and the stretch goal goes.

---

## Starting point

**Have:** `print`, `if`/`else`, functions, data types.
**Need:** loops → lists/dicts → files → classes → exceptions.

### Self-check — write each from scratch, no reference

1. ☐ Function returning the larger of two numbers ← *functions*
2. ☐ Print every number 1–100 divisible by 7 ← *loops*
3. ☐ Average of a list of numbers ← *lists*
4. ☐ Count each word's occurrences in a sentence ← *dicts*
5. ☐ Read a file, print lines containing "error" ← *file I/O*
6. ☐ A `Dog` class with a name and `bark()` ← *classes*
7. ☐ Handle a missing file without crashing ← *exceptions*

**The first one you can't do is where you start.** Don't grind through material you've already got.

---

## How to learn Python

**Spine: CS50P** (Harvard, free, self-paced). Chosen because the problem sets are
**auto-graded** — you can't finish a week without writing working code, which kills the main
self-teaching failure: feeling like you understood a video and being unable to write anything.

**Four rules that decide whether it works:**

1. **Type everything, never paste.** The slowness is where the learning is.
2. **Pause before the answer.** Watching someone solve a problem feels identical to solving it.
3. **After every topic, write one program nobody assigned you.** This is the entire difference
   between "did a Python course" and "can program."
4. **20 min stuck → search. 45 → ask.**

**Point it at this repo as you go:**

| After you learn | Write |
|---|---|
| loops + files | read `ModLog.txt`, print only lines containing "HkRl" |
| lists | pull every number from the log; print min/max/average |
| dicts | count how many times each mod logged something |
| classes | a `LogEntry` class that parses one line into fields |
| matplotlib | plot something from the log |

The last one is a baby training dashboard. None of it is throwaway.

---

## Session shape

| | Length | Work |
|---|---|---|
| **Weekdays** ×5 | 1 hr | Theory, reading, small self-contained code, commits |
| **Weekend** ×1 | 2–3 hrs | The mod. High-startup-cost work. |
| **Vacations** | 5–6 hrs | The hard engineering: lockstep, navigation, integration |

Hard debugging in 1-hour slices loses ~20 min/session to context reload. Save it for blocks.

---

## Phases

### P0 · Python fundamentals — **Sep 2026** (~50 hrs, intensive)

CS50P start to finish, compressed into one month. Feasible because functions are already there.

> **Done when:** all 7 self-check items, cold. Plus 10+ programs nobody assigned you.

### P0.5 · Python for real work — **Oct 2026** (~40 hrs)

numpy, matplotlib, virtualenvs, reading docs, pytest basics.

> **Done when:** load a CSV, compute stats, plot it — no tutorial open.

### P1 · C# and the mod — **Nov 2026–Jan 2027** (~90 hrs)

C# by diffing against Python. `WEEK1.md` Days 3–5 properly.

> **Done when:** GATE 1/2/3 pass and you can explain every line you wrote.

### P2 · RL fundamentals — **Feb–May 2027** (~130 hrs)

Sutton & Barto ch. 3–6, Spinning Up. Tabular Q → DQN → PPO, each from scratch.

> **Done when:** your own PPO solves CartPole and you can explain GAE and clipping without notes.

### P3 · The environment — **Jun–Sep 2027** (~160 hrs) ⚠️ hardest engineering

Boss HP, PlayMaker FSM state, input injection, fast reset, **lockstep**, `HollowKnightEnv`.

> **Done when:** `check_env` passes, stepping is deterministic, reset < 200 ms.
> 🏦 **BANK IT** — open-source release. First portfolio-safe artifact.

### P4 · False Knight — **Oct–Nov 2027** (~80 hrs)

Reward design, curriculum, ablations, seeds and error bars.

> **Done when:** ≥90% win rate on Attuned over 200 eval episodes + written experiment report.
> 🏦 **BANK IT.** Note: not novel — this has been done before. It's a checkpoint, not the result.

### P5 · Multi-boss — **Dec 2027–Jun 2028** (~250 hrs)

Multi-task policy across 15–25 Hall of Gods bosses. Ascended, then Radiant.

> **Done when:** ≥15 bosses ≥90%, ≥3 Radiant clears.
> 🏦 **BANK IT.** Radiant clears are the headline video.

### P6 · Navigation — **Jul–Aug 2028** (~200 hrs, summer sprint)

Room graph, learned movement skills, traversal between arenas.

### P7 · Hybrid planner + full run — **Sep–Oct 2028** (~270 hrs) 🎯 **THE SWING**

VLM/LLM planner for long-horizon goal selection, RL policies underneath for combat and
movement. Pure RL cannot do this — HAC Explore tops out ~1,000 primitive actions, an any%
run is ~54,000.

> **Nobody has completed a metroidvania with a learned agent.** If it lands, it's novel.
> If it doesn't, P3–P5 are already banked and "how far I got and exactly why it's hard"
> is still a strong writeup.

### P8 · Portfolio — **Oct 2028** (~50 hrs)

Video, technical report, README, SlideRoom items.

---

## Why the sequencing matters

```
BANK   environment        Sep 2027   portfolio-safe
BANK   False Knight       Nov 2027   portfolio-safe
BANK   multi-boss         Jun 2028   portfolio-safe
SWING  full-game hybrid   Oct 2028   pure upside
```

Every bank is independently presentable. The swing is the last four months, after the
application is already safe. That's how you go for the unclaimed result without risking
anything.

## Where it slips, in order

1. **P0 fundamentals run long** — normal; push everything right rather than skipping ahead
2. **P3 lockstep eats a month** — hardest engineering in the project
3. **P6/P7 get cut** — ← designed shock absorber, let these go first
4. Never cut P3 quality. A great environment with one boss beats a shaky one with ten.

## Prior art — read before P4

`AdityaJain1030/HKRL` · `dkleitsas/SilksongRL` · `ailec0623/DQN_HollowKnight` ·
`lucasmr19/Hollow-knight-AI` · Stanford CS224R HK boss project.

Read their code, note what they got wrong, cite them. Boss RL is solved; your novelty is
either the environment quality or the full run.

---

*Rewritten 2026-08-20. Re-cost in Jan 2027 against measured pace.*

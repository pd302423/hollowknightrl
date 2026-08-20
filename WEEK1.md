# Week 1 — Detailed Plan

**Goal of week 1:** prove the environment is *possible*. By Sunday you should have a Python
script printing Hollow Knight's live game state, streamed out of a C# mod you wrote, at 60 Hz.

Everything in Phase 2 (`ROADMAP.md`) builds on that one pipe. If it works, the project is real.
If it doesn't, you find out in 7 days instead of 7 months.

> **"Days" = sessions, not calendar days.** If you're in school, Day 3 might span Wed+Thu.
> Total budget: **~18–25 hours**. Don't skip Day 1 to get to the fun part — Day 1 is what
> stops you bricking your game install in month 4.

---

> ⚠️ **Code in this file is a REFERENCE, not verified truth.** The Day 3 snippet was wrong
> once already (`GetName()` is sealed and cannot be overridden). Before trusting any API
> name here, open `Assembly-CSharp.dll` in ILSpy and confirm it exists with that signature
> in *your* game version.

## The three gates

If the week collapses, these are the only things that matter. Do them in this order.

| Gate | Proof it's done |
|---|---|
| **G1** | A mod you wrote appears in Hollow Knight's main-menu mod list |
| **G2** | That mod prints the knight's HP and position every frame to `ModLog.txt` |
| **G3** | A Python script receives that state over a socket at ~60 msg/sec |

Everything else this week is scaffolding around those three.

---

## Day 1 — Repo, log, and pinning the game (2–3 hrs)

You are about to start modifying a game install. Do this **before** you touch Scarab.

### 1.1 Create the repo

```bash
git init && git branch -M main
```

Create `.gitignore`:

```
# Python
__pycache__/
*.py[cod]
.venv/
venv/
*.egg-info/

# C# / .NET
bin/
obj/
*.user

# Training artifacts
runs/
wandb/
checkpoints/
*.pt
*.ckpt
data/

# Game files — NEVER commit these (copyrighted)
*.dll
*.exe
game/
hollow_knight_Data/

# OS
Thumbs.db
desktop.ini
```

> ⚠️ The `*.dll` rule matters. Committing `Assembly-CSharp.dll` puts Team Cherry's
> code in your public repo. That is the one thing that could get the project taken down.
> Your *own* built mod DLL also gets ignored — that's correct, it's a build output.

Push it public on GitHub. Public from day one — it forces you to keep it presentable.

### 1.2 Start the build log

`LOG.md` is the highest-ROI file in this repo for your portfolio. One entry per session.
Write it **as you go**, not from memory afterward — the confusion is the part worth capturing.

```markdown
# Build Log

## 2026-08-04 — Day 1: repo + game pinning

**Goal:** set up infrastructure, back up and pin the game version before modding.

**Did:**
- ...

**Hit a wall on:**
- ...

**Learned:**
- ...

**Next:**
- ...
```

The "Hit a wall on" section is the one admissions readers actually find interesting.
Never delete an entry. Never clean up the failures.

### 1.3 Find and record your game install

Default Steam path:

```
C:\Program Files (x86)\Steam\steamapps\common\Hollow Knight\
```

If it's not there, in Steam: right-click Hollow Knight → Manage → Browse local files.

Find your **exact version**: launch the game, look at the **bottom-left of the main menu**
(e.g. `1.5.78.11833`). Write it into `LOG.md`. You will need this number when something
breaks in 2027 and you can't remember what you built against.

### 1.4 Pin the version — do not skip this

A Steam update that changes `Assembly-CSharp.dll` will break every hook you write. Two layers
of defense:

**Layer 1 — tell Steam to back off.** Steam → Library → right-click Hollow Knight →
Properties → Updates → set *"Only update this game when I launch it."*
This reduces surprise updates. It does **not** guarantee anything.

**Layer 2 — the real protection. Back up the whole install:**

```bash
cp -r "/c/Program Files (x86)/Steam/steamapps/common/Hollow Knight" "/c/HKBackups/HollowKnight-vanilla-1.5.78"
```

Adjust the version in the folder name to match yours. ~10 GB, worth every byte.

This vanilla backup is your undo button for the entire project. When a mod install
corrupts something, you restore this folder instead of re-downloading and re-pinning.

### 1.5 Directory layout

```bash
mkdir -p mod/src python/hkrl docs media
```

```
hollowknightrl/
├── ROADMAP.md
├── WEEK1.md
├── LOG.md
├── mod/          # C# — the Hollow Knight mod
│   └── src/
├── python/       # Python — env wrapper, RL, tooling
│   └── hkrl/
├── docs/         # notes, protocol spec, findings
└── media/        # screen recordings, screenshots
```

**Day 1 done when:** repo is public on GitHub, `LOG.md` has entry #1, your game version is
written down, and a vanilla backup exists at a path you noted in the log.

---

## Day 2 — Toolchain (2–4 hrs)

### 2.1 Install Scarab

Scarab is the standard Hollow Knight mod installer (github.com/fifty-six/Scarab). It installs
the Modding API — which patches `Assembly-CSharp.dll` and generates the MonoMod hook assembly
your code will use.

1. Download and run Scarab.
2. Let it locate your Hollow Knight install (it usually auto-detects Steam).
3. Install the Modding API when prompted.

### 2.2 Install one existing mod as a control experiment

Install **DebugMod**. Launch the game.

This matters more than it looks: it separates *"modding is broken on my machine"* from
*"my code is broken."* If DebugMod doesn't load, stop and fix that before writing any C#.
Debugging your first mod while the loader itself is broken is a miserable way to lose two days.

Verify all three:
- Main menu shows a mod list including `DebugMod` and the API version
- DebugMod's hotkeys work in-game
- `ModLog.txt` exists and has content

### 2.3 Learn where things live

Memorize these four paths — you'll use them constantly:

| What | Path |
|---|---|
| Game root | `C:\Program Files (x86)\Steam\steamapps\common\Hollow Knight\` |
| **Managed DLLs** (you reference these) | `...\Hollow Knight\hollow_knight_Data\Managed\` |
| **Mods folder** (you deploy here) | `...\Hollow Knight\hollow_knight_Data\Managed\Mods\` |
| **ModLog.txt** (your stdout) | `C:\Users\Parth Dalal\AppData\LocalLow\Team Cherry\Hollow Knight\ModLog.txt` |

Confirm the Managed folder now contains **`MMHOOK_Assembly-CSharp.dll`**. If it's missing,
launch the game once — the API generates it on first run after install. Nothing works without it.

### 2.4 Install the .NET toolchain

- **.NET SDK 8** (for the `dotnet` CLI) — you need it to build.
- An IDE: **Visual Studio Community** or **JetBrains Rider** (free for students — get it, the
  decompiler is built in).
- **ILSpy** (or dnSpy) — standalone decompiler. Non-negotiable. This is how you discover what
  fields and methods actually exist in *your* version of the game, instead of guessing from a
  wiki written for an older one.

### 2.5 Join the Hollow Knight modding Discord

Find the invite from the hk-modding GitHub org. Join, read the pinned messages, don't post yet.

These people have already solved every problem you're about to hit. This is the single biggest
accelerator available to you and it costs nothing.

**Day 2 done when:** DebugMod loads and works, `MMHOOK_Assembly-CSharp.dll` exists,
`dotnet --version` works, ILSpy is installed, Discord joined.

---

## Day 3 — Hello world mod → **GATE 1** (3–5 hrs)

The hardest day of the week. Most of the difficulty is project configuration, not code.

### 3.1 Create the project

```bash
dotnet new classlib -n HkRl -o mod
```

Then delete the generated `Class1.cs`.

### 3.2 The `.csproj` — this is where people get stuck

Two things must be right or nothing loads: **`net472`** (Hollow Knight runs Mono, not modern
.NET) and **`Private=False`** on every reference (never copy the game's DLLs into your output —
that causes type-identity conflicts at load time).

Replace `mod/HkRl.csproj` with:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net472</TargetFramework>
    <AssemblyName>HkRl</AssemblyName>
    <RootNamespace>HkRl</RootNamespace>
    <LangVersion>latest</LangVersion>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
    <AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>

    <!-- EDIT THIS if your Steam library is elsewhere -->
    <HKManaged>C:\Program Files (x86)\Steam\steamapps\common\Hollow Knight\hollow_knight_Data\Managed</HKManaged>
  </PropertyGroup>

  <ItemGroup>
    <Compile Include="src\**\*.cs" />
  </ItemGroup>

  <ItemGroup>
    <Reference Include="Assembly-CSharp">
      <HintPath>$(HKManaged)\Assembly-CSharp.dll</HintPath>
      <Private>False</Private>
    </Reference>
    <Reference Include="UnityEngine">
      <HintPath>$(HKManaged)\UnityEngine.dll</HintPath>
      <Private>False</Private>
    </Reference>
    <Reference Include="UnityEngine.CoreModule">
      <HintPath>$(HKManaged)\UnityEngine.CoreModule.dll</HintPath>
      <Private>False</Private>
    </Reference>
    <Reference Include="UnityEngine.Physics2DModule">
      <HintPath>$(HKManaged)\UnityEngine.Physics2DModule.dll</HintPath>
      <Private>False</Private>
    </Reference>
    <Reference Include="MMHOOK_Assembly-CSharp">
      <HintPath>$(HKManaged)\MMHOOK_Assembly-CSharp.dll</HintPath>
      <Private>False</Private>
    </Reference>
  </ItemGroup>

  <!-- Auto-deploy to the game's Mods folder on every build -->
  <Target Name="DeployMod" AfterTargets="Build">
    <MakeDir Directories="$(HKManaged)\Mods\HkRl" />
    <Copy SourceFiles="$(TargetPath)" DestinationFolder="$(HKManaged)\Mods\HkRl" />
    <Message Importance="high" Text="Deployed HkRl.dll to $(HKManaged)\Mods\HkRl" />
  </Target>

</Project>
```

That `DeployMod` target is a small thing that saves you hundreds of manual copies over two
years. Build → the DLL lands in the game. Set this up now.

### 3.3 The mod

`mod/src/HkRlMod.cs`:

```csharp
using Modding;
using UnityEngine;

namespace HkRl
{
    public class HkRlMod : Mod
    {
        internal static HkRlMod Instance;

        // NOTE: GetName() is `virtual sealed` in this API version — it CANNOT be
        // overridden. The name is passed to the base constructor instead.
        // Verified 2026-08-20 by reflecting on Assembly-CSharp.dll.
        public HkRlMod() : base("HkRl") { }

        public override string GetVersion() => "0.1.0";

        public override void Initialize()
        {
            Instance = this;
            Log("=== HkRl initialize ===");

            // A persistent GameObject that survives scene loads.
            // Its Update() runs once per rendered frame.
            var go = new GameObject("HkRlBridge");
            go.AddComponent<HkRlBridge>();
            Object.DontDestroyOnLoad(go);

            Log("=== HkRl bridge attached ===");
        }
    }
}
```

`mod/src/HkRlBridge.cs`:

```csharp
using UnityEngine;

namespace HkRl
{
    public class HkRlBridge : MonoBehaviour
    {
        private int _frame;

        private void Update()
        {
            _frame++;
            if (_frame % 300 == 0)   // ~ every 5 seconds at 60fps
            {
                HkRlMod.Instance.Log($"heartbeat frame={_frame} t={Time.time:F1}");
            }
        }
    }
}
```

### 3.4 Build and verify

```bash
dotnet build mod -c Release
```

Launch the game. Check both:
1. **Main menu mod list** shows `HkRl 0.1.0`
2. **`ModLog.txt`** contains `=== HkRl initialize ===` and heartbeat lines

Tail the log live in a second terminal:

```bash
tail -f "/c/Users/Parth Dalal/AppData/LocalLow/Team Cherry/Hollow Knight/ModLog.txt"
```

Keep that terminal open for the rest of the project.

### 3.5 If it doesn't load

| Symptom | Cause |
|---|---|
| Mod not in the menu list | DLL isn't in `Managed\Mods\HkRl\`, or wrong target framework |
| `ModLog.txt` shows a load exception | Read the stack trace — usually a missing reference |
| `TypeLoadException` / weird type errors | A reference is missing `<Private>False</Private>` |
| `Assembly-CSharp.dll not found` at build | `HKManaged` path is wrong |
| `MMHOOK_...` not found | Launch vanilla game once to regenerate it |

Still stuck after 45 minutes → ask the modding Discord. Paste the `ModLog.txt` exception.
Do not burn a day on this out of pride; this specific problem is *fully solved* by other people.

> ## 🚩 **GATE 1** — your code is running inside Hollow Knight.
> Screenshot the mod list. Put it in `media/`. Log it. This is a real milestone.

---

## Day 4 — Read game state → **GATE 2** (2–4 hrs)

Now find out what the game will tell you about itself.

### 4.1 Recon with ILSpy first

Open `Assembly-CSharp.dll` in ILSpy. Look at:

- **`HeroController`** — the knight. Find `instance`, and the movement/state fields
  (grounded, dashing, invulnerability/i-frames, `cState`).
- **`PlayerData`** — the save data. Find `instance`, `health`, `maxHealth`, `MPCharge`.
- **`HealthManager`** — enemy/boss HP. Find `hp`. This is what you'll read for boss health.

**Verify these names against your own decompile rather than trusting this document or a wiki.**
Field names differ across game versions, and getting comfortable reading decompiled code is
itself one of the skills this project is teaching you.

Write what you find into `docs/game-api-notes.md` as you go.

### 4.2 Extend the bridge

Replace `Update()` in `HkRlBridge.cs`:

```csharp
private void Update()
{
    var hc = HeroController.instance;
    if (hc == null) return;             // main menu / loading — no knight exists

    var pd = PlayerData.instance;
    if (pd == null) return;

    Vector3 pos = hc.transform.position;

    Vector2 vel = Vector2.zero;
    var rb = hc.GetComponent<Rigidbody2D>();
    if (rb != null) vel = rb.velocity;

    _frame++;
    if (_frame % 30 == 0)               // twice a second, so the log stays readable
    {
        HkRlMod.Instance.Log(
            $"pos=({pos.x:F2},{pos.y:F2}) vel=({vel.x:F2},{vel.y:F2}) " +
            $"hp={pd.health}/{pd.maxHealth} soul={pd.MPCharge}");
    }
}
```

Build, launch, **load an actual save file** (not the main menu — `HeroController.instance`
is null there), and walk around.

### 4.3 Validate against reality

This is the step people skip, and it's the one that catches sign errors that would silently
poison your reward function for months.

- Walk **right** → does `pos.x` increase?
- **Jump** → does `vel.y` go positive then negative?
- **Take a hit** → does `hp` drop by exactly 1?
- **Hit an enemy** → does `soul` increase?
- **Stand still** → is `vel` ≈ 0, or is there jitter? (note the magnitude)

Record the answers in `docs/game-api-notes.md`. Note the *units* — Hollow Knight world units,
not pixels. You'll need the scale when you normalize observations.

> ## 🚩 **GATE 2** — you can observe the game programmatically.

---

## Day 5 — TCP bridge to Python → **GATE 3** (3–5 hrs)

The pipe. After today, Python can see the game.

### 5.1 Protocol decision: newline-delimited JSON

For week 1, use **JSON, one object per line, over TCP loopback**. Not because it's fast — it
isn't — but because you can read it with your eyes when it breaks. Swap to msgpack or a packed
binary struct in Phase 2 once the schema stops changing. Premature optimization here costs you
debuggability at exactly the moment you need it most.

### 5.2 C# side — add a socket server

`mod/src/StateServer.cs`:

```csharp
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace HkRl
{
    public class StateServer
    {
        private TcpListener _listener;
        private TcpClient _client;
        private NetworkStream _stream;

        public bool Connected => _client != null && _client.Connected;

        public void Start(int port = 9999)
        {
            _listener = new TcpListener(IPAddress.Loopback, port);
            _listener.Start();
            _listener.BeginAcceptTcpClient(OnAccept, null);
            HkRlMod.Instance.Log($"StateServer listening on 127.0.0.1:{port}");
        }

        private void OnAccept(IAsyncResult ar)
        {
            try
            {
                _client = _listener.EndAcceptTcpClient(ar);
                _client.NoDelay = true;          // disable Nagle — we want low latency
                _stream = _client.GetStream();
                HkRlMod.Instance.Log("StateServer client connected");
            }
            catch (Exception e)
            {
                HkRlMod.Instance.Log("Accept failed: " + e);
            }
            _listener.BeginAcceptTcpClient(OnAccept, null);   // accept the next one
        }

        public void Send(string json)
        {
            if (!Connected) return;
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(json + "\n");
                _stream.Write(bytes, 0, bytes.Length);
            }
            catch (Exception e)
            {
                HkRlMod.Instance.Log("Send failed, dropping client: " + e.Message);
                _client = null;
                _stream = null;
            }
        }

        public void Stop()
        {
            try { _stream?.Close(); _client?.Close(); _listener?.Stop(); }
            catch { }
        }
    }
}
```

### 5.3 Serialize state with `JsonUtility`

Unity has a built-in JSON serializer. It only handles `[Serializable]` types with **public
fields** (not properties) — a common gotcha.

Add to `HkRlBridge.cs`:

```csharp
[System.Serializable]
public class StateMsg
{
    public int   frame;
    public float t;
    public float x, y, vx, vy;
    public int   hp, maxHp, soul;
}
```

And in `Update()`:

```csharp
var msg = new StateMsg {
    frame = _frame, t = Time.time,
    x = pos.x, y = pos.y, vx = vel.x, vy = vel.y,
    hp = pd.health, maxHp = pd.maxHealth, soul = pd.MPCharge,
};
_server.Send(JsonUtility.ToJson(msg));
```

Start the server in `Initialize()` (`_server = new StateServer(); _server.Start();`) and
send **every frame** now — no `% 30` throttle. You want to measure the real rate.

### 5.4 Python side

```bash
py -m venv python/.venv
```

Activate it (`source python/.venv/Scripts/activate` in bash).

`python/listen.py`:

```python
"""Connect to the HkRl mod and print live game state + throughput."""
import json
import socket
import time

HOST, PORT = "127.0.0.1", 9999


def main():
    print(f"connecting to {HOST}:{PORT} ...")
    sock = socket.create_connection((HOST, PORT))
    sock.setsockopt(socket.IPPROTO_TCP, socket.TCP_NODELAY, 1)
    print("connected")

    buf = b""
    n, t0 = 0, time.time()

    while True:
        chunk = sock.recv(65536)
        if not chunk:
            print("server closed the connection")
            return
        buf += chunk

        while b"\n" in buf:
            line, buf = buf.split(b"\n", 1)
            if not line.strip():
                continue
            s = json.loads(line)
            n += 1

            if n % 60 == 0:
                dt = time.time() - t0
                print(
                    f"{s['frame']:>7}  "
                    f"pos=({s['x']:7.2f},{s['y']:7.2f})  "
                    f"vel=({s['vx']:6.2f},{s['vy']:6.2f})  "
                    f"hp={s['hp']}/{s['maxHp']}  soul={s['soul']:>3}  "
                    f"| {n/dt:5.1f} msg/s"
                )


if __name__ == "__main__":
    main()
```

### 5.5 Run it

1. Launch Hollow Knight, load a save.
2. `python python/listen.py`
3. Move the knight. Watch the numbers change.

**Check the message rate.** You should see ~60 msg/s (or your monitor's refresh rate — HK
renders at the display rate). If it's much lower, something is blocking; note it in the log.
This number is your first throughput measurement and you'll be comparing against it all year.

Windows Firewall may prompt on first `TcpListener.Start()`. Allow it for private networks.
It's loopback-only, so nothing is exposed off your machine.

> ## 🚩 **GATE 3** — Python can see inside Hollow Knight in real time.
> **Screen-record this.** Game on one side, terminal scrolling on the other. This 20-second
> clip is the opening shot of your portfolio video in 2028. You cannot re-shoot it later
> with the same honesty.

---

## Day 6 — The RL track (3–4 hrs)

Switch brains completely. Environment work and RL work are different skills; interleaving them
weekly keeps both moving and stops either from going stale.

### 6.1 Install

```bash
pip install torch --index-url https://download.pytorch.org/whl/cu124
```

```bash
pip install "gymnasium[classic-control,box2d]" numpy tensorboard wandb
```

Verify CUDA is actually being used — a surprising number of people train on CPU for months
without noticing:

```bash
python -c "import torch; print(torch.__version__, torch.cuda.is_available())"
```

### 6.2 Run CleanRL's PPO

Clone CleanRL somewhere **outside** this repo (it's a reference, not a dependency):

```bash
git clone https://github.com/vwxyzjn/cleanrl.git "/c/Users/Parth Dalal/reference/cleanrl"
```

Run `ppo.py` on CartPole. Get it to solve (episodic return ~500). Watch the TensorBoard curves.

### 6.3 Now read it line by line

Open `cleanrl/cleanrl/ppo.py`. It's ~250 lines and it is the best-written RL code you will find.
Read it with a notebook open and make sure you can answer:

- What shape is the rollout buffer, and why does it have those dimensions?
- What is GAE computing, and why not just use the discounted return?
- Why are advantages normalized per-minibatch?
- What does the clipped surrogate objective prevent?
- Why are there multiple epochs over the same batch of data?
- What does the entropy bonus do, and what happens if you set its coefficient to 0?

Write your answers in `docs/rl-notes.md`. If you can't answer one, that's your reading
assignment — not a reason to move on.

### 6.4 Start your own

Create `python/rl/ppo_scratch.py`. **Empty file.** Do not copy CleanRL into it.

You'll write your own PPO over the next 2–3 weeks, referring to CleanRL only when stuck.
This is the difference between "I ran an RL library" and "I understand PPO" — and it is the
difference an MIT interviewer will find in about 90 seconds of conversation.

Start today with just the environment loop and the network definition.

---

## Day 7 — Consolidate (2–3 hrs)

Don't skip this. A week of work you can't explain is worth much less than a week you can.

### 7.1 Write `README.md`

Not a stub. What the project is, the architecture diagram from `ROADMAP.md`, current status,
how to build the mod, how to run `listen.py`. Assume a stranger is reading.

### 7.2 Write `docs/protocol.md`

Spec the state message: every field, its type, its units, its range, and where in
`Assembly-CSharp.dll` it comes from. This document will grow into the observation space.
Writing it now, while the schema is 9 fields, sets a habit that pays off when it's 150.

### 7.3 Back up the *modded* install

You now have a working modded install. Snapshot it separately from the vanilla backup:

```bash
cp -r "/c/Program Files (x86)/Steam/steamapps/common/Hollow Knight" "/c/HKBackups/HollowKnight-modded-week1"
```

### 7.4 Consolidate the log and commit

Fill in all seven `LOG.md` entries properly, including the failures. Then commit and push.

### 7.5 Plan week 2

Week 2's target, so you don't have to decide on Monday:

1. **Read boss HP** — find `HealthManager` on a boss, expose its `hp`. Test on any enemy first.
2. **Read the boss's FSM state name** — `PlayMakerFSM` components, `Fsm.ActiveStateName`.
   This is the killer feature from `ROADMAP.md` §1.2. Getting the string `"Slam"` out of
   False Knight is the moment this project stops being a screen-scraper.
3. **Input injection** — make the mod press a button, so the knight jumps on command from Python.

---

## Week 1 scorecard

| | Deliverable |
|---|---|
| ☐ | Public GitHub repo with real commit history |
| ☐ | `LOG.md` with 7 honest entries |
| ☐ | Vanilla + modded game backups, version recorded |
| ☐ | **G1** — your mod loads in Hollow Knight |
| ☐ | **G2** — mod logs live player state |
| ☐ | **G3** — Python receives state at ~60 Hz |
| ☐ | Screen recording of G3 in `media/` |
| ☐ | CleanRL PPO solving CartPole, and you can explain it |
| ☐ | `docs/game-api-notes.md`, `docs/protocol.md`, `docs/rl-notes.md` |
| ☐ | Modding Discord joined |

### If you only have 6 hours this week

Do Day 1 (§1.4 backup especially), Day 3, and Day 5. G1 + G3 are the week.
Everything else can slide to week 2 without hurting anything.

---

## Two habits to start now

**1. Log the failures, in the moment.** The entry that says *"lost 3 hours because
`HeroController.instance` is null on the main menu and I didn't null-check"* is more valuable
to your portfolio than any clean result. It's evidence of real engineering. Nobody believes a
two-year project that went smoothly.

**2. Record video constantly.** Storage is free. A 20-second clip of the very first janky
state-streaming demo cannot be recreated in 2028. Every milestone gets a clip.
ROADMAP §7 explains why this matters more than the code.

---

*Written 2026-08-04. Update as you go — if a step here is wrong, fix it and note the fix in `LOG.md`.*

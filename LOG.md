# Build Log

Every work session gets an entry. Write it while you work, not afterwards.
Never delete an entry. Never tidy up the failures — they are the most valuable part.

---

## 2026-08-18 — Day 1: repo setup + pinning the game

**Goal:** get version control running and make a safe copy of the game before any modding.

**Machine baseline:**
- Git 2.54.0
- Python 3.14.4
- .NET SDK: NOT INSTALLED (day 2 task)
- Hollow Knight: `C:\Program Files (x86)\Steam\steamapps\common\Hollow Knight` (7.5 GB, clean vanilla)
- Free space on C: 202 GB

**Game version:** _______________  <-- FILL IN (main menu, bottom-left corner)

**Did:**
- Created git repo, `.gitignore`, folder structure
- Backed up vanilla game to `C:\Users\Parth Dalal\HKBackups\HollowKnight-vanilla`
  - VERIFIED: 1745 files both sides, 7,972,876,294 bytes exact match
  - `Assembly-CSharp.dll` md5 = `929aebf85b6060997404f3b3ff1738ed` (vanilla, pre-mod)
  - Re-run that md5 on the live install later to tell whether it has been patched
- (todo) Created GitHub account + pushed
- (todo) Set Steam to "only update this game when I launch it"

**Hit a wall on:**
-

**Learned:**
-

**Next:** Day 2 — install .NET SDK, Scarab, DebugMod, ILSpy. Join modding Discord.

---

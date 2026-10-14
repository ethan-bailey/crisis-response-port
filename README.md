# Crisis Response — iPhone Web Port Research

This repository is for a source-preserving, evidence-driven effort to make the original *Crisis Response* playable on iPhone 16 through a web runtime if feasible.

**The original archive has been located** in the ChatGPT Library as `Crisis Response.rar` (about 67.9 MB). It has not yet been transferred into this repository or a build workspace.

## Important distinction

A separate generated file named `CrisisResponseMobile_iPhone.zip` is a from-scratch HTML canvas adaptation. It explicitly does not contain the original Windows executable or original game assets. It is not a faithful port and must not be represented as one.

## Start here

Read [the reverse-engineering and parity plan](docs/REVERSE_ENGINEERING_PLAN.md) for source facts, constraints, feasibility gates, and staged work.

## Current state

- GitHub access verified; this repository is private.
- Original RAR located outside the repository in the ChatGPT Library.
- Prior notes identify a Unity 2017.2.0f3 Windows build with `Assembly-CSharp.dll`; details remain to be verified from the archive.
- No verified faithful web build exists yet.

The goal is to preserve original logic and assets wherever possible, document unavoidable changes, and validate behavior subsystem by subsystem. Do not claim full parity without evidence and tests.

# Copilot Instructions for Space Escaper

## Project Status

- This project is an active work-in-progress.
- Current snapshot date: May 11, 2026.
- Unity version in use: 6000.2.6f2.

## Working Rules

- Keep changes focused and minimal.
- Prefer fixing the root cause over patching symptoms.
- Preserve the existing Unity project structure and naming unless a change is explicitly requested.
- Do not remove or rename gameplay systems without checking all call sites first.
- Avoid destructive git or file operations unless explicitly requested.

## Audio System Notes

- The project uses `AudioSystem` as the central audio controller.
- `AudioSystem` exposes `AudioClip` references in the Inspector.
- The GameObject named `AudioSystem` is expected to exist in the main gameplay scene.
- Main menu music should start automatically when the game loads.
- Starting gameplay should stop menu music and start in-game music.
- Button sounds should be triggered from menu actions such as play, shop open, and shop close.
- Explosion, engine, coin, ship select, and ship purchase sounds should use `AudioSystem` methods.

## Unity / VS Code Notes

- Unity compilation is the source of truth for script validity.
- VS Code project model issues can happen independently of Unity compilation.
- If a script is visible in Unity and compiles there, do not assume a VS Code warning means the game is broken.
- When audio or scene behavior seems wrong, check whether the relevant method is actually called from the gameplay flow.

## README Expectations

- Keep the README clear that the project is still in progress.
- Keep the latest date visible near the top of the README.

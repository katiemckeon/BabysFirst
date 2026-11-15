<p align="center">
  <picture>
    <source media="(prefers-color-scheme: dark)" srcset=".github/assets/hero-dark.svg">
    <img src=".github/assets/hero.svg" alt="pot pie labs" width="420">
  </picture>
</p>

# Baby's First

A series of toddler-toy-styled 3D picture books that explain grown-up technical concepts with
farm animals. Made with Unity 6 (URP), played in the browser.

This repository is public so the game can be built and hosted in the open. It is **not open
source** (see [Licences](#licences)), and it does not build on its own: the paid asset packs it
uses live in a private repository.

## Clone

```bash
git clone --recurse-submodules <this repo's URL>
# or, in an existing clone:
git lfs install
git submodule update --init
```

Both submodules are private. `Assets/ThirdParty` (asset packs) is required to open or build the
project; `Tools` (authoring tools) is not.

Open the folder with Unity 6000.6.8f1 and open `Assets/_Project/Scenes/Intro.unity`.

## Layout

| Path | What it is |
|---|---|
| `Assets/_Project/Core/` | Code every episode uses: `Scripts/` (`Palette.cs` is the source of truth for every color), `Data/` (`GameCatalog.cs`, the list of episodes), `Services/` |
| `Assets/_Project/Episodes/<Name>/` | One folder per episode holding everything it owns: its scene, scripts, narration, materials and UI |
| `Assets/_Project/Menus/` | The intro and the main menu |
| `Assets/_Project/Shared/` | Assets used by more than one episode: music, fonts, storybook UI sprites |
| `Assets/_Project/Scenes/` | `Intro` and `Menu`, the shell around the episodes |
| `Assets/_Project/Settings/` | Render pipeline, post-processing and input settings |
| `Assets/_Project/Editor/` | Editor code: the web build, the storybook UI kit, import settings, the project check |
| `Assets/TextMesh Pro/` | TextMesh Pro essential resources |
| `Assets/ThirdParty/` | Asset Store packs (private submodule) |
| `Tools/` | Authoring tools (private submodule); nothing in the game depends on it |

An episode ships once it is listed with a scene name in `GameCatalog.cs`; the menu and the web
build both read that list.

## Building for the web

In the Editor: **Baby's First > Build Web**. From the command line, with the Editor closed:

```bash
Unity -batchmode -quit -projectPath . -buildTarget WebGL -executeMethod BabysFirst.EditorTools.WebBuild.Build
```

The build lands in `Builds/Web/` (git-ignored). To try it locally:

```bash
cd Builds/Web
python -m http.server 8000
```

`.github/workflows/web.yml` runs the same build with [GameCI](https://game.ci/) and publishes it
to GitHub Pages.

**Baby's First > Check Project** opens every scene and reports missing scripts and broken
references; run it after moving assets.

## Git setup

`.gitattributes` keeps text files on LF line endings and stores binaries (audio, images, fonts,
font atlases) in Git LFS. Install Git LFS before cloning or committing.

## Licences

Copyright (c) 2026 Katie McKeon (PotPie Labs). All rights reserved; see [`LICENSE`](LICENSE).
The source is here to be read, not reused.

Third-party material keeps its own licence; see [`THIRD_PARTY_NOTICES.md`](THIRD_PARTY_NOTICES.md).
In short: the fonts are under the SIL Open Font License, and the Asset Store packs are under the
Unity Asset Store EULA and are not part of this repository.

# Baby's First

## Clone

```bash
git clone --recurse-submodules <this repo's URL>
# or, in an existing clone:
git submodule update --init
```

Open the folder with Unity 6000.6.8f1 and open `Assets/Scenes/Rehypothecation.unity`.

## Layout

| Path | What it is |
|---|---|
| `Assets/Scenes/` | Game scenes |
| `Assets/Scripts/` | Runtime code. `Palette.cs` is the source of truth for every color |
| `Assets/Editor/Storybook/` | The UI kit that builds the storybook screens |
| `Assets/Editor/WebBuild.cs` | The web build entry point |
| `Assets/Fonts/`, `Assets/UI/` | Fonts (SIL Open Font License) and generated UI sprites |
| `Assets/ThirdParty/` | Asset Store packs (private submodule) |
| `AgentScripts/` | Scene builder scripts; they never ship |

## Building for the web

In the Editor: **Baby's First > Build Web**. From the command line, with the Editor closed:

```bash
unity build . --target WebGL --execute-method BabysFirst.EditorTools.WebBuild.Build
```

The build lands in `Builds/Web/` (git-ignored) and is ready to serve from GitHub Pages: copy its
contents to the branch or folder that Pages publishes.

```
cd Builds/Web
python -m http.server 8000
```

## Licences

Code in this repository: not yet licensed for reuse. Fonts in `Assets/Fonts/` are under the SIL
Open Font License; the licence texts sit beside them. Asset Store packs remain under the Unity
Asset Store EULA and are not part of this repository.

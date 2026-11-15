# Baby's First

Have you ever felt frustrated by:
- A concept that seemingly made someone else rich?
- Smart people jokes you weren't in on?
- Deliberately opaque vocabulary?

*Baby's First* is a learning resource that empowers you **and your children** to unlock an ensemble of morally ambiguous behaviors.

<p align="center">
  <a href="https://katiemckeon.github.io/BabysFirst/">
    <img src=".github/assets/play.svg" alt="Play" width="300">
  </a>
</p>

> [!WARNING]
> This is satire. I would not recommend you let your child play this. Nothing spicy, but probably the humor is lost...

## Developer Notes

Two private submodules are required (audio generation, private AI workflows, and third party assets). The commands below assume you have access to them.

Requirements: Taskfile, Unity `6000.6.8f1` (web builder), Musehub, MuseScore, Claude Code.

### Taskfile commands

Each command stands alone. Run whichever one you need:

```bash
task build                        # make the web version
task up                           # serve locally
task submodule:sync -- main       # update private submodules
task claude muse transfer takes   # transform local takes into game narration
```

### Starting fresh 

Run these in order:

```bash
git clone git@github.com:katiemckeon/BabysFirst.git
cd BabysFirst
task submodule:init   # check out the private submodules
task build            # make the web version
```

## Licences

Copyright (c) 2026 Katie McKeon. All rights reserved; see [`LICENSE`](LICENSE).

<p align="center">
  <img src=".github/assets/hero.svg" alt="pot pie labs" width="200">
</p>


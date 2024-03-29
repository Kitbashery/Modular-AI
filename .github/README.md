<!-- ONLINE DOCUMENTATION FOUND @ https://kitbashery.com/docs/modular-ai -->

![](https://kitbashery.com/assets/images/kitbashery-github-banner.jpg)

[![Unity Version](https://img.shields.io/badge/Unity-2021.3%2B-blue.svg)](https://unity3d.com/get-unity/download)
[![GitHub](https://img.shields.io/github/license/kitbashery/modular-ai.svg)](https://github.com/Kitbashery/Modular-AI/blob/main/.github/LICENSE.md)
[![OpenUPM](https://img.shields.io/badge/Install-openUPM-blue)](https://openupm.com/packages/com.kitbashery.modular-ai.html)
[![DevKit](https://img.shields.io/badge/Dev%20Kit-.unitypackage-blue)](https://github.com/Kitbashery/Modular-AI/releases/download/Development-Package/Kitbashery_ModularAI.unitypackage)
[![Documentation](https://img.shields.io/badge/Docs-Kitbashery.com-%23ffaf0c)](https://kitbashery.com/docs/modular-ai)
[![Contributing](https://img.shields.io/badge/Contribute-guidelines-lightgrey)](https://github.com/Kitbashery/.github/blob/main/.github/CONTRIBUTING.md)

#### For a more performant, stable and fully featured solution check out our latest asset:
[![Smart GameObjects](https://kitbashery.com/assets/images/smart-gameobjects-uas-sale.jpg)](https://assetstore.unity.com/packages/slug/248930?aid=1100lvf66)

# Modular-AI
Modular AI is an inspector based visual behaviour designer.

## Features:
* Implements competing utility theory behaviours for dynamic AI behaviours.
* Zero string comparisons or calls to reflection.
* Behaviours can be fully configured during runtime.
* Not tied to a specific pathfinding solution.
* Fully extendable via code modules.

![](https://kitbashery.com/assets/images/kitbashery-modular-ai-agent-component.jpg)

### Built-in Modules:

#### Unity Pathfinding:
* Flee/Follow
* Wander
* Patrol

#### Memory:
* Remember players, AI agents, or environment objects.
* Focus/target objects in memory
* Invoke custom events.

#### Sensors:
* Eye-level scans
* Physics Scan Options for 2D & 3D
* Integrated with memory

#### Animation:
* Idle, Walk, Run & Jump
* Attack, Death Animations
* Dynamic Hit reactions

# Getting Started:
All components can be found under Kitbashery in the component menu:
![](https://kitbashery.com/assets/images/kitbashery-modular-ai-component-navigation.jpg)

Online documentation & scripting API is found at:

https://kitbashery.com/docs/modular-ai

## Utility Theory:

Modular AI uses utility theory for its AI behaviour logic. An AI agent can have as many behaviours as you want.

#### Note: Since v.2.0.2 Modular AI supports linear state-machine-like evaluation by default with optional competing utility theory behaviours.

### Behaviours:
Behaviours are comprised of conditions and actions and have a score value. The behaviour with the score that best meets the score type you set will execute its actions.
### Conditions:
Conditions are true/false statements based on what the AI knows about the game world. If a condition meets its desired state then it will add its score to the behaviour's total score.
### Actions:
Actions are executed in the order they are arranged if a behaviour's total score meets the score type better than any other behaviour.


## Module Scripting:
Take a look at the docs for [ExampleModule.cs](https://kitbashery.com/docs/modular-ai/example-module.html) for how to create your own modules.



---
The name Kitbashery & all associated images Copyright &copy; 2023 Kitbashery. All Rights Reserved.

# WaEvent

#### Intro
This is a tool for Unity3D game engine. Aim for easy editing animation events and decouple logic event from artwork assets. Inspired by [Mecanim Event System](https://github.com/Ginurx/MecanimEventSystem).

#### Structure

`WaEvent/Core` Codebase

`WaEvent/Editor` Editor and inspector

`WaEvent/Util` 

#### Intall

You can either

 - add this repo as a submodule to your project
 - clone/download and unzip `WaEvent` folder to your project
 
#### Tutorial

1. Create a new event data set via context menu `WaEvent/New Event Data`.

2. Setup event data step by step.

![](WaEvent/Docs/1.png)

3. Attach `AnimatorEventEmitter` to your `Animator` instance and give it the data just created.

![](WaEvent/Docs/2.png)

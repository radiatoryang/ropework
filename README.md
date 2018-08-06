# ropework
<img width=30% align=right src=https://raw.githubusercontent.com/radiatoryang/ropework/master/ropework_demo1.gif> 

a visual novel framework template for [Yarn Spinner](https://github.com/thesecretlab/YarnSpinner/) / Unity C#... where you can use [Yarn](https://github.com/InfiniteAmmoInc/Yarn) scripts to control a lot of scene drawing (scene background, sprites), and in the future, even manage characters / audio / video

it is technically possible to use this to make a whole visual novel in Unity without knowing any C# code, but that's not recommended... it is intended more as a simple framework / template for you to modify, or to learn from

## quick install
1. clone or download this repo into a project folder
2. open the project folder in Unity (currently being developed in Unity 2017.4.8)
3. open the example scene(s), edit the scripts, dig around and explore

## proper install
1. install Unity, create project, import a [YarnSpinner.unitypackage release](https://github.com/thesecretlab/YarnSpinner/releases)
2. import Ropework.unitypackage (see ["Releases"](https://github.com/radiatoryang/ropework/releases) )
3. add RopeworkManager prefab to a scene (TODO: set folder path to detect and load Yarn scripts)
4. all sprites and sounds go in /Assets/Resources/, make sure they're imported as sprites (TODO: let you manually assign sprites too)
5. download the [Yarn Editor](https://github.com/InfiniteAmmoInc/Yarn) and start writing! make sure you read the [YarnSpinner documentation](https://github.com/thesecretlab/YarnSpinner/blob/master/Documentation/YarnSpinner-Unity/YarnSpinner-with-Unity-StepByStep.md) so you understand how it all fits together too

## usage
see the [Ropework wiki](https://github.com/radiatoryang/ropework/wiki) for documentation and examples

## uses the following:
- Yarn https://github.com/InfiniteAmmoInc/Yarn
- YarnSpinner https://github.com/thesecretlab/YarnSpinner/

## license?
MIT

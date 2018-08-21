# ropework
<img width=30% align=right src=https://raw.githubusercontent.com/radiatoryang/ropework/master/ropework_demo1.gif> 

a visual novel framework template built on [Yarn Spinner](https://github.com/thesecretlab/YarnSpinner/) / Unity C#... where you can use [Yarn](https://github.com/InfiniteAmmoInc/Yarn) scripts to control scene drawing (scene background, sprites, fades) and manage characters / audio

it is technically possible to use this to make a whole visual novel in Unity without knowing any C# code, but that's not recommended... it is intended more as a simple framework / template for you to modify, or to learn from

### simple in-browser demo example (Unity WebGL)
https://radiatoryang.github.io/ropework/demo_simple/

## quick install
1. clone or download this repo into a new folder
2. open your new project folder in Unity (currently being developed in Unity 2018.2.4f1)
3. open the example scene(s), edit the scripts, dig around and explore
4. a "blank" template is there for you to use too

## proper install
1. install Unity, create project, import a [YarnSpinner.unitypackage release](https://github.com/thesecretlab/YarnSpinner/releases)
2. import Ropework.unitypackage (see ["Releases"](https://github.com/radiatoryang/ropework/releases) )
3. add RopeworkManager prefab to a scene (TODO: set folder path to detect and load Yarn scripts)
4. all sprites and sounds go in /Assets/Resources/, make sure they're imported as sprites (TODO: let you manually assign sprites too)
5. download the [Yarn Editor](https://github.com/InfiniteAmmoInc/Yarn) and start writing! make sure you read the [YarnSpinner documentation](https://github.com/thesecretlab/YarnSpinner/blob/master/Documentation/YarnSpinner-Unity/YarnSpinner-with-Unity-StepByStep.md) so you understand how it all fits together too

## usage
see the [Ropework wiki](https://github.com/radiatoryang/ropework/wiki) for documentation, API, and examples

## uses the following:
- Yarn https://github.com/InfiniteAmmoInc/Yarn
- YarnSpinner https://github.com/thesecretlab/YarnSpinner/

## license?
MIT

## assets used in example project:
- Visual Novel Tutorial Set (public domain) https://opengameart.org/content/visual-novel-tutorial-set
- Louis George Cafe font ("100% free") https://www.dafont.com/louis-george-caf.font
- Lovely Piano Song by Rafael Krux (public domain) https://freepd.com
- Comic Game Loop - Mischief by Kevin MacLeod (public domain) https://freepd.com 

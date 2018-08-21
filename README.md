# ropework
<img width=30% align=right src=https://raw.githubusercontent.com/radiatoryang/ropework/master/ropework_demo2.gif> 

a visual novel framework template built on [Yarn Spinner](https://github.com/thesecretlab/YarnSpinner/) / Unity C#... where you can use [Yarn](https://github.com/InfiniteAmmoInc/Yarn) scripts to control scene drawing (scene background, sprites, fades) and manage characters / audio

it is technically possible to use this to make a whole visual novel in Unity without knowing any C# code, but that's not recommended... **it is intended more as a simple framework / template for you to modify, or to learn from**

again: **this is aimed at people who already know some Unity! if you're a total beginner, then do some Unity tutorials first!** I recommend [Unity's video tutorials](https://unity3d.com/learn) or the [Catlike Coding text tutorials](https://catlikecoding.com/unity/tutorials/).

### simple in-browser demo example (Unity WebGL)
https://radiatoryang.github.io/ropework/demo_simple/

## download / install
download a pre-configured blank Unity project template OR a .unitypackage from **[Releases](https://github.com/radiatoryang/ropework/releases)**

## usage
see the **[Ropework wiki](https://github.com/radiatoryang/ropework/wiki) for documentation, API, and examples**

your basic workflow will look like this:
- in the [Yarn Editor](https://github.com/InfiniteAmmoInc/Yarn), write a Yarn.txt script and save it into your Unity project /Assets/ folder
- in Unity on the RopeworkManager prefab, assign the Yarn script to the YarnSpinner DialogueRunner's "Source Text" array (in the Unity inspector)
- press Play and see if it works!... keep in mind that, by default, YarnSpinner will start with a node called "Start"

## example Yarn script
<code><pre>// sets background image to sprite called "bg_office"
<<Scene @ bg_office>>

// adds actor named "Eve" using sprite "cool-girl", placed in left-half + center of screen, with green text label
<<Act @ Eve, cool-girl, left, center, green>>
Eve: Finally, a quiet day at the office. Maybe I'll be able to get some work done.

// adds actor "Adam" with sprite "biz-guy" off-screen right, with blue-ish text label
<<Act @ Adam, biz-guy, 1.25, center, #226677>>
// animate Adam into new position in right-half + center, within 0.5 seconds
<<Move @ Adam, right, center, 0.5>>
// start playing audioclip "music_funny" at 100% volume, loop forever
<<PlayAudio @ music_funny, 1.0, loop>>

Adam: Hey Eve! I have a question!
Eve: Oh no...</pre></code>

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

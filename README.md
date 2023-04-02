# 2D Platformer Template

This is a simple 2D Platformer template that is a good starting point for any Platformer game. This is made using Godot 4 and is a C# project.

## General Information

This template includes the following features:
* A playable character with the following features
    * **Coyote Time** - Give a fraction of a second after leaving the platform to still be able to jump (`0.0f` to disable)
    * **Jump buffer** - Give a fraction of a second after pressing jump to still perform the action (`0.0f` to disable)
    * **Multiple Jumps** - Give the player an option to jump more than once
    * **Terminal Velocity** - Limit the player's fall speed to give more air control if needed
    * **Gravity scale** - Multiply gravity by scale on fall (or/and when releasing jump button) (on stop jump and fall)
    * **Apex Bonus** - Give a small nudge to the player when apex is reached in a jump (`Vector2(0.0f, 0.0f)` for non)
* A simple example of Animation Tree and multiple animation sprite sheets
* A simple level to play around in and test out the capabilities
* Player Start locator for the level that makes the camera in the level follow the character

## How To Use?

* Clone the repository `git@github.com:odinn1984/Godot-4-2D-Platformer-Template.git`
    * Feel free to also fork the repository instead
    * You can clone this repo to any directory (e.g: the name of the project you want to create)
* In the directory that you cloned the repo to:
    * Open `project.godot` file
    * Update `config/name` value on line 13 to whatever you want to call your project (e.g: `config/name="My Awesome Project"`)
    * Do the same for `project/assembly_name`
* Open the project in Godot
* Open your favorite IDE/Text Editor
* Have fun :)

## TODO

* Introduce Multiplayer Support
* Add extendable abilities system (e.g: Dash, Blink, etc...)

## Credits
* Character sprite is something I got a while back and don't remember who made it... If you know (or are) the maker of this free asset please tell me so I can give you credit :)

# Zombie Standard Time

A launcher for [State of Decay](http://undeadlabs.com/stateofdecay/) that lets _you_ take control of time in the persistent world!

## The State of Decay Persistent World

State of Decay [simulates in-game events in the persistent world](http://forums.undeadlabs.com/showthread.php?541-Faq) while you're not playing the game.

Survivors hunt and gather, use resources, and die horrible and messy deaths without you.

The amount of time since your last play session is used to calculate these in-game events and the passage of time in the persistent world.

The longer you don't play the game, the longer your survivors have to fend for themselves without you!

With Zombie Standard Time, _you_ control how much time has passed since you last played.

## How does Zombie Standard Time work?

ZST _is not_ a patch or a crack. We don't modify or change any game files, and you can stop using ZST at any time.

The technical explanation is that ZST intercepts calls from the StateOfDecay.exe process to check the current time in Windows. We hook GetSystemTimeAsFileTime with [EasyHook](http://easyhook.codeplex.com/), and respond with a date and time of our choosing.

State of Decay receives an adjusted time based on the persistent world settings _you_ choose.

We call this timezone _Zombie Standard Time_.

**Enable simulated time**

Simulate the persistent world as normal between play sessions.

In-game time will proceed as Undead Labs intended.

**Disable simulated time**

Prevent simulation of the persistent world between play sessions.

No in-game time will pass when you load your save game.

**Limit simulated time**

Allow simulation of the persistent world between play sessions up to a maximum real-world time span.

If you haven't played for a week, pretend you've only been gone for an hour.

**Force simulated time**

Force simulation of the persistent world between play sessions for a fixed real-world time span.

Can your survivors last a month without you?

## Instructions

**Disclaimer**

You use Zombie Standard Time at your own risk!

We're messing with the game's time keeping functions, and that may (or may not) cause problems, for example:

*   You've Played counter in Steam may not be correct
*   Last Played date in Steam may not be correct
*   Save games may be unusable if the game is patched
*   Some in-game events may not occur
*   Some in-game functions may not work as expected (repairs, healing etc.)

Undead Labs _has not_ approved this launcher, and may change the game at any time to prevent it working.

**Requirements**

Zombie Standard Time requires [Steam](http://store.steampowered.com/) and a Steam-activated copy of [State of Decay](http://store.steampowered.com/app/241540/).

If you're not using Windows 8, Zombie Standard Time requires [Microsoft .NET Framework 4.5](http://www.microsoft.com/en-us/download/details.aspx?id=30653).

Windows 8 includes .NET Framework 4.5 out of the box, so you don't need to install anything.

.NET Framework 4.5 requires Windows Vista SP2 or higher. That means Zombie Standard Time won't run on Windows XP!

The latest version of Zombie Standard Time is v1.0.0.2.

**To use Zombie Standard Time:**

1.  Download [ZombieStandardTime.zip](https://github.com/Spksh/ZombieStandardTime/releases/download/v1.0.0.2/ZombieStandardTime.1.0.0.2.zip)
2.  Unzip to a folder on your hard drive
3.  Run ZombieStandardTime.exe
4.  Select your Steam profile and set persistent world options
5.  Click "Launch"

Having problems? **Run ZST as an administrator** (right-click ZombieStandardTime.exe, select _Run as administrator_).

**Known issues:**

*   ZST supports one in-game profile; detected time offsets may be incorrect with more than one profile
*   Crossing daylight savings boundaries in your local timezone may cause the loss or gain of one hour when ZST calculates time offsets

**Changes:**

*   v1.0.0.2
    *   Resolve launch crash on some CPUs by using DateTime.Now after Process.WaitForExit() instead of Process.ExitTime
*   v1.0.0.1
    *   Guess Steam install location instead of crashing if SteamPath registry key is missing
    *   Add "Unknown" profile instead of crashing if no Steam profiles are found
    *   Explicitly check that second StateOfDecay.exe process (started by internal StateOfDecay.exe launcher) is not null before returning
    *   Add timeout when waiting for second StateOfDecay.exe process to start
    *   Change "invalid path to userdata folder" error to indicate that State of Decay must be run one time without ZST to create required folders
*   v1.0.0.0
    *   Initial release

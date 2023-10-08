# Undertale SimRec (Simulator and Recorder)

This project aims to simulate and calculate the probabilities of Undertale speedruns. This repository contains two parts:

* Simulator - A C++ program
* Recorder - A [UndertaleModTool](https://github.com/krzys-h/UndertaleModTool) script that creates an Undertale mod

The recorder is a mod that outputs files that are then read by the simulator to generate the probability results.

# How to install and use the recorder mod


## INSTALLING
To install the mod, download the patch in the releases and apply it to an unmodified `data.win` for the linux version 1.001.
To apply the patch you can use resources such as [this website](https://www.marcrobledo.com/RomPatcher.js/). Replace the
old `data.win` with the patched one and the mod will be ready to play.

## INSTRUCTIONS

To start recording, all needed is to start a segment and start playing. In the top left, you have information about the current segment, including how to start it and what you should be doing. Here are the general instructions for using this mod:

* 0-reset: Make sure to delete all the Undertale savefiles. If you use a macro that wipes out all files in your Undertale save folder, watch out since that's where the recording data is stored as well!

* Before you start a segment, you can select the segment you want to start at using Page Up and Page Down (and to speed that up, press SHIFT and CTRL which will increment the amount of segments skippped). Though generally you will want to start from the beginning, if you have any specific reason to start at some other place, you can. Press T to warp to the selected segment

* If you want to restart a segment, you can do so with R.

* Sometimes, the next segment will require you to do something specific. In this case, the mod will lock your player and tint your character **PURPLE** to warn you that you should read the instructions. You can unlock pressing SPACE

* Once you start a segment, you will have a brief reminder of what the segment requires you doing. In general, you will see these keywords:

* * PROCEED - If you see this, continue as if you were in a normal Genocide run! Do everything you would normally. At the end of this there's a list of the routing choices considered.

* * WALK - If you see this, then you will do the same as normal, but you will not GRIND for or get random ENCOUNTERs.

* * GRIND-  If you see this, then you are assumed to be in the middle of a grind, and you do the same as you would: Kill an encounter and then swap room for the next.

* * GRIND AT THE END - If you see this, consider that you have to walk to the end of the room and grind an encounter. You should try to do it as optimally you would in a normal run, trying to get as close to the end of the room but without actually transitioning!

If you have doubts about what a "normal" Genocide run is to the eyes of the mod, you must do the following:

* Kill first Froggit

* Grind 13 encounters total in the first half, grinding 11 in the initial room and 2 in the next two rooms

* Kill Snowdrake upon entering Snowdin, and the Icecap encounter in the next room. Kill Lesser Dog and then grind for an encounter before Dogamy and Dogaressa

* Finish grinding before Snowdin Town. At 10 kills, switch to the left room, and at 13, come back

* Kill Jerry at 13 kills with a triple and 14 kills with a double

* Kill Aaron and Woshua upon entering Waterfall. Grinding for the encounter before the Ballet shoes room, then equipping the Ballet Shoes and killing the next random encounter

* Dropping Stick and Tough Glove at some point (usually while Monster Kid is thinking)

* Grind in the room after Gerson until having at least 14 kills, then grind in the mushroom maze until having at least 16 kills, and then grinding at the end of the next maze until having 18 kills

* Obtain the Burnt Pan through the Early Pan method

* Kill Vulkin and Tsunderplane, still with the Ballet Shoes

* Equip Burnt Pan after Muffet

* Grind in Core rightside room until having 27 kills

* Grind in near Warrior Path until having 32 kills, and then going to the Warrior Path

* If 1 kill is left, grind in the bridge, else just equip the Ballet Shoes and proceed

# How to instal and use the simulator program

***NO RELEASE AVAILABLE FOR THE PROGRAM YET***

These are the available arguments for the program.

| Command | Result                                                                                                                                           |
|---------|--------------------------------------------------------------------------------------------------------------------------------------------------|
| -d arg  | Set the directory to the given argument. If left out, the program tries to reach for the folder in the User's Undertale save folder.             |
| -c      | If used, the program will print the chance of a run being in a given time range. If `-x` and `-n` are not supplied, the chance will always be 1. |
| -s arg  | Set the number of simulations to run. Default is 1 million.                                                                                      |
| -a      | If used, the program will print the average time of the simulations.                                                                             |
| -e      | If used, the program will print the standard deviation of the simulatins.                                                                        |
| -n arg  | Set the minimum time in the range for the probability.                                                                                           |
| -x arg  | Set the maximum time in the range for the probability.                                                                                           |
| -b      | If included, the program will use the best times it finds in the recordings directory. By default, it uses the average of the recordings.        |

An example use would be in windows shell:

```./simulator.exe -c -s 100000 -a -e -x 18000 -b```

Which for 100 thousand simulations, gets the chance of all sub 10 minute runs and prints the average and standard deviation.

# Build

To build the simulator, use Visual Studio Code and run the build task with `mingw64` installed.

To build the recorder you can open a `data.win` in Undertale mod tool and run the script, then save.
The script is developed for the *linux version 1.001*, but it may be compatible with other versions.

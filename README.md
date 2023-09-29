# Undertale SimRec (Simulator and Recorder)

This project aims to simulate and calculate the probabilities of Undertale speedruns. This repository contains two parts:

* Simulator - A C++ program
* Recorder - A [UndertaleModTool](https://github.com/krzys-h/UndertaleModTool) script that creates an Undertale mod

The recorder is a mod that outputs files that are then read by the simulator to generate the probability results.

# How to install and use the recorder mod

To install the mod, download the patch in the releases and apply it to an unmodified `data.win` for the linux version 1.001.
To apply the patch you can use resources such as [this website](https://www.marcrobledo.com/RomPatcher.js/). Replace the
old `data.win` with the patched one and the mod will be ready to play.

To use it, follow the instructions given in-game.

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

# Build

To build the simulator, use Visual Studio Code and run the build task with `mingw64` installed.

To build the recorder you can open a `data.win` in Undertale mod tool and run the script, then save.
The script is developed for the *linux version 1.001*, but it may be compatible with other versions.

# Development progress and plans

So far, the programs only have been developed up to Ruins (in Genocide). Before expanding to the rest of the game
(and possibly for categories besides Genocide), the following are desired:

* Possibly set up a declarative system for defining the stages in the recording, since they follow patterns. Then
expanding would be simpler
* Not sure if UTMT can let the script be split into different files, would ease organization if possible

Plans for the simulator:

* Create a GUI
* Have more ways to use the segments instead of just taking the "BEST" and "AVERAGE". One idea is to use the average and
standard deviations to simulate a random distribution for each segment.
* Graphical visualization of distributions
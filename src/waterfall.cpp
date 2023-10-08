#include "waterfall.hpp"
#include "undertale.hpp"
#include "encounters.hpp"

Waterfall::Waterfall (Times& times_value) : Simulator(times_value) {}

int Waterfall::simulate () {
    int time = times.segments["waterfall"];
    time += Undertale::encounter_time_random(times.static_blcons["waterfall"]);
    
    // already counting the first 2 scripted
    int kills = 2;
    // scripted double mold
    time += Undertale::src_steps(360, 30, 18, kills);
    kills = 4;
    
    // the random glowing water encounter
    int encounter = Undertale::glowing_water_encounter();

    if (encounter == Encounters::SingleAaron || encounter == Encounters::SingleWoshua) {
        kills++;
        if (encounter == Encounters::SingleAaron) {
            time += times.segments["sgl-aaron-shoes"];
        } else {
            time += times.segments["sgl-woshua-shoes"];
        }
    } else {
        kills += 2;
        if (encounter == Encounters::WoshuaAaron) {
            time += times.segments["woshu-aaron-surprise"];
        } else {
            time += times.segments["dbl-mold-shoes"];
        }
    }
    // shyren and glad dummy
    kills += 2;
    // first two grind encounters (first being temmie) happen with same number of kills
    for (int i = 0; i < 2; i++) {
        time += Undertale::waterfall_grind_steps(kills);
    }
    kills += 3;
    // remaining encounters before going to the mazes
    for (int i = 0; i < 2; i++) {
        time += Undertale::waterfall_grind_steps(kills);
        kills += 2;
    }

    // random encounters in the maze
    int first_maze_progress = 0;
    int second_maze_progress = 0;
    while (kills < 18) {
        int encounter = Undertale::waterfall_grind_encounter();

        if (encounter == Encounters::WoshuaAaron || encounter == Encounters::WoshuaMoldbygg) {
            kills += 2;
            if (encounter == Encounters::WoshuaAaron) {
                time += times.segments["woshua-aaron-surprise"];
            } else {
                time += times.segments["woshua-mold"];
            }
        } else {
            time += times.segments["temmie"];
            kills++;
        }

        int steps = Undertale::waterfall_grind_steps(kills);
        if (kills < 16) {
            first_maze_progress++;
            if (first_maze_progress == 1) steps = fix_step_total(steps, "mushroom-maze");
            else time += times.segments["mushroom-maze-going-back"] + times.segments["mushroom-maze-exit-after-backtrack"];
        } else {
            second_maze_progress++;
            if (second_maze_progress == 1) steps = fix_step_total(steps, "crystal-maze");
            else time += times.segments["crystal-going-back"] + times.segments["crystal-exit-after-backtrack"];
        }

        // non guaranteed blcons
        time += Undertale::encounter_time_random();
        time += steps;
    }
    
    return time;
}

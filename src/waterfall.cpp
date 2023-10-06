#include "waterfall.hpp"
#include "undertale.hpp"
#include "encounters.hpp"

Waterfall::Waterfall (Times& times_value) : Simulator(times_value) {}

int Waterfall::simulate () {
    int time = times.waterfall_general;
    
    // already counting the first 2 scripted
    int kills = 2;

    // guaranteed blcons
    time += Undertale::encounter_time_random(9);

    // scripted double mold
    time += Undertale::src_steps(360, 30, 18, kills);
    kills += 2;
    
    // the random glowing water encounter
    int encounter = Undertale::glowing_water_encounter();

    // increment here since its guaranteed at least single
    kills++;
    switch (encounter) {
        case Encounters::SingleWoshua:
            time += times.single_woshua_shoes;
            break;
        case Encounters::SingleAaron:
            time += times.single_aaron_shoes;
            break;
        case Encounters::DoubleMoldsmal:
            kills++;
            time += times.double_mold_shoes;
            break;
        case Encounters::WoshuaAaron:
            kills++;
            time += times.aaron_woshua_surprise;
            break;
        default:
            break;
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

        kills++;
        switch (encounter) {
            case Encounters::Temmie:
                time += times.temmie;
                break;
            case Encounters::WoshuaAaron:
                time += times.aaron_woshua_surprise;
                kills++;
                break;
            case Encounters::WoshuaMoldbygg:
                time += times.woshua_mold;
                kills++;
                break;
        }

        int steps = Undertale::waterfall_grind_steps(kills);
        if (kills < 16) {
            first_maze_progress++;
            if (first_maze_progress == 1) steps = fix_step_total(steps, times.mushroom_steps);
            else time += times.mushroom_backtrack;
        } else {
            second_maze_progress++;
            if (second_maze_progress == 1) steps = fix_step_total(steps, times.crystal_steps);
            else time += times.crystal_backtrack;
        }

        // non guaranteed blcons
        time += Undertale::encounter_time_random();
        time += steps;
    }
    
    return time;
}

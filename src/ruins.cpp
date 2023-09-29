#include "ruins.hpp"
#include "undertale.hpp"

Ruins::Ruins (Times& times_value) : Simulator(times_value) {}

int Ruins::simulate() {
    // initializing vars
    
    // initial time includes execution that is always the same
    int time = times.ruins_general;
    int kills = 0;

    // add all static blcons (6x nobody came, 1x first froggit, 13x first half)
    time += Undertale::encounter_time_random(20);

    // lv and exp are like this since the grind begins after killing first frog
    int lv = 2;
    int exp = 10;

    // loop for the first half
    while (kills < 13) {
        // lv 3 is guaranteed before first half end, therefore put this here
        // and leave both loops for each half independent
        if (exp >= 30) {
            lv = 3;
        }

        // first half step counting
        int steps = Undertale::src_steps(80, 40, 20, kills);
        // for first encounter, you need to at least get to the enter, imposing a higher minimum step count
        if (kills == 0 && steps < Undertale::leaf_pile_steps) {
            steps = Undertale::leaf_pile_steps;
        }
        time += steps;

        int encounter = Undertale::ruins1();
        // for the froggit encounter
        if (encounter == 0) {
            exp += 4;
            // do arithmetic on the lv for a slight optimization based on how the array is built
            time += times.single_froggit[lv - 2];
            time += times.frog_skip_save * Undertale::frogskip();
        } else {
            time += times.whimsun;
            exp += 3;
        }
        kills++;
    }

    while (kills < 20) {
        int encounter = Undertale::ruins3();
        
        // define variables as being "1" because of how the time arrays are made, this can be
        // used to sum the index
        int at_18 = kills >= 18 ? 1 : 0; 
        int at_19 = kills >= 19 ? 1 : 0;

        // don't add on first since it's included in general
        if (kills != 13) {
            time += times.ruins_second_half_transition;
        }
        // general encounter times
        time += times.ruins_second_half_transition +
            Undertale::src_steps(60, 60, 20, kills) +
            Undertale::encounter_time_random();

        bool is_encounter_0 = encounter == 0;
        if (is_encounter_0 || encounter > 2) { // 2 monster encounters
            if (is_encounter_0 || encounter == 3) { // for frog encounters
                if (is_encounter_0) { // for frog whim
                    time += times.froggit_whimsun[at_19];
                } else { // for 2x frog
                    time += times.double_froggit[at_19];
                }
                // number of frog skips achievable depends on how many it is being fought
                for (int max = 2 - at_19, i = 0; i < max; i++) {
                    time += times.frog_skip_save * Undertale::frogskip();
                }
            } else { // for 2x mold
                time += times.double_moldsmal[at_19];
            }
            kills += 2;
        } else if (encounter == 1) { // single mold
            time += times.single_moldsmal;
            kills++;
        } else { // triple mold
            time += times.triple_moldsmal[at_18 + at_19];
            kills += 3;
        }
    }

    return time;
}
#include "ruins.hpp"
#include "undertale.hpp"
#include "encounters.hpp"

Ruins::Ruins (Times& times_value) : Simulator(times_value) {}

int Ruins::simulate() {
    // initializing vars
    
    // static time
    int time = times.segments["ruins"];
    time += Undertale::encounter_time_random(times.static_blcons["ruins"]);

    int kills = 0;
    // lv and exp are like this since the grind begins after killing first frog
    int lv = 2;
    int exp = 10;

    // loop for the first half
    while (kills < 13) {
        int steps = Undertale::ruins_first_half_steps(kills);

        // for first encounter, you need to at least get to the end of the room, requiring a step fix
        if (kills == 0) {
            steps = fix_step_total(steps, "ruins-leaf-pile");
        }
        time += steps;

        // the minimum amount of kills needed to LV up is 7
        // the maximum amount of kills needed is 10
        // LV 4 is not possible thus no need to worry
        if (kills > 4 && kills < 11 && exp >= 30) {
            lv = 3;
        }

        int encounter = Undertale::ruins1();
        // for the froggit encounter
        if (encounter == Encounters::SingleFroggit) {
            exp += 3;
            // do arithmetic on the lv for a slight optimization based on how the array is built
            if (lv == 2) {
                time += times.segments["froggit-lv2"];
            } else {
                time += times.segments["froggit-lv3"];
            }
            time += times.segments["frogskip-save"] * Undertale::frogskip();
        // for whimsun
        } else {
            time += times.segments["whim"];
            exp += 2;
        }
        kills++;
    }

    int second_half_count = 0;
    while (kills < 20) {
        // first two encounters have STATIC values
        if (second_half_count < 2) {
            second_half_count++;
        } else {
            time += times.segments["ruins-second-transition"];
            time += Undertale::encounter_time_random();;
        }

        time += Undertale::scr_steps(60, 60, 20, kills);;

        int encounter = Undertale::ruins3();

        bool at_18 = kills >= 18; 
        bool at_19 = kills >= 19;

        if (
            encounter == Encounters::FroggitWhimsun ||
            encounter == Encounters::DoubleMoldsmal ||
            encounter == Encounters::DoubleFroggit
        ) { // 2 monster encounters
            if (encounter == Encounters::FroggitWhimsun || encounter == Encounters::DoubleFroggit) { // for frog encounters
                if (encounter == Encounters::FroggitWhimsun) { // for frog whim
                    time += at_19 ? times.segments["frog-whim-19"] : times.segments["frog-whim"]; 
                } else { // for 2x frog
                    time += at_19 ? times.segments["dbl-frog-19"] : times.segments["dbl-frog"];
                }
                // number of frog skips achievable depends on how many are being fought
                for (int max = at_19 ? 1 : 2, i = 0; i < max; i++) {
                    time += times.segments["frogskip-save"] * Undertale::frogskip();
                }
            } else { // for 2x mold
                time += at_19 ? times.segments["dbl-mold-19"] : times.segments["dbl-mold"];
            }
            kills += 2;
        } else if (encounter == Encounters::SingleMoldsmal) { // single mold
            time += times.segments["sgl-mold"];
            kills++;
        } else { // triple mold
            if (at_18) {
                time += times.segments["tpl-mold-18"];
            } else if (at_19) {
                time += times.segments["tpl-mold-19"];
            } else {
                time += times.segments["tpl-mold"];
            }
            kills += 3;
        }
    }

    return time;
}

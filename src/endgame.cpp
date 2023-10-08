#include "endgame.hpp"
#include "undertale.hpp"
#include "encounters.hpp"

Endgame::Endgame (Times& times_value) : Simulator(times_value) {}

int Endgame::simulate () {
    int time = times.segments["endgame"];
    time += Undertale::encounter_time_random(times.static_blcons["endgame"]);
    
    int kills = 5;

    // to create a buffer round between being on right and left
    bool went_left = false;
    while (kills < 40) {
        if (kills < 27) {
            time += times.segments["core-right-transition"];
        } else if (!went_left) {
            went_left = true;
        } else {
            // warriors path
            if (kills >= 32) kills += 7;
            // if ending it here, it means we did warrior path and then finished: get the nobody cames and such
            if (kills >= 40) {
                time += 3 * times.segments["nobody-came"];
                time += Undertale::encounter_time_random(3);
                time += times.segments["core-bridge"];
                break;
            }
            // grind an encounter at 39 in the bridge after coming back
            if (kills == 39) time += times.segments["grind-end-transition"];
            // grinding in the left side
            else time += times.segments["core-left-side-transition-2"] + times.segments["core-left-side-transition-3"];
        }
        int steps = Undertale::src_steps(70, 50, 40, kills);
        int encounter = Undertale::core_encounter();

        if (
            encounter == Encounters::FinalFroggitAstigmatism ||
            encounter == Encounters::WhimsalotAstigmatism ||
            encounter == Encounters::WhimsalotFinalFroggit ||
            encounter == Encounters::KnightKnightMadjick
        ) {
            kills += 2;
            if (encounter == Encounters::FinalFroggitAstigmatism) {
                time += times.segments["frog-astig"];
            } else if (encounter == Encounters::WhimsalotAstigmatism) {
                time += times.segments["whim-astig"];
            } else if (encounter == Encounters::WhimsalotFinalFroggit) {
                time += times.segments["core-frog-whim"];
            }
        } else if (
            encounter == Encounters::SingleAstigmatism ||
            encounter == Encounters::SingleKnightKnight ||
            encounter == Encounters::SingleMadjick
        ) {
            kills++;
            if (encounter == Encounters::SingleAstigmatism) {
                time += times.segments["sgl-astig"];
            } else if (encounter == Encounters::SingleKnightKnight) {
                time += times.segments["sgl-knight"];
            } else {
                time += times.segments["sgl-madjick"];
            }
        } else {
            kills += 3;
            time += times.segments["core-triple"];
        }

        time += steps;
        time += Undertale::encounter_time_random();
    }

    return time;
}
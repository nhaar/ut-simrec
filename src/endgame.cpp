#include "endgame.hpp"
#include "undertale.hpp"
#include "encounters.hpp"

Endgame::Endgame (Times& times_value) : Simulator(times_value) {}

int Endgame::simulate () {
    int time = times.segments["endgame"];
    time += Undertale::encounter_time_random(times.static_blcons["endgame"]);
    
    // scripted encounter step count
    int kills = 5;
    time += Undertale::core_steps(kills);
    
    kills = 6;
    while (kills < 14) {
        time += Undertale::core_steps(kills);
        kills += 2;
    }

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
                time += 4 * times.segments["nobody-came"];
                time += Undertale::encounter_time_random(4);
                time += times.segments["core-bridge"];
                break;
            }
            // grind an encounter at 39 in the bridge after coming back
            if (kills == 39) time += times.segments["grind-end-transition"];
            // grinding in the left side
            else time += times.segments["core-left-side-transition-2"] + times.segments["core-left-side-transition-3"];
        }
        int steps = Undertale::core_steps(kills);
        int encounter = Undertale::core_encounter();

        bool flee_one = kills == 39;
        if (
            encounter == Encounters::FinalFroggitAstigmatism ||
            encounter == Encounters::WhimsalotAstigmatism ||
            encounter == Encounters::WhimsalotFinalFroggit ||
            encounter == Encounters::KnightKnightMadjick
        ) {
            kills += 2;
            if (encounter == Encounters::FinalFroggitAstigmatism) {
                time += flee_one ? times.segments["frog-astig-flee"] : times.segments["frog-astig"];
            } else if (encounter == Encounters::WhimsalotAstigmatism) {
                time += flee_one ? times.segments["whim-astig-flee"] : times.segments["whim-astig"];
            } else {
                time += flee_one ? times.segments["core-frog-whim-flee"] : times.segments["core-frog-whim"];
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
            if (flee_one) {
                time += times.segments["core-triple-kill-one"];
            } else if (kills == 31) {
                time += times.segments["core-triple-kill-two"];
            } else {
                time += times.segments["core-triple"];
            }
            kills += 3;
        }

        time += steps;
        time += Undertale::encounter_time_random();
    }

    return time;
}
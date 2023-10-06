#include "endgame.hpp"
#include "undertale.hpp"
#include "encounters.hpp"

Endgame::Endgame (Times& times_value) : Simulator(times_value) {}

int Endgame::simulate () {
    int time = times.endgame_general;
    int kills = 5;

    // scripted blcons
    time += Undertale::encounter_time_random(2);
    bool went_left = false;

    while (kills < 40) {
        int steps = Undertale::src_steps(70, 50, 40, kills);
        int encounter = Undertale::core_encounter();
        if (kills < 27) {
            time += times.core_right_transition;
        } else if (!went_left) {
            went_left = true;
        } else {
            if (kills >= 32) kills += 7;
            if (kills >= 40) break;
            if (kills == 39) time += times.core_grind_end;
            else time += times.core_left_transition;
        }

        switch (encounter) {
            case Encounters::FinalFroggitAstigmatism:
                time += times.final_froggit_astig;
                kills += 2;
                break;
            case Encounters::SingleAstigmatism:
                time += times.single_astig;
                break;
            case Encounters::WhimsalotAstigmatism:
                time += times.whimsalot_astig;
                kills += 2;
                break;
            case Encounters::WhimsalotFinalFroggit:
                time += times.final_froggit_whimsalot;
                kills += 2;
                break;
            case Encounters::KnightKnightMadjick:
                time += times.knight_madjick;
                kills += 2;
                break;
            case Encounters::SingleKnightKnight:
                time += times.single_knight;
                kills++;
                break;
            case Encounters::SingleMadjick:
                time += times.single_madjick;
                kills++;
                break;
            case Encounters::CoreTriple:
                time += times.core_triple;
                kills += 3;
                break;
        }

        time += steps;
        time += Undertale::encounter_time_random();
    }

    return time;
}
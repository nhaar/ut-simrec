#include "snowdin.hpp"
#include "undertale.hpp"
#include "encounters.hpp"

Snowdin::Snowdin (Times& times_value) : Simulator(times_value) {}

int Snowdin::simulate () {
    int time = times.snowdin_general;
    int kills = 0;

    for (int i = 0; i < 2; i++) {
        int dogskip = Undertale::dogskip();
        if (dogskip == 0) {
            time += times.not_dogskip_turn;
        } else if (dogskip == 1) {
            time += times.dogskip_turn;
        } 
    }
 
    while (kills < 2) {
        int steps = Undertale::src_steps(120, 30, 16, kills);
        
        if (kills == 0) {
            steps = fix_step_total(steps, times.box_road_steps);
        }

        time += Undertale::encounter_time_random();
        kills++;
    }

    // lesser dog kill
    kills++;

    bool right_side = true;
    while (kills < 16) {
        int steps = 0;

        if (kills == 3) {
            steps = fix_step_total(Undertale::src_steps(220, 30, 16, kills), times.dogi_steps);
        } else {
            steps = Undertale::src_steps(120, 30, 16, kills);
        }

        time += steps;
        
        time += Undertale::encounter_time_random();

        if (right_side) {
            time += times.snowdin_right_transition;
            if (kills >= 10) right_side = false;
        } else {
            time += times.snowdin_left_transition;
            if (kills >= 13) right_side = false;
        }

        int encounter = Undertale::snowdin();

        if (encounter == Encounters::SnowdinDouble) {
            if (kills >= 14) {
                time += times.snowdin_encounters_jerry[0];
                kills += 2;
            } else {
                time += times.snowdin_encounters[0];
                kills++;
            }
        } else if (encounter == Encounters::SnowdinTriple) {
            if (kills >= 13) {
                time += times.snowdin_encounters_jerry[1];
                kills += 3;
            } else {
                time += times.snowdin_encounters[1];
                kills += 2;
            }
        }
    }
        return time;
}
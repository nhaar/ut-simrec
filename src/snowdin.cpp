#include "snowdin.hpp"
#include "undertale.hpp"
#include "encounters.hpp"

Snowdin::Snowdin (Times& times_value) : Simulator(times_value) {}

int Snowdin::simulate () {
    int time = times.segments["snowdin"];
    time += Undertale::encounter_time_random(times.static_blcons["snowdin"]);
    int kills = 0;

    // single snowdrake steps
    time += fix_step_total(Undertale::snowdin_general_steps(kills), "snowdin-box-road");

    kills = 3;
    while (kills < 16) {
        int encounter = Undertale::snowdin();
        bool fight_jerry =
            encounter == Encounters::SnowdinDouble && kills == 14 ||
            encounter == Encounters::SnowdinTriple && kills == 13;
        
        if (kills == 3) {
            // dogi bridge (steps + encounter)
            time += fix_step_total(Undertale::dogi_room_steps(kills), "snowdin-dogi");
        } else {
            time += Undertale::snowdin_general_steps(kills);
            time += Undertale::encounter_time_random();

            if (kills < 10 || (kills >= 13 && !fight_jerry)) {
                time += times.segments["snowdin-right-transition"];

            } else if (kills < 13) {
                time += times.segments["snowdin-left-transition"];
            }
        }

        if (encounter == Encounters::SnowdinDouble) {
            if (fight_jerry) {
                time += times.segments["snowdin-dbl-jerry"];
                kills += 2;
            } else {
                time += times.segments["snowdin-dbl"];
                kills++;
            }
        } else if (encounter == Encounters::SnowdinTriple) {
            if (fight_jerry) {
                time += times.segments["snowdin-tpl"];
                kills += 3;
            } else {
                time += times.segments["snowdin-dbl"];
                kills += 2;
            }
        }
    }
    return time;
}
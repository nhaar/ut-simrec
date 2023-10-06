#include "times.hpp"

Times::Times () {}

Times::Times (std::unordered_map<std::string, int> map) {
    ruins_general = 0;
    int lv_up_msg = 0;
    int you_won_msg = 0;
    int frogskip_turn = 0;
    int not_frogskip_turn = 0;
    for (const auto& pair : map) {
        const std::string& key = pair.first;
        int time = map[key];
        if (
            key == "ruins-start" ||
            key == "ruins-hallway" ||
            key == "ruins-leafpile" ||
            key == "leaf-pile-transition" ||
            key == "leaf-fall-downtime" ||
            key == "leaf-fall-transition" ||
            key == "ruins-one-rock" ||
            key == "one-rock-transition" ||
            key == "ruins-maze" ||
            key == "three-rock-downtime" ||
            key == "three-rock-transition" ||
            key == "ruins-napsta" ||
            key == "ruins-switches" ||
            key == "perspective-a" ||
            key == "perspective-b" ||
            key == "perspective-c" ||
            key == "perspective-d" ||
            key == "ruins-end"
        ) {
            ruins_general += time;
        } else if (key == "ruins-first-transition") {
            ruins_general += 10 * time;
        } else if (key == "froggit-lv2") {
            single_froggit[0] = time;
        } else if (key == "froggit-lv3") {
            single_froggit[1] = time;
        } else if (key == "frogskip") {
            frogskip_turn = time;
        } else if (key == "not-frogskip") {
            not_frogskip_turn = time;
        } else if (key == "whim") {
            whimsun = time;
        } else if (key == "lv-up") {
            lv_up_msg = time;
        } else if (key == "you-won") {
            you_won_msg = time;
        } else if (key == "sgl-mold") {
            single_moldsmal = time;
        } else if (key == "dbl-mold") {
            double_moldsmal[0] = time;
        } else if (key == "dbl-mold-19") {
            double_moldsmal[1] = time;
        } else if (key == "tpl-mold") {
            triple_moldsmal[0] = time;
        } else if (key == "tpl-mold-18") {
            triple_moldsmal[1] = time;
        } else if (key == "tpl-mold-19") {
            triple_moldsmal[2] = time;
        } else if (key == "frog-whim") {
            froggit_whimsun[0] = time;
        } else if (key == "frog-whim-19") {
            froggit_whimsun[1] = time;
        } else if (key == "dbl-frog") {
            double_froggit[0] = time;
        } else if (key == "dbl-frog-19") {
            double_froggit[1] = time;
        } else if (key == "ruins-second-transition") {
            ruins_second_half_transition = time;
        } else if (key == "nobody-came") {
            nobody_came = time;
        } else if (key == "ruins-leaf-pile-steps") {
            leaf_pile_steps[0] = time;
        } else if (key == "ruins-leaf-pile-endsteps") {
            leaf_pile_steps[1] = time;
        } else if (
            key == "ruins-to-snowdin" ||
            key == "snowdin-box-road" ||
            key == "sgl-snowdrake" ||
            key == "snowdin-human-rock" ||
            key == "sgl-icecap" ||
            key == "snowdin-doggo" ||
            key == "ice-slide-downtime" ||
            key == "lesser-dog" ||
            key == "before-dogi" ||
            key == "snowdin-dogi" ||
            key == "before-greater-dog" ||
            key == "greater-dog-1" ||
            key == "greater-dog-2" ||
            key == "greater-dog-end" ||
            key == "snowdin-end"
        ) {
            snowdin_general += time;
        } else if (key == "snowdin-box-road-steps") {
            box_road_steps[0] = time;
        } else if (key == "box-road-endsteps") {
            box_road_steps[1] = time;
        } else if (key == "snowdin-dogi-steps") {
            dogi_steps[0] = time;
        } else if (key == "snowdin-dogi-endsteps") {
            dogi_steps[1] = time;
        } else if (key == "dogskip") {
            dogskip_turn = time;
        } else if (key == "not-dogskip") {
            not_dogskip_turn = time;
        } else if (key == "snowdin-dbl") {
            snowdin_encounters[0] = time;
        } else if (key == "snowdin-tpl") {
            snowdin_encounters[1] = time;
        } else if (key == "snowdin-dbl-jerry") {
            snowdin_encounters_jerry[0] = time;
        } else if (key == "snowdin-tpl-jerry") {
            snowdin_encounters_jerry[1] = time;
        } else if (key == "snowdin-right-transition") {
            snowdin_right_transition = time;
        } else if (key == "snowdin-left-transition") {
            snowdin_left_transition = time;
        }
    }
    // add "relative" times
    ruins_general += lv_up_msg - you_won_msg;
    frog_skip_save = not_frogskip_turn - frogskip_turn;
}
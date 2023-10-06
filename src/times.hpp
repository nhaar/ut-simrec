#ifndef TIMES_H
#define TIMES_H

#include <string>
#include <unordered_map>

// class stores all the times that rely on execution
class Times {
public:
    // comments below refer to the name of the segments in the recorder

    // whim
    int whimsun;
    // sgl-mold
    int single_moldsmal;

    // ruins-second-transition
    int ruins_second_half_transition;

    // ruins-start +
    // ruins-hallway +
    // ruins-leafpile +
    // leaf-pile-transition +
    // 10 * ruins-first-transition +
    // leaf-fall-downtime +
    // leaf-fall-transition +
    // ruins-one-rock +
    // one-rock-transition +
    // ruins-maze +
    // three-rock-downtime +
    // ruins-naspta +
    // ruins-switches +
    // ruins-perspective-a +
    // ruins-perspective-b +
    // ruins-perspective-c +
    // ruins-perspective-d +
    // ruins-end +
    // you-won - lv-up
    int ruins_general;

    // not-frogskip - frogskip
    int frog_skip_save;

    // { froggit-lv2, froggit-lv3 }
    int single_froggit [2];

    // { dbl-mold, dbl-mold-19 }
    int double_moldsmal [2];

    // { tpl-mold, tpl-mold-18, tpl-mold-19 }
    int triple_moldsmal [3];

    // { frog-whim, frog-whim-19 }
    int froggit_whimsun [2];

    // { dbl-frog, dbl-frog-19 }
    int double_froggit [2];

    // nobody-came
    int nobody_came;

    int leaf_pile_steps [2];

    int snowdin_general;

    int snowdin_encounters [2];

    int snowdin_encounters_jerry [2];

    int snowdin_right_transition;

    int snowdin_left_transition;

    int dogskip_turn;

    int not_dogskip_turn;

    int box_road_steps [2];

    int dogi_steps [2];

    int waterfall_general;

    int single_woshua_shoes;

    int single_aaron_shoes;

    int double_mold_shoes;

    int aaron_woshua_surprise;

    int woshua_mold;

    int temmie;

    int crystal_backtrack;

    int mushroom_steps [2];

    int crystal_steps [2];

    Times ();

    Times (std::unordered_map<std::string, int> map);
};

#endif
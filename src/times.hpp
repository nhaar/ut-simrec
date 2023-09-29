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
    // ruins-leafpile-transition +
    // 10 * ruins-first-transition +
    // ruins-leaf-fall +
    // leaf-fall-transition +
    // ruins-one-rock +
    // ruins-maze +
    // ruins-three-rock +
    // ruins-naspta +
    // ruins-switches +
    // ruins-perspective-a +
    // ruins-perspective-b +
    // ruins-perspective-c +
    // ruins-perspective-d +
    // ruins-end +
    // you-won - lv-up
    int ruins_general;

    // not_frogskip - frogskip
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

    Times ();

    Times (std::unordered_map<std::string, int> map);
};

#endif
#include "undertale.hpp"
#include "random.hpp"
#include <cmath>

// including this method since technically Undertale's rounding at halfway rounds to nearest even number
// will leave it here for easy of changing that but the difference is technically negligible considering
// halfway is not something that will be generated from random anyways
int Undertale::round (double number) {
    return std::round(number);
}

// simulating the round random generator from Mr Tobias
int Undertale::roundrandom (int max) {
    return round(Random::random_number() * max);
}
// replica of the undertale code, with no optimization in mind
int Undertale::src_steps (int min_steps, int steps_delta, int max_kills, int kills) {
    double populationfactor = (double) max_kills / (double) (max_kills - kills);
    if (populationfactor > 8) {
        populationfactor = 8;
    }
    double steps = (min_steps + roundrandom(steps_delta)) * populationfactor;
    return (int) steps + 1;
}

// encounterer for first half
// 0 = froggit
// 1 = whimsun
int Undertale::ruins1 () {
    double roll = Random::random_number();
    if (roll < 0.5) return 0;
    return 1;
}

// encounterer for ruins second half (called ruins3 because in-game it is the third encounterer)
// 0 = frog whimsun
// 1 = 1x mold
// 2 = 3x mold
// 3 = 2x froggit
// 4 = 2x mold
int Undertale::ruins3 () {
    double roll = Random::random_number();
    if (roll < 0.25) return 0;
    if (roll < 0.5) return 1;
    if (roll < 0.75) return 2;
    if (roll < 0.9) return 3;
    return 4;
}

// random odds for a frog skip
// 1 = no frogskip
// 0 = gets frogskip
// choice of these numbers comes from how the simulator and recorder work (by default frogskip is assumed)
int Undertale::frogskip () {
    double roll = Random::random_number();
    if (roll < 0.405) return 0;
    return 1;
}

// total time it takes after the "!" disappears for the battle to begin (with the heart flick animation)
int Undertale::heart_flick = 47;

// total time required to enter an encounter (blcon + flick) using random values
int Undertale::encounter_time_random () {
    return encounter_time_random(1);
}

// total time required to enter an encounter (blcon + flick) a number of times using random values
int Undertale::encounter_time_random(int number_of_times) {
    int total = heart_flick * number_of_times;
    for (int i = 0; i < number_of_times; i++) {
        total += roundrandom(5);
    }
    return total;
}

// total time required to enter an encounter (blcon + flick) a certain number of times using average values
int Undertale::encounter_time_average_total (int number_of_times) {
    // 2.5 is the avg of roundrandom(5)
    return static_cast<int>(std::round(2.5 * number_of_times));
}

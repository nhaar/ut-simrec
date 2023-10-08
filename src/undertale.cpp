#include <cmath>
#include "undertale.hpp"
#include "random.hpp"
#include "encounters.hpp"

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
int Undertale::scr_steps (int min_steps, int steps_delta, int max_kills, int kills) {
    double populationfactor = (double) max_kills / (double) (max_kills - kills);
    if (populationfactor > 8) {
        populationfactor = 8;
    }
    double steps = (min_steps + roundrandom(steps_delta)) * populationfactor;
    return (int) steps + 1;
}

// step counter for the rooms in the first half of ruins
int Undertale::ruins_first_half_steps (int kills) {
    return scr_steps(80, 40, 20, kills);
}

// encounterer for first half
int Undertale::ruins1 () {
    double roll = Random::random_number();

    if (roll < 0.5) return Encounters::SingleFroggit;
    return Encounters::Whimsun;
}

// encounterer for ruins second half (called ruins3 because in-game it is the third encounterer)
int Undertale::ruins3 () {
    double roll = Random::random_number();
    if (roll < 0.25) return Encounters::FroggitWhimsun;
    if (roll < 0.5) return Encounters::SingleMoldsmal;
    if (roll < 0.75) return Encounters::TripleMoldsmal;
    if (roll < 0.9) return Encounters::DoubleFroggit;
    return Encounters::DoubleMoldsmal;
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

// snowdin grind encounter results
int Undertale::snowdin () {
    double roll = Random::random_number();
    if (roll > 0.5) return Encounters::SnowdinDouble;
    else return Encounters::SnowdinTriple;
}

// getting a dogskip or not
// 0 - no dogskip
// 1 - dogskip
int Undertale::dogskip () {
    double roll = Random::random_number();
    if (roll < 0.5) return 0;
    else return 1;
}

// encounters for the first random encounter in Waterfall
int Undertale::glowing_water_encounter () {
    double roll = Random::random_number();
    if (roll < 0.2666666666) {
        return Encounters::SingleWoshua;
    }
    if (roll < 0.53333333333) {
        return Encounters::DoubleMoldsmal;
    } 
    if (roll < 0.7333333333) {
        return Encounters::SingleAaron;
    }
    return Encounters::WoshuaAaron;

}

// random encounters at the end of Waterfall
int Undertale::waterfall_grind_encounter () {
    double roll = Random::random_number();
    if (roll < 0.33333333) return Encounters::WoshuaAaron;
    if (roll < 0.73333333) return Encounters::WoshuaMoldbygg;
    return Encounters::Temmie;
}

// steps for the rooms in the waterfall grind
int Undertale::waterfall_grind_steps (int kills) {
    return scr_steps(60, 20, 18, kills);
}

// steps for the rooms in core
int Undertale::core_encounter () {
    double roll = Random::random_number();
    if (roll < 0.133333333) return Encounters::FinalFroggitAstigmatism;
    if (roll < 0.333333333) return Encounters::WhimsalotFinalFroggit;
    if (roll < 0.533333333) return Encounters::WhimsalotAstigmatism;
    if (roll < 0.733333333) return Encounters::KnightKnightMadjick;
    if (roll < 0.866666666) return Encounters::CoreTriple;
    if (roll < 0.933333333) return Encounters::SingleKnightKnight;
    return Encounters::SingleMadjick;
}
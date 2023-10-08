#include <cmath>
#include "simulator.hpp"

Simulator::Simulator (Times& times_value) : times(times_value) {}

// run simulations and generate a probability distribution for the results
ProbabilityDistribution Simulator::get_dist (int simulations) {
    int* results = new int[simulations];
    for (int i = 0; i < simulations; i++) {
        results[i] = simulate();
    }

    int size = sizeof(results) / sizeof(results[0]);
    return ProbabilityDistribution (1, results, simulations);
}

// uses a mathematical formula to calculate the marging of error from a calculated probability
double Simulator::get_error_margin (int n, double probability) {
    return 2.6 * std::sqrt((probability) * (1 - probability) / (double) n);
}

// this function is used when in a room where you must get somewhere, and the total
// time to get there may be smaller than the steps required for an encounter
// `calculated_steps` are the steps to the encounter from `scr_steps`, and
// downtime_steps is an array where the first element is the total number of steps it takes to
// traverse the desired path, and the second element is basically the amount of time
// that in the occasion the player stopped to grind in a place, it's how long it takes
// to go from the place they were grinding to the next destination (usually the room transition)
int Simulator::fix_step_total(int calculated_steps, std::string segment_name) {
    // add 1 step because the methods for recording `downtime_steps` don't record the last frame
    // used to touch a door
    // TO-DO review how this applies to the dogi downtime-step
    int steps = calculated_steps + 1;
    int* segments = times.steps[segment_name];
    // the + 1 turns the < into a <=
    // then this first case is where the step occurs before the end, so must AT LEAST traverse the whole path
    if (steps <= segments[0]) return segments[0];
    // in the other case, the total step required will then be the amount needed to grind the encounter
    // and then leave after grinding
    else return steps + segments[1];
}
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

#ifndef SIMULATOR_H
#define SIMULATOR_H

#include <string>
#include "times.hpp"
#include "probability_distribution.hpp"

// handles methods for generating simulations and gathering its results
class Simulator {
public:
    Times& times;

    virtual int simulate() = 0;

    int fix_step_total (int calculated_steps, std::string segment_name);
    
    Simulator (Times& times_value);

    ProbabilityDistribution get_dist (int simulations);

    double get_error_margin (int n, double probability);
};

#endif
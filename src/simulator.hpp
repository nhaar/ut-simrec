#ifndef SIMULATOR_H
#define SIMULATOR_H

#include "times.hpp"
#include "probability_distribution.hpp"

// handles methods for generating simulations and gathering its results
class Simulator {
public:
    Times& times;

    virtual int simulate() = 0;
    
    Simulator (Times& times_value);

    ProbabilityDistribution get_dist (int simulations);

    double get_error_margin (int n, double probability);
};

#endif
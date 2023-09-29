#ifndef PROBABILITY_DISTRIBUTION_H
#define PROBABILITY_DISTRIBUTION_H

#include <string>

// class handle probability distributions, a discrete description is used to approximate a continuous distribution
// the distribution starts at a minimum value up to a maximum value, with "bins" with a size
// everything outside the range is given as 0
// the distributions are not normalized
class ProbabilityDistribution {
    int min;
    int max;
    int interval;
    int* distribution;
    int length;
    int total;

    void build_dist (int* values, int size);

public:
    ProbabilityDistribution (int interval_value, int* values, int size);

    ProbabilityDistribution (int min_value, int max_value, int interval_value, int* values, int size);

    int get_distribution_pos (int value);

    double get_chance (int min, int max);

    double get_chance_up_to (int max);

    double get_chance_from (int min);
    
    double get_average ();

    double get_sqr_avg ();

    double get_stdev ();

    void export_dist (std::string name);
};

#endif
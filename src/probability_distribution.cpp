#include <cmath>
#include <fstream>
#include "probability_distribution.hpp"

void ProbabilityDistribution::build_dist (int* values, int size) {
    // add +1 to include the maximum value as well, due to 0-indexsng
    // length is calculated from the range and bin size
    length = (max + 1 - min) / interval;
    
    total = 0;

    // actual distribution is just an array where each element of the array represents a "x" position
    // and the value is the number of time it appears in that "x" position
    distribution = new int[length] {0};
    for (int i = 0; i < size; i++) {
        int pos = get_distribution_pos(values[i]);
        distribution[pos]++;
        total++;
    }
}

ProbabilityDistribution::ProbabilityDistribution (int interval_value, int* values, int size)
: interval(interval_value), max(0), min(std::numeric_limits<int>::max()) {
    // determine max and minimum values in the distribution
    for (int i = 0; i < size; i++) {
        int current = values[i];
        if (current > max) max = current;
        else if (current < min) min = current;
    }
    build_dist(values, size);
}

ProbabilityDistribution::ProbabilityDistribution (int min_value, int max_value, int interval_value, int* values, int size)
    : min(min_value), max(max_value), interval(interval_value) {
        build_dist(values, size);
    }

// get the "x" position for a value in the distribution
int ProbabilityDistribution::get_distribution_pos (int value) {
    if (value < min) return 0;
    if (value > max) return length;
    return (value - min) / interval;
}

// get the chance a value is in the interval min (including) to max (excluding)
double ProbabilityDistribution::get_chance (int min, int max) {
    int favorable = 0;
    int lower_pos = get_distribution_pos(min);
    int higher_pos = get_distribution_pos(max);
    for (int i = lower_pos; i < higher_pos; i++) {
        favorable += distribution[i];
    }
    return (double) favorable / (double) total;
}

// get the chance a value is in the interval starting at the minimum up to a value
double ProbabilityDistribution::get_chance_up_to (int max) {
    return get_chance(min, max);
}

// get the chance a value is in the interval starting at a given minimum value up to the max
double ProbabilityDistribution::get_chance_from (int min) {
    return get_chance(min, max);
}

// simulating a rieman integral for the functions below like \int x \rho(x) dx / \int \rho(x) dx

// get average value
double ProbabilityDistribution::get_average () {
    double numerator = 0;
    for (int i = 0; i < length; i++) {
        numerator += (min + interval * i) * distribution[i];
    }
    return numerator / (double) total;
}

// get average of square of values
double ProbabilityDistribution::get_sqr_avg () {
    double numerator = 0;
    for (int i = 0; i < length; i++) {
        numerator += std::pow(min + interval * i, 2) * distribution[i];
    }
    return numerator / (double) total;
}

// get the standard deviation of the values
double ProbabilityDistribution::get_stdev () {
    return std::sqrt(get_sqr_avg() - std::pow(get_average(), 2));
}

void ProbabilityDistribution::export_dist (std::string name) {
    std::ofstream file(name);
    for (int i = 0; i < length; i++) {
        file << min + i << "," << distribution[i] << std::endl;
    }
    file.close();
}
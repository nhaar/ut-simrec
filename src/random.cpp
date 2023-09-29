#include <cmath>
#include "random.hpp"

// generates a random number between 0 and 1
double Random::random_number() {
    return (double)(rand()) / (double)(RAND_MAX);
}
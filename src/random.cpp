#include <cmath>
#include "random.hpp"

double Random::random_number() {
    return (double)(rand()) / (double)(RAND_MAX);
}
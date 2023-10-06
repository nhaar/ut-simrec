#ifndef WATERFALL_H
#define WATERFALL_H

#include "simulator.hpp"

// Simulator for Waterfall
class Waterfall : public Simulator {
public:
    Waterfall (Times& times_value);

    int simulate() override;
};

#endif
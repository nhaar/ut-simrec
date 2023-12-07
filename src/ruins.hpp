#ifndef RUINS_H
#define RUINS_H

#include "simulator.hpp"

// handles the method for simulating a ruins run
class Ruins : public Simulator {

public:
    Ruins (Times& times_value, bool glitchless, int first_half_kills);

    int simulate() override;

    bool glitchless;

    int first_half_kills;
};

#endif
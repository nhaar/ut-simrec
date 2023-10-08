#ifndef FULL_GAME_H
#define FULL_GAME_H

#include "simulator.hpp"

// simulator for the entirety of the genocide run
class FullGame : public Simulator {
public:
    FullGame (Times& times_value);

    int simulate() override;

    static int const area_count = 4;

    Simulator* children [area_count];
};

#endif
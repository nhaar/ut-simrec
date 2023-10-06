#ifndef SNOWDIN_H
#define SNOWDIN_H

#include "simulator.hpp"

// handles the method for simulating a snowdin run
class Snowdin : public Simulator {

public:
    Snowdin (Times& times_value);

    int simulate() override;
};

#endif
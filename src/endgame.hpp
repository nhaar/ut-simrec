#ifndef ENDGAME_H
#define ENDGAME_H

#include "simulator.hpp"

// Class for the Hotland/Core/Post core simulator
class Endgame : public Simulator {
public:
    Endgame (Times& times_value);

    int simulate() override;
};

#endif
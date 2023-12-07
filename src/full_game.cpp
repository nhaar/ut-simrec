#include "full_game.hpp"
#include "ruins.hpp"
#include "snowdin.hpp"
#include "waterfall.hpp"
#include "endgame.hpp"

FullGame::FullGame (Times& times_value) : Simulator (times_value) {
    children[0] = new Ruins(times_value, false);
    children[1] = new Snowdin(times_value);
    children[2] = new Waterfall(times_value);
    children[3] = new Endgame(times_value);
}

int FullGame::simulate () {
    int time = 0;
    for (int i = 0; i < area_count; i++) {
        time += children[i]->simulate();
    }
    return time;
}
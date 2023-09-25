#include <iostream>
#include <cmath>

using namespace std;


// handle methods for generating random numbers
class Random {
public:
    Random () {
        // seeding; exact seeding is not important
        srand(time(0));
    }

    // get a random double between 0 and 1
    double random_number () {
        return (double)(rand()) / (double)(RAND_MAX);
    }
};


// handle methods specific to the undertale engine
class Undertale {
private:
    Random random;

    // including this method since technically Undertale's rounding at halfway rounds to nearest even number
    // will leave it here for easy of changing that but the different is technically negligible considering
    // halfway is not something that will be generated from random anyways
    static int round (double number) {
        return std::round(number);
    }

    // simulating the round random generator from gamemaker
    int roundrandom (int max) {
        return round(random.random_number() * max);
    }
public:
    // replica of the undertale code, with no optimization in mind
    int src_steps (int min_steps, int steps_delta, int max_kills, int kills) {
        double populationfactor = (double) max_kills / (double) (max_kills - kills);
        if (populationfactor > 8) {
            populationfactor = 8;
        }
        double steps = (min_steps + roundrandom(steps_delta)) * populationfactor;
        return (int) steps;
    }

    // encounterer for first half
    // 0 = froggit
    // 1 = whimsun
    int ruins1 () {
        double roll = random.random_number();
        if (roll < 0.5) return 0;
        return 1;
    }

    // encounterer for ruins second half
    // 0 = frog whimsun
    // 1 = 1x mold
    // 2 = 3x mold
    // 3 = 2x froggit
    // 4 = 2x mold
    int ruins3 () {
        double roll = random.random_number();
        if (roll < 0.25) return 0;
        if (roll < 0.5) return 1;
        if (roll < 0.75) return 2;
        if (roll < 0.9) return 3;
        return 4;
    }

    // random odds for a frog skip
    // 0 = no frogskip
    // 1 = gets frogskip
    int frogskip () {
        double roll = random.random_number();
        if (roll < 0.405) return 1;
        return 0;
    }

    // total time required to encounter + blcon
    int encounter_time () {
        return 47 + roundrandom(5);

    }
};

class Times {
public:
    int single_froggit [2];
    int whimsun = 101;
    int single_moldsmal = 368;
    int double_moldsmal [2];
    int triple_moldsmal [3];
    int froggit_whimsun [2];
    int double_froggit [2];

    // last term is for the number of transitions in the first half which is static
    int ruins_first_half_transition = 9;
    int ruins_second_half_transition = 19;
    int ruins_general = 4057 + 28 + 90 + 50 + 31 + 33 + 5 + 12 * ruins_first_half_transition + 244 + 116 + 27 - ruins_second_half_transition + 4565;

    int leaf_pile_steps = 97;
    int frog_skip_save = 90;

    Times ()
    : single_froggit{ 343, 328 },
    double_moldsmal { 845, 518 },
    triple_moldsmal { 1326, 998, 516 },
    froggit_whimsun { 527, 248 },
    double_froggit { 765, 486} {

    }

};

// handles the method for simulating a ruins run
class Ruins {
    int time = 0;
    Undertale& undertale;
    Times& times;

public:
    // assigning undertale from the constructor so that the randomizer won't be reconstructed and such
    Ruins (Undertale& undertale_engine, Times& execution_times)
    : undertale(undertale_engine),
    times(execution_times)
    {}

    int simulate () {
        // initializing vars
        
        // initial time includes execution that is always the same
        time = times.ruins_general;
        int kills = 0;

        // lv and exp are like this since the grind begins after killing first frog
        int lv = 2;
        int exp = 10;

        // loop for the first half
        while (kills < 13) {
            // lv 3 is guaranteed before first half end, therefore put this here
            // and leave both loops for each half independent
            if (exp >= 30) {
                lv = 3;
            }

            // first half step counting
            int steps = undertale.src_steps(80, 40, 20, kills);
            // for first encounter, you need to at least get to the enter, imposing a higher minimum step count
            if (kills == 0 && steps < times.leaf_pile_steps) {
                steps = times.leaf_pile_steps;
            }
            time += steps + undertale.encounter_time();

            int encounter = undertale.ruins1();
            // for the froggit encounter
            if (encounter == 0) {
                exp += 4;
                if (lv == 2) time += times.single_froggit[0];
                else time += times.single_froggit[1];
                time -= times.frog_skip_save * undertale.frogskip();
            } else {
                time += times.whimsun;
                exp += 3;
            }
            kills++;
        }

        while (kills < 20) {
            int encounter = undertale.ruins3();
            
            // define variables as being "1" because of how the time arrays are made, this can be
            // used to sum the index
            int at_18 = kills >= 18 ? 1 : 0; 
            int at_19 = kills >= 19 ? 1 : 0;

            // general encounter times
            time += times.ruins_second_half_transition +
                undertale.src_steps(60, 60, 20, kills) +
                undertale.encounter_time();

            bool is_encounter_0 = encounter == 0;
            if (is_encounter_0 || encounter > 2) { // 2 monster encounters
                if (is_encounter_0 || encounter == 3) { // for frog encounters
                    if (is_encounter_0) { // for frog whim
                        time += times.froggit_whimsun[at_19];
                    } else { // for 2x frog
                        time += times.double_froggit[at_19];
                    }
                    // number of frog skipps achievable depends on how many it is being fought
                    for (int max = 1 + at_19, i = 0; i < max; i++) {
                        time -= times.frog_skip_save * undertale.frogskip();
                    }
                } else { // for for 2x mold
                    time += times.double_moldsmal[at_19];
                }
                kills += 2;
            } else if (encounter == 1) { // single mold
                time += times.single_moldsmal;
                kills++;
            } else { // triple mold
                time += times.triple_moldsmal[at_18 + at_19];
                kills += 3;
            }
        }

        return time;
    }
};

class Simulator {
    Times times;
    Undertale undertale;
public:
    double get_ruins_percentage (int simulations, int minutes, int seconds) {
        int success = 0;
        int treshold = (minutes * 60 + seconds) * 30;
        for (int i = 0; i < simulations; i++) {
            if (i % 10000 == 0) cout << i << endl;
            Ruins ruins(undertale, times);
            if (ruins.simulate() <= treshold) success++;
        }

        return (double) success / (double) simulations;
    }
};

int main () {
    // testing all chances
    Undertale undertale;
    Simulator simulator;
    int simulations = 1'000'000;
    int success = 0;
    for (int i = 0; i < simulations; i++) {
        if (undertale.src_steps(60, 60, 20, 15) > 361) success++;
    }   

    cout << (double) success / (double) simulations << endl;
    cout << simulator.get_ruins_percentage(simulations, 10, 0) << endl;


    return 0;
}

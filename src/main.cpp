#include <iostream>
#include <cmath>
#include <string>
#include <fstream>

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
        return (int) steps + 1;
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
    : single_froggit { 343, 328 },
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
                // do arithmetic on the lv for a slight optimization based on how the array is built
                time += times.single_froggit[lv - 2];
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
                    // number of frog skips achievable depends on how many it is being fought
                    for (int max = 2 - at_19, i = 0; i < max; i++) {
                        time -= times.frog_skip_save * undertale.frogskip();
                    }
                } else { // for 2x mold
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

// class handle probability distributions, a discrete description is used to approximate a continuous distribution
// the distribution starts at a minimum value up to a maximum value, with "bins" with a size
// everything outside the range is given as 0
// the distributions are not normalized
class ProbabilityDistribution {
    int min;
    int max;
    int interval;
    int* distribution;
    int length;
    int total = 0;

public:
    ProbabilityDistribution (int min_value, int max_value, int interval_value, int* values, int size)
        : min(min_value),
        max(max_value),
        interval(interval_value) {
            // add +1 to include the maximum value as well, due to 0-indexing
            // length is calculated from the range and bin size
            length = (max + 1 - min) / interval;

            // actual distribution is just an array where each element of the array represents a "x" position
            // and the value is the number of time it appears in that "x" position
            distribution = new int[length] {0};
            for (int i = 0; i < size; i++) {
                int pos = get_distribution_pos(values[i]);
                distribution[pos]++;
                total++;
            }
        }

    // get the "x" position for a value in the distribution
    int get_distribution_pos (int value) {
        return (value - min) / interval;
    }

    // get the chance a value is in the inclusive interval min to max
    double get_chance (int min, int max) {
        int favorable = 0;
        int lower_pos = get_distribution_pos(min);
        int higher_pos = get_distribution_pos(max);
        for (int i = lower_pos; i < higher_pos + 1; i++) {
            favorable += distribution[i];
        }
        return (double) favorable / (double) total;
    }

    // get the chance a value is in the interval starting at the minimum up to a value
    double get_chance_up_to (int max) {
        return get_chance(min, max);
    }

    // get the chance a value is in the interval starting at a given minimum value up to the max
    double get_chance_from (int min) {
        return get_chance(min, max);
    }


    // simulating a rieman integral for the functions below like \int x \rho(x) dx / \int \rho(x) dx
    
    // get average value
    double get_average () {
        double numerator = 0;
        for (int i = 0; i < length; i++) {
            numerator += (min + interval * i) * distribution[i];
        }
        return numerator / (double) total;
    }

    // get average of square of values
    double get_sqr_avg () {
        double numerator = 0;
        for (int i = 0; i < length; i++) {
            numerator += std::pow(min + interval * i, 2) * distribution[i];
        }
        return numerator / (double) total;
    }

    // get the standard deviation of the values
    double get_stdev () {
        return std::sqrt(get_sqr_avg() - std::pow(get_average(), 2));
    }

    void export_dist (std::string name) {
        std::ofstream file(name);
        for (int i = 0; i < length; i++) {
            file << min + i << "," << distribution[i] << std::endl;
        }
        file.close();
    }
};

// handles methods for gathering results from simulations
class Simulator {
    Times times;
    Undertale undertale;
public:
    // get the distribution for ruins runs
    ProbabilityDistribution get_dist (int simulations, int min, int max) {
        int* results = new int[simulations];
        for (int i = 0; i < simulations; i++) {
            Ruins ruins(undertale, times);
            results[i] = ruins.simulate();
        }

        int size = sizeof(results) / sizeof(results[0]);
        ProbabilityDistribution dist(min, max, 1, results, simulations);
        return dist;
    }

    // uses a mathematical formula to calculate the marging of error from a calculated probability
    double get_error_margin (int n, double probability) {
        return 2.6 * std::sqrt((probability) * (1 - probability) / (double) n);
    }
};

int main () {
    // // testing all chances
    // Undertale undertale;
    Simulator simulator;
    int simulations = 1'000'000;


    int values[] = { 1, 4, 3, 3, 5, 9, 10, 5, 5, 5, 3, 1, 10, 8, 9, 7, 1, 1 };
    int length = sizeof(values) / sizeof(values[0]);
    ProbabilityDistribution dist = simulator.get_dist(simulations, 8 * 60 * 30, 13 * 60 * 30);
    cout << dist.get_average() << endl;
    cout << dist.get_stdev() << endl;
    cout << dist.get_chance(8 * 60 * 30, 10 * 60 * 30);
    dist.export_dist("testing.txt");

    return 0;
}

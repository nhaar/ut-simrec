#include <iostream>
#include <cmath>
#include <string>
#include <fstream>
#include <unordered_map>
#include <vector>
#include <sstream>
#include <filesystem>
#include <shlobj.h>
#include "random.hpp"
#include "undertale.hpp"
#include "times.hpp"
#include "recording_reader.hpp"

using namespace std;

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

    void build_dist (int* values, int size) {
        // add +1 to include the maximum value as well, due to 0-indexsng
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

public:
    ProbabilityDistribution (int interval_value, int* values, int size)
    : interval(interval_value), max(0), min(std::numeric_limits<int>::max()) {
        // determine max and minimum values in the distribution
        for (int i = 0; i < size; i++) {
            int current = values[i];
            if (current > max) max = current;
            else if (current < min) min = current;
        }
        build_dist(values, size);
    }

    ProbabilityDistribution (int min_value, int max_value, int interval_value, int* values, int size)
        : min(min_value), max(max_value), interval(interval_value) {
            build_dist(values, size);
        }

    // get the "x" position for a value in the distribution
    int get_distribution_pos (int value) {
        if (value < min) return 0;
        if (value > max) return length;
        return (value - min) / interval;
    }

    // get the chance a value is in the interval min (including) to max (excluding)
    double get_chance (int min, int max) {
        int favorable = 0;
        int lower_pos = get_distribution_pos(min);
        int higher_pos = get_distribution_pos(max);
        for (int i = lower_pos; i < higher_pos; i++) {
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

// handles methods for generating simulations and gathering its results
class Simulator {
public:
    Times& times;

    virtual int simulate() = 0;
    
    Simulator (Times& times_value) : times(times_value) {}

    // run simulations and generate a probability distribution for the results
    ProbabilityDistribution get_dist (int simulations) {
        int* results = new int[simulations];
        for (int i = 0; i < simulations; i++) {
            results[i] = simulate();
        }

        int size = sizeof(results) / sizeof(results[0]);
        return ProbabilityDistribution (1, results, simulations);
    }

    // uses a mathematical formula to calculate the marging of error from a calculated probability
    double get_error_margin (int n, double probability) {
        return 2.6 * std::sqrt((probability) * (1 - probability) / (double) n);
    }
};

// handles the method for simulating a ruins run
class Ruins : public Simulator {

public:
    Ruins (Times& times_value) : Simulator(times_value) {}

    int simulate() override {
        // initializing vars
        
        // initial time includes execution that is always the same
        int time = times.ruins_general;
        int kills = 0;

        // add all static blcons (6x nobody came, 1x first froggit, 13x first half)
        time += Undertale::encounter_time_random(20);

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
            int steps = Undertale::src_steps(80, 40, 20, kills);
            // for first encounter, you need to at least get to the enter, imposing a higher minimum step count
            if (kills == 0 && steps < Undertale::leaf_pile_steps) {
                steps = Undertale::leaf_pile_steps;
            }
            time += steps;

            int encounter = Undertale::ruins1();
            // for the froggit encounter
            if (encounter == 0) {
                exp += 4;
                // do arithmetic on the lv for a slight optimization based on how the array is built
                time += times.single_froggit[lv - 2];
                time += times.frog_skip_save * Undertale::frogskip();
            } else {
                time += times.whimsun;
                exp += 3;
            }
            kills++;
        }

        while (kills < 20) {
            int encounter = Undertale::ruins3();
            
            // define variables as being "1" because of how the time arrays are made, this can be
            // used to sum the index
            int at_18 = kills >= 18 ? 1 : 0; 
            int at_19 = kills >= 19 ? 1 : 0;

            // don't add on first since it's included in general
            if (kills != 13) {
                time += times.ruins_second_half_transition;
            }
            // general encounter times
            time += times.ruins_second_half_transition +
                Undertale::src_steps(60, 60, 20, kills) +
                Undertale::encounter_time_random();

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
                        time += times.frog_skip_save * Undertale::frogskip();
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

int main (int arc, char *argv[]) {
    // reading argv for the settings
    PWSTR app_data;
    HRESULT hr = SHGetKnownFolderPath(FOLDERID_LocalAppData, 0, NULL, &app_data);
    wstring_convert<codecvt_utf8_utf16<wchar_t>> converter;
    string dir = converter.to_bytes(app_data) + "\\UNDERTALE_linux_steamver\\recordings";
    bool calculate_chance = false;
    bool get_avg = false;
    bool get_stdev = false;
    int chance_min = -1;
    int chance_max = -1;
    bool use_best = false;
    int simulations = 1'000'000;

    /*
    command line arguments
    -d "..." -> set directory
    -c -> sets calculate chance to true
    -s -> set number of simulations
    -a -> calculate average
    -e -> calculate error (stdev)
    -n ... -> set min time in frames
    -x ... -> set max time in frames
    -b -> set use_best to true
    */

    int cur_arg = 1;
    while (cur_arg < arc) {
        switch (argv[cur_arg][1]) {
            case 'd':
                cur_arg++;
                dir = argv[cur_arg];
                break;
            case 'c':
                calculate_chance = true;
                break;
            case 's':
                cur_arg++;
                simulations = stoi(argv[cur_arg]);
                break;
            case 'a':
                get_avg = true;
                break;
            case 'e':
                get_stdev = true;
                break;
            case 'n':
                cur_arg++;
                chance_min = stoi(argv[cur_arg]);
                break;
            case 'x':
                cur_arg++;
                chance_max = stoi(argv[cur_arg]);
                break;
            case 'b':
                use_best = true;
                break;
        }
        cur_arg++;
    }

    // seed program
    srand(time(0));

    RecordingReader reader(dir);
    Times times;
    if (use_best) times = reader.get_best();
    else times = reader.get_average();
    Ruins simulator(times);
    ProbabilityDistribution dist = simulator.get_dist(simulations);
    if (calculate_chance) {
        double chance;
        if (chance_min == -1 && chance_max == -1) chance = 1;
        else if (chance_min == -1) chance = dist.get_chance_up_to(chance_max);
        else if (chance_max == -1) chance = dist.get_chance_from(chance_min);
        else chance = dist.get_chance(chance_min, chance_max);
        cout << "Chance: " << chance << endl;;
    }
    if (get_avg) {
        cout << "Average: " << dist.get_average() << endl;
    }
    if (get_stdev) {
        cout << "Standard Deviation: " << dist.get_stdev() << endl;
    }

    return 0;
}

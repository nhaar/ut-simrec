#include <iostream>
#include <cmath>
#include <string>
#include <fstream>
#include <unordered_map>
#include <vector>
#include <sstream>
#include <filesystem>

namespace fs = std::filesystem;
using namespace std;


// handle methods for generating random numbers
class Random {
public:
    // get a random double between 0 and 1
    static double random_number () {
        return (double)(rand()) / (double)(RAND_MAX);
    }
};


// handle methods specific to the undertale engine
class Undertale {
private:
    // including this method since technically Undertale's rounding at halfway rounds to nearest even number
    // will leave it here for easy of changing that but the difference is technically negligible considering
    // halfway is not something that will be generated from random anyways
    static int round (double number) {
        return std::round(number);
    }

    // simulating the round random generator from Mr Tobias
    int roundrandom (int max) {
        return round(Random::random_number() * max);
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
        double roll = Random::random_number();
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
        double roll = Random::random_number();
        if (roll < 0.25) return 0;
        if (roll < 0.5) return 1;
        if (roll < 0.75) return 2;
        if (roll < 0.9) return 3;
        return 4;
    }

    // random odds for a frog skip
    // 1 = no frogskip
    // 0 = gets frogskip
    int frogskip () {
        double roll = Random::random_number();
        if (roll < 0.405) return 0;
        return 1;
    }

    // total time required to encounter + blcon
    int encounter_time () {
        return 47 + roundrandom(5);

    }
};

class Times {
public:

    int whimsun;
    int single_moldsmal;

    // last term is for the number of transitions in the first half which is static
    int ruins_second_half_transition;
    int ruins_general;

    int leaf_pile_steps = 97;
    int frog_skip_save;

    int single_froggit [2];
    int double_moldsmal [2];
    int triple_moldsmal [3];
    int froggit_whimsun [2];
    int double_froggit [2];

    Times () {}

    Times (std::unordered_map<string, int> map) {
        ruins_general = 0;
        int lv_up_msg = 0;
        int you_won_msg = 0;
        int frogskip_turn = 0;
        int not_frogskip_turn = 0;
        for (const auto& pair : map) {
            const std::string& key = pair.first;
            int time = map[key];
            if (
                key == "ruins-start" ||
                key == "ruins-hallway" ||
                key == "ruins-leafpile" ||
                key == "ruins-leafpile-transition" ||
                key == "ruins-leaf-fall" ||
                key == "leaf-fall-transition" ||
                key == "ruins-one-rock" ||
                key == "ruins-maze" ||
                key == "ruins-three-rock" ||
                key == "three-rock-transition" ||
                key == "ruins-napsta" ||
                key == "ruins-switches" ||
                key == "ruins-perspective-a" ||
                key == "ruins-perspective-b" ||
                key == "ruins-perspective-c" ||
                key == "ruins-perspective-d" ||
                key == "ruins-end"
            ) {
                ruins_general += time;
            } else if (key == "ruins-first-transition") {
                ruins_general += 10 * time;
            } else if (key == "froggit-lv2") {
                single_froggit[0] = time;
            } else if (key == "froggit-lv3") {
                single_froggit[1] = time;
            } else if (key == "frogskip") {
                frogskip_turn = time;
            } else if (key == "not-frogskip") {
                not_frogskip_turn = time;
            } else if (key == "whim") {
                whimsun = time;
            } else if (key == "lv-up") {
                lv_up_msg = time;
            } else if (key == "you-won") {
                you_won_msg = time;
            } else if (key == "sgl-mold") {
                single_moldsmal = time;
            } else if (key == "dbl-mold") {
                double_moldsmal[0] = time;
            } else if (key == "dbl-mold-19") {
                double_moldsmal[1] = time;
            } else if (key == "tpl-mold") {
                triple_moldsmal[0] = time;
            } else if (key == "tpl-mold-18") {
                triple_moldsmal[1] = time;
            } else if (key == "tpl-mold-19") {
                triple_moldsmal[2] = time;
            } else if (key == "frog-whim") {
                froggit_whimsun[0] = time;
            } else if (key == "frog-whim-19") {
                froggit_whimsun[1] = time;
            } else if (key == "dbl-frog") {
                double_froggit[0] = time;
            } else if (key == "dbl-frog-19") {
                double_froggit[1] = time;
            } else if (key == "ruins-second-transition") {
                ruins_second_half_transition = time;
            }
        }
        // add "relative" times
        ruins_general += lv_up_msg - you_won_msg;
        frog_skip_save = not_frogskip_turn - frogskip_turn;
    }
};

class RecordingReader {
    std::string dir;
public:
    RecordingReader (std::string dirPath) {
        dir = dirPath;
    }

    Times get_average () {
        std::unordered_map<std::string, int> avg_map;

        bool is_first = true;
        int total = 0;
        for (const auto& entry : fs::directory_iterator(dir)) {
            total++;
            if (fs::is_regular_file(entry)) {
                auto cur_map = read_file(entry.path().string());
                if (is_first) {
                    avg_map = cur_map;
                    is_first = false;
                } else {
                    for (const auto& pair : cur_map) {
                        const std::string& key = pair.first;
                        avg_map[key] += cur_map[key];
                    }
                }
            }
        }
        // divide everything by total
        for (const auto& pair : avg_map) {
            const std::string& key = pair.first;
            avg_map[key] = static_cast<int>(std::round((double)avg_map[key] / (double)total));
        }
        return avg_map;
    }

    Times get_max () {
        std::unordered_map<std::string, int> max_map;

        bool is_first = true;
        for (const auto& entry : fs::directory_iterator(dir)) {
            if (fs::is_regular_file(entry)) {
                auto cur_map = read_file(entry.path().string());
                if (is_first) {
                    max_map = cur_map;
                    is_first = false;
                } else {
                    for (const auto& pair : cur_map) {
                        const std::string& key = pair.first;
                        int cur_time = cur_map[key];
                        if (cur_time > max_map[key]) max_map[key] = cur_time;
                    }
                }
            }
        }

        Times times(max_map);

        return times;
    }

    std::unordered_map<std::string, int> read_file (std::string filePath) {
        std::ifstream file(filePath);
        std::stringstream buffer;
        buffer << file.rdbuf();
        std::string content = buffer.str();

        std::unordered_map<std::string, int> map;
        std::string key = "";
        std::string value = "";
        bool is_key = true;

        for (char c : content) {
            if (is_key) {
                if (c == '=') is_key = false;
                else key += c;
            } else {
                if (c == ';') {
                    is_key = true;
                    // `value` is in microseconds
                    // then round to nearest 30fps frame
                    int time = static_cast<int>(std::round(std::stod(value) / 1e6 * 30));
                    map[key] = time;
                    key = "";
                    value = "";
                }
                else value += c;
            }
        }

        return map;
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

        // add all static blcons (6x nobody came, 1x first froggit, 13x first half)
        for (int i = 0; i < 20; i++) {
            time += undertale.encounter_time();
        }

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
            time += steps;

            int encounter = undertale.ruins1();
            // for the froggit encounter
            if (encounter == 0) {
                exp += 4;
                // do arithmetic on the lv for a slight optimization based on how the array is built
                time += times.single_froggit[lv - 2];
                time += times.frog_skip_save * undertale.frogskip();
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

            // don't add on first since it's included in general
            if (kills != 13) {
                time += times.ruins_second_half_transition;
            }
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
                        time += times.frog_skip_save * undertale.frogskip();
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
        std::cout << max << std::endl;
        std::cout << min << std::endl;
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

// handles methods for gathering results from simulations
class Simulator {
    Times times;
    Undertale undertale;
public:
    Simulator (Times times_arg) {
        times = times_arg;
    }

    // get the distribution for ruins runs
    ProbabilityDistribution get_dist (int simulations) {
        int* results = new int[simulations];
        for (int i = 0; i < simulations; i++) {
            Ruins ruins(undertale, times);
            results[i] = ruins.simulate();
        }

        int size = sizeof(results) / sizeof(results[0]);
        return ProbabilityDistribution (1, results, simulations);
    }

    // uses a mathematical formula to calculate the marging of error from a calculated probability
    double get_error_margin (int n, double probability) {
        return 2.6 * std::sqrt((probability) * (1 - probability) / (double) n);
    }
};

int main () {
    // seed program
    std::srand(time(0));

    // // testing all chances
    // Undertale undertale;
    int simulations = 1'000'000;

    RecordingReader reader(".\\recordings");
    Times times = reader.get_max();
    
    Simulator simulator(times);

    ProbabilityDistribution dist = simulator.get_dist(simulations);
    cout << dist.get_average() << endl;
    cout << dist.get_stdev() << endl;
    cout << dist.get_chance(5 * 60 * 30, 6 * 60 * 30);

    return 0;
}

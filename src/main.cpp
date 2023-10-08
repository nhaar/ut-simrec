#include <iostream>
#include <filesystem>
#include <shlobj.h>
#include "random.hpp"
#include "undertale.hpp"
#include "times.hpp"
#include "recording_reader.hpp"
#include "probability_distribution.hpp"
#include "simulator.hpp"
#include "ruins.hpp"
#include "snowdin.hpp"
#include "waterfall.hpp"
#include "endgame.hpp"
#include "full_game.hpp"

using namespace std;

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
    string run;

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
            case 'r':
                cur_arg++;
                run = argv[cur_arg];
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

    Simulator* simulator = nullptr;
    if (run == "ruins") {
        simulator = new Ruins(times);
    } else if (run == "snowdin") {
        simulator = new Snowdin(times);
    } else if (run == "waterfall") {
        simulator = new Waterfall(times);
    } else if (run == "endgame") {
        simulator = new Endgame(times);
    } else if (run == "full") {
        simulator = new FullGame(times);
    }
    else throw new exception();

    ProbabilityDistribution dist = simulator->get_dist(simulations);
    delete simulator;
    
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

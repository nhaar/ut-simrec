#include <filesystem>
#include <cmath>
#include <fstream>
#include "recording_reader.hpp"

namespace fs = std::filesystem;

RecordingReader::RecordingReader (std::string dir_path) : dir(dir_path) {}

// create an object with the average times of all files in the directory
Times RecordingReader::get_average () {
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
    // divide everything by total to get average
    for (const auto& pair : avg_map) {
        const std::string& key = pair.first;
        avg_map[key] = static_cast<int>(std::round((double)avg_map[key] / (double)total));
    }

    return avg_map;
}

// create an object with the fastest times in the directory
Times RecordingReader::get_best () {
    std::unordered_map<std::string, int> best_map;

    bool is_first = true;
    for (const auto& entry : fs::directory_iterator(dir)) {
        if (fs::is_regular_file(entry)) {
            auto cur_map = read_file(entry.path().string());
            if (is_first) {
                best_map = cur_map;
                is_first = false;
            } else {
                for (const auto& pair : cur_map) {
                    const std::string& key = pair.first;
                    int cur_time = cur_map[key];
                    if (cur_time < best_map[key]) best_map[key] = cur_time;
                }
            }
        }
    }

    Times times(best_map);

    return times;
}

// read a file and generate a map with all its segments pointing to their time
std::unordered_map<std::string, int> RecordingReader::read_file (std::string filePath) {
    // getting all file content as a string
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

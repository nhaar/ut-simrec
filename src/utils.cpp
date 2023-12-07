#include <vector>
#include <string>
#include <iomanip>
#include <sstream>
#include <cmath>
#include "utils.hpp"
#include <iostream>

int Utils::time_to_frame (char* time) {
    std::vector<std::string> numbers = std::vector<std::string>();
    numbers.push_back("");
    int number_count = 0;
    for (int i = 0; time[i] != '\0'; i++) {
        char c = time[i];
        if (c == ':') {
            number_count++;
            numbers.push_back("");
        } else {
            numbers[number_count] += c;
        }
    }
    
    int hours = 0;
    int start_index = number_count == 2 ? 1 : 0;
    if (number_count == 2) {
        hours = std::stoi(numbers[0]);
    }
    int minutes = hours * 60 + stoi(numbers[start_index]);
    double seconds = minutes * 60 + std::stod(numbers[start_index + 1]);
    int frames = static_cast<int>(std::round(seconds * 30));
    return frames;
}

std::string Utils::frame_to_time (int frame) {
    int seconds = frame / 30;
    int hours = seconds / 3600;
    int minutes = (seconds % 3600) / 60;
    int remaining_seconds = seconds % 60;

    std::ostringstream time_stream;
    time_stream << std::setfill('0') << std::setw(2) << hours << ":"
              << std::setfill('0') << std::setw(2) << minutes << ":"
              << std::setfill('0') << std::setw(2) << remaining_seconds;

    return time_stream.str();
}
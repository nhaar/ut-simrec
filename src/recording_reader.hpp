#ifndef RECORDING_READER_H
#define RECORDING_READER_H

#include "times.hpp"


// class that handles reading the recording files outputted by the mod
class RecordingReader {
    // path to directory where recording files are stored
    std::string dir;
public:
    RecordingReader (std::string dir_path);

    Times get_average ();

    Times get_best ();

    std::unordered_map<std::string, int> read_file (std::string filePath);
};

#endif
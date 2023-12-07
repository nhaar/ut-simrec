#ifndef UTILS_H
#define UTILS_H

class Utils {
public:
    static int time_to_frame (char* time);

    static std::string frame_to_time (int frame);
};

#endif
#ifndef UNDERTALE_H
#define UNDERTALE_H

// handle methods specific to the undertale engine
class Undertale {
private:
    static int round (double number);

    static int roundrandom (int max);
public:
    static int src_steps (int min_steps, int steps_delta, int max_kills, int kills);

    static int ruins1 ();

    static int ruins3 ();

    static int frogskip ();

    static int heart_flick;

    static int encounter_time_random ();

    static int encounter_time_random(int number_of_times);

    static int encounter_time_average_total (int number_of_times);

    static int snowdin ();

    static int dogskip ();

    static int glowing_water_encounter ();

    static int waterfall_grind_encounter ();

    static int waterfall_grind_steps (int kills);
};

#endif
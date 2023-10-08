#include <iostream>
#include "thirdparty/pugixml.hpp"
#include "time_structure.hpp"
#include "times.hpp"

Times::Times () {}

bool Times::value_walker::for_each (pugi::xml_node& node) {
    std::string name = node.name();
    if (name == "") {
        found_value = node.value();
        return false;
    }
    
    return true;
}

bool Times::delta_walker::for_each (pugi::xml_node& node) {
    std::string name = node.name();

    if (name == "name" || name == "pos" || name == "neg") {
        value_walker walker;
        node.traverse(walker);
        if (name == "name") delta_name = walker.found_value;
        else if (name == "pos") delta_value += segments[walker.found_value];
        else delta_value -= segments[walker.found_value];
    }

    return true;
}

bool Times::def_walker::for_each (pugi::xml_node& node) {
    std::string name = node.name();
    if (depth() == 1) {
        if (name == "delta") {
            delta_walker walker(segments);
            node.traverse(walker);
            segments[walker.delta_name] = walker.delta_value;
        }
    }

    return true;
}

bool Times::loop_walker::for_each (pugi::xml_node& node) {
    std::string name = node.name();
    if (depth() == 0) {
        std::string name = node.name();
        if (name == "static" || name == "variant") {
            value_walker walker;
            node.traverse(walker);
            if (name == "static") {
                segments[area] += times * segments[walker.found_value];
            } else {
                if (walker.found_value == "blcon") {
                    static_blcons[area] += times;
                }
            }
        }
    }
    return true;
}

bool Times::structure_walker::for_each (pugi::xml_node& node) {
    std::string name = node.name();
    if (depth() == 1) {
        if (name == "area") {
            area = node.first_attribute().value();
            segments[area] = 0;
            static_blcons[area] = 0;
        }
    } else if (depth() == 2) {
        if (name == "def") {
            def_walker walker(segments);
            node.traverse(walker);
        } else if (name == "static") {
            value_walker walker;
            node.traverse(walker);
            last_static = walker.found_value;
            segments[area] += segments[walker.found_value];
        } else if (name == "variant") {
            value_walker walker;
            node.traverse(walker);
            if (walker.found_value == "steps*") {
                int* steps_value = new int[2];
                steps_value[0] = segments[last_static + "-steps"];
                steps_value[1] = segments[last_static + "-endsteps"];
                steps[last_static] = steps_value;
            } else if (walker.found_value == "blcon") {
                static_blcons[area] += 1;
            }
        } else if (name == "delta") {
            delta_walker walker(segments);
            node.traverse(walker);
            segments[area] += walker.delta_value;
        } else if (name == "loop") {
            std::string times = node.first_attribute().value();
            if (times != "?") {
                int loop_times = std::stoi(times);
                loop_walker walker(segments, static_blcons, area, loop_times);
                node.traverse(walker);
            }
        }
    }

    return true;
}

Times::Times (std::unordered_map<std::string, int> map) {
    pugi::xml_document doc;
    pugi::xml_parse_result result = doc.load_string(time_structure);

    segments = map;
    structure_walker walker(segments, static_blcons, steps);
    doc.traverse(walker); 
}
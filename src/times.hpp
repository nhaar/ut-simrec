#ifndef TIMES_H
#define TIMES_H

#include <string>
#include <unordered_map>
#include "thirdparty/pugixml.hpp"

// class stores all the times that rely on execution
class Times {
public:
    // map keeps track of all recorded and some calculated segments
    std::unordered_map<std::string, int> segments;

    // map keeps track of all step information required for fixing
    std::unordered_map<std::string, int*> steps;

    // map keeps track of how many static (guaranted, that is always happen) random "blcon" animations are in a given area
    std::unordered_map<std::string, int> static_blcons;
    // comments below refer to the name of the segments in the recorder

    Times ();

    Times (std::unordered_map<std::string, int> map);

    // traverse all of the XML tree to find what the relevant segments are
    struct structure_walker : pugi::xml_tree_walker {
        structure_walker (
            std::unordered_map<std::string, int>& segments,
            std::unordered_map<std::string, int>& static_blcons,
            std::unordered_map<std::string, int*>& steps
        ) : segments(segments), static_blcons(static_blcons), steps(steps) {}

        // references below point to the ones in Times
        std::unordered_map<std::string, int>& segments;
        std::unordered_map<std::string, int>& static_blcons;
        std::unordered_map<std::string, int*>& steps;

        // value of the current area being traversed
        std::string area;

        // value of the last static node analyzed
        std::string last_static;

        virtual bool for_each (pugi::xml_node& node);
    };

    // traverser for the XML tree for a def node
    struct def_walker : pugi::xml_tree_walker {
        def_walker (
            std::unordered_map<std::string, int>& segments
        ) : segments(segments) {}

        // reference to the `Times` one
        std::unordered_map<std::string, int>& segments;

        virtual bool for_each (pugi::xml_node& node);
    };

    // traverser of the delta nodes
    struct delta_walker : pugi::xml_tree_walker {
        delta_walker (std::unordered_map<std::string, int>& segments) : segments(segments), delta_value(0) {}

        // reference to the one in `Times`
        std::unordered_map<std::string, int>& segments;

        // if the delta node has a name (that is inside a def), find its name
        std::string delta_name;

        // to store the total time of the delta after finishing reading it
        int delta_value;

        virtual bool for_each (pugi::xml_node& node);
    };

    // traverse a node to find its value
    struct value_walker : pugi::xml_tree_walker {
        // to store the value found
        std::string found_value;

        virtual bool for_each (pugi::xml_node& node);
    };

    // traverse a loop node
    struct loop_walker : pugi::xml_tree_walker {
        loop_walker (
            std::unordered_map<std::string, int>& segments,
            std::unordered_map<std::string, int>& static_blcons,
            std::string area, int times) : segments(segments), static_blcons(static_blcons), area(area), times(times) {}

        // reference to the ones in `Times`
        std::unordered_map<std::string, int>& segments;
        std::unordered_map<std::string, int>& static_blcons;

        // name of the current area
        std::string area;

        // times value found in the loop
        int times;

        virtual bool for_each (pugi::xml_node& node);
    };
};

#endif
#ifndef TIME_STRUCTURE_H
#define TIME_STRUCTURE_H

// XML containing the definitions of the variables used in the simulator and acquired from the recorder
const char* time_structure = R"(
<segments>
    <area name="ruins">
        <def>
            <name>frogskip-save</name>
            <or><null /></or>
            <or>
                <delta>
                    <name>frogskip-save</name>
                    <pos><static>not-frogskip</static></pos>
                    <neg><static>frogskip</static></neg>
                </delta>
            </or>
        </def>
        <def>
            <name>first-half-encounter</name>
            <or>
                <static>froggit-lv2</static>
                <variant>frogskip-save</variant>
            </or>
            <or>
                <static>froggit-lv2</static>
                <variant>frogskip-save</variant>
            </or>
        </def>
        <def>
            <name>2x-mold</name>
            <or><static>dbl-mold</static></or>
            <or><static>dbl-mold-19</static></or>
        </def>
        <def>
            <name>3x-mold</name>
            <or><static>tpl-mold</static></or>
            <or><static>tpl-mold-18</static></or>
            <or><static>tpl-mold-19</static></or>
        </def>
        <def>
            <name>froggit-whimsun</name>
            <or>
                <static>frog-whim</static>
                <variant>frogskip-save</variant>
            </or>
            <or>
                <static>frog-whim-19</static>
                <variant>frogskip-save</variant>
            </or>
        </def>
        <def>
            <name>2x-frog</name>
            <or>
                <static>dbl-frog</static>
                <variant>frogskip-save</variant>
            </or>
            <or>
                <static>dbl-frog-19</static>
                <variant>frogskip-save</variant>
            </or>
        </def>
        <def>
            <name>second-half-encounter</name>
            <or><static>sgl-mold</static></or>
            <or><variant>2x-mold</variant></or>
            <or><variant>3x-mold</variant></or>
            <or><variant>froggit-whimsun</variant></or>
            <or><variant>2x-frog</variant></or>
        </def>
        
        <static>ruins-start</static>
        <variant>blcon</variant>
        <static>ruins-hallway</static>
        <static>ruins-leaf-pile</static>
        <variant>steps*</variant>
        <variant>blcon</variant>
        <variant>first-half-encounter</variant>
        <static>leaf-pile-transition</static>
        <delta>
            <pos><static>lv-up</static></pos>
            <neg><static>you-won</static></neg>
        </delta>
        <loop times="10">
            <variant>steps</variant>
            <variant>blcon</variant>
            <variant>first-half-encounter</variant>
            <static>ruins-first-transition</static>
        </loop>
        
        <static>leaf-fall-downtime</static>
        <variant>steps</variant>
        <variant>blcon</variant>
        <variant>first-half-encounter</variant>
        <static>leaf-fall-transition</static>
        <static>ruins-one-rock</static>
        <variant>steps</variant>
        <variant>blcon</variant>
        <variant>first-half-encounter</variant>
        <static>one-rock-transition</static>
        <static>ruins-maze</static>
        <static>three-rock-downtime</static>
        <variant>steps</variant>
        <variant>blcon</variant>
        <variant>second-half-encounter</variant>
        <static>three-rock-transition</static>
        <variant>steps</variant>
        <variant>blcon</variant>
        <variant>second-half-encounter</variant>
        <loop times="?">
            <static>ruins-second-transition</static>
            <variant>steps</variant>
            <variant>blcon</variant>
            <variant>second-half-encounter</variant>
        </loop>
        <static>ruins-napsta</static>
        <variant>blcon</variant>
        <static>nobody-came</static>
        <static>ruins-switches</static>
        <variant>blcon</variant>
        <static>perspective-a</static>
        <variant>blcon</variant>
        <static>perspective-b</static>
        <variant>blcon</variant>
        <static>perspective-c</static>
        <variant>blcon</variant>
        <static>perspective-d</static>
        <variant>blcon</variant>
        <static>ruins-end</static>
    </area>
    <area name="snowdin">
        <def>
            <name>greater-dog-turn</name>
            <or><static>dogskip</static></or>
            <or><static>not-dogskip</static></or>
        </def>
        <def>
            <name>grind-encounter</name>
            <or><static>snowdin-dbl</static></or>
            <or><static>snowdin-tpl</static></or>
        </def>
        <def>
            <name>grind-jerry-encounter</name>
            <or><static>snowdin-dbl-jerry</static></or>
            <or><static>snowdin-tpl-jerry</static></or>
        </def>
        <static>ruins-to-snowdin</static>
        <static>snowdin-box-road</static>
        <variant>steps*</variant>
        <variant>blcon</variant>
        <static>sgl-snowdrake</static>
        <static>snowdin-human-rock</static>
        <variant>blcon</variant>
        <static>sgl-icecap</static>
        <static>snowdin-doggo</static>
        <static>ice-slide-downtime</static>
        <variant>blcon</variant>
        <static>lesser-dog</static>
        <static>before-dogi</static>
        <static>snowdin-dogi</static>
        <variant>steps*</variant>
        <variant>blcon</variant>
        <variant>grind-encounter</variant>
        <static>before-greater-dog</static>
        <static>greater-dog-1</static>
        <variant>greater-dog-turn</variant>
        <static>greater-dog-2</static>
        <variant>greater-dog-turn</variant>
        <static>greater-dog-end</static>

        <!-- grind begins, before moving right -->
        <loop times="?">    
            <variant>steps</variant>
            <variant>blcon</variant>
            <variant>grind-encounter</variant>
            <static>snowdin-right-transition</static>
        </loop>
        <!-- after moving left -->
        <loop times="?">
            <variant>steps</variant>
            <variant>blcon</variant>
            <variant>grind-encounter</variant>
            <static>snowdin-left-transition</static>
        </loop>
        <!-- all these below are after moving back right -->
        <!-- not killing Jerry and still having to grind at the end -->
        <loop times="?">    
            <variant>steps</variant>
            <variant>blcon</variant>
            <variant>grind-encounter</variant>
            <static>snowdin-right-transition</static>
        </loop>
        <!-- not killing Jerry and ending grind -->
        <loop times="?">    
            <variant>steps</variant>
            <variant>blcon</variant>
            <variant>grind-encounter</variant>
        </loop>
        <!-- killing Jerry (always ends grind by definition) -->
        <loop times="?">    
            <variant>steps</variant>
            <variant>blcon</variant>
            <variant>grind-jerry-encounter</variant>
        </loop>
        <static>snowdin-end</static>
    </area>
    <area name="waterfall">
        <def>
            <name>glowing-water-encounter</name>
            <or><static>sgl-woshua-shoes</static></or>
            <or><static>sgl-aaron-shoes</static></or>
            <or><static>woshua-aaron-surprise</static></or>
            <or><static>dbl-mold-shoes</static></or>
        </def>
        <def>
            <name>waterfall-grind-encounter</name>
            <or><static>woshua-mold</static></or>
            <or><static>woshua-aaron-surprise</static></or>
            <or><static>temmie</static></or>
        </def>
        <static>waterfall-start</static>
        <static>near-quiche</static>
        <variant>blcon</variant>
        <static>sgl-aaron-glove</static>
        <static>post-telescope</static>
        <variant>blcon</variant>
        <static>sgl-woshua-glove</static>
        <variant>steps</variant>
        <variant>blcon</variant>
        <static>dbl-mold-glove</static>
        <static>ballet-shoes-get</static>
        <static>glowing-water</static>
        <variant>blcon</variant>
        <variant>glowing-water-encounter</variant>
        <static>before-shyren</static>
        <variant>steps</variant>
        <variant>blcon</variant>
        <static>temmie</static>
        <variant>steps</variant>
        <variant>blcon</variant>
        <static>impostor-mold</static>
        <static>waterfall-grind-start-transition</static>
        <variant>steps</variant>
        <vartiant>blcon</vartiant>
        <static>woshua-aaron-aware</static>
        <static>waterfall-grind-transition-to-left</static>
        <variant>steps</variant>
        <variant>blcon</variant>
        <static>woshua-mold</static>
        <static>waterfall-grind-transition-to-right</static>
        <static>mushroom-maze</static>
        <variant>steps*</variant>
        <variant>blcon</variant>
        <variant>waterfall-grind-encounter</variant>
        <!-- below is if need to grind an extra encounter in mushroom maze -->
        <loop times="?">
            <static>mushroom-maze-going-back</static>
            <variant>steps</variant>
            <variant>blcon</variant>
            <variant>waterfall-grind-encounter</variant>
            <static>mushroom-maze-exit-after-backtrack</static>
        </loop>
        <static>mushroom-transition</static>
        <static>crystal-maze</static>
        <variant>steps*</variant>
        <loop times="?">
            <static>crystal-going-back</static>
            <variant>steps</variant>
            <variant>blcon</variant>
            <variant>waterfall-grind-encounter</variant>
            <static>crystal-exit-after-backtrack</static>
        </loop>
        <static>waterfall-end</static>
    </area>
    <area name="endgame">
        <def>
            <name>core-encounter</name>
            <or><static>sgl-astig</static></or>
            <or><static>core-frog-whim</static></or>
            <or><static>frog-astig</static></or>
            <or><static>whim-astig</static></or>
            <or><static>knight-madjick</static></or>
            <or><static>core-triple</static></or>
            <or><static>sgl-knight</static></or>
            <or><static>sgl-madjick</static></or>
        </def>
        <def>
            <name>core-end</name>
            <or>
                <loop times="3">
                    <variant>blcon</variant>
                    <static>nobody-came</static>
                </loop>
                <static>core-bridge</static>
            </or>
            <or>
                <variant>steps</variant>
                <variant>blcon</variant>
                <variant>core-encounter</variant>
                <static>grind-end-transition</static>
            </or>
        </def>
        <static>hotland-start</static>
        <variant>blcon</variant>
        <static>vulkin</static>
        <static>post-vulkin</static>
        <variant>blcon</variant>
        <static>tsunderplane</static>
        <static>post-tsunderplane</static>
        <loop times="?">
            <variant>steps</variant>
            <variant>blcon</variant>
            <variant>core-encounter</variant>
            <static>core-right-transition</static>
        </loop>
        <static>left-side-travel</static>
        <static>core-left-side-transition-1</static>
        <loop times="?">
            <static>core-left-side-transition-2</static>
            <variant>steps</variant>
            <variant>blcon</variant>
            <variant>core-encounter</variant>
            <static>core-left-side-transition-3</static>
        </loop>
        <static>core-warrior-path</static>
        <variant>core-end</variant>
        <static>run-end</static>
    </area>
</segments>
)";

#endif
/******
helper variables and functions
******/
void append (string codeName, string code) {
    Data.Code.ByName(codeName).AppendGML(code, Data);
}

void replace (string codeName, string text, string replacement) {
    ReplaceTextInGML(codeName, text, replacement);
}

void place (string codeName, string preceding, string placement) {
    ReplaceTextInGML(codeName, preceding, $"{preceding}{placement}");
}

string assignNonTakenIndex (string arr, string index, string value) {
    var arrAccess = arr + "[target_index]";
    return @$"
    var target_index = {index};
    // using 0 to indicate non existent elements
    while ({arrAccess} != 0) {{
        target_index++;
    }}
    {arrAccess} = {value};
    ";
}

string appendNewTime (string name, string time) {
    return @$"
    var file = file_text_open_append('recording_' + string(obj_time.session_name));
    file_text_write_string(file, {name} + '=' + string({time}) + ';');
    file_text_close(file);
    ";
}



var startSession = @"
obj_time.session_name = string(current_year) + string(current_month) + string(current_day) + string(current_hour) + string(current_minute) + string(current_second);
var file = file_text_open_write('recording_' + string(obj_time.session_name));
file_text_close(file);
";

string newStage (int stage = -1) {
    if (stage == -1) return "";
    return $"obj_time.stage = {stage};";
}

var startTime = @"
obj_time.is_timer_running = 1;
obj_time.time_start = get_timer();
";

string startSegment (string name, int stage = -1, bool isVarName = false) {
    string nameString = isVarName ? name : $"'{name}'";
    return @$"
    if (!obj_time.is_timer_running) {{
        {startTime}
        {newStage(stage)}
        obj_time.segment_name = {nameString};
    }}
    ";
}

string startDowntime (string name, int steps, int stage = -1) {
    return @$"
    if (!obj_time.is_downtime_mode) {{
        obj_time.is_downtime_mode = 1;
        obj_time.downtime = 0;
        obj_time.downtime_start = 0;
        obj_time.step_count = global.encounter;
        obj_time.optimal_steps = {steps.ToString()}
        {newStage(stage)}
        obj_time.downtime_name = '{name}';
    }}
    ";
}

var stopTime = @$"
if (obj_time.is_timer_running) {{
    obj_time.is_timer_running = 0;
    {appendNewTime("obj_time.segment_name", "get_timer() - obj_time.time_start")}
}}
";

var stopDowntime = @$"
// in case the downtime ends during a downtime, must not lose the time being counted
if (obj_time.is_downtime_mode) {{
    if (obj_time.is_downtime_running) {{
        obj_time.downtime += get_timer() + obj_time.downtime_start
    }}
    obj_time.is_downtime_mode = 0;
    {appendNewTime("obj_time.downtime_name", "obj_time.downtime")}
}}
";

string isFirstHalfStage = "obj_time.stage > 6 && obj_time.stage < 13";

// 7 is first possible stage here
string firstHalfCurrentEncounter = "var current_encounter = obj_time.first_half_encounters[obj_time.stage - 7];";

string isSecondHalfNoFleeStage = "obj_time.stage > 26 && obj_time.stage < 32";

string secondHalfNoFleeEncounter = "var current_encounter = obj_time.second_half_encounters[obj_time.stage - 27];";

string isSecondHalfFleeStage = "obj_time.stage > 32 && obj_time.stage < 37";

string secondHalfFleeEncounter = "var current_encounter = obj_time.second_half_encounters[obj_time.stage - 28];";


string tpTo (int room, int x, int y) {
    return tpTo(room.ToString(), x, y);
}

string tpTo (string room, int x, int y) {
    return @$"
    obj_time.tp_flag = 1;
    room = {room};
    obj_time.tp_x = {x};
    obj_time.tp_y = {y};
    ";
}

string isMoving = @"
keyboard_check(vk_left) || keyboard_check(vk_right) || keyboard_check(vk_up) || keyboard_check(vk_down)
";

string leafpileTp = tpTo(12, 240, 340);

string tpRuinsHallway = tpTo(11, 2400, 80);

/******
start of main script
******/

// testing script-game compatibility
EnsureDataLoaded();

if (Data?.GeneralInfo?.DisplayName?.Content.ToLower() != "undertale") {
    ScriptError("Error 0: Script must be used in Undertale");
}

// will be using obj_time for the API of the mod
var create = "gml_Object_obj_time_Create_0";
var step = "gml_Object_obj_time_Step_1";
var draw = "gml_Object_obj_time_Draw_64";

// code for specific game manipulation

// using blcon to check whenver an encounter is found
var blcon = "gml_Object_obj_battleblcon_Create_0";
var blconAlarm = "gml_Object_obj_battleblcon_Alarm_0";

// for knowing the naming screen before the run start
var naming = "gml_Script_scr_namingscreen";

// for the YOU WON screen
var battlecontrol = "gml_Object_obj_battlecontroller_Step_0";

// froggit enemy step
var froggitStep = "gml_Object_obj_froggit_Step_0";

// froggit enemy alarm used for deciding the attacks
var froggitAlarm = "gml_Object_obj_froggit_Alarm_6";

// code for the room transition doors being touched

var doorA = "gml_Object_obj_doorA_Other_19";
var doorC = "gml_Object_obj_doorC_Other_19";

// make drawing work
Data.GameObjects.ByName("obj_time").Visible = true;

// initializing
append(create, $@"
session_name = 0;

stage = 0;
current_msg = '';

is_timer_running = 0;
time_start = 0;
time_end = 0;
segment_name = '';


// mode is for when downtime is being watched
// running is for when downtime is being watched and a downtime has been reached
is_downtime_mode = 0;
downtime_name = '';
is_downtime_mode = 0;
is_downtime_running = 0;
downtime_start = 0;
downtime = 0;
step_count = 0;
previous_time = 0;

prevprev_room = 0;
previous_room = 0;
current_room = 0;

// see in step for explanation on tp
tp_flag = 0;
tp_x = 0;
tp_y = 0;
lock_player = 0;

//randomize call is in gamestart, which only runs after obj_time
randomize();

// finding the order of encounters

// ruins first half must have 6 encounters, 4 of them are froggits, 1 is a whimsun, the other is random
// gml 1 array so must initialize with highest index
first_half_encounters[5] = 0;

// lv 2 froggit must come before lv 3 one, thus it cant be last one
// aditionally, I want to LV up in a frogskip or a not frogskip because then the LV up slowdown won't in the LV 2 time
var lv2_index = irandom(3);

// lv 3 must be at least after LV up so minimum position is 2 after LV 2 one
var lv3_index = irandom(3 - lv2_index) + lv2_index + 2;

first_half_encounters[lv2_index] = '2';
first_half_encounters[lv3_index] = '3';

// leave an empty encounter for the LV up so that LV up itself can be measured 
first_half_encounters[lv2_index + 1] = 'A';

{assignNonTakenIndex("first_half_encounters", "irandom(2)", "'W'")}
{assignNonTakenIndex("first_half_encounters", "irandom(1)", "'F'")}
{assignNonTakenIndex("first_half_encounters", "0", "'N'")}

// ruins first half encounters array guide:
// W: whimsun
// 2: Froggit at LV2
// 3: Froggit at LV3
// F: Froggit with frogskip
// N: Froggit without frogskip
// A: any (random encounter)

second_half_encounters[9] = 0;
{assignNonTakenIndex("second_half_encounters", "irandom(4)", "'W'")}
{assignNonTakenIndex("second_half_encounters", "irandom(3)", "'F'")} 
{assignNonTakenIndex("second_half_encounters", "irandom(2)", "'A'")} 
{assignNonTakenIndex("second_half_encounters", "irandom(1)", "'B'")} 
{assignNonTakenIndex("second_half_encounters", "0", "'C'")}
{assignNonTakenIndex("second_half_encounters", "irandom(3) + 5", "'W'")} 
{assignNonTakenIndex("second_half_encounters", "irandom(2) + 5", "'F'")} 
{assignNonTakenIndex("second_half_encounters", "irandom(1) + 5", "'B'")} 
{assignNonTakenIndex("second_half_encounters", "5", "'C'")}
"
// ruins second half encounters array guide:
// W: frog whim (2 times)
// F: 2x frog (2 times)
// A: 1x mold (1 time)
// B: 2x mold (2 times)
// C: 3x mold (3 times)
);

// room tracker; it is useful for some segments that are room based
append(step, @"
prevprev_room = previous_room;
previous_room = current_room;
current_room = room;
");

// in order to tp to the proper places, will be using the tp flag which notifies the
// next frame that a room teleportation was carried out last frame
// and we have a specific x,y position to go to
append(step, @$"
// use two flags to wait a frame
// wait a frame to overwrite the default position
if (tp_flag) {{
    tp_flag = 0;
    // to avoid the player moving to places they don't want to
    // player will be locked until they stop moving after teleporting
    if ({isMoving}) {{
        lock_player = 1;
        global.interact = 1;
    }}
    obj_mainchara.x = tp_x;
    obj_mainchara.y = tp_y;
    // previous x and y must be updated too due to how obj_mainchara's collision events work
    obj_mainchara.xprevious = tp_x;
    obj_mainchara.yprevious = tp_y;
}} else if (lock_player && !({isMoving})) {{
    lock_player = 0;
    global.interact = 0;
}}
");

// downtime timer api
append(step, @"
// will likely need to tweak this slightly depending on the order of things
// downtime begins whenever not progressing step count OR stepcount has gone over the optimal number
// since downtime uses global.encounter, which is reset by encounters, it is not designed to work while encounters are on
if (is_downtime_mode) {
    if (is_downtime_running) {
        // being greater means it has incremented
        // but there is a frame delay transmitting the message
        // and we want to stop the downtime one frame before the movement actually happened (when the movement was granted)
        if (global.encounter > step_count) {
            downtime += prevprev_time - downtime_start;
            is_downtime_running = 0;
        }
    } else {
        // being equals means global.encounter did not increment, thus downtime has begun
        // but there is a frame delay transmitting the message
        if (global.encounter == step_count || step_count > optimal_steps) {
            downtime_start = previous_time;
            is_downtime_running = 1;
        }
    }
}
prevprev_time = previous_time;
previous_time = get_timer();
step_count = global.encounter;
");

// message drawer
append(draw, @"
draw_set_font(fnt_main)
draw_set_color(c_yellow);
draw_text(20, 20, current_msg);
");

// STEP CHECKS (every frame)

// message check
append(step, @$"
if (stage == 0) {{
    current_msg = 'RECORDING SESSION WAITING TO START
To start it, begin a normal run,
and keep playing until the mod stops you';
}} else if (stage == 3) {{
    current_msg = 'Next, walk through the next room
as quickly as possible';
}} else if (stage == 6) {{
    current_msg = 'Now, go through the next room
and proceed as if you were in a normal
run until the mod stops you';
}} else if (stage == 14) {{
    current_msg = ""Now, walk to the right room
and simply cross it (don't grind)
"";
}} else if (stage == 16) {{
    current_msg = 'Now, grind an encounter at
the end of this room and proceed as if it
were a normal run until you are stopped
';
}} else if (stage == 19) {{
    current_msg = 'Now, go through the next room
from beginning to end as if it was a
normal run but without grinding an encounter
at the end
';
}} else if (stage == 21) {{
    current_msg = 'Now grind at the
end of the room, and proceed as a normal
run until you are stopped
';
}} else if (stage == 24) {{
    current_msg = 'Now, go through the next
room from begining to end as if it was
a normal run but without grinding an encounter
at the end
';
}} else if (stage == 26) {{
    current_msg = 'Now, grind an
encounter at the end of the room,
and proceed grinding and killing
encounters until you are stopped
Grind as you would in a normal
run
';
}} else if (stage == 32) {{
    current_msg = 'Now, continue grinding
just the same, but as if you had 19 kills,
that is, flee after killing the
first enemy for ALL encounters
';
}} else if (stage == 37) {{
    current_msg = 'Finally, kill one last encounter
it will be a triple mold, and you must only
kill TWO monsters, then flee
Feel free to still attack second one
to simulate the Froggit Whimsun attacks
';
}} else if (stage == 39) {{
    current_msg = 'Finally, walk to the right as if
you have finished killing all
monsters and play normally until
the end of Ruins
';
}} else {{
    current_msg = '';
}}
");

// ROOM TRANSITIONS
append(step, @$"
// from ruins hallway to leafpile
if (previous_room == 11 && current_room == 12) {{
    if (stage == 2) {{
        {newStage(3)}
        {tpRuinsHallway}
        // crank up kill count just high enough so it will not give an encounter in the next room
        global.flag[202] = 4;
    }} else if (stage == 3) {{
        {startDowntime("ruins-leafpile", 97, 4)}
    }} else if (stage == 6) {{
        {newStage(7)}
    }}
// exitting leafpile room
}} else if (previous_room == 12 && current_room == 14) {{
    // ending leafpile transition
    if (stage == 8) {{
        {stopTime}
    // starting leaf fall downtime 
    }} else if (stage == 14) {{
        // arbitrarily high steps because it doesn't matter
        {startDowntime("ruins-leaf-fall", 1000, 15)}
    }}
// ruins first half transition
}} else if (previous_room == 14 && current_room == 12 && stage == 9) {{
    {stopTime}
// exit leaf fall room
}} else if (previous_room == 14 && current_room == 15) {{
    // end leaf fall transition
    if (stage == 18) {{
        {stopTime}
        stage = 19;
        {tpTo(14, 200, 80)}
    // start one rock downtime
    }} else if (stage == 19) {{
        {startDowntime("ruins-one-rock", 10000, 20)}
    }}
// ending second half grind
}} else if (previous_room == 17 && current_room == 18 && stage == 39) {{
    {startSegment("ruins-napsta", 40)}
    
// exit leaf maze
}} else if (previous_room == 16 && current_room == 17) {{
    // end leaf maze segment
    if (stage == 23) {{
        {stopTime}
        stage = 24;
        {tpTo(16, 520, 220)}
    // start three rock downtime
    }} else if (stage == 24) {{
        {startDowntime("ruins-three-rock", 10000, 25)}
    }}
// exit ruins (end ruins segment)
}} else if (previous_room == 41 && current_room == 42 && stage == 46) {{
    {stopTime}
// entering battle
}} else if (previous_room != room_battle && current_room == room_battle) {{
    // first half segments for encounters
    if ({isFirstHalfStage}) {{ 
        {firstHalfCurrentEncounter}
        name = 0;
        if (current_encounter == '2') {{
            name = 'froggit-lv2';
        }} else if (current_encounter == '3') {{
            name = 'froggit-lv3';
        }} else if (current_encounter == 'W') {{
            name = 'whim';
        }}
        //filtering out frogskip related ones
        if (name != 0) {{
            {startSegment("name", -1, true)}
        }}
    }} else if ({isSecondHalfNoFleeStage}) {{
        {secondHalfNoFleeEncounter}
        var name = 0;
        if (current_encounter == 'W') {{
            name = 'frog-whim';
        }} else if (current_encounter == 'F') {{
            name = 'dbl-frog';
        }} else if (current_encounter == 'A') {{
            name = 'sgl-mold';
        }} else if (current_encounter == 'B') {{
            name = 'dbl-mold';
        }} else if (current_encounter == 'C') {{
            name = 'tpl-mold';
        }}
        {startSegment("name", -1, true)}
    }} else if ({isSecondHalfFleeStage}) {{
        {secondHalfFleeEncounter}
        var name = 0;
        if (current_encounter == 'W') {{
            name = 'frog-whim-19';
        }} else if (current_encounter == 'F') {{
            name = 'dbl-frog-19';
        }} else if (current_encounter == 'A') {{
            name = 'sgl-mold-19';
        }} else if (current_encounter == 'B') {{
            name = 'dbl-mold-19';
        }} else if (current_encounter == 'C') {{
            name = 'tpl-mold-19';
        }}
        {startSegment("name", - 1, true)}
    }} else if (stage == 38) {{
        {startSegment("tpl-mold-18")}
    }}
// exitting out of a battle
}} else if (previous_room == room_battle && current_room != room_battle) {{
    // exitting too early requires manually setting off persistence
    room_persistent = false;
    if ({isFirstHalfStage}) {{
        {firstHalfCurrentEncounter}
        // leave the player high enough XP for guaranteed LV up next encounter if just fought the LV 2 encounter
        if (current_encounter == '2') {{
            global.xp = 29;
        }}
        // 'F' is the only encounter that by its end we don't have a timer happening
        // for 2, 3, W we have clearly the encounter timer and for A and N we measure the 'you won' text
        // so it just leaves F for having no reason to stop time here
        if (current_encounter != 'F') {{ 
            {stopTime}
        }}
        // increment for BOTH the in turn and the whole battle segments 
        obj_time.stage++;
    }} else if (({isSecondHalfFleeStage})  || ({isSecondHalfNoFleeStage})) {{
        {stopTime}
        // last ones so we TP for explanation
        if (obj_time.stage == 31 || obj_time.stage == 36) {{
            {tpTo(18, 40, 110)}
            global.flag[202] = 0;
        }}
        obj_time.stage++;
    }} else if (stage == 38) {{
        {stopTime}
        // TP back for final explanation
        stage = 39;
        {tpTo(17, 500, 110)}
        global.flag[202] = 20;
    }}
}}
");

// starting a segment post a battle
append(step, @$"
if (prevprev_room == room_battle && previous_room != room_battle) {{
    if (stage == 8) {{
        // stage 8 (first one) means we have the transition from the leafpile to the right
        {startSegment("ruins-leafpile-transition")}
    }} else if (stage == 9) {{
        // this one is for any given first half grind transition (but measured in the second one)  
        {startSegment("ruins-first-transition")}
    }} else if (stage == 17) {{
        // transition from the end of the encounter in room 14
        {startSegment("leaf-fall-transition", 18)}
    }} else if (stage == 22) {{
        // leaf maze segment
        {startSegment("ruins-maze", 23)}
    }}
}}
");

// naming screen time start
replace(naming, "naming = 4", @$" {{
    naming = 4;
    {startSegment("ruins-start", 1)}
    {startSession}
}}
");

// encountering first froggit in ruins
// battlegroup 3 is first froggit

// everything at the start the start of blcon; end of ruins start
append(blcon, @$"
if (global.battlegroup == 3 || obj_time.stage >= 40) {{
    // end of ruins start or reached nobody came in ruins
    {stopTime}
}} else if (
    obj_time.stage == 16 ||
    obj_time.stage == 21 ||
    obj_time.stage == 26 ||
    obj_time.stage == 32 ||
    obj_time.stage == 37
) {{ // stages used just to remove the message
    obj_time.stage++;
}}");

// everything at the end of the blcon

place(blconAlarm, "battle = 1", @$"
// end of ruins-hallway
if (global.battlegroup == 3) {{
    {startSegment("ruins-hallway", 2)}
}} else if (obj_time.stage == 40) {{
    {startSegment("ruins-switches", 41)}
}} else if (obj_time.stage == 41) {{
    {startSegment("perspective-a", 42)}
}} else if (obj_time.stage == 42) {{
    {startSegment("perspective-b", 43)}
}} else if (obj_time.stage == 43) {{
    {startSegment("perspective-c", 44)}
}} else if (obj_time.stage == 44) {{
    {startSegment("perspective-d", 45)}
}} else if (obj_time.stage == 45) {{
    {startSegment("ruins-end", 46)}
}}

// rigging encounters
if ({isFirstHalfStage}) {{ 
    {firstHalfCurrentEncounter}
    // only 'A' is not rigged
    if (current_encounter != 'A') {{
        // default to froggit, since it's the most probable
        var to_battle = 4;
        if (current_encounter == 'W') {{
            // whimsun battlegroup
            to_battle = 5;
        }}
        global.battlegroup = to_battle;
    }}
}} else if (obj_time.stage == 17 || obj_time.stage == 22) {{ // rigging to whimsun just to speed things up
    global.battlegroup = 5;
}} else if (({isSecondHalfFleeStage}) || ({isSecondHalfNoFleeStage})) {{
    var current_encounter;
    if ({isSecondHalfNoFleeStage}) {{
        current_encounter = obj_time.second_half_encounters[obj_time.stage - 27];
    }} else {{
        current_encounter = obj_time.second_half_encounters[obj_time.stage - 28];
    }}
    if (current_encounter == 'W') {{
        global.battlegroup = 6;
    }} else if (current_encounter == 'F') {{
        global.battlegroup = 9;
    }} else if (current_encounter == 'A') {{
        global.battlegroup = 7;
    }} else if (current_encounter == 'B') {{
        global.battlegroup = 10;
    }} else if (current_encounter == 'C') {{
        global.battlegroup = 8;
    }}
}} else if (obj_time.stage == 38) {{
    global.battlegroup = 8;
}}
");

// DOOR C ACCESS
append(doorC, @$"
// stop leafpile downtime
if (obj_time.stage == 4 && room == 12) {{
    {stopDowntime}
    {newStage(5)}
}}
");

// DOOR A ACCESS
append(doorA, @$"
if (obj_time.stage == 15 && room == 14) {{
    // end leaf fall downtime
    {stopDowntime}
    {newStage(16)}
}} else if (obj_time.stage == 20 && room == 15) {{
    // end one rock downtime
    {stopDowntime}
    {newStage(21)}
}} else if (obj_time.stage == 25 && room == 17) {{
    // end three rock downtime
    {stopDowntime}
    {newStage(26)}
}}
");

// teleporting at the end of downtimes
append(step, @$"
if (stage == 5 && room > 12) {{ 
    {tpRuinsHallway}
    {newStage(6)}
}} else if (stage == 16 && room == 15) {{
    {tpTo(14, 210, 100)}
}} else if (stage == 21 && room == 16) {{
    {tpTo(15, 340, 100)}
}} else if (stage == 26 && room == 18) {{
    {tpTo(17, 430, 110)}
    // reset kills to make things quick
    global.flag[202] = 0;
}}
");

// rigging attacks for froggit
// it is placed right after mycommand declaration
place(froggitAlarm, "0))", @$"
if ({isFirstHalfStage}) {{
    {firstHalfCurrentEncounter}
    if (current_encounter == 'N') {{
        mycommand = 100;
    }} else {{ // as a means to speed up practice, all of them will have frog skip by default
        mycommand = 0;
    }}
}}
");

// start the first half segments for the froggit attacks
place(froggitStep, "if (global.mnfight == 2)\n{", @$"
if ({isFirstHalfStage}) {{
    {firstHalfCurrentEncounter}
    var name = 0;
    switch (current_encounter) {{
        case 'F':
            name = 'frogskip';
            break;
        case 'N':
            name = 'not-frogskip';
            break;
    }}
    if (name != 0) {{
        {startSegment("name", -1, true)}
    }}
}}
");

// end first half segments for froggit attacks
place(froggitStep, "attacked = 0", @$" {{
    attacked = 0;
    if ({isFirstHalfStage}) {{
        {firstHalfCurrentEncounter}
        if (current_encounter == 'F' || current_encounter == 'N') {{
            {stopTime}
        }}
    }}
}}
");

// start count for the "YOU WON!" text
place(battlecontrol, "earned \"", @$"
    if ({isFirstHalfStage}) {{
        {firstHalfCurrentEncounter}
        // for 'A', we are starting time for the LV up text
        if (current_encounter == 'A') {{
            {startSegment("lv-up")}
        }} else if (current_encounter == 'N') {{
            // 'N' will be the reserved item for measuring the normal you won text ('F' could be as well, just a choice)
            {startSegment("you-won")}
        }}
    }}
");

// get out of first half (beginning interlude stage)
append(step, @$"
if (stage == 13 && room != room_battle) {{
    stage = 14;
    {leafpileTp}
}}
");

// debug function
void useDebug () {
    // updating it every frame is just a lazy way of doing it since it can't be done in obj_time's create event
    // since it gets overwritten by gamestart
    append(step, "global.debug = 1;");

    // stage skip keybinds
    // Q skips to stage 6
    append(step, @$"
    if (keyboard_check_pressed(ord('Q'))) {{
        is_timer_running = 0;
        stage = 6;
        global.xp = 10;
        global.flag[202] = 4;
        script_execute(scr_levelup);
        global.plot = 9.5;
        {tpRuinsHallway}
    }}
    ");

    // E skips to stage 14
    append(step, @$"
    if (keyboard_check_pressed(ord('E'))) {{
        is_timer_running = 0;
        stage = 14;
        global.xp = 30;
        global.flag[202] = 11;
        script_execute(scr_levelup);
        global.plot = 9.5;
        {leafpileTp}
    }}
    ");

    string[] watchVars = {
        "is_timer_running",
        "obj_time.stage",
        "is_downtime_mode",
        "is_downtime_running",
        "tp_flag",
        "tp_x",
        "tp_y",
        "global.interact",
        "global.entrance"
    };

    // start just with line break just to not interefere with anything
    string code = @"
    ";
    int i = 0;
    foreach (string watchVar in watchVars) {
        code += $"draw_text(20, {110 + i * 25}, '{watchVar}: ' + string({watchVar}));";
        i++;
    }
    append(draw, code);

    // coordinates
    append(draw, @$"
    if (instance_exists(obj_mainchara)) {{
        draw_text(20, {(110 + i * 25)}, 'x:' +  string(obj_mainchara.x));
        draw_text(20, {(110 + (i + 1) * 25)}, 'y:' + string(obj_mainchara.y));
        draw_text(20, {(110 + (i + 2) * 25)}, 'xprevious:' +  string(obj_mainchara.x));
        draw_text(20, {(110 + (i + 3) * 25)}, 'yprevious:' + string(obj_mainchara.y));
    }}
    ");
}

// debug mode - REMOVE FOR BUILD
useDebug();

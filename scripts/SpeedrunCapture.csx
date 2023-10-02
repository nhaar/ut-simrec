/******
helper variables and functions
******/

// code entries

var blcon = "gml_Object_obj_battleblcon_Create_0";

var blconAlarm = "gml_Object_obj_battleblcon_Alarm_0";

// code for picking how many steps are needed for an encounter
var scrSteps = "gml_Script_scr_steps";

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
var doorAmusic = "gml_Object_obj_doorAmusicfade_Other_19";
var doorC = "gml_Object_obj_doorC_Other_19";

/// <summary>
/// Append GML to the end of a code entry
/// </summary>
/// <param name="codeName">Name of the code entry</param>
/// <param name="code">Code to append</param>
void append (string codeName, string code) {
    Data.Code.ByName(codeName).AppendGML(code, Data);
}

/// <summary>
/// Replace text in a code entry
/// </summary>
/// <param name="codeName">Name of the code entry</param>
/// <param name="text">Exact text to be replaced</param>
/// <param name="replacement">Text to overwrite the old text</param>
void replace (string codeName, string text, string replacement) {
    ReplaceTextInGML(codeName, text, replacement);
}

/// <summary>
/// Place text inside a code entry
/// </summary>
/// <param name="codeName">Name of the code entry</param>
/// <param name="preceding">String matching the exact text that precedes where the code should be placed</param>
/// <param name="placement">New code to place</param>
void place (string codeName, string preceding, string placement) {
    ReplaceTextInGML(codeName, preceding, $"{preceding}{placement}");
}

void placeInIf (string codeName, string preceding, string placement) {
    ReplaceTextInGML(codeName, preceding, @$"
    {{
        {preceding};
        {placement}
    }}
    ");
}

/// <summary>
/// Generate GML code that (assuming it exists) finds the first index in an array that contains only `0`
/// and assigns it a value
/// </summary>
/// <param name="arr">Name of the array variable in GML</param>
/// <param name="index">The first index to start searching, it will go up from here</param>
/// <param name="value">Value to assign to the found spot</param>
/// <remarks>
/// If it incorrectly assumes there is a free index then it will crash the game with an out of range error
/// </remarks>
/// <returns></returns>
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

/// <summary>
/// Generate GMl code that appends the time for a segment/downtime to the end of the current session's file
/// </summary>
/// <param name="name">Name for the segment/downtime</param>
/// <param name="time">String containing the total time obtained (in microseconds)</param>
/// <returns></returns>
string appendNewTime (string name, string time) {
    return @$"
    var file = file_text_open_append('recordings/recording_' + string(obj_time.session_name));
    file_text_write_string(file, {name} + '=' + string({time}) + ';');
    file_text_close(file);
    ";
}

/// <summary>
/// GML code that starts a new recording session
/// </summary>
var startSession = @"
obj_time.session_name = string(current_year) + string(current_month) + string(current_day) + string(current_hour) + string(current_minute) + string(current_second);
var file = file_text_open_write('recordings/recording_' + string(obj_time.session_name));
file_text_close(file);
";

/// <summary>
/// Generate GML code that starts the segment timer
/// </summary>
/// <param name="name">Name of the segment</param>
/// <param name="stage">Stage to update to, or `-1` to not update the stage</param>
/// <param name="isVarName">
/// Should be set to `true` if the `name` param should be interpreted as being a GML variable name
/// and to `false` if it should be interpreted as being a GML string literal
/// </param>
/// <returns></returns>
string startSegment (string segmentName, bool isVarName = false) {
    string nameString = isVarName ? segmentName : $"'{segmentName}'";
    return @$"
    if (!obj_time.is_timer_running) {{
        obj_time.is_timer_running = 1;
        obj_time.time_start = get_timer();
        obj_time.segment_name = {nameString};
    }}
    ";
}

var next = @"
obj_time.stage++;
";

/// <summary>
/// Generate GML code that starts the downtime mode
/// </summary>
/// <param name="name">Name of the downtime</param>
/// <param name="steps">Number of optimal steps to complete downtime</param>
/// <param name="stage">Value to set the stage to, or `-1` if the stage should not be changed</param>
/// <returns></returns>
string startDowntime (string downtimeName, int steps) {
    return @$"
    if (!obj_time.is_downtime_mode) {{
        obj_time.is_downtime_mode = 1;
        obj_time.downtime = 0;
        obj_time.downtime_start = 0;
        obj_time.step_count = global.encounter;
        obj_time.optimal_steps = {steps}
        obj_time.downtime_name = '{downtimeName}';
    }}
    ";
}

/// <summary>
/// GML code that stops the segment timer
/// </summary>
var stopTime = @$"
if (obj_time.is_timer_running) {{
    obj_time.is_timer_running = 0;
    {appendNewTime("obj_time.segment_name", "get_timer() - obj_time.time_start")}
}}
";

/// <summary>
/// GML code that stops downtime mode
/// </summary>
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

/// <summary>
/// Number of encounters in the first half array
/// </summary>
var firstHalfLength = 6;

/// <summary>
/// GML code that assigns a `var` `encounter_name` the value of the current first half encounter
/// </summary>
string firstHalfCurrentEncounter = "var encounter_name = obj_time.first_half_encounters[obj_time.current_encounter];";

string firstHalfTransition (int encounter) {
    return @$"
    {firstHalfCurrentEncounter}
    if (current_encounter == {encounter}) {{
        {stopTime}
    }}
    ";
}

/// <summary>
/// GML code that disables the encounters
/// </summary>
string disableEncounters = @"
obj_time.fast_encounters = 0;
";

var firstHalfEncounter = $@"
{firstHalfCurrentEncounter}
name = 0;
if (encounter_name == '2') {{
    name = 'froggit-lv2';
}} else if (encounter_name == '3') {{
    name = 'froggit-lv3';
}} else if (encounter_name == 'W') {{
    name = 'whim';
}}
//filtering out frogskip related ones
if (name != 0) {{
    {startSegment("name", true)}
}}
";

var firstHalfVictory = $@"
{firstHalfCurrentEncounter}
// leave the player high enough XP for guaranteed LV up next encounter if just fought the LV 2 encounter
if (encounter_name == '2') {{
    global.xp = 29;
}}
// 'F' is the only encounter that by its end we don't have a timer happening
// for 2, 3, W we have clearly the encounter timer and for A and N we measure the 'you won' text
// so it just leaves F for having no reason to stop time here
if (encounter_name != 'F') {{ 
    {stopTime}
}}
// increment for BOTH the in turn and the whole battle segments 
obj_time.current_encounter++;
show_debug_message(obj_time.current_encounter)
if (obj_time.current_encounter == {firstHalfLength}) {{
    {disableEncounters}
    obj_time.stage++;
}}

// the first one means we have the transition from the leafpile to the right
if (current_encounter == 1) {{
    {startSegment("ruins-leafpile-transition")}
// this one is for any given first half grind transition (but measured in the second one)  
}} else if (current_encounter == 2) {{
    {startSegment("ruins-first-transition")}
}}
";



/// <summary>
/// Length of the second half encounters array up to elements in the without flee grind
/// </summary>
// currently it is just equal to number of unique encounters in the second half
var noFleeLength = 5;

/// <summary>
/// Length of the second half encounters array excluding the last element
/// </summary>
// because with fleeing is just one encounter less
var secondHalfLength = noFleeLength * 2 - 1;

/// <summary>
/// GML code that assigns a `var` `encounter_name` the value of the current second half encounter
/// </summary>
var secondHalfCurrentEncounter = "var encounter_name = obj_time.second_half_encounters[obj_time.current_encounter];";

/// <summary>
/// Generate GML code that teleports the player to a room and in a given position inside the room
/// </summary>
/// <param name="room">The room ID</param>
/// <param name="x">x position to teleport to</param>
/// <param name="y">y position to telport to</param>
/// <returns></returns>
string tpTo (int room, int x, int y) {
    return tpTo(room.ToString(), x.ToString(), y.ToString());
}

/// <summary>
/// Generate GML code that teleports the player to a room and in a given position inside the room
/// </summary>
/// <param name="room">The room ID as a string or the room name</param>
/// <param name="x">x position to teleport to</param>
/// <param name="y">y position to telport to</param>
/// <returns></returns>
string tpTo (string room, int x, int y) {
    return tpTo(room, x.ToString(), y.ToString());
}


string tpTo (string room, string x, string y) {
    return @$"
    obj_time.tp_flag = 1;
    room = {room};
    obj_time.tp_x = {x};
    obj_time.tp_y = {y};
    ";
}

/// <summary>
/// GML variable that is `1` if any of the arrow keys are currently held or `0` otherwise
/// </summary>
string isMoving = @"
keyboard_check(vk_left) || keyboard_check(vk_right) || keyboard_check(vk_up) || keyboard_check(vk_down)
";

/// <summary>
/// GML code that teleports to the end of the leaf pile room
/// </summary>
string leafpileTp = tpTo(12, 240, 340);

/// <summary>
/// GML code that teleports to the end of the ruins long hallway
/// </summary>
string tpRuinsHallway = tpTo(11, 2400, 80);



/// <summary>
/// GML code that enables the encounters
/// </summary>
string enableEncounters = @"
obj_time.fast_encounters = 1;
";


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


// make drawing work
Data.GameObjects.ByName("obj_time").Visible = true;

// initializing
append(create, $@"
// where recording text files will be saved
directory_create('recordings');

session_name = 0;

stage = 0;
current_msg = '';

is_timer_running = 0;
time_start = 0;
time_end = 0;
segment_name = '';

// this flag will controll how encounters are given
// if `0`, then encounters will be disabled
// else if `1` then encounters will be given quickly
fast_encounters = 0;

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

previous_room = 0;
current_room = 0;

// counter variable for the end segment
nobody_came = 0;

// see in step for explanation on tp
tp_flag = 0;
tp_x = 0;
tp_y = 0;
lock_player = 0;

//randomize call is in gamestart, which only runs after obj_time
randomize();

// save the index for the current encounter, is used during the grinds
// that access the encounter arrays bellow
current_encounter = 0;

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

// edit step counts
replace(scrSteps, @"
    populationfactor = (argument2 / (argument2 - global.flag[argument3]))
    if (populationfactor > 8)
        populationfactor = 8
    steps = ((argument0 + round(random(argument1))) * populationfactor)
", $@"
if (obj_time.fast_encounters) {{
    // if we want fast encounters, we are probably killing things, so setting kills to 0 is
    // a control method to not go over the limit which is manually set always 
    global.flag[argument3] = 0;
    // max hp for the user convenience due to unusual amount of encounters and frog skips
    global.hp = global.maxhp;
    steps = argument0;
}} else {{
    // practically disabling encounters with an arbitrarily high number since GMS1 does not have infinity
    // this would be ~5 minutes of walking
    steps = 10000;
}}
");

// room tracker; it is useful for some segments that are room based
append(step, @"
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
        // but it also means that last frame was the first one incrementing, thus use previous time
        if (global.encounter > step_count) {
            downtime += previous_time - downtime_start;
            is_downtime_running = 0;
        }
    } else {
        // being equals means global.encounter did not increment, thus downtime has begun
        // but it also means that it stopped incrementing last frame, thus use previous_time
        // also, the frame it goes above optimal steps is when downtime begins (since we're using is previous_time it also
        // means we use step_count instead of global.encounter)
        if (global.encounter == step_count || step_count > optimal_steps) {
            downtime_start = previous_time;
            is_downtime_running = 1;
        }
    }
}
previous_time = get_timer();
step_count = global.encounter;
");

// message drawer
append(draw, @"
draw_set_font(fnt_main)
draw_set_color(c_yellow);
draw_text(20, 0, current_msg);
");

// rigging attacks for froggit
// it is placed right after mycommand declaration
place(froggitAlarm, "0))", @$"
// as a means to speed up practice, all of them will have frog skip by default
var use_frogskip = 1;
if (use_frogskip) {{
    mycommand = 0;
}} else {{
    mycommand = 100;
}}
");

// // STEP CHECKS (every frame)

// // helper dictionary that keeps all messages and is used to be iterated later
// Dictionary<int, string> messages = new Dictionary<int, string> {
//     { (int)Stages.Offline, @"
// RECORDING SESSION WAITING TO START
// To start it, begin a normal run,
// and keep playing until the mod stops you
//     " },
//     { (int)Stages.Start, @"
// PROCEED
//     " },
//     { (int)Stages.Hallway, @"
// PROCEED
//     " },
//     { (int)Stages.PreLeafPile, @"
// Next, walk through the next room
// as quickly as possible
//     " },
//     { (int)Stages.LeafPileDowntime, @"
// WALK
//     " },
//     { (int)Stages.PreFirstGrind, @"
// Now, grind and encounter at the end of
// the room and continue grinding as if you were
// in a normal run
//     " },
//     { (int)Stages.InFirstGrind, @"
// GRIND
//     " },
//     { (int)Stages.PostFirstGrind, @"
// Now, walk to the right room
// and simply cross it (don't grind)
//     " },
//     { (int)Stages.LeafFallDowntime, @"
// WALK
//     " },
//     { (int)Stages.PreFallEncounter, @"
// Now, grind an encounter at
// the end of this room and proceed as if it
// were a normal run until you are stopped
//     " },
//     { (int)Stages.InFallEncounter, @"
// PROCEED
//     " },
//     { (int)Stages.LeafFallTransition, @"
// PROCEED
//     " },
//     { (int)Stages.PreOneRock, @"
// Now, go through the next room
// from beginning to end as if it was a
// normal run but without grinding an encounter
// at the end
//     " },
//     { (int)Stages.OneRockDowntime, @"
// WALK
//     " },
//     { (int)Stages.PreLeafMaze, @"
// Now grind at the
// end of the room, and proceed as a normal
// run until you are stopped
//     " },
//     { (int)Stages.OneRockEncounter, @"
// PROCEED
//     " },
//     { (int)Stages.InLeafMaze, @"
// PROCEED
//     " },
//     { (int)Stages.PreThreeRock, @"
// Now, go through the next
// room from begining to end as if it was
// a normal run but without grinding an encounter
// at the end
//     " },
//     { (int)Stages.ThreeRockDowntime, @"
// WALK
//     " },
//     { (int)Stages.PreSecondGrind, @"
// Now, grind an
// encounter at the end of the room,
// and proceed grinding and killing
// encounters until you are stopped
// Grind as you would in a normal
// run
//     " },
//     { (int)Stages.InSecondGrind, @"
// GRIND
//     " },
//     { (int)Stages.PreFleeGrind, @"
// Now, continue grinding
// just the same, but as if you had 19 kills,
// that is, flee after killing the
// first enemy for ALL encounters
//     " },
//     { (int)Stages.InFleeGrind, @"
// GRIND (KILL ONLY ONE)
//     " },
//     { (int)Stages.PreTripleMold, @"
// Now, kill one last encounter
// it will be a triple mold, and you must only
// kill TWO monsters, then flee
// Feel free to still attack second one
// to simulate the Froggit Whimsun attacks
//     " },
//     { (int)Stages.InTripleMold, @"
// GRIND (KILL ONLY TWO)
//     " },
//     { (int)Stages.PreEnd, @"
// Finally, walk to the right as if
// you have finished killing all
// monsters and play normally until
// the end of Ruins
//     " },
//     { (int)Stages.NobodyCame, @"
// PROCEED
//     " },
//     { (int)Stages.End, @"
// PROCEED
//     " },
//     { (int)Stages.Finished, @"
// Session finished!
//     " }
// };

// // building the code that assigns the `current_msg` variable
// var currentMsgAssign = "";
// var msgTotal = Enum.GetNames(typeof(Stages)).Length;
// for (int i = 0; i < msgTotal; i++) {
//     var msg = messages[i];
//     if (i > 0) currentMsgAssign += "else";
//     currentMsgAssign += @$"
//     if (stage == {i}) {{
//         current_msg = ""{msg}"";
//     }}    
//     ";
// }
// // currently all stages have a message, so this has become obsolete
// currentMsgAssign += @$"
// else {{
//     current_msg = '';
// }}
// ";

// // message check
// append(step, currentMsgAssign);

// // ROOM TRANSITIONS
// append(step, @$"
// // from ruins hallway to leafpile
// if (previous_room == 11 && current_room == 12) {{
//     if (stage == {(int)Stages.Hallway}) {{
//         {stopTime}
//         {newStage((int)Stages.PreLeafPile)}
//         {tpRuinsHallway}
//     }} else if (stage == {(int)Stages.PreLeafPile}) {{
//         {startDowntime("ruins-leafpile", 97, (int)Stages.LeafPileDowntime)}
//     }}
// // exitting leafpile room
// }} else if (previous_room == 12 && current_room == 14) {{
//     // ending leafpile transition
//     if ({isFirstGrind} && current_encounter == 1) {{
//         {stopTime}
//     // starting leaf fall downtime 
//     }} else if (stage == {(int)Stages.PostFirstGrind}) {{
//         // arbitrarily high steps because it doesn't matter
//         {startDowntime("ruins-leaf-fall", 1000, (int)Stages.LeafFallDowntime)}
//     }}
// // ruins first half transition
// }} else if (previous_room == 14 && current_room == 12 && {isFirstGrind} && current_encounter == 2) {{
//     {stopTime}
// // exit leaf fall room
// }} else if (previous_room == 14 && current_room == 15) {{
//     // end leaf fall transition
//     if (stage == {(int)Stages.LeafFallTransition}) {{
//         {stopTime}
//         {disableEncounters}
//         stage = {(int)Stages.PreOneRock};
//         {tpTo(14, 200, 80)}
//     // start one rock downtime
//     }} else if (stage == {(int)Stages.PreOneRock}) {{
//         {startDowntime("ruins-one-rock", 10000, (int)Stages.OneRockDowntime)}
//     }}
// // ending second half transition
// }} else if (previous_room == 18 && current_room == 17 && {isSecondHalf}) {{
//     // stop the transition timer
//     if (current_encounter == 1 || current_encounter == 2) {{
//         {stopTime}
//     }}
// // exit leaf maze
// }} else if (previous_room == 16 && current_room == 17) {{
//     // end leaf maze segment
//     if (stage == {(int)Stages.InLeafMaze}) {{
//         {stopTime}
//         {disableEncounters}
//         stage = {(int)Stages.PreThreeRock};
//         {tpTo(16, 520, 220)}
//     // start three rock downtime
//     }} else if (stage == {(int)Stages.PreThreeRock}) {{
//         {startDowntime("ruins-three-rock", 10000, (int)Stages.ThreeRockDowntime)}
//     }}
// // entering battle
// }} else if (previous_room != room_battle && current_room == room_battle) {{
//     if (obj_time.stage == {(int)Stages.Start}) {{
//         {startSegment("ruins-hallway", (int)Stages.Hallway)}
//     // first half segments for encounters
//     }} else if ({isFirstGrind}) {{ 
//         {firstHalfCurrentEncounter}
//         name = 0;
//         if (encounter_name == '2') {{
//             name = 'froggit-lv2';
//         }} else if (encounter_name == '3') {{
//             name = 'froggit-lv3';
//         }} else if (encounter_name == 'W') {{
//             name = 'whim';
//         }}
//         //filtering out frogskip related ones
//         if (name != 0) {{
//             {startSegment("name", -1, true)}
//         }}
//     }} else if ({isSecondHalf}) {{
//         {secondHalfCurrentEncounter}
//         var name = 0;
//         if (encounter_name == 'W') {{
//             name = 'frog-whim';
//         }} else if (encounter_name == 'F') {{
//             name = 'dbl-frog';
//         }} else if (encounter_name == 'A') {{
//             name = 'sgl-mold';
//         }} else if (encounter_name == 'B') {{
//             name = 'dbl-mold';
//         }} else if (encounter_name == 'C') {{
//             name = 'tpl-mold';
//         }}
//         if ({isFleeGrind}) {{
//             name += '-19';
//         }}
//         {startSegment("name", - 1, true)}
//     }} else if (stage == {(int)Stages.InTripleMold}) {{
//         {startSegment("tpl-mold-18")}
//     }} else if (obj_time.stage == {(int)Stages.NobodyCame}) {{
//         if (obj_time.nobody_came == 0) {{
//             {startSegment("ruins-switches")}
//         }} else if (obj_time.nobody_came == 1) {{
//             {startSegment("perspective-a")}
//         }} else if (obj_time.nobody_came == 2) {{
//             {startSegment("perspective-b")}
//         }} else if (obj_time.nobody_came == 3) {{
//             {startSegment("perspective-c")}
//         }} else if (obj_time.nobody_came == 4) {{
//             {startSegment("perspective-d")}
//         }} else {{
//             {startSegment("ruins-end", (int)Stages.End)}
//         }}
//         obj_time.nobody_came++;
//     }}
// // exitting out of a battle
// }} else if (previous_room == room_battle && current_room != room_battle) {{
//     // exitting too early requires manually setting off persistence
//     room_persistent = false;
//     if ({isFirstGrind}) {{
//         {firstHalfCurrentEncounter}
//         // leave the player high enough XP for guaranteed LV up next encounter if just fought the LV 2 encounter
//         if (encounter_name == '2') {{
//             global.xp = 29;
//         }}
//         // 'F' is the only encounter that by its end we don't have a timer happening
//         // for 2, 3, W we have clearly the encounter timer and for A and N we measure the 'you won' text
//         // so it just leaves F for having no reason to stop time here
//         if (encounter_name != 'F') {{ 
//             {stopTime}
//         }}
//         // increment for BOTH the in turn and the whole battle segments 
//         obj_time.current_encounter++;
//         if (obj_time.current_encounter == {firstHalfLength}) {{
//             {disableEncounters}
//             {newStage((int)Stages.PostFirstGrind)}
//         }}

//         // the first one means we have the transition from the leafpile to the right
//         if (current_encounter == 1) {{
//             {startSegment("ruins-leafpile-transition")}
//         // this one is for any given first half grind transition (but measured in the second one)  
//         }} else if (current_encounter == 2) {{
//             {startSegment("ruins-first-transition")}
//         }}
//     }} else if (stage == {(int)Stages.PostFirstGrind}) {{
//         {leafpileTp}
//     }} else if (stage == {(int)Stages.InFallEncounter}) {{
//         // transition from the end of the encounter in room 14
//         {startSegment("leaf-fall-transition", (int)Stages.LeafFallTransition)}
//     }} else if (stage == {(int)Stages.OneRockEncounter}) {{
//         // leaf maze segment
//         {disableEncounters}
//         {startSegment("ruins-maze", (int)Stages.InLeafMaze)}
//     }} else if ({isSecondHalf}) {{
//         {stopTime}
//         // last ones so we TP for explanation
//         obj_time.current_encounter++;
//         if (obj_time.current_encounter == {noFleeLength} || obj_time.current_encounter == {secondHalfLength}) {{
//             {tpTo(18, 40, 110)}
//             if (obj_time.current_encounter == {noFleeLength}) {{
//                 {newStage((int)Stages.PreFleeGrind)}
//             }} else {{
//                 {newStage((int)Stages.PreTripleMold)}
//             }}
//         }}

//         // first one means we are coming from the incomplete transition from three rock
//         if (current_encounter == 1) {{
//             {startSegment("three-rock-transition")}
//         // second one for measuring the condition that happens for any given one
//         }} else if (current_encounter == 2) {{
//             {startSegment("ruins-second-transition")}
//         }}
//     }} else if (stage == {(int)Stages.InTripleMold}) {{
//         {stopTime}
//         // TP back for final explanation
//         stage = {(int)Stages.PreEnd};
//         {tpTo(17, 500, 110)}
//         // max out kills
//         global.flag[202] = 20;
//     }}
// }}
// ");

// // naming screen time start
// replace(naming, "naming = 4", @$" {{
//     naming = 4;
//     {startSegment("ruins-start", (int)Stages.Start)}
//     {startSession}
// }}
// ");

// // encountering first froggit in ruins
// // battlegroup 3 is first froggit

// // everything at the start the start of blcon; end of ruins start
// append(blcon, @$"
// if (obj_time.stage == {(int)Stages.Start} || obj_time.stage == {(int)Stages.NobodyCame}) {{
//     {stopTime}
// // all stages below are mostly used just to remove the message
// }} else if (obj_time.stage =={(int)Stages.PreFirstGrind}) {{
//     {newStage((int)Stages.InFirstGrind)}
// }} else if (obj_time.stage == {(int)Stages.PreFallEncounter}) {{
//     {newStage((int)Stages.InFallEncounter)}
// }} else if (obj_time.stage == {(int)Stages.PreLeafMaze}) {{
//     {newStage((int)Stages.OneRockEncounter)}
// }} else if (obj_time.stage == {(int)Stages.PreSecondGrind}) {{
//     obj_time.current_encounter = 0;
//     {newStage((int)Stages.InSecondGrind)}
// }} else if (obj_time.stage == {(int)Stages.PreFleeGrind}) {{
//     {newStage((int)Stages.InFleeGrind)}
// }} else if (obj_time.stage == {(int)Stages.PreTripleMold}) {{
//     {newStage((int)Stages.InTripleMold)};
// }}");

// // everything at the end of the blcon

// place(blconAlarm, "battle = 1", @$"
// // rigging encounters
// if ({isFirstGrind}) {{ 
//     {firstHalfCurrentEncounter}
//     // only 'A' is not rigged
//     if (encounter_name != 'A') {{
//         // default to froggit, since it's the most probable
//         var to_battle = 4;
//         if (encounter_name == 'W') {{
//             // whimsun battlegroup
//             to_battle = 5;
//         }}
//         global.battlegroup = to_battle;
//     }}
// // rigging to whimsun just to speed things up
// }} else if (obj_time.stage == {(int)Stages.InFallEncounter} || obj_time.stage == {(int)Stages.OneRockEncounter}) {{ 
//     global.battlegroup = 5;
// }} else if ({isSecondHalf}) {{
//     {secondHalfCurrentEncounter}
//     if (encounter_name == 'W') {{
//         global.battlegroup = 6;
//     }} else if (encounter_name == 'F') {{
//         global.battlegroup = 9;
//     }} else if (encounter_name == 'A') {{
//         global.battlegroup = 7;
//     }} else if (encounter_name == 'B') {{
//         global.battlegroup = 10;
//     }} else if (encounter_name == 'C') {{
//         global.battlegroup = 8;
//     }}
// }} else if (obj_time.stage == {(int)Stages.InTripleMold}) {{
//     global.battlegroup = 8;
// }}
// ");

// // DOOR C ACCESS
// append(doorC, @$"
// // stop leafpile downtime
// if (obj_time.stage == {(int)Stages.LeafPileDowntime} && room == 12) {{
//     {stopDowntime}
//     {enableEncounters}
//     {newStage((int)Stages.PreFirstGrind)}
// }}
// ");

// // DOOR A ACCESS
// append(doorA, @$"
// if (obj_time.stage == {(int)Stages.LeafFallDowntime} && room == 14) {{
//     // end leaf fall downtime
//     {stopDowntime}
//     {enableEncounters}
//     {newStage((int)Stages.PreFallEncounter)}
// }} else if (obj_time.stage == {(int)Stages.OneRockDowntime} && room == 15) {{
//     // end one rock downtime
//     {stopDowntime}
//     {enableEncounters}
//     {newStage((int)Stages.PreLeafMaze)}
// }} else if (obj_time.stage == {(int)Stages.ThreeRockDowntime} && room == 17) {{
//     // end three rock downtime
//     {stopDowntime}
//     {enableEncounters}
//     {newStage((int)Stages.PreSecondGrind)}
// // ending second half grind
// }} else if (obj_time.stage == {(int)Stages.PreEnd} && room == 17) {{
//     {startSegment("ruins-napsta", (int)Stages.NobodyCame)}
// // exit ruins (end ruins segment)
// }}
// ");

// // DOOR A MUSIC FADE ACCESS
// append(doorAmusic, @$"
// if (room == 41 && obj_time.stage == {(int)Stages.End}) {{
//     {stopTime}
//     {newStage((int)Stages.Finished)}
// }}
// ");

// // teleporting at the end of downtimes
// append(step, @$"
// if (stage == {(int)Stages.PreFirstGrind} && room == 14) {{ 
//     {tpTo(12, 180, 260)}
// }} else if (stage == {(int)Stages.PreFallEncounter} && room == 15) {{
//     {tpTo(14, 210, 100)}
// }} else if (stage == {(int)Stages.PreLeafMaze} && room == 16) {{
//     {tpTo(15, 340, 100)}
// }} else if (stage == {(int)Stages.PreSecondGrind} && room == 18) {{
//     {tpTo(17, 430, 110)}
// }}
// ");

// // rigging attacks for froggit
// // it is placed right after mycommand declaration
// place(froggitAlarm, "0))", @$"
// // as a means to speed up practice, all of them will have frog skip by default
// var use_frogskip = 1;
// // find the only case to not use frogskip (when we are measuring the speed of not having frogskip)
// if ({isFirstGrind}) {{
//     {firstHalfCurrentEncounter}
//     if (encounter_name == 'N') {{
//         use_frogskip = 0;
//     }}
// }}
// if (use_frogskip) {{
//     mycommand = 0;
// }} else {{
//     mycommand = 100;
// }}
// ");

// // start the first half segments for the froggit attacks
// place(froggitStep, "if (global.mnfight == 2)\n{", @$"
// if ({isFirstGrind}) {{
//     {firstHalfCurrentEncounter}
//     var name = 0;
//     switch (encounter_name) {{
//         case 'F':
//             name = 'frogskip';
//             break;
//         case 'N':
//             name = 'not-frogskip';
//             break;
//     }}
//     if (name != 0) {{
//         {startSegment("name", -1, true)}
//     }}
// }}
// ");

// // end first half segments for froggit attacks
// replace(froggitStep, "attacked = 0", @$" {{
//     attacked = 0;
//     if ({isFirstGrind}) {{
//         {firstHalfCurrentEncounter}
//         if (encounter_name == 'F' || encounter_name == 'N') {{
//             {stopTime}
//         }}
//     }}
// }}
// ");

// // start count for the "YOU WON!" text
// place(battlecontrol, "earned \"", @$"
//     if ({isFirstGrind}) {{
//         {firstHalfCurrentEncounter}
//         // for 'A', we are starting time for the LV up text
//         if (encounter_name == 'A') {{
//             {startSegment("lv-up")}
//         }} else if (encounter_name == 'N') {{
//             // 'N' will be the reserved item for measuring the normal you won text ('F' could be as well, just a choice)
//             {startSegment("you-won")}
//         }}
//     }}
// ");

/// <summary>
/// Function that if called will add debug functions to the game
/// </summary>
void useDebug () {
    // updating it every frame is just a lazy way of doing it since it can't be done in obj_time's create event
    // since it gets overwritten by gamestart
    append(step, "global.debug = 1;");

    // stage skip keybinds
    // Q skips to stage 6
    append(step, @$"
    if (keyboard_check_pressed(ord('Q'))) {{
        is_timer_running = 0;
        stage = 3;
        global.xp = 10;
        script_execute(scr_levelup);
        global.plot = 9;
        {tpRuinsHallway}
    }}
    ");

    // E skips to stage 14
    append(step, @$"
    if (keyboard_check_pressed(ord('E'))) {{
        is_timer_running = 0;
        obj_time.stage = 7;
        global.xp = 30;
        script_execute(scr_levelup);
        global.plot = 9.5;
        {leafpileTp}
    }}
    ");

    string[] watchVars = {
        "obj_time.stage",
        "is_timer_running",
        "segment_name",
        "is_downtime_mode",
        "is_downtime_running",
        "downtime_name",
        "global.encounter", 
        "step_count",
        "get_timer()",
        "previous_time"
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
    }}
    ");
}


/**
main script
**/


// so this is how the program works
// script -> defines everything
// break the script into stages
// break the stages into events
// group all listeners for each event by their stage
// place the code for the listeners inside the respective events

// IDEA
// have events be part of enum
// create event handler to handle each event
//

// EVENT CLASSES (not sure if I like this!!)

abstract class UndertaleEvent {
    public abstract string EventId ();
}

enum EventName {
    PickName,
    Blcon,
    BeforeBattle,
    EnterBattle,
    LeaveBattle,
    RoomTransition,
    Room,
    FroggitAttack,
    FroggitTurnStart,
    FroggitTurnEnd,
    YouWon,
    Door


}

class PickName : UndertaleEvent {
    public override string EventId () {
        return EventName.PickName.ToString();
    }
}

class Blcon : UndertaleEvent {
    public override string EventId () {
        return EventName.Blcon.ToString();
    }
}

class EnterBattle : UndertaleEvent {
    public int Battlegroup;
    
    public EnterBattle () {
        Battlegroup = -1;
    }

    public EnterBattle (int battlegroup) {
        Battlegroup = battlegroup;
    }

    public override string EventId () {
        string group = Battlegroup > -1 ? $",{Battlegroup.ToString()}" : "";
        return $"{EventName.EnterBattle}{group}";
    }
}

class RoomTransition : UndertaleEvent {
    public int PreviousRoom;

    public int CurrentRoom;

    public RoomTransition (int prev, int cur) {
        PreviousRoom = prev;
        CurrentRoom = cur;
    }

    public override string EventId () {
        return $"{EventName.RoomTransition},{PreviousRoom},{CurrentRoom}";
    }
}

class Room : UndertaleEvent {
    public int RoomId;

    public Room (int room) {
        RoomId = room;
    }

    public override string EventId () {
        return $"{EventName.Room},{RoomId}";
    }
}
class LeaveBattle : UndertaleEvent {
    public override string EventId () {
        return EventName.LeaveBattle.ToString();
    }
}

class BeforeBattle : UndertaleEvent {
    public override string EventId () {
        return EventName.BeforeBattle.ToString();
    }
}

class FroggitAttack : UndertaleEvent {
    public override string EventId () {
        return EventName.FroggitAttack.ToString();
    }
}

class FroggitTurnEnd : UndertaleEvent {
    public override string EventId () {
        return EventName.FroggitTurnEnd.ToString();
    }
}

class FroggitTurnStart : UndertaleEvent {
    public override string EventId () {
        return EventName.FroggitTurnStart.ToString();
    }
}

class YouWon : UndertaleEvent {
    public override string EventId () {
        return EventName.YouWon.ToString();
    }
}

class Door : UndertaleEvent {
    public string Name;

    public int Room;
    
    public Door (string name, int room) {
        Name = name;
        Room = room;
    }

    public override string EventId () {
        return $"{EventName.Door},{Name},{Room}";
    }
}


class Stage {
    public Listener[] Listeners;

    public Stage (params Listener[] listeners) {
        Listeners = listeners;
    }
}

class Session {
    public Stage[] Stages;

    public Session (params Stage[] stages) {
        Stages = stages;
    }
}


var rigFirstHalf = $@"
{firstHalfCurrentEncounter}
// only 'A' is not rigged
if (encounter_name != 'A') {{
    // default to froggit, since it's the most probable
    var to_battle = 4;
    if (encounter_name == 'W') {{
        // whimsun battlegroup
        to_battle = 5;
    }}
    global.battlegroup = to_battle;
}}
";

var unrigFrogskip = $@"
{firstHalfCurrentEncounter}
if (encounter_name == 'N') {{
    use_frogskip = 0;
}}
";

var timeFrogTurn = $@"
{firstHalfCurrentEncounter}
var name = 0;
switch (encounter_name) {{
    case 'F':
        name = 'frogskip';
        break;
    case 'N':
        name = 'not-frogskip';
        break;
}}
if (name != 0) {{
    {startSegment("name", true)}
}}
";

string endFrogTime = $@"
{firstHalfCurrentEncounter}
if (encounter_name == 'F' || encounter_name == 'N') {{
    {stopTime}
}}
";

var timeYouWon = @$"
{firstHalfCurrentEncounter}
// for 'A', we are starting time for the LV up text
if (encounter_name == 'A') {{
    {startSegment("lv-up")}
}} else if (encounter_name == 'N') {{
    // 'N' will be the reserved item for measuring the normal you won text ('F' could be as well, just a choice)
    {startSegment("you-won")}
}}
";

var rigWhimsun = @$"
global.battlegroup = 5;
";

var secondHalfTransition = @$"
if (current_encounter == 1 || current_encounter == 2) {{
    {stopTime}
}}
";


string secondHalfEncounter (bool isFleeGrind) {
    int gmlBool = isFleeGrind ? 1 : 0;
    return @$"
    {secondHalfCurrentEncounter}
    var name = 0;
    if (encounter_name == 'W') {{
        name = 'frog-whim';
    }} else if (encounter_name == 'F') {{
        name = 'dbl-frog';
    }} else if (encounter_name == 'A') {{
        name = 'sgl-mold';
    }} else if (encounter_name == 'B') {{
        name = 'dbl-mold';
    }} else if (encounter_name == 'C') {{
        name = 'tpl-mold';
    }}
    if ({gmlBool}) {{
        name += '-19';
    }}
    {startSegment("name", true)}
    ";
}



var secondHalfVictory = @$"
{stopTime}
// last ones so we TP for explanation
obj_time.current_encounter++;
if (obj_time.current_encounter == {noFleeLength} || obj_time.current_encounter == {secondHalfLength}) {{
    {tpTo(18, 40, 110)}
    obj_time.stage++;
}}

// first one means we are coming from the incomplete transition from three rock
if (current_encounter == 1) {{
    {startSegment("three-rock-transition")}
// second one for measuring the condition that happens for any given one
}} else if (current_encounter == 2) {{
    {startSegment("ruins-second-transition")}
}}
";

var secondHalfRig = @$"
{secondHalfCurrentEncounter}
if (encounter_name == 'W') {{
    global.battlegroup = 6;
}} else if (encounter_name == 'F') {{
    global.battlegroup = 9;
}} else if (encounter_name == 'A') {{
    global.battlegroup = 7;
}} else if (encounter_name == 'B') {{
    global.battlegroup = 10;
}} else if (encounter_name == 'C') {{
    global.battlegroup = 8;
}}
";

var maxRuinsKills = @$"
global.flag[202] = 20;
";

var rigTripleMold = @$"
global.battlegroup = 8;
";

var nobodyCameSegments = @$"
if (obj_time.nobody_came == 0) {{
    {startSegment("ruins-switches")}
}} else if (obj_time.nobody_came == 1) {{
    {startSegment("perspective-a")}
}} else if (obj_time.nobody_came == 2) {{
    {startSegment("perspective-b")}
}} else if (obj_time.nobody_came == 3) {{
    {startSegment("perspective-c")}
}} else if (obj_time.nobody_came == 4) {{
    {startSegment("perspective-d")}
}} else {{
    {startSegment("ruins-end")}
    obj_time.stage++;
}}
obj_time.nobody_came++;
";




var secondHalfSetup = @$"
obj_time.current_encounter = 0;
";

class Callback {
    public string GMLCode;

    public Callback (params string[] code) {
        GMLCode = "\n" + String.Join("\n", code) + "\n";
    }
}

class Listener {
    public UndertaleEvent Event;

    public Callback ListenerCallback; 

    public Listener (UndertaleEvent undertaleEvent, Callback callback) {
        Event = undertaleEvent;
        ListenerCallback = callback;
    }
}

var offline = new Stage(
    new Listener(
        new PickName(),
        new Callback(
            startSession,
            startSegment("ruins-start"),
            next
        )
    )
);

var ruinsStart = new Stage(
    new Listener(
        new Blcon(),
        new Callback(
            stopTime
        )
    ),
    new Listener(
        new EnterBattle(3),
        new Callback(
            startSegment("ruins-hallway"),
            next
        )
    )
);

var ruinsHallway = new Stage(
    new Listener(
        new RoomTransition(11, 12),
        new Callback(
            stopTime,
            next,
            tpTo(11, 2400, 80)
        )
    )
);

var preLeafPile = new Stage(
    new Listener(
        new RoomTransition(11, 12),
        new Callback(
            startDowntime("ruins-leafpile", 97),
            next
        )
    )
);

var leafPileDowntime = new Stage(
    new Listener(
        new Door("C", 12),
        new Callback(
            stopDowntime,
            enableEncounters,
            next
        )
    )
);

var preFirstGrind = new Stage(
    new Listener(
        new Room(14),
        new Callback(
            tpTo(12, 180, 260)
        )
    ),
    new Listener(
        new Blcon(),
        new Callback(
            next
        )
    )
);

var inFirstGrind = new Stage(
    new Listener(
        new RoomTransition(12, 14),
        new Callback(
            firstHalfTransition(1)
        )
    ),
    new Listener(
        new RoomTransition(14, 12),
        new Callback(
            firstHalfTransition(2)
        )
    ),
    new Listener(
        new EnterBattle(),
        new Callback(
            firstHalfEncounter
        )
    ),
    new Listener(
        new LeaveBattle(),
        new Callback(
            firstHalfVictory
        )
    ),
    new Listener(
        new BeforeBattle(),
        new Callback(
            rigFirstHalf
        )
    ),
    new Listener(
        new FroggitAttack(),
        new Callback(
            timeFrogTurn
        )
    ),
    new Listener(
        new FroggitTurnStart(),
        new Callback(
            unrigFrogskip,
            timeFrogTurn
        )
    ),
    new Listener(
        new FroggitTurnEnd(),
        new Callback(
            endFrogTime
        )
    ),
    new Listener(
        new YouWon(),
        new Callback(
            timeYouWon
        )
    )
);

var postFirstGrind = new Stage(
    new Listener(
        new LeaveBattle(),
        new Callback(
            tpTo(12, 240, 340)
        )
    ),
    new Listener(
        new RoomTransition(12, 14),
        new Callback(
            startDowntime("ruins-leaf-fall", 1000),
            next
        )
    )
);

var leafFallDowntime = new Stage(
    new Listener(
        new Door("A", 14),
        new Callback(
            stopDowntime,
            enableEncounters,
            next
        )
    )
);

var preFalLEncounter = new Stage(
    new Listener(
        new Room(15),
        new Callback(
            tpTo(14, 210, 100)
        )
    ),
    new Listener(
        new Blcon(),
        new Callback(
            next
        )
    )
);

var inFalLEncounter = new Stage(
    new Listener(
        new BeforeBattle(),
        new Callback(
            rigWhimsun
        )
    ),
    new Listener(
        new LeaveBattle(),
        new Callback(
            startSegment("leaf-fall-transition"),
            next
        )
    )
);

var leafFallTransition = new Stage(
    new Listener(
        new RoomTransition(14, 15),
        new Callback(
            stopTime,
            disableEncounters,
            tpTo(14, 200, 80),
            next
        )
    )
);

var preOneRock = new Stage(
    new Listener(
        new RoomTransition(14, 15),
        new Callback(
            startDowntime("ruins-one-rock", 10000),
            next
        )
    )
);

var oneRockDowntime = new Stage(
    new Listener(
        new Door("A", 15),
        new Callback(
            stopDowntime,
            enableEncounters,
            next
        )
    )
);

var preLeafMaze = new Stage(
    new Listener(
        new Room(16),
        new Callback(
            tpTo(15, 340, 100)
        )
    ),
    new Listener(
        new Blcon(),
        new Callback(
            next
        )
    )
);

var oneRockEncounter = new Stage(
    new Listener(
        new BeforeBattle(),
        new Callback(
            rigWhimsun
        )
    ),
    new Listener(
        new LeaveBattle(),
        new Callback(
            disableEncounters,
            startSegment("ruins-maze"),
            next
        )
    )
);

var inLeafMaze = new Stage(
    new Listener(
        new RoomTransition(16, 17),
        new Callback(
            stopTime,
            disableEncounters,
            tpTo(16, 520, 220),
            next
        )
    )
);

var preThreeRock = new Stage(
    new Listener(
        new RoomTransition(16, 17),
        new Callback(
            startDowntime("ruins-three-rock", 10000),
            next
        )
    )
);

var threeRockDowntime = new Stage(
    new Listener(
        new Door("A", 17),
        new Callback(
            stopDowntime,
            enableEncounters,
            next
        )
    )
);

var preSecondGrind = new Stage(
    new Listener(
        new Room(18),
        new Callback(
            tpTo(17, 430, 110)
        )
    ),
    new Listener(
        new Blcon(),
        new Callback(
            secondHalfSetup,
            next
        )
    )
);

var inSecondGrind = new Stage(
    new Listener(
        new RoomTransition(18, 17),
        new Callback(
            secondHalfTransition
        )
    ),
    new Listener(
        new BeforeBattle(),
        new Callback(
            secondHalfRig
        )
    ),
    new Listener(
        new EnterBattle(),
        new Callback(
            secondHalfEncounter(false)
        )
    ),
    new Listener(
        new LeaveBattle(),
        new Callback(
            secondHalfVictory
        )
    )
);

var preFleeGrind = new Stage(
    new Listener(
        new Blcon(),
        new Callback(
            next
        )
    )
);

var inFleeGrind = new Stage(
    new Listener(
        new BeforeBattle(),
        new Callback(
            secondHalfRig
        )
    ),
    new Listener(
        new EnterBattle(),
        new Callback(
            secondHalfEncounter(true)
        )
    ),
    new Listener(
        new LeaveBattle(),
        new Callback(
            secondHalfVictory
        )
    )
);

var preTripleMold = new Stage(
    new Listener(
        new Blcon(),
        new Callback(
            next
        )
    )
);

var inTripleMold = new Stage(
    new Listener(
        new BeforeBattle(),
        new Callback(
            rigTripleMold
        )
    ),
    new Listener(
        new EnterBattle(),
        new Callback(
            startSegment("tpl-mold-18")
        )
    ),
    new Listener(
        new LeaveBattle(),
        new Callback(
            stopTime,
            tpTo(17, 500, 110),
            maxRuinsKills,
            next
        )
    )
);

var ruinsPreEnd = new Stage(
    new Listener(
        new Door("A", 17),
        new Callback(
            startSegment("ruins-napsta"),
            next
        )
    )
);

var ruinsNobodyCame = new Stage(
    new Listener(
        new Blcon(),
        new Callback(
            stopTime
        )
    ),
    new Listener(
        new EnterBattle(),
        new Callback(
            nobodyCameSegments
        )
    )
);

var ruinsEnd = new Stage(
    new Listener(
        new Door("Amusic", 41),
        new Callback(
            stopTime,
            next
        )
    )
);

var ruinsScript = new Session(
    offline,
    ruinsStart,
    ruinsHallway,
    preLeafPile,
    leafPileDowntime,
    preFirstGrind,
    inFirstGrind,
    postFirstGrind,
    leafFallDowntime,
    preFalLEncounter,
    inFalLEncounter,
    leafFallTransition,
    preOneRock,
    oneRockDowntime,
    preLeafMaze,
    oneRockEncounter,
    inLeafMaze,
    preThreeRock,
    threeRockDowntime,
    preSecondGrind,
    inSecondGrind,
    preFleeGrind,
    inFleeGrind,
    preTripleMold,
    inTripleMold,
    ruinsPreEnd,
    ruinsNobodyCame,
    ruinsEnd
);

// 1st step is to break into stages


// 2nd step is to break into events
var events = new Dictionary<string, Dictionary<int, Callback>>();

int i = 0;
foreach (Stage stage in ruinsScript.Stages) {
    foreach (Listener listener in stage.Listeners) {
        if (!events.ContainsKey(listener.Event.EventId())) {
            events[listener.Event.EventId()] = new Dictionary<int, Callback>();
        }
        events[listener.Event.EventId()][i] = listener.ListenerCallback;
    }
    i++;
}


// 4th step: define variables to use in each event listener

var roomTransitions = new Dictionary<string, string>();

var enterBattles = new Dictionary<string, string>();

string leaveBattle;

var doorTouches = new Dictionary<string, string>();

var roomPresent = new Dictionary<string, string>();


string generateIfElseBlock (List<string> ifBlocks) {
    return generateIfElseBlock(ifBlocks.ToArray());
}

string generateIfElseBlock (string[] ifBlocks) {
    string code = "";
    bool isFirst = true;
    for (int i = 0; i < ifBlocks.Length; i++) {
        if (isFirst) {
            isFirst = false;
        } else code += "else";
        code += ifBlocks[i];
    }
    return code;
}

// step 5: assemble the code that goes in each event and place it

foreach (string eventId in events.Keys) {
    var stageCodeBlocks = new List<string>();
    Dictionary<int, Callback> eventStages = events[eventId];
    foreach (int stage in eventStages.Keys) {
        stageCodeBlocks.Add(@$"
        if (obj_time.stage == {stage}) {{
            {eventStages[stage].GMLCode}
        }}
        ");

    }
    string eventCode = generateIfElseBlock(stageCodeBlocks);

    bool isEvent (EventName eventName) {
        var split = splitString(eventId);
        return split[0] == eventName.ToString();
    }

    if (isEvent(EventName.PickName)) {
        placeInIf(naming, "naming = 4", eventCode);
    } else if (isEvent(EventName.Blcon)) {
        append(blcon, eventCode);
    } else if (isEvent(EventName.EnterBattle)) {
        enterBattles[eventId] = eventCode;
    } else if (isEvent(EventName.LeaveBattle)) {
        leaveBattle = eventCode;
    } else if (isEvent(EventName.RoomTransition)) {
        roomTransitions[eventId] = eventCode;
    } else if (isEvent(EventName.Door)) {
        doorTouches[eventId] = eventCode;
    } else if (isEvent(EventName.Room)) {
        roomPresent[eventId] = eventCode;
    } else if (isEvent(EventName.BeforeBattle)) {
        place(blconAlarm, "battle = 1", eventCode);
    } else if (isEvent(EventName.FroggitAttack)) {
        place(froggitAlarm, "use_frogskip = 1", eventCode);
    } else if (isEvent(EventName.FroggitTurnStart)) {
        place(froggitStep, "if (global.mnfight == 2)\n{", eventCode);
    } else if (isEvent(EventName.FroggitTurnEnd)) {
        placeInIf(froggitStep, "attacked = 0", eventCode);
    } else if (isEvent(EventName.YouWon)) {
        place(battlecontrol, "earned \"", eventCode);
    }
}

string[] splitString (string str) {
    string[] args = str.Split(new string[] { "," }, StringSplitOptions.None);
    return args;
}

// room related code
List<string> roomCode = new List<string>();

var staticRoomCode = new List<string>();
// text for being in a room
foreach (string roomEvent in roomPresent.Keys) {
    string[] args = splitString(roomEvent);
    staticRoomCode.Add($@"
    if (room == {args[1]}) {{
        {roomPresent[roomEvent]}
    }}
    ");
}


// place text for room transitions
// bool isFirst = true;
foreach (string roomEvent in roomTransitions.Keys) {
    string[] split = splitString(roomEvent);
    roomCode.Add(@$"
    if (obj_time.previous_room == {split[1]} && room == {split[2]}) {{
        {roomTransitions[roomEvent]}
    }}
    ");
}

List<string> withBattlegroup = new List<string>();
string battlegroupless = "";
foreach (string battle in enterBattles.Keys) {
    string[] args = splitString(battle);
    if (args.Length == 1) {
        battlegroupless = enterBattles[battle];
    } else {
        withBattlegroup.Add(@$"
        if (global.battlegroup == {args[1]}) {{
            {enterBattles[battle]}
        }}
        ");
    }
}
roomCode.Add(
    @$"
    if (previous_room != room_battle && room == room_battle) {{
        // exitting too early requires manually setting off persistence
        {generateIfElseBlock(withBattlegroup) + @$"
            else {{
                {battlegroupless}
            }}
            "}    
    }}
    "
);

// string[] battleEvents = new string[] { leaveBattle };
// for (int i = 0; i < 2; i++) {
    string code;
    string firstComparison;
    string secondComparison;
    // if (i == 0) {
    //     code = enterBattle;
    //     firstComparison = "!=";
    //     secondComparison = "==";
    // } else {
        code = leaveBattle;
        firstComparison = "==";
        secondComparison = "!=";
    // }

    roomCode.Add(@$"
    if (obj_time.previous_room {firstComparison} room_battle && room {secondComparison} room_battle) {{
        room_persistent = false;
        {code}
    }}
    ");
// }/

Console.WriteLine(generateIfElseBlock(roomCode));

append(step, generateIfElseBlock(roomCode));
append(step, generateIfElseBlock(staticRoomCode));

Dictionary<string, Dictionary<string, List<string>>> doorCodes = new Dictionary<string, Dictionary<string, List<string>>>();

foreach (string doorTouch in doorTouches.Keys) {
    string[] args = splitString(doorTouch);
    if (!doorCodes.ContainsKey(args[1])) {
        doorCodes[args[1]] = new Dictionary<string, List<string>>();
    }
    if (!doorCodes[args[1]].ContainsKey(args[2])) {
        doorCodes[args[1]][args[2]] = new List<string>();
    }
    doorCodes[args[1]][args[2]].Add(doorTouches[doorTouch]);
}

foreach (string door in doorCodes.Keys) {
    List<string> roomCodes = new List<string>();
    foreach (string room in doorCodes[door].Keys ) {
        roomCodes.Add($@"
        if (room == {room}) {{
            {generateIfElseBlock(doorCodes[door][room])}
        }}
        ");
    }
    string doorCodeEntry = "";
    Console.WriteLine(doorCodeEntry);
    if (door == "C") doorCodeEntry = doorC;
    else if (door == "A") doorCodeEntry = doorA;
    else if (door == "Amusic") doorCodeEntry = doorAmusic;
    append(doorCodeEntry, generateIfElseBlock(roomCodes));
}



// debug mode - REMOVE FOR BUILD
useDebug();

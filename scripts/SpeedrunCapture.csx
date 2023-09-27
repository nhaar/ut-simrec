
// testing script-game compatibility
EnsureDataLoaded();

if (Data?.GeneralInfo?.DisplayName?.Content.ToLower() != "undertale") {
    ScriptError("Error 0: Script must be used in Undertale");
}

// helper method that adds text in gml based on a preceding text
void placeTextInGML(string codeName, string preceding, string replacement) {
    ReplaceTextInGML(codeName, preceding, $"{preceding}{replacement}");
}

// will be using obj_time for all the API of the mod
var create = Data.Code.ByName("gml_Object_obj_time_Create_0");
var step = Data.Code.ByName("gml_Object_obj_time_Step_1");
var draw = Data.Code.ByName("gml_Object_obj_time_Draw_64");

// using blcon to check whenver an encounter is found
var blcon = Data.Code.ByName("gml_Object_obj_battleblcon_Create_0");
var blconAlarm = Data.Code.ByName("gml_Object_obj_battleblcon_Alarm_0");

// make drawing work
Data.GameObjects.ByName("obj_time").Visible = true;


string assignNonTakenIndex (string arr, string index, string value) {
    var arrAccess = arr + "[target_index]";
    return @"
    var target_index = " + index + @";
    // using 0 to indicate non existent elements
    while (" + arrAccess + @" != 0) {
        target_index++;
    }" +
    arrAccess + @" = " + value + @";
    ";
}

// initializing
create.AppendGML(@"
session_name = 0;

stage = 0;
current_msg = '';

is_timer_running = 0;
time_start = 0;
time_end = 0;

segment_name = '';

end_down_time_flag = 0;
// mode is for when downtime is being watched
// running is for when downtime is being watched and a downtime has been reached
is_downtime_mode = 0;
downtime_name = '';
is_downtime_mode = 0;
is_downtime_running = 0;
downtime_start = 0;
downtime = 0;
step_count = 0;
met_encounter = 0;
previous_time = 0;

previous_room = 0;
current_room = 0;

//randomize call is in gamestart, which only runs after obj_time
randomize();

// finding the order of encounters

// ruins first half must have 5 encounters, 4 of them are froggits and 1 is a whimsun
// gml 1 array so must initialize with highest index
first_half_encounters[4] = 0;
// lvl 2 froggit must come before lvl 3 one, thus it cant be last one
// aditionally, I want to LV up in a frogskip or a not frogskip because then the LV up slowdown won't in the LV 2 time
var lvl2_index = irandom(2);
// next one is an index between lvl2_index and 3
// the index + 1 is due to irandom including the last one
var frogskiprel = irandom(3 - lvl2_index - 1) + lvl2_index + 1;
// lvl3 must be after the frogskip one (where you LV up)
var lvl3_index = irandom(4 - frogskiprel - 1) + frogskiprel + 1;
var frogskip_roll = irandom(1);
first_half_encounters[lvl2_index] = '2';
first_half_encounters[lvl3_index] = '3';
var rolled_frogskip = 'N';
var notrolled_frogskip = 'F';
if (frogskip_roll) {
    rolled_frogskip = 'F';
    notrolled_frogskip = 'N';
}
first_half_encounters[frogskiprel] = rolled_frogskip;
" +
assignNonTakenIndex("first_half_encounters", "irandom(1)", "'W'") +
assignNonTakenIndex("first_half_encounters", "0", "notrolled_frogskip") + @"
show_debug_message(first_half_encounters[0]);
show_debug_message(first_half_encounters[1]);
show_debug_message(first_half_encounters[2]);
show_debug_message(first_half_encounters[3]);
show_debug_message(first_half_encounters[4]);
"
// ruins first half encounters array guide:
// W: whimsun
// 2: Froggit at LV2
// 3: Froggit at LV3
// F: Froggit with frogskip
// N: Froggit without frogskip
, Data);

// room tracker; it is useful for some segments that are room based
step.AppendGML(@"
previous_room = current_room;
current_room = room;
", Data);

// message updater
step.AppendGML(@"
if (stage == 0) {
    current_msg = 'RECORDING SESSION WAITING TO START
To start it, begin a normal run,
and keep playing until the mod stops you';
} else if (stage == 3) {
    current_msg = 'Next, walk through the next room
as quickly as possible';
} else if (stage == 6) {
    current_msg = 'Now, go through the next room
and proceed as if you were in a normal
run until the mod stops you';
} else if (stage == 13) {
    current_msg = 'Now, proceed as if you have
finished the first 11 kills, and keep
going as if you were in a run,
that is, you will begin by
walking right, grinding at the end,
and keep going until stopped
';
} else {
    current_msg = '';
}
", Data);

string appendNewTime (string name, string time) {
    return @"
    var file = file_text_open_append('recording_' + string(obj_time.session_name));
    file_text_write_string(file, " + name + @" + '=' + string(" + time + @") + ';');
    file_text_close(file);
    ";
}

// downtime timer api
step.AppendGML(@"
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
", Data);

// still part of the downtime timer api
blcon.AppendGML(@"
    if (obj_time.is_downtime_mode) {
        obj_time.met_encounter = 1;
    }
", Data);

// message drawer
draw.AppendGML(@"
draw_set_font(fnt_main)
draw_set_color(c_yellow);
draw_text(20, 20, current_msg);
", Data);

// adding listeners for starting/ending sessions and changing stages

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
    return @"
    if (!obj_time.is_timer_running) {" +
        startTime + newStage(stage) + $"obj_time.segment_name = {nameString};" + @"
    }
    ";
}

string startDowntime (string name, int steps, int stage = -1) {
    return @"
    if (!obj_time.is_downtime_mode) {
        obj_time.is_downtime_mode = 1;
        obj_time.downtime = 0;
        obj_time.downtime_start = 0;
        obj_time.step_count = global.encounter;
        obj_time.optimal_steps = " + steps.ToString() + @"
        " + newStage(stage) + $"obj_time.downtime_name = '{name}';" + @"
    }
    ";
}

var stopTime = @"
if (obj_time.is_timer_running) {
    obj_time.is_timer_running = 0;" + appendNewTime("obj_time.segment_name", "get_timer() - obj_time.time_start") + @"
}
";

var stopDowntime = @"
// in case the downtime ends during a downtime, must not lose the time being counted
if (obj_time.is_downtime_mode) {
    if (obj_time.is_downtime_running) {
        obj_time.downtime += get_timer() + obj_time.downtime_start
    }
    obj_time.is_downtime_mode = 0;" + appendNewTime("obj_time.downtime_name", "obj_time.downtime") + @"
}
";

// naming screen time start
ReplaceTextInGML("gml_Script_scr_namingscreen", "naming = 4", @"
{ naming = 4;" + startSegment("ruins-start", 1) + startSession + @"}
");

// encountering first froggit in ruins
// battlegroup 3 is first froggit

// at the start of blcon; end of ruins start
blcon.AppendGML(@"if (global.battlegroup == 3) {" + stopTime + @"}", Data);

// at the end of blcon; being of ruins hallway
placeTextInGML("gml_Object_obj_battleblcon_Alarm_0", "battle = 1", @"
if (global.battlegroup == 3) {" + startSegment("ruins-hallway", 2) + @"
}
");

// go back to past room and put at the end
string tpRuinsHallway = @"
    room = 11;
    obj_mainchara.x = 2400;
";

// exiting ruins hallway; various stages use this event
step.AppendGML(@"
// as soon as gain movement for the first time
if (room == 12 && global.interact == 0) {
    if (stage == 2) {" +
        newStage(3) +
        tpRuinsHallway + @"
        // crank up kill count just high enough so it will not give an encounter in the next room
        global.flag[202] = 4;
    } else if (stage == 3) {" +
        startDowntime("ruins-leafpile", 97, 4) + @"
    } else if (stage == 6) {" +
        newStage(7) + @"
    }
}
", Data);

// exitting ruins leafpile; end of downtime

Data.Code.ByName("gml_Object_obj_doorC_Other_19").AppendGML(@"
if (obj_time.stage == 4 && room == 12) {"  +
    
    stopDowntime +
    newStage(5) + @"
}
", Data);

// to go back to the hallway for next stage
step.AppendGML(@"
if (stage == 5 && room > 12) {" + 
    tpRuinsHallway +
    newStage(6) + @"
}
", Data);

string isFirstHalfStage = "obj_time.stage > 6 && obj_time.stage < 12";
string firstHalfCurrentEncounter = "var current_encounter = obj_time.first_half_encounters[obj_time.stage - 7];";

//
placeTextInGML("gml_Object_obj_battleblcon_Alarm_0", "battle = 1", @"
if (" + isFirstHalfStage + @") {
    // default to froggit, since it's the most probable
    var to_battle = 4;
    // 7 is first possible stage here
    if (obj_time.first_half_encounters[obj_time.stage - 7] == 'W') {
        // whimsun battlegroup
        to_battle = 5;
    }
    global.battlegroup = to_battle;
}
");

// starting a segment from a battle
step.AppendGML(@"
if (previous_room != room_battle && current_room == room_battle) {
    // first half segments for encounters
    if (" + isFirstHalfStage + @") {" + 
        firstHalfCurrentEncounter + @"
        name = 0;
        if (current_encounter == '2') {
            name = 'froggit-lv2';
        } else if (current_encounter == '3') {
            name = 'froggit-lv3';
        } else if (current_encounter == 'W') {
            name = 'whim';
        }
        //filtering out frogskip related ones
        if (name != 0) {" +
            startSegment("name", -1, true) + @"
        }
    }
}
", Data);

// ending a segment from a battle
step.AppendGML(@"
if (previous_room == room_battle && current_room != room_battle) {
    if (" + isFirstHalfStage + @") {
        " + firstHalfCurrentEncounter + @"
        // leave the player high enough XP for guaranteed LV up next encounter if just fought the LV 2 encounter
        if (current_encounter == '2') {
            global.xp = 29;
        }
        if (current_encounter != 'W' || current_encounter != 'F') {" + 
            stopTime + @"
        }
        // increment for BOTH the in turn and the whole battle segments 
        obj_time.stage++;
    }
}
", Data);

// rigging attacks for froggit
// it is placed right after mycommand declaration
placeTextInGML("gml_Object_obj_froggit_Alarm_6", "0))", @"
if (" + isFirstHalfStage + @") {
    " + firstHalfCurrentEncounter + @"
    if (current_encounter == 'N') {
        mycommand = 100;
    } else { // as a means to speed up practice, all of them will have frog skip by default
        mycommand = 0;
    }
}
");

// start the first half segments for the froggit attacks
placeTextInGML("gml_Object_obj_froggit_Step_0", "if (global.mnfight == 2)\n{", @"
if (" + isFirstHalfStage + @") {
    " + firstHalfCurrentEncounter + @"
    var name = 0;
    switch (current_encounter) {
        case 'F':
            name = 'frogskip';
            break;
        case 'N':
            name = 'not-frogskip';
            break;
    }
    if (name != 0) {" +
        startSegment("name", -1, true) + @"
    }
}
");

// end first half segments for froggit attacks
ReplaceTextInGML("gml_Object_obj_froggit_Step_0", "attacked = 0", @" {
    attacked = 0;
    if (" + isFirstHalfStage + @") {
        " + firstHalfCurrentEncounter + @"
        if (current_encounter == 'F' || current_encounter == 'N') {" +
            stopTime + @"
        }
    }
}");

// get out of first half (beginning interlude stage)
step.AppendGML(@"
// room < 20 just to say not battle
if (stage == 12 && room < 20) {
    stage = 13;
    room = 12;
    obj_mainchara.x = 140;
    obj_mainchara.y = 360;
}
", Data);

// debug function
void useDebug () {
    // updating it every frame is just a lazy way of doing it since it can't be done in obj_time's create event
    // since it gets overwritten by gamestart
    step.AppendGML("global.debug = 1;", Data);

    // stage skip keybinds
    // Q skips to stage 6
    step.AppendGML(@"
    if (keyboard_check_pressed(ord('Q'))) {
        is_timer_running = 0;
        stage = 6;
        global.xp = 10;
        global.flag[202] = 4;
        script_execute(scr_levelup);
        global.plot = 9.5;" +
        tpRuinsHallway + @"
    }
    ", Data);

    string[] watchVars = {
        "is_timer_running",
        "obj_time.stage",
        "previous_room",
        "current_room",
        "global.battlegroup"
    };

    // start just with line break just to not interefere with anything
    string code = @"
    ";
    int i = 0;
    foreach (string watchVar in watchVars) {
        code += $"draw_text(20, {110 + i * 25}, '{watchVar}: ' + string({watchVar}));";
        i++;
    }
    draw.AppendGML(code, Data);
}

// debug mode - REMOVE FOR BUILD
useDebug();
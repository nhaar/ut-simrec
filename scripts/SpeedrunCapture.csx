
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

", Data);

// message updater
step.AppendGML(@"
if (stage == 0) {
    current_msg = 'RECORDING SESSION WAITING TO START
To start it, begin a normal run,
and keep playing until the mod stops you';
} else if (stage == 1) {
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

string newStage (int stage) {
    return $"obj_time.stage = {stage};";
}

var startTime = @"
obj_time.is_timer_running = 1;
obj_time.time_start = get_timer();
";

string startSegment (int stage, string name) {
    return startTime + newStage(stage) + $"obj_time.segment_name = '{name}';";
}

string startDowntime (int stage, string name, int steps) {
    return @"
    obj_time.is_downtime_mode = 1;
    obj_time.downtime = 0;
    obj_time.downtime_start = 0;
    obj_time.step_count = global.encounter;
    obj_time.optimal_steps = " + steps.ToString() + @"
    " + newStage(stage) + $"obj_time.downtime_name = '{name}';";
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
{ naming = 4;" + startSegment(1, "ruins-start") + startSession + @"}
");

// encountering first froggit in ruins
// battlegroup 3 is first froggit

// at the start of blcon; end of ruins start
blcon.AppendGML(@"if (global.battlegroup == 3) {" + stopTime + @"}", Data);

// at the end of blcon; being of ruins hallway
placeTextInGML("gml_Object_obj_battleblcon_Alarm_0", "battle = 1", @"
if (global.battlegroup == 3) {" + startSegment(2, "ruins-hallway") + @"
}
");

// exiting ruins hallway; end ruins hallway and start downtime
step.AppendGML(@"
// as soon as gain movement for the first time
if (stage == 2 && room == 12 && global.interact == 0) {" +
    stopTime + startDowntime(3, "ruins-leafpile", 97) + 
@"
}
", Data);

// exitting ruins leafpile; end of downtime

Data.Code.ByName("gml_Object_obj_doorC_Other_19").AppendGML(@"
// starting downtime gives stage 2, but a new stage is started on the same room with the encounter
if (obj_time.stage < 4 && room == 12) {"  + stopDowntime + @"
}
", Data);

// 

// debug - REMOVE FOR BUILD


// debug function
void useDebug () {
    step.AppendGML("global.debug = 1;", Data);

    string[] watchVars = {
        "is_timer_running",
        "obj_time.stage",
        "global.encounter",
        "step_count",
        "global.interact",
        "is_downtime_running",
        "is_downtime_mode",
        "downtime",
        "downtime_start",
        "get_timer()"
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

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
start_time_flag = 0;
stop_time_flag = 0;
time_start = 0;
time_end = 0;

segment_name = '';

start_downtime = 0;
is_downtime_mode = 0;
is_downtime_running = 0;
donwtime_start = 0;
downtime_end = 0;
step_count = 0;
met_encounter = 0;


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

// continuous timer api
step.AppendGML(@"
if (stop_time_flag) {
    stop_time_flag = 0;
    if (is_timer_running) {
        is_timer_running = 0;
        var file = file_text_open_append('recording_' + string(session_name));
        file_text_write_string(file, segment_name + '=' + string(get_timer() - time_start) + ';');
        file_text_close(file);
    }
}
// stop being first is on strategical, and if being two if statements as well since they can run in the same frame
if (start_time_flag) {
    start_time_flag = 0;
    is_timer_running = 1;
    time_start = get_timer();
}

", Data);

// downtime timer api
step.AppendGML(@"
// will likely need to tweak this slightly depending on the order of things
if (start_downtime && !is_downtime_mode) {
    is_downtime_mode = 1;
    downtime = 0;
    downtime_start = 0;
    downtime_end = 0;
    step_count = global.encounter;
} else if (is_downtime_mode) {
    if (is_downtime_running) {
        // being greater means it has incremented, and there was no downtime this frame
        // else if the encounter has been met, it is expected we must leave the room asap and anytime not leaving is downtime
        if (global.encounter > step_count || met_encounter) {
            downtime_end = get_timer();
            downtime += downtime_end - downtime_start;
            is_downtime_running = 0;
        }
    } else {
        // being equals means global.encounter did not increment, thus downtime has begun
        if (global.encounter == step_count) {
            downtime_start = get_timer();
            is_downtime_running = 1;
        }
    }
    step_count = global.encouter;
}
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

var startTime = @"obj_time.start_time_flag = 1;";

string startSegment (int stage, string name) {
    return startTime + newStage(stage) +  $"obj_time.segment_name = '{name}'";
}

var stopTime = @"obj_time.stop_time_flag = 1;";

// naming screen time start
placeTextInGML("gml_Script_scr_namingscreen", "if (naming == 4)\n{", startSegment(1, "ruins-start") + startSession);

// encountering first froggit in ruins
// battlegroup 3 is first froggit

// at the start of blcon; end of ruins start
blcon.AppendGML(@"if (global.battlegroup == 3) {" + stopTime + @"}", Data);

// at the end of blcon; being of ruins hallway
placeTextInGML("gml_Object_obj_battleblcon_Alarm_0", "battle = 1", startSegment(2, "ruins-hallway"));

// exiting ruins hallway
step.AppendGML(@"
// as soon as gain movement for the first time
if (stage == 2 && room == 12 && global.interact == 0) {" +
    stopTime + newStage(3) + 
@"
}
", Data);

// debug - REMOVE FOR BUILD
step.AppendGML(@"global.debug = 1;", Data);
draw.AppendGML(@"
draw_text(20, 100, is_timer_running);
draw_text(20, 120, obj_time.stage);
", Data);
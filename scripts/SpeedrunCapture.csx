using System.Linq;

/******
helper variables and functions
******/

// Class for a GML if-else code block
class IfElseBlock {
    /// <summary>
    /// List of strings of GML code of the form `if (CONDITION) {CODE}`
    /// </summary>
    private List<string> IfBlocks = new List<string>();

    /// <summary>
    /// Code that should go at the `else` part of the block (after all `if`s and `else if`s)
    /// </summary>
    private string ElseBlock = "";

    /// <summary>
    /// Add all if blocks from an array of blocks
    /// </summary>
    /// <param name="blocks"></param>
    public void AddBlocks (string[] blocks) {
        foreach (string block in blocks) AddIfBlock(block);
    }

    /// <summary>
    /// Add all if blocks from a list of blocks
    /// </summary>
    /// <param name="blocks"></param>
    public void AddBlocks(List<string> blocks) {
        AddBlocks(blocks.ToArray());
    }

    /// <summary>
    /// Add a block from a string
    /// </summary>
    /// <param name="code"></param>
    public void AddIfBlock (string code) {
        IfBlocks.Add(code);
    } 

    /// <summary>
    /// Set the value of the `else` block
    /// </summary>
    /// <param name="code"></param>
    public void SetElseBlock (string code) {
        ElseBlock = code;
    }

    /// <summary>
    /// Get the final GML code for the block
    /// </summary>
    /// <returns></returns>
    public string GetCode () {
        // if there are no if statements, then the `else` block should not be wrapped on anything
        if (IfBlocks.Any()) {
            return String.Join("else ", IfBlocks) + $"else {{ {ElseBlock} }}";
        } else return ElseBlock;
    }

    /// <summary>
    /// Create an empty if-else block
    /// </summary>
    public IfElseBlock () {}

    /// <summary>
    /// Create an if-else block with if statements from a list
    /// </summary>
    /// <param name="blocks"></param>
    public IfElseBlock (List<string> blocks) {
        AddBlocks(blocks);
    }

    /// <summary>
    /// Create an if-else block with if statements from an array
    /// </summary>
    /// <param name="blocks"></param>
    public IfElseBlock (string[] blocks) {
        AddBlocks(blocks);
    }

    /// <summary>
    /// Get the if else block GML code from an array of if blocks
    /// </summary>
    /// <param name="blocks"></param>
    /// <returns></returns>
    public static string GetIfElseBlock (string[] blocks) {
        var block = new IfElseBlock(blocks);
        return block.GetCode();
    }

    /// <summary>
    /// Get the if else block GML code from a list of if blocks
    /// </summary>
    /// <param name="blocks"></param>
    /// <returns></returns>
    public static string GetIfElseBlock (List<string> blocks) {
        return GetIfElseBlock(blocks.ToArray());
    }
}

/// <summary>
/// Contains the name of all code entries that will be used
/// </summary>
public static class CodeEntryClass {
    // will be using obj_time for the API of the mod

    /// <summary>
    /// `Create` script for `obj_time`
    /// </summary>
    public static string create = "gml_Object_obj_time_Create_0";

    /// <summary>
    /// `BeginStep` script for `obj_time`
    /// </summary>
    public static string step = "gml_Object_obj_time_Step_1";

    /// <summary>
    /// `Draw` script for `obj_time`
    /// </summary>
    public static string draw = "gml_Object_obj_time_Draw_64";

    /// <summary>
    /// Code that runs at the start of "blcon"s
    /// </summary>
    public static string blcon = "gml_Object_obj_battleblcon_Create_0";

    /// <summary>
    /// Code that runs at the end of "blcon"s
    /// </summary>
    public static string blconAlarm = "gml_Object_obj_battleblcon_Alarm_0";

    /// <summary>
    /// Code for picking how many steps are needed for an encounter
    /// </summary>
    public static string scrSteps = "gml_Script_scr_steps";

    /// <summary>
    /// Code where the player picks the name for the game
    /// </summary>
    public static string naming = "gml_Script_scr_namingscreen";

    /// <summary>
    /// Step code that contains the "YOU WON" screen
    /// </summary>
    public static string battlecontrol = "gml_Object_obj_battlecontroller_Step_0";

    /// <summary>
    /// Step code for Froggit
    /// </summary>
    public static string froggitStep = "gml_Object_obj_froggit_Step_0";

    /// <summary>
    /// Froggit enemy alarm used to decide the attacks
    /// </summary>
    public static string froggitAlarm = "gml_Object_obj_froggit_Alarm_6";

    // code for the room transition doors being touched

    /// <summary>
    /// Code for touching `doorA`
    /// </summary>
    public static string doorA = "gml_Object_obj_doorA_Other_19";

    /// <summary>
    /// Code for touching `doorAmusic`
    /// </summary>
    public static string doorAmusic = "gml_Object_obj_doorAmusicfade_Other_19";

    /// <summary>
    /// Code for touching `doorC`
    /// </summary>
    public static string doorC = "gml_Object_obj_doorC_Other_19";
}

/*
UNDERTALE CODE MANIPULATION
*/

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

/// <summary>
/// Place text inside a code entry in which the location to place it is currently a "braceless" if statement
/// (with only one statement) and you want to put it inside this braceless if statement
/// </summary>
/// <param name="codeName">Name of the code entry</param>
/// <param name="preceding">String matching the exact text for the statement inside if</param>
/// <param name="placement">New code to place</param>
void placeInIf (string codeName, string preceding, string placement) {
    ReplaceTextInGML(codeName, preceding, @$"
    {{
        {preceding};
        {placement}
    }}
    ");
}

/// <summary>
/// Contains all GML code, code generator and GML variable methods and fields 
/// </summary>
static class GMLCodeClass {
    /// <summary>
    /// Generate GML code that (assuming the array has enough empty indexes) populates entries of the array randomly
    /// with a given set of string values
    /// </summary>
    /// <param name="arr">Name of the array variable</param>
    /// <param name="elements">Value of all elements to populate</param>
    /// <returns></returns>
    public static string RandomPopulateArray (string arr, params string[] elements) {
        var arrAccess = arr + "[target_index]";
        var code = "";
        for (int i = elements.Length - 1; i >= 0; i--) {
            code += @$"
            var target_index = irandom({i});
            // using 0 to indicate non existent elements
            while ({arrAccess} != 0) {{
                target_index++;
            }}
            {arrAccess} = '{elements[i]}';
            ";
        }
        return code;
    }

    /// <summary>
    /// Create a GML string literal from a string 
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string GMLString (string str) {
        return $"\"{str}\"";
    }

    /// <summary>
    /// Generate GML code that appends the time for a segment/downtime to the end of the current session's file
    /// </summary>
    /// <param name="name">Name for the segment/downtime</param>
    /// <param name="time">String containing the total time obtained (in microseconds)</param>
    /// <returns></returns>
    public static string AppendNewTime (string name, string time) {
        return @$"
        var file = file_text_open_append('recordings/recording_' + string(obj_time.session_name));
        file_text_write_string(file, {name} + '=' + string({time}) + ';');
        file_text_close(file);
        ";
    }

    /// <summary>
    /// GML code that starts a new recording session
    /// </summary>
    public static string StartSession = @"
    obj_time.session_name = string(current_year) + string(current_month) + string(current_day) + string(current_hour) + string(current_minute) + string(current_second);
    var file = file_text_open_write('recordings/recording_' + string(obj_time.session_name));
    file_text_close(file);
    ";

    /// <summary>
    /// Generate GML code that starts the segment timer
    /// </summary>
    /// <param name="segmentName">Name of the segment</param>
    /// <param name="isVarName">
    /// Should be set to `true` if the `name` param should be interpreted as being a GML variable name
    /// and to `false` if it should be interpreted as being a GML string literal
    /// </param>
    /// <returns></returns>
    public static string StartSegment (string segmentName, bool isVarName = false) {
        string nameString = isVarName ? segmentName : $"'{segmentName}'";
        return @$"
        if (!obj_time.is_timer_running) {{
            obj_time.is_timer_running = 1;
            obj_time.time_start = get_timer();
            obj_time.segment_name = {nameString};
        }}
        ";
    }

    /// <summary>
    /// GML code that advances the stage
    /// </summary>
    public static string Next = @"
    obj_time.stage++;
    ";

    /// <summary>
    /// Generate GML code that starts the downtime mode
    /// </summary>
    /// <param name="downtimeName">Name of the downtime</param>
    /// <param name="steps">
    /// Number of optimal steps to complete downtime. If not given, an arbitrarily high
    /// number will be given assuming it is not important
    /// </param>
    /// <returns></returns>
    public static string StartDowntime (string downtimeName, int steps = 10000) {
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
    public static string StopTime = @$"
    if (obj_time.is_timer_running) {{
        obj_time.is_timer_running = 0;
        {AppendNewTime("obj_time.segment_name", "get_timer() - obj_time.time_start")}
    }}
    ";

    /// <summary>
    /// GML code that stops downtime mode
    /// </summary>
    public static string StopDowntime = @$"
    // in case the downtime ends during a downtime, must not lose the time being counted
    if (obj_time.is_downtime_mode) {{
        if (obj_time.is_downtime_running) {{
            obj_time.downtime += get_timer() + obj_time.downtime_start
        }}
        obj_time.is_downtime_mode = 0;
        {AppendNewTime("obj_time.downtime_name", "obj_time.downtime")}
    }}
    ";

    /// <summary>
    /// GML code that adds adds to a GML code context a `var` `encounter_name` with the value of the current encounter
    /// </summary>
    /// <param name="arr">Name of the array that contains the current encounter</param>
    /// <param name="code">Code to add the variable to</param> 
    /// <returns></returns>
    public static string WithEncounterName (string arr, string code) {
        return @$"
        var encounter_name = obj_time.{arr}[obj_time.current_encounter]
        {code}
        ";
    }

    /// <summary>
    /// GML code that adds adds to a GML code context a `var` `encounter_name` with the value of the first half encounter
    /// </summary>
    /// <param name="code">Code to add the variable to</param>
    /// <returns></returns>
    public static string WithFirstHalfEncounter (string code) {
        return WithEncounterName("first_half_encounters", code);
    }

    /// <summary>
    /// GML code that disables the encounters
    /// </summary>
    public static string DisableEncounters = @"
    obj_time.fast_encounters = 0;
    ";

    /// <summary>
    /// Generate GML code that teleports the player to a room and in a given position inside the room
    /// </summary>
    /// <param name="room">The room ID</param>
    /// <param name="x">x position to teleport to</param>
    /// <param name="y">y position to telport to</param>
    /// <returns></returns>
    public static string TPTo (int room, int x, int y) {
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
    public static string IsMoving = @"
    (keyboard_check(vk_left) || keyboard_check(vk_right) || keyboard_check(vk_up) || keyboard_check(vk_down))
    ";

    /// <summary>
    /// GML code that enables the encounters
    /// </summary>
    public static string EnableEncounters = @"
    obj_time.fast_encounters = 1;
    ";

    /// <summary>
    /// GML code that unrigs the frogskip in the first half for a specific encounters
    /// </summary>
    public static string UnrigFrogskip = WithFirstHalfEncounter(@$"
    if (encounter_name == 'N') {{
        use_frogskip = 0;
    }}
    ");

    /// <summary>
    /// GML code that starts timing Froggit's attacks
    /// </summary>
    public static string TimeFrogTurn = WithFirstHalfEncounter($@"
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
        {StartSegment("name", true)}
    }}
    ");

    /// <summary>
    /// GML code that ends the time for Froggit's attacks
    /// </summary>
    public static string EndFrogTime = WithFirstHalfEncounter($@"
    if (encounter_name == 'F' || encounter_name == 'N') {{
        {StopTime}
    }}
    ");

    /// <summary>
    /// GML code that starts the segments for the "YOU WON!" message
    /// </summary>
    public static string TimeYouWon = WithFirstHalfEncounter(@$"
    // for 'A', we are starting time for the LV up text
    if (encounter_name == 'A') {{
        {StartSegment("lv-up")}
    }} else if (encounter_name == 'N') {{
        // 'N' will be the reserved item for measuring the normal you won text ('F' could be as well, just a choice)
        {StartSegment("you-won")}
    }}
    ");

    /// <summary>
    /// GML code that rigs the encounter to bein a Whimsun
    /// </summary>
    public static string RigWhimsun = @$"
    global.battlegroup = 5;
    ";

    /// <summary>
    /// GML code that sets the Ruins area kills to max
    /// </summary>
    public static string MaxRuinsKills = @$"
    global.flag[202] = 20;
    ";

    /// <summary>
    /// GML code that rigs the encounter to be a triple mold
    /// </summary>
    public static string RigTripleMold = @$"
    global.battlegroup = 8;
    ";

    /// <summary>
    /// GML code that start segments for the nobody came section in Ruins
    /// </summary>
    public static string NobodyCameSegments = @$"
    if (obj_time.nobody_came == 0) {{
        {StartSegment("ruins-switches")}
    }} else if (obj_time.nobody_came == 1) {{
        {StartSegment("perspective-a")}
    }} else if (obj_time.nobody_came == 2) {{
        {StartSegment("perspective-b")}
    }} else if (obj_time.nobody_came == 3) {{
        {StartSegment("perspective-c")}
    }} else if (obj_time.nobody_came == 4) {{
        {StartSegment("perspective-d")}
    }} else {{
        {StartSegment("ruins-end")}
        obj_time.stage++;
    }}
    obj_time.nobody_came++;
    ";

    /// <summary>
    /// GML code to run when starting the second half grind
    /// </summary>
    public static string ResetEncounters = @$"
    obj_time.current_encounter = 0;
    ";

    /// <summary>
    /// Generate GML code that assigns a variable to a `assignVar` variable based on the value of `readVar`
    /// </summary>
    /// <param name="map">Dictionary that maps values to test `readVar` to values to assign `assignVar` if was equal</param>
    /// <param name="readVar">Name of the variable</param>
    /// <param name="assignVar">Name of the variable</param>
    /// <returns></returns>
    public static string GetVariableSwitch (Dictionary<string, string> map, string readVar, string assignVar) {
        var block = new IfElseBlock();
        foreach (string readValue in map.Keys) {
            var assignValue = map[readValue];
            block.AddIfBlock(@$"
            if ({readVar} == {readValue}) {{
                {assignVar} = {assignValue};
            }}
            ");
        }

        return block.GetCode();
    }
}

/// <summary>
/// Function that if called will add debug functions to the game
/// </summary>
void useDebug () {
    // updating it every frame is just a lazy way of doing it since it can't be done in obj_time's create event
    // since it gets overwritten by gamestart
    append(CodeEntryClass.step, "global.debug = 1;");

    // stage skip keybinds
    append(CodeEntryClass.step, @$"
    if (keyboard_check_pressed(ord('Q'))) {{
        is_timer_running = 0;
        stage = 3;
        global.xp = 10;
        script_execute(scr_levelup);
        global.plot = 9;
        {GMLCodeClass.TPTo(11, 2400, 80)}
    }}
    ");

    append(CodeEntryClass.step, @$"
    if (keyboard_check_pressed(ord('E'))) {{
        is_timer_running = 0;
        obj_time.stage = 7;
        global.xp = 30;
        script_execute(scr_levelup);
        global.plot = 9.5;
        {GMLCodeClass.TPTo(12, 240, 340)}
    }}
    ");

    // variables to print
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
    append(CodeEntryClass.draw, code);

    // coordinates
    append(CodeEntryClass.draw, @$"
    if (instance_exists(obj_mainchara)) {{
        draw_text(20, {(110 + i * 25)}, 'x:' +  string(obj_mainchara.x));
        draw_text(20, {(110 + (i + 1) * 25)}, 'y:' + string(obj_mainchara.y));
    }}
    ");
}

/// <summary>
/// Name for all GML placement methods
/// </summary>
enum PlaceMethod {
    /// <summary>
    /// Appends to the end of file
    /// </summary>
    Append,

    /// <summary>
    /// Place inside a file
    /// </summary>
    Place,

    /// <summary>
    /// Place inside an if statement with a single statement
    /// </summary>
    PlaceInIf
}

/*
EVENT CLASSES
*/

/// <summary>
/// Abstract class for the events that will be watched for the script
/// </summary>
abstract class UndertaleEvent {
    /// <summary>
    /// Must return a unique string with its arguments comma separated
    /// </summary>
    /// <returns></returns>
    public abstract string EventArgs ();

    /// <summary>
    /// Generate GML code for the condition needed for this event.
    /// 
    /// If none in particular exist, the method should return "1"
    /// </summary>
    /// <returns></returns>
    public virtual string GMLCondition () {
        return "1";
    }

    /// <summary>
    /// GML place method this event will use
    /// </summary>
    public abstract PlaceMethod Method { get; }
    
    /// <summary>
    /// If applicable, the `replacement` string to give to the method that will place the GML
    /// </summary>
    public virtual string Replacement { get; }

    /// <summary>
    /// Should return the code that will be executed when this event is fired 
    /// </summary>
    /// <param name="code">Base code</param>
    /// <returns></returns>
    public virtual string Placement (string code) {
        return code;
    }

    /// <summary>
    /// Should return the GML code entry where the code for this event is placed
    /// </summary>
    /// <returns></returns>
    public abstract string CodeEntry ();

    /// <summary>
    /// Get a unique identifier for this event and its arguments
    /// </summary>
    /// <returns></returns>
    public string EventId () {
        var type = this.GetType().Name;
        var args = EventArgs();
        return args == "" ? type : $"{type},{args}";
    }

    // the two methods below are for using this class inside dictionaries

    public override int GetHashCode () {
        return EventId().GetHashCode();
    }

    public override bool Equals (object obj) {
        if (obj is UndertaleEvent otherObj) {
            return otherObj.EventId() == this.EventId();
        }
        return false;
    }
}

/// <summary>
/// Class for a Undertale Event that takes no arguments (is unique)
/// </summary>
abstract class UniqueEvent : UndertaleEvent {    
    public override string EventArgs () {
        return "";
    }
}

/// <summary>
/// Event for picking the name
/// </summary>
class PickName : UniqueEvent {   
    public override PlaceMethod Method => PlaceMethod.PlaceInIf;

    public override string Replacement => "naming = 4";

    public override string CodeEntry() {
        return CodeEntryClass.naming;
    }
}

/// <summary>
/// Event for when the blcon shows up in the screen
/// </summary>
class Blcon : UniqueEvent {
    public override PlaceMethod Method => PlaceMethod.Append;

    public override string CodeEntry() {
        return CodeEntryClass.blcon;
    }
}

/// <summary>
/// Event for when a battle starts
/// </summary>
class EnterBattle : UndertaleEvent {
    /// <summary>
    /// Battlegroup id for the battle that is being watched, equal to `-1` if all battles are being watched
    /// </summary>
    public int Battlegroup;
    
    /// <summary>
    /// Create event listening to all battles
    /// </summary>
    public EnterBattle () {
        Battlegroup = -1;
    }

    /// <summary>
    /// Create event listening to a specific battle
    /// </summary>
    /// <param name="battlegroup">Battlegroup id that will be watched</param>
    public EnterBattle (int battlegroup) {
        Battlegroup = battlegroup;
    }

    public override PlaceMethod Method => PlaceMethod.Append;

    public override string Placement (string code) {
        Console.WriteLine("Hello!");
        return @$"
        if (obj_time.previous_room != room_battle && room == room_battle) {{
            {code}
        }}
        ";
    }

    public override string GMLCondition () {
        if (Battlegroup < 0) {
            return "1";
        } else return $"global.battlegroup == {Battlegroup}";
    }

    public override string CodeEntry() {
        return CodeEntryClass.step;
    }

    public override string EventArgs () {
        return Battlegroup > -1 ? Battlegroup.ToString() : "";
    }
}

/// <summary>
/// Event for a room transition
/// </summary>
class RoomTransition : UndertaleEvent {
    /// <summary>
    /// Id of the room being transitioned out of
    /// </summary>
    public int PreviousRoom;

    /// <summary>
    /// Id of the room being transitioned into
    /// </summary>
    public int CurrentRoom;

    /// <summary>
    /// Create event listening to transition from room with ids `prev` to `cur`
    /// </summary>
    /// <param name="prev"></param>
    /// <param name="cur"></param>
    public RoomTransition (int prev, int cur) {
        PreviousRoom = prev;
        CurrentRoom = cur;
    }
    
    public override PlaceMethod Method => PlaceMethod.Append;

    public override string GMLCondition () {
        return $"obj_time.previous_room == {PreviousRoom} && room == {CurrentRoom}";
    }
    
    public override string CodeEntry() {
        return CodeEntryClass.step;
    }

    public override string EventArgs () {
        return $"{PreviousRoom},{CurrentRoom}";
    }
}

/// <summary>
/// Event for being in a room
/// </summary>
class Room : UndertaleEvent {
    /// <summary>
    /// Id of the room to watch
    /// </summary>
    public int RoomId;

    /// <summary>
    /// Create event watching a room
    /// </summary>
    /// <param name="room">Id of the room</param>
    public Room (int room) {
        RoomId = room;
    }

    public override PlaceMethod Method => PlaceMethod.Append;

    public override string GMLCondition () {
        return $"room == {RoomId}";
    }
    
    public override string CodeEntry() {
        return CodeEntryClass.step;
    }

    public override string EventArgs () {
        return RoomId.ToString();
    }
}

/// <summary>
/// Event for leaving a battle
/// </summary>
class LeaveBattle : UniqueEvent {
    public override PlaceMethod Method => PlaceMethod.Append;

    public override string Placement (string code) {
        return @$"
        if (obj_time.previous_room == room_battle && room != room_battle) {{
            // prevent player from getting locked if they TP out of battle
            room_persistent = false;
            {code}
        }}
        ";
    }

    public override string CodeEntry() {
        return CodeEntryClass.step;
    }
}

/// <summary>
/// Event for before entering a battle
/// </summary>
class BeforeBattle : UniqueEvent {
    public override PlaceMethod Method => PlaceMethod.Place;

    public override string Replacement => "battle = 1";

    public override string CodeEntry() {
        return CodeEntryClass.blconAlarm;
    }
}

/// <summary>
/// Event for when Froggit's attack is decided
/// </summary>
class FroggitAttack : UniqueEvent {
    public override PlaceMethod Method => PlaceMethod.Place;

    public override string Replacement => "use_frogskip = 1";

    public override string CodeEntry() {
        return CodeEntryClass.froggitAlarm;
    }
}

/// <summary>
/// Event for when Froggit's turn ends
/// </summary>
class FroggitTurnEnd : UniqueEvent {
    public override PlaceMethod Method => PlaceMethod.PlaceInIf;

    public override string Replacement => "attacked = 0";

    public override string CodeEntry() {
        return CodeEntryClass.froggitStep;
    }
}

/// <summary>
/// Event for when Froggit's turn starts
/// </summary>
class FroggitTurnStart : UniqueEvent {
    public override PlaceMethod Method => PlaceMethod.Place;

    public override string Replacement => "if (global.mnfight == 2)\n{";

    public override string CodeEntry() {
        return CodeEntryClass.froggitStep;
    }
}

/// <summary>
/// Event for when the "YOU WON" message in battle begins
/// </summary>
class YouWon : UniqueEvent {
    public override PlaceMethod Method => PlaceMethod.Place;

    public override string Replacement => "earned \"";

    public override string CodeEntry() {
        return CodeEntryClass.battlecontrol;
    }
}

/// <summary>
/// Event for when a door is touched
/// </summary>
class Door : UndertaleEvent {
    /// <summary>
    /// Name of the door to listen to
    /// </summary>
    public string Name;

    /// <summary>
    /// Name of the room to listen for the door touch
    /// </summary>
    public int Room;
    
    /// <summary>
    /// Creates event for touching a door in a room
    /// </summary>
    /// <param name="name">Name of the door</param>
    /// <param name="room">Room id</param>
    public Door (string name, int room) {
        Name = name;
        Room = room;
    }

    public override PlaceMethod Method => PlaceMethod.Append;

    public override string GMLCondition () {
        return $"room == {Room}";
    }
    
    public override string CodeEntry () {
        if (Name == "A") {
            return CodeEntryClass.doorA;
        }
        if (Name == "C") {
            return CodeEntryClass.doorC;
        }
        if (Name == "Amusic") {
            return CodeEntryClass.doorAmusic;
        }
        return "";
    }

    public override string EventArgs () {
        return $"{Name},{Room}";
    }
}

/*
STAGE CLASSES
*/

/// <summary>
/// Class for a stage in a session
/// </summary>
class Stage {
    /// <summary>
    /// All event listeners in the current stage
    /// </summary>
    public Listener[] Listeners;

    /// <summary>
    /// The drawn message associated with the stage
    /// </summary>
    public string Message;

    /// <summary>
    /// Default creator
    /// </summary>
    public Stage () {}

    /// <summary>
    /// Create stage with a message and event listeners
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="listeners"></param>
    public Stage (string msg, params Listener[] listeners) {
        Listeners = listeners;
        Message = msg;
    }
}

/// <summary>
/// Class for stages with the message "PROCEED'
/// </summary>
class ProceedStage : Stage {
    /// <summary>
    /// Create stage with listeners
    /// </summary>
    /// <param name="listeners"></param>
    public ProceedStage (params Listener[] listeners) : base(@"
PROCEED
    ", listeners) {}
}

/// <summary>
/// Class for stages with the message "WALK"
/// </summary>
class WalkStage : Stage {
    /// <summary>
    /// Create stage with listeners
    /// </summary>
    /// <param name="listeners"></param>
    public WalkStage (params Listener[] listeners) : base(@"
WALK
    ", listeners) {}
}

/// <summary>
/// Class for representing an encounter inside a grind
/// </summary>
class EncounterName {
    /// <summary>
    /// Identifier name to save the encounter as
    /// </summary>
    public string Name;

    /// <summary>
    /// Battlegroup for the encounter
    /// </summary>
    public int Battlegroup;

    /// <summary>
    /// If applicable, the name of the segment that encompasses fighting the encounter
    /// </summary>
    public string SegmentName;

    /// <summary>
    /// Should be set to `true` if the timer will be running at the end of the encounter (and should be stopped)
    /// and `false` otherwise
    /// </summary>
    public bool RunningTimer;

    /// <summary>
    /// Initialize the variables
    /// </summary>
    /// <param name="name"></param>
    /// <param name="battlegroup"></param>
    /// <param name="segmentName"></param>
    /// <param name="runningTimer"></param>
    private void init (string name, int battlegroup, string segmentName, bool runningTimer) {
        Name = name;
        Battlegroup = battlegroup;
        SegmentName = segmentName;
        RunningTimer = runningTimer;
    }

    /// <summary>
    /// Create encounter name for a battle that has a segment
    /// </summary>
    /// <param name="name"></param>
    /// <param name="battlegroup"></param>
    /// <param name="segmentName"></param>
    public EncounterName (string name, int battlegroup, string segmentName) {
        init(name ,battlegroup, segmentName, true);
    }

    /// <summary>
    /// Create encounter name for a battle without a segment
    /// </summary>
    /// <param name="name"></param>
    /// <param name="battlegroup"></param>
    /// <param name="runningTimer"></param>
    public EncounterName (string name, int battlegroup, bool runningTimer) {
        init(name, battlegroup, "", runningTimer);
    }
}

/// <summary>
/// Class for a timed transition in a grind
/// </summary>
class GrindTransition {
    /// <summary>
    /// Room that the transition starts
    /// </summary>
    public int OriginRoom;

    /// <summary>
    /// Room that the transition ends
    /// </summary>
    public int DestinationRoom;

    /// <summary>
    /// Condition for starting and ending the segment timer for this condition
    /// </summary>
    public string Condition;

    /// <summary>
    /// Segment name associated with the transition
    /// </summary>
    public string SegmentName;

    /// <summary>
    /// Create timed transition
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="destination"></param>
    /// <param name="condition"></param>
    /// <param name="segmentName"></param>
    public GrindTransition (int origin, int destination, string condition, string segmentName) {
        OriginRoom = origin;
        DestinationRoom = destination;
        Condition = condition;
        SegmentName = segmentName;
    }
}

/// <summary>
/// Class for a timed grind transition that always runs in a specific `current_encounter` value
/// </summary>
class StaticGrindTransition : GrindTransition {
    /// <summary>
    /// Number of the encounter the transition runs
    /// </summary>
    public int EncounterNumber;

    /// <summary>
    /// Create static grind transition with the number of the `current_encounter` to watch
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="destination"></param>
    /// <param name="number">Number of the `current_encounter` the transition should play</param>
    /// <param name="segmentName"></param>
    public StaticGrindTransition (int origin, int destination, int number, string segmentName) : base(
        origin,
        destination,
        $"(obj_time.current_encounter == {number})",
        segmentName
    ) {}
}

/// <summary>
/// Class for a stage that carries out a grind
/// </summary>
class GrindStage : Stage {
    /// <summary>
    /// Create stage for the grind
    /// </summary>
    /// <param name="encounters">Array with all the encounters in the grind</param>
    /// <param name="transitions">Array with all timed transitions in the grind</param>
    /// <param name="victoryCode">GML code to run at the start of the leave battle event</param>
    /// <param name="msg">Message to be displayed in the session message</param>
    /// <param name="arr">Name of the array with the encounters for the grind</param>
    /// <param name="tpRoom">Room to teleport at the end of the grind</param>
    /// <param name="tpX">X position to teleport at the end of the grind</param>
    /// <param name="tpY">Y position to teleport at the end of the grind</param>
    /// <param name="disableOnEnd">
    /// Should be set to `true` if the encounters should be disabled at the end of the encounter and `false` otherwise
    /// </param>
    /// <param name="customListeners">Extra listeners to add to the stage</param>
    public GrindStage (EncounterName[] encounters, GrindTransition[] transitions, string victoryCode, string msg, string arr, int tpRoom, int tpX, int tpY, bool disableOnEnd, params Listener[] customListeners) {
        // using a list here to later convert to array
        var listeners = new List<Listener>();

        // add given ones
        listeners.AddRange(customListeners);

        // these two maps will be used for a variable switch (see GMLCodeClass)
        
        // map of encounter name to segment names
        var segmentMap = new Dictionary<string, string>();
        
        // map of encounter name to battlegroup
        var rigMap = new Dictionary<string, string>();

        // all conditions required to stop the time for the current encounter
        // it will just be used to filter out the encounters that don't stop the timer at the end
        var stopTimeConditions = new List<string>();

        // code that will (or will not) disable encounters
        var disableEncounters = disableOnEnd ? GMLCodeClass.DisableEncounters : "";

        foreach (EncounterName encounter in encounters) {
            // converting to a GML string representation since it will be used in the switches
            var stringName = GMLCodeClass.GMLString(encounter.Name);
            if (encounter.SegmentName != "") segmentMap[stringName] = GMLCodeClass.GMLString(encounter.SegmentName);

            if (!encounter.RunningTimer) stopTimeConditions.Add($"(encounter_name != '{encounter.Name}')");

            rigMap[stringName] = encounter.Battlegroup.ToString();
        }

        var stopTimeCondition = String.Join(" && ", stopTimeConditions);

        // no stop conditions means it should always stop
        if (stopTimeCondition == "") stopTimeCondition = "1";

        // saving all the transitions to add the code for starting the transition timer
        var transitionVictoryCode = new IfElseBlock();

        foreach (GrindTransition transition in transitions) {
            transitionVictoryCode.AddIfBlock($@"
            if ({transition.Condition}) {{
                {GMLCodeClass.StartSegment(transition.SegmentName)}
            }}
            ");

            listeners.Add(
                new Listener(
                    new RoomTransition(transition.OriginRoom, transition.DestinationRoom),
                    new Callback(
                        @$"
                        if ({transition.Condition}) {{
                            {GMLCodeClass.StopTime}
                        }}
                        "
                    )
                )             
            );
        }

        var encounterName = GMLCodeClass.WithEncounterName(arr, "");

        listeners.AddRange(
            new Listener [] {
                new Listener(
                    new EnterBattle(),
                    new Callback(
                        @$"
                        {encounterName}
                        
                        var name = 0;
                        {GMLCodeClass.GetVariableSwitch(segmentMap, "encounter_name", "name")}

                        if (name != 0) {{
                            {GMLCodeClass.StartSegment("name", true)}
                        }}
                        "
                    )
                ),
                new Listener(
                    new LeaveBattle(),
                    new Callback(
                        encounterName,
                        victoryCode,
                        @$"
                        if ({stopTimeCondition}) {{
                            {GMLCodeClass.StopTime}
                        }}

                        obj_time.current_encounter++;
                        if (obj_time.current_encounter == {encounters.Length}) {{
                            {disableEncounters}
                            {GMLCodeClass.TPTo(tpRoom, tpX, tpY)}
                            obj_time.stage++;
                        }}
                        ",
                        transitionVictoryCode.GetCode()
                    )
                ),
                new Listener(
                    new BeforeBattle(),
                    new Callback(
                        encounterName,
                        GMLCodeClass.GetVariableSwitch(rigMap, "encounter_name", "global.battlegroup")
                    )
                )
            }  
        );

        Listeners = listeners.ToArray();
        Message = msg;
    }
}

/*
GENERAL CLASSES FOR THE SESSIONS
*/

/// <summary>
/// Class representing a recording session
/// </summary>
class Session {
    /// <summary>
    /// All stages that compose the session
    /// </summary>
    public Stage[] Stages;

    /// <summary>
    /// Create session with stages
    /// </summary>
    /// <param name="stages"></param>
    public Session (params Stage[] stages) {
        Stages = stages;
    }
}

/// <summary>
/// Class for the GML code executed after an event is fired
/// </summary>
class Callback {
    /// <summary>
    /// Code that will be executed
    /// </summary>
    public string GMLCode;

    /// <summary>
    /// Create callback with arbitrary code blocks
    /// </summary>
    /// <param name="code"></param>
    public Callback (params string[] code) {
        GMLCode = "\n" + String.Join("\n", code) + "\n";
    }
}

/// <summary>
/// Class representing the listener for an event
/// </summary>
class Listener {
    /// <summary>
    /// Event being watched
    /// </summary>
    public UndertaleEvent Event;

    /// <summary>
    /// Code to execute after the event is fired
    /// </summary>
    public Callback ListenerCallback; 

    /// <summary>
    /// Create an event listener with an event and a callback
    /// </summary>
    /// <param name="undertaleEvent"></param>
    /// <param name="callback"></param>
    public Listener (UndertaleEvent undertaleEvent, Callback callback) {
        Event = undertaleEvent;
        ListenerCallback = callback;
    }
}

/*
STAGE DEFINITIONS
*/

/// <summary>
/// Before the session begins
/// </summary>
var offline = new Stage(
    @"
RECORDING SESSION WAITING TO START
To start it, begin a normal run,
and keep playing until the mod stops you
    ",
    new Listener(
        new PickName(),
        new Callback(
            GMLCodeClass.StartSession,
            GMLCodeClass.StartSegment("ruins-start"),
            GMLCodeClass.Next
        )
    )
);

/// <summary>
/// Normal run from the start up to the first froggit
/// </summary>
var ruinsStart = new ProceedStage(
    new Listener(
        new Blcon(),
        new Callback(
            GMLCodeClass.StopTime
        )
    ),
    new Listener(
        new EnterBattle(3),
        new Callback(
            GMLCodeClass.StartSegment("ruins-hallway"),
            GMLCodeClass.Next
        )
    )
);

/// <summary>
/// Normal run from first froggit up to long hallway exit
/// </summary>
var ruinsHallway = new ProceedStage(
    new Listener(
        new RoomTransition(11, 12),
        new Callback(
            GMLCodeClass.StopTime,
            GMLCodeClass.Next,
            GMLCodeClass.TPTo(11, 2400, 80)
        )
    )
);

/// <summary>
/// Explanation before leaf pile
/// </summary>
var preLeafPile = new Stage(
    @"
Next, walk through the next room
as quickly as possible
    ",
    new Listener(
        new RoomTransition(11, 12),
        new Callback(
            GMLCodeClass.StartDowntime("ruins-leafpile", 97),
            GMLCodeClass.Next
        )
    )
);

/// <summary>
/// Walking across the leaf pile room
/// </summary>
var leafPileDowntime = new WalkStage(
    new Listener(
        new Door("C", 12),
        new Callback(
            GMLCodeClass.StopDowntime,
            GMLCodeClass.EnableEncounters,
            GMLCodeClass.Next
        )
    )
);

/// <summary>
/// Explanation before first half grind
/// </summary>
var preFirstGrind = new Stage(
    @"
Now, grind and encounter at the end of
the room and continue grinding as if you were
in a normal run
    ",
    new Listener(
        new Room(14),
        new Callback(
            GMLCodeClass.TPTo(12, 180, 260)
        )
    ),
    new Listener(
        new Blcon(),
        new Callback(
            GMLCodeClass.Next
        )
    )
);

/// <summary>
/// Minified version of the first half grind
/// </summary>
var inFirstGrind = new GrindStage(
    new EncounterName[] {
        new EncounterName("W", 5, "whim"),
        new EncounterName("F", 4, false),
        new EncounterName("N", 4, true),
        new EncounterName("2", 4, "froggit-lv2"),
        new EncounterName("3", 4, "froggit-lv3")
    },
    new GrindTransition[] {
        new StaticGrindTransition(12, 14, 1, "leaf-pile-transition"),
        new StaticGrindTransition(14, 12, 2, "ruins-first-transition")
    },
    @$"
    // leave the player high enough XP for guaranteed LV up next encounter if just fought the LV 2 encounter
    if (encounter_name == '2') {{
        global.xp = 29;
    }}
    ",
    @"
GRIND
    ",
    "first_half_encounters",
    12,
    220,
    320,
    true,
    new Listener(
        new FroggitAttack(),
        new Callback(
            GMLCodeClass.UnrigFrogskip
        )
    ),
    new Listener(
        new FroggitTurnStart(),
        new Callback(
            GMLCodeClass.TimeFrogTurn
        )
    ),
    new Listener(
        new FroggitTurnEnd(),
        new Callback(
            GMLCodeClass.EndFrogTime
        )
    ),
    new Listener(
        new YouWon(),
        new Callback(
            GMLCodeClass.TimeYouWon
        )
    )
);

/// <summary>
/// Explanation after the first half grind
/// </summary>
var postFirstGrind = new Stage(
    @"
Now, walk to the right room
and simply cross it (don't grind)
    ",
    new Listener(
        new LeaveBattle(),
        new Callback(
            GMLCodeClass.TPTo(12, 240, 340)
        )
    ),
    new Listener(
        new RoomTransition(12, 14),
        new Callback(
            GMLCodeClass.StartDowntime("ruins-leaf-fall"),
            GMLCodeClass.Next
        )
    )
);

/// <summary>
/// Walking across the leaf fall room
/// </summary>
var leafFallDowntime = new WalkStage(
    new Listener(
        new Door("A", 14),
        new Callback(
            GMLCodeClass.StopDowntime,
            GMLCodeClass.EnableEncounters,
            GMLCodeClass.Next
        )
    )
);

/// <summary>
/// Explanation after leaf fall downtime
/// </summary>
var preFallEncounter = new Stage(
    @"
Now, grind an encounter at
the end of this room and proceed as if it
were a normal run until you are stopped
    ",
    new Listener(
        new Room(15),
        new Callback(
            GMLCodeClass.TPTo(14, 210, 100)
        )
    ),
    new Listener(
        new Blcon(),
        new Callback(
            GMLCodeClass.Next
        )
    )
);

/// <summary>
/// Grinding encounter in the leaf fall room
/// </summary>
var inFalLEncounter = new ProceedStage(
    new Listener(
        new BeforeBattle(),
        new Callback(
            GMLCodeClass.RigWhimsun
        )
    ),
    new Listener(
        new LeaveBattle(),
        new Callback(
            GMLCodeClass.StartSegment("leaf-fall-transition"),
            GMLCodeClass.Next
        )
    )
);

/// <summary>
/// Exitting leaf fall room
/// </summary>
var leafFallTransition = new ProceedStage(
    new Listener(
        new RoomTransition(14, 15),
        new Callback(
            GMLCodeClass.StopTime,
            GMLCodeClass.DisableEncounters,
            GMLCodeClass.TPTo(14, 200, 80),
            GMLCodeClass.Next
        )
    )
);

/// <summary>
/// Explanation before one rock room
/// </summary>
var preOneRock = new Stage(
    @"
Now, go through the next room
from beginning to end as if it was a
normal run but without grinding an encounter
at the end
    ",
    new Listener(
        new RoomTransition(14, 15),
        new Callback(
            GMLCodeClass.StartDowntime("ruins-one-rock"),
            GMLCodeClass.Next
        )
    )
);

/// <summary>
/// Walking across the one rock room
/// </summary>
var oneRockDowntime = new WalkStage(
    new Listener(
        new Door("A", 15),
        new Callback(
            GMLCodeClass.StopDowntime,
            GMLCodeClass.EnableEncounters,
            GMLCodeClass.Next
        )
    )
);

/// <summary>
/// Explanation before the leaf maze
/// </summary>
var preLeafMaze = new Stage(
    @"
Now grind at the
end of the room, and proceed as a normal
run until you are stopped
    ",
    new Listener(
        new Room(16),
        new Callback(
            GMLCodeClass.TPTo(15, 340, 100)
        )
    ),
    new Listener(
        new Blcon(),
        new Callback(
            GMLCodeClass.Next
        )
    )
);

/// <summary>
/// Grinding encounter at the end of one rock room
/// </summary>
var oneRockEncounter = new ProceedStage(
    new Listener(
        new BeforeBattle(),
        new Callback(
            GMLCodeClass.RigWhimsun
        )
    ),
    new Listener(
        new LeaveBattle(),
        new Callback(
            GMLCodeClass.DisableEncounters,
            GMLCodeClass.StartSegment("ruins-maze"),
            GMLCodeClass.Next
        )
    )
);

/// <summary>
/// Walking across the leaf maze room
/// </summary>
var inLeafMaze = new ProceedStage(
    new Listener(
        new RoomTransition(16, 17),
        new Callback(
            GMLCodeClass.StopTime,
            GMLCodeClass.DisableEncounters,
            GMLCodeClass.TPTo(16, 520, 220),
            GMLCodeClass.Next
        )
    )
);

/// <summary>
/// Explanation before three rock room
/// </summary>
var preThreeRock = new Stage(
    @"
Now, go through the next
room from begining to end as if it was
a normal run but without grinding an encounter
at the end
    ",
    new Listener(
        new RoomTransition(16, 17),
        new Callback(
            GMLCodeClass.StartDowntime("ruins-three-rock"),
            GMLCodeClass.Next
        )
    )
);

/// <summary>
/// Walking across three rock room
/// </summary>
var threeRockDowntime = new WalkStage(
    new Listener(
        new Door("A", 17),
        new Callback(
            GMLCodeClass.StopDowntime,
            GMLCodeClass.EnableEncounters,
            GMLCodeClass.Next
        )
    )
);

/// <summary>
/// Explanation before second half grinding
/// </summary>
var preSecondGrind = new Stage(
    @"
Now, grind an
encounter at the end of the room,
and proceed grinding and killing
encounters until you are stopped
Grind as you would in a normal
run
    ",
    new Listener(
        new Room(18),
        new Callback(
            GMLCodeClass.TPTo(17, 430, 110)
        )
    ),
    new Listener(
        new Blcon(),
        new Callback(
            GMLCodeClass.ResetEncounters,
            GMLCodeClass.Next
        )
    )
);

/// <summary>
/// Grinding encounters (killing every enemy) in the second half
/// </summary>
///
var inSecondGrind = new GrindStage(
    new EncounterName[] {
        new EncounterName("F", 9, "dbl-froggit"),
        new EncounterName("W", 6, "frog-whim"),
        new EncounterName("A", 7, "sgl-mold"),
        new EncounterName("B", 10, "dbl-mold"),
        new EncounterName("C", 8, "tpl-mold")
    },
    new StaticGrindTransition[] {
        new StaticGrindTransition(17, 18, 1, "three-rock-transition"),
        new StaticGrindTransition(17, 18, 2, "ruins-second-transition")
    },
    "",
    "GRIND",
    "second_half_encounters",
    18,
    40,
    110,
    false
);


/// <summary>
/// Explanation before second half grinding with fleeing
/// </summary>
var preFleeGrind = new Stage(
    @"
Now, continue grinding
just the same, but as if you had 19 kills,
that is, flee after killing the
first enemy for ALL encounters
    ",
    new Listener(
        new Blcon(),
        new Callback(
            GMLCodeClass.ResetEncounters,
            GMLCodeClass.Next
        )
    )
);


/// <summary>
/// Grinding encounters (killing one enemy and fleeing) in the second half
/// </summary>
var inFleeGrind = new GrindStage(
    new EncounterName[] {
        new EncounterName("F", 9, "dbl-froggit-19"),
        new EncounterName("W", 6, "frog-whim-19"),
        new EncounterName("B", 10, "dbl-mold-19"),
        new EncounterName("C", 8, "tpl-mold-19")
    },
    new StaticGrindTransition[] {},
    "",
    "GRIND (KILL ONLY ONE)",
    "second_half_flee_encounters",
    18,
    40,
    110,
    false
);

/// <summary>
/// Explanation before triple moldsmal
/// </summary>
var preTripleMold = new Stage(
    @"
Now, kill one last encounter
it will be a triple mold, and you must only
kill TWO monsters, then flee
Feel free to still attack second one
to simulate the Froggit Whimsun attacks
    ",
    new Listener(
        new Blcon(),
        new Callback(
            GMLCodeClass.Next
        )
    )
);

/// <summary>
/// Battling triple moldsmal killing two enemies and fleeing
/// </summary>
var inTripleMold = new Stage(
    @"
GRIND (KILL ONLY TWO)
    ",
    new Listener(
        new BeforeBattle(),
        new Callback(
            GMLCodeClass.RigTripleMold
        )
    ),
    new Listener(
        new EnterBattle(),
        new Callback(
            GMLCodeClass.StartSegment("tpl-mold-18")
        )
    ),
    new Listener(
        new LeaveBattle(),
        new Callback(
            GMLCodeClass.StopTime,
            GMLCodeClass.TPTo(17, 500, 110),
            GMLCodeClass.MaxRuinsKills,
            GMLCodeClass.Next
        )
    )
);

/// <summary>
/// Explanation before the end
/// </summary>
var ruinsPreEnd = new Stage(
    @"
Finally, walk to the right as if
you have finished killing all
monsters and play normally until
the end of Ruins
    ",
    new Listener(
        new Door("A", 17),
        new Callback(
            GMLCodeClass.StartSegment("ruins-napsta"),
            GMLCodeClass.Next
        )
    )
);

/// <summary>
/// Walking across rooms with "But nobody came"
/// </summary>
var ruinsNobodyCame = new ProceedStage(
    new Listener(
        new Blcon(),
        new Callback(
            GMLCodeClass.StopTime
        )
    ),
    new Listener(
        new EnterBattle(),
        new Callback(
            GMLCodeClass.NobodyCameSegments
        )
    )
);

/// <summary>
/// Going to the end of the Ruins
/// </summary>
var ruinsEnd = new ProceedStage(
    new Listener(
        new Door("Amusic", 41),
        new Callback(
            GMLCodeClass.StopTime,
            GMLCodeClass.Next
        )
    )
);

/// <summary>
/// After the session is finished
/// </summary>
var sessionFinished = new Stage(
    @"
Session finished!
    "
);

/*
SESSION DEFINITIONS
*/

/// <summary>
/// Session for timing all segments in Ruins
/// </summary>
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
    preFallEncounter,
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
    ruinsEnd,
    sessionFinished
);

/******
start of main script
******/

// testing script-game compatibility
EnsureDataLoaded();

if (Data?.GeneralInfo?.DisplayName?.Content.ToLower() != "undertale") {
    ScriptError("Error 0: Script must be used in Undertale");
}

// make drawing work
Data.GameObjects.ByName("obj_time").Visible = true;

// initializing variables
append(CodeEntryClass.create, $@"
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

{GMLCodeClass.RandomPopulateArray("first_half_encounters", "W", "F", "N")}

// ruins first half encounters array guide:
// W: whimsun
// 2: Froggit at LV2
// 3: Froggit at LV3
// F: Froggit with frogskip
// N: Froggit without frogskip
// A: any (random encounter)

second_half_encounters[4] = 0;
second_half_flee_encounters[3] = 0;

{GMLCodeClass.RandomPopulateArray("second_half_encounters", "W", "F", "A", "B", "C")}
{GMLCodeClass.RandomPopulateArray("second_half_flee_encounters", "W", "F", "B", "C")}
"
// ruins second half encounters array guide:
// W: frog whim (2 times)
// F: 2x frog (2 times)
// A: 1x mold (1 time)
// B: 2x mold (2 times)
// C: 3x mold (3 times)
);

// add switch for enabling and disabling encounters
replace(CodeEntryClass.scrSteps, @"
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

// room tracker, widely used for room transition events
append(CodeEntryClass.step, @"
previous_room = current_room;
current_room = room;
");

// in order to tp to the proper places, will be using the tp flag which notifies the
// next frame that a room teleportation was carried out last frame
// and we have a specific x,y position to go to
append(CodeEntryClass.step, @$"
// use two flags to wait a frame
// wait a frame to overwrite the default position
if (tp_flag) {{
    tp_flag = 0;
    // to avoid the player moving to places they don't want to
    // player will be locked until they stop moving after teleporting
    if ({GMLCodeClass.IsMoving}) {{
        lock_player = 1;
        global.interact = 1;
    }}
    obj_mainchara.x = tp_x;
    obj_mainchara.y = tp_y;
    // previous x and y must be updated too due to how obj_mainchara's collision events work
    obj_mainchara.xprevious = tp_x;
    obj_mainchara.yprevious = tp_y;
}} else if (lock_player && !({GMLCodeClass.IsMoving})) {{
    lock_player = 0;
    global.interact = 0;
}}
");

// downtime timer api
append(CodeEntryClass.step, @"
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
append(CodeEntryClass.draw, @"
draw_set_font(fnt_main)
draw_set_color(c_yellow);
draw_text(20, 0, current_msg);
");

// code that assigns the current_msg variable
// TO-DO: Current only takes for ruins. Refactor once more sessions are supported
var messageList = new List<string>();
for (int i = 0; i < ruinsScript.Stages.Length; i++) {
    messageList.Add($@"
    if (stage == {i}) {{
        current_msg = ""{ruinsScript.Stages[i].Message}"";
    }}
    ");
}

append(CodeEntryClass.step, IfElseBlock.GetIfElseBlock(messageList));

// rigging frogskip for all froggit encounters to speed up practice
// it is placed right after mycommand declaration
place(CodeEntryClass.froggitAlarm, "0))", @$"
// as a means to speed up practice, all of them will have frog skip by default
var use_frogskip = 1;
if (use_frogskip) {{
    mycommand = 0;
}} else {{
    mycommand = 100;
}}
");

// placing all listeners for all stages

// first, create a map of all unique events mapped to a map of all stages and their respective code
var events = new Dictionary<UndertaleEvent, Dictionary<int, Callback>>();

// only ruinScript supported for now
// TO-DO: add other session support
for (int i = 0; i < ruinsScript.Stages.Length; i++) {
    var stage = ruinsScript.Stages[i];
    foreach (Listener listener in stage.Listeners) {
        if (!events.ContainsKey(listener.Event)) {
            events[listener.Event] = new Dictionary<int, Callback>();
        }
        events[listener.Event][i] = listener.ListenerCallback;
    }
}

// then, group all the event types and their unique events

// this is a map of event names to a map of unique events and their respective code
var eventCodes = new Dictionary<string, Dictionary<UndertaleEvent, string>>();

// the code for each unique event is just an if-else block separating each of the stages
foreach (UndertaleEvent undertaleEvent in events.Keys) {
    var stageCodeBlocks = new List<string>();
    Dictionary<int, Callback> eventStages = events[undertaleEvent];
    foreach (int stage in eventStages.Keys) {
        stageCodeBlocks.Add(@$"
        if (obj_time.stage == {stage}) {{
            {eventStages[stage].GMLCode}
        }}
        ");

    }
    string eventCode = IfElseBlock.GetIfElseBlock(stageCodeBlocks);
    var eventName = undertaleEvent.GetType().Name;

    if (!eventCodes.ContainsKey(eventName)) eventCodes[eventName] = new Dictionary<UndertaleEvent, string>();
    eventCodes[eventName][undertaleEvent] = eventCode;
}

// finally, go through each event type, and determine how the code will be placed
foreach (string eventName in eventCodes.Keys) {
    // get the map of unique events to their code
    var eventMap = eventCodes[eventName];
    
    // pick any of the events (in this case the first) to access the general methods of this event type
    // regarding how to place the code
    UndertaleEvent baseEvent = eventMap.Keys.ToList()[0];

    // split the events based on the code entries they edit
    // so create a map of code entries to the if-else block for the events in the code entry
    // in short, since the unique events are already separated, we presume that we want to access only one of them
    // and that is filtered by doing an if-else block in all of their conditions
    var entryMap = new Dictionary<string, IfElseBlock>();

    foreach (UndertaleEvent undertaleEvent in eventMap.Keys) {
        var eventCode = eventMap[undertaleEvent];
        var eventEntry = undertaleEvent.CodeEntry();
        if (!entryMap.ContainsKey(eventEntry)) {
            entryMap[eventEntry] = new IfElseBlock();
        }
        var condition = undertaleEvent.GMLCondition();
        if (condition == "1") {
            entryMap[eventEntry].SetElseBlock(eventCode);
        } else {    
            entryMap[eventEntry].AddIfBlock(@$"
            if ({undertaleEvent.GMLCondition()}) {{
                {eventCode}
            }}
            ");
        }
    }

    foreach (string entry in entryMap.Keys) {
        string code = entryMap[entry].GetCode();

        switch (baseEvent.Method) {        
            case PlaceMethod.Append:
                append(entry, baseEvent.Placement(code));
                break;
            case PlaceMethod.Place:
                place(entry, baseEvent.Replacement, baseEvent.Placement(code));
                break;
            case PlaceMethod.PlaceInIf:
                placeInIf(entry, baseEvent.Replacement, baseEvent.Placement(code));
                break;
        }
    }
}

// debug mode - REMOVE FOR PRODUCTION BUILD!!
useDebug();

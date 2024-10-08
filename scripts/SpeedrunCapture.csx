using System.Linq;
using System.Xml;
using System.Reflection;

/******
helper classes
******/

/// <summary>
/// Class for getting the assembly name of classes
/// </summary>
static class AssemblyNameClass
{
    /// <summary>
    /// Get what the assembly qualified name for a type is (assuming the type exists)
    /// </summary>
    /// <param name="typeName"></param>
    /// <returns></returns>
    public static string GetAssemblyName (string typeName)
    {
        return typeof(AssemblyNameClass).AssemblyQualifiedName.Replace("AssemblyNameClass", typeName);
    }
}

/// <summary>
/// Class for a bool attribute in the segments XML
/// </summary>
static class XmlBool
{
    /// <summary>
    /// Thrown if an attribute is not a boolean
    /// </summary>
    private class XmlBoolException : Exception
    {
        public XmlBoolException(string attribute) : base($"Bool attribute \"{attribute}\" was not \"true\" or \"false\"") {}
    }

    /// <summary>
    /// Read a XML attribute as a bool
    /// </summary>
    /// <param name="reader">Reader with the node that has the value</param>
    /// <param name="attribute">Name of the attribute</param>
    /// <returns></returns>
    /// <exception cref="XmlBoolException"></exception>
    public static bool ParseXmlBool (XmlReader reader, string attribute)
    {
        var value = reader.GetAttribute(attribute);
        if (value == "true") return true;
        if (value == "false") return false;
        throw new XmlBoolException(attribute);
    }
}

/// <summary>
/// The type for a segment
/// </summary>
class SegmentType
{
    /// <summary>
    /// Variable in Undertale that says the segment is running
    /// </summary>
    public string RunningVariable;

    /// <summary>
    /// Undertale code that will start the segment
    /// </summary>
    public string StartCode;

    /// <summary>
    /// Undertale code that will end the segment
    /// </summary>
    public string EndCode;

    /// <summary>
    /// Thrown if the segment type from the XML is invalid
    /// </summary>
    private class SegmentTypeException : Exception
    {
        public SegmentTypeException(string type) : base($"\"{type}\" is not a valid segment type") {}
    }

    /// <summary>
    /// Default constructor
    /// </summary>
    public SegmentType () {}

    /// <summary>
    /// Build type from the "type" attribute from the XML segments
    /// </summary>
    /// <param name="xmlName">Value of the type attribute</param>
    /// <param name="segmentName">Name of the segment</param>
    /// <exception cref="SegmentTypeException"></exception>
    public SegmentType (string xmlName, string segmentName)
    {
        Console.WriteLine(xmlName);
        if (xmlName == "continuous")
        {
            RunningVariable = "obj_time.is_timer_running";
            StartCode = GMLCodeClass.StartSegment(segmentName);
            EndCode = GMLCodeClass.StopTime;
        }
        else if (xmlName == "downtime" || xmlName == "downtime-step")
        {
            RunningVariable = "obj_time.is_downtime_mode";
            StartCode = GMLCodeClass.StartDowntime(segmentName);
            EndCode = GMLCodeClass.StopDowntime(xmlName == "downtime-step");
        }
        else if (xmlName == "step")
        {
            RunningVariable = "obj_time.is_step_counting";
            StartCode = GMLCodeClass.StartStepCount(segmentName);
            EndCode = GMLCodeClass.StopStepCount;
        }
        else throw new SegmentTypeException(xmlName);
    }   
}

/// <summary>
/// A time segment to be recorded
/// </summary>
class Segment
{
    /// <summary>
    /// Its type
    /// </summary>
    public SegmentType Type;

    /// <summary>
    /// The segment previous to this one
    /// </summary>
    public Segment Previous = null;

    /// <summary>
    /// The segment next to this one
    /// </summary>
    public Segment Next = null;

    /// <summary>
    /// Name of the segment
    /// </summary>
    public string Name = null;

    /// <summary>
    /// Event that triggers the segment to start
    /// </summary>
    public UndertaleEvent Start = null;

    /// <summary>
    /// Event that triggers the segment to end
    /// </summary>
    public UndertaleEvent End = null;

    /// <summary>
    /// Other events that can be triggered during the segment execution
    /// </summary>
    public List<UndertaleEvent> Other = new List<UndertaleEvent>();

    /// <summary>
    /// Should be `true` if the segment can be continued as if it was a "normal run" after its end, and `false` if it
    /// would require some setup such as going back the room
    /// </summary>
    public bool Uninterruptable = false;

    /// <summary>
    /// Message to display while the segment is being played
    /// </summary>
    public string Message = "";

    public string Explanation = "";

    /// <summary>
    /// Should be set to `true` if an explanation is needed for what should be done, and `false` if it is the same
    /// as a normal run and no special explanation is needed
    /// </summary>
    public bool Tutorial = false;

    /// <summary>
    /// Should be set to `true` if encounters should be enabled, and `false` if it shouldn't or it does't matter
    /// </summary>
    public bool FastEncounters = false;

    /// <summary>
    /// If not 0, the number of steps required for the next encounter
    /// </summary>
    public int NextStepCount = 0;

    /// <summary>
    /// String representing the number of the plot value at the start (just before) the segment begins
    /// </summary>
    public string Plot = null;

    /// <summary>
    /// Room the segment begins at
    /// </summary>
    public UndertaleRoom Room = null;

    /// <summary>
    /// If applicable, the X position the character should be to start the segment
    /// </summary>
    public int X = 0;

    /// <summary>
    /// If applicable, the Y position the character should be to start the segment
    /// </summary>
    public int Y = 0;

    /// <summary>
    /// The XP the player should have at the start of the segment
    /// </summary>
    public int XP = 0;

    /// <summary>
    /// The battlegroup to rig the next encounter with, or `null` if it doesn't need to be rigged
    /// </summary>
    public Battlegroup? NextEncounter = null;

    /// <summary>
    /// Current murder level at the start of the segment
    /// </summary>
    public int MurderLevel = 0;

    /// <summary>
    /// All items the player holds at the start of the segment
    /// </summary>
    public UndertaleInventory Inventory = new UndertaleInventory();

    /// <summary>
    /// If set to `true`, the phone for SGS will be setup
    /// </summary>
    public bool SetPhone = false;

    /// <summary>
    /// Format the messages as they are read in XML to a readable format
    /// </summary>
    /// <param name="msg"></param>
    /// <returns></returns>
    private string FormatMessage (string msg)
    {
        // to remove single line breaks which are part of the same line
        var output = Regex.Replace(msg, @"\n(?=[^\S\r\n]*[\S])", "");
        // to remove double line breaks which are just a single line break
        output = Regex.Replace(output, @"\n[^\S\r\n]*\n", @"\n");

        // remove all chunks of whitespace with a single space
        output = Regex.Replace(output, @"[^\S\r\n]+", " ");

        var lines = output.Split("\n");
        var trimmedLines = lines.Select(lines => lines.TrimStart());
        var finalLines = new List<string>();
        int currentLine = -1;
        int currentChars = 0;
        int CharLimit = 43;
        // transform from trimmed lines to an array with lines capped at a number of characters
        foreach (string line in trimmedLines)
        {
            currentLine++;
            var words = line.Split(" ");
            bool isFirstWord = true;
            foreach (string word in words)
            {
                if (isFirstWord)
                {
                    isFirstWord = false;
                    finalLines.Add(word);
                    currentChars = word.Length;
                }
                else
                {
                    if (currentChars + word.Length > CharLimit)
                    {
                        currentLine++;
                        currentChars = word.Length;
                        finalLines.Add(word);
                    }
                    else
                    {
                        finalLines[currentLine] += $" {word}";
                        currentChars+= word.Length + 1;
                    }
                }
            }
        }
        return String.Join("\n", finalLines);
    }

    /// <summary>
    /// Get an undertale event from its XML node representation
    /// </summary>
    /// <param name="reader">Reader with the node for the event</param>
    /// <returns></returns>
    private static UndertaleEvent ParseXmlEvent (XmlReader reader)
    {
        var type = System.Type.GetType(AssemblyNameClass.GetAssemblyName(reader.Name));
        UndertaleEvent instance = (UndertaleEvent)Activator.CreateInstance(type, args: new XmlReader[] { reader });
        return instance;
    }

    /// <summary>
    /// Default constructor
    /// </summary>
    public Segment () {}

    /// <summary>
    /// Build segment from the XML node for a segment
    /// </summary>
    /// <param name="reader">Reader with the node for the segment</param>
    /// <param name="previous">Segment previous to this one</param>
    public Segment (XmlReader reader, Segment previous)
    {
        Previous = previous;

        // read attributes
        Uninterruptable = XmlBool.ParseXmlBool(reader, "uninterruptable");
        FastEncounters = XmlBool.ParseXmlBool(reader, "fast-encounters");
        Tutorial = XmlBool.ParseXmlBool(reader, "tutorial");

        var incrementMurder = reader.GetAttribute("increment-murder");
        MurderLevel = incrementMurder == null ? Previous.MurderLevel : Previous.MurderLevel + Int32.Parse(incrementMurder);

        // save type for later use
        var type = reader.GetAttribute("type");

        Inventory.CopyFrom(Previous.Inventory);

        bool finishedSegment = false;
        while (!finishedSegment && reader.Read())
        {
            // skip end ones to avoid the switch bugging out and look out for segment end
            while (reader.NodeType == XmlNodeType.EndElement && !finishedSegment)
            {
                if (reader.Name == "segment") finishedSegment = true;
                else reader.Read();
            }
            switch (reader.Name)
            {
                case "name":
                    reader.Read();
                    Name = reader.Value;
                    break;
                case "start":
                    reader.Read();
                    Start = ParseXmlEvent(reader);
                    break;
                case "end":
                    reader.Read();
                    End = ParseXmlEvent(reader);
                    break;
                case "other":
                    do
                    {
                        reader.Read();
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            Other.Add(ParseXmlEvent(reader));
                        }
                    }
                    while (reader.Name != "other");
                    break;
                case "plot":
                    reader.Read();
                    Plot = reader.Value;
                    break;
                case "room":
                    reader.Read();
                    Room = RoomClass.GetRoom(reader.Value);
                    break;
                case "x":
                    reader.Read();
                    X = Int32.Parse(reader.Value);
                    break;
                case "y":
                    reader.Read();
                    Y = Int32.Parse(reader.Value);
                    break;
                case "xp":
                    reader.Read();
                    XP = Int32.Parse(reader.Value);
                    break;
                case "step-count":
                    reader.Read();
                    NextStepCount = Int32.Parse(reader.Value);
                    break;
                case "message":
                    reader.Read();
                    Message = FormatMessage(reader.Value);
                    break;
                case "explanation":
                    reader.Read();
                    Explanation = FormatMessage(reader.Value);
                    break;
                case "next-encounter":
                    reader.Read();
                    object battlegroup;
                    try
                    {                        
                        Enum.TryParse(typeof(Battlegroup), reader.Value, out battlegroup);
                        Other.Add(new BeforeBattle(
                            GMLCodeClass.RigEncounter((Battlegroup)battlegroup)
                        ));
                    }
                    catch (System.Exception)
                    {     
                        throw new Exception ($"Error reading battlegroup \"{reader.Value}\"");
                    }
                    break;
                case "items":
                    while (reader.Read() && reader.Name != "items")
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            var name = reader.Name;
                            reader.Read();
                            var value = reader.Value;
                            switch (name)
                            {
                                case "get":
                                    object item;
                                    try
                                    {                        
                                        Enum.TryParse(typeof(UndertaleWeapon), value, out item);
                                    }
                                    catch (System.Exception)
                                    {     
                                        throw new Exception ($"Error reading item \"{value}\"");
                                    }
                                    Inventory.PickUp((UndertaleWeapon)item);
                                    break;
                                case "drop":
                                    Inventory.Drop(Int32.Parse(value));
                                    break;
                                case "equip":
                                    Inventory.Equip(Int32.Parse(value));
                                    break;
                            }
                        }
                    }
                    break;
                case "phone":
                    SetPhone = true;
                    break;
            }
        }

        Type = new SegmentType(type, Name);

        // updating boundary event code
        Start.Code += '\n' + Type.StartCode;
        End.Code += '\n' + Type.EndCode;

        // taking from previous if none given because that's a special feature of the XML! (also these values are mandatory)
        if (Plot == null) Plot = Previous.Plot;
        if (Room == null) Room = Previous.Room;
        if (X == 0) X = Previous.X;
        if (Y == 0) Y = Previous.Y;
        if (XP == 0) XP = Previous.XP;
    }
}

/// <summary>
/// Represent a room from Undertale
/// </summary>
class UndertaleRoom
{
    /// <summary>
    /// The room that comes before this room in the "chronological" order
    /// </summary>
    public UndertaleRoom Previous = null;

    /// <summary>
    /// The room that comes after this room in the "chronological" order
    /// </summary>
    public UndertaleRoom Next = null;

    /// <summary>
    /// Should be set to `true` if the room is not a room that needs to be accessed in a normal playthrough
    /// </summary>
    public bool IsSideRoom = false;

    /// <summary>
    /// Id for the room
    /// </summary>
    public int RoomId;

    /// <summary>
    /// Initiate fields
    /// </summary>
    /// <param name="id"></param>
    /// <param name="prev"></param>
    private void Init (int id, UndertaleRoom prev)
    {
        Previous = prev;
        RoomId = id;

        // asserting neighbors correctness
        Previous.Next = this;
    }

    /// <summary>
    /// Default constructor
    /// </summary>
    public UndertaleRoom () {}

    /// <summary>
    /// Initiate room without any neighbors
    /// </summary>
    /// <param name="id"></param>
    public UndertaleRoom (int id)
    {
        Init(id, new UndertaleRoom());
    }

    /// <summary>
    /// Initiate room with a neighbor previous to it
    /// </summary>
    /// <param name="id"></param>
    /// <param name="prev"></param>
    /// <param name="isSide">`true` if this is a side room, `false` otherwise</param>
    public UndertaleRoom (int id, UndertaleRoom neighbor, bool isSide = false)
    {
        if (isSide)
        {
            RoomId = id;
            Next = neighbor;
        }
        else
        {
            Init(id, neighbor);
        }
    }
}

/// <summary>
/// Class containing all the rooms
/// </summary>
static class RoomClass
{
    /// <summary>
    /// Get a room by its name (the name as defined in this class)
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    /// <exception cref="Exception">Room does not exist</exception>
    public static UndertaleRoom GetRoom (string name)
    {
        try
        {
            return (UndertaleRoom) typeof(RoomClass).GetField(name).GetValue(null);
        }
        catch (System.Exception)
        {
            throw new Exception($"Invalid room {name}");
        }
    }
    
    /// <summary>
    /// Game menu room (where you name the character)
    /// </summary>
    public static UndertaleRoom GameMenu = new UndertaleRoom(3);
    
    public static UndertaleRoom RuinsPreDummy = new UndertaleRoom(8);

    public static UndertaleRoom RuinsDummy = new UndertaleRoom(9, RuinsPreDummy);

    /// <summary>
    /// Room where Toriel holds your hand across the spikes
    /// </summary>
    public static UndertaleRoom RuinsSpikeMaze = new UndertaleRoom(10, RuinsDummy);

    /// <summary>
    /// Room with the long hallway and unnecessary tension
    /// </summary>
    public static UndertaleRoom RuinsHallway = new UndertaleRoom(11, RuinsSpikeMaze);

    /// <summary>
    /// Long vertical room adjacent to the monster candy, with the savepoint
    /// </summary>
    public static UndertaleRoom RuinsLeafPile = new UndertaleRoom(12, RuinsHallway);

    /// <summary>
    /// The room where you must fall through the leaves, before the room with the first rock
    /// </summary>
    public static UndertaleRoom RuinsLeafFall = new UndertaleRoom(14, RuinsLeafPile);

    /// <summary>
    /// Room with one rock that needs to be pushed
    /// </summary>
    public static UndertaleRoom RuinsOneRock = new UndertaleRoom(15, RuinsLeafFall);

    /// <summary>
    /// Room with the pit maze where failing makes you fall
    /// </summary>
    public static UndertaleRoom RuinsLeafMaze = new UndertaleRoom(16, RuinsOneRock);

    /// <summary>
    /// Room with three rocks that need to be pushed (though actually only one is required)
    /// </summary>
    public static UndertaleRoom RuinsThreeRock = new UndertaleRoom(17, RuinsLeafMaze);

    /// <summary>
    /// Room with the mouse and cheese in Ruins
    /// </summary>
    public static UndertaleRoom RuinsCheese = new UndertaleRoom(18, RuinsThreeRock);

    /// <summary>
    /// Room where Napstablook is found to be fought in the Ruins
    /// </summary>
    public static UndertaleRoom RuinsNapstablook = new UndertaleRoom(19, RuinsCheese);

    /// <summary>
    /// Room with the three/two/four frogs, where pink names can be acquired
    /// </summary>
    public static UndertaleRoom RuinsThreeFrogs = new UndertaleRoom(21, RuinsNapstablook);

    /// <summary>
    /// Room with the six switches, where the ribbon and Napstablook are found as well
    /// </summary>
    public static UndertaleRoom RuinsSwitches = new UndertaleRoom(22, RuinsThreeFrogs);

    /// <summary>
    /// First room of perspective (no switches to press)
    /// </summary>
    public static UndertaleRoom PerspectiveA = new UndertaleRoom(23, RuinsSwitches);
    
    /// <summary>
    /// Second room of perspective (blue switch to press)
    /// </summary>
    public static UndertaleRoom PerspectiveB = new UndertaleRoom(24, PerspectiveA);
    
    /// <summary>
    /// Third room of perspective (red switch to press)
    /// </summary>
    public static UndertaleRoom PerspectiveC = new UndertaleRoom(25, PerspectiveB);
    
    /// <summary>
    /// Fourth room of perspective (green switch to press)
    /// </summary>
    public static UndertaleRoom PerspectiveD = new UndertaleRoom(26, PerspectiveC);
    
    /// <summary>
    /// Room where Toriel is fought
    /// </summary>
    public static UndertaleRoom RuinsTorielFight = new UndertaleRoom(41);

    /// <summary>
    /// Room at the end of the Ruins, with the Flowey dialogue
    /// </summary>
    public static UndertaleRoom RuinsExit = new UndertaleRoom(43);

    /// <summary>
    /// Room where the mini-credits post Ruins play
    /// </summary>
    public static UndertaleRoom RuinsCredits = new UndertaleRoom(325, RuinsExit);

    /// <summary>
    /// First room of Snowdin, outside the Ruins door
    /// </summary>
    public static UndertaleRoom SnowdinRuinsDoor = new UndertaleRoom(44, RuinsCredits);

    /// <summary>
    /// Room where Frisk hides behind a conveniently-shaped lamp
    /// </summary>
    public static UndertaleRoom SnowdinConvenientLamp = new UndertaleRoom(45, SnowdinRuinsDoor);

    /// <summary>
    /// First room with the box and savepoint in Snowdin
    /// </summary>
    public static UndertaleRoom SnowdinBoxRoad = new UndertaleRoom(46, SnowdinConvenientLamp);

    /// <summary>
    /// Room where Papyrus finds a rock and a human
    /// </summary>
    public static UndertaleRoom SnowdinHumanRock = new UndertaleRoom(48, SnowdinBoxRoad);

    /// <summary>
    /// Room where Doggo is fought
    /// </summary>
    public static UndertaleRoom SnowdinDoggo = new UndertaleRoom(49, SnowdinHumanRock);

    /// <summary>
    /// Room with a sign pointing directions in the middle of a patch of ice
    /// </summary>
    public static UndertaleRoom SnowdinDirectionsSign = new UndertaleRoom(50, SnowdinDoggo);

    /// <summary>
    /// Room where Papyrus shows the invisible electric maze
    /// </summary>
    public static UndertaleRoom SnowdinElectricMaze = new UndertaleRoom(52, SnowdinDirectionsSign);

    /// <summary>
    /// Room with the mouse and spaghetti in Snowdin
    /// </summary>
    public static UndertaleRoom SnowdinSpaghetti = new UndertaleRoom(56);

    /// <summary>
    /// Room where Dogamy and Dogaressa are fought
    /// </summary>
    public static UndertaleRoom SnowdinDogi = new UndertaleRoom(57, SnowdinSpaghetti);

    /// <summary>
    /// Room where Greater Dog is fought
    /// </summary>
    public static UndertaleRoom SnowdinPoffZone = new UndertaleRoom(66);

    /// <summary>
    /// Room with the long bridge at the end of Snowdin
    /// </summary>
    public static UndertaleRoom SnowdinDangerBridge = new UndertaleRoom(67, SnowdinPoffZone);

    /// <summary>
    /// The big room with the Snowdin town
    /// </summary>
    public static UndertaleRoom SnowdinTown = new UndertaleRoom(68, SnowdinDangerBridge);

    /// <summary>
    /// Room where Papyrus is fought
    /// </summary>
    public static UndertaleRoom SnowdinFog = new UndertaleRoom(81, SnowdinTown);

    /// <summary>
    /// Room with the first bridge seed puzzle
    /// </summary>
    public static UndertaleRoom WaterfallFirstBridgeSeeds = new UndertaleRoom(87);

    /// <summary>
    /// Long room with the second bridge seed puzzle
    /// </summary>
    public static UndertaleRoom WaterfallSecondBridgeSeeds = new UndertaleRoom(88, WaterfallFirstBridgeSeeds);

    /// <summary>
    /// Room with the telescope and the wishing room sign
    /// </summary>
    public static UndertaleRoom WaterfallWishingRoom = new UndertaleRoom(90, WaterfallSecondBridgeSeeds);

    /// <summary>
    /// Room where Sans shows up with a telescope
    /// </summary>
    public static UndertaleRoom WaterfallSansTelescope = new UndertaleRoom(95);

    /// <summary>
    /// Big room before Onion Sans
    /// </summary>
    public static UndertaleRoom WaterfallGlowingWater = new UndertaleRoom(97, WaterfallSansTelescope);

    /// <summary>
    /// Room where the Ballet Shoes are found
    /// </summary>
    public static UndertaleRoom WaterfallBalletShoes = new UndertaleRoom(98, WaterfallGlowingWater, true);

    /// <summary>
    /// Room where Onion Sans resides
    /// </summary>
    public static UndertaleRoom WaterfallOnionSan = new UndertaleRoom(100, WaterfallGlowingWater);

    /// <summary>
    /// Room where Gerson's shop is accessible
    /// </summary>
    public static UndertaleRoom WaterfallOutsideGerson = new UndertaleRoom(124);

    /// <summary>
    /// Room between Gerson's shop and the mushroom maze
    /// </summary>
    public static UndertaleRoom WaterfallBeforeMazes = new UndertaleRoom(126, WaterfallOutsideGerson);

    /// <summary>
    /// Room that connects to Temmie village, and contains a mushroom maze
    /// </summary>
    public static UndertaleRoom WaterfallMushroomMaze = new UndertaleRoom(127, WaterfallBeforeMazes);

    /// <summary>
    /// Room with the crystal maze, which gets darker progressively
    /// </summary>
    public static UndertaleRoom WaterfallCrystalMaze = new UndertaleRoom(129, WaterfallMushroomMaze);

    /// <summary>
    /// Room with the echo flower taunting "behind you" and Undyne appears
    /// </summary>
    public static UndertaleRoom WaterfallFlowerflowRoom = new UndertaleRoom(130, WaterfallCrystalMaze);

    /// <summary>
    /// Room where Undyne the Undying is fought
    /// </summary>
    public static UndertaleRoom WaterfallMonsterKidBridge = new UndertaleRoom(132);

    /// <summary>
    /// Room immediately after Alphy's Lab
    /// </summary>
    public static UndertaleRoom HotlandPostLab = new UndertaleRoom(143);

    /// <summary>
    /// Room with the very long vertical conveyor belt in Hotland, where you first fight Vulkin
    /// </summary>
    public static UndertaleRoom HotlandLongConveyor = new UndertaleRoom(144, HotlandPostLab);

    /// <summary>
    /// Room with the vents that has access to the room with the Burnt Pan
    /// </summary>
    public static UndertaleRoom HotlandNearPan = new UndertaleRoom(145, HotlandLongConveyor);
    
    // making a simplification for now that there's a linear path from warrior room to mettaton, if it ever becomes
    // necessary to do otherwise, UndertaleRoom will need to be expanded to have more endpoints
    
    /// <summary>
    /// Room with the warrior path in Core
    /// </summary>
    public static UndertaleRoom CoreWarrior = new UndertaleRoom(208);

    /// <summary>
    /// Room immediately before the warrior path
    /// </summary>
    public static UndertaleRoom CorePreWarrior = new UndertaleRoom(200, CoreWarrior);

    /// <summary>
    /// Room where the player grind for encounters in Core after moving to the left side
    /// </summary>
    public static UndertaleRoom CoreLeftGrindRoom = new UndertaleRoom(201, CorePreWarrior);

    /// <summary>
    /// Room immediately before the Core bridge, where the forcefield would be
    /// </summary>
    public static UndertaleRoom CoreCenter = new UndertaleRoom(202, CoreLeftGrindRoom);

    /// <summary>
    /// Room with the long bridge in Core before the room with the elevator
    /// </summary>
    public static UndertaleRoom CoreBridge = new UndertaleRoom(209, CoreCenter);

    /// <summary>
    /// Room with the elevator and before the room where the final Mettaton battle is
    /// </summary>
    public static UndertaleRoom CorePreMettaton = new UndertaleRoom(210, CoreBridge);
}

/// <summary>
/// Represent a battlegroup in-game
/// </summary>
enum Battlegroup
{
    /// <summary>
    /// Scripted first froggit
    /// </summary>
    FirstFroggit = 3,
    
    /// <summary>
    /// Lone Froggit
    /// </summary>
    SingleFroggit = 4,
    
    /// <summary>
    /// Lone Whimsun
    /// </summary>
    Whimsun = 5,

    /// <summary>
    /// Froggit with Whimsun
    /// </summary>
    FroggitWhimsun = 6,

    /// <summary>
    /// Lone Moldsmal
    /// </summary>
    SingleMoldsmal = 7,

    /// <summary>
    /// Three moldsmals
    /// </summary>
    TripleMoldsmal = 8,
    
    /// <summary>
    /// Two froggits
    /// </summary>
    DoubleFroggit = 9,

    /// <summary>
    /// Two moldsmals
    /// </summary>
    DoubleMoldsmal = 10,

    /// <summary>
    /// Greater Dog fight
    /// </summary>
    GreaterDog = 26,

    /// <summary>
    /// Lonely Snowdrake/Chilldrake
    /// </summary>
    SingleSnowdrake = 30,

    /// <summary>
    /// Lonely Ice Cap
    /// </summary>
    SingleIceCap = 32,

    /// <summary>
    /// Ice Cap, Jerry and Snowdrake
    /// </summary>
    SnowdinDouble = 35,

    /// <summary>
    /// Ice Cap, Jerry
    /// </summary>
    SnowdinTriple = 36,

    /// <summary>
    /// Lonely Aaron
    /// </summary>
    SingleAaron = 40,

    /// <summary>
    /// Temmie battle
    /// </summary>
    Temmie = 41,

    /// <summary>
    /// Lonely Woshua
    /// </summary>
    SingleWoshua = 43,

    /// <summary>
    /// Battle with the two moldsmals where one is secretly a Moldbygg
    /// </summary>
    WaterfallImpostorMoldsmal = 42,

    /// <summary>
    /// Lonely Tsunderplane
    /// </summary>
    Tsunderplane = 50,

    /// <summary>
    /// Lonely Vulkin
    /// </summary>
    Vulkin = 51,

    /// <summary>
    /// Waterfall encounter with two moldsmals
    /// </summary>
    WaterfallDoubleMoldsmal = 53,

    /// <summary>
    /// Woshua, Aaron encounter
    /// </summary>
    WoshuaAaron = 54,

    /// <summary>
    /// Moldbygg, Woshua encounter
    /// </summary>
    MoldbyggWoshua = 55,

    /// <summary>
    /// Lonely Madjick
    /// </summary>
    SingleMadjick = 59,

    /// <summary>
    /// Lonely Knight Knight
    /// </summary>
    SingleKnightKnight = 60,

    /// <summary>
    /// (Core) Lonely Astigmatism
    /// </summary>
    SingleAstigmatism = 62,

    /// <summary>
    /// (Core) Whimsalot, Final Froggit encounter
    /// </summary>
    WhimsalotFinalFroggit = 64,

    /// <summary>
    /// (Core) Whimsalot, Astigmatism encounter
    /// </summary>
    WhimsalotAstigmatism = 65,
    
    /// <summary>
    /// (Core) Final Froggit, Astigmatism encounter
    /// </summary>
    FinalFroggitAstigmatism = 66,

    /// <summary>
    /// (Core) Final Froggit, Whimsalot, Astigmatism encounter
    /// </summary>
    CoreTriple = 67,

    /// <summary>
    /// Knight Knight, Madjick encounter
    /// </summary>
    KnightKnightMadjick = 68
}

/// <summary>
/// Represents all items a player can hold
/// </summary>
/// <remarks>
/// For the time being, the inventory is assumed to contain only weapons, and all are equippable (and nothing
/// more than that has been necessary so far)
/// </remarks>
class UndertaleInventory
{
    /// <summary>
    /// Weapon the player holds
    /// </summary>
    private UndertaleWeapon EquippedWeapon = UndertaleWeapon.Stick;

    /// <summary>
    /// Maximum number of items one can hold
    /// </summary>
    public static int Capacity = 8;

    /// <summary>
    /// All items stored in the inventory
    /// </summary>
    private UndertaleWeapon[] Inventory = new UndertaleWeapon[Capacity];

    /// <summary>
    /// Number of items in the inventory
    /// </summary>
    private int InventoryCount = 0;

    /// <summary>
    /// Representing picking up an item
    /// </summary>
    /// <param name="item"></param>
    public void PickUp (UndertaleWeapon item)
    {
        if (InventoryCount < Capacity)
        {
            Inventory[InventoryCount] = item;
        }
        InventoryCount++;
    }

    /// <summary>
    /// Represent dropping the item at an index
    /// </summary>
    /// <param name="index"></param>
    public void Drop (int index)
    {
        Inventory[index] = UndertaleWeapon.None;
        for (int i = index + 1; i < Capacity; i++)
        {
            Inventory[i - 1] = Inventory[i];
        }
        Inventory[Capacity - 1] = UndertaleWeapon.None;
        InventoryCount--;
    }

    /// <summary>
    /// Represent equipping a weapon at an index
    /// </summary>
    /// <param name="index"></param>
    public void Equip (int index)
    {
        var indexItem = Inventory[index];
        Inventory[index] = EquippedWeapon;
        EquippedWeapon = indexItem;
    }

    /// <summary>
    /// Get the GML code that updates all the item slots to match with the items in this inventory
    /// </summary>
    /// <returns></returns>
    public string GetInventoryInitializer ()
    {

        var code = @$"
        obj_time.segment_inventory[{Capacity}] = {(int)EquippedWeapon}
        ";

        for (int i = 0; i < Capacity; i++)
        {
            code += @$"obj_time.segment_inventory[{i}] = {(int)Inventory[i]};";
        }

        return code;
    }

    /// <summary>
    /// Copy another inventory's items
    /// </summary>
    /// <param name="other"></param>
    public void CopyFrom (UndertaleInventory other)
    {
        InventoryCount = 0;
        for (int i = 0; i < Capacity; i++)
        {
            Inventory[i] = other.Inventory[i];
            if (Inventory[i] != UndertaleWeapon.None) InventoryCount++;
        }
        EquippedWeapon = other.EquippedWeapon;
    }

    /// <summary>
    /// Build with empty inventory
    /// </summary>
    public UndertaleInventory () {}
}

/// <summary>
/// Represent a weapon in-game
/// </summary>
enum UndertaleWeapon
{
    None = 0,
    Stick = 3,
    ToughGlove = 14,
    BalletShoes = 25,
    BurntPan = 47
}

// Class for a GML if-else code block
class IfElseBlock
{
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
    public void AddBlocks (string[] blocks)
    {
        foreach (string block in blocks) AddIfBlock(block);
    }

    /// <summary>
    /// Add all if blocks from a list of blocks
    /// </summary>
    /// <param name="blocks"></param>
    public void AddBlocks(List<string> blocks)
    {
        AddBlocks(blocks.ToArray());
    }

    /// <summary>
    /// Add a block from a string
    /// </summary>
    /// <param name="code"></param>
    public void AddIfBlock (string code)
    {
        IfBlocks.Add(code);
    } 

    /// <summary>
    /// Set the value of the `else` block
    /// </summary>
    /// <param name="code"></param>
    public void SetElseBlock (string code)
    {
        ElseBlock = code;
    }

    /// <summary>
    /// Get the final GML code for the block
    /// </summary>
    /// <returns></returns>
    public string GetCode ()
    {
        // if there are no if statements, then the `else` block should not be wrapped on anything
        if (IfBlocks.Any()) {
            return String.Join("else ", IfBlocks) + $"else {{ {ElseBlock} }}";
        }
        else return ElseBlock;
    }

    /// <summary>
    /// Create an empty if-else block
    /// </summary>
    public IfElseBlock () {}

    /// <summary>
    /// Create an if-else block with if statements from a list
    /// </summary>
    /// <param name="blocks"></param>
    public IfElseBlock (List<string> blocks)
    {
        AddBlocks(blocks);
    }

    /// <summary>
    /// Create an if-else block with if statements from an array
    /// </summary>
    /// <param name="blocks"></param>
    public IfElseBlock (string[] blocks)
    {
        AddBlocks(blocks);
    }

    /// <summary>
    /// Get the if else block GML code from an array of if blocks
    /// </summary>
    /// <param name="blocks"></param>
    /// <returns></returns>
    public static string GetIfElseBlock (string[] blocks)
    {
        var block = new IfElseBlock(blocks);
        return block.GetCode();
    }

    /// <summary>
    /// Get the if else block GML code from a list of if blocks
    /// </summary>
    /// <param name="blocks"></param>
    /// <returns></returns>
    public static string GetIfElseBlock (List<string> blocks)
    {
        return GetIfElseBlock(blocks.ToArray());
    }
}

/// <summary>
/// Contains the name of all code entries that are used
/// </summary>
public static class CodeEntryClass
{
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

    public static string targetChoice = "gml_Object_obj_targetchoice_Step_0";

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

    /// <summary>
    /// Code for touching `doorB`
    /// </summary>
    public static string DoorB = "gml_Object_obj_doorB_Other_19";

    /// <summary>
    /// Step code for Greater Dog's battle
    /// </summary>
    public static string GreaterDog = "gml_Object_obj_greatdog_Step_0";

    /// <summary>
    /// Creater code for Greater Dog's battle
    /// </summary>
    public static string GreaterDogCreate = "gml_Object_obj_greatdog_Create_0";

    /// <summary>
    /// The draw event for Chara at the end of the run
    /// </summary>
    public static string Chara = "gml_Object_obj_truechara_Draw_0";

    /// <summary>
    /// Event that fires when you hit the trigger that forces you to go right
    /// into the dogi fight
    /// </summary>
    public static string EnterDogi = "gml_Object_obj_forcedright_Collision_1570";

    /// <summary>
    /// Step event for the object that control's Frisk
    /// </summary>
    public static string Mainchara = "gml_Object_obj_mainchara_Step_0";

    public static string LevelUp = "gml_Script_scr_levelup";
}

/*
UNDERTALE CODE MANIPULATION
*/

/// <summary>
/// Append GML to the end of a code entry
/// </summary>
/// <param name="codeName">Name of the code entry</param>
/// <param name="code">Code to append</param>
void append (string codeName, string code)
{
    Data.Code.ByName(codeName).AppendGML(code, Data);
}

/// <summary>
/// Replace text in a code entry
/// </summary>
/// <param name="codeName">Name of the code entry</param>
/// <param name="text">Exact text to be replaced</param>
/// <param name="replacement">Text to overwrite the old text</param>
void replace (string codeName, string text, string replacement)
{
    ReplaceTextInGML(codeName, text, replacement);
}

/// <summary>
/// Place text inside a code entry
/// </summary>
/// <param name="codeName">Name of the code entry</param>
/// <param name="preceding">String matching the exact text that precedes where the code should be placed</param>
/// <param name="placement">New code to place</param>
void place (string codeName, string preceding, string placement)
{
    ReplaceTextInGML(codeName, preceding, $"{preceding}{placement}");
}

/// <summary>
/// Place text inside a code entry in which the location to place it is currently a "braceless" if statement
/// (with only one statement) and you want to put it inside this braceless if statement
/// </summary>
/// <param name="codeName">Name of the code entry</param>
/// <param name="preceding">String matching the exact text for the statement inside if</param>
/// <param name="placement">New code to place</param>
void placeInIf (string codeName, string preceding, string placement)
{
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
static class GMLCodeClass
{
    /// <summary>
    /// Converts a boolean into a GMS 1 bool
    /// </summary>
    /// <param name="boolean"></param>
    /// <returns></returns>
    public static string GMLBool (bool boolean)
    {
        return boolean ? "1" : "0";
    }

    /// <summary>
    /// Create a GML string literal from a C# string 
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string GMLString (string str)
    {
        return $"\"{str}\"";
    }

    /// <summary>
    /// Generate GML code that appends the time for a segment to the end of the current session's file
    /// </summary>
    /// <param name="time">String containing the total time obtained (in microseconds)</param>
    /// <returns></returns>
    public static string AppendNewTime (string time)
    {
        return @$"
        var file = file_text_open_append('recordings/recording_' + string(obj_time.session_name));
        file_text_write_string(file, obj_time.segment_name + '=' + string({time}) + ';');
        file_text_close(file);
        ";
    }

    /// <summary>
    /// GML code that starts a new recording session
    /// </summary>
    public static string StartSession = @"
    if (!obj_time.is_session_running)
    {{
        obj_time.is_session_running = 1;
        obj_time.session_name = string(current_year) + string(current_month) + string(current_day) + string(current_hour) + string(current_minute) + string(current_second);
        var file = file_text_open_write('recordings/recording_' + string(obj_time.session_name));
        file_text_close(file);
    }}
    ";

    /// <summary>
    /// Generate GML code that starts the segment timer
    /// </summary>
    /// <param name="segmentName">Name of the segment</param>
    /// <returns></returns>
    public static string StartSegment (string segmentName)
    {
        return @$"
        {StartSession}
        obj_time.is_timer_running = 1;
        obj_time.time_start = get_timer();
        obj_time.segment_name = {GMLString(segmentName)};
        obj_time.current_msg = obj_time.segment_message;
        ";
    }

    /// <summary>
    /// Generate GML code that begins a downtime segment
    /// </summary>
    /// <param name="downtimeName">Name of the segment</param>
    /// <returns></returns>
    public static string StartDowntime (string downtimeName)
    {
        return @$"
        {StartSession}
        obj_time.is_downtime_mode = 1;
        obj_time.downtime = 0;
        obj_time.downtime_start = 0;
        obj_time.step_count = global.encounter;
        obj_time.segment_name = '{downtimeName}';
        obj_time.current_msg = obj_time.segment_message;
        ";
    }

    /// <summary>
    /// Generate GML code that starts a step count segment
    /// </summary>
    /// <param name="segmentName">Name of the segment</param>
    /// <returns></returns>
    public static string StartStepCount (string segmentName)
    {
        return @$"
        {StartSession}
        obj_time.is_step_counting = 1;
        obj_time.start_count = global.encounter;
        obj_time.segment_name = {GMLString(segmentName)};
        obj_time.current_msg = obj_time.segment_message;
        ";
    }

    /// <summary>
    /// Generate GML code that converts a time time in miliseconds into the nearest 30 fps frame
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public static string ConvertToFrames (string time)
    {
        return $"round(({time}) / 1000000 * 30)";
    }

    /// <summary>
    /// GML code that stops the segment timer
    /// </summary>
    public static string StopTime = @$"
    obj_time.is_timer_running = 0;
    obj_time.segment++;
    {AppendNewTime(ConvertToFrames("get_timer() - obj_time.time_start"))}
    ";

    /// <summary>
    /// Stop an ongoing downtime segment
    /// <param name="saveSteps">Should be set to `true` if the total step count should be saved and `false` otherwise</param>
    /// </summary>
    public static string StopDowntime (bool saveSteps)
    {
        var stepCode = saveSteps
            ? @$"
            obj_time.segment_name += '-steps';
            // refer to `StopStepCount` as to why I am using `step_count` over `global.encounter`
            {GMLCodeClass.AppendNewTime("obj_time.step_count")}
            "
            : "";

        return @$"
        // in case the downtime ends during a downtime, must not lose the time being counted
        if (obj_time.is_downtime_running)
        {{
            obj_time.downtime += get_timer() + obj_time.downtime_start
        }}
        obj_time.is_downtime_mode = 0;
        obj_time.segment++;
        {GMLCodeClass.AppendNewTime(ConvertToFrames("obj_time.downtime"))}
        {stepCode}
        ";
    }

    /// <summary>
    /// Generate GML code that stops a step count segment
    /// </summary>
    public static string StopStepCount = @$"
    obj_time.is_step_counting = 0;
    obj_time.segment++;

    // so the usage of `step_count` as opposed to `global.encounter` here is very specific and so far seems to work out well
    // the reason for it being that with this, the final step count is actually going be one lower than it should be
    // EXCEPT if it ends at a door transition, it will be equal. This is interesting because in the C++ simulator we
    // are assuming it to be a door transition and adding one extra step. So counting the previous one still lets
    // this work out properly
    {AppendNewTime("obj_time.step_count - obj_time.start_count")}
    ";

    /// <summary>
    /// Generate GML code that teleports the player to a room and in a given position inside the room
    /// </summary>
    /// <param name="room">The room ID</param>
    /// <param name="x">x position to teleport to</param>
    /// <param name="y">y position to telport to</param>
    /// <returns></returns>
    public static string TPTo (UndertaleRoom room, int x, int y)
    {
        return TPTo(room.RoomId.ToString(), x.ToString(), y.ToString());
    }

    /// <summary>
    /// Generate GML code that teleports the player to a room and in a given position inside the room
    /// </summary>
    /// <param name="room">Valid name for a room</param>
    /// <param name="x">Valid name for a number</param>
    /// <param name="y">Valid name for a number</param>
    /// <returns></returns>
    public static string TPTo (string room, string x, string y)
    {
        return @$"
        obj_time.tp_flag = 1;
        // lock player
        obj_time.lock_player = 1;
        // prevent player from getting locked if they TP out of battle
        room_persistent = false;
        room = {room};
        obj_time.tp_x = {x};
        obj_time.tp_y = {y};
        ";
    }

    /// <summary>
    /// Rig the encounter to a battlegroup
    /// </summary>
    /// <param name="battlegroup"></param>
    /// <returns></returns>
    public static string RigEncounter (Battlegroup? battlegroup)
    {
        return @$"
        global.battlegroup = {(int)battlegroup};
        ";
    }

    /// <summary>
    /// Generate GML code that update flags to set murder level
    /// </summary>
    /// <param name="value">Valid number name</param>
    /// <returns></returns>
    public static string SetMurderLevel (string value)
    {
        string code = @$"
        var murder_lv = {value};
        // ""redemption"" flag
        global.flag[26] = 0;
        ";

        // maps for each index (murder level) the flags and their values needed
        // NOTE: this is slightly out of order with the murderlv script, because of snowdrake
        // flag 57 for the snowdrake is placed first before the dogs, changing the order for everything
        // in snowdin
        // NOTE 2: At the lab there is one extra murder level inserted here from the flags in mettaton lab in geno
        var flagMaps = new []
        {
            new Dictionary<int, int> { { 202,  20 } },
            new Dictionary<int, int> { { 45,  4 } },
            new Dictionary<int, int> { { 57, 2 } },
            new Dictionary<int, int> { { 52, 1 } },
            new Dictionary<int, int> { { 53, 1 } },
            new Dictionary<int, int> { { 54, 1 } },
            new Dictionary<int, int> { { 203, 16 } },
            new Dictionary<int, int> { { 67, 1 } },
            new Dictionary<int, int> { { 81, 1 } },
            new Dictionary<int, int> { { 252, 1 } },
            new Dictionary<int, int> { { 204, 18 } },
            new Dictionary<int, int> { { 251, 1 }, { 350, 1 } },
            new Dictionary<int, int>
            {
                { 367, 1 },
                { 368, 1 },
                { 369, 99 },
                { 371, 1 },
                { 374, 1 },
                { 375, 1 },
                { 399, 1 },
                { 400, 1 },
                { 417, 1 },
                { 418, 1 }
            },
            new Dictionary<int, int> { { 402, 1 } },
            new Dictionary<int, int> { { 397, 1 } },
            new Dictionary<int, int> { { 205, 40 } },
            new Dictionary<int, int> { { 425, 1 } }
        };

        // generate hardcoded GML to do checks
        for (int i = 0; i < flagMaps.Length; i++)
        {
            var flagMap = flagMaps[i];
            var ifBlock = new IfElseBlock();
            for (int j = 0; j < 2; j ++)
            {
                var assignmentCode = "";
                foreach (int flag in flagMap.Keys) {
                    int flagValue = j == 0 ? 0 : flagMap[flag];
                    assignmentCode += @$"
                    global.flag[{flag}] = {flagValue};
                    ";
                }
                if (j == 0)
                {
                    ifBlock.SetElseBlock(assignmentCode);
                }
                else
                {
                    ifBlock.AddIfBlock(@$"
                    if (murder_lv > {i}) {{
                        {assignmentCode}
                    }}
                    ");
                }
            }
            code += ifBlock.GetCode();
        }

        return code;
    }

    /// <summary>
    /// Generate GML code that updates a variable's value to the desired step count for the next encounter
    /// </summary>
    /// <param name="variable">Variable name</param>
    /// <returns></returns>
    public static string SetSteps (string variable)
    {
        return @$"
        if (obj_time.fast_encounters)
        {{
            {variable} = 60;
        }}
        else
        {{
            if (obj_time.next_step_count)
            {{
                {variable} = obj_time.next_step_count;
            }}
            else
            {{
                {variable} = 10000;
            }}
        }}
        ";
    }

    /// <summary>
    /// Generate GML code that updates the AT stat to the proper one based on the equipped weapon
    /// </summary>
    /// <returns></returns>
    public static string UpdateAT ()
    {
        // copied straight out of gml_Script_scr_weaponeq
        return @$"
        if (global.weapon == 3)
            global.wstrength = 0
        if (global.weapon == 13)
            global.wstrength = 3
        if (global.weapon == 14)
            global.wstrength = 5
        if (global.weapon == 25)
            global.wstrength = 7
        if (global.weapon == 45)
            global.wstrength = 2
        if (global.weapon == 47)
            global.wstrength = 10
        if (global.weapon == 49)
            global.wstrength = 12
        if (global.weapon == 51)
            global.wstrength = 15
        if (global.weapon == 52)
            global.wstrength = 99
        if (global.armor == 48)
            global.wstrength += 5
        if (global.armor == 64)
            global.wstrength += 10
        ";
    }

    /// <summary>
    /// Generate GML code that draws yellow text with outline in a x, y posiition
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="text"></param>
    /// <returns></returns>
    public static string DrawTextOutline (int x, int y, string text)
    {
        string textLine (int x, int y)
        {
            return $"draw_text({x}, {y}, {text});";
        }

        var code = $"draw_set_color(c_black);";

        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                var outline = 2;
                code += textLine(x + outline * (int)Math.Pow(-1, i), y + outline * (int)Math.Pow(-1, j));
            }
        }

        return @$"
        {code}
        draw_set_color(c_yellow);
        {textLine(x, y)};
        ";
    }
}

/// <summary>
/// Function that if called will add debug functions to the game
/// </summary>
void useDebug ()
{
    // updating it every frame is just a lazy way of doing it since it can't be done in obj_time's create event
    // since it gets overwritten by gamestart
    append(CodeEntryClass.step, "global.debug = 1;");

    // variables to print
    string[] watchVars =
    {
        "is_timer_running",
        "is_downtime_mode",
        "is_downtime_running",
        "segment_changed",
        "global.plot",
        "room",
        "segment_murder_lv"
    };

    // start with line break just to not interefere with anything
    string code = @"
    draw_set_color(c_red);
    ";
    int i = 0;
    foreach (string watchVar in watchVars)
    {
        code += $"draw_text(20, {110 + i * 25}, '{watchVar}: ' + string({watchVar}));";
        i++;
    }
    append(CodeEntryClass.draw, code);

    // print coordinates
    append(CodeEntryClass.draw, @$"
    if (instance_exists(obj_mainchara))
    {{
        draw_text(20, {(110 + i * 25)}, 'x:' +  string(obj_mainchara.x));
        draw_text(20, {(110 + (i + 1) * 25)}, 'y:' + string(obj_mainchara.y));
    }}
    ");
}

/// <summary>
/// Name for all GML placement methods
/// </summary>
enum PlaceMethod
{
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
    PlaceInIf,

    Replace
}

/*
EVENT CLASSES
*/

/// <summary>
/// Abstract class for the events that run code
/// </summary>
abstract class UndertaleEvent
{
    /// <summary>
    /// Must return a string that is unique and the same for all instances with the same "arguments"
    /// </summary>
    /// <returns></returns>
    public abstract string EventArgs ();

    /// <summary>
    /// Generate GML code for the condition needed for this event to fire
    /// 
    /// If none in particular exist, the method should return "1"
    /// </summary>
    /// <returns></returns>
    public virtual string GMLCondition ()
    {
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
    /// Should return the code that will be placed in the code entry
    /// </summary>
    /// <param name="code">Base code</param>
    /// <returns></returns>
    public virtual string Placement ()
    {
        return Code;
    }

    /// <summary>
    /// Should return the GML code entry where the code for this event is placed
    /// </summary>
    /// <returns></returns>
    public abstract string CodeEntry ();

    /// <summary>
    /// Get a unique identifier for all instances with the same arguments and same type
    /// </summary>
    /// <returns></returns>
    public string EventId ()
    {
        var type = this.GetType().Name;
        var args = EventArgs();
        return args == "" ? type : $"{type},{args}";
    }

    // the two methods below are for using this class inside dictionaries

    public override int GetHashCode ()
    {
        return EventId().GetHashCode();
    }

    public override bool Equals (object obj)
    {
        if (obj is UndertaleEvent otherObj)
        {
            return otherObj.EventId() == this.EventId();
        }
        return false;
    }

    /// <summary>
    /// Should parse all the attributes to save the event "arguments", variables that may be used to specialize the event
    /// </summary>
    /// <param name="reader">Reader with the node for the current event</param>
    protected abstract void ParseAttributes (XmlReader reader);

    /// <summary>
    /// GML code to execute when this event fires
    /// </summary>
    public string Code = "";

    /// <summary>
    /// Build event only with its execution code
    /// </summary>
    /// <param name="code"></param>
    public UndertaleEvent (string code)
    {
        Code = code;
    }

    /// <summary>
    /// Build event from its XML node
    /// </summary>
    /// <param name="reader">Reader with the XML node for the event</param>
    public UndertaleEvent (XmlReader reader)
    {
        ParseAttributes(reader);
        reader.Read();
        Code += '\n' + reader.Value;
    }
}

/// <summary>
/// Class for a Undertale Event that takes no arguments (all instances of this event are the same)
/// </summary>
abstract class UniqueEvent : UndertaleEvent
{    
    public UniqueEvent (string code) : base(code) {}
    
    public UniqueEvent (XmlReader reader) : base(reader) {}

    protected override void ParseAttributes (XmlReader reader) {}

    public override string EventArgs ()
    {
        return "";
    }
}

/// <summary>
/// Event that fires when the player picks the name
/// </summary>
class PickName : UniqueEvent
{   
    public PickName (XmlReader reader) : base(reader) {}

    public override PlaceMethod Method => PlaceMethod.PlaceInIf;

    public override string Replacement => "naming = 4";

    public override string CodeEntry()
    {
        return CodeEntryClass.naming;
    }
}

/// <summary>
/// Event that fires when the blcon shows up in the screen before an encounter
/// </summary>
class Blcon : UniqueEvent
{
    public Blcon (XmlReader reader) : base(reader) {}

    public override PlaceMethod Method => PlaceMethod.Append;

    public override string CodeEntry()
    {
        return CodeEntryClass.blcon;
    }
}

/// <summary>
/// Event that fires when a battle starts
/// </summary>
class EnterBattle : UndertaleEvent
{
    /// <summary>
    /// Battlegroup id for the battle that is being watched, equal to `-1` if all battles are being watched
    /// </summary>
    public int BattlegroupId;

    public EnterBattle (XmlReader reader) : base(reader) {}

    public override PlaceMethod Method => PlaceMethod.Append;

    public override string Placement ()
    {
        return @$"
        if (obj_time.previous_room != room_battle && room == room_battle) {{
            {Code}
        }}
        ";
    }

    public override string GMLCondition ()
    {
        if (BattlegroupId < 0)
        {
            return "1";
        }
        else return $"global.battlegroup == {BattlegroupId}";
    }

    public override string CodeEntry()
    {
        return CodeEntryClass.step;
    }

    protected override void ParseAttributes (XmlReader reader)
    {
        object battlegroup;
        var attribute = reader.GetAttribute("battlegroup");
        if (attribute == null) BattlegroupId = -1;
        else
        {
            Enum.TryParse(typeof(Battlegroup), attribute, out battlegroup);
            BattlegroupId = (int)(Battlegroup)battlegroup;
        }
    }

    public override string EventArgs ()
    {
        return BattlegroupId > -1 ? BattlegroupId.ToString() : "";
    }
}


/// <summary>
/// Event that fires at the end of a room transition
/// </summary>
class RoomTransition : UndertaleEvent
{
    /// <summary>
    /// Room id for the room where the transition begins in
    /// </summary>
    public int Start;

    /// <summary>
    /// Room id for the room where the transition ends at
    /// </summary>
    public int End;

    public RoomTransition (XmlReader reader) : base(reader) {}
    
    public override PlaceMethod Method => PlaceMethod.Append;

    public override string GMLCondition ()
    {
        return $"obj_time.previous_room == {Start} && room == {End}";
    }
    
    public override string CodeEntry()
    {
        return CodeEntryClass.step;
    }

    protected override void ParseAttributes(XmlReader reader)
    {
        var startRoom = RoomClass.GetRoom(reader.GetAttribute("room"));
        Start = startRoom.RoomId;
        var backwards = XmlBool.ParseXmlBool(reader, "backwards");
        if (backwards) End = startRoom.Previous.RoomId;
        else
        {
            try
            {
                End = startRoom.Next.RoomId;
            }
            catch (System.Exception)
            {
                throw new Exception ($"Room with ID \"{startRoom.RoomId}\" does not have a \"next\" neighbor");
            }
        }
    }

    public override string EventArgs ()
    {
        return $"{Start},{End}";
    }
}

/// <summary>
/// Event that fires when the player is found to be in a room
/// </summary>
class RoomEvent : UndertaleEvent
{
    /// <summary>
    /// Id of the room to watch
    /// </summary>
    public int RoomId;

    public RoomEvent (XmlReader reader) : base(reader) {}

    public override PlaceMethod Method => PlaceMethod.Append;

    public override string GMLCondition ()
    {
        return $"room == {RoomId}";
    }
    
    public override string CodeEntry()
    {
        return CodeEntryClass.step;
    }

    protected override void ParseAttributes(XmlReader reader)
    {
        RoomId = RoomClass.GetRoom(reader.GetAttribute("room")).RoomId;
    }

    public override string EventArgs ()
    {
        return RoomId.ToString();
    }
}

/// <summary>
/// Event that fires when a battle is left (access to the "overworld")
/// </summary>
class LeaveBattle : UniqueEvent
{
    public LeaveBattle (XmlReader reader) : base(reader) {}

    public override PlaceMethod Method => PlaceMethod.Append;

    public override string Placement ()
    {
        return @$"
        if (obj_time.previous_room == room_battle && room != room_battle)
        {{
            {Code}
        }}
        ";
    }

    public override string CodeEntry()
    {
        return CodeEntryClass.step;
    }
}

/// <summary>
/// Event that fires the frame before entering a attle
/// </summary>
class BeforeBattle : UniqueEvent
{
    public BeforeBattle (string code) : base(code) {}

    public BeforeBattle (XmlReader reader) : base(reader) {}

    public override PlaceMethod Method => PlaceMethod.Place;

    public override string Replacement => "battle = 1";

    public override string CodeEntry()
    {
        return CodeEntryClass.blconAlarm;
    }
}

/// <summary>
/// Event that fires when Froggit's attack is decided
/// </summary>
class FroggitAttack : UniqueEvent
{
    public FroggitAttack (XmlReader reader) : base(reader) {}

    public override PlaceMethod Method => PlaceMethod.Place;

    public override string Replacement => "use_frogskip = 1";

    public override string CodeEntry()
    {
        return CodeEntryClass.froggitAlarm;
    }
}

class DamageRoll : UniqueEvent
{
    public DamageRoll (XmlReader reader) : base(reader) {}

    public override PlaceMethod Method => PlaceMethod.Replace;

    public override string Replacement => "global.damage += random(2)";

    public override string CodeEntry()
    {
        return CodeEntryClass.targetChoice;
    }
}

/// <summary>
/// Event that fires when Froggit's turn ends
/// </summary>
class FroggitTurnEnd : UniqueEvent
{
    public FroggitTurnEnd (XmlReader reader) : base(reader) {}

    public override PlaceMethod Method => PlaceMethod.PlaceInIf;

    public override string Replacement => "attacked = 0";

    public override string CodeEntry()
    {
        return CodeEntryClass.froggitStep;
    }
}

/// <summary>
/// Event that fires Froggit's turn starts
/// </summary>
class FroggitTurnStart : UniqueEvent
{
    public FroggitTurnStart (XmlReader reader) : base(reader) {}

    public override PlaceMethod Method => PlaceMethod.Place;

    public override string Replacement => "if (global.mnfight == 2)\n{";

    public override string CodeEntry()
    {
        return CodeEntryClass.froggitStep;
    }
}

/// <summary>
/// Event that fires when the "YOU WON" message in battle begins displaying
/// </summary>
class YouWon : UniqueEvent
{
    public YouWon (XmlReader reader) : base(reader) {}

    public override PlaceMethod Method => PlaceMethod.Place;

    public override string Replacement => "earned \"";

    public override string CodeEntry()
    {
        return CodeEntryClass.battlecontrol;
    }
}

/// <summary>
/// Event that fires when a door is touched
/// </summary>
class Door : UndertaleEvent
{
    /// <summary>
    /// Name of the door to listen to
    /// </summary>
    public string Name;

    /// <summary>
    /// Id of the room to listen for the door touch
    /// </summary>
    public int Room;

    public Door (XmlReader reader) : base(reader) {}
    
    public override PlaceMethod Method => PlaceMethod.Append;

    public override string GMLCondition ()
    {
        return $"room == {Room}";
    }
    
    public override string CodeEntry ()
    {
        if (Name == "A")
        {
            return CodeEntryClass.doorA;
        }
        if (Name == "C")
        {
            return CodeEntryClass.doorC;
        }
        if (Name == "Amusic")
        {
            return CodeEntryClass.doorAmusic;
        }
        if (Name == "B")
        {
            return CodeEntryClass.DoorB;
        }
        return "";
    }

    protected override void ParseAttributes(XmlReader reader)
    {
        Name = reader.GetAttribute("name");
        Room = RoomClass.GetRoom(reader.GetAttribute("room")).RoomId;
    }

    public override string EventArgs ()
    {
        return $"{Name},{Room}";
    }
}

/// <summary>
/// Event that fires when the steps for the next encounter are calculated
/// </summary>
class ScrSteps : UniqueEvent
{
    public ScrSteps (XmlReader reader) : base(reader) {}

    public override PlaceMethod Method => PlaceMethod.Place;

    public override string Replacement => "steps = 10000"; // TO-DO: maybe reduce redundancy of this code with the one in scrSteps

    public override string CodeEntry ()
    {
        return CodeEntryClass.scrSteps;
    }

}

/// <summary>
/// Event that fires when Greater Dog's turn starts
/// </summary>
class GreaterDogTurnStart : UndertaleEvent
{
    /// <summary>
    /// The turn number (1-indexed) the event should fire
    /// </summary>
    public int Turn;

    public GreaterDogTurnStart (XmlReader reader) : base(reader) {}

    public override PlaceMethod Method => PlaceMethod.Place;
    
    public override string Replacement => "(global.firingrate * 1.7)";

    public override string Placement ()
    {
        return @$"
        current_turn++;
        {Code}
        ";
    }

    public override string GMLCondition ()
    {
        return $"(current_turn == {Turn})";
    }

    protected override void ParseAttributes(XmlReader reader)
    {
        Turn = Int32.Parse(reader.GetAttribute("turn"));
    }

    public override string CodeEntry ()
    {
        return CodeEntryClass.GreaterDog;
    }

    public override string EventArgs ()
    {
        return Turn.ToString();
    }
}


/// <summary>
/// Event that fires when Greater Dog's turn ends
/// </summary>
class GreaterDogTurnEnd : UndertaleEvent
{
    /// <summary>
    /// The turn number (1-indexed) the event should fire
    /// </summary>
    public int Turn;
    
    public GreaterDogTurnEnd (XmlReader reader) : base(reader) {}

    public override PlaceMethod Method => PlaceMethod.PlaceInIf;

    public override string Replacement => "attacked = 0";

    public override string GMLCondition ()
    {
        return $"(current_turn == {Turn})";
    }

    protected override void ParseAttributes (XmlReader reader)
    {
        Turn = Int32.Parse(reader.GetAttribute("turn"));
    }

    public override string CodeEntry()
    {
        return CodeEntryClass.GreaterDog;
    }

    public override string EventArgs ()
    {
        return Turn.ToString();
    }
}

/// <summary>
/// Event that fires when the player picks any of the choices at the end of a Genocide run
/// </summary>
class RunEnd : UniqueEvent
{
    public RunEnd (XmlReader reader) : base(reader) {}

    public override PlaceMethod Method => PlaceMethod.Place;

    public override string Replacement => "snd_play(snd_select)";

    public override string CodeEntry ()
    {
        return CodeEntryClass.Chara;
    }

}

/// <summary>
/// Event that fires when the player steps into the trigger that forced them to go right
/// to the Dogi fight
/// </summary>
class EnterDogi : UniqueEvent
{
    public EnterDogi (XmlReader reader) : base(reader) {}

    public override PlaceMethod Method => PlaceMethod.Place;

    public override string Replacement => "vol1 = vol";

    public override string CodeEntry ()
    {
        return CodeEntryClass.EnterDogi;
    }
}

/******
start of main script
******/

void main ()
{
    // testing script-game compatibility
    EnsureDataLoaded();

    if (Data?.GeneralInfo?.DisplayName?.Content.ToLower() != "undertale")
    {
        ScriptError("Error 0: Script must be used in Undertale");
    }

    // reading all segments from the XML file
    var segments = new List<Segment>();

    XmlReaderSettings settings = new XmlReaderSettings();
    settings.IgnoreWhitespace = true;
    using (XmlReader reader = XmlReader.Create(Path.Combine(ScriptPath, "..\\segments.xml"), settings))
    {
        // use a blank one for the first previous, it won't be used for anything
        var previous = new Segment();
        while (reader.Read()) {
            if (reader.NodeType == XmlNodeType.Element)
            {
                if (reader.Name == "segment")
                {
                    Segment segment = new Segment(reader, previous);
                    Console.WriteLine($"Successfully read segment \"{segment.Name}\"");
                    previous = segment;
                    segments.Add(segment);
                }
                else if (reader.Name != "segments")
                {
                    throw new Exception(@$"Expected to find a segment node, instead found {reader.Name.ToString()}");
                }
            }
        }
    }

    // make drawing work
    Data.GameObjects.ByName("obj_time").Visible = true;

    // make it so levelup also correctly assigns LV 1
    place(CodeEntryClass.LevelUp, "currentlevel = global.lv", @"
    global.lv = 1
    ");

    // initializing variables
    append(CodeEntryClass.create, $@"
    // where recording text files will be saved
    directory_create('recordings');

    // name is a ""date timestamp""
    session_name = 0;

    is_session_running = 0;

    // keeps track of segments
    segment = 0;
    segment_total = {segments.Count - 1};
    previous_segment = -1;
    segment_changed = 0;
    segment_name = '';
    // using last index for the weapon
    segment_inventory[{UndertaleInventory.Capacity}] = 0;

    // continuous segment variables
    is_timer_running = 0;
    time_start = 0;
    time_end = 0;

    // this flag will control if explanations should be given
    read_tutorial = 1;

    // store message to display
    current_msg = '';

    // this flag will controll how encounters are given
    // if `0`, then encounters will be disabled or set according to ""next_step_count""
    // else if `1` then encounters will be given quickly
    fast_encounters = 0;
    next_step_count = 0;

    // downtime segment variables
    // mode is for when downtime is being watched
    // running is for when downtime is being watched and a downtime has been reached
    is_downtime_mode = 0;
    is_downtime_running = 0;
    downtime_start = 0;
    downtime = 0;
    step_count = 0;
    previous_time = 0;

    // step count variables
    is_step_counting = 0;
    start_count = 0;

    // room tracker
    previous_room = 0;
    current_room = 0;

    // see in step for explanation on tp
    tp_flag = 0;
    tp_x = 0;
    tp_y = 0;
    lock_player = 0;
    ");

    // watching for when to tint frisk
    append(CodeEntryClass.Mainchara, @$"
    if (obj_time.lock_player)
    {{
        image_blend = c_purple;
    }}
    else
    {{
        image_blend = c_white;
    }}
    ");

    // add convenient settings for the encounters
    place(CodeEntryClass.scrSteps, @"steps = ((argument0 + round(random(argument1))) * populationfactor)", $@"
    if (obj_time.fast_encounters)
    {{
        // if we want fast encounters, we are probably killing things, so setting kills to 0 is
        // a control method to not go over the limit which is manually set always 
        global.flag[argument3] = 0;
        // max hp for the user convenience due to unusual amount of encounters and frog skips
        global.hp = global.maxhp;
    }}
    {GMLCodeClass.SetSteps("steps")}
    ");

    // track segment changes
    append(CodeEntryClass.step, $@"
    if (previous_segment != segment)
    {{
        segment_changed = 1;
    }}
    else
    {{
        segment_changed = 0;
    }}
    previous_segment = segment;
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
    if (tp_flag)
    {{
        tp_flag = 0;
        if (instance_exists(obj_mainchara))
        {{
            obj_mainchara.x = tp_x;
            obj_mainchara.y = tp_y;
            // previous x and y must be updated too due to how obj_mainchara's collision events work
            obj_mainchara.xprevious = tp_x;
            obj_mainchara.yprevious = tp_y;
        }}
    }}
    else if (lock_player && keyboard_check_pressed(vk_space))
    {{
        lock_player = 0;
        global.interact = 0;
    }}

    // prevent player from moving AT ALL
    if (lock_player)
    {{
        up = 0;
        down = 0;
        right = 0;
        left = 0;
    }}
    ");

    // add keybinds for changing segments and warping
    append(CodeEntryClass.step, $@"

    if (global.debug || !is_session_running)
    {{
        var segment_increment = 1;
        if (keyboard_check(vk_shift))
        {{
            segment_increment = 5;
        }}
        else if (keyboard_check(vk_control))
        {{
            segment_increment = 20;
        }}
        if (keyboard_check_pressed(vk_pageup))
        {{
            segment += segment_increment;
        }}
        else if (keyboard_check_pressed(vk_pagedown))
        {{
            segment -= segment_increment;
        }}

        segment = clamp(segment, 0, segment_total);
    }}
    ");

    // downtime timer controller
    append(CodeEntryClass.step, @"
    // downtime begins whenever not progressing step count
    // since downtime uses global.encounter, which is reset by encounters, it is not designed to work while encounters are on
    if (is_downtime_mode)
    {
        if (is_downtime_running)
        {
            // being greater means it has incremented
            // but it also means that last frame was the first one incrementing, thus use previous time
            if (global.encounter > step_count)
            {
                downtime += previous_time - downtime_start;
                is_downtime_running = 0;
            }
        }
        else
        {
            // being equals means global.encounter did not increment, thus downtime has begun
            // but it also means that it stopped incrementing last frame, thus use previous_time
            // means we use step_count instead of global.encounter)
            if (global.encounter == step_count)
            {
                downtime_start = previous_time;
                is_downtime_running = 1;
            }
        }
    }
    previous_time = get_timer();
    step_count = global.encounter;
    ");

    // message drawer
    append(CodeEntryClass.draw, @$"
    draw_set_font(fnt_main)
    {GMLCodeClass.DrawTextOutline(20, 0, "'segment: ' + string(segment) + '/' + string(segment_total)")}
    {GMLCodeClass.DrawTextOutline(20, 20, "'name: ' + string(segment_name)")}
    {GMLCodeClass.DrawTextOutline(20, 40, "current_msg")}
    ");

    // rigging frogskip for all froggit encounters to speed up practice
    // it is placed right after mycommand declaration
    place(CodeEntryClass.froggitAlarm, "0))", @$"
    var use_frogskip = 1;
    if (use_frogskip)
    {{
        mycommand = 0;
    }}
    else
    {{
        mycommand = 100;
    }}
    ");

    // Add turn counter for the start and end events
    append(CodeEntryClass.GreaterDogCreate, @$"
    current_turn = 0;
    ");

    // create if-else block that updates the state variables when a segment changes
    var initBlock = new IfElseBlock();
    for (int i = 0; i < segments.Count; i++) {
        var segment = segments[i];
        // copied from gml_Object_obj_undertaletitle_Draw_0
        var phoneCode = segment.SetPhone ? @$"
            global.menuchoice[2] = 1
            global.phone[0] = 0
            global.phone[1] = 0
            global.phone[2] = 0
            global.phone[3] = 0
            global.phone[4] = 0
            global.phone[5] = 0
            global.phone[6] = 0
            global.phone[7] = 0
            global.phone[0] = 206
        " : "";
        initBlock.AddIfBlock(@$"
        if (segment == {i})
        {{
            segment_name = {GMLCodeClass.GMLString(segment.Name)};
            segment_is_tutorial = {GMLCodeClass.GMLBool(segment.Tutorial)};
            segment_tutorial = {GMLCodeClass.GMLString(segment.Explanation)};
            segment_message = {GMLCodeClass.GMLString(segment.Message)};
            fast_encounters = {GMLCodeClass.GMLBool(segment.FastEncounters)};
            segment_room = {segment.Room.RoomId};
            segment_x = {segment.X};
            segment_y = {segment.Y};
            segment_xp = {segment.XP};
            segment_plot = {segment.Plot};
            next_step_count = {segment.NextStepCount};
            segment_murder_lv = {segment.MurderLevel};
            {segment.Inventory.GetInventoryInitializer()}
            {phoneCode}
        }}
        ");
    }

    var encounterers = new []
    {
        "obj_encount_core1",
        "obj_encount_fire1",
        "obj_encounter_ruins1",
        "obj_encounter_ruins2",
        "obj_encounterer_glyde",
        "obj_encounterer_jerry",
        "obj_encounterer_ruins3",
        "obj_encounterer_ruins4",
        "obj_encounterer_ruins5",
        "obj_encounterer_ruins6",
        "obj_encounterer_tundra1",
        "obj_encounterer_water1",
        "obj_encounterer_water2",
        "obj_encoutnerer_gyftrot"
    };

    var encountererUpdate = new IfElseBlock();
    foreach (string encounterer in encounterers)
    {
        encountererUpdate.AddIfBlock(@$"
        if (instance_exists({encounterer}))
        {{
            {encounterer}.steps = encounter_steps;
        }}
        ");
    }

    append(CodeEntryClass.step, @$"
    if (segment_changed)
    {{
        {initBlock.GetCode()}
    }}

    if (keyboard_check_pressed(ord('H')))
    {{
        read_tutorial = (read_tutorial + 1) % 2;
    }}

    var pressed_r = keyboard_check_pressed(ord('R'));
    var pressed_t = keyboard_check_pressed(ord('T')) && (global.debug || !is_session_running);
    if (pressed_r || segment_changed || pressed_t)
    {{
        // also useful when the next segment updates plot and we must go back
        global.plot = segment_plot;
        if (pressed_r || segment_changed)
        {{
            if (segment_is_tutorial)
            {{
                current_msg = segment_tutorial;
            }}

            global.xp = segment_xp;
            script_execute(scr_levelup);
            global.hp = global.maxhp;
            {GMLCodeClass.SetMurderLevel("segment_murder_lv")}

            var encounter_steps;
            {GMLCodeClass.SetSteps("encounter_steps")}
            {encountererUpdate.GetCode()}
            for (var i = 0; i < {UndertaleInventory.Capacity}; i++)
            {{
                global.item[i] = segment_inventory[i];
            }}
            global.weapon = segment_inventory[{UndertaleInventory.Capacity}];
            {GMLCodeClass.UpdateAT()}
        }}
        if (pressed_r || pressed_t)
        {{
            global.interact = 0;
            is_timer_running = 0;
            is_downtime_mode = 0;
            {GMLCodeClass.TPTo("segment_room", "segment_x", "segment_y")}
        }}
        if ((!is_session_running && segment_changed) || pressed_t)
        {{
            current_msg = segment_tutorial;
        }}
    }}
    ");

    // below, we add the code to run when each event is fired
    // the events are created split across each each segment, so the goal is to merge them all into one
    // and add a segment check in that merging

    // first, create a map of all unique events mapped to a map of all segments and the code that run for that event in
    // that segment
    var events = new Dictionary<UndertaleEvent, Dictionary<int, List<string>>>();

    for (int j = 0; j < segments.Count; j++)
    {
        var segment = segments[j];
        var eventListeners = new List<UndertaleEvent>();
        // if not the last segment, we check if we need to start the next segment while this one is ending
        // or if we need to teleport at the end of this segment to setup the next one
        if (j != segments.Count - 1)
        {
            var next = segments[j + 1];

            // equal to next one: do checks if the segment will be interrupted, if not, start next segment
            if (segment.End.Equals(next.Start))
            {
                if (segment.Uninterruptable) {
                    var condition = next.Tutorial ? "!obj_time.read_tutorial" : "1";
                    segment.End.Code += @$"
                    if ({condition})
                    {{
                        {next.Start.Code}
                    }}
                    ";
                }
            }

            // doing checks to see if we need to teleport to setup for the next segment
            var tpCondition = "0";
            if (!segment.Uninterruptable)
            {
                tpCondition = "1";
            }
            else if (next.Tutorial)
            {
                tpCondition = "obj_time.read_tutorial";
            }
            if (tpCondition != "0")
            {
                segment.End.Code += @$"
                if ({tpCondition})
                {{
                    {GMLCodeClass.TPTo(next.Room, next.X, next.Y)}
                }}
                ";
            }
        }


        // wrap code so that it wont run in unintended orders (run end without start and etc)
        var runningVariable = segment.Type.RunningVariable;

        segment.Start.Code = @$"
        if (!{runningVariable})
        {{
            {segment.Start.Code}
        }}
        ";
        segment.End.Code = @$"
        if ({runningVariable})
        {{
            {segment.End.Code}
        }}
        ";

        eventListeners.Add(segment.End);
        eventListeners.Add(segment.Start);
        if (segment.Other.Count > 0)
        {
            eventListeners.AddRange(segment.Other);
        }
        
        foreach (UndertaleEvent eventListener in eventListeners)
        {
            if (!events.ContainsKey(eventListener))
            {
                events[eventListener] = new Dictionary<int, List<string>>();
            }
            if (!events[eventListener].ContainsKey(j)) events[eventListener][j] = new List<string>();
            events[eventListener][j].Add(eventListener.Code);
        }
    }

    // then, group all the event types and their unique events, so that we can properly split events of same type
    // but different arguments

    // this is a map of event names to a map of unique events and their respective code
    var eventCodes = new Dictionary<string, Dictionary<UndertaleEvent, string>>();

    // now that we mapped the segments, we just need to separate them in a if-else block
    foreach (UndertaleEvent undertaleEvent in events.Keys)
    {
        var segmentCodeBlocks = new List<string>();
        var eventStages = events[undertaleEvent];
        foreach (int stage in eventStages.Keys)
        {
            segmentCodeBlocks.Add(@$"
            if (obj_time.segment == {stage})
            {{
                {// join the codes since it is a list
                String.Join("\n", eventStages[stage])}
            }}
            ");
        }
        IfElseBlock ifElseBlock = new IfElseBlock(segmentCodeBlocks);
        if (undertaleEvent.Method == PlaceMethod.Replace)
        {
            ifElseBlock.SetElseBlock(undertaleEvent.Replacement);
        }
        string eventCode = ifElseBlock.GetCode();
        var eventName = undertaleEvent.GetType().Name;

        if (!eventCodes.ContainsKey(eventName)) eventCodes[eventName] = new Dictionary<UndertaleEvent, string>();
        eventCodes[eventName][undertaleEvent] = eventCode;
    }


    // finally, go through each event type to merge all the code for each event and determine how it will be placed
    foreach (string eventName in eventCodes.Keys)
    {
        // get the map of unique events to their code
        var eventMap = eventCodes[eventName];
        
        // pick any of the events (in this case the first) to access the general methods of this event type
        // and to be used as a "dummy" event to place the code for each entry
        UndertaleEvent baseEvent = eventMap.Keys.ToList()[0];

        // split the events based on the code entries they edit
        // so create a map of code entries to the if-else block for the events in the code entry
        // in short, since the unique events are already separated, we presume that we want to access only one of them
        // and that is filtered by doing an if-else block in all of their conditions
        var entryMap = new Dictionary<string, IfElseBlock>();

        foreach (UndertaleEvent undertaleEvent in eventMap.Keys)
        {
            var eventCode = eventMap[undertaleEvent];
            var eventEntry = undertaleEvent.CodeEntry();
            if (!entryMap.ContainsKey(eventEntry))
            {
                entryMap[eventEntry] = new IfElseBlock();
            }
            var condition = undertaleEvent.GMLCondition();
            if (condition == "1")
            {
                entryMap[eventEntry].SetElseBlock(eventCode);
            }
            else
            {    
                entryMap[eventEntry].AddIfBlock(@$"
                if ({undertaleEvent.GMLCondition()})
                {{
                    {eventCode}
                }}
                ");
            }
        }

        foreach (string entry in entryMap.Keys)
        {
            baseEvent.Code = entryMap[entry].GetCode();
            switch (baseEvent.Method)
            {        
                case PlaceMethod.Append:
                    append(entry, baseEvent.Placement());
                    break;
                case PlaceMethod.Place:
                    place(entry, baseEvent.Replacement, baseEvent.Placement());
                    break;
                case PlaceMethod.PlaceInIf:
                    placeInIf(entry, baseEvent.Replacement, baseEvent.Placement());
                    break;
                case PlaceMethod.Replace:
                    replace(entry, baseEvent.Replacement, baseEvent.Placement());
                    break;
            }
        }
    }
}

main();

// debug mode - REMOVE FOR PRODUCTION BUILD!!
useDebug();

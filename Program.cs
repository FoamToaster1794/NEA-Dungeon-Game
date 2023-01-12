using static System.Console;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace NEA_Dungeon_Game
{
    internal static class Program
    {
        private const string saveNamesFileName = "saveNames.txt";
        private const string genSettingsFileName = "genSettings.txt";
        //private static readonly string[] tiles = {"██", "  ", "><", "██", ""};
        private static readonly string[] tiles = { "█", " ", "x", "█", "█" };
        private const int tileWidth = 1;
        private static readonly char[] invalidFileChars = {'<', '>', ':', '"', '/', '\\', '|', '?', '*' };
        private const int wallIndex = 0, floorIndex = 1, playerIndex = 2, exitIndex = 3, chestIndex = 4;
        private const int menuFontWeight = 400;
        private const int menuFontFamily = 54;
        private const int menuFontSize = 48;
        private const string menuFontName = "Consolas";
        private static Random random;

        public static void Main()
        {
            GenerateFiles();
            ConsoleManager.MaximiseConsole();
            ConsoleManager.SetUpMenuConsole();

            //ReadLine();
            int mainMenuChoice = 0;
            Dungeon currentDungeon;
            GenSettings genSettings = LoadGenSettings();
            while (mainMenuChoice != 3)
            {
                Clear();
                ConsoleManager.SetUpMenuConsole();
                mainMenuChoice = MainMenu();
                Clear();
                switch (mainMenuChoice)
                {
                    case 0:
                        string saveName = LoadDungeonMenu();
                        if (saveName?.Length == 0) break;
                        currentDungeon = LoadDungeonFromFile(saveName);
                        //int fontSize = CalculateFontSize(currentDungeon.size);
                        ConsoleManager.SetupConsole(400, 54, 8, "Consolas");
                        Clear();
                        currentDungeon.Display();
                        PlayDungeon(currentDungeon, saveName);
                        break;
                    case 1:
                        ConsoleManager.SetupConsole(400, 48, 8, "Terminal");
                        //ConsoleManager.SetupConsole(400, 54, 8, "Consolas");
                        currentDungeon = new Dungeon(genSettings.size);
                        currentDungeon.Generate(genSettings, true);
                        SetCursorPosition(0, 0);
                        currentDungeon.Display();
                        PlayDungeon(currentDungeon, "");
                        break;
                    case 2:
                        GenerationMenu();
                        genSettings = LoadGenSettings();
                        break;
                }
            }
            //GenerateFiles();
        }

        private static int MainMenu()
        {
            ConsoleKey keyPressed = 0;
            const int topPos = 0, bottomPos = 3;
            int cursorPos = 0;
            WriteLine(" Load Dungeon\n Generate new dungeon\n Change generation parameters\n Exit");
            while (keyPressed != ConsoleKey.Enter)
            {
                SetCursorPosition(0, cursorPos);
                Write(">");
                keyPressed = ReadKey(true).Key;
                switch (keyPressed)
                {
                    case ConsoleKey.DownArrow:
                        CursorLeft--;
                        Write(" ");
                        if (cursorPos < bottomPos) cursorPos++;
                        break;
                    case ConsoleKey.UpArrow:
                        CursorLeft--;
                        Write(" ");
                        if (cursorPos > topPos) cursorPos--;
                        break;
                }
            }
            return cursorPos;
        }

        private static Dungeon LoadDungeonFromFile(string saveName)
        {
            string[] lines = File.ReadAllLines($"{saveName}.txt");
            string input = lines[0].Remove(0, 6);
            Vec size, playerPos, exitPos;
            if (!(int.TryParse(input.Split(',')[0], out size.x) && size.x > 0))
            {
                WriteLine("Error has occured when loading dungeon");
                WriteLine("Width is not an accepted number");
                return null;
            }
            if (!(int.TryParse(input.Split(',')[1], out size.y) && size.y > 0))
            {
                WriteLine("Error has occured when loading dungeon");
                WriteLine("Height is not an accepted number");
                return null;
            }
            input = lines[1].Remove(0, 11);
            if (!(int.TryParse(input.Split(',')[0], out playerPos.x) && playerPos.x >= 0 && playerPos.x < size.x))
            {
                WriteLine("Error has occured when loading dungeon");
                WriteLine("Player Position(x) is not a valid number");
                return null;
            }
            if (!(int.TryParse(input.Split(',')[1], out playerPos.y) && playerPos.y >= 0 && playerPos.y < size.y))
            {
                WriteLine("Error has occured when loading dungeon");
                WriteLine("Player Position(y) is not a valid number");
                return null;
            }
            input = lines[2].Remove(0, 9);
            if (!(int.TryParse(input.Split(',')[0], out exitPos.x) && exitPos.x >= 0 && exitPos.x < size.x))
            {
                WriteLine("Error has occured when loading dungeon");
                WriteLine("Exit Position(x) is not a valid number");
                return null;
            }

            if (!(int.TryParse(input.Split(',')[1], out exitPos.y) && exitPos.y >= 0 && exitPos.y < size.y))
            {
                WriteLine("Error has occured when loading dungeon");
                WriteLine("Exit Position(y) is not a valid number");
                return null;
            }
            Dungeon dungeon = new Dungeon(size, playerPos, exitPos);
            for (int y = 0; y < size.y; y++)
            {
                for (int x = 0; x < size.x; x++)
                {
                    int tile = lines[y + 3][x].ToInt();
                    if (tile < 0 || tile >= tiles.Length)
                    {
                        WriteLine("Error has occured when loading dungeon grid");
                        WriteLine($"Grid cell at position ({x}, {y}) is not valid");
                        return null;
                    }
                    dungeon.cells[x, y] = tile;
                }
            }
            return dungeon;
        }

        private static string LoadDungeonMenu()
        {
            string[] saveNames;
            if (!File.Exists(saveNamesFileName) || (saveNames = File.ReadAllLines(saveNamesFileName)).Length < 1)
            {
                WriteLine("No files found");
                WriteLine("> Return to main menu");
                CursorLeft = 0;
                while (ReadKey(true).Key != ConsoleKey.Enter){}
                return "";
            }

            ConsoleKey keypressed = 0;
            const int topPos = 0;
            int bottomPos = saveNames.Length;
            int cursorPos = 0;
            WriteLine(" Back to main menu");
            foreach (string saveName in saveNames)
            {
                WriteLine($" {saveName}");
            }
            while (keypressed != ConsoleKey.Enter)
            {
                SetCursorPosition(0, cursorPos);
                Write(">");
                keypressed = ReadKey(true).Key;
                switch (keypressed)
                {
                    case ConsoleKey.DownArrow:
                        CursorLeft--;
                        Write(" ");
                        if (cursorPos < bottomPos) cursorPos++;
                        break;
                    case ConsoleKey.UpArrow:
                        CursorLeft--;
                        Write(" ");
                        if (cursorPos > topPos) cursorPos--;
                        break;
                }
            }
            return cursorPos == 0 ? "" : saveNames[cursorPos-1];
        }

        private static GenSettings LoadGenSettings()
        {
            GenSettings settings = new GenSettings();
            int[] lines = File.ReadAllLines(genSettingsFileName).Select(x => int.Parse(x)).ToArray();
            settings.size = new Vec(lines[0], lines[1]);
            settings.roomTryCount = lines[2];
            settings.extraDoorChance = lines[3];
            settings.roomExtraSize = lines[4];
            settings.windChance = lines[5];
            settings.cutDeadEnds = lines[6] == 1;
            settings.showGen = lines[7] != 1;
            settings.seed = lines[8];
            settings.lootChestChance = lines[9];
            return settings;
        }

        private static void GenerationMenu()
        {
            ConsoleKey keypressed;
            const int topPos = 2, bottomPos = 8;
            int cursorPos = 0;
            string[] settings = File.ReadAllLines(genSettingsFileName);
            if (settings[6] == "1") settings[6] = "true";
            else settings[6] = "false";
            if (settings[7] == "1") settings[7] = "true";
            else settings[7] = "false";
            string[] lines = {
                "Maze width (odd integer 21-943): ", "Maze height (odd integer 21-325): ",
                "No. of attempts to place a room (0-1000 recommended): ",
                "Percentage chance for extra room connections (0-100): ",
                "Extra room size (0-10 recommended): ",
                "Percentage chance for paths to wind (0-100): ",
                "Dead end cutting True-False: ",
                "Instant generation True-False: ",
                "Seed (leave as 0 for random seed): "
            };
            WriteLine("Generation Settings(difficulty settings)");
            WriteLine("Press Enter to save settings");
            for (int x = 0; x < lines.Length; x++)
            {
                WriteLine($"{lines[x]}{settings[x]}");
            }
            string errorMessage = " ";
            while (errorMessage.Length > 0)
            {
                bool isInputting = false;
                int inputLength = 0;
                keypressed = 0;
                while (keypressed != ConsoleKey.Enter)
                {
                    if (!isInputting)
                    {
                        SetCursorPosition(lines[cursorPos].Length - 1, cursorPos + topPos);
                        Write(">");
                    }
                    keypressed = ReadKey(true).Key;
                    switch (keypressed)
                    {
                        case ConsoleKey.DownArrow:
                            CursorLeft = lines[cursorPos].Length - 1;
                            Write(" ");
                            if (cursorPos < bottomPos) cursorPos++;
                            isInputting = false;
                            inputLength = 0;
                            break;
                        case ConsoleKey.UpArrow:
                            CursorLeft = lines[cursorPos].Length - 1;
                            Write(" ");
                            if (cursorPos > 0) cursorPos--;
                            isInputting = false;
                            inputLength = 0;
                            break;
                        case ConsoleKey.Enter:
                            break;
                        default:
                            if (inputLength >= 11) return;
                            isInputting = true;
                            
                            if (inputLength == 0 && settings[cursorPos].Length > 0)
                            {
                                Write("           ");
                                CursorLeft -= 11;
                                settings[cursorPos] = "";
                            }
                            inputLength++;
                            char lowered = char.ToLower((char)keypressed);
                            settings[cursorPos] += lowered;
                            Write(lowered);
                            break;
                    }
                }
                for (int x = 0; x < bottomPos + 1; x++)
                {
                    errorMessage = IsValidInput(settings[x], x);
                    if (errorMessage.Length > 0) break;
                }
                SetCursorPosition(0, bottomPos + topPos + 2);
                WriteLine(errorMessage + "                        ");
            }

            if (settings[6] == "true") settings[6] = "1";
            else settings[6] = "0";
            if (settings[7] == "true") settings[7] = "1";
            else settings[7] = "0";
            File.WriteAllLines(genSettingsFileName, settings);
        }

        private static string IsValidInput(string input, int pos)
        {
            switch (pos)
            {
                case 0:
                    if (!int.TryParse(input, out int width)) return "dungeon width is not a valid number";
                    if (width % 2 != 1) return "dungeon width is not odd";
                    if (width is < 21 or > 943) return "dungeon width is out of range";
                    break;
                case 1:
                    if (!int.TryParse(input, out int height)) return "dungeon height is not a valid number";
                    if (height % 2 != 1) return "dungeon height is not odd";
                    if (height is < 21 or > 325) return "dungeon height is out of range";
                    break;
                case 2:
                    if (!int.TryParse(input, out int roomTryCount))
                        return "No. of tries to place a room is not a valid number";
                    if (roomTryCount < 1) return "No. of tries to place a room is out of range";
                    break;
                case 3:
                    if (!int.TryParse(input, out int connectorChance))
                        return "Extra room connector chance is not a valid number";
                    if (connectorChance < 0) return "Extra room connector chance is out of range";
                    break;
                case 4:
                    if (!int.TryParse(input, out int extraRoomSize))
                        return "Extra room size is not a valid number";
                    if (extraRoomSize < 0) return "Extra room size is out of range";
                    break;
                case 5:
                    if (!int.TryParse(input, out int pathWindChance))
                        return "Path winding chance is not a valid number";
                    if (pathWindChance is < 0 or > 100) return "Path winding chance is out of range";
                    break;
                case 6:
                    if (!bool.TryParse(input.ToLower(), out _))
                        return "Should cut dead ends is not true or false";
                    break;
                case 7:
                    if (!bool.TryParse(input.ToLower(), out _))
                        return "Should instantly generate is not true or false";
                    break;
                case 8:
                    if (!int.TryParse(input, out _))
                        return "Seed is not a valid number";
                    break;
            }
            return "";
        }

        private static void PlayDungeon(Dungeon dungeon, string saveName)
        {
            bool shouldExit = false;
            int moveCount = 0;
            while (!shouldExit)
            {
                Vec playerPos = dungeon.playerPos;
                dungeon.cells[playerPos.x, playerPos.y] = playerIndex;
                SetCursorPosition((playerPos.x + 1) * tileWidth, playerPos.y + 1);
                Write(tiles[playerIndex]);
                CursorLeft -= tileWidth;
                ConsoleKey keyPressed = 0;
                while (keyPressed is < ConsoleKey.LeftArrow or > ConsoleKey.DownArrow && keyPressed != ConsoleKey.Enter)
                {
                    keyPressed = ReadKey(true).Key;
                }
                if (keyPressed == ConsoleKey.Enter)
                {
                    Clear();
                    ConsoleManager.SetUpMenuConsole();
                    WriteLine("What do you want to call the save ?");
                    string errorMessage = " ";
                    Write(saveName);
                    CursorLeft = 0;
                    //string saveName = "";
                    while (errorMessage.Length > 0)
                    {
                        keyPressed = ReadKey(false).Key;
                        if (keyPressed != ConsoleKey.Enter)
                        {
                            Write("               ");
                        }
                        CursorLeft = 1;
                        saveName = (keyPressed + ReadLine()).ToLower();
                        if (saveName.Length == 0)
                        {
                            errorMessage = "Save name is too short";
                        }
                        else if (saveName.Any(c => invalidFileChars.Contains(c)))
                        {
                            errorMessage = "Save name contains invalid characters";
                        }
                        else errorMessage = "";
                        CursorTop--;
                    }
                    SaveDungeon(dungeon, saveName);
                    return;
                }

                Direction direction = keyPressed.GetDirection();
                Vec testPos = playerPos + direction.GetVector();
                if (dungeon.IsWithinBounds(testPos) && dungeon.GetCell(testPos) != wallIndex)
                {
                    dungeon.cells[playerPos.x, playerPos.y] = floorIndex;
                    dungeon.playerPos = testPos;
                    Write(tiles[floorIndex]);
                    moveCount++;
                }
                shouldExit = testPos == dungeon.exitPos;
            }
            Clear();
            ConsoleManager.SetUpMenuConsole();
            WriteLine("You have won !");
            WriteLine($"You took {moveCount} moves");
            WriteLine("Press Enter to return to menu...");
            ReadLine();
        }
        private static void GenerateFiles()
        {
            if (!File.Exists(saveNamesFileName))
            {
                File.Create(saveNamesFileName);
            }
            if (!File.Exists(genSettingsFileName))
            {
                File.Create(genSettingsFileName);
            }
        }

        private static void SaveDungeon(Dungeon dungeon, string saveName)
        {
            StringBuilder lines = new();
            lines.AppendLine($"Size: {dungeon.size.x.ToString()},{dungeon.size.y.ToString()}");
            lines.AppendLine($"PlayerPos: {dungeon.playerPos.x.ToString()},{dungeon.playerPos.y.ToString()}");
            lines.AppendLine($"ExitPos: {dungeon.exitPos.x.ToString()},{dungeon.exitPos.y.ToString()}");
            for (int y = 0; y < dungeon.size.y; y++)
            {
                for (int x = 0; x < dungeon.size.x; x++)
                {
                    lines.Append(dungeon.cells[x, y]);
                }
                lines.AppendLine();
            }

            lines.AppendLine("Chests:");
            //TODO: save chest data
            string fileName = saveName + ".txt";
            if (!File.Exists(fileName)) File.AppendAllText(saveNamesFileName, saveName + "\n");
            File.WriteAllText(fileName, lines.ToString());
        }

        private static void SyncSaves()
        {
            
        }

        private static int CalculateFontSize(Vec mazeSize)
        {
            return (int)Math.Min(Math.Ceiling(535d / (mazeSize.y + 2)), Math.Ceiling(955d / (mazeSize.x + 2)));
        }

        private static int GetRnd(int min, int max) //inclusive on both ends
        {
            return random.Next(min, max + 1);
        }

        private static int ToInt(this char c)
        {
            return c - '0';
        }

        private class Dungeon
        {
            public readonly Vec size;
            public Vec playerPos, exitPos;
            public int[,] cells;
            private List<Room> roomList;
            private Chest[] chests;
            public int playerBombCount, playerGoldCount;
            public Dungeon(Vec size = default, Vec playerPos = default, Vec exitPos = default)
            {
                this.size = size;
                if (playerPos == default) playerPos = Vec.zero;
                this.playerPos = playerPos;
                if (exitPos == default) exitPos = size - Vec.one;
                this.exitPos = exitPos;
                cells = new int[size.x, size.y];
                roomList = new List<Room>();
                chests = new Chest[]{};
            }

            public void Carve(Vec pos, bool showChanges)
            {
                Carve(pos.x, pos.y, showChanges);
            }

            public void Carve(int x, int y, bool showChanges)
            {
                if (!IsWithinBounds(x, y)) return;
                cells[x, y] = floorIndex;
                if (!showChanges) return;
                SetCursorPosition((x + 1) * tileWidth, y + 1);
                Write(tiles[floorIndex]);
            }

            public bool IsWithinBounds(Vec pos)
            {
                return IsWithinBounds(pos.y, pos.x);
            }
            public bool IsWithinBounds(int x, int y)
            {
                return y < size.y && y > -1 && x < size.x && x > -1;
            }

            public int GetCell(Vec pos)
            {
                return cells[pos.x, pos.y];
            }

            public void Display()
            {
                StringBuilder lines = new StringBuilder();
                lines.Append('█', (size.x + 2) * tileWidth);
                lines.AppendLine();
                for (int y = 0; y < size.y; y++)
                {
                    lines.Append(tiles[wallIndex]);
                    for (int x = 0; x < size.x; x++)
                    {
                        lines.Append(tiles[cells[x, y]]);
                    }

                    lines.AppendLine(tiles[wallIndex]);
                }

                lines.Append('█', (size.x + 2) * tileWidth);
                lines.AppendLine();
                WriteLine(lines.ToString());
                //needs to be done here so colours can be changed
                ForegroundColor = ConsoleColor.Red;
                SetCursorPosition((playerPos.x + 1) * tileWidth, playerPos.y + 1);
                Write(tiles[playerIndex]);
                SetCursorPosition((exitPos.x + 1) * tileWidth, exitPos.y + 1);
                Write(tiles[exitIndex]);
                
                ForegroundColor = ConsoleColor.Cyan;
                for (int i = 0; i < chests.Length; i++)
                {
                    SetCursorPosition((chests[i].pos.x + 1) * tileWidth, chests[i].pos.y + 1);
                    Write(tiles[chestIndex]);
                }
                
                ForegroundColor = ConsoleColor.Gray;
                SetCursorPosition(0, size.y + 3);
                WriteLine("Inventory:");
                WriteLine($"{playerGoldCount.ToString()}x Gold");
                WriteLine($"{playerBombCount.ToString()}x Bombs");
            }

            public void Generate(GenSettings genSettings, bool showGen)
            {
                random = genSettings.seed == 0 ? new Random() : new Random(genSettings.seed);
                Display();

                if (showGen) ReadLine();
                AddRooms(genSettings, showGen);
                if (showGen) ReadLine();
                GrowMaze(genSettings, showGen);
                AddDoors(genSettings, showGen);
                if (genSettings.showGen) ReadLine();
                if (genSettings.cutDeadEnds)
                {
                    RemoveDeadEnds(genSettings);
                    if (showGen) ReadLine();
                }
                AddLootChests(genSettings);
            }
            
            private void AddRooms(GenSettings genSettings, bool showGen)
            {
                //Add rooms
                int modCount = 1, sleepTime = 0;
                if (showGen)
                {
                    modCount = (int)((genSettings.roomTryCount ^ 2) * 30d / 700d + 1d);
                    sleepTime = 100 / genSettings.roomTryCount;
                }
            
                int cellCount = 0;
                for (int i = 0; i < genSettings.roomTryCount; i++)
                {
                    int tempSize = GetRnd(3, 3 + genSettings.roomExtraSize) * 2 + 1;
                    int rectangularity = GetRnd(0, 1 + tempSize / 2) * 2;
                    Vec roomSize = new Vec(tempSize, tempSize);
                    if (GetRnd(0, 1) == 1) roomSize.x += rectangularity;
                    else roomSize.y += rectangularity;
                    Vec newRoomPos = new Vec(GetRnd(0, (size.x - roomSize.x) / 2) * 2,
                        GetRnd(0, (size.y - roomSize.y) / 2) * 2);
                    if (newRoomPos.x < 2 && newRoomPos.y < 2) continue;
                    if (roomList.Any(r => newRoomPos.x <= r.pos.x + r.size.x + 2 &&
                                          newRoomPos.x + roomSize.x + 2 >= r.pos.x &&
                                          newRoomPos.y <= r.pos.y + r.size.y + 2 &&
                                          newRoomPos.y + roomSize.y + 2 >= r.pos.y)) continue;
                    Room newRoom = new Room(newRoomPos, roomSize);
                    roomList.Add(newRoom);
                    //carving room out
                    for (int x = 0; x < roomSize.x; x++)
                    {
                        for (int y = 0; y < roomSize.y; y++)
                        {
                            Carve(newRoomPos.x + x, newRoomPos.y + y, true);
                            if (modCount > 0 && cellCount % modCount == 0)
                            {
                                Thread.Sleep(sleepTime);
                            }
                        }
                    }
                }
            
                SetCursorPosition(0, 0);
                Display();
            }
            
            private void GrowMaze(GenSettings genSettings, bool showGen)
            {
                //hunt and kill generation
                int modCount = 1, sleepTime = 0;
                if (showGen)
                {
                    modCount = (int)(genSettings.size.x * genSettings.size.y * 1.4d / 1000 + 1);
                    sleepTime = 900 / (genSettings.size.x + genSettings.size.y);
                }
            
                //grow dungeon
                List<Vec> visitedCells = new();
                Direction lastDirection = Direction.None;
                Carve(0, 0, false);
            
                visitedCells.Add(Vec.zero);
                while (visitedCells.Count > 0)
                {
                    Vec currentCell = visitedCells.Last();
                    List<Direction> availableDirections = new();
                    for (int i = 0; i < 4; i++)
                    {
                        Vec nextCell = currentCell + GetVector((Direction)i) * 2;
                        if (IsWithinBounds(nextCell) && GetCell(nextCell) == wallIndex)
                            availableDirections.Add((Direction)i);
                    }
            
                    if (availableDirections.Count == 0)
                    {
                        visitedCells.RemoveAt(visitedCells.Count - 1);
                        lastDirection = Direction.None;
                        continue;
                    }
            
                    //applying windiness
                    Direction direction;
                    if (availableDirections.Contains(lastDirection) && GetRnd(1, 100) > genSettings.windChance)
                        direction = lastDirection;
                    else direction = availableDirections[GetRnd(0, availableDirections.Count - 1)];
                    //carving
                    Vec cell1 = currentCell + direction.GetVector();
                    Vec cell2 = currentCell + direction.GetVector() * 2;
                    Carve(cell1, true);
                    Carve(cell2, true);
                    visitedCells.Add(cell2);
                    lastDirection = direction;
                    //ReadKey(true);
                    if (modCount > 0 && visitedCells.Count % modCount == 0) Thread.Sleep(sleepTime);
                }
            }
            
            private void AddDoors(GenSettings genSettings, bool showGen)
            {
                //connect rooms to maze
                for (int i = 0; i < roomList.Count; i++)
                {
                    Room room = roomList[i];
                    int doorsToBeAdded = 1;
                    if (GetRnd(1, 100) < genSettings.extraDoorChance) doorsToBeAdded++;
                    while (doorsToBeAdded > 0)
                    {
                        bool isOnNS = GetRnd(0, 1) == 1;
                        int isOnSW = GetRnd(0, 1);
                        Direction direction;
                        Vec holePos = new();
                        if (isOnNS)
                        {
                            direction = (Direction)(isOnSW * 2); //function so 0=>0, 1=>2
                            holePos.x = GetRnd(0, room.size.x - 1);
                            holePos.y = isOnSW * (room.size.y - 1) + (2 * isOnSW - 1); //function so 0=>-1, 1=>1
                        }
                        else
                        {
                            direction = (Direction)(3 - isOnSW * 2); //function so 0=>3, 1=>1
                            holePos.y = GetRnd(0, room.size.y - 1);
                            holePos.x = isOnSW * (room.size.x - 1) + (2 * isOnSW - 1); //function so 0=>-1, 1=>1
                        }
            
                        holePos += room.pos;
                        Vec holePosAdded = holePos + GetVector(direction);
                        if (!IsWithinBounds(holePosAdded) || GetCell(holePosAdded) != floorIndex) continue;
                        Carve(holePos, true);
                        doorsToBeAdded--;
                    }
                }
            }
            
            private void RemoveDeadEnds(GenSettings genSettings)
            {
                int modCount;
                int sleepTime;
                //remove dead ends
                modCount = 1;
                sleepTime = 0;
                if (genSettings.showGen)
                {
                    modCount = (int)(genSettings.size.x * genSettings.size.y * 1.4d / 1000 + 1);
                    sleepTime = 900 / (genSettings.size.x + genSettings.size.y);
                }
            
                int removedCount = 0;
                for (int y = 0; y < size.y; y++)
                {
                    for (int x = 0; x < size.x; x++)
                    {
                        if (cells[x, y] == wallIndex) continue;
                        Vec currentCell = new Vec(x, y);
                        if (currentCell == exitPos || currentCell == Vec.zero) continue;
                        List<Direction> exits = new();
                        while (true)
                        {
                            exits.Clear();
                            for (int i = 0; i < 4; i++)
                            {
                                Vec nextCell = currentCell + GetVector((Direction)i);
                                if (IsWithinBounds(nextCell) && GetCell(nextCell) == floorIndex)
                                    exits.Add((Direction)i);
                            }
            
                            if (exits.Count > 1) break;
                            cells[currentCell.x, currentCell.y] = wallIndex;
                            SetCursorPosition((currentCell.x + 1) * tileWidth, currentCell.y + 1);
                            Write(tiles[wallIndex]);
                            if (exits.Count == 0) break;
                            currentCell += GetVector(exits[0]);
                            removedCount++;
                            if (modCount > 0 && removedCount % modCount == 0)
                            {
                                Thread.Sleep(sleepTime);
                            }
                        }
                    }
                }
            }

            private void AddLootChests(GenSettings genSettings)
            {
                List<Chest> chestList = new();
                ForegroundColor = ConsoleColor.Cyan;
                for (int i = 0; i < roomList.Count; i++)
                {
                    if (GetRnd(0, 100) > genSettings.lootChestChance) continue;
                    Room room = roomList[i];
                    Vec chestPos = new Vec(room.pos.x + room.size.x / 2, room.pos.y + room.size.y / 2);
                    cells[chestPos.x, chestPos.y] = chestIndex;
                    SetCursorPosition((chestPos.x + 1) * tileWidth, chestPos.y + 1);
                    Write(tiles[chestIndex]);
                    //Adding items
                    int goldCount = GetRnd(0, 10);
                    int bombCount = GetRnd(0, 100) > 70 ? 1 : 0;
                    if (GetRnd(0, 100) > 95) bombCount++;
                    Chest chest = new Chest(chestPos, goldCount, bombCount);
                    chestList.Add(chest);
                }
                ForegroundColor = ConsoleColor.Gray;
                chests = chestList.ToArray();
            }
        }

        private struct Chest
        {
            public readonly Vec pos;
            public int goldCount, bombCount;

            public Chest(Vec position, int goldAmount, int bombAmount)
            {
                pos = position;
                goldCount = goldAmount;
                bombCount = bombAmount;
            }
        }

        private struct Room
        {
            public readonly Vec pos;
            public readonly Vec size;
            public Room(Vec position, Vec roomSize)
            {
                pos = position;
                size = roomSize;
            }
        }

        private struct GenSettings
        {
            public int seed, lootChestChance;
            public Vec size;
            public int roomTryCount, extraDoorChance, roomExtraSize, windChance;
            public bool cutDeadEnds, showGen;
        }

        private struct Vec
        {
            public int x, y;
            public static readonly Vec one = new Vec(1, 1);
            public static readonly Vec zero = new Vec(0, 0);

            public Vec(int xPos, int yPos)
            {
                x = xPos;
                y = yPos;
            }
            public static Vec operator +(Vec vec1, Vec vec2)
            {
                return new Vec(vec1.x + vec2.x, vec1.y + vec2.y);
            }
            public static bool operator ==(Vec vec1, Vec vec2)
            {
                return vec1.x == vec2.x && vec1.y == vec2.y;
            }
            public static bool operator !=(Vec vec1, Vec vec2)
            {
                return vec1.x != vec2.x || vec1.y != vec2.y;
            }
            public static Vec operator -(Vec vec1, Vec vec2)
            {
                return new Vec(vec1.x - vec2.x, vec1.y - vec2.y);
            }
            public static Vec operator *(Vec vec, int multiplier)
            {
                return new Vec(vec.x * multiplier, vec.y * multiplier);
            }
        }

        private enum Direction
        {
            Up, Right, Down, Left, None
        }

        private static Direction GetDirection(this ConsoleKey key)
        {
            return key switch
            {
                ConsoleKey.LeftArrow => Direction.Left,
                ConsoleKey.UpArrow => Direction.Up,
                ConsoleKey.RightArrow => Direction.Right,
                ConsoleKey.DownArrow => Direction.Down,
                _ => Direction.None
            };
        }

        private static Vec GetVector(this Direction direction)
        {
            return direction switch
            {
                Direction.Up => new Vec(0, -1),
                Direction.Right => new Vec(1, 0),
                Direction.Down => new Vec(0, 1),
                Direction.Left => new Vec(-1, 0),
                _ => Vec.zero
            };
        }

        private static class ConsoleManager
        {
            private static IntPtr STD_OUTPUT_HANDLE = new(-11);
            private static IntPtr INVALID_HANDLE_VALUE = new(-1);
            private const int LfFacesize = 32;

            public static void SetUpMenuConsole()
            {
                SetupConsole(menuFontWeight, menuFontFamily, menuFontSize, menuFontName);
            }
            public static void SetupConsole(uint fontWeight, uint fontFamily, short fontSize, string fontName)
            {
                IntPtr stdHandle = GetStdHandle(STD_OUTPUT_HANDLE);
                checked
                {
                    if (stdHandle != INVALID_HANDLE_VALUE)
                    {
                        CONSOLE_FONT_INFOEX newInfo = new CONSOLE_FONT_INFOEX();
                        newInfo.cbSize = (uint)Marshal.SizeOf(newInfo);
                        GetCurrentConsoleFontEx(stdHandle, false, ref newInfo);
                        newInfo.FontFamily = fontFamily;
                        newInfo.FontWeight = fontWeight;
                        newInfo.FaceName = fontName;
                        newInfo.dwFontSize = new COORD(fontSize, fontSize);
                        SetCurrentConsoleFontEx(stdHandle, false, ref newInfo);
                    }
                    SetWindowSize(LargestWindowWidth, LargestWindowHeight);
                    SetBufferSize(WindowWidth, WindowHeight);
                }
            }
            
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            private struct CONSOLE_FONT_INFOEX
            {
                public uint cbSize;
                public int nFont;
                public COORD dwFontSize;
                public uint FontFamily;
                public uint FontWeight;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
                public string FaceName;
            }
        
            private struct COORD
            {
                private short X;
                private short Y;
                public COORD(short x, short y)
                {
                    X = x;
                    Y = y;
                }
            }

            [DllImport("Kernel32.dll", SetLastError = true)]
            private static extern bool SetCurrentConsoleFontEx(IntPtr hConsoleOutput, bool bMaximumWindow, ref CONSOLE_FONT_INFOEX lpConsoleCurrentFontEx);
            [DllImport("Kernel32.dll", SetLastError = true)]
            private static extern bool GetCurrentConsoleFontEx(IntPtr hConsoleOutput, bool bMaximumWindow, ref CONSOLE_FONT_INFOEX lpConsoleCurrentFontEx);
            [DllImport("Kernel32.dll", SetLastError = true)]
            private static extern IntPtr GetStdHandle(IntPtr nStdHandle);
            
            //code to maximise console window
            private const int SW_SHOWMAXIMISED = 3;
            public static void MaximiseConsole()
            {
                ShowWindow(GetConsoleWindow(), SW_SHOWMAXIMISED);
                SetBufferSize(WindowWidth, WindowHeight);
            }

            [DllImport("kernel32.dll", ExactSpelling = true)]
            private static extern IntPtr GetConsoleWindow();

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        }
    }
}
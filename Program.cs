using static System.Console;
using System;
using System.Collections.Specialized;
using System.Text;

namespace NEA_Dungeon_Game
{
    internal class Program
    {
        private const string saveNamesFileName = "saveNames.txt";
        private const string genSettingsFileName = "genSettings.txt";
        private const string wall = "██";
        private const string floor = "  ";
        private const string player = "><";
        private const string exitTile = "██";
        private const int initialFontWeight = 400;
        private const int initialFontSize = 36;
        private const string initialFontName = "Lucida Sans Typewriter";
        public static void Main(string[] args)
        {
            
        }

        private static int MainMenu()
        {
            ConsoleKey keyPressed = 0;
            const int topPos = 0, bottomPos = 3;
            int cursorPos = 0;
            WriteLine(" Load Maze\n Generate new maze\n Change generation parameters\n Exit");
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

        private struct vec
        {
            public int x, y;
            public static readonly vec one = new vec(1, 1);
            private static readonly vec zeroVec = new vec(0, 0);
            public static vec zero => zeroVec;

            public vec(int xPos, int yPos)
            {
                x = xPos;
                y = yPos;
            }
            public static vec operator +(vec vec1, vec vec2)
            {
                return new vec(vec1.x + vec2.x, vec1.y + vec2.y);
            }
            public static bool operator ==(vec vec1, vec vec2)
            {
                return vec1.x == vec2.x && vec1.y == vec2.y;
            }
            public static bool operator !=(vec vec1, vec vec2)
            {
                return vec1.x != vec2.x || vec1.y != vec2.y;
            }
            public static vec operator -(vec vec1, vec vec2)
            {
                return new vec(vec1.x - vec2.x, vec1.y - vec2.y);
            }
        }

        private void DisplayDungeon(Dungeon dungeon)
        {
            StringBuilder lines = new StringBuilder();
            lines.Append('█', dungeon.size.x + 2);
            lines.AppendLine();
            for (int y = 0; y < dungeon.size.y; y++)
            {
                lines.Append(wall);
                for (int x = 0; x < dungeon.size.x; x++)
                {
                    lines.Append()
                }
            }
            
        }

        private class Dungeon
        {
            public readonly vec size;
            public vec playerPos, exitPos;
            public string[,] cells;
            public Dungeon(vec size, vec playerPos = new vec(), vec exitPos = default)
            {
                this.size = size;
                this.playerPos = playerPos;
                if (exitPos == default) exitPos = size;
                this.exitPos = exitPos;
                cells = new string[size.x, size.y];
            }

            public int GetCell(vec pos)
            {
                if (pos.y < size.y && pos.y > -1 && pos.x < size.x && pos.x > -1) return cells[pos.x, pos.y];
                return -1;
            }

            public int GetCell(int x, int y)
            {
                if (y < size.y && y > -1 && x < size.x && x > -1) return cells[x, y];
                return -1;
            }
        }
    }
}
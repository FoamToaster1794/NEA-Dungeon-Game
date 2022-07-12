using static System.Console;
using System;
using System.Collections.Specialized;
using System.ComponentModel;

namespace NEA_Dungeon_Game
{
    internal class Program
    {
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
            private static readonly vec zeroVec = new vec(1, 1);
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

        private struct maze
        {
            public vec mazeSize, playerPos, exitPos;
            public int[,] cells;
            public maze(vec mazeSize, vec playerPos = new vec(), vec exitPos = default)
            {
                this.mazeSize = mazeSize;
                this.playerPos = playerPos;
                if (exitPos == default) exitPos = mazeSize;
                this.exitPos = exitPos;
                cells = new int[mazeSize.x, mazeSize.y];
            }

            public int GetCell(vec pos)
            {
                if (pos.y < mazeSize.y && pos.y > -1 && pos.x < mazeSize.x && pos.x > -1) return cells[pos.x, pos.y];
                return -1;
            }
        }
    }
}
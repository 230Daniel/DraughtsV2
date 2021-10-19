using System.Linq;
using Draughts.GameLogic;

namespace Draughts.Console
{
    internal class Program
    {
        private static void Main()
        {
            var board = new Board();
            while (board.Winner == -1)
            {
                System.Console.Clear();
                PrintBoard(board);
                System.Console.WriteLine($"\nPlayer to move: {board.NextPlayer}\n");
                
                System.Console.Write("Origin: ");
                var origin = System.Console.ReadLine().Split(", ").Select(int.Parse).ToArray();
                System.Console.Write("Destination: ");
                var destination = System.Console.ReadLine().Split(", ").Select(int.Parse).ToArray();

                board.TakeMove((origin[0], origin[1]), (destination[0], destination[1]));
            }

            System.Console.Clear();
            System.Console.WriteLine($"Winner: {board.Winner}!");
        }

        private static void PrintBoard(Board board)
        {
            for (var y = 0; y < 8; y++)
            {
                if (y % 2 == 1) System.Console.Write("   ");
                for (var x = 0; x < 4; x++)
                {
                    System.Console.Write($"{board.Tiles[y][x]}".PadRight(2));
                    System.Console.Write("   ");
                }
                System.Console.Write("\n");
            }
        }
    }
}

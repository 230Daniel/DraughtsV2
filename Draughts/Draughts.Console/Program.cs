using System.Text;
using Draughts.GameLogic;

namespace Draughts.Console
{
    internal class Program
    {
        private static void Main()
        {
            // Create a new board and continue until someone wins
            var board = new Board();
            while (board.Winner == -1)
            {
                // Clear the console, print the board, and print instructions
                System.Console.Clear();
                PrintBoard(board);
                System.Console.WriteLine($"{(board.NextPlayer == 0 ? "Black" : "White")} to move.\n" +
                                         "Enter two Letter-Number coordinates seperated by a space.\n");
                
                // Get a move input until a valid move is provided and made
                var moveMade = false;
                while (!moveMade)
                {
                    var (origin, destination) = GetMoveInput();
                    moveMade = board.TakeMove(origin, destination);
                    if (!moveMade) System.Console.WriteLine("Invalid move");
                }
            }
            
            // Someone won so clear the console, print the board, and print who won.
            System.Console.Clear();
            PrintBoard(board);
            System.Console.WriteLine($"\n{(board.Winner == 0 ? "Black" : "White")} won!");
        }

        private static void PrintBoard(Board board)
        {
            var tileValueIcons = new[] {" / ", " b ", " w ", " B ", " W "};

            var boardBuilder = new StringBuilder();
            boardBuilder.AppendLine("     A  B  C  D  E  F  G  H");
            boardBuilder.AppendLine("  ╔══════════════════════════╗");
            
            for (var y = 0; y < 8; y++)
            {
                boardBuilder.Append($"{y + 1} ║ ");
                if (y % 2 == 0) boardBuilder.Append("   ");
                for (var x = 0; x < 4; x++)
                {
                    boardBuilder.Append(tileValueIcons[board.Tiles[y][x] + 1]);
                    if(x != 3 || y % 2 == 1) boardBuilder.Append("   ");
                }
                boardBuilder.Append($" ║ {y + 1}");

                if (y == 2) boardBuilder.Append("     b = Black pawn");
                if (y == 3) boardBuilder.Append("     w = White pawn");
                if (y == 4) boardBuilder.Append("     B = Black king");
                if (y == 5) boardBuilder.Append("     W = White king");

                boardBuilder.AppendLine();
            }
            
            boardBuilder.AppendLine("  ╚══════════════════════════╝");
            boardBuilder.AppendLine("     A  B  C  D  E  F  G  H ");
            
            System.Console.WriteLine(boardBuilder);
        }

        private static ((int, int), (int, int)) GetMoveInput()
        {
            // Loop until the user provides a input and we return it (escaping the loop)
            while (true)
            {
                var inputs = System.Console.ReadLine().Split(" ");
                
                // Validate the input type
                if (inputs.Length != 2 || inputs[0].Length != 2 || inputs[1].Length != 2)
                {
                    System.Console.WriteLine("Please input a valid pair of coordinates, eg. \"B3 C4\"");
                    continue;
                }
                
                // Translate the letter components to x positions
                var originX = GetXFromLetter(inputs[0][0]);
                var destX = GetXFromLetter(inputs[1][0]);

                // Check that the letter components were translated without issue
                // and that the number components are numbers
                if (originX is -1 
                    || destX is -1 
                    || !int.TryParse(inputs[0][1].ToString(), out var originY) 
                    || !int.TryParse(inputs[1][1].ToString(), out var destY))
                {
                    System.Console.WriteLine("Please input a valid pair of coordinates, eg. \"B3 C4\"");
                    continue;
                }
                
                // Because the Y labels are 1 to 4 but programatically we use 0 to 3, subtract one from the y values
                originY--;
                destY--;

                // Return the specified move, escaping the while true loop
                return ((originX, originY), (destX, destY));
            }
        }
        
        private static int GetXFromLetter(char letter)
            => letter switch
            {
                'A' => 0,
                'B' => 0,
                'C' => 1,
                'D' => 1,
                'E' => 2,
                'F' => 2,
                'G' => 3,
                'H' => 3,
                _ => -1
            };
    }
}

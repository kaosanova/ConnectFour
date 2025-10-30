using ConnectFourSpel.DAL;
using ConnectFourSpel.Models;
using Microsoft.AspNetCore.Mvc;

namespace ConnectFourSpel.Controllers
{
    public class GameController : Controller
    {
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult New()
        {
            var sessionId = HttpContext.Session.Id;
            var gameId = GameMethods.Create(sessionId);
            return RedirectToAction(nameof(Play), new { id = gameId });
        }
        [HttpGet]
        public IActionResult Play(int id)
        {
            var vm = GameMethods.GetWithMoves(id);
            if (vm == null) return NotFound();

            var board = RebuildBoard(vm.Moves); // helper nedan
            ViewBag.GameId = id;
            return View(board); // Play.cshtml: @model GameBoard
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Drop(int id, int col)
        {
            var vm = GameMethods.GetWithMoves(id);
            if (vm == null) return NotFound();

            var board = RebuildBoard(vm.Moves);

            // Validera kolumn
            var cols = board.Grid.GetLength(1);
            if (col < 0 || col >= cols)
            {
                TempData["Err"] = "Ogiltig kolumn.";
                return RedirectToAction(nameof(Play), new { id });
            }

            // Hitta landningsrad
            var row = FindLandingRow(board, col);
            if (row < 0)
            {
                TempData["Err"] = "Kolumnen är full.";
                return RedirectToAction(nameof(Play), new { id });
            }

            // Bestäm spelare från antal drag
            var playerNo = (vm.Moves.Count % 2 == 0) ? 1 : 2;
            var cell = (playerNo == 1) ? CellState.Player1 : CellState.Player2;

            // Uppdatera brädet och spara draget
            board.Grid[row, col] = cell;
            MoveMethods.Add(id, playerNo, col, row);

            // Vinst?
            if (IsWinningMove(board, row, col, cell))
            {
                GameMethods.Delete(id); // din policy: radera när klart
                                        // Visa gärna ett meddelande på nästa sida om du vill
                return RedirectToAction(nameof(Done));
            }

            // Tillbaka till brädet
            return RedirectToAction(nameof(Play), new { id });
        }

        [HttpGet] public IActionResult Done() => View();

        // ===== Helpers (enkla, behåll i controllern) =====

        private static GameBoard RebuildBoard(IList<MoveDetails> moves)
        {
            var board = new GameBoard(); // default 6x7
            foreach (var m in moves.OrderBy(x => x.MoveNo))
            {
                var cell = (m.PlayerNo == 1) ? CellState.Player1 : CellState.Player2;
                board.Grid[m.Row, m.Col] = cell;
            }
            // Sätt INTE board.CurrentTurn här – setter är inte publik och behövs inte.
            return board;
        }

        private static int FindLandingRow(GameBoard board, int col)
        {
            var rows = board.Grid.GetLength(0);
            for (int r = rows - 1; r >= 0; r--)
                if (board.Grid[r, col] == CellState.Empty)
                    return r;
            return -1;
        }

        private static bool IsWinningMove(GameBoard b, int r, int c, CellState cell)
        {
            int R = b.Grid.GetLength(0), C = b.Grid.GetLength(1);
            int CountDir(int dr, int dc)
            {
                int cnt = 0, i = r, j = c;
                while (i >= 0 && i < R && j >= 0 && j < C && b.Grid[i, j] == cell) { cnt++; i += dr; j += dc; }
                return cnt - 1;
            }
            int line(int dr, int dc) => 1 + CountDir(dr, dc) + CountDir(-dr, -dc);
            return line(0, 1) >= 4 || line(1, 0) >= 4 || line(1, 1) >= 4 || line(1, -1) >= 4;
        }
    }
}

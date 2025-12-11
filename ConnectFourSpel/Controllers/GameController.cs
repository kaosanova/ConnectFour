using ConnectFourSpel.DAL;
using ConnectFourSpel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ConnectFourSpel.Controllers
{
    [Authorize]
    public class GameController : Controller
    {
        // Skapa nytt spel (inloggad användare blir Player1)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult New()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var gameId = GameMethods.Create(userId);
            return RedirectToAction(nameof(Play), new { id = gameId });
        }

        // Spelare 2 går med i ett befintligt spel
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Join(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var ok = GameMethods.AddPlayer2(id, userId);
            if (!ok)
            {
                TempData["Err"] = "Kunde inte ansluta till spelet (någon annan kanske redan gått med?).";
                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction(nameof(Play), new { id });
        }

        // Visa spelbrädet
        [HttpGet]
        public IActionResult Play(int id)
        {
            var vm = GameMethods.GetWithMoves(id);
            if (vm == null) return NotFound();

            // Om spelet är avslutat -> skicka spelare till Done-sidan
            if (vm.Game.Status == 2)
            {
                return RedirectToAction(nameof(Done), new { id });
            }

            var board = RebuildBoard(vm.Moves);

            ViewBag.GameId = id;
            ViewBag.MovesCount = vm.Moves.Count;
            ViewBag.Game = vm.Game;

            return View(board);
        }

        // Hantera drag (drop)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Drop(int id, int col)
        {
            var vm = GameMethods.GetWithMoves(id);
            if (vm == null) return NotFound();

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // kontroll: är du med i spelet?
            if (vm.Game.Player1Id != userId && vm.Game.Player2Id != userId)
                return Forbid();

            // Om spelet redan är slut -> skicka till Done
            if (vm.Game.Status == 2)
            {
                return RedirectToAction(nameof(Done), new { id });
            }

            var board = RebuildBoard(vm.Moves);

            // validera kolumn
            var cols = board.Grid.GetLength(1);
            if (col < 0 || col >= cols)
            {
                TempData["Err"] = "Ogiltig kolumn.";
                return RedirectToAction(nameof(Play), new { id });
            }

            // hitta ledig rad
            var row = FindLandingRow(board, col);
            if (row < 0)
            {
                TempData["Err"] = "Kolumnen är full.";
                return RedirectToAction(nameof(Play), new { id });
            }

            // räkna ut vilken spelare som ska spela nu (turordning)
            var playerNo = (vm.Moves.Count % 2 == 0) ? 1 : 2;
            var expectedUserId = playerNo == 1
                ? vm.Game.Player1Id
                : vm.Game.Player2Id ?? -1;

            // fel spelare försöker spela
            if (userId != expectedUserId)
            {
                TempData["Err"] = "Det är inte din tur.";
                return RedirectToAction(nameof(Play), new { id });
            }

            var cell = (playerNo == 1) ? CellState.Player1 : CellState.Player2;

            // uppdatera brädet och spara draget
            board.Grid[row, col] = cell;
            MoveMethods.Add(id, playerNo, col, row);

            // kolla vinst
            if (IsWinningMove(board, row, col, cell))
            {
                var winnerUserId = playerNo == 1
                    ? vm.Game.Player1Id
                    : vm.Game.Player2Id!.Value;

                GameMethods.SetWinner(id, winnerUserId);

                TempData["Msg"] = "Spelet är slut!";
                return RedirectToAction(nameof(Done), new { id });
            }

            // inget slut än -> tillbaka till Play
            return RedirectToAction(nameof(Play), new { id });
        }

        // Visa slutskärm
        [HttpGet]
        public IActionResult Done(int id)
        {
            var game = GameMethods.Get(id);
            if (game == null) return NotFound();

            string? winnerName = null;
            if (game.WinnerUserId.HasValue)
            {
                var winner = UserMethods.GetById(game.WinnerUserId.Value);
                winnerName = winner?.Username ?? $"Spelare med id {game.WinnerUserId}";
            }

            ViewBag.GameId = id;
            ViewBag.WinnerName = winnerName;

            return View();
        }

        // ---------------- Hjälpmetoder ----------------

        private static GameBoard RebuildBoard(IList<MoveDetails> moves)
        {
            var board = new GameBoard();
            foreach (var m in moves.OrderBy(x => x.MoveNo))
            {
                var cell = (m.PlayerNo == 1) ? CellState.Player1 : CellState.Player2;
                board.Grid[m.Row, m.Col] = cell;
            }
            return board;
        }

        private static int FindLandingRow(GameBoard board, int col)
        {
            var rows = board.Grid.GetLength(0);
            for (int r = rows - 1; r >= 0; r--)
            {
                if (board.Grid[r, col] == CellState.Empty)
                    return r;
            }
            return -1;
        }

        private static bool IsWinningMove(GameBoard b, int r, int c, CellState cell)
        {
            int R = b.Grid.GetLength(0), C = b.Grid.GetLength(1);

            int CountDir(int dr, int dc)
            {
                int cnt = 0, i = r, j = c;
                while (i >= 0 && i < R && j >= 0 && j < C && b.Grid[i, j] == cell)
                {
                    cnt++;
                    i += dr;
                    j += dc;
                }
                return cnt - 1;
            }

            int line(int dr, int dc) => 1 + CountDir(dr, dc) + CountDir(-dr, -dc);

            return line(0, 1) >= 4   // horisontellt
                || line(1, 0) >= 4   // vertikalt
                || line(1, 1) >= 4   // diagonal \
                || line(1, -1) >= 4; // diagonal /
        }
    }
}

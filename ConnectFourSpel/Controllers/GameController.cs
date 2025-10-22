using ConnectFourSpel.Models;
using ConnectFourSpel.Services;
using Microsoft.AspNetCore.Mvc;

namespace ConnectFourSpel.Controllers;

public class GameController : Controller
{
    private readonly IGameStore _store;
    public GameController(IGameStore store) => _store = store;

    // Skapa nytt spel och gå till spelplanen
    [HttpGet]
    public IActionResult New()
    {
        var id = _store.CreateNew();
        return RedirectToAction(nameof(Play), new { id });
    }

    // Visa ett spel
    [HttpGet]
    public IActionResult Play(string id)
    {
        var board = _store.Get(id);
        if (board is null) return NotFound();

        ViewBag.GameId = id;
        return View(board); // renderar Views/Game/Play.cshtml
    }

    // Släpp en bricka i kolumn
    [HttpPost]
    public IActionResult Drop(string id, int col)
    {
        var board = _store.Get(id);
        if (board is null) return NotFound();

        (bool ok, int row) = board.DropDisc(col);
        _store.Set(id, board);

        if (!ok)
            return BadRequest(new { message = "Ogiltigt drag eller full kolumn." });

        return Json(new
        {
            ok,
            row,
            currentTurn = board.CurrentTurn.ToString(),
            finished = board.IsFinished,
            winner = board.Winner.ToString()
        });
    }
}

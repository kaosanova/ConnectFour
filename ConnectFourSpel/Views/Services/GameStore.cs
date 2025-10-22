using System.Collections.Concurrent;
using ConnectFourSpel.Models;

namespace ConnectFourSpel.Services;

public interface IGameStore
{
    string CreateNew();
    GameBoard? Get(string id);
    void Set(string id, GameBoard board);
}

public class InMemoryGameStore : IGameStore
{
    private readonly ConcurrentDictionary<string, GameBoard> _games = new();

    public string CreateNew()
    {
        var id = Guid.NewGuid().ToString("N");
        _games[id] = new GameBoard();
        return id;
    }

    public GameBoard? Get(string id) =>
        _games.TryGetValue(id, out var board) ? board : null;

    public void Set(string id, GameBoard board) =>
        _games[id] = board;
}

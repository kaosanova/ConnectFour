namespace ConnectFourSpel.Models;

public class GameBoard
{
    private readonly BoardSize _size;
    public CellState[,] Grid { get; }
    public CellState CurrentTurn { get; private set; } = CellState.Player1;
    public bool IsFinished { get; private set; }
    public CellState Winner { get; private set; } = CellState.Empty;
    public int MovesCount { get; private set; }

    public GameBoard(BoardSize? size = null)
    {
        _size = size ?? new BoardSize();
        Grid = new CellState[_size.Rows, _size.Cols];
        // Allt startar med Empty
    }

    public (bool ok, int row) DropDisc(int col)
    {
        if (IsFinished) return (false, -1);
        if (col < 0 || col >= _size.Cols) return (false, -1);

        // Släpp bricka i första lediga plats från botten
        for (int row = _size.Rows - 1; row >= 0; row--)
        {
            if (Grid[row, col] == CellState.Empty)
            {
                Grid[row, col] = CurrentTurn;
                MovesCount++;

                if (HasConnectFour(row, col))
                {
                    Winner = CurrentTurn;
                    IsFinished = true;
                }
                else if (MovesCount == _size.Rows * _size.Cols)
                {
                    // Oavgjort
                    IsFinished = true;
                }
                else
                {
                    // Växla tur
                    CurrentTurn = (CurrentTurn == CellState.Player1) ? CellState.Player2 : CellState.Player1;
                }

                return (true, row);
            }
        }

        // Kolumnen är full
        return (false, -1);
    }

    private bool HasConnectFour(int row, int col)
    {
        var who = Grid[row, col];
        if (who == CellState.Empty) return false;

        // H, V, diag /
        return Count(row, col, 0, 1, who) + Count(row, col, 0, -1, who) - 1 >= 4 ||
               Count(row, col, 1, 0, who) + Count(row, col, -1, 0, who) - 1 >= 4 ||
               Count(row, col, 1, 1, who) + Count(row, col, -1, -1, who) - 1 >= 4 ||
               Count(row, col, 1, -1, who) + Count(row, col, -1, 1, who) - 1 >= 4;
    }

    private int Count(int r, int c, int dr, int dc, CellState who)
    {
        int n = 0;
        while (r >= 0 && r < _size.Rows && c >= 0 && c < _size.Cols && Grid[r, c] == who)
        {
            n++;
            r += dr; c += dc;
        }
        return n;
    }
}

namespace ConnectFourSpel.Models;

public enum CellState { Empty = 0, Player1 = 1, Player2 = 2 }

public record BoardSize(int Rows = 6, int Cols = 7);

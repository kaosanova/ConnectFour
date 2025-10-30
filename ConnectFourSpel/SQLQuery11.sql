CREATE TABLE [dbo].[tbl_Move] 
(
    [Id] INT IDENTITY(1,1) NOT NULL,
    [GameId] INT NOT NULL,
    [MoveNo] INT NOT NULL,
    [PlayerNo] TINYINT NOT NULL,
    [Col] INT NOT NULL,
    [Row] INT NOT NULL,
    [MadeAt] DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_tbl_Move] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_Move_tbl_Game]
        FOREIGN KEY ([GameId]) REFERENCES [dbo].[tbl_Game] ([Id]) ON DELETE CASCADE
);
GO

CREATE NONCLUSTERED INDEX [IX_Moves_Game_MoveNo]
    ON [dbo].[tbl_Move]([GameId] ASC, [MoveNo] ASC);
GO

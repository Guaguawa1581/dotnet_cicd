USE GuaDB_local;
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Logs')
BEGIN
    CREATE TABLE Logs (
        LogId    NVARCHAR(20)  NOT NULL PRIMARY KEY,
        Message  NVARCHAR(MAX) NOT NULL,
        CreateAt DATETIME2     NOT NULL DEFAULT GETDATE()
    );
END
GO

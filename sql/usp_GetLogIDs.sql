USE GuaDB_local;
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetLogIDs
    @Count INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Result TABLE (LogID NVARCHAR(30));
    DECLARE @i INT = 0;
    WHILE @i < @Count
    BEGIN
        INSERT INTO @Result VALUES (
            FORMAT(GETDATE(), 'yyyyMMddHHmmssfff')
            + RIGHT('0000000' + CAST(NEXT VALUE FOR dbo.seq_getLogID AS NVARCHAR(7)), 7)
        );
        SET @i += 1;
    END
    SELECT LogID FROM @Result;
END
GO

CREATE PROCEDURE NTT_TURATERVEZES
    @rendszam NVARCHAR(15),
    @szallKor NVARCHAR(15),
    @szallDatum NVARCHAR(8) -- Assuming YYYYMMDD format
AS
BEGIN
    SET NOCOUNT ON;

    -- Convert szallDatum to DATE for proper comparison
    DECLARE @dateParam DATE;
    BEGIN TRY
        SET @dateParam = CAST(@szallDatum AS DATE);
    END TRY
    BEGIN CATCH
        -- Handle invalid date format
        THROW 50001, 'Invalid date format for szallDatum. Expected YYYYMMDD.', 1;
        RETURN;
    END CATCH;

    -- Sample query: Select trip planning data
    SELECT 
        @@ROWCOUNT,
        DocDueDate,
		--Rendszam.Name,
		--Sofor.Name,
		--Szallkor
		CardName,
		Address2,
		Weight,
		--Turasuly
		Comments
		--megjegyzes cim
		--
		TrnspCode

        
       
    FROM 
        dbo.ORDR
  
        
END;
GO
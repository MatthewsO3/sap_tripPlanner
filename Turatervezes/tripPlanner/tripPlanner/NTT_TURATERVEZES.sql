USE [SBODemoHU]
GO
/****** Object:  StoredProcedure [dbo].[NTT_TURATERVEZES]    Script Date: 2025. 05. 18. 11:05:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[NTT_TURATERVEZES]
    @rendszam NVARCHAR(15),
    @szallKor NVARCHAR(15),
    @szallDatum NVARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @dateParam DATE = NULL;
    
   
    IF @szallDatum IS NOT NULL AND @szallDatum <> ''
    BEGIN TRY
        SET @dateParam = CONVERT(DATE, @szallDatum, 102);
    END TRY
    BEGIN CATCH
        THROW 50001, 'Invalid date', 1;
        RETURN;
    END CATCH;

     SELECT  
    ROW_NUMBER() OVER(ORDER BY o.DocDueDate DESC) AS order_id,
	o.DocEntry as docEntry,
    o.DocDueDate AS del_date,
    r.Name as car_id, -- rendszam
    s.Name AS driver, -- sofor neve
    k.Name as del_circle, -- szallkor
    o.CardName as order_name,
    o.Address2 AS del_addr,
    o.Weight as order_kg,
    SUM(o.Weight) OVER (
        PARTITION BY o.DocDueDate, r.Code, k.Name
    ) as tour_kg, -- total tour weight
    o.Comments as desc_ord,
    o.U_desc_addr as desc_addr, -- cim megjegyzes
    o.U_days as days, --napok
    r.U_kg_cap as car_kg -- auto kapacitas
FROM 
    dbo.ORDR o
LEFT JOIN dbo.[@NTT_RENDSZAM] r ON o.U_NTT_RENDSZAM = r.Code
LEFT JOIN dbo.[@NTT_SOFOR] s ON o.U_NTT_SOFOR = s.Code
LEFT JOIN dbo.[@NTT_SZALLKOR] k ON o.U_NTT_SZALLKOR = k.Code 
WHERE
    (@rendszam IS NULL OR r.Name = @rendszam)
    AND (@szallKor IS NULL OR k.Name = @szallKor)
    AND (@dateParam IS NULL OR o.DocDueDate = @dateParam)
GROUP BY
    o.DocDueDate, o.CardName, o.GrossBase, o.CardCode, o.DocEntry, 
    o.Address2, o.Weight, o.Comments, o.U_desc_addr,  
    o.U_days, r.U_kg_cap, r.Name, r.Code, s.Name, k.Name
ORDER BY
    o.DocDueDate DESC;
END;

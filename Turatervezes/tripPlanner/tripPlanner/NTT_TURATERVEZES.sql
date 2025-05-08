Alter PROCEDURE NTT_TURATERVEZES
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
        o.DocDueDate AS del_date,
        r.Name AS car_id,
        s.Name AS driver, 
        k.Name AS del_circle,
        o.CardName AS order_name,
        o.Address2 AS del_addr,
        SUM(o.Weight) AS order_kg,
        o.U_tour_kg AS tour_kg, -- turasuly
        o.Comments AS desc_ord,
        o.U_desc_addr AS desc_addr, -- cim megjegyzes
        o.U_days AS days, -- napok
        o.U_car_cap AS car_kg -- auto kapacitas
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
        o.Address2, o.Weight, o.Transfered, o.Comments, o.DocStatus, 
        o.CANCELED, o.TrnspCode, r.Name, s.Name, k.Name
    ORDER BY
        o.DocDueDate DESC;
END;
GO
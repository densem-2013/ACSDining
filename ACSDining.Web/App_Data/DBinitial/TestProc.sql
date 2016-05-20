IF OBJECT_ID ('new_paiment_on_new_weekorder') IS NOT NULL
   DROP TRIGGER new_paiment_on_new_weekorder;
GO
CREATE TRIGGER "dbo"."new_paiment_on_new_weekorder" ON WeekOrderMenu
AFTER INSERT
AS
DECLARE @TestRowCount INT,  @WEEKORDERID INT
INSERT INTO WeekPaiment([Note],[Paiment],[WeekOrderMenu_Id])
SELECT ' ',0.00,0,0.00,INSERTED.ID 
FROM INSERTED
GO
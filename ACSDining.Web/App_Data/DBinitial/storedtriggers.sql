﻿USE "ACS_Dining"
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ====
-- Создаёт пустые заказы при окончании создания нового меню а также пустые оплаты на эти заказы
CREATE TRIGGER "dbo"."set_weekmenu_as_orderable" ON dbo.MenuForWeek
AFTER UPDATE
AS
IF NOT UPDATE(OrderCanBeCreated) RETURN
DECLARE @TestRowCount INT, @OCBCRES BIT, @MENUID INT
SELECT @TestRowCount=COUNT(*) FROM INSERTED
IF @TestRowCount=1
BEGIN
SELECT @OCBCRES=INSERTED.OrderCanBeCreated, @MENUID=INSERTED.ID FROM INSERTED
IF NOT @OCBCRES = 1 RETURN
--добавляем фактические недельные заявки на это меню
	INSERT INTO WeekOrderMenu
	SELECT 0.00,@MENUID,USERS.ID 
	FROM AspNetUsers AS USERS 
	INNER JOIN .AspNetUserRoles AS USROLES 
	ON USROLES.UserId = USERS.Id and USERS.IsExisting=1
	INNER JOIN AspNetRoles AS ROLES 
	ON ROLES.Id = USROLES.RoleId AND ROLES.Name = 'Employee'
	--INNER JOIN MenuForWeek
	--ON  MenuForWeek.ID=@MENUID 
	WHERE USERS.IsExisting=1
	ORDER BY USERS.UserName
		
	--добавляем фактические дневные заявки на дневные меню из этого недельного меню
	INSERT INTO DayOrderMenu
	SELECT 0.00,MenuForDay.ID,WeekOrderMenu.Id FROM MenuForWeek 
	INNER JOIN WeekOrderMenu
	ON WeekOrderMenu.MenuForWeek_ID=MenuForWeek.ID 
	INNER JOIN AspNetUsers AS USERS 
	ON WeekOrderMenu.[User_Id]=USERS.Id AND USERS.IsExisting=1
	INNER JOIN .AspNetUserRoles AS USROLES 
	ON USROLES.UserId = USERS.Id 
	INNER JOIN AspNetRoles AS ROLES 
	ON ROLES.Id = USROLES.RoleId AND ROLES.Name = 'Employee'	
	INNER JOIN MenuForDay
	ON MenuForDay.MenuForWeek_ID=MenuForWeek.ID
	WHERE MenuForWeek.ID=@MENUID
	
	--ДОБАВЛЯЕМ НУЛЕВЫЕ КОЛИЧЕСТВА ЗАКАЗАННЫХ БЛЮД ВО ВСЕ ФАКТИЧЕСКИЕ ДНЕВНЫЕ ЗАКАЗЫ
	--ИЗ ЭТОГО ФАКТИЧЕСКОГО НЕДЕЛЬНОГО ЗАКАЗА
	INSERT INTO DishQuantityRelations 
	SELECT 1 ,DishType.Id,DayOrderMenu.Id 
	FROM MenuForWeek 
	INNER JOIN WeekOrderMenu
	ON WeekOrderMenu.MenuForWeek_ID=MenuForWeek.ID AND MenuForWeek.ID=@MENUID 
	INNER JOIN AspNetUsers AS USERS 
	ON WeekOrderMenu.[User_Id]=USERS.Id AND USERS.IsExisting=1
	INNER JOIN .AspNetUserRoles AS USROLES 
	ON USROLES.UserId = USERS.Id 
	INNER JOIN AspNetRoles AS ROLES 
	ON ROLES.Id = USROLES.RoleId AND ROLES.Name = 'Employee'	
	INNER JOIN MenuForDay
	ON MenuForDay.MenuForWeek_ID=MenuForWeek.ID 
	INNER JOIN MFD_Dishes
	ON MenuForDay.ID=MFD_Dishes.MenuID
	INNER JOIN DayOrderMenu
	ON DayOrderMenu.MenuForDay_ID=MenuForDay.ID AND DayOrderMenu.WeekOrderMenu_Id=WeekOrderMenu.Id
	INNER JOIN Dish
	ON MFD_Dishes.DishID=Dish.DishID
	INNER JOIN DishType
	ON DishType.Id=Dish.DishType_Id 
	INNER JOIN WorkingWeek
	ON WorkingWeek.ID=MenuForWeek.WorkingWeek_ID 
	INNER JOIN WorkingDay
	ON WorkingDay.Id=MenuForDay.WorkingDay_Id AND WorkingDay.IsWorking=1 AND WorkingDay.WorkingWeek_ID=WorkingWeek.ID
	ORDER BY DayOrderMenu.Id, DishType.Id
	
	--добавляем недельные оплаты на заказы из этого меню
	INSERT INTO WeekPaiment
	SELECT ' ',0.00,0,AspNetUsers.Balance,WeekOrderMenu.ID 
	FROM WeekOrderMenu 
	INNER JOIN AspNetUsers
	ON WeekOrderMenu.[User_Id]=AspNetUsers.Id
	WHERE WeekOrderMenu.MenuForWeek_ID = @MENUID
END
GO

--Создаёт новый заказ  на меню текущей недели при регистрации нового пользователя в системе 
CREATE TRIGGER "dbo"."new_week_order_on_new_user" ON dbo.AspNetUsers
AFTER INSERT
AS
DECLARE  @MENUID INT, @USERID NVARCHAR(128), @CURWORKDAYID INT
--Получаем ID меню на текущую неделю
 SET @CURWORKDAYID=dbo.GetCurrentWorkDayId()
 IF (@CURWORKDAYID IS NULL) RETURN
 SET @MENUID=(SELECT MenuForWeek.ID FROM MenuForWeek
	INNER JOIN MenuForDay
	ON MenuForDay.MenuForWeek_ID=MenuForWeek.ID AND MenuForDay.WorkingDay_Id=@CURWORKDAYID);
 IF (@MENUID IS NULL) RETURN
	
DECLARE INS_USER_CURSOR CURSOR
SCROLL
FOR SELECT [ID] FROM INSERTED 
OPEN INS_USER_CURSOR
FETCH NEXT FROM INS_USER_CURSOR INTO @USERID
WHILE @@FETCH_STATUS=0
BEGIN
	exec CreateNewUserWeekOrder @MENUID, @USERID
	FETCH NEXT FROM INS_USER_CURSOR INTO @USERID
END
GO
--удаляет все связи для данного клиента перед его удалением из системы
CREATE TRIGGER "dbo"."todo_on_userdelete" ON dbo.AspNetUsers
INSTEAD OF DELETE
AS
DECLARE @USERID NVARCHAR(128)
DECLARE USER_DEL_CURSOR CURSOR
DYNAMIC
FOR SELECT [ID] FROM DELETED
OPEN USER_DEL_CURSOR
FETCH NEXT FROM USER_DEL_CURSOR INTO @USERID
WHILE @@FETCH_STATUS=0
BEGIN
	exec TODOONDELETEUSER @USERID
	DELETE FROM dbo.AspNetUsers WHERE ID=@USERID
	FETCH NEXT FROM USER_DEL_CURSOR INTO @USERID
END
GO
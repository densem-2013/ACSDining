USE "ACS_Dining"
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Обновляет значение заказа на блюдо а также сумму заказа на день и рабочую неделю>
-- =============================================
CREATE PROC [dbo].[UpdateDishQuantity]
		-- Add the parameters for the stored procedure here
	@dayorderid int,
	@dishtypeid int,
	@quantity float
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	UPDATE DishQuantityRelations 
	SET [DishQuantityId]=(SELECT DQUA.[Id] FROM  DishQuantity AS DQUA WHERE DQUA.[Quantity]=@quantity)
	WHERE [DayOrderMenuId]=@dayorderid AND [DishTypeId]=@dishtypeid
	
	UPDATE planrels
	SET [DishQuantityId]=(SELECT DQUA.[Id] FROM  DishQuantity AS DQUA WHERE DQUA.[Quantity]=@quantity)
	FROM PlanDishQuantityRelations planrels
	INNER JOIN DayOrderMenu
	ON DayOrderMenu.Id=@dayorderid
	INNER JOIN WeekOrderMenu
	ON WeekOrderMenu.Id=DayOrderMenu.WeekOrderMenu_Id
	INNER JOIN MenuForDay
	ON MenuForDay.ID=DayOrderMenu.MenuForDay_ID
	INNER JOIN PlannedDayOrderMenu
	ON PlannedDayOrderMenu.MenuForDay_ID=MenuForDay.ID
	INNER JOIN PlannedWeekOrderMenu
	ON PlannedWeekOrderMenu.Id=PlannedDayOrderMenu.PlannedWeekOrderMenu_Id and PlannedWeekOrderMenu.[User_Id]=WeekOrderMenu.[User_Id]
	WHERE planrels.[PlannedDayOrderMenuId]=PlannedDayOrderMenu.Id AND [DishTypeId]=@dishtypeid
	
	DECLARE @weekorderid INT
	SELECT @weekorderid=[WeekOrderMenu_Id] FROM [ACS_Dining].[dbo].[DayOrderMenu] WHERE ID=@dayorderid
	--Обновляем баланс пользователя по его id
	
	EXEC UpdateBalanceByWeekOrderId @weekorderid
	
END
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Обновляет пользовательский заказ со стороны SU - только факт>
-- =============================================
CREATE PROC [dbo].[UpdateDishQuantityBySU]
		-- Add the parameters for the stored procedure here
	@dayorderid int,
	@dishtypeid int,
	@quantity float
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	UPDATE DishQuantityRelations 
	SET [DishQuantityId]=(SELECT DQUA.[Id] FROM  DishQuantity AS DQUA WHERE DQUA.[Quantity]=@quantity)
	WHERE [DayOrderMenuId]=@dayorderid AND [DishTypeId]=@dishtypeid
		
	DECLARE @weekorderid INT
	SELECT @weekorderid=[WeekOrderMenu_Id] FROM [ACS_Dining].[dbo].[DayOrderMenu] WHERE ID=@dayorderid
	--Обновляем баланс пользователя по его id
	
	EXEC UpdateBalanceByWeekOrderId @weekorderid
	
END
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Переводит плановые заявки из фактических на текущий день,
--				 закрывает возможность редактирования меню на этот день и
--				 закрывает возможность редактировать заказ на этот день>
-- =============================================
CREATE PROCEDURE "DailyOffEditMode"
	-- Add the parameters for the stored procedure here
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	DECLARE @WDAYID INT
	SET @WDAYID=dbo.GetCurrentWorkDayId();
	--закрываем редактирование заказа для клиента
	--закрываем редактирование меню для пользователя
	UPDATE MenuForDay
	SET [OrderCanBeChanged]=0,[DayMenuCanBeChanged]=0
	WHERE [WorkingDay_Id]=@WDAYID
	--------------------------------
	DECLARE DAYORD_CURSOR CURSOR
	DYNAMIC
	FOR SELECT DAYORDS.[ID] FROM [ACS_Dining].[dbo].[DayOrderMenu] DAYORDS
	INNER JOIN [ACS_Dining].[dbo].[MenuForDay] MFDAYS
	ON MFDAYS.ID=DAYORDS.MenuForDay_ID AND MFDAYS.WorkingDay_Id=@WDAYID
	DECLARE @DAYORDID INT
	OPEN DAYORD_CURSOR
	FETCH NEXT FROM DAYORD_CURSOR INTO @DAYORDID
	WHILE @@FETCH_STATUS=0
	BEGIN
		--создаём плановую дневную заявку
		FETCH NEXT FROM DAYORD_CURSOR INTO @DAYORDID
	END
END
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Создаёт пустой заказ для пользователя с ID=@userid на недельное меню с ID=@weekmenuid>
-- =============================================
CREATE PROC "CreateNewUserWeekOrder"
	@MENUID int,
	@userid nvarchar(128)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	--добавляем фактическую недельную заявку на это меню
	INSERT INTO WeekOrderMenu
	VALUES( 0.00, @MENUID, @userid)
	--добавляем фактические дневные заявки на дневные меню из этого недельного меню
	INSERT INTO DayOrderMenu
	SELECT 0.00,MenuForDay.ID,WeekOrderMenu.Id FROM MenuForWeek 
	INNER JOIN WeekOrderMenu
	ON WeekOrderMenu.MenuForWeek_ID=MenuForWeek.ID AND WeekOrderMenu.[User_Id]=@userid	
	INNER JOIN MenuForDay
	ON MenuForDay.MenuForWeek_ID=MenuForWeek.ID
	WHERE MenuForWeek.ID=@MENUID
	
	--ДОБАВЛЯЕМ НУЛЕВЫЕ КОЛИЧЕСТВА ЗАКАЗАННЫХ БЛЮД ВО ВСЕ ФАКТИЧЕСКИЕ ДНЕВНЫЕ ЗАКАЗЫ
	--ИЗ ЭТОГО ФАКТИЧЕСКОГО НЕДЕЛЬНОГО ЗАКАЗА
	INSERT INTO DishQuantityRelations 
	SELECT dqua.Id ,DishType.Id,DayOrderMenu.Id 
	FROM MenuForWeek 
	INNER JOIN WeekOrderMenu
	ON WeekOrderMenu.MenuForWeek_ID=MenuForWeek.ID AND MenuForWeek.ID=@MENUID AND WeekOrderMenu.[User_Id]=@userid
	INNER JOIN MenuForDay
	ON MenuForDay.MenuForWeek_ID=MenuForWeek.ID 
	INNER JOIN [ACS_Dining].[dbo].[MfdDishPriceRelations] AS MPRELS
	ON MPRELS.[MenuForDayId]=MenuForDay.ID
	INNER JOIN DayOrderMenu
	ON DayOrderMenu.MenuForDay_ID=MenuForDay.ID AND DayOrderMenu.WeekOrderMenu_Id=WeekOrderMenu.Id
	INNER JOIN Dish
	ON MPRELS.DishID=Dish.DishID
	INNER JOIN DishType
	ON DishType.Id=Dish.DishType_Id 
	INNER JOIN WorkingWeek
	ON WorkingWeek.ID=MenuForWeek.WorkingWeek_ID 
	INNER JOIN WorkingDay
	ON WorkingDay.Id=MenuForDay.WorkingDay_Id  /*AND WorkingDay.IsWorking=1*/ AND WorkingDay.WorkingWeek_ID=WorkingWeek.ID
	inner join DishQuantity dqua
	on dqua.Quantity=0.00
	ORDER BY DayOrderMenu.Id, DishType.Id
	
	--добавляем недельную оплату на заказ из этого меню
	INSERT INTO WeekPaiment
	SELECT ' ',0.00,0,AspNetUsers.Balance,WeekOrderMenu.ID 
	FROM WeekOrderMenu
	INNER JOIN AspNetUsers
	ON WeekOrderMenu.[User_Id]=AspNetUsers.Id
	WHERE WeekOrderMenu.MenuForWeek_ID = @MENUID AND WeekOrderMenu.[User_Id]=@userid
	
	--добавляем плановую заявку на это меню для данного сотрудника
	INSERT INTO PlannedWeekOrderMenu
	SELECT WeekOrderMenu.WeekOrderSummaryPrice, @MENUID, @userid
	FROM WeekOrderMenu
	WHERE WeekOrderMenu.MenuForWeek_ID=@MENUID AND WeekOrderMenu.[User_Id]=@userid
	
	--добавляем плановые дневные заявки на дневные меню из этого недельного меню
	INSERT INTO PlannedDayOrderMenu 
	SELECT DayOrderMenu.[DayOrderSummaryPrice], MenuForDay.ID, PlannedWeekOrderMenu.Id
	FROM DayOrderMenu 
	inner join MenuForDay
	on MenuForDay.ID=DayOrderMenu.MenuForDay_ID
	INNER JOIN AspNetUsers AS USERS 
	ON USERS.Id =@userid
	INNER JOIN PlannedWeekOrderMenu
	ON PlannedWeekOrderMenu.MenuForWeek_ID=@MENUID AND PlannedWeekOrderMenu.[User_Id]=USERS.Id
	INNER JOIN WeekOrderMenu
	ON DayOrderMenu.WeekOrderMenu_Id=WeekOrderMenu.Id 
	AND  WeekOrderMenu.MenuForWeek_ID=PlannedWeekOrderMenu.MenuForWeek_ID
	AND WeekOrderMenu.[User_Id]= USERS.Id
	
	--ДОБАВЛЯЕМ ПЛАНОВЫЕ НЕДЕЛЬНЫЕ ЗАЯВКИ НА ЭТО НЕДЕЛЬНОЕ МЕНЮ ДЛЯ ДАННОГО ПОЛЬЗОВАТЕЛЯ
	--С НУЛЕВЫМИ КОЛИЧЕСТВАМИ ЗАКАЗАННЫХ БЛЮД
	INSERT INTO PlanDishQuantityRelations
	SELECT DishQuantity.Id ,DishQuantityRelations.DishTypeId,PlannedDayOrderMenu.Id
	FROM DishQuantityRelations
	INNER JOIN DayOrderMenu
	ON DayOrderMenu.Id=DishQuantityRelations.DayOrderMenuId
	INNER JOIN WeekOrderMenu
	ON DayOrderMenu.WeekOrderMenu_Id=WeekOrderMenu.Id
	INNER JOIN MenuForWeek
	ON MenuForWeek.ID=WeekOrderMenu.MenuForWeek_ID AND MenuForWeek.ID=@MENUID
	INNER JOIN PlannedWeekOrderMenu
	ON PlannedWeekOrderMenu.MenuForWeek_ID=MenuForWeek.ID AND PlannedWeekOrderMenu.MenuForWeek_ID=WeekOrderMenu.MenuForWeek_ID
	INNER JOIN DishType
	ON DishType.Id=DishQuantityRelations.DishTypeId
	INNER JOIN MenuForDay
	ON MenuForDay.ID=DayOrderMenu.MenuForDay_ID AND MenuForDay.MenuForWeek_ID=MenuForWeek.ID
	INNER JOIN PlannedDayOrderMenu
	ON PlannedDayOrderMenu.MenuForDay_ID=DayOrderMenu.MenuForDay_ID AND PlannedDayOrderMenu.MenuForDay_ID=MenuForDay.ID AND PlannedDayOrderMenu.PlannedWeekOrderMenu_Id=PlannedWeekOrderMenu.Id
	INNER JOIN DishQuantity
	ON DishQuantity.Quantity=0.00
	INNER JOIN	AspNetUsers AS USERS 
	ON USERS.Id=WeekOrderMenu.[User_Id] AND USERS.Id=PlannedWeekOrderMenu.[User_Id]
	AND USERS.Id=@userid
END
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Создаёт новое меню на неделю и заполняет его пустыми блюдами>
-- =============================================
CREATE PROC "CreateNewWeekMenu"
		@week int,
	@year int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	if exists(select mfw.id from [ACS_Dining].[dbo].[MenuForWeek] mfw
	inner join [ACS_Dining].[dbo].[WorkingWeek] wweek
	on wweek.ID=mfw.WorkingWeek_ID and wweek.ID=@week
	inner join [ACS_Dining].[dbo].[Year] years
	on years.Id=wweek.Year_Id and years.YearNumber=@year) return
	--Получаем   Id указанного года из параметров, если его нет, вносим новую запись в года
	Declare @yearid int
	if not exists( select id from [ACS_Dining].[dbo].[Year] where [YearNumber]=@year)
	insert [ACS_Dining].[dbo].[Year]	values(@year)		
	select @yearid=id from [ACS_Dining].[dbo].[Year] where [YearNumber]=@year
	--Получаем Id указанной рабочей недели в году,
	--усли недели нет, создаём новую рабочую неделю и заполняем её рабочими днями
	-- по умолчанию первые пять дней недели - рабочие
	Declare @workweekid int
	if not exists( select id from [ACS_Dining].[dbo].[WorkingWeek] where [Year_Id]=@yearid and [WeekNumber]=@week)
	begin
			insert [ACS_Dining].[dbo].[WorkingWeek]
			values(@week,1,@yearid)
			select @workweekid=id from [ACS_Dining].[dbo].[WorkingWeek] 
			where [Year_Id]=@yearid and [WeekNumber]=@week
			insert [ACS_Dining].[dbo].[WorkingDay]
			select cast(case when Id<=5 then 1 else 0 end as bit),Id,@workweekid
			from [ACS_Dining].[dbo].[DayOfWeek]
			order by Id
	 end
	 select @workweekid=id from [ACS_Dining].[dbo].[WorkingWeek] 
	 where [Year_Id]=@yearid and [WeekNumber]=@week
	 --Создаём меню на неделю и заполняем его дневными меню
	 if exists(select * from [ACS_Dining].[dbo].[MenuForWeek] where [WorkingWeek_ID]=@workweekid) return
	 declare @weekmenuid int
	 insert [ACS_Dining].[dbo].[MenuForWeek]
	 values(0.00,1,0,0,@workweekid)
	 select @weekmenuid=id from [ACS_Dining].[dbo].[MenuForWeek] where [WorkingWeek_ID]=@workweekid
	 --print @weekmenuid
	 insert [ACS_Dining].[dbo].[MenuForDay]
	 select 0.00,1,1,@weekmenuid,wday.Id
	 from  [ACS_Dining].[dbo].[WorkingDay] wday
	 where WorkingWeek_ID=@workweekid
	 order by Id
	 --Заполняем дневные меню пустыми блюдами
	 insert [ACS_Dining].[dbo].[MfdDishPriceRelations]
	 select dishes.CurrentPrice_Id, dishes.DishID, mfd.id
	 from [ACS_Dining].[dbo].[MenuForDay] mfd
	 inner join [ACS_Dining].[dbo].[Dish] dishes
	 on dishes.Title is null
	 inner join [ACS_Dining].[dbo].[DishType] dt
	 on dt.Id=dishes.DishType_Id
	 where MenuForWeek_ID=@weekmenuid
END
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Удаляем меню на неделю>
-- =============================================
CREATE PROC  "DeleteWeekMenu"
	
	
	@week int,
	@year int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	declare @weekmenuid int
	if not exists( select mfw.id from [ACS_Dining].[dbo].[MenuForWeek] mfw
	inner join [ACS_Dining].[dbo].[WorkingWeek] wweek
	on wweek.ID=mfw.WorkingWeek_ID and wweek.WeekNumber=@week
	inner join [ACS_Dining].[dbo].[Year] years
	on years.Id=wweek.Year_Id and years.YearNumber=@year) return
	
	select @weekmenuid=mfw.[ID] from [ACS_Dining].[dbo].[MenuForWeek] mfw
	inner join [ACS_Dining].[dbo].[WorkingWeek] wweek
	on wweek.[ID]=mfw.[WorkingWeek_ID] and wweek.[WeekNumber]=@week
	inner join [ACS_Dining].[dbo].[Year] years
	on years.[Id]=wweek.[Year_Id] and years.[YearNumber]=@year
	--удаляем связи на блюда и их количества из фактических дневных заказов
	delete from [ACS_Dining].[dbo].[DishQuantityRelations] 
	where [DayOrderMenuId] in (select dords.Id from [ACS_Dining].[dbo].[DayOrderMenu] dords
	inner join [ACS_Dining].[dbo].[WeekOrderMenu] words
	on words.Id=dords.WeekOrderMenu_Id and words.MenuForWeek_ID=@weekmenuid)
	--удаляем связи на блюда и их количества из плановых дневных заказов
	delete from [ACS_Dining].[dbo].[PlanDishQuantityRelations] 
	where [PlannedDayOrderMenuId] in (select plandords.Id from [ACS_Dining].[dbo].[PlannedDayOrderMenu] plandords
	inner join [ACS_Dining].[dbo].[PlannedWeekOrderMenu] planwords
	on planwords.Id=plandords.[PlannedWeekOrderMenu_Id] and planwords.MenuForWeek_ID=@weekmenuid)
	--удаляем все фактические дневные заказы на дневные меню из этого недельного меню
	delete from [ACS_Dining].[dbo].[DayOrderMenu] 
	where [WeekOrderMenu_Id] in (select WeekOrderMenu.Id from [ACS_Dining].[dbo].[WeekOrderMenu] where [MenuForWeek_ID]=@weekmenuid)
	--удаляем все плановые дневные заказы на дневные меню из этого недельного меню
	delete from [ACS_Dining].[dbo].[PlannedDayOrderMenu] 
	where [PlannedWeekOrderMenu_Id] in (select Id from [ACS_Dining].[dbo].[PlannedWeekOrderMenu] where [MenuForWeek_ID]=@weekmenuid)
	--удаляем все недельные оплаты на заказы из этого меню
	delete from [ACS_Dining].[dbo].[WeekPaiment]
	where [WeekOrderMenu_Id] in (select WeekOrderMenu.Id from [ACS_Dining].[dbo].[WeekOrderMenu] where [MenuForWeek_ID]=@weekmenuid)
	--удаляем все заказы на это недельное меню
	delete from [ACS_Dining].[dbo].[WeekOrderMenu]
	where [MenuForWeek_ID]=@weekmenuid
	--удаляем все плановые заказы на это недельное меню
	delete from [ACS_Dining].[dbo].[PlannedWeekOrderMenu]
	where [MenuForWeek_ID]=@weekmenuid
	--удаляем связи между блюдами и дневные меню этой недели
	delete mfddprels from [ACS_Dining].[dbo].[MfdDishPriceRelations] mfddprels
	inner join [ACS_Dining].[dbo].[MenuForDay] daymenu
	on daymenu.MenuForWeek_ID=@weekmenuid and mfddprels.MenuForDayId=daymenu.ID
	--удаляем дневные меню
	delete  from [ACS_Dining].[dbo].[MenuForDay] 
	where MenuForWeek_ID=@weekmenuid
	--удаляем рабочие дни
	delete wdays from  [ACS_Dining].[dbo].[WorkingDay] wdays
	inner join [ACS_Dining].[dbo].[WorkingWeek] wweek
	on wdays.WorkingWeek_ID=wweek.ID
	inner join [ACS_Dining].[dbo].[MenuForWeek] mfw
	on mfw.WorkingWeek_ID=wweek.ID and mfw.ID=@weekmenuid	
	--удаляем рабочую неделю
	delete wweek from  [ACS_Dining].[dbo].[WorkingWeek] wweek
	inner join  [ACS_Dining].[dbo].[MenuForWeek] mfw
	on mfw.WorkingWeek_ID=wweek.ID and mfw.ID=@weekmenuid
	
	--удаляем меню на неделю
	delete  from [ACS_Dining].[dbo].[MenuForWeek]
	where ID=@weekmenuid
END
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Вычисляет суммы к оплате для дневных заказов>
-- =============================================
CREATE PROCEDURE "CalcWeekDayOrderSummary"
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	update dordmenu
	set dordmenu.[DayOrderSummaryPrice]=( select sum(dprice.Price*dqua.Quantity) from [ACS_Dining].[dbo].[DishQuantityRelations] rels
	join [ACS_Dining].[dbo].[MfdDishPriceRelations] mfddprice
	on mfddprice.MenuForDayId=dordmenu.MenuForDay_ID
	inner join [ACS_Dining].[dbo].[DishPrice] dprice
	on dprice.Id=mfddprice.DishPriceId
	inner join [ACS_Dining].[dbo].[Dish] dishes
	on dishes.DishID=mfddprice.DishID
	inner join [ACS_Dining].[dbo].[MenuForDay] mfd
	on mfd.ID=dordmenu.MenuForDay_ID and mfddprice.MenuForDayId=mfd.ID
	inner join [ACS_Dining].[dbo].[DishQuantity] dqua
	on dqua.Id=rels.DishQuantityId
	inner join  [ACS_Dining].[dbo].[DishType] dtypes
	on dtypes.Id=rels.DishTypeId and dtypes.Id=dishes.DishType_Id
	where rels.DayOrderMenuId=dordmenu.Id)
	from [ACS_Dining].[dbo].[DayOrderMenu] dordmenu
	
	update weekordmenu
	set weekordmenu.[WeekOrderSummaryPrice]=( select sum(dordmenu.DayOrderSummaryPrice) from [ACS_Dining].[dbo].[DayOrderMenu] dordmenu
	inner join  [ACS_Dining].[dbo].[MenuForDay] mfd
	on mfd.ID=dordmenu.MenuForDay_ID
	inner join [ACS_Dining].[dbo].[WorkingDay] wdays
	on wdays.Id=mfd.WorkingDay_Id and wdays.IsWorking=1
	where dordmenu.WeekOrderMenu_Id=weekordmenu.Id)
	from  [ACS_Dining].[dbo].[WeekOrderMenu] weekordmenu
END
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<удаляет все связи данного клиента в системе>
-- =============================================
CREATE PROC "TODOONDELETEUSER"
	-- Add the parameters for the stored procedure here
	@USERID NVARCHAR(128)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	declare @roleid nvarchar(128)
	select @roleid=id from [ACS_Dining].[dbo].[AspNetRoles] where [Name]='Employee'
	if not exists(select * from [ACS_Dining].[dbo].[AspNetUserRoles] where [RoleId]=@roleid) return
	--удаляем связи на блюда и их количества из фактических дневных заказов
	delete from [ACS_Dining].[dbo].[DishQuantityRelations] 
	where [DayOrderMenuId] in (select dords.Id from [ACS_Dining].[dbo].[DayOrderMenu] dords
	inner join [ACS_Dining].[dbo].[WeekOrderMenu] words
	on words.Id=dords.WeekOrderMenu_Id and words.[User_Id]=@USERID)
	--удаляем связи на блюда и их количества из плановых дневных заказов
	delete from [ACS_Dining].[dbo].[PlanDishQuantityRelations] 
	where [PlannedDayOrderMenuId] in (select plandords.Id from [ACS_Dining].[dbo].[PlannedDayOrderMenu] plandords
	inner join [ACS_Dining].[dbo].[PlannedWeekOrderMenu] planwords
	on planwords.Id=plandords.[PlannedWeekOrderMenu_Id] and planwords.[User_Id]=@USERID)
	--удаляем все фактические дневные заказы на дневные меню из этого недельного меню
	delete from [ACS_Dining].[dbo].[DayOrderMenu] 
	where [WeekOrderMenu_Id] in (select WeekOrderMenu.Id from [ACS_Dining].[dbo].[WeekOrderMenu] where [User_Id]=@USERID)
	--удаляем все плановые дневные заказы на дневные меню из этого недельного меню
	delete from [ACS_Dining].[dbo].[PlannedDayOrderMenu] 
	where [PlannedWeekOrderMenu_Id] in (select Id from [ACS_Dining].[dbo].[PlannedWeekOrderMenu] where [User_Id]=@USERID)
	--удаляем все недельные оплаты на заказы из этого меню
	delete from [ACS_Dining].[dbo].[WeekPaiment]
	where [WeekOrderMenu_Id] in (select WeekOrderMenu.Id from [ACS_Dining].[dbo].[WeekOrderMenu] where [User_Id]=@USERID)
	--удаляем все заказы на это недельное меню
	delete from [ACS_Dining].[dbo].[WeekOrderMenu]
	where [User_Id]=@USERID
	--удаляем все плановые заказы на это недельное меню
	delete from [ACS_Dining].[dbo].[PlannedWeekOrderMenu]
	where [User_Id]=@USERID
END
GO
-- =============================================
-- Create date: <Create Date,,>
-- Description:	<Обновляет все плановые заявки на меню текущего дня по
-- текущему на 9.00 значению фактических заявок
-- и закрывает возможность редактирования заказа на этот день для клиентов,
-- редактирование меню на этот день для пользователя>
-- =============================================
CREATE PROCEDURE [dbo].[DayFactToPlan] 
	-- Add the parameters for the stored procedure here
	@WDAYID int=null
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	DECLARE @DAYMENUID INT,@WEEKMENUID INT, @ISWORKING BIT
	if @WDAYID is null	SET @WDAYID=dbo.GetCurrentWorkDayId();
	
	--ЕСЛИ ТЕКУЩИЙ ДЕНЬ НЕ РАБОЧИЙ или РАБОЧЕГО ДНЯ НЕ СУЩЕСТВУЕТ(т.е. меню не создано)- ВЫХОДИМ
	SELECT @ISWORKING=WorkingDay.IsWorking FROM WorkingDay WHERE WorkingDay.Id=@WDAYID
	IF @ISWORKING is null or @ISWORKING=0 RETURN
	
	SELECT @WEEKMENUID=MenuForWeek_ID, @DAYMENUID=ID  FROM MenuForDay WHERE MenuForDay.WorkingDay_Id=@WDAYID
		
	--обновляем плановые недельные заявки по сумме заказа
		UPDATE PLANWOMENU
		SET 	PLANWOMENU.WeekOrderSummaryPrice=WeekOrderMenu.WeekOrderSummaryPrice
		FROM PlannedWeekOrderMenu AS PLANWOMENU
		INNER JOIN WeekOrderMenu
		ON WeekOrderMenu.MenuForWeek_ID=PLANWOMENU.MenuForWeek_ID
		INNER JOIN  AspNetUsers AS USERS 
		ON USERS.Id=PLANWOMENU.[User_Id] AND PLANWOMENU.[User_Id]=WeekOrderMenu.[User_Id]
		INNER JOIN AspNetUserRoles AS USROLES 
		ON USROLES.UserId = USERS.Id and USERS.IsExisting=1
		INNER JOIN AspNetRoles AS ROLES 
		ON ROLES.Id = USROLES.RoleId AND ROLES.Name = 'Employee'
		WHERE PLANWOMENU.MenuForWeek_ID=@WEEKMENUID
		
	--обновляем плановые дневные заявки на дневные меню из этого недельного меню
	--по сумме заказа
	  UPDATE PLANDOMENU
	  SET PLANDOMENU.[DayOrderSummaryPrice]=DayOrderMenu.[DayOrderSummaryPrice]
	  FROM PlannedDayOrderMenu PLANDOMENU
	  INNER JOIN DayOrderMenu
	  ON  DayOrderMenu.MenuForDay_ID=PLANDOMENU.MenuForDay_ID 
	 INNER JOIN MenuForDay
	 ON  MenuForDay.ID=DayOrderMenu.MenuForDay_ID 
	 --AND MenuForDay.OrderCanBeChanged=1  AND MenuForDay.DayMenuCanBeChanged=1
	 AND MenuForDay.WorkingDay_Id<=@WDAYID
	  INNER JOIN WeekOrderMenu
	  ON DayOrderMenu.WeekOrderMenu_Id=WeekOrderMenu.Id 
	  AND PLANDOMENU.PlannedWeekOrderMenu_Id=WeekOrderMenu.Id
	 AND  WeekOrderMenu.MenuForWeek_ID=@WEEKMENUID
	  INNER JOIN AspNetUsers AS USERS 
	  ON WeekOrderMenu.[User_Id]=USERS.Id 
	  INNER JOIN AspNetUserRoles AS USROLES 
	  ON USROLES.UserId = USERS.Id and USERS.IsExisting=1
	  INNER JOIN AspNetRoles AS ROLES 
	  ON ROLES.Id = USROLES.RoleId AND ROLES.Name = 'Employee'
	--ОБНОВЛЯЕМ ПЛАНОВЫЕ ДНЕВНЫЕ ЗАКАЗЫ
	 UPDATE planrels
	SET planrels.DishQuantityId= DishQuantityRelations.DishQuantityId 
	FROM PlanDishQuantityRelations planrels
	INNER JOIN PlannedDayOrderMenu 
	ON PlannedDayOrderMenu.Id=planrels.PlannedDayOrderMenuId
	INNER JOIN MenuForDay
	ON MenuForDay.ID=PlannedDayOrderMenu.MenuForDay_ID 
	--AND MenuForDay.OrderCanBeChanged=1  AND MenuForDay.DayMenuCanBeChanged=1
	AND MenuForDay.WorkingDay_Id<=@WDAYID
	INNER JOIN PlannedWeekOrderMenu
	ON PlannedDayOrderMenu.PlannedWeekOrderMenu_Id=PlannedWeekOrderMenu.Id
	AND  PlannedWeekOrderMenu.MenuForWeek_ID=@WEEKMENUID
	INNER JOIN DayOrderMenu
	ON DayOrderMenu.MenuForDay_ID=MenuForDay.ID
	INNER JOIN WeekOrderMenu
	ON DayOrderMenu.WeekOrderMenu_Id=WeekOrderMenu.Id
	INNER JOIN DishQuantityRelations
	ON DishQuantityRelations.DayOrderMenuId= DayOrderMenu.Id AND DishQuantityRelations.DishTypeId=planrels.DishTypeId
	INNER JOIN	AspNetUsers AS USERS 
	ON USERS.Id=PlannedWeekOrderMenu.[User_Id] and WeekOrderMenu.[User_Id]=PlannedWeekOrderMenu.[User_Id]
	INNER JOIN AspNetUserRoles AS USROLES 
	ON USROLES.UserId = USERS.Id and USERS.IsExisting=1
	INNER JOIN AspNetRoles AS ROLES 
	ON ROLES.Id = USROLES.RoleId AND ROLES.Name = 'Employee'
	
	--ЗАКРЫВАЕМ ВОЗМОЖНОСТЬ РЕДАКТИРОВАНИЯ МЕНЮ ДЛЯ ПОЛЬЗОВАТЕЛЯ 
	--И ЗАКАЗОВ ДЛЯ КЛИЕНТОВ
	UPDATE MenuForDay
	SET [OrderCanBeChanged]=0,  [DayMenuCanBeChanged]=0
	WHERE MenuForDay.MenuForWeek_ID=@WEEKMENUID AND MenuForDay.WorkingDay_Id<=@WDAYID
	
	--Проверяем, что этот день является первым рабочим днём недели
	declare @minwdid int;
	
	select @minwdid=MIN(MenuForDay.WorkingDay_Id) from MenuForDay
	where MenuForDay.MenuForWeek_ID=@WEEKMENUID
	
	if @minwdid!=@WDAYID return
	
	--Находим id меню прошлой недели 
	declare @prevweekmenuid int
	select @prevweekmenuid=MenuForWeek.ID from MenuForWeek
	inner join WorkingWeek
	on WorkingWeek.ID=MenuForWeek.WorkingWeek_ID
	inner join [ACS_Dining].[dbo].[Year] Years
	on Years.Id=WorkingWeek.Year_Id
	inner join  [dbo].[GetPrevWeekYear](null,null) prev
	on prev.[WEEK]=WorkingWeek.WeekNumber and prev.[YEAR]=Years.YearNumber
	
	--Закрываем возможность редактирования заказов и оплат за прошлую неделю для суперюзера
	if @prevweekmenuid is null return
	update MenuForWeek
	set SUCanChangeOrder=0
	where ID=@prevweekmenuid
	
END
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Обновляет значения сумм к оплате для дневных заказов,
-- сумму заказа на неделю и баланс пользователя, которому принадлежит заказ>
-- =============================================
CREATE PROC [dbo].[UpdateBalanceByWeekOrderId]
	-- Add the parameters for the function here
	@weekorderid int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	DECLARE @curbalance float, @nextbalance float
	--Обновляем суммы дневных оплат для данного недельного заказа
	update dordmenu
	set dordmenu.[DayOrderSummaryPrice]=( select sum(dprice.Price*dqua.Quantity) from [ACS_Dining].[dbo].[DishQuantityRelations] rels
	join [ACS_Dining].[dbo].[MfdDishPriceRelations] mfddprice
	on mfddprice.MenuForDayId=dordmenu.MenuForDay_ID
	inner join [ACS_Dining].[dbo].[DishPrice] dprice
	on dprice.Id=mfddprice.DishPriceId
	inner join [ACS_Dining].[dbo].[Dish] dishes
	on dishes.DishID=mfddprice.DishID
	inner join [ACS_Dining].[dbo].[MenuForDay] mfd
	on mfd.ID=dordmenu.MenuForDay_ID and mfddprice.MenuForDayId=mfd.ID
	inner join [ACS_Dining].[dbo].[WorkingDay] wdays
	on wdays.Id=mfd.WorkingDay_Id and wdays.IsWorking=1
	inner join [ACS_Dining].[dbo].[DishQuantity] dqua
	on dqua.Id=rels.DishQuantityId
	inner join  [ACS_Dining].[dbo].[DishType] dtypes
	on dtypes.Id=rels.DishTypeId and dtypes.Id=dishes.DishType_Id
	where rels.DayOrderMenuId=dordmenu.Id)
	from [ACS_Dining].[dbo].[DayOrderMenu] dordmenu
	inner join [ACS_Dining].[dbo].[MenuForDay] mfd
	on mfd.ID=dordmenu.MenuForDay_ID 
	inner join [ACS_Dining].[dbo].[WorkingDay] wdays
	on wdays.Id=mfd.WorkingDay_Id and wdays.IsWorking=1
	where dordmenu.WeekOrderMenu_Id=@weekorderid
	--обновляем сумму к оплате для данного недельного заказа
	update weekordmenu
	set weekordmenu.[WeekOrderSummaryPrice]=( select sum(dordmenu.DayOrderSummaryPrice) from [ACS_Dining].[dbo].[DayOrderMenu] dordmenu
	inner join  [ACS_Dining].[dbo].[MenuForDay] mfd
	on mfd.ID=dordmenu.MenuForDay_ID
	inner join [ACS_Dining].[dbo].[WorkingDay] wdays
	on wdays.Id=mfd.WorkingDay_Id and wdays.IsWorking=1
	where dordmenu.WeekOrderMenu_Id=weekordmenu.Id)
	from  [ACS_Dining].[dbo].[WeekOrderMenu] weekordmenu
	where weekordmenu.Id=@weekorderid
	
	select @curbalance=[WeekOrderSummaryPrice] 
	from  [ACS_Dining].[dbo].[WeekOrderMenu] weekordmenu
	where weekordmenu.Id=@weekorderid
	
	--добавляем остаток по оплате с прошлой недели
	select @curbalance=-1*@curbalance+WeekPaiment.PreviousWeekBalance + WeekPaiment.Paiment
	from WeekPaiment where WeekPaiment.WeekOrderMenu_Id=@weekorderid
	--Получаем запрашиваемую неделю
	declare @week int,@year int
	select @week=wweek.WeekNumber, @year=years.YearNumber from WeekOrderMenu
	inner join MenuForWeek
	on MenuForWeek.ID=WeekOrderMenu.MenuForWeek_ID 
	inner join WorkingWeek wweek
	on wweek.ID=MenuForWeek.WorkingWeek_ID
	inner join dbo.[Year] years
	on years.Id=wweek.Year_Id
	where WeekOrderMenu.Id=@weekorderid
	--Получаем ID меню на следующую неделю
	declare @userid nvarchar(128)
	select @userid=[User_Id] from WeekOrderMenu where WeekOrderMenu.Id=@weekorderid
	
	declare @NEXTWEEKMENUID int
    SET @NEXTWEEKMENUID=(SELECT MenuForWeek.ID FROM MenuForWeek
	INNER JOIN WorkingWeek
	ON MenuForWeek.[WorkingWeek_ID]=WorkingWeek.[ID]
	INNER JOIN dbo.GetNextWeekYear(@week,@year) NEXTWEEK
	ON NEXTWEEK.[WEEK]=WorkingWeek.[WeekNumber]
	INNER JOIN [ACS_Dining].[dbo].[Year] YEARS
	ON YEARS.[YearNumber]=NEXTWEEK.[YEAR] AND YEARS.ID=WorkingWeek.[Year_Id])
	--если существует меню на след неделю, ищем заказ текущего пользователя на это меню
	--и обновляем в нём значение остатка с прошлой недели
	if @NEXTWEEKMENUID is not null
	begin
		declare @nextweekordid int
		select @nextweekordid=WeekOrderMenu.[Id] from WeekOrderMenu
		where WeekOrderMenu.MenuForWeek_ID=@NEXTWEEKMENUID and WeekOrderMenu.[User_Id]=@userid
		
		if @nextweekordid is not null and @nextweekordid!=@weekorderid
		begin			
			update WP
			set WP.PreviousWeekBalance=@curbalance
			from WeekPaiment WP
			where WP.WeekOrderMenu_Id=@nextweekordid
			select @nextbalance=WeekPaiment.PreviousWeekBalance-WeekOrderMenu.WeekOrderSummaryPrice+WeekPaiment.Paiment
			from WeekOrderMenu
			inner join WeekPaiment on WeekPaiment.WeekOrderMenu_Id=WeekOrderMenu.Id
			where WeekOrderMenu.Id=@nextweekordid
			set @curbalance=@nextbalance
		end
	end
	--заносим полученное значение в баланс данного пользователя
	update AspNetUsers
	set Balance=@curbalance --, CheckDebt=cast(case when -1*@curbalance<AllowableDebt then 1 else 0 end as bit)
	from AspNetUsers 	where Id=@userid
END
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Обновляет значение оплаты за заказ на текущую неделю а также текущий баланс пользователя>
-- =============================================
CREATE PROC [dbo].[UpdateWeekPaiment]
		-- Add the parameters for the stored procedure here
	@weekpaimentid int,
	@paiment float
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	UPDATE WeekPaiment 
	SET [Paiment]=@paiment
	WHERE [Id]=@weekpaimentid
	
	DECLARE @weekorderid INT
	SELECT @weekorderid=WeekPaiment.[WeekOrderMenu_Id] FROM WeekPaiment WHERE WeekPaiment.Id=@weekpaimentid
	EXEC UpdateBalanceByWeekOrderId @weekorderid
		
	select Balance from AspNetUsers
	inner join WeekOrderMenu
	on WeekOrderMenu.Id=@weekorderid and WeekOrderMenu.[User_Id]=AspNetUsers.Id
		
END
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Обновляем баланс по тем пользователям, которые 
-- заказали ненулевое количество изменяемого блюда в данном дневном меню>
-- =============================================
CREATE PROCEDURE "dbo"."UpdateBalanceByDayMenuDishTypeId"
	-- Add the parameters for the stored procedure here
	@daymenuid INT,
	@dishtypeid int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	DECLARE @WEEKORDID INT
	DECLARE	WEEKORD_USER_CURSOR CURSOR
	SCROLL
	FOR   SELECT word.[Id]
		  FROM [ACS_Dining].[dbo].[WeekOrderMenu] word
		  INNER JOIN DayOrderMenu
		  ON DayOrderMenu.WeekOrderMenu_Id=word.Id 
		  INNER JOIN DishQuantityRelations drels
		  ON drels.DayOrderMenuId=DayOrderMenu.Id 
		  INNER JOIN MfdDishPriceRelations
		  ON MfdDishPriceRelations.MenuForDayId=@daymenuid AND DayOrderMenu.MenuForDay_ID=MfdDishPriceRelations.MenuForDayId
		  INNER JOIN Dish
		  ON MfdDishPriceRelations.DishId=Dish.DishID AND Dish.DishType_Id=drels.DishTypeId AND Dish.DishType_Id=@dishtypeid
		  INNER JOIN DishQuantity
		  ON DishQuantity.Id=drels.DishQuantityId AND DishQuantity.Quantity!=0.00
	OPEN WEEKORD_USER_CURSOR
	FETCH NEXT FROM WEEKORD_USER_CURSOR INTO @WEEKORDID
	WHILE @@FETCH_STATUS=0
	BEGIN
		exec UpdateBalanceByWeekOrderId @WEEKORDID
		FETCH NEXT FROM WEEKORD_USER_CURSOR INTO @WEEKORDID
	END
END
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Осуществляет заказ "Всё по одному">
-- =============================================
CREATE PROC "dbo"."OrderAllByOne" 
	-- Add the parameters for the stored procedure here
	@weekorderid INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	update drels
	set drels.DishQuantityId=(select Id from DishQuantity where Quantity=1)
	from DishQuantityRelations drels
	inner join DayOrderMenu
	on DayOrderMenu.WeekOrderMenu_Id=@weekorderid and drels.DayOrderMenuId=DayOrderMenu.Id
	inner join MenuForDay
	on DayOrderMenu.MenuForDay_ID=MenuForDay.ID and MenuForDay.OrderCanBeChanged=1
	inner join WorkingDay
	on MenuForDay.WorkingDay_Id=WorkingDay.Id and WorkingDay.IsWorking=1	
	
	exec UpdateBalanceByWeekOrderId  @weekorderid
	
    -- Insert statements for procedure here
	SELECT * from dbo.FactDishQuantByWeekOrderId(@weekorderid)
END
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Создаёт такой же заказ на данной неделе 
-- как и на предыдущей для данного пользователя>
-- =============================================
CREATE PROCEDURE [dbo].[OrderAsPrevWeek] 
	-- Add the parameters for the stored procedure here
	@weekorderid INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	Declare @curWeek int, @curyear int
	select @curWeek=WorkingWeek.[WeekNumber],@curyear=years.[YearNumber] from WeekOrderMenu
	inner join MenuForWeek
	on MenuForWeek.ID=WeekOrderMenu.MenuForWeek_ID
	inner join WorkingWeek
	on WorkingWeek.ID=MenuForWeek.WorkingWeek_ID
	inner join dbo.[Year] years
	on years.Id=WorkingWeek.Year_Id
	where WeekOrderMenu.Id=@weekorderid
	
	declare @preweekordid int
	select @preweekordid=WeekOrderMenu.Id from WeekOrderMenu
	inner join MenuForWeek
	on MenuForWeek.ID=WeekOrderMenu.MenuForWeek_ID
	inner join WorkingWeek
	on WorkingWeek.ID=MenuForWeek.WorkingWeek_ID
	inner join dbo.[Year] years
	on years.Id=WorkingWeek.Year_Id
	inner join dbo.GetPrevWeekYear(@curWeek,@curyear) prew
	on prew.[WEEK]=WorkingWeek.WeekNumber and prew.[YEAR]=years.YearNumber
	inner join AspNetUsers 
	on AspNetUsers.Id=(select [User_Id] from WeekOrderMenu where Id=@weekorderid)
	where WeekOrderMenu.[User_Id]=AspNetUsers.Id
	
	update drelscurrent
	set drelscurrent.DishQuantityId = dqua.Id
	from DishQuantityRelations drelscurrent
	INNER JOIN [ACS_Dining].[dbo].[DayOrderMenu] AS dayordmenuscurrent
	ON dayordmenuscurrent.Id=drelscurrent.[DayOrderMenuId] and dayordmenuscurrent.WeekOrderMenu_Id=@weekorderid
	inner join [ACS_Dining].[dbo].[MenuForDay] as daymenucurrent
	ON daymenucurrent.ID=dayordmenuscurrent.MenuForDay_ID and daymenucurrent.OrderCanBeChanged=1
	INNER JOIN [ACS_Dining].[dbo].[WorkingDay] AS curWDAYS
	ON curWDAYS.Id=daymenucurrent.WorkingDay_Id 
	INNER JOIN [ACS_Dining].[dbo].[DishType] DISHTYPES
	ON DISHTYPES.Id=drelscurrent.[DishTypeId] 	
	inner join [ACS_Dining].[dbo].[DayOfWeek] dow
	on dow.Id=curWDAYS.DayOfWeek_Id
	
	inner join DishQuantityRelations drelsprev
	on drelsprev.DishTypeId=DISHTYPES.Id
	INNER JOIN [ACS_Dining].[dbo].[DayOrderMenu] AS dayordmenusprev
	ON dayordmenusprev.Id=drelsprev.[DayOrderMenuId] and dayordmenusprev.WeekOrderMenu_Id=@preweekordid
	inner join [ACS_Dining].[dbo].[MenuForDay] as daymenuprew
	ON daymenuprew.ID=dayordmenusprev.MenuForDay_ID
	INNER JOIN [ACS_Dining].[dbo].[WorkingDay] AS prevWDAYS
	ON prevWDAYS.Id=daymenuprew.WorkingDay_Id and prevWDAYS.DayOfWeek_Id=dow.Id
	inner join DishQuantity dqua
	on drelsprev.DishQuantityId=dqua.Id
	
	exec dbo.UpdateBalanceByWeekOrderId  @weekorderid
	
	--SELECT * from dbo.FactDishQuantByWeekOrderId(@weekorderid)
END
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Обновляет пользовательский заказ по id недельного заказа используя значения 
-- Id дневных заказов и массив значений количеств заказанных блюд>
-- =============================================
CREATE PROCEDURE [dbo].[UpdateAllQuantitiesOnWeekOrder] 
	-- Add the parameters for the stored procedure here
	@DayordidArray AS dbo.DayOrdArray READONLY, --workdays ids those who ordercanbechanged=true
	@weekordid int, -- weekorderid
	@QuantArray AS dbo.QuantArray READONLY -- order dish quantity array
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	Declare @planweekordid int, @dayordid int, @plandayordid int, @dishtypecount int,
	 @dishtypeid int, @dishquant float
	 
	select @dishtypecount=COUNT(*) from DishType
	
	select @planweekordid=planword.Id from PlannedWeekOrderMenu planword
	inner join WeekOrderMenu weekord
	on weekord.MenuForWeek_ID=planword.MenuForWeek_ID and weekord.Id=@weekordid
	
	 IF OBJECT_ID('tempdb..#ActiveOrderQuantity') IS NOT NULL DROP TABLE #ActiveOrderQuantity
	 
	 -- tempare table for QuantArray
	 Create Table  #ActiveOrderQuantity(
		 RowID int IDENTITY(1, 1), 
		 QuantVal float
	)
	 -- Insert the resultset we want to loop through
		-- into the temporary table
		INSERT INTO #ActiveOrderQuantity (QuantVal)
		select quant from @QuantArray
	 
	 declare @NumberQuants int, @RowCount int
	 set @NumberQuants=@@ROWCOUNT
	 set @RowCount=1
	 
	 IF OBJECT_ID('tempdb..#ActiveDayOrders') IS NOT NULL DROP TABLE #ActiveDayOrders
	 
	  Create Table  #ActiveDayOrders(
			 RowID int IDENTITY(1, 1), 
			 DayOrdId int
		)
	 -- Insert the resultset we want to loop through
		-- into the temporary table
		INSERT INTO #ActiveDayOrders (DayOrdId)
		select dayord from @DayordidArray
		
		
	 declare @NumberDayOrds int
	 set @NumberDayOrds=@@ROWCOUNT
	 	
	while @RowCount<=@NumberQuants
	begin
		select @dishquant=QuantVal from #ActiveOrderQuantity
		where RowID=@RowCount
		
		set @dishtypeid=case when @RowCount%@dishtypecount=0 then @dishtypecount else @RowCount%@dishtypecount end
		select @dayordid=DayOrdId from #ActiveDayOrders where RowID=ceiling((@RowCount-1)/@dishtypecount)+1
		
		update drels
		set  [DishQuantityId]=DQUA.[Id]
		from DishQuantityRelations drels
		inner join DishType
		on DishType.Id=drels.DishTypeId and DishType.Id=@dishtypeid
		inner join  DishQuantity AS DQUA
		on DQUA.[Quantity]=@dishquant
		where drels.DayOrderMenuId=@dayordid
		
		set @RowCount = @RowCount+1
	end
	
	exec UpdateBalanceByWeekOrderId  @weekordid
	
END
GO
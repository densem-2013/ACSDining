USE "ACS_Dining"
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE FUNCTION "CurrentWeekYear"()
RETURNS @curweekyear TABLE 
(
	[WEEK] integer,
	[YEAR] integer
)
AS
	begin
	-- Add the SELECT statement with parameter references here
	INSERT @curweekyear SELECT DATEPART(WEEK,DATEADD(WEEK,-1,DATEADD(DAY,-1,GETDATE()))) , DATEPART(YEAR,GETDATE()) ;
	return
	end
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Возвращает номер недели и год для следующей недели>
-- =============================================
CREATE FUNCTION GetNextWeekYear 
(	)
RETURNS @nextweekyear TABLE 
(
	[WEEK] integer,
	[YEAR] integer
)
AS
	begin
	-- Add the SELECT statement with parameter references here
	INSERT @nextweekyear SELECT DATEPART(WEEK,DATEADD(DAY,-1,GETDATE())) , DATEPART(YEAR,GETDATE()) ;
	return
	end
GO
/****** Object:  UserDefinedFunction [dbo].[GetDishQuantity]    Script Date: 05/03/2016 17:31:53 ******/
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Возвращает значение хранимого фактическое количества по введенным ID категории, дневного меню, дневного заказа, плановой дневной заявки>
-- =============================================
CREATE FUNCTION "GetFuctDishQuantity"
(
	-- Add the parameters for the function here
	@dtypeid integer,
	@daymenuid integer,
	@dayorderid integer
)
RETURNS FLOAT(2)
BEGIN
	DECLARE @Quant FLOAT(2)
	SELECT @Quant=[Quantity] FROM [ACS_Dining].[dbo].[DishQuantity] DQUA
	INNER JOIN [ACS_Dining].[dbo].[DishQuantityRelations] RELS
	ON  RELS.[DishQuantityId]=DQUA.[Id]
	INNER JOIN [ACS_Dining].[dbo].[DishType] DISHTYPES
	ON DISHTYPES.Id=RELS.[DishTypeId] AND DISHTYPES.Id=@dtypeid	
	INNER JOIN [ACS_Dining].[dbo].[DayOrderMenu] AS dayordmenus
	ON dayordmenus.Id=RELS.[DayOrderMenuId] AND dayordmenus.Id=@dayorderid
	RETURN @Quant
END
GO

/****** Object:  UserDefinedFunction [dbo].[GetDishQuantity]    Script Date: 05/03/2016 17:31:53 ******/
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Возвращает значение хранимого планового количества по введенным ID категории, дневного меню, дневного заказа, плановой дневной заявки>
-- =============================================
CREATE FUNCTION "GetPlanDishQuantity"
(
	-- Add the parameters for the function here
	@dtypeid integer,
	@daymenuid integer,
	@plandayordid integer
)
RETURNS FLOAT(2)
BEGIN
	DECLARE @Quant FLOAT(2)
	SELECT @Quant=[Quantity] FROM [ACS_Dining].[dbo].[DishQuantity] DQUA
	INNER JOIN [ACS_Dining].[dbo].[PlanDishQuantityRelations] PLANRELS
	ON  PLANRELS.[DishQuantityId]=DQUA.[Id]
	INNER JOIN [ACS_Dining].[dbo].[DishType] DISHTYPES
	ON DISHTYPES.Id=PLANRELS.[DishTypeId] AND DISHTYPES.Id=@dtypeid	
	INNER JOIN [ACS_Dining].[dbo].[PlannedDayOrderMenu] AS plandayordmenus
	ON plandayordmenus.Id=PLANRELS.[PlannedDayOrderMenuId] AND plandayordmenus.Id=@plandayordid
	RETURN @Quant
END
GO
/****** Object:  UserDefinedFunction [dbo].[WeekOrdersByWeekYear]    Script Date: 05/03/2016 17:16:41 ******/

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Возвращает имена пользователей и ID их соответствующих недельных заказов
--               по входящему номеру недели и года>
-- =============================================
CREATE FUNCTION "WeekOrdersByWeekYear"
(
	@week integer,
	@year integer
)
RETURNS @weekords TABLE 
(
	[UserId]nvarchar(128),
	[UserName] nvarchar(128)
	,[WeekOrderId] int
)
AS
BEGIN
INSERT @weekords 
SELECT USERS.[Id],USERS.[UserName] ,ORDS.[Id]
  FROM [ACS_Dining].[dbo].[WeekOrderMenu] AS ORDS 
  INNER JOIN [ACS_Dining].[dbo].[AspNetUsers] AS USERS
  ON USERS.[Id]=ORDS.[User_Id]
  INNER JOIN [ACS_Dining].[dbo].[MenuForWeek] AS MENU
  ON ORDS.[MenuForWeek_ID]=MENU.[ID] 
  INNER JOIN [ACS_Dining].[dbo].[WorkingWeek] AS WEEKS 
  ON MENU.[WorkingWeek_ID]=WEEKS.[ID] AND WEEKS.[WeekNumber]=@week
  INNER JOIN [ACS_Dining].[dbo].[YEAR] AS YEARS
  ON WEEKS.[Year_Id]= YEARS.[ID] AND YEARS.[YearNumber]=@year
  ORDER BY USERS.[UserName];
  RETURN 
END
GO
/****** Object:  UserDefinedFunction [dbo].[DishQuantByWeekOrderId]    Script Date: 05/03/2016 18:05:46 ******/

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Возвращает фактические значения количеств заказанных блюд для указанного недельного заказа>
-- =============================================
CREATE FUNCTION "FactDishQuantByWeekOrderId"
(
	@weekordid integer
)
RETURNS @userweekord TABLE 
(
	[DishQuant] float
)
AS
BEGIN
	INSERT @userweekord 	
	SELECT [Quantity] FROM [ACS_Dining].[dbo].[DishQuantity] DQUA
	INNER JOIN [ACS_Dining].[dbo].[DishQuantityRelations] RELS
	ON  RELS.[DishQuantityId]=DQUA.[Id]
	INNER JOIN [ACS_Dining].[dbo].[DishType] DISHTYPES
	ON DISHTYPES.Id=RELS.[DishTypeId] 	
	INNER JOIN [ACS_Dining].[dbo].[DayOrderMenu] AS dayordmenus
	ON dayordmenus.Id=RELS.[DayOrderMenuId] 
	INNER JOIN [ACS_Dining].[dbo].[WeekOrderMenu] AS WORDMENU
	ON  WORDMENU.[ID]=@weekordid AND dayordmenus.[WeekOrderMenu_Id]=WORDMENU.[ID] 
	inner join [ACS_Dining].[dbo].[MenuForDay] as daymenu
	ON daymenu.ID=dayordmenus.MenuForDay_ID
	INNER JOIN [ACS_Dining].[dbo].[WorkingDay] AS WDAYS
	ON WDAYS.Id=daymenu.WorkingDay_Id AND WDAYS.IsWorking=1	
	ORDER BY WDAYS.DayOfWeek_Id
	
	INSERT  @userweekord 
	SELECT SUM(DQUA.[Quantity]*DP.[Price]) FROM [ACS_Dining].[dbo].[DishQuantity] DQUA
	INNER JOIN [ACS_Dining].[dbo].[DishQuantityRelations] RELS
	ON  RELS.[DishQuantityId]=DQUA.[Id]
	INNER JOIN [ACS_Dining].[dbo].[DishType] DISHTYPES
	ON DISHTYPES.Id=RELS.[DishTypeId] 	
	INNER JOIN [ACS_Dining].[dbo].[Dish] DISHES
	ON RELS.[DishTypeId]=DISHES.DishType_Id
	INNER JOIN [ACS_Dining].[dbo].[DayOrderMenu] AS dayordmenus
	ON dayordmenus.Id=RELS.[DayOrderMenuId] 
	INNER JOIN [ACS_Dining].[dbo].[WeekOrderMenu] AS WORDMENU
	ON  WORDMENU.[ID]=@weekordid AND dayordmenus.[WeekOrderMenu_Id]=WORDMENU.[ID] 
	inner join [ACS_Dining].[dbo].[MenuForDay] as daymenu
	ON daymenu.ID=dayordmenus.MenuForDay_ID
	INNER JOIN [ACS_Dining].[dbo].[MfdDishPriceRelations] AS MPRELS
	ON MPRELS.DishID=DISHES.DishID AND daymenu.ID=MPRELS.[MenuForDayId]
	INNER JOIN [ACS_Dining].[dbo].[DishPrice] DP
	ON DP.Id=MPRELS.[DishPriceId]
	INNER JOIN [ACS_Dining].[dbo].[WorkingDay] AS WDAYS
	ON WDAYS.Id=daymenu.WorkingDay_Id AND WDAYS.IsWorking=1	
	INNER JOIN [ACS_Dining].[dbo].[WorkingWeek] WORKINGWEEKS
	ON WORKINGWEEKS.ID=WDAYS.WorkingWeek_ID 
	INNER JOIN [ACS_Dining].[dbo].[Year] YEARS
	ON YEARS.Id=WORKINGWEEKS.Year_Id 	
	RETURN 
END
GO
/****** Object:  UserDefinedFunction [dbo].[DishQuantByWeekOrderId]    Script Date: 05/03/2016 18:05:46 ******/

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Возвращает плановые значения количеств заказанных блюд для указанного недельного заказа>
-- =============================================
CREATE FUNCTION "PlanDishQuantByWeekOrderId"
(
	@planweekordid integer
)
RETURNS @userweekord TABLE 
(
	[DishQuant] float
)
AS
BEGIN
	INSERT @userweekord 	
	SELECT [Quantity] FROM [ACS_Dining].[dbo].[DishQuantity] DQUA
	INNER JOIN [ACS_Dining].[dbo].[PlanDishQuantityRelations] PLANRELS
	ON  PLANRELS.[DishQuantityId]=DQUA.[Id]
	INNER JOIN [ACS_Dining].[dbo].[DishType] DISHTYPES
	ON DISHTYPES.Id=PLANRELS.[DishTypeId] 	
	INNER JOIN [ACS_Dining].[dbo].[PlannedDayOrderMenu] AS plandayordmenus
	ON plandayordmenus.Id=PLANRELS.[PlannedDayOrderMenuId] 
	INNER JOIN [ACS_Dining].[dbo].[PlannedWeekOrderMenu] AS PLANWORDMENU
	ON PLANWORDMENU.[ID]=@planweekordid AND plandayordmenus.[PlannedWeekOrderMenu_Id]=PLANWORDMENU.[ID]  
	inner join [ACS_Dining].[dbo].[MenuForDay] as daymenu
	ON daymenu.ID=plandayordmenus.MenuForDay_ID
	INNER JOIN [ACS_Dining].[dbo].[WorkingDay] AS WDAYS
	ON WDAYS.Id=daymenu.WorkingDay_Id AND WDAYS.IsWorking=1	
	ORDER BY WDAYS.DayOfWeek_Id
	
	INSERT @userweekord 
	SELECT SUM(DQUA.[Quantity]*DP.[Price]) FROM [ACS_Dining].[dbo].[DishQuantity] DQUA
	INNER JOIN [ACS_Dining].[dbo].[PlanDishQuantityRelations] PLANRELS
	ON  PLANRELS.[DishQuantityId]=DQUA.[Id]
	INNER JOIN [ACS_Dining].[dbo].[DishType] DISHTYPES
	ON DISHTYPES.Id=PLANRELS.[DishTypeId] 	
	INNER JOIN [ACS_Dining].[dbo].[Dish] DISHES
	ON PLANRELS.[DishTypeId]=DISHES.DishType_Id
	INNER JOIN [ACS_Dining].[dbo].[PlannedDayOrderMenu] AS plandayordmenus
	ON plandayordmenus.Id=PLANRELS.[PlannedDayOrderMenuId] 
	INNER JOIN [ACS_Dining].[dbo].[PlannedWeekOrderMenu] AS PLANWORDMENU
	ON PLANWORDMENU.[ID]=@planweekordid AND plandayordmenus.[PlannedWeekOrderMenu_Id]=PLANWORDMENU.[ID]  
	inner join [ACS_Dining].[dbo].[MenuForDay] as daymenu
	ON daymenu.ID=plandayordmenus.MenuForDay_ID
	INNER JOIN [ACS_Dining].[dbo].[MfdDishPriceRelations] AS MPRELS
	ON MPRELS.DishID=DISHES.DishID AND daymenu.ID=MPRELS.[MenuForDayId]
	INNER JOIN [ACS_Dining].[dbo].[DishPrice] DP
	ON DP.Id=MPRELS.[DishPriceId]
	INNER JOIN [ACS_Dining].[dbo].[WorkingDay] AS WDAYS
	ON WDAYS.Id=daymenu.WorkingDay_Id AND WDAYS.IsWorking=1	
	INNER JOIN [ACS_Dining].[dbo].[WorkingWeek] WORKINGWEEKS
	ON WORKINGWEEKS.ID=WDAYS.WorkingWeek_ID 
	INNER JOIN [ACS_Dining].[dbo].[Year] YEARS
	ON YEARS.Id=WORKINGWEEKS.Year_Id 	
	RETURN 
END
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Выводит цены на блюда меню указанной рабочей недели в году>
-- =============================================
CREATE FUNCTION "GetWeekDishPrices"
(
	@week integer,
	@year integer
)
RETURNS @WeekDishPrices TABLE 
(
	[Price] float
)
AS
BEGIN
INSERT @WeekDishPrices

	SELECT DP.[Price] FROM [ACS_Dining].[dbo].[Dish] DISHES
	INNER JOIN [ACS_Dining].[dbo].[DishType] DISHTYPES
	ON DISHTYPES.Id=DISHES.DishType_Id
	INNER JOIN [ACS_Dining].[dbo].[MfdDishPriceRelations] AS MPRELS
	ON MPRELS.DishID=DISHES.DishID 
	INNER JOIN [ACS_Dining].[dbo].[DishPrice] DP
	ON DP.Id=MPRELS.[DishPriceId]
	INNER JOIN [ACS_Dining].[dbo].[MenuForDay] DAYMENUS
	ON DAYMENUS.ID=MPRELS.[MenuForDayId]
	INNER JOIN [ACS_Dining].[dbo].[WorkingDay] WORKINGDAYS
	ON WORKINGDAYS.Id=DAYMENUS.WorkingDay_Id AND WORKINGDAYS.IsWorking=1
	INNER JOIN [ACS_Dining].[dbo].[WorkingWeek] WORKINGWEEKS
	ON WORKINGWEEKS.ID=WORKINGDAYS.WorkingWeek_ID AND WORKINGWEEKS.WeekNumber=@week
	INNER JOIN [ACS_Dining].[dbo].[Year] YEARS
	ON YEARS.Id=WORKINGWEEKS.Year_Id AND YEARS.YearNumber=@year
	ORDER BY WORKINGDAYS.DayOfWeek_Id
	
	RETURN 
END
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Получает суммарные фактические количества по каждому заказанному блюду на неделе всеми пользователями>
-- =============================================
CREATE FUNCTION	"GetFactSumWeekUserCounts"
(	
	@week integer,
	@year integer
)
RETURNS @SumByDishes TABLE 
(
	[SumByDishes] float
)
AS
BEGIN
    INSERT @SumByDishes
	SELECT SUM([Quantity]) FROM [ACS_Dining].[dbo].[DishQuantity] DQUA
	INNER JOIN [ACS_Dining].[dbo].[DishQuantityRelations] RELS
	ON  RELS.[DishQuantityId]=DQUA.[Id]
	INNER JOIN [ACS_Dining].[dbo].[DishType] DISHTYPES
	ON DISHTYPES.Id=RELS.[DishTypeId] 	
	INNER JOIN [ACS_Dining].[dbo].[DayOrderMenu] AS dayordmenus
	ON dayordmenus.Id=RELS.[DayOrderMenuId] 
	INNER JOIN [ACS_Dining].[dbo].[WeekOrderMenu] AS WORDMENU
	ON dayordmenus.[WeekOrderMenu_Id]=WORDMENU.[ID] 
	inner join [ACS_Dining].[dbo].[MenuForDay] as daymenu
	ON daymenu.ID=dayordmenus.MenuForDay_ID
	INNER JOIN [ACS_Dining].[dbo].[WorkingDay] AS WDAYS
	ON WDAYS.Id=daymenu.WorkingDay_Id AND WDAYS.IsWorking=1	
	INNER JOIN [ACS_Dining].[dbo].[WorkingWeek] WORKINGWEEKS
	ON WORKINGWEEKS.ID=WDAYS.WorkingWeek_ID AND WORKINGWEEKS.WeekNumber=@week
	INNER JOIN [ACS_Dining].[dbo].[Year] YEARS
	ON YEARS.Id=WORKINGWEEKS.Year_Id AND YEARS.YearNumber=@year
	
	GROUP BY WDAYS.DayOfWeek_Id,DISHTYPES.Id
	ORDER BY WDAYS.DayOfWeek_Id,DISHTYPES.Id
	RETURN 
END
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Получает суммарные плановые количества по каждому заказанному блюду на неделе всеми пользователями>
-- =============================================
CREATE FUNCTION	"GetPlanSumWeekUserCounts"
(	
	@week integer,
	@year integer
)
RETURNS @SumByDishes TABLE 
(
	[SumByDishes] float
)
AS
BEGIN
    INSERT @SumByDishes
	SELECT SUM([Quantity]) FROM [ACS_Dining].[dbo].[DishQuantity] DQUA
	INNER JOIN [ACS_Dining].[dbo].[DishQuantityRelations] RELS
	ON  RELS.[DishQuantityId]=DQUA.[Id]
	INNER JOIN [ACS_Dining].[dbo].[DishType] DISHTYPES
	ON DISHTYPES.Id=RELS.[DishTypeId] 	
	INNER JOIN [ACS_Dining].[dbo].[PlannedDayOrderMenu] AS plandayordmenus
	ON plandayordmenus.Id=RELS.[DayOrderMenuId] 
	INNER JOIN [ACS_Dining].[dbo].[PlannedWeekOrderMenu] AS PLANWORDMENU
	ON plandayordmenus.[PlannedWeekOrderMenu_Id]=PLANWORDMENU.[ID] 
	inner join [ACS_Dining].[dbo].[MenuForDay] as daymenu
	ON daymenu.ID=plandayordmenus.MenuForDay_ID
	INNER JOIN [ACS_Dining].[dbo].[WorkingDay] AS WDAYS
	ON WDAYS.Id=daymenu.WorkingDay_Id AND WDAYS.IsWorking=1	
	INNER JOIN [ACS_Dining].[dbo].[WorkingWeek] WORKINGWEEKS
	ON WORKINGWEEKS.ID=WDAYS.WorkingWeek_ID AND WORKINGWEEKS.WeekNumber=@week
	INNER JOIN [ACS_Dining].[dbo].[Year] YEARS
	ON YEARS.Id=WORKINGWEEKS.Year_Id AND YEARS.YearNumber=@year
	
	GROUP BY WDAYS.DayOfWeek_Id,DISHTYPES.Id
	ORDER BY WDAYS.DayOfWeek_Id,DISHTYPES.Id
	RETURN 
END
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Возвращает значения оплаты за каждую позицию блюда для указанного недельного заказа>
-- =============================================
CREATE FUNCTION "WeekPaimentByOrderId"
(
	@weekordid integer
)
RETURNS @userweekpaiment TABLE 
(
	[DishPaiment] float
)
AS
BEGIN
	INSERT @userweekpaiment 	
	SELECT [Quantity]*DP.[Price] FROM [ACS_Dining].[dbo].[DishQuantity] DQUA
	INNER JOIN [ACS_Dining].[dbo].[DishQuantityRelations] RELS
	ON  RELS.[DishQuantityId]=DQUA.[Id]
	INNER JOIN [ACS_Dining].[dbo].[DishType] DISHTYPES
	ON DISHTYPES.Id=RELS.[DishTypeId] 	
	INNER JOIN [ACS_Dining].[dbo].[Dish] DISHES
	ON RELS.[DishTypeId]=DISHES.DishType_Id
	INNER JOIN [ACS_Dining].[dbo].[DayOrderMenu] AS dayordmenus
	ON dayordmenus.Id=RELS.[DayOrderMenuId] 
	INNER JOIN [ACS_Dining].[dbo].[WeekOrderMenu] AS WORDMENU
	ON  WORDMENU.[ID]=@weekordid AND dayordmenus.[WeekOrderMenu_Id]=WORDMENU.[ID] 
	inner join [ACS_Dining].[dbo].[MenuForDay] as daymenu
	ON daymenu.ID=dayordmenus.MenuForDay_ID
	INNER JOIN [ACS_Dining].[dbo].[MfdDishPriceRelations] AS MPRELS
	ON MPRELS.DishID=DISHES.DishID AND MPRELS.[MenuForDayId]=daymenu.ID
	INNER JOIN [ACS_Dining].[dbo].[DishPrice] DP
	ON DP.Id=MPRELS.[DishPriceId]
	INNER JOIN [ACS_Dining].[dbo].[WorkingDay] AS WDAYS
	ON WDAYS.Id=daymenu.WorkingDay_Id AND WDAYS.IsWorking=1	
	INNER JOIN [ACS_Dining].[dbo].[WorkingWeek] WORKINGWEEKS
	ON WORKINGWEEKS.ID=WDAYS.WorkingWeek_ID 
	INNER JOIN [ACS_Dining].[dbo].[Year] YEARS
	ON YEARS.Id=WORKINGWEEKS.Year_Id 
	ORDER BY WDAYS.DayOfWeek_Id
	
	INSERT @userweekpaiment
	SELECT SUM([DishPaiment])FROM @userweekpaiment
	RETURN 
END
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Суммы по планируемым оплатам на заказанное количество каждого блюда в меню на неделю>
-- =============================================
CREATE FUNCTION "SumWeekPaimentsByDishes"
(
	@week integer,
	@year integer
)
RETURNS @sumbydishes TABLE 
(
	[SumByDishes] float
)
AS
BEGIN
	INSERT @sumbydishes 	
	SELECT SUM([Quantity]*DP.[Price]) FROM [ACS_Dining].[dbo].[DishQuantity] DQUA
	INNER JOIN [ACS_Dining].[dbo].[DishQuantityRelations] RELS
	ON  RELS.[DishQuantityId]=DQUA.[Id]
	INNER JOIN [ACS_Dining].[dbo].[DishType] DISHTYPES
	ON DISHTYPES.Id=RELS.[DishTypeId] 	
	INNER JOIN [ACS_Dining].[dbo].[Dish] DISHES
	ON RELS.[DishTypeId]=DISHES.DishType_Id
	INNER JOIN [ACS_Dining].[dbo].[DayOrderMenu] AS dayordmenus
	ON dayordmenus.Id=RELS.[DayOrderMenuId] 
	INNER JOIN [ACS_Dining].[dbo].[WeekOrderMenu] AS WORDMENU
	ON  dayordmenus.[WeekOrderMenu_Id]=WORDMENU.[ID] 
	inner join [ACS_Dining].[dbo].[MenuForDay] as daymenu
	ON daymenu.ID=dayordmenus.MenuForDay_ID
	INNER JOIN [ACS_Dining].[dbo].[MfdDishPriceRelations] AS MPRELS
	ON MPRELS.DishID=DISHES.DishID AND MPRELS.[MenuForDayId]=daymenu.ID
	INNER JOIN [ACS_Dining].[dbo].[DishPrice] DP
	ON DP.Id=MPRELS.[DishPriceId]
	INNER JOIN [ACS_Dining].[dbo].[WorkingDay] AS WDAYS
	ON WDAYS.Id=daymenu.WorkingDay_Id AND WDAYS.IsWorking=1	
	INNER JOIN [ACS_Dining].[dbo].[WorkingWeek] WORKINGWEEKS
	ON WORKINGWEEKS.ID=WDAYS.WorkingWeek_ID AND WORKINGWEEKS.WeekNumber=@week
	INNER JOIN [ACS_Dining].[dbo].[Year] YEARS
	ON YEARS.Id=WORKINGWEEKS.Year_Id AND YEARS.YearNumber=@year
	
	GROUP BY WDAYS.DayOfWeek_Id,DISHTYPES.Id
	ORDER BY WDAYS.DayOfWeek_Id,DISHTYPES.Id
	
	RETURN 
END
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Обновляет значение заказа на блюдо а также сумму заказа на день и рабочую неделю>
-- =============================================
CREATE PROC "UpdateDishQuantity"
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
	EXEC UpdateBalanceByWeekOrderId @weekorderid
END
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Возвращает id текущего рабочего дня>
-- =============================================
CREATE FUNCTION "GetCurrentWorkDayId"()
RETURNS INT
BEGIN
	DECLARE @wdid int,@curweek INT,@year INT,@weekday INT
	SET @curweek=DATEPART(WEEK,DATEADD(WEEK,-1,DATEADD(DAY,-1,GETDATE())))
	SET @year=DATEPART(YEAR,GETDATE())
	SET @weekday=DATEPART(WEEKDAY,DATEADD(DAY,-1,GETDATE()))
	
	SET @wdid=(SELECT WDAYS.[Id] FROM [ACS_Dining].[dbo].[WorkingDay] WDAYS
	INNER JOIN [ACS_Dining].[dbo].[WorkingWeek] WWEKS
	ON WWEKS.[ID]=WDAYS.[WorkingWeek_ID] AND WWEKS.[WeekNumber]=@curweek
	INNER JOIN [ACS_Dining].[dbo].[Year] YEARS
	ON YEARS.[ID]=WWEKS.[Year_Id] AND YEARS.[YearNumber]=@year
	WHERE WDAYS.[DayOfWeek_Id]=@weekday)
	
	RETURN @wdid
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
--SET IDENTITY_INSERT dbo.WeekOrderMenu ON
--GO
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
	SELECT 1 ,DishType.Id,DayOrderMenu.Id 
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
	ON WorkingDay.Id=MenuForDay.WorkingDay_Id AND WorkingDay.IsWorking=1 AND WorkingDay.WorkingWeek_ID=WorkingWeek.ID
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
	SELECT DayOrderMenu.[DayOrderSummaryPrice], @MENUID, PlannedWeekOrderMenu.Id
	FROM DayOrderMenu 
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
-- Description:	<Возвращает названия рабочих дней недели для меню на неделю>
-- =============================================
IF OBJECT_ID (N'GetDayNames') IS NOT NULL
DROP FUNCTION GetDayNames;
GO
CREATE FUNCTION "GetDayNames"
(
	@week integer,
	@year integer,
	@withnoworking bit
)
RETURNS @daynames TABLE 
(
	[DayName] nvarchar(30)
)
AS
BEGIN
IF(@withnoworking=0)
BEGIN
	INSERT @daynames 	
	SELECT WEDAY.Name  FROM [ACS_Dining].[dbo].[DayOfWeek] AS WEDAY
	
	INNER JOIN [ACS_Dining].[dbo].[WorkingWeek] WorkingWeek
	ON WorkingWeek.WeekNumber=@week 
	INNER JOIN [ACS_Dining].[dbo].[WorkingDay] AS WDAYS
	ON WDAYS.WorkingWeek_ID=WorkingWeek.[ID] AND WDAYS.[DayOfWeek_Id]=WEDAY.[ID] 
	INNER JOIN [ACS_Dining].[dbo].[Year] YEARS
	ON YEARS.Id=WorkingWeek.Year_Id AND YEARS.YearNumber=@year	
	ORDER BY WEDAY.[Id]
END
ELSE
BEGIN
	INSERT @daynames 	
	SELECT WEDAY.Name  FROM [ACS_Dining].[dbo].[DayOfWeek] AS WEDAY
	
	INNER JOIN [ACS_Dining].[dbo].[WorkingWeek] WorkingWeek
	ON WorkingWeek.WeekNumber=@week 
	INNER JOIN [ACS_Dining].[dbo].[WorkingDay] AS WDAYS
	ON WDAYS.WorkingWeek_ID=WorkingWeek.[ID] AND WDAYS.[DayOfWeek_Id]=WEDAY.[ID] AND WDAYS.[IsWorking]=1
	INNER JOIN [ACS_Dining].[dbo].[Year] YEARS
	ON YEARS.Id=WorkingWeek.Year_Id AND YEARS.YearNumber=@year	
	ORDER BY WEDAY.[Id]
END
	RETURN 
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
	 insert [ACS_Dining].[dbo].[MenuForDay]
	 select 0.00,1,1,id,@weekmenuid
	 from  [ACS_Dining].[dbo].[WorkingDay]
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
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	DECLARE @WDAYID INT,@DAYMENUID INT,@WEEKMENUID INT, @ISWORKING BIT
	
	SET @WDAYID=dbo.GetCurrentWorkDayId();
	
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
	 AND MenuForDay.OrderCanBeChanged=1  AND MenuForDay.DayMenuCanBeChanged=1
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
	AND MenuForDay.OrderCanBeChanged=1  AND MenuForDay.DayMenuCanBeChanged=1
	AND MenuForDay.WorkingDay_Id<=@WDAYID
	INNER JOIN PlannedWeekOrderMenu
	ON PlannedDayOrderMenu.PlannedWeekOrderMenu_Id=PlannedWeekOrderMenu.Id
	AND  PlannedWeekOrderMenu.MenuForWeek_ID=@WEEKMENUID
	INNER JOIN DayOrderMenu
	ON DayOrderMenu.MenuForDay_ID=MenuForDay.ID
	INNER JOIN DishQuantityRelations
	ON DishQuantityRelations.DayOrderMenuId= DayOrderMenu.Id AND DishQuantityRelations.DishTypeId=planrels.DishTypeId
	INNER JOIN	AspNetUsers AS USERS 
	ON USERS.Id=PlannedWeekOrderMenu.[User_Id] 
	INNER JOIN AspNetUserRoles AS USROLES 
	ON USROLES.UserId = USERS.Id and USERS.IsExisting=1
	INNER JOIN AspNetRoles AS ROLES 
	ON ROLES.Id = USROLES.RoleId AND ROLES.Name = 'Employee'
	
	--ЗАКРЫВАЕМ ВОЗМОЖНОСТЬ РЕДАКТИРОВАНИЯ МЕНЮ ДЛЯ ПОЛЬЗОВАТЕЛЯ 
	--И ЗАКАЗОВ ДЛЯ КЛИЕНТОВ
	UPDATE MenuForDay
	SET [OrderCanBeChanged]=0,  [DayMenuCanBeChanged]=0
	WHERE MenuForDay.MenuForWeek_ID=@WEEKMENUID AND MenuForDay.WorkingDay_Id<=@WDAYID
END
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Обновляет значения сумм к оплате для дневных заказов,
-- сумму заказа на неделю и баланс пользователя, которому принадлежит заказ>
-- =============================================
CREATE PROCEDURE UpdateBalanceByWeekOrderId
	-- Add the parameters for the function here
	@weekorderid int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	DECLARE @curbalance float
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
	inner join [ACS_Dining].[dbo].[DishQuantity] dqua
	on dqua.Id=rels.DishQuantityId
	inner join  [ACS_Dining].[dbo].[DishType] dtypes
	on dtypes.Id=rels.DishTypeId and dtypes.Id=dishes.DishType_Id
	where rels.DayOrderMenuId=dordmenu.Id)
	from [ACS_Dining].[dbo].[DayOrderMenu] dordmenu
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
	select @curbalance=-1*@curbalance-WeekPaiment.PreviousWeekBalance + WeekPaiment.Paiment
	from WeekPaiment where WeekPaiment.WeekOrderMenu_Id=@weekorderid
	
	--заносим полученное значение в баланс данного пользователя
	update AspNetUsers
	set Balance=@curbalance
	from AspNetUsers 
	inner join WeekOrderMenu
	on WeekOrderMenu.[User_Id]=AspNetUsers.Id and WeekOrderMenu.Id=@weekorderid
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
END
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
CREATE FUNCTION [dbo].[CurrentWeekYear]()
RETURNS @curweekyear TABLE 
(
	[WEEK] integer,
	[YEAR] integer
)
AS
	begin
	-- Add the SELECT statement with parameter references here
	INSERT @curweekyear SELECT DATEPART(WEEK,DATEADD(WEEK,-1,DATEADD(DAY,-1,GETDATE()))) , DATEPART(YEAR,DATEADD(DAY,-1,GETDATE())) ;
	return
	end
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Возвращает номер недели и год для следующей недели>
-- =============================================
CREATE FUNCTION [dbo].[GetNextWeekYear] 
(	)
RETURNS @nextweekyear TABLE 
(
	[WEEK] integer,
	[YEAR] integer
)
AS
	begin
	-- Add the SELECT statement with parameter references here
	INSERT @nextweekyear SELECT DATEPART(WEEK,DATEADD(DAY,-1,GETDATE())) , DATEPART(YEAR,DATEADD(DAY,-1,GETDATE())) ;
	return
	end
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Возвращает номер недели и год для предыдущей недели>
-- =============================================
CREATE FUNCTION  [dbo].[GetPrevWeekYear] 
(
	@week int = null,
	@year int = null
)
RETURNS @prevweekyear TABLE 
(
	[WEEK] integer,
	[YEAR] integer
)
AS
begin
	-- Add the SELECT statement with parameter references here
	if @week is null
	begin
		INSERT @prevweekyear 
		SELECT DATEPART(WEEK,DATEADD(WEEK,-2,DATEADD(DAY,-1,GETDATE()))) , DATEPART(YEAR,DATEADD(WEEK,-2,DATEADD(DAY,-1,GETDATE()))) ;
	end
	else
	begin
		declare @needdate date
		select @needdate=CONVERT(date,convert(char(10),cast(@year as CHAR(10)) +'-01-01'))
		set @needdate=DATEADD(WEEK,-2,DATEADD(WEEK,@week,@needdate))
		INSERT @prevweekyear
		SELECT DatePart(ww,@needdate), DatePart(yy,@needdate)
	end
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
-- Create date: <Create Date, ,>
-- Description:	<Возвращает id текущего рабочего дня>
-- =============================================
CREATE FUNCTION [dbo].[GetCurrentWorkDayId]()
RETURNS INT
BEGIN
	DECLARE @wdid int,@curweek INT,@year INT,@weekday INT
	SET @curweek=DATEPART(WEEK,DATEADD(WEEK,-1,DATEADD(DAY,-1,GETDATE())))
	SET @year=DATEPART(YEAR,DATEADD(DAY,-1,GETDATE()))
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

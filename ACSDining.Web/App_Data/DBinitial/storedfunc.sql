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
CREATE FUNCTION CurrentWeekYear()
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

USE [ACS_Dining]
GO
/****** Object:  UserDefinedFunction [dbo].[GetDishQuantity]    Script Date: 05/03/2016 17:31:53 ******/
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Возвращает значение хранимого количества по введенным ID категории, дневного меню, дневного заказа, плановой дневной заявки>
-- =============================================
CREATE FUNCTION "GetDishQuantity"
(
	-- Add the parameters for the function here
	@dtypeid integer,
	@daymenuid integer,
	@dayorderid integer,
	@planorderid integer
)
RETURNS FLOAT(2)
BEGIN
	DECLARE @Quant FLOAT(2)
	SELECT @Quant=[Quantity] FROM [ACS_Dining].[dbo].[DishQuantity] WHERE [Id]=(
	SELECT [DishQuantityId]
	FROM [ACS_Dining].[dbo].[DishQuantityRelations] where [DishTypeId]=@dtypeid and [MenuForDayId]=@daymenuid and [DayOrderMenuId]=@dayorderid and [PlannedDayOrderMenuId]=@planorderid);
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
	[UserName] [nvarchar](128)
	,[WeekOrderId] [int] 
)
AS
BEGIN
INSERT @weekords 
SELECT USERS.[UserName]
	  ,ORDS.[Id]
  FROM [ACS_Dining].[dbo].[WeekOrderMenu] AS ORDS INNER JOIN [ACS_Dining].[dbo].[AspNetUsers] AS USERS
  ON USERS.[Id]=ORDS.[User_Id]
   WHERE ORDS.[MenuForWeek_ID]=(SELECT ID FROM [ACS_Dining].[dbo].[MenuForWeek] AS MENU 
								WHERE MENU.[WorkingWeek_ID]=(SELECT ID FROM [ACS_Dining].[dbo].[WorkingWeek] AS WEEKS 
								WHERE WEEKS.[ID]=MENU.[WorkingWeek_ID] AND WEEKS.[WeekNumber]=@week
								AND WEEKS.[Year_Id]=(SELECT ID FROM [ACS_Dining].[dbo].[YEAR] AS YEARS
								WHERE  YEARS.[YearNumber]=@year)))
	ORDER BY USERS.[UserName];
	RETURN 
END
GO
/****** Object:  UserDefinedFunction [dbo].[GetDayCount]    Script Date: 05/03/2016 17:26:46 ******/

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Возвращает количество рабочих дней в указанной неделе года,>
-- =============================================
CREATE FUNCTION "GetDayCount"
(
	@week integer,
	@year integer
)
RETURNS integer
AS
BEGIN
	-- Declare the return variable here
	DECLARE @daycount integer

	-- Add the T-SQL statements to compute the return value here
	SELECT @daycount=(SELECT COUNT(*) FROM [ACS_Dining].[dbo].[WorkingDay] AS WORKDAYS 
	WHERE WORKDAYS.[IsWorking]=1 AND WORKDAYS.[WorkingWeek_ID]=(SELECT WWEEKS.[ID] FROM [ACS_Dining].[dbo].[WorkingWeek] AS WWEEKS
	WHERE WWEEKS.[ID]=WORKDAYS.[WorkingWeek_ID] AND WWEEKS.[WeekNumber]=@week AND WWEEKS.[Year_Id]=(SELECT ID FROM [ACS_Dining].[dbo].[YEAR] AS YEARS
	WHERE YEARS.[YearNumber]=@year)));
	-- Return the result of the function
	RETURN @daycount

END
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	< Создаёт динамический SQL запрос для занесения в таблицу WeekUserOrdersReport 
--              данных о заказанных блюдах за неделю >
-- =============================================
CREATE FUNCTION [dbo].[DynSqlWeekOrders]
(
	@week integer,
	@year integer
)
RETURNS nvarchar(max)
AS
BEGIN
	-- Declare the return variable here
	DECLARE @dynsql nvarchar(max),@dynforinstitle nvarchar(512), @cats integer,@dcount integer
	DECLARE @DAYCOUNT INTEGER
	SET @DAYCOUNT=dbo.GetDayCount(@week,@year)
	DECLARE @CATCOUNT INTEGER
	SET @CATCOUNT=dbo.GetDishCategoriesCount()
	-- Add the T-SQL statements to compute the return value here
	SET @dcount=1
	SELECT @dynsql='CREATE TABLE WeekUserOrdersReport ([Username] nvarchar(128),'
	SELECT @dynforinstitle='INSERT WeekUserOrdersReport'
	WHILE @dcount<=@DAYCOUNT
	BEGIN
		SET @cats=1
		WHILE @cats<=@CATCOUNT
		BEGIN
			SET @dynsql=@dynsql+'[D'+cast(@dcount as nvarchar)+'-C'+cast(@cats as nvarchar)+']'+' float(2),';
			SET @cats=@cats+1;
		END	
		SET @dcount=@dcount+1;
	END
	-- Return the result of the function
	SET @dynsql=LEFT(@dynsql,len(@dynsql)-1)
	SET @dynsql=@dynsql+');'
	SET @dynforinstitle=@dynforinstitle+' VALUES('
	
	DECLARE MY_US_CURSOR_2 CURSOR
	DYNAMIC
	FOR SELECT * from dbo.WeekOrdersByWeekYear(@week,@year)
	
	OPEN MY_US_CURSOR_2
	
    declare @username nvarchar(128)
    declare @weekordid integer 
	declare @dqua float(2)
	
	FETCH NEXT FROM MY_US_CURSOR_2 INTO @username,@weekordid
	
	WHILE @@FETCH_STATUS = 0
	BEGIN
		SET @dynsql=@dynsql + @dynforinstitle
		SET @dynsql=@dynsql + ''''+@username+ ''''+','
				
		DECLARE DISHQUANT_CURSOR_2 CURSOR
		DYNAMIC
		FOR SELECT * from dbo.DishQuantByWeekOrderId(@weekordid)
		OPEN DISHQUANT_CURSOR_2
		
		SET @dcount=1
		WHILE @dcount<=@DAYCOUNT
		BEGIN
			SET @cats=1
			WHILE @cats<=@CATCOUNT
			BEGIN
				
				FETCH NEXT FROM DISHQUANT_CURSOR_2 INTO @dqua
				
				WHILE @@FETCH_STATUS = 0
				BEGIN
						SET @dynsql=@dynsql+STR(@dqua,4,2)+','
						FETCH NEXT FROM DISHQUANT_CURSOR_2 INTO @dqua
				END				
				SET @cats=@cats+1;
			END	
			SET @dcount=@dcount+1;
		END
		SET @dynsql=LEFT(@dynsql,len(@dynsql)-1)
		SET @dynsql=@dynsql+');'
		CLOSE DISHQUANT_CURSOR_2
		DEALLOCATE DISHQUANT_CURSOR_2
		FETCH NEXT FROM MY_US_CURSOR_2 INTO @username,@weekordid
	END;
	CLOSE MY_US_CURSOR_2
	DEALLOCATE MY_US_CURSOR_2
			
	RETURN @dynsql
END
GO

/****** Object:  StoredProcedure [dbo].[UpdateWeekOrdersView]    Script Date: 05/03/2016 17:05:41 ******/

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Вызывает обновление таблицы WeekUserOrdersReport в зависимости от
--				введенного номера недели и года >
-- =============================================
CREATE PROCEDURE "UpdateWeekOrdersView"
	@week integer,
	@year integer
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	IF OBJECT_ID('WeekUserOrdersReport') IS NOT NULL DROP TABLE WeekUserOrdersReport;
    -- Insert statements for procedure here
    declare @dynsql nvarchar(max)
    set @dynsql=dbo.DynSqlWeekOrders(@week,@year)
    exec(@dynsql)
    set @dynsql=dbo.DynSqlSumWeekOrdersByWeekYear(@week,@year)
    exec(@dynsql)
    
    select * from dbo.WeekUserOrdersReport
END
GO
/****** Object:  UserDefinedFunction [dbo].[GetDishCategoriesCount]    Script Date: 05/03/2016 17:29:25 ******/

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Возвращает количество категорий блюд ,>
-- =============================================
CREATE FUNCTION "GetDishCategoriesCount" ()
RETURNS integer
AS
BEGIN
	RETURN (SELECT count(*) from [ACS_Dining].[dbo].[DishType])
END

GO
/****** Object:  UserDefinedFunction [dbo].[DishQuantByWeekOrderId]    Script Date: 05/03/2016 18:05:46 ******/

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Возвращает значения количеств заказанных блюд для указанного недельного заказа>
-- =============================================
CREATE FUNCTION "DishQuantByWeekOrderId" 
(
	@weekordid integer
)
RETURNS @userweekord TABLE 
(
	[DishQuant] float(2)
)
AS
BEGIN
	DECLARE @cats integer, @catcount integer
	set @catcount=dbo.GetDishCategoriesCount()
	DECLARE DAY_CURSOR CURSOR
	SCROLL
	FOR SELECT plandayords.Id ,dayordmenus.Id ,daymenu.ID  FROM [ACS_Dining].[dbo].[DayOrderMenu] AS dayordmenus 
		INNER JOIN [ACS_Dining].[dbo].[WeekOrderMenu] AS WORDMENU
		ON  WORDMENU.[ID]=@weekordid AND dayordmenus.[WeekOrderMenu_Id]=WORDMENU.[ID] 
		inner join [ACS_Dining].[dbo].[MenuForDay] as daymenu
		on daymenu.ID=dayordmenus.MenuForDay_ID
		INNER JOIN [ACS_Dining].[dbo].[WorkingDay] AS WDAYS
		ON WDAYS.Id=daymenu.WorkingDay_Id AND WDAYS.IsWorking=1
		inner join [ACS_Dining].[dbo].[PlannedDayOrderMenu] as plandayords
		on plandayords.DayOrderMenu_Id=dayordmenus.ID
	OPEN DAY_CURSOR
	DECLARE @plandayordid integer
	DECLARE @dayordid integer
	DECLARE @daymenuid integer
	FETCH NEXT FROM DAY_CURSOR INTO @plandayordid,@dayordid,@daymenuid
	
	WHILE @@FETCH_STATUS = 0
	BEGIN
		SET @cats=1
		WHILE @cats<=@catcount
		BEGIN
			INSERT INTO @userweekord VALUES([dbo].[GetDishquantity](@cats,@daymenuid,@dayordid,@plandayordid))
			SET @cats=@cats+1;
		END	
		FETCH NEXT FROM DAY_CURSOR INTO @plandayordid,@dayordid,@daymenuid
	END
	CLOSE DAY_CURSOR
	DEALLOCATE DAY_CURSOR
	RETURN 
END
GO

/****** Object:  StoredProcedure [dbo].[GetSumDishCounts]    Script Date: 05/03/2016 23:16:04 ******/
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Возвращает суммарные количества блюд на неделе>
-- =============================================
CREATE PROCEDURE "GetSumDishCounts"
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
    
    select * from dbo.SumWeekUnitReport
END
GO

/****** Object:  UserDefinedFunction [dbo].[DynSqlSumWeekOrdersByWeekYear]    Script Date: 05/03/2016 23:23:40 ******/
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Возвращает sql для обновления таблицы SumWeekUnitReport 
--	значениями суммарных количеств блюд за указанную неделю>
-- =============================================
CREATE FUNCTION "DynSqlSumWeekOrdersByWeekYear"
(
	@week integer,
	@year integer
)
RETURNS nvarchar(max)
AS
BEGIN
	-- Declare the return variable here
	DECLARE @dynsql nvarchar(max), @cats integer,@dcount integer
	DECLARE @DAYCOUNT INTEGER
	SET @DAYCOUNT=dbo.GetDayCount(@week,@year)
	DECLARE @CATCOUNT INTEGER
	SET @CATCOUNT=dbo.GetDishCategoriesCount()
	-- Add the T-SQL statements to compute the return value here
	SET @dcount=1
	SET @dynsql='IF OBJECT_ID(''SumWeekUnitReport'') IS NOT NULL DROP TABLE SumWeekUnitReport;CREATE TABLE SumWeekUnitReport ([SumQuant] float(2));  DECLARE @VT FLOAT(2);'
			
		SET @dcount=1
		WHILE @dcount<=@DAYCOUNT
		BEGIN
			SET @cats=1
			WHILE @cats<=@CATCOUNT
			BEGIN
					SET @dynsql=@dynsql+' SET @VT=(SELECT SUM(P.[D'+cast(@dcount as nvarchar)+'-C'+cast(@cats as nvarchar)+']) FROM [ACS_Dining].[dbo].[WeekUserOrdersReport] AS P  GROUP BY ());'
					SET @dynsql=@dynsql+' INSERT SumWeekUnitReport VALUES(@VT);'
				SET @cats=@cats+1;
			END	
			SET @dcount=@dcount+1;
		END
		
	RETURN @dynsql
END
GO
USE "ACS_Dining"
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TYPE dbo.QuantArray
AS TABLE
(
  quant float
);
GO
CREATE TYPE dbo.DayOrdArray
AS TABLE
(
  dayord int
);
GO
USE "ACS_Dining"
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW "dbo"."EmployeeUsers"
AS
SELECT     TOP (100) PERCENT USERS.UserName, USERS.FirstName, USERS.LastName, USERS.LastLoginTime, USERS.Balance, USERS.RegistrationDate, USERS.CanMakeBooking, USERS.IsExisting, 
                      USERS.Email, USERS.PhoneNumber, USERS.PhoneNumberConfirmed
FROM         dbo.AspNetUsers AS USERS INNER JOIN
                      dbo.AspNetUserRoles AS USROLES ON USROLES.UserId = USERS.Id INNER JOIN
                      dbo.AspNetRoles AS ROLES ON ROLES.Id = USROLES.RoleId AND ROLES.Name = 'Employee'
ORDER BY USERS.UserName
GO
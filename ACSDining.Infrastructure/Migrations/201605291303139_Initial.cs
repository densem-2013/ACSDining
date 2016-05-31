namespace ACSDining.Infrastructure.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DayOrderMenu",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DayOrderSummaryPrice = c.Double(nullable: false),
                        MenuForDay_ID = c.Int(),
                        WeekOrderMenu_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MenuForDay", t => t.MenuForDay_ID)
                .ForeignKey("dbo.WeekOrderMenu", t => t.WeekOrderMenu_Id)
                .Index(t => t.MenuForDay_ID)
                .Index(t => t.WeekOrderMenu_Id);
            
            CreateTable(
                "dbo.MenuForDay",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        TotalPrice = c.Double(nullable: false),
                        DayMenuCanBeChanged = c.Boolean(nullable: false),
                        OrderCanBeChanged = c.Boolean(nullable: false),
                        MenuForWeek_ID = c.Int(),
                        WorkingDay_Id = c.Int(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.MenuForWeek", t => t.MenuForWeek_ID)
                .ForeignKey("dbo.WorkingDay", t => t.WorkingDay_Id)
                .Index(t => t.MenuForWeek_ID)
                .Index(t => t.WorkingDay_Id);
            
            CreateTable(
                "dbo.MfdDishPriceRelations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DishPriceId = c.Int(nullable: false),
                        DishId = c.Int(nullable: false),
                        MenuForDayId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Dish", t => t.DishId, cascadeDelete: true)
                .ForeignKey("dbo.DishPrice", t => t.DishPriceId, cascadeDelete: true)
                .ForeignKey("dbo.MenuForDay", t => t.MenuForDayId, cascadeDelete: true)
                .Index(t => t.DishPriceId)
                .Index(t => t.DishId)
                .Index(t => t.MenuForDayId);
            
            CreateTable(
                "dbo.Dish",
                c => new
                    {
                        DishID = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        Description = c.String(),
                        ProductImage = c.String(),
                        Deleted = c.Boolean(nullable: false),
                        CurrentPrice_Id = c.Int(),
                        DishDetail_ID = c.Int(),
                        DishType_Id = c.Int(),
                    })
                .PrimaryKey(t => t.DishID)
                .ForeignKey("dbo.DishPrice", t => t.CurrentPrice_Id)
                .ForeignKey("dbo.DishDetail", t => t.DishDetail_ID)
                .ForeignKey("dbo.DishType", t => t.DishType_Id)
                .Index(t => t.CurrentPrice_Id)
                .Index(t => t.DishDetail_ID)
                .Index(t => t.DishType_Id);
            
            CreateTable(
                "dbo.DishPrice",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Price = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.DishDetail",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        Recept = c.String(unicode: false, storeType: "text"),
                        Foods = c.String(unicode: false, storeType: "text"),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.DishType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Category = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.PlannedDayOrderMenu",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DayOrderSummaryPrice = c.Double(nullable: false),
                        MenuForDay_ID = c.Int(),
                        PlannedWeekOrderMenu_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MenuForDay", t => t.MenuForDay_ID)
                .ForeignKey("dbo.PlannedWeekOrderMenu", t => t.PlannedWeekOrderMenu_Id)
                .Index(t => t.MenuForDay_ID)
                .Index(t => t.PlannedWeekOrderMenu_Id);
            
            CreateTable(
                "dbo.PlannedWeekOrderMenu",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        WeekOrderSummaryPrice = c.Double(nullable: false),
                        MenuForWeek_ID = c.Int(),
                        User_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MenuForWeek", t => t.MenuForWeek_ID)
                .ForeignKey("dbo.AspNetUsers", t => t.User_Id)
                .Index(t => t.MenuForWeek_ID)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.MenuForWeek",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        SummaryPrice = c.Double(nullable: false),
                        SUCanChangeOrder = c.Boolean(nullable: false),
                        OrderCanBeCreated = c.Boolean(nullable: false),
                        WorkingDaysAreSelected = c.Boolean(nullable: false),
                        WorkingWeek_ID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.WorkingWeek", t => t.WorkingWeek_ID, cascadeDelete: true)
                .Index(t => t.WorkingWeek_ID);
            
            CreateTable(
                "dbo.WeekOrderMenu",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        WeekOrderSummaryPrice = c.Double(nullable: false),
                        MenuForWeek_ID = c.Int(),
                        User_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MenuForWeek", t => t.MenuForWeek_ID)
                .ForeignKey("dbo.AspNetUsers", t => t.User_Id)
                .Index(t => t.MenuForWeek_ID)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        FirstName = c.String(nullable: false),
                        LastName = c.String(nullable: false),
                        LastLoginTime = c.DateTime(nullable: false),
                        Balance = c.Double(nullable: false),
                        AllowableDebt = c.Double(nullable: false),
                        RegistrationDate = c.DateTime(nullable: false),
                        CanMakeBooking = c.Boolean(nullable: false),
                        IsExisting = c.Boolean(nullable: false),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.WorkingWeek",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        WeekNumber = c.Int(nullable: false),
                        CanBeChanged = c.Boolean(nullable: false),
                        Year_Id = c.Int(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Year", t => t.Year_Id)
                .Index(t => t.Year_Id);
            
            CreateTable(
                "dbo.WorkingDay",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsWorking = c.Boolean(nullable: false),
                        DayOfWeek_Id = c.Int(),
                        WorkingWeek_ID = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.DayOfWeek", t => t.DayOfWeek_Id)
                .ForeignKey("dbo.WorkingWeek", t => t.WorkingWeek_ID)
                .Index(t => t.DayOfWeek_Id)
                .Index(t => t.WorkingWeek_ID);
            
            CreateTable(
                "dbo.DayOfWeek",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Year",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        YearNumber = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.DishQuantity",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Quantity = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.DishQuantityRelations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DishQuantityId = c.Int(nullable: false),
                        DishTypeId = c.Int(nullable: false),
                        DayOrderMenuId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.DayOrderMenu", t => t.DayOrderMenuId, cascadeDelete: true)
                .ForeignKey("dbo.DishQuantity", t => t.DishQuantityId, cascadeDelete: true)
                .ForeignKey("dbo.DishType", t => t.DishTypeId, cascadeDelete: true)
                .Index(t => t.DishQuantityId)
                .Index(t => t.DishTypeId)
                .Index(t => t.DayOrderMenuId);
            
            CreateTable(
                "dbo.FoodCategory",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        MeasureUnit = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Food",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        Category_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.FoodCategory", t => t.Category_Id)
                .Index(t => t.Category_Id);
            
            CreateTable(
                "dbo.FoodQuantity",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Quantity = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.FoodQuantityRelations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FoodID = c.Int(nullable: false),
                        FoodQuantityID = c.Int(nullable: false),
                        DishID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Dish", t => t.DishID, cascadeDelete: true)
                .ForeignKey("dbo.Food", t => t.FoodID, cascadeDelete: true)
                .ForeignKey("dbo.FoodQuantity", t => t.FoodQuantityID, cascadeDelete: true)
                .Index(t => t.FoodID)
                .Index(t => t.FoodQuantityID)
                .Index(t => t.DishID);
            
            CreateTable(
                "dbo.PlanDishQuantityRelations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DishQuantityId = c.Int(nullable: false),
                        DishTypeId = c.Int(nullable: false),
                        PlannedDayOrderMenuId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.DishQuantity", t => t.DishQuantityId, cascadeDelete: true)
                .ForeignKey("dbo.DishType", t => t.DishTypeId, cascadeDelete: true)
                .ForeignKey("dbo.PlannedDayOrderMenu", t => t.PlannedDayOrderMenuId, cascadeDelete: true)
                .Index(t => t.DishQuantityId)
                .Index(t => t.DishTypeId)
                .Index(t => t.PlannedDayOrderMenuId);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                        Description = c.String(),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.WeekPaiment",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Note = c.String(),
                        Paiment = c.Double(nullable: false),
                        WeekIsPaid = c.Boolean(nullable: false),
                        PreviousWeekBalance = c.Double(nullable: false),
                        WeekOrderMenu_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.WeekOrderMenu", t => t.WeekOrderMenu_Id, cascadeDelete: true)
                .Index(t => t.WeekOrderMenu_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.WeekPaiment", "WeekOrderMenu_Id", "dbo.WeekOrderMenu");
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.PlanDishQuantityRelations", "PlannedDayOrderMenuId", "dbo.PlannedDayOrderMenu");
            DropForeignKey("dbo.PlanDishQuantityRelations", "DishTypeId", "dbo.DishType");
            DropForeignKey("dbo.PlanDishQuantityRelations", "DishQuantityId", "dbo.DishQuantity");
            DropForeignKey("dbo.FoodQuantityRelations", "FoodQuantityID", "dbo.FoodQuantity");
            DropForeignKey("dbo.FoodQuantityRelations", "FoodID", "dbo.Food");
            DropForeignKey("dbo.FoodQuantityRelations", "DishID", "dbo.Dish");
            DropForeignKey("dbo.Food", "Category_Id", "dbo.FoodCategory");
            DropForeignKey("dbo.DishQuantityRelations", "DishTypeId", "dbo.DishType");
            DropForeignKey("dbo.DishQuantityRelations", "DishQuantityId", "dbo.DishQuantity");
            DropForeignKey("dbo.DishQuantityRelations", "DayOrderMenuId", "dbo.DayOrderMenu");
            DropForeignKey("dbo.MenuForDay", "WorkingDay_Id", "dbo.WorkingDay");
            DropForeignKey("dbo.PlannedWeekOrderMenu", "User_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.PlannedDayOrderMenu", "PlannedWeekOrderMenu_Id", "dbo.PlannedWeekOrderMenu");
            DropForeignKey("dbo.PlannedWeekOrderMenu", "MenuForWeek_ID", "dbo.MenuForWeek");
            DropForeignKey("dbo.MenuForWeek", "WorkingWeek_ID", "dbo.WorkingWeek");
            DropForeignKey("dbo.WorkingWeek", "Year_Id", "dbo.Year");
            DropForeignKey("dbo.WorkingDay", "WorkingWeek_ID", "dbo.WorkingWeek");
            DropForeignKey("dbo.WorkingDay", "DayOfWeek_Id", "dbo.DayOfWeek");
            DropForeignKey("dbo.WeekOrderMenu", "User_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.WeekOrderMenu", "MenuForWeek_ID", "dbo.MenuForWeek");
            DropForeignKey("dbo.DayOrderMenu", "WeekOrderMenu_Id", "dbo.WeekOrderMenu");
            DropForeignKey("dbo.MenuForDay", "MenuForWeek_ID", "dbo.MenuForWeek");
            DropForeignKey("dbo.PlannedDayOrderMenu", "MenuForDay_ID", "dbo.MenuForDay");
            DropForeignKey("dbo.MfdDishPriceRelations", "MenuForDayId", "dbo.MenuForDay");
            DropForeignKey("dbo.MfdDishPriceRelations", "DishPriceId", "dbo.DishPrice");
            DropForeignKey("dbo.MfdDishPriceRelations", "DishId", "dbo.Dish");
            DropForeignKey("dbo.Dish", "DishType_Id", "dbo.DishType");
            DropForeignKey("dbo.Dish", "DishDetail_ID", "dbo.DishDetail");
            DropForeignKey("dbo.Dish", "CurrentPrice_Id", "dbo.DishPrice");
            DropForeignKey("dbo.DayOrderMenu", "MenuForDay_ID", "dbo.MenuForDay");
            DropIndex("dbo.WeekPaiment", new[] { "WeekOrderMenu_Id" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.PlanDishQuantityRelations", new[] { "PlannedDayOrderMenuId" });
            DropIndex("dbo.PlanDishQuantityRelations", new[] { "DishTypeId" });
            DropIndex("dbo.PlanDishQuantityRelations", new[] { "DishQuantityId" });
            DropIndex("dbo.FoodQuantityRelations", new[] { "DishID" });
            DropIndex("dbo.FoodQuantityRelations", new[] { "FoodQuantityID" });
            DropIndex("dbo.FoodQuantityRelations", new[] { "FoodID" });
            DropIndex("dbo.Food", new[] { "Category_Id" });
            DropIndex("dbo.DishQuantityRelations", new[] { "DayOrderMenuId" });
            DropIndex("dbo.DishQuantityRelations", new[] { "DishTypeId" });
            DropIndex("dbo.DishQuantityRelations", new[] { "DishQuantityId" });
            DropIndex("dbo.WorkingDay", new[] { "WorkingWeek_ID" });
            DropIndex("dbo.WorkingDay", new[] { "DayOfWeek_Id" });
            DropIndex("dbo.WorkingWeek", new[] { "Year_Id" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.WeekOrderMenu", new[] { "User_Id" });
            DropIndex("dbo.WeekOrderMenu", new[] { "MenuForWeek_ID" });
            DropIndex("dbo.MenuForWeek", new[] { "WorkingWeek_ID" });
            DropIndex("dbo.PlannedWeekOrderMenu", new[] { "User_Id" });
            DropIndex("dbo.PlannedWeekOrderMenu", new[] { "MenuForWeek_ID" });
            DropIndex("dbo.PlannedDayOrderMenu", new[] { "PlannedWeekOrderMenu_Id" });
            DropIndex("dbo.PlannedDayOrderMenu", new[] { "MenuForDay_ID" });
            DropIndex("dbo.Dish", new[] { "DishType_Id" });
            DropIndex("dbo.Dish", new[] { "DishDetail_ID" });
            DropIndex("dbo.Dish", new[] { "CurrentPrice_Id" });
            DropIndex("dbo.MfdDishPriceRelations", new[] { "MenuForDayId" });
            DropIndex("dbo.MfdDishPriceRelations", new[] { "DishId" });
            DropIndex("dbo.MfdDishPriceRelations", new[] { "DishPriceId" });
            DropIndex("dbo.MenuForDay", new[] { "WorkingDay_Id" });
            DropIndex("dbo.MenuForDay", new[] { "MenuForWeek_ID" });
            DropIndex("dbo.DayOrderMenu", new[] { "WeekOrderMenu_Id" });
            DropIndex("dbo.DayOrderMenu", new[] { "MenuForDay_ID" });
            DropTable("dbo.WeekPaiment");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.PlanDishQuantityRelations");
            DropTable("dbo.FoodQuantityRelations");
            DropTable("dbo.FoodQuantity");
            DropTable("dbo.Food");
            DropTable("dbo.FoodCategory");
            DropTable("dbo.DishQuantityRelations");
            DropTable("dbo.DishQuantity");
            DropTable("dbo.Year");
            DropTable("dbo.DayOfWeek");
            DropTable("dbo.WorkingDay");
            DropTable("dbo.WorkingWeek");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.WeekOrderMenu");
            DropTable("dbo.MenuForWeek");
            DropTable("dbo.PlannedWeekOrderMenu");
            DropTable("dbo.PlannedDayOrderMenu");
            DropTable("dbo.DishType");
            DropTable("dbo.DishDetail");
            DropTable("dbo.DishPrice");
            DropTable("dbo.Dish");
            DropTable("dbo.MfdDishPriceRelations");
            DropTable("dbo.MenuForDay");
            DropTable("dbo.DayOrderMenu");
        }
    }
}

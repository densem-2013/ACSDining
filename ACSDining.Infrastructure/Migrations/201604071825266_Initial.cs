namespace ACSDining.Web.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DayOfWeek",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
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
                "dbo.Dish",
                c => new
                    {
                        DishID = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        Description = c.String(),
                        ProductImage = c.String(),
                        Price = c.Double(nullable: false),
                        DishDetail_ID = c.Int(),
                        DishType_Id = c.Int(),
                    })
                .PrimaryKey(t => t.DishID)
                .ForeignKey("dbo.DishDetail", t => t.DishDetail_ID)
                .ForeignKey("dbo.DishType", t => t.DishType_Id)
                .Index(t => t.DishDetail_ID)
                .Index(t => t.DishType_Id);
            
            CreateTable(
                "dbo.DishType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Category = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.MenuForDay",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        TotalPrice = c.Double(nullable: false),
                        WorkingDay_ID = c.Int(),
                        WorkingWeek_ID = c.Int(),
                        MenuForWeek_ID = c.Int(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.WorkingDay", t => t.WorkingDay_ID)
                .ForeignKey("dbo.WorkingWeek", t => t.WorkingWeek_ID)
                .ForeignKey("dbo.MenuForWeek", t => t.MenuForWeek_ID)
                .Index(t => t.WorkingDay_ID)
                .Index(t => t.WorkingWeek_ID)
                .Index(t => t.MenuForWeek_ID);
            
            CreateTable(
                "dbo.WorkingDay",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        IsWorking = c.Boolean(nullable: false),
                        DayOfWeek_ID = c.Int(),
                        WorkingWeek_ID = c.Int(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.DayOfWeek", t => t.DayOfWeek_ID)
                .ForeignKey("dbo.WorkingWeek", t => t.WorkingWeek_ID)
                .Index(t => t.DayOfWeek_ID)
                .Index(t => t.WorkingWeek_ID);
            
            CreateTable(
                "dbo.WorkingWeek",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        WeekNumber = c.Int(nullable: false),
                        Year_Id = c.Int(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Year", t => t.Year_Id)
                .Index(t => t.Year_Id);
            
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
                        DishQuantityRelationsID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.DishQuantityRelations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DishQuantityID = c.Int(nullable: false),
                        PlannedOrderMenuID = c.Int(nullable: false),
                        DishTypeID = c.Int(nullable: false),
                        WorkDayID = c.Int(nullable: false),
                        OrderMenuID = c.Int(nullable: false),
                        MenuForWeekID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.DishQuantity", t => t.DishQuantityID, cascadeDelete: true)
                .ForeignKey("dbo.DishType", t => t.DishTypeID, cascadeDelete: true)
                .ForeignKey("dbo.MenuForWeek", t => t.MenuForWeekID, cascadeDelete: true)
                .ForeignKey("dbo.OrderMenu", t => t.OrderMenuID, cascadeDelete: true)
                .ForeignKey("dbo.PlannedOrderMenu", t => t.PlannedOrderMenuID, cascadeDelete: true)
                .ForeignKey("dbo.WorkingDay", t => t.WorkDayID, cascadeDelete: true)
                .Index(t => t.DishQuantityID)
                .Index(t => t.PlannedOrderMenuID)
                .Index(t => t.DishTypeID)
                .Index(t => t.WorkDayID)
                .Index(t => t.OrderMenuID)
                .Index(t => t.MenuForWeekID);
            
            CreateTable(
                "dbo.MenuForWeek",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        SummaryPrice = c.Double(nullable: false),
                        WorkingWeek_ID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.WorkingWeek", t => t.WorkingWeek_ID, cascadeDelete: true)
                .Index(t => t.WorkingWeek_ID);
            
            CreateTable(
                "dbo.OrderMenu",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        WeekPaid = c.Double(nullable: false),
                        Balance = c.Double(nullable: false),
                        OrderSummaryPrice = c.Double(nullable: false),
                        Note = c.String(),
                        MenuForWeek_ID = c.Int(),
                        User_Id = c.String(maxLength: 128),
                        PlannedOrderMenu_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MenuForWeek", t => t.MenuForWeek_ID)
                .ForeignKey("dbo.AspNetUsers", t => t.User_Id)
                .ForeignKey("dbo.PlannedOrderMenu", t => t.PlannedOrderMenu_Id)
                .Index(t => t.MenuForWeek_ID)
                .Index(t => t.User_Id)
                .Index(t => t.PlannedOrderMenu_Id);
            
            CreateTable(
                "dbo.PlannedOrderMenu",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
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
                        RegistrationDate = c.DateTime(nullable: false),
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
                "dbo.MFD_Dishes",
                c => new
                    {
                        MenuID = c.Int(nullable: false),
                        DishID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.MenuID, t.DishID })
                .ForeignKey("dbo.MenuForDay", t => t.MenuID, cascadeDelete: true)
                .ForeignKey("dbo.Dish", t => t.DishID, cascadeDelete: true)
                .Index(t => t.MenuID)
                .Index(t => t.DishID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.FoodQuantityRelations", "FoodQuantityID", "dbo.FoodQuantity");
            DropForeignKey("dbo.FoodQuantityRelations", "FoodID", "dbo.Food");
            DropForeignKey("dbo.FoodQuantityRelations", "DishID", "dbo.Dish");
            DropForeignKey("dbo.Food", "Category_Id", "dbo.FoodCategory");
            DropForeignKey("dbo.DishQuantityRelations", "WorkDayID", "dbo.WorkingDay");
            DropForeignKey("dbo.DishQuantityRelations", "PlannedOrderMenuID", "dbo.PlannedOrderMenu");
            DropForeignKey("dbo.DishQuantityRelations", "OrderMenuID", "dbo.OrderMenu");
            DropForeignKey("dbo.DishQuantityRelations", "MenuForWeekID", "dbo.MenuForWeek");
            DropForeignKey("dbo.MenuForWeek", "WorkingWeek_ID", "dbo.WorkingWeek");
            DropForeignKey("dbo.OrderMenu", "PlannedOrderMenu_Id", "dbo.PlannedOrderMenu");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.PlannedOrderMenu", "User_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.OrderMenu", "User_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.PlannedOrderMenu", "MenuForWeek_ID", "dbo.MenuForWeek");
            DropForeignKey("dbo.OrderMenu", "MenuForWeek_ID", "dbo.MenuForWeek");
            DropForeignKey("dbo.MenuForDay", "MenuForWeek_ID", "dbo.MenuForWeek");
            DropForeignKey("dbo.DishQuantityRelations", "DishTypeID", "dbo.DishType");
            DropForeignKey("dbo.DishQuantityRelations", "DishQuantityID", "dbo.DishQuantity");
            DropForeignKey("dbo.MenuForDay", "WorkingWeek_ID", "dbo.WorkingWeek");
            DropForeignKey("dbo.MenuForDay", "WorkingDay_ID", "dbo.WorkingDay");
            DropForeignKey("dbo.WorkingWeek", "Year_Id", "dbo.Year");
            DropForeignKey("dbo.WorkingDay", "WorkingWeek_ID", "dbo.WorkingWeek");
            DropForeignKey("dbo.WorkingDay", "DayOfWeek_ID", "dbo.DayOfWeek");
            DropForeignKey("dbo.MFD_Dishes", "DishID", "dbo.Dish");
            DropForeignKey("dbo.MFD_Dishes", "MenuID", "dbo.MenuForDay");
            DropForeignKey("dbo.Dish", "DishType_Id", "dbo.DishType");
            DropForeignKey("dbo.Dish", "DishDetail_ID", "dbo.DishDetail");
            DropIndex("dbo.MFD_Dishes", new[] { "DishID" });
            DropIndex("dbo.MFD_Dishes", new[] { "MenuID" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.FoodQuantityRelations", new[] { "DishID" });
            DropIndex("dbo.FoodQuantityRelations", new[] { "FoodQuantityID" });
            DropIndex("dbo.FoodQuantityRelations", new[] { "FoodID" });
            DropIndex("dbo.Food", new[] { "Category_Id" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.PlannedOrderMenu", new[] { "User_Id" });
            DropIndex("dbo.PlannedOrderMenu", new[] { "MenuForWeek_ID" });
            DropIndex("dbo.OrderMenu", new[] { "PlannedOrderMenu_Id" });
            DropIndex("dbo.OrderMenu", new[] { "User_Id" });
            DropIndex("dbo.OrderMenu", new[] { "MenuForWeek_ID" });
            DropIndex("dbo.MenuForWeek", new[] { "WorkingWeek_ID" });
            DropIndex("dbo.DishQuantityRelations", new[] { "MenuForWeekID" });
            DropIndex("dbo.DishQuantityRelations", new[] { "OrderMenuID" });
            DropIndex("dbo.DishQuantityRelations", new[] { "WorkDayID" });
            DropIndex("dbo.DishQuantityRelations", new[] { "DishTypeID" });
            DropIndex("dbo.DishQuantityRelations", new[] { "PlannedOrderMenuID" });
            DropIndex("dbo.DishQuantityRelations", new[] { "DishQuantityID" });
            DropIndex("dbo.WorkingWeek", new[] { "Year_Id" });
            DropIndex("dbo.WorkingDay", new[] { "WorkingWeek_ID" });
            DropIndex("dbo.WorkingDay", new[] { "DayOfWeek_ID" });
            DropIndex("dbo.MenuForDay", new[] { "MenuForWeek_ID" });
            DropIndex("dbo.MenuForDay", new[] { "WorkingWeek_ID" });
            DropIndex("dbo.MenuForDay", new[] { "WorkingDay_ID" });
            DropIndex("dbo.Dish", new[] { "DishType_Id" });
            DropIndex("dbo.Dish", new[] { "DishDetail_ID" });
            DropTable("dbo.MFD_Dishes");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.FoodQuantityRelations");
            DropTable("dbo.FoodQuantity");
            DropTable("dbo.Food");
            DropTable("dbo.FoodCategory");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.PlannedOrderMenu");
            DropTable("dbo.OrderMenu");
            DropTable("dbo.MenuForWeek");
            DropTable("dbo.DishQuantityRelations");
            DropTable("dbo.DishQuantity");
            DropTable("dbo.Year");
            DropTable("dbo.WorkingWeek");
            DropTable("dbo.WorkingDay");
            DropTable("dbo.MenuForDay");
            DropTable("dbo.DishType");
            DropTable("dbo.Dish");
            DropTable("dbo.DishDetail");
            DropTable("dbo.DayOfWeek");
        }
    }
}

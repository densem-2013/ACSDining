namespace ACSDining.Core.Migrations
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
                "dbo.DishQuantity",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Quantity = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.DishType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Category = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.MenuForWeek",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        WeekNumber = c.Int(nullable: false),
                        SummaryPrice = c.Double(nullable: false),
                        Year_Id = c.Int(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Year", t => t.Year_Id)
                .Index(t => t.Year_Id);
            
            CreateTable(
                "dbo.MenuForDay",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        TotalPrice = c.Double(nullable: false),
                        DayOfWeek_ID = c.Int(),
                        MenuForWeek_ID = c.Int(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.DayOfWeek", t => t.DayOfWeek_ID)
                .ForeignKey("dbo.MenuForWeek", t => t.MenuForWeek_ID)
                .Index(t => t.DayOfWeek_ID)
                .Index(t => t.MenuForWeek_ID);
            
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
                "dbo.OrderMenu",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        WeekPaid = c.Double(nullable: false),
                        Balance = c.Double(nullable: false),
                        SummaryPrice = c.Double(nullable: false),
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
                        IsDiningRoomClient = c.Boolean(nullable: false),
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
                "dbo.Year",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        YearNumber = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
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
                "dbo.DishQuantityDayOfWeek",
                c => new
                    {
                        DishQuantity_Id = c.Int(nullable: false),
                        DayOfWeek_ID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.DishQuantity_Id, t.DayOfWeek_ID })
                .ForeignKey("dbo.DishQuantity", t => t.DishQuantity_Id, cascadeDelete: true)
                .ForeignKey("dbo.DayOfWeek", t => t.DayOfWeek_ID, cascadeDelete: true)
                .Index(t => t.DishQuantity_Id)
                .Index(t => t.DayOfWeek_ID);
            
            CreateTable(
                "dbo.DishTypeDishQuantity",
                c => new
                    {
                        DishType_Id = c.Int(nullable: false),
                        DishQuantity_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.DishType_Id, t.DishQuantity_Id })
                .ForeignKey("dbo.DishType", t => t.DishType_Id, cascadeDelete: true)
                .ForeignKey("dbo.DishQuantity", t => t.DishQuantity_Id, cascadeDelete: true)
                .Index(t => t.DishType_Id)
                .Index(t => t.DishQuantity_Id);
            
            CreateTable(
                "dbo.MenuForWeekDishQuantity",
                c => new
                    {
                        MenuForWeek_ID = c.Int(nullable: false),
                        DishQuantity_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.MenuForWeek_ID, t.DishQuantity_Id })
                .ForeignKey("dbo.MenuForWeek", t => t.MenuForWeek_ID, cascadeDelete: true)
                .ForeignKey("dbo.DishQuantity", t => t.DishQuantity_Id, cascadeDelete: true)
                .Index(t => t.MenuForWeek_ID)
                .Index(t => t.DishQuantity_Id);
            
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
            
            CreateTable(
                "dbo.OrderMenuDishQuantity",
                c => new
                    {
                        OrderMenu_Id = c.Int(nullable: false),
                        DishQuantity_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.OrderMenu_Id, t.DishQuantity_Id })
                .ForeignKey("dbo.OrderMenu", t => t.OrderMenu_Id, cascadeDelete: true)
                .ForeignKey("dbo.DishQuantity", t => t.DishQuantity_Id, cascadeDelete: true)
                .Index(t => t.OrderMenu_Id)
                .Index(t => t.DishQuantity_Id);
            
            CreateTable(
                "dbo.PlannedOrderMenuDishQuantity",
                c => new
                    {
                        PlannedOrderMenu_Id = c.Int(nullable: false),
                        DishQuantity_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.PlannedOrderMenu_Id, t.DishQuantity_Id })
                .ForeignKey("dbo.PlannedOrderMenu", t => t.PlannedOrderMenu_Id, cascadeDelete: true)
                .ForeignKey("dbo.DishQuantity", t => t.DishQuantity_Id, cascadeDelete: true)
                .Index(t => t.PlannedOrderMenu_Id)
                .Index(t => t.DishQuantity_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.MenuForWeek", "Year_Id", "dbo.Year");
            DropForeignKey("dbo.OrderMenu", "PlannedOrderMenu_Id", "dbo.PlannedOrderMenu");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.PlannedOrderMenu", "User_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.OrderMenu", "User_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.PlannedOrderMenu", "MenuForWeek_ID", "dbo.MenuForWeek");
            DropForeignKey("dbo.PlannedOrderMenuDishQuantity", "DishQuantity_Id", "dbo.DishQuantity");
            DropForeignKey("dbo.PlannedOrderMenuDishQuantity", "PlannedOrderMenu_Id", "dbo.PlannedOrderMenu");
            DropForeignKey("dbo.OrderMenu", "MenuForWeek_ID", "dbo.MenuForWeek");
            DropForeignKey("dbo.OrderMenuDishQuantity", "DishQuantity_Id", "dbo.DishQuantity");
            DropForeignKey("dbo.OrderMenuDishQuantity", "OrderMenu_Id", "dbo.OrderMenu");
            DropForeignKey("dbo.MenuForDay", "MenuForWeek_ID", "dbo.MenuForWeek");
            DropForeignKey("dbo.MFD_Dishes", "DishID", "dbo.Dish");
            DropForeignKey("dbo.MFD_Dishes", "MenuID", "dbo.MenuForDay");
            DropForeignKey("dbo.Dish", "DishType_Id", "dbo.DishType");
            DropForeignKey("dbo.Dish", "DishDetail_ID", "dbo.DishDetail");
            DropForeignKey("dbo.MenuForDay", "DayOfWeek_ID", "dbo.DayOfWeek");
            DropForeignKey("dbo.MenuForWeekDishQuantity", "DishQuantity_Id", "dbo.DishQuantity");
            DropForeignKey("dbo.MenuForWeekDishQuantity", "MenuForWeek_ID", "dbo.MenuForWeek");
            DropForeignKey("dbo.DishTypeDishQuantity", "DishQuantity_Id", "dbo.DishQuantity");
            DropForeignKey("dbo.DishTypeDishQuantity", "DishType_Id", "dbo.DishType");
            DropForeignKey("dbo.DishQuantityDayOfWeek", "DayOfWeek_ID", "dbo.DayOfWeek");
            DropForeignKey("dbo.DishQuantityDayOfWeek", "DishQuantity_Id", "dbo.DishQuantity");
            DropIndex("dbo.PlannedOrderMenuDishQuantity", new[] { "DishQuantity_Id" });
            DropIndex("dbo.PlannedOrderMenuDishQuantity", new[] { "PlannedOrderMenu_Id" });
            DropIndex("dbo.OrderMenuDishQuantity", new[] { "DishQuantity_Id" });
            DropIndex("dbo.OrderMenuDishQuantity", new[] { "OrderMenu_Id" });
            DropIndex("dbo.MFD_Dishes", new[] { "DishID" });
            DropIndex("dbo.MFD_Dishes", new[] { "MenuID" });
            DropIndex("dbo.MenuForWeekDishQuantity", new[] { "DishQuantity_Id" });
            DropIndex("dbo.MenuForWeekDishQuantity", new[] { "MenuForWeek_ID" });
            DropIndex("dbo.DishTypeDishQuantity", new[] { "DishQuantity_Id" });
            DropIndex("dbo.DishTypeDishQuantity", new[] { "DishType_Id" });
            DropIndex("dbo.DishQuantityDayOfWeek", new[] { "DayOfWeek_ID" });
            DropIndex("dbo.DishQuantityDayOfWeek", new[] { "DishQuantity_Id" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
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
            DropIndex("dbo.Dish", new[] { "DishType_Id" });
            DropIndex("dbo.Dish", new[] { "DishDetail_ID" });
            DropIndex("dbo.MenuForDay", new[] { "MenuForWeek_ID" });
            DropIndex("dbo.MenuForDay", new[] { "DayOfWeek_ID" });
            DropIndex("dbo.MenuForWeek", new[] { "Year_Id" });
            DropTable("dbo.PlannedOrderMenuDishQuantity");
            DropTable("dbo.OrderMenuDishQuantity");
            DropTable("dbo.MFD_Dishes");
            DropTable("dbo.MenuForWeekDishQuantity");
            DropTable("dbo.DishTypeDishQuantity");
            DropTable("dbo.DishQuantityDayOfWeek");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.Year");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.PlannedOrderMenu");
            DropTable("dbo.OrderMenu");
            DropTable("dbo.DishDetail");
            DropTable("dbo.Dish");
            DropTable("dbo.MenuForDay");
            DropTable("dbo.MenuForWeek");
            DropTable("dbo.DishType");
            DropTable("dbo.DishQuantity");
            DropTable("dbo.DayOfWeek");
        }
    }
}

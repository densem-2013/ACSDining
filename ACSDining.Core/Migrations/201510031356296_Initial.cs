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
                "dbo.DishQuantity",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Quantity = c.String(nullable: false),
                        Dish_DishID = c.Int(),
                        MenuForDay_ID = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Dish", t => t.Dish_DishID)
                .ForeignKey("dbo.MenuForDay", t => t.MenuForDay_ID)
                .Index(t => t.Dish_DishID)
                .Index(t => t.MenuForDay_ID);
            
            CreateTable(
                "dbo.MenuForDay",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        TotalPrice = c.Double(nullable: false),
                        IsEditing = c.Boolean(nullable: false),
                        DayOfWeek_ID = c.Int(),
                        MenuForWeek_ID = c.Int(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.DayOfWeek", t => t.DayOfWeek_ID)
                .ForeignKey("dbo.MenuForWeek", t => t.MenuForWeek_ID)
                .Index(t => t.DayOfWeek_ID)
                .Index(t => t.MenuForWeek_ID);
            
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
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.OrderMenu",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CurrentWeekIsPaid = c.Boolean(nullable: false),
                        WeekNumber = c.Int(nullable: false),
                        CurrentWeek_ID = c.Int(),
                        NextWeek_ID = c.Int(),
                        User_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MenuForWeek", t => t.CurrentWeek_ID)
                .ForeignKey("dbo.MenuForWeek", t => t.NextWeek_ID)
                .ForeignKey("dbo.AspNetUsers", t => t.User_Id)
                .Index(t => t.CurrentWeek_ID)
                .Index(t => t.NextWeek_ID)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        FirstName = c.String(nullable: false),
                        LastName = c.String(nullable: false),
                        IsDiningRoomClient = c.Boolean(nullable: false),
                        LastLoginTime = c.DateTime(),
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
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.OrderMenu", "User_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.OrderMenu", "NextWeek_ID", "dbo.MenuForWeek");
            DropForeignKey("dbo.OrderMenu", "CurrentWeek_ID", "dbo.MenuForWeek");
            DropForeignKey("dbo.MenuForDay", "MenuForWeek_ID", "dbo.MenuForWeek");
            DropForeignKey("dbo.Dish", "DishType_Id", "dbo.DishType");
            DropForeignKey("dbo.DishQuantity", "MenuForDay_ID", "dbo.MenuForDay");
            DropForeignKey("dbo.MFD_Dishes", "DishID", "dbo.Dish");
            DropForeignKey("dbo.MFD_Dishes", "MenuID", "dbo.MenuForDay");
            DropForeignKey("dbo.MenuForDay", "DayOfWeek_ID", "dbo.DayOfWeek");
            DropForeignKey("dbo.DishQuantity", "Dish_DishID", "dbo.Dish");
            DropForeignKey("dbo.Dish", "DishDetail_ID", "dbo.DishDetail");
            DropIndex("dbo.MFD_Dishes", new[] { "DishID" });
            DropIndex("dbo.MFD_Dishes", new[] { "MenuID" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.OrderMenu", new[] { "User_Id" });
            DropIndex("dbo.OrderMenu", new[] { "NextWeek_ID" });
            DropIndex("dbo.OrderMenu", new[] { "CurrentWeek_ID" });
            DropIndex("dbo.MenuForDay", new[] { "MenuForWeek_ID" });
            DropIndex("dbo.MenuForDay", new[] { "DayOfWeek_ID" });
            DropIndex("dbo.DishQuantity", new[] { "MenuForDay_ID" });
            DropIndex("dbo.DishQuantity", new[] { "Dish_DishID" });
            DropIndex("dbo.Dish", new[] { "DishType_Id" });
            DropIndex("dbo.Dish", new[] { "DishDetail_ID" });
            DropTable("dbo.MFD_Dishes");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.OrderMenu");
            DropTable("dbo.MenuForWeek");
            DropTable("dbo.DishType");
            DropTable("dbo.MenuForDay");
            DropTable("dbo.DishQuantity");
            DropTable("dbo.Dish");
            DropTable("dbo.DishDetail");
            DropTable("dbo.DayOfWeek");
        }
    }
}

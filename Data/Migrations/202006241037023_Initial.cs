namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SharedVulnerabilities",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CommitMessage = c.String(),
                        CweId = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.SharedFiles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SharedVulnerabilityId = c.Int(nullable: false),
                        Name = c.String(),
                        VulnerabilityState = c.Int(nullable: false),
                        Content = c.Binary(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SharedVulnerabilities", t => t.SharedVulnerabilityId, cascadeDelete: true)
                .Index(t => t.SharedVulnerabilityId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SharedFiles", "SharedVulnerabilityId", "dbo.SharedVulnerabilities");
            DropIndex("dbo.SharedFiles", new[] { "SharedVulnerabilityId" });
            DropTable("dbo.SharedFiles");
            DropTable("dbo.SharedVulnerabilities");
        }
    }
}

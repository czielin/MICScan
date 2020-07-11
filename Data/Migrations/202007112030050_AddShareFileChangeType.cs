namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddShareFileChangeType : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SharedFiles", "ChangeKind", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SharedFiles", "ChangeKind");
        }
    }
}

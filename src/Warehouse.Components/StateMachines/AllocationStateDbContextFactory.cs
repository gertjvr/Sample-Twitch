namespace Warehouse.Components.StateMachines
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;


    public class AllocationStateDbContextFactory : IDesignTimeDbContextFactory<AllocationStateDbContext>
    {
        public AllocationStateDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AllocationStateDbContext>();
            optionsBuilder.UseSqlServer("Server=tcp:gertjvr.database.windows.net,1433;Initial Catalog=gertjvr;Persist Security Info=False;User ID=gertjvr;Password=Works4me!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");

            return new AllocationStateDbContext(optionsBuilder.Options);
        }
    }
}
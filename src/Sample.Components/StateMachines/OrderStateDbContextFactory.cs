namespace Sample.Components.StateMachines
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;


    public class OrderStateDbContextFactory : IDesignTimeDbContextFactory<OrderStateDbContext>
    {
        public OrderStateDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<OrderStateDbContext>();
            optionsBuilder.UseSqlServer("Server=tcp:gertjvr.database.windows.net,1433;Initial Catalog=gertjvr;Persist Security Info=False;User ID=gertjvr;Password=Works4me!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");

            return new OrderStateDbContext(optionsBuilder.Options);
        }
    }
}
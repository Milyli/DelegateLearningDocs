using Microsoft.EntityFrameworkCore;

namespace DelegateLearningDocs.Data
{
    public class DelegateLearningContextInitializer
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new DelegateLearningContext(serviceProvider.GetRequiredService<DbContextOptions<DelegateLearningContext>>()))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
            }
        }
    }
}

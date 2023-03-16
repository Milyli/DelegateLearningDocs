using Microsoft.EntityFrameworkCore;

namespace DelegateLearningDocs.Data
{
    public class DelegateLearningContext : DbContext
    {
        public DelegateLearningContext(DbContextOptions<DelegateLearningContext> options): base(options) { }
    }
}

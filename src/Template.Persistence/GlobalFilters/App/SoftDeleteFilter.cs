using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Linq.Expressions;
using Template.Domain.SeedWork;

namespace Template.Persistence.GlobalFilters.App
{
    public static partial class AppDbContextFilters
    {
        public static void ApplySoftDeleteFilter(this ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var type = entityType.ClrType;
                if (type.IsAssignableTo(typeof(ISoftDelete)))
                {
                    var entityMethod = typeof(ModelBuilder).GetMethods()
                        .Where(m => m.Name == nameof(ModelBuilder.Entity) &&
                            m.IsGenericMethod &&
                            m.GetParameters().Length == 0)
                        .FirstOrDefault()!
                        .MakeGenericMethod(type);

                    var entityTypeBuilder = entityMethod.Invoke(modelBuilder, null);
                    var addFilterMethod = entityTypeBuilder!.GetType()
                        .GetMethods()
                        .Where(m => m.Name == nameof(EntityTypeBuilder.HasQueryFilter) &&
                            m.GetParameters()[0].ParameterType == typeof(LambdaExpression))
                        .FirstOrDefault()!;

                    var parameter = Expression.Parameter(type, "e");
                    var property = Expression.Property(parameter, nameof(ISoftDelete.IsDeleted));
                    var condition = Expression.Equal(property, Expression.Constant(false));
                    var lambdaFunc = Expression.Lambda(condition, parameter);

                    addFilterMethod.Invoke(entityTypeBuilder, new object[] { lambdaFunc });
                }
            }
        }
    }
}

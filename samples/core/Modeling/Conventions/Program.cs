using EFModeling.Conventions.EntityTypes;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace EFModeling.Conventions
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var context = new MyContextWithFunctionMapping())
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                context.Database.ExecuteSqlRaw(
                    @"CREATE FUNCTION dbo.BlogsWithMultiplePosts()
                        RETURNS @blogs TABLE
                        (
                            Url nvarchar(max),
                            PostCount int not null
                        )
                        AS
                        BEGIN
                            INSERT INTO @blogs
                            SELECT b.Url, COUNT(p.BlogId)
                            FROM Blogs AS b
                            JOIN Posts AS p ON b.BlogId = p.BlogId
                            GROUP BY b.BlogId, b.Url
                            HAVING COUNT(p.BlogId) > 1

                            RETURN
                        END");

                #region ToFunctionQuery
                var query = from b in context.Set<BlogWithMultiplePosts>()
                            where b.PostCount > 3
                            select new { b.Url, b.PostCount };
                #endregion
                var result = query.ToList();
            }
        }
    }
}

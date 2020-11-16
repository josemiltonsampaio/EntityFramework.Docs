using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace EFModeling.CustomFunctionMapping
{
    class Program
    {
        static void Main(string[] args)
        {
        }
    }
    //        using var context = new BloggingContext();
    //        context.Database.EnsureDeleted();
    //        context.Database.EnsureCreated();

    //        context.Database.ExecuteSqlRaw(
    //            @"CREATE FUNCTION dbo.CommentedPostCountForBlog(@id int)
    //                RETURNS int
    //                AS
    //                BEGIN
    //                    RETURN (SELECT COUNT(*)
    //                        FROM [Posts] AS [p]
    //                        WHERE ([p].[BlogId] = @id) AND ((
    //                            SELECT COUNT(*)
    //                            FROM [Comments] AS [c]
    //                            WHERE [p].[PostId] = [c].[PostId]) > 0));
    //                END");

    //        context.Database.ExecuteSqlRaw(
    //            @"CREATE FUNCTION dbo.PostsTaggedWith(@likeTreshold int)
    //                RETURNS @posts TABLE
    //                (
    //                    PostId int not null,
    //                    BlogId int not null,
    //                    Content nvarchar(max),
    //                    Rating int not null,
    //                    Title nvarchar(max)
    //                )
    //                AS
    //                BEGIN
    //                    INSERT INTO @posts
    //                    SELECT[p].[PostId], [p].[BlogId], [p].[Content], [p].[Rating], [p].[Title]
    //                    FROM[Posts] AS[p]
    //                    WHERE(
    //                        SELECT COUNT(*)
    //                        FROM[Comments] AS[c]
    //                        WHERE([p].[PostId] = [c].[PostId]) AND([c].[Likes] >= @likeTreshold)) > 0

    //                    RETURN
    //                END");

    //        var likeTreshold = 1;
    //        var blah = context.Posts.Where(p => p.Comments.Where(c => c.Likes >= likeTreshold).Count() > 0).ToList();



    //        #region BasicQuery
    //        var query1 = from b in context.Blogs
    //                     where context.ActivePostCountForBlog(b.BlogId) > 2
    //                     select b;
    //        #endregion
    //        var result1 = query1.ToList();

    //        #region HasTranslationQuery
    //        var query2 = from p in context.Posts
    //                     where p.PostId < context.Difference(p.BlogId, 3)
    //                     select p;
    //        #endregion
    //        var result2 = query2.ToList();

    //        //#region TableValuedFunctionQuery
    //        //var query4 = from t in context.Tags
    //        //             where t.TagId.Length < 10
    //        //             select context.PostsTaggedWith(t.TagId).ToList();
    //        //#endregion
    //        //var result4 = query4.ToList();
    //    }
    //}
}

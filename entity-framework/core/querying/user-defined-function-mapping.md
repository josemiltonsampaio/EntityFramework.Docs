---
title: User-defined function mapping - EF Core
description: Mapping user-defined functions to database functions
author: maumar
ms.date: 11/23/2020
uid: core/modeling/custom-function-mapping
---
# User-defined function mapping

EF Core allows for using user-defined SQL functions in queries. To do that, the functions need to be mapped to a CLR method during model configuration. When translating the LINQ query to SQL, the user-defined function is called instead of the CLR function it has been mapped to.

## Mapping a method to a SQL function

To illustrate how user-defined function mapping work, let's define the following entities:

[!code-csharp[Main](../../../samples/core/Querying/UserDefinedFunctionMapping/Model.cs#BlogEntity)]

[!code-csharp[Main](../../../samples/core/Querying/UserDefinedFunctionMapping/Model.cs#PostEntity)]

[!code-csharp[Main](../../../samples/core/Querying/UserDefinedFunctionMapping/Model.cs#CommentEntity)]

And the following model configuration:

[!code-csharp[Main](../../../samples/core/Querying/UserDefinedFunctionMapping/Model.cs#EntityConfiguration)]

Blog can have many posts, each post can be tagged with multiple tags, and each tag can be associated with multiple posts; we have set up a many-to-many relationship.

Next, create the user-defined function `CommentedPostCountForBlog`, which returns the count of posts with at least one comment for a given blog, based on the blog `Id`:

```sql
CREATE FUNCTION dbo.CommentedPostCountForBlog(@id int)
RETURNS int
AS
BEGIN
    RETURN (SELECT COUNT(*)
        FROM [Posts] AS [p]
        WHERE ([p].[BlogId] = @id) AND ((
            SELECT COUNT(*)
            FROM [Comments] AS [c]
            WHERE [p].[PostId] = [c].[PostId]) > 0));
END
```

To use this function in EF Core, we define the following CLR method, which we map to the user-defined function:

[!code-csharp[Main](../../../samples/core/Querying/UserDefinedFunctionMapping/Model.cs#BasicFunctionDefinition)]

In the example, the method is defined on `DbContext`, but it can also be defined as a static method in other places. Note that the body of the CLR method is not important, as it will never be invoked client-side; EF Core only looks at the method signature.

This function definition can now be associated with user-defined function in the model configuration:

[!code-csharp[Main](../../../samples/core/Querying/UserDefinedFunctionMapping/Model.cs#BasicFunctionConfiguration)]

> [!NOTE]
> By default, EF Core tries to map CLR function to a user-defined function with the same name. If the names differ, we can use `HasName` to provide the correct name for the user-defined function we want to map to.

Now, executing the following query:

[!code-csharp[Main](../../../samples/core/Querying/UserDefinedFunctionMapping/Program.cs#BasicQuery)]

Will produce this SQL:

```sql
SELECT [b].[BlogId], [b].[Rating], [b].[Url]
FROM [Blogs] AS [b]
WHERE [dbo].[CommentedPostCountForBlog]([b].[BlogId]) > 1
```

## Mapping a method to a custom SQL

EF Core also allows for user-defined functions that get converted to a specific SQL. This can be done by specifying a SQL expression using the [Microsoft.EntityFrameworkCore.Query.SqlExpressions](/dotnet/api/microsoft.entityframeworkcore.query.sqlexpressions) API. The SQL expression is provided using `HasTranslation` method during user-defined function configuration.

In the example below, we'll create a function that computes difference between two integers.

The CLR method is as follows:

[!code-csharp[Main](../../../samples/core/Querying/UserDefinedFunctionMapping/Model.cs#HasTranslationFunctionDefinition)]

The function definition is as follows:

[!code-csharp[Main](../../../samples/core/Querying/UserDefinedFunctionMapping/Model.cs#HasTranslationFunctionConfiguration)]

Note that [SqlExpressionFactory](/dotnet/api/microsoft.entityframeworkcore.query.sqlexpressionfactory) is used to construct a `SqlExpression` tree.

Once we define the function, it can be used in the query. Instead of calling database function, EF Core will translate the method body directly into SQL based on the SQL expression tree constructed from the HasTranslation. The following LINQ query:

[!code-csharp[Main](../../../samples/core/Querying/UserDefinedFunctionMapping/Program.cs#HasTranslationQuery)]

Produces the following SQL:

```sql
SELECT [p].[PostId], [p].[BlogId], [p].[Content], [p].[Rating], [p].[Title]
FROM [Posts] AS [p]
WHERE [p].[PostId] < CASE
    WHEN [p].[BlogId] > 3 THEN [p].[BlogId] - 3
    ELSE 3 - [p].[BlogId]
END
```

## Mapping a Queryable function to a table-valued gunction

EF Core also supports mapping to a table-valued function using a user-defined CLR function returning an `IQueryable` of entity types, allowing EF Core to map TVFs with parameters. The process is similar to mapping a scalar user-defined function to a SQL function: we need a TVF in the database, a CLR function that is used in the LINQ queries, and a mapping between the two.

As an example, we'll use a table-valued function that returns all posts having at least one comment that meets a given "Like" threshold:

```sql
CREATE FUNCTION dbo.PostsWithPopularComments(@likeThreshold int)
RETURNS @posts TABLE
(
    PostId int not null,
    BlogId int not null,
    Content nvarchar(max),
    Rating int not null,
    Title nvarchar(max)
)
AS
BEGIN
    INSERT INTO @posts
    SELECT[p].[PostId], [p].[BlogId], [p].[Content], [p].[Rating], [p].[Title]
    FROM[Posts] AS[p]
    WHERE(
        SELECT COUNT(*)
        FROM[Comments] AS[c]
        WHERE([p].[PostId] = [c].[PostId]) AND([c].[Likes] >= @likeThreshold)) > 0

    RETURN
END
```

The CLR function signature is as follows:

[!code-csharp[Main](../../../samples/core/Querying/UserDefinedFunctionMapping/Model.cs#QueryableFunctionDefinition)]

> [!TIP]
> The `FromExpression` call in the CLR function body allows for the function to be the root of the EF Core query, meaning it can be used instead of a regular DbSet.

And below is the mapping:

[!code-csharp[Main](../../../samples/core/Querying/UserDefinedFunctionMapping/Model.cs#QueryableFunctionConfigurationHasDbFunction)]

> [!CAUTION]
> Mapping to an `IQueryable` of entity types overrides the default mapping to a table for the DbSet. If necessary - for example when the entity is not keyless - mapping to the table must be specified explicitly using `ToTable` method.

When the function is mapped, the following query:

[!code-csharp[Main](../../../samples/core/Querying/UserDefinedFunctionMapping/Program.cs#TableValuedFunctionQuery)]

Produces:

```sql
SELECT [p].[PostId], [p].[BlogId], [p].[Content], [p].[Rating], [p].[Title]
FROM [dbo].[PostsWithPopularComments](@likeThreshold) AS [p]
ORDER BY [p].[Rating]
```

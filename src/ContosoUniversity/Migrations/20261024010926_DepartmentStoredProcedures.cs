using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContosoUniversity.Migrations
{
    /// <summary>
    /// Re-creates the Department_Insert / Department_Update / Department_Delete stored procedures
    /// that the EF6 migrations 201411021957458_DepartmentSP and 201411022009135_RowVersion created
    /// (the RowVersion-aware final versions). They back the EF Core
    /// Insert/Update/DeleteUsingStoredProcedure mapping configured in SchoolContext.OnModelCreating.
    /// </summary>
    public partial class DepartmentStoredProcedures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE PROCEDURE [dbo].[Department_Insert]
    @Name nvarchar(50),
    @Budget money,
    @StartDate datetime2,
    @InstructorID int
AS
BEGIN
    INSERT [dbo].[Department]([Name], [Budget], [StartDate], [InstructorID])
    VALUES (@Name, @Budget, @StartDate, @InstructorID)

    DECLARE @DepartmentID int
    SELECT @DepartmentID = [DepartmentID]
    FROM [dbo].[Department]
    WHERE @@ROWCOUNT > 0 AND [DepartmentID] = scope_identity()

    SELECT t0.[DepartmentID], t0.[RowVersion]
    FROM [dbo].[Department] AS t0
    WHERE @@ROWCOUNT > 0 AND t0.[DepartmentID] = @DepartmentID
END");

            migrationBuilder.Sql(@"
CREATE PROCEDURE [dbo].[Department_Update]
    @DepartmentID int,
    @Name nvarchar(50),
    @Budget money,
    @StartDate datetime2,
    @InstructorID int,
    @RowVersion_Original rowversion
AS
BEGIN
    UPDATE [dbo].[Department]
    SET [Name] = @Name, [Budget] = @Budget, [StartDate] = @StartDate, [InstructorID] = @InstructorID
    WHERE (([DepartmentID] = @DepartmentID) AND (([RowVersion] = @RowVersion_Original) OR ([RowVersion] IS NULL AND @RowVersion_Original IS NULL)))

    SELECT t0.[RowVersion]
    FROM [dbo].[Department] AS t0
    WHERE @@ROWCOUNT > 0 AND t0.[DepartmentID] = @DepartmentID
END");

            // The delete procedure additionally returns @@ROWCOUNT so that EF Core can detect an
            // optimistic-concurrency failure (the EF6 mapping relied on the provider for this).
            migrationBuilder.Sql(@"
CREATE PROCEDURE [dbo].[Department_Delete]
    @DepartmentID int,
    @RowVersion_Original rowversion
AS
BEGIN
    DELETE [dbo].[Department]
    WHERE (([DepartmentID] = @DepartmentID) AND (([RowVersion] = @RowVersion_Original) OR ([RowVersion] IS NULL AND @RowVersion_Original IS NULL)))

    RETURN @@ROWCOUNT
END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[Department_Delete]");
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[Department_Update]");
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[Department_Insert]");
        }
    }
}

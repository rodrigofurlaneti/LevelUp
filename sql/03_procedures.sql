USE LevelUp;
GO

IF OBJECT_ID('dbo.usp_UserAccount_Upsert','P') IS NOT NULL DROP PROCEDURE dbo.usp_UserAccount_Upsert;
GO
CREATE PROCEDURE dbo.usp_UserAccount_Upsert
  @userName VARCHAR(60),
  @displayName VARCHAR(100)
AS
BEGIN
  SET NOCOUNT ON;
  DECLARE @id INT = (SELECT UserId FROM dbo.UserAccount WHERE UserName = @userName);
  IF @id IS NULL
  BEGIN
    INSERT INTO dbo.UserAccount(UserName, DisplayName) VALUES(@userName, @displayName);
    SET @id = SCOPE_IDENTITY();
  END
  ELSE
  BEGIN
    UPDATE dbo.UserAccount SET DisplayName = @displayName, IsActive = 1 WHERE UserId = @id;
  END
  SELECT @id;
END
GO

IF OBJECT_ID('dbo.usp_Activity_Create','P') IS NOT NULL DROP PROCEDURE dbo.usp_Activity_Create;
GO
CREATE PROCEDURE dbo.usp_Activity_Create
  @userId INT,
  @activityName VARCHAR(100),
  @activityKind INT,
  @defaultPoints INT
AS
BEGIN
  SET NOCOUNT ON;
  INSERT INTO dbo.Activity(UserId, ActivityName, ActivityKind, DefaultPoints)
    VALUES(@userId, @activityName, @activityKind, @defaultPoints);
  SELECT SCOPE_IDENTITY();
END
GO

IF OBJECT_ID('dbo.usp_ActivityLog_Log','P') IS NOT NULL DROP PROCEDURE dbo.usp_ActivityLog_Log;
GO
CREATE PROCEDURE dbo.usp_ActivityLog_Log
  @userId INT,
  @activityDate DATETIME2,
  @activityId INT = NULL,
  @fundamentalCode INT = NULL,
  @pointsAwarded INT,
  @notesText VARCHAR(255) = NULL
AS
BEGIN
  SET NOCOUNT ON;
  -- enforce uniqueness
  IF @fundamentalCode IS NOT NULL AND EXISTS(SELECT 1 FROM dbo.ActivityLog WHERE UserId=@userId AND ActivityDate=CAST(@activityDate AS DATE) AND FundamentalCode=@fundamentalCode)
    BEGIN RAISERROR('Fundamental already logged today.', 16, 1); RETURN; END
  IF @activityId IS NOT NULL AND EXISTS(SELECT 1 FROM dbo.ActivityLog WHERE UserId=@userId AND ActivityDate=CAST(@activityDate AS DATE) AND ActivityId=@activityId)
    BEGIN RAISERROR('Activity already logged today.', 16, 1); RETURN; END

  INSERT INTO dbo.ActivityLog(UserId, ActivityId, FundamentalCode, ActivityDate, PointsAwarded, NotesText)
    VALUES(@userId, @activityId, @fundamentalCode, CAST(@activityDate AS DATE), @pointsAwarded, @notesText);
  SELECT CAST(SCOPE_IDENTITY() AS BIGINT);
END
GO

IF OBJECT_ID('dbo.usp_Score_GetDaily','P') IS NOT NULL DROP PROCEDURE dbo.usp_Score_GetDaily;
GO
CREATE PROCEDURE dbo.usp_Score_GetDaily
  @userId INT,
  @activityDate DATETIME2
AS
BEGIN
  SET NOCOUNT ON;
  SELECT ISNULL(SUM(PointsAwarded),0) AS TotalPoints
  FROM dbo.ActivityLog
  WHERE UserId=@userId AND ActivityDate = CAST(@activityDate AS DATE);
END
GO

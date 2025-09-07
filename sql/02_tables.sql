USE LevelUp;
GO

IF OBJECT_ID('dbo.UserAccount','U') IS NOT NULL DROP TABLE dbo.UserAccount;
IF OBJECT_ID('dbo.Activity','U') IS NOT NULL DROP TABLE dbo.Activity;
IF OBJECT_ID('dbo.ActivityLog','U') IS NOT NULL DROP TABLE dbo.ActivityLog;

CREATE TABLE dbo.UserAccount(
  UserId INT IDENTITY PRIMARY KEY,
  UserName VARCHAR(60) NOT NULL UNIQUE,
  DisplayName VARCHAR(100) NOT NULL,
  IsActive BIT NOT NULL DEFAULT 1,
  CreatedAtUtc DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

CREATE TABLE dbo.Activity(
  ActivityId INT IDENTITY PRIMARY KEY,
  UserId INT NOT NULL REFERENCES dbo.UserAccount(UserId),
  ActivityName VARCHAR(100) NOT NULL,
  ActivityKind INT NOT NULL, -- 0 Fundamental, 1 Task, 2 Negative
  DefaultPoints INT NOT NULL,
  IsActive BIT NOT NULL DEFAULT 1,
  CreatedAtUtc DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

CREATE TABLE dbo.ActivityLog(
  LogId BIGINT IDENTITY PRIMARY KEY,
  UserId INT NOT NULL REFERENCES dbo.UserAccount(UserId),
  ActivityId INT NULL REFERENCES dbo.Activity(ActivityId),
  FundamentalCode INT NULL, -- enum FundamentalCode
  ActivityDate DATE NOT NULL,
  PointsAwarded INT NOT NULL,
  NotesText VARCHAR(255) NULL,
  CreatedAtUtc DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

-- Unique filtered indexes
CREATE UNIQUE INDEX UX_ActivityLog_User_Fundamental_Date
ON dbo.ActivityLog(UserId, ActivityDate, FundamentalCode)
WHERE FundamentalCode IS NOT NULL;

CREATE UNIQUE INDEX UX_ActivityLog_User_Activity_Date
ON dbo.ActivityLog(UserId, ActivityDate, ActivityId)
WHERE ActivityId IS NOT NULL;

-- Score query helper
CREATE INDEX IX_ActivityLog_User_Date ON dbo.ActivityLog(UserId, ActivityDate);

USE LevelUp;
GO
EXEC dbo.usp_UserAccount_Upsert @userName='demo', @displayName='Demo User';
DECLARE @userId INT = (SELECT UserId FROM dbo.UserAccount WHERE UserName='demo');
EXEC dbo.usp_Activity_Create @userId=@userId, @activityName='Custom Task', @activityKind=1, @defaultPoints=2;
EXEC dbo.usp_Activity_Create @userId=@userId, @activityName='Custom Negative', @activityKind=2, @defaultPoints=-2;
GO

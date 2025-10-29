IF DB_ID('ConnectFourDB') IS NULL 

    CREATE DATABASE ConnectFourDB; 

GO 

  

USE ConnectFourDB; 

GO 
 
IF OBJECT_ID('dbo.Users', 'U') IS NULL 

BEGIN 

    CREATE TABLE dbo.Users ( 

        Id           INT IDENTITY(1,1) PRIMARY KEY, 

        Username     NVARCHAR(50)  NOT NULL UNIQUE, 

        PasswordHash NVARCHAR(200) NOT NULL 

    ); 

END 

GO 
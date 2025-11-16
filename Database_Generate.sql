IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Order]') AND type in (N'U'))
    DROP TABLE [dbo].[Order]

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Product]') AND type in (N'U'))
    DROP TABLE [dbo].[Product]

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Category]') AND type in (N'U'))
    DROP TABLE [dbo].[Category]

CREATE TABLE [dbo].[Category] (
    ID int identity primary key not null,
    [Name] nvarchar(50) not null
)

CREATE TABLE [dbo].[Product] (
    ID int identity primary key not null,
    [Name] nvarchar(50) not null,
    Price decimal(18, 2) not null,
    Stock int not null,
    VATPercentage int not null,
    CategoryID int references Category(ID) not null,
    [Description] XML not null
)

CREATE TABLE [dbo].[Order] (
    ID int identity primary key not null,
    [Date] datetime not null,
    Deadline datetime not null,
    [Status] nvarchar(20) not null,
    PaymentMethod nvarchar(20) not null,
    ProductID int references Product(ID) not null
)
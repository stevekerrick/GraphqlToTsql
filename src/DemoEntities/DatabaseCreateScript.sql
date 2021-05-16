USE [master]
GO

DROP DATABASE IF EXISTS [GraphqlToTsqlDemoDB]
GO

CREATE DATABASE [GraphqlToTsqlDemoDB]
GO

USE [GraphqlToTsqlDemoDB]
GO


CREATE TABLE Seller (
    [Name]        NVARCHAR(64) NOT NULL PRIMARY KEY CLUSTERED
,   DistributorName NVARCHAR(64) NULL
,   City          VARCHAR(64) NULL
,   [State]       VARCHAR(64) NULL
,   PostalCode    VARCHAR(15) NULL
,   CONSTRAINT FK_Seller_Seller FOREIGN KEY (DistributorName) REFERENCES Seller ([Name])
);

CREATE TABLE Product (
    [Name]        NVARCHAR(64) NOT NULL PRIMARY KEY CLUSTERED
,   [Description] NVARCHAR(MAX) NULL
,   Price         DECIMAL(5,2) NOT NULL
);

CREATE TABLE [Order] (
    Id            INT NOT NULL IDENTITY(1,1) PRIMARY KEY CLUSTERED
,   SellerName    NVARCHAR(64) NOT NULL
,   [Date]        DATETIMEOFFSET NOT NULL
,   Shipping      DECIMAL(5,2) NOT NULL
,   CONSTRAINT FK_Order_Seller FOREIGN KEY (SellerName) REFERENCES Seller ([Name])
);

CREATE TABLE OrderDetail (
    OrderId       INT NOT NULL
,   ProductName   NVARCHAR(64) NOT NULL
,   Quantity      INT NOT NULL
,   CONSTRAINT PK_OrderDetail PRIMARY KEY NONCLUSTERED (OrderId, ProductName)
,   CONSTRAINT FK_OrderDetail_Order FOREIGN KEY (OrderId) REFERENCES [Order] (Id)
,   CONSTRAINT FK_OrderDetail_Product FOREIGN KEY (ProductName) REFERENCES Product ([Name])
);

CREATE TABLE Badge (
    [Name]        VARCHAR(64) NOT NULL PRIMARY KEY CLUSTERED
,   IsSpecial     BIT NOT NULL
);

CREATE TABLE SellerBadge (
    SellerName    NVARCHAR(64) NOT NULL
,   BadgeName     VARCHAR(64) NOT NULL
,   DateAwarded   DATE NOT NULL
,   CONSTRAINT PK_SellerBadge PRIMARY KEY NONCLUSTERED (SellerName, BadgeName)
,   CONSTRAINT FK_SellerBadge_Seller FOREIGN KEY (SellerName) REFERENCES Seller ([Name])
,   CONSTRAINT FK_SellerBadge_Badge FOREIGN KEY (BadgeName) REFERENCES Badge ([Name])
);
GO

-- Create temp stored proc to make it easier to script in orders
CREATE PROCEDURE #CreateOrder (
  @sellerName NVARCHAR(64)
, @date DATETIMEOFFSET
, @shipping DECIMAL(5,2)
, @productName1 NVARCHAR(64), @quantity1 INT
, @productName2 NVARCHAR(64) = NULL, @quantity2 INT = NULL
, @productName3 NVARCHAR(64) = NULL, @quantity3 INT = NULL
, @productName4 NVARCHAR(64) = NULL, @quantity4 INT = NULL
, @productName5 NVARCHAR(64) = NULL, @quantity5 INT = NULL
, @productName6 NVARCHAR(64) = NULL, @quantity6 INT = NULL
, @productName7 NVARCHAR(64) = NULL, @quantity7 INT = NULL
, @productName8 NVARCHAR(64) = NULL, @quantity8 INT = NULL
, @productName9 NVARCHAR(64) = NULL, @quantity9 INT = NULL
, @productName10 NVARCHAR(64) = NULL, @quantity10 INT = NULL
)
AS
  INSERT [Order] VALUES (@sellerName, @date, @shipping);
  DECLARE @orderId INT = SCOPE_IDENTITY();

  INSERT OrderDetail VALUES (@orderId, @productName1, @quantity1);
  IF @productName2 IS NOT NULL INSERT OrderDetail VALUES (@orderId, @productName2, @quantity2);
  IF @productName3 IS NOT NULL INSERT OrderDetail VALUES (@orderId, @productName3, @quantity3);
  IF @productName4 IS NOT NULL INSERT OrderDetail VALUES (@orderId, @productName4, @quantity4);
  IF @productName5 IS NOT NULL INSERT OrderDetail VALUES (@orderId, @productName5, @quantity5);
  IF @productName6 IS NOT NULL INSERT OrderDetail VALUES (@orderId, @productName6, @quantity6);
  IF @productName7 IS NOT NULL INSERT OrderDetail VALUES (@orderId, @productName7, @quantity7);
  IF @productName8 IS NOT NULL INSERT OrderDetail VALUES (@orderId, @productName8, @quantity8);
  IF @productName9 IS NOT NULL INSERT OrderDetail VALUES (@orderId, @productName9, @quantity9);
  IF @productName10 IS NOT NULL INSERT OrderDetail VALUES (@orderId, @productName10, @quantity10);
GO


PRINT 'Populating: Seller';
DECLARE
  @amber NVARCHAR(64) = 'Amber'
, @bill NVARCHAR(64) = 'Bill'
, @chris NVARCHAR(64) = 'Chris'
, @donada NVARCHAR(64) = 'Donada'
, @erik NVARCHAR(64) = 'Erik'
, @francesca NVARCHAR(64) = 'Francesca'
, @georgey NVARCHAR(64) = 'Georgey'
, @helena NVARCHAR(64) = 'Helena'
, @ivan NVARCHAR(64) = 'Ivan'
, @jolie NVARCHAR(64) = 'Jolie'
, @kevin NVARCHAR(64) = 'Kevin'
, @lynnette NVARCHAR(64) = 'Lynnette'
, @marco NVARCHAR(64) = 'Marco'
, @novalee NVARCHAR(64) = 'Novalee'
, @ovette NVARCHAR(64) = 'Ovette'
, @pete NVARCHAR(64) = 'Pete'
, @queenie NVARCHAR(64) = 'Queenie'
, @roy NVARCHAR(64) = 'Roy'
, @steve NVARCHAR(64) = 'Steve'
, @tariq NVARCHAR(64) = 'Tariq'
, @ursula NVARCHAR(64) = 'Ursula'
, @vanessa NVARCHAR(64) = 'Vanessa'
, @willem NVARCHAR(64) = 'Willem'
, @xavier NVARCHAR(64) = 'Xavier'
, @yvette NVARCHAR(64) = 'Yvette'
, @zeus NVARCHAR(64) = 'Zeus'
;
INSERT Seller
 ([Name],     DistributorName, City,         [State], PostalCode)
VALUES
 (@amber,     NULL,           'Atlanta',     'GA',   '30316')
,(@bill,      @amber,         'Los Angeles', 'CA',   '90001')
,(@chris,     @amber,         'Miami',       'FL',   '33101')
,(@donada,    @bill,          'Evansville',  'IN',   '47711')
,(@erik,      @bill,          'Baldwin',     'NY',   '11510')
,(@francesca, @bill,          'Akron',       'OH',   '44312')
,(@georgey,   @bill,          'Farmington',  'MI',   '48331')
,(@helena,    @bill,          'Eastlake',    'OH',   '44095')
,(@ivan,      @erik,          'San Angelo',  'TX',   '76901')
,(@jolie,     @ivan,          'Goodlettsville', 'TN', '37072')
,(@kevin,     @ivan,          'Millville',   'NJ',   '08332')
,(@lynnette,  @chris,         'Rockville',   'MD',   '20850')
,(@marco,     @chris,         'Riverside',   'NJ',   '08075')
,(@novalee,   @marco,         'Navarre',     'FL',   '32566')
,(@ovette,    @marco,         'Louisville',  'KY',   '40207')
,(@pete,      @marco,         'Traverse City', 'MI', '49684')
,(@queenie,   @marco,         'Manahawkin',  'NJ',   '08050')
,(@roy,       @marco,         'Brunswick',   'GA',   '31525')
,(@steve,     @marco,         'Plainfield',  'NJ',   '07060')
,(@tariq,     @ovette,        'Richmond',    'VA',   '23223')
,(@ursula,    @ovette,         NULL,          NULL,   NULL)
,(@vanessa,   @ovette,        'Grand Island', 'NE',  '68801')
,(@willem,    @vanessa,       'Norwood',     'MA',   '02062')
,(@xavier,    @willem,        'Greenville',  'NC',   '27834')
,(@yvette,    @xavier,        'Petersburg',  'VA',   '23803')
,(@zeus,      @xavier,        'Dublin',      'GA',   '31021')
;


PRINT 'Populating: Product';
DECLARE
  @hammer NVARCHAR(64) = N'Hammer'
, @pliers NVARCHAR(64) = N'Pliers'
, @drill NVARCHAR(64) = N'Drill'
, @handSaw NVARCHAR(64) = N'Hand Saw'
, @circularSaw NVARCHAR(64) = N'Circular Saw'
, @pipeWrench NVARCHAR(64) = N'Pipe Wrench'
, @screwdriver NVARCHAR(64) = N'Screwdriver'
, @wireBrush NVARCHAR(64) = N'Wire Brush'
, @chisel NVARCHAR(64) = N'Chisel'
, @workLight NVARCHAR(64) = N'Work Light'
, @woodGlue NVARCHAR(64) = N'Wood Glue'
;
INSERT Product
 ([Name],        [Description], Price)
VALUES
 (@hammer,      N'Sturdy claw hammer with fiberglass handle',     29.95)
,(@pliers,      N'Sturdy pliers with rubber grip and two jaw widths',     17.45)
,(@drill,       N'Sturdy electric drill, 3/8" chuck, with variety of drill bits',     59.90)
,(@handSaw,     N'Sturdy hand saw, crosscut teeth',     15.50)
,(@circularSaw, N'Sturdy circular saw with 2 foot cord',     80.99)
,(@pipeWrench,  N'Sturdy pipe wrench, red',     29.95)
,(@screwdriver, N'Sturdy screwdriver, Philips #2 head',     7.99)
,(@wireBrush,   N'Sturdy wire brush, to strip the fussiest paint',     12.99)
,(@chisel,      N'Sturdy chisel, 3/4" blade, with wooden handle',     14.80)
,(@workLight,   N'Sturdy work light, 6V, batteries included',     64.75)
,(@woodGlue,    N'Sturdy wood glue with super strong hold',     7.45)
;


PRINT 'Populating: Order';
EXEC #CreateOrder @amber, '2020-01-01T12:30:15Z', 7.95, @hammer, 1;
EXEC #CreateOrder @bill, '2020-01-29T07:58:13-06:00', 12.95, @hammer, 1, @pliers, 1;
EXEC #CreateOrder @bill, '2020-02-06T14:12:12-06:00', 11.95, @hammer, 3, @drill, 3;
EXEC #CreateOrder @bill, '2020-02-11T09:30:00-05:00', 15.95, @hammer, 1, @handSaw, 1, @circularSaw, 1;
EXEC #CreateOrder @bill, '2020-02-14T10:10:15+00:00', 14.95, @hammer, 1, @pipeWrench, 3, @screwdriver, 3;
EXEC #CreateOrder @bill, '2020-02-17T11:11:58+03:30', 12.95, @hammer, 1, @wireBrush, 1;
EXEC #CreateOrder @bill, '2020-03-12T12:12:00Z', 14.95, @hammer, 1, @chisel, 4, @workLight, 1;
EXEC #CreateOrder @Chris, '2020-03-25T13:13:15-04:00', 13.95, @hammer, 1, @woodGlue, 3, @handSaw, 1;
EXEC #CreateOrder @Chris, '2020-04-23T14:14:35-05:00', 152.95, @hammer, 3, @pliers, 3, @drill, 4, @handSaw, 2, @circularSaw, 5, @pipeWrench, 2, @screwdriver, 15, @wireBrush, 6, @chisel, 2, @workLight, 4;
EXEC #CreateOrder @Chris, '2020-05-04T15:15:00+02:00', 8.95, @hammer, 1, @chisel, 2;
EXEC #CreateOrder @donada, '2020-05-12T16:16:20Z', 11.95, @hammer, 1, @workLight, 1;
EXEC #CreateOrder @donada, '2020-05-19T17:17:17+04:00', 23.95, @hammer, 1, @woodGlue, 50;
EXEC #CreateOrder @erik, '2020-06-03T18:18:18-02:00', 14.95, @hammer, 1, @pliers, 4, @drill, 1;
EXEC #CreateOrder @erik, '2020-06-04T19:19:19Z', 15.95, @hammer, 1, @handSaw, 1, @workLight, 1;
EXEC #CreateOrder @erik, '2020-06-12T20:20:20Z', 14.95, @hammer, 1, @circularSaw, 1, @pipeWrench, 1;
EXEC #CreateOrder @francesca, '2020-07-06T21:21:21-04:00', 7.95, @hammer, 1;
EXEC #CreateOrder @francesca, '2020-07-13T22:22:22-06:00', 13.95, @hammer, 1, @circularSaw, 1, @wireBrush, 2, @workLight, 1;
EXEC #CreateOrder @georgey, '2020-07-24T23:23:23-05:00', 10.95, @hammer, 1, @screwdriver, 1, @workLight, 1;
EXEC #CreateOrder @helena, '2020-08-17T20:15:02-05:00', 21.95, @hammer, 1, @pipeWrench, 1, @screwdriver, 1;
EXEC #CreateOrder @helena, '2020-09-10T19:00:00Z', 9.95, @hammer, 1, @screwdriver, 1, @chisel, 1;
EXEC #CreateOrder @helena, '2020-09-16T19:15:02Z', 7.95, @hammer, 1;
EXEC #CreateOrder @ivan, '2020-09-25T21:50:24-05:00', 23.95, @hammer, 1, @circularSaw, 1, @wireBrush, 1, @woodGlue, 1, @workLight, 3;
EXEC #CreateOrder @ivan, '2020-10-05T20:59:14-06:00', 18.95, @hammer, 1, @pipeWrench, 1, @screwdriver, 1;
EXEC #CreateOrder @ivan, '2020-10-30T08:14:03-04:00', 9.95, @hammer, 1, @screwdriver, 1, @woodGlue, 1;
EXEC #CreateOrder @ivan, '2020-12-29T17:52:33Z', 11.95, @hammer, 1, @wireBrush, 4, @workLight, 1, @chisel, 1;
EXEC #CreateOrder @ivan, '2020-04-06T22:04:04+4:00', 7.95, @hammer, 1;
EXEC #CreateOrder @lynnette, '2020-04-29T06:12:00Z', 15.95, @hammer, 1, @circularSaw, 1, @woodGlue, 1, @chisel, 1;
EXEC #CreateOrder @lynnette, '2020-05-15T08:40:12-06:00', 13.95, @hammer, 1, @screwdriver, 1, @workLight, 2;
EXEC #CreateOrder @lynnette, '2020-05-28T09:50:13-07:00', 7.95, @hammer, 1;
EXEC #CreateOrder @lynnette, '2020-05-29T10:13:16-04:00', 12.95, @hammer, 1, @circularSaw, 1, @workLight, 1;
EXEC #CreateOrder @lynnette, '2020-06-17T11:43:00+03:00', 8.95, @hammer, 1, @woodGlue, 1, @chisel, 1;
EXEC #CreateOrder @lynnette, '2020-07-03T12:41:09Z', 7.95, @hammer, 1;
EXEC #CreateOrder @marco, '2020-07-06T13:52:41+04:00', 8.95, @hammer, 1, @chisel, 1;
EXEC #CreateOrder @marco, '2020-07-15T13:52:50+04:00', 17.95, @hammer, 1, @circularSaw, 1, @screwdriver, 1, @woodGlue, 1;
EXEC #CreateOrder @novalee, '2020-08-06T14:22:00Z', 16.95, @hammer, 1, @pipeWrench, 1;
EXEC #CreateOrder @novalee, '2020-08-07T15:59:03-05:00', 15.95, @hammer, 1, @woodGlue, 1, @workLight, 1, @chisel, 1;
EXEC #CreateOrder @queenie, '2020-09-17T16:32:21-04:00', 7.95, @hammer, 1;
EXEC #CreateOrder @roy, '2020-09-22T17:26:23Z', 7.95, @hammer, 1;
EXEC #CreateOrder @steve, '2020-10-05T18:30:30Z', 8.95, @hammer, 1, @chisel, 1;
EXEC #CreateOrder @tariq, '2020-10-21T19:21:00-06:00', 7.95, @hammer, 1;
EXEC #CreateOrder @tariq, '2020-10-26T20:26:00-05:00', 19.95, @hammer, 1, @wireBrush, 2, @woodGlue, 1, @workLight, 1, @chisel, 1;
EXEC #CreateOrder @ursula, '2020-11-27T21:20:02-04:00', 12.95, @hammer, 1, @pipeWrench, 1, @screwdriver, 1;
EXEC #CreateOrder @vanessa, '2020-11-30T22:09:00Z', 15.95, @hammer, 1, @circularSaw, 1, @pipeWrench, 1;
EXEC #CreateOrder @willem, '2020-12-10T23:57:49-05:00', 12.95, @hammer, 1, @screwdriver, 1, @woodGlue, 1;
EXEC #CreateOrder @yvette, '2020-07-03T16:39:09-06:00', 7.95, @hammer, 1;
EXEC #CreateOrder @yvette, '2020-07-09T12:00:00Z', 8.95, @hammer, 1, @woodGlue, 1;
EXEC #CreateOrder @zeus, '2020-07-15T14:29:44Z', 31.95, @hammer, 1, @pipeWrench, 1, @screwdriver, 1, @woodGlue, 1, @workLight, 1, @chisel, 3;

DROP PROCEDURE #CreateOrder;


PRINT 'Populating: Badge';
DECLARE
  @founder VARCHAR(64) = 'Founder'
, @diamond VARCHAR(64) = 'Diamond'
, @gold VARCHAR(64) = 'Gold'
, @silver VARCHAR(64) = 'Silver'
, @bronze VARCHAR(64) = 'Bronze'
;
INSERT Badge
 ([Name],     IsSpecial)
VALUES
 (@founder,   1)
,(@diamond,   1)
,(@gold,      0)
,(@silver,    0)
,(@bronze,    0)
;


PRINT 'Populating: SellerBadge';
INSERT SellerBadge
 (SellerName, BadgeName, DateAwarded)
VALUES
 (@amber,    @founder,  '2020-01-28')
,(@amber,    @diamond,  '2020-02-21')
,(@bill,     @founder,  '2020-02-07')
,(@bill,     @diamond,  '2020-04-07')
,(@chris,    @diamond,  '2020-05-05')
,(@erik,     @gold,     '2020-06-15')
,(@marco,    @diamond,  '2020-06-26')
,(@ovette,   @diamond,  '2020-07-15')
,(@ivan,     @silver,   '2020-07-20')
,(@ursula,   @silver,   '2020-08-03')
,(@vanessa,  @silver,   '2020-08-11')
,(@willem,   @bronze,   '2020-09-16')
,(@xavier,   @bronze,   '2020-09-23')
,(@queenie,  @bronze,   '2020-11-04')
,(@ivan,     @bronze,   '2020-11-16')
,(@jolie,    @bronze,   '2020-11-19')
;
GO


CREATE FUNCTION tvf_AllDescendants (
  @parentName VARCHAR(64)
)
RETURNS TABLE
AS
RETURN
  WITH ParentCTE AS (
    SELECT
      [Name]
    , DistributorName
    FROM Seller s
    WHERE s.DistributorName = @parentName

    UNION ALL

    SELECT
      child.[Name]
    , child.DistributorName
    FROM ParentCTE parent
    INNER JOIN Seller child
      ON child.DistributorName = parent.[Name]
  )

  SELECT
    [Name]
  FROM ParentCTE;
GO

CREATE FUNCTION tvf_AllAncestors (
  @name VARCHAR(64)
)
RETURNS TABLE
AS
RETURN
  WITH ChildCTE AS (
    SELECT
      [Name]
    , DistributorName
    FROM Seller s
    WHERE s.[Name] = @name AND s.DistributorName IS NOT NULL

    UNION ALL

    SELECT
      parent.[Name]
    , parent.DistributorName
    FROM ChildCTE child
    INNER JOIN Seller parent
      ON parent.[Name] = child.DistributorName AND parent.DistributorName IS NOT NULL
  )

  SELECT
    DistributorName AS [Name]
  FROM ChildCTE;

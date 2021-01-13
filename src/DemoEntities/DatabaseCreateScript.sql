USE [master]
GO

DROP DATABASE IF EXISTS [GraphqlToTsqlDemoDB]
GO

CREATE DATABASE [GraphqlToTsqlDemoDB]
GO

USE [GraphqlToTsqlDemoDB]
GO

CREATE TABLE Disposition (
    Urn           VARCHAR(128) NOT NULL PRIMARY KEY CLUSTERED
,   [Name]        NVARCHAR(128) NOT NULL
);

CREATE TABLE [Location] (
    Id            INT NOT NULL PRIMARY KEY CLUSTERED
,   Urn           VARCHAR(128) NOT NULL
,   [Name]        NVARCHAR(128) NOT NULL
,   IsActive      BIT NOT NULL
);

CREATE TABLE Product (
    Id            INT NOT NULL PRIMARY KEY CLUSTERED
,   Urn           VARCHAR(128) NOT NULL
,   [Name]        NVARCHAR(128) NOT NULL
,   [Weight]      DECIMAL(5,2) NULL
);

CREATE TABLE Lot (
    LotNumber     NVARCHAR(128) NOT NULL  PRIMARY KEY CLUSTERED
,   ExpirationDt  DATE NULL
,   ProductId     INT NOT NULL REFERENCES Product (Id)
);

CREATE TABLE Epc (
    Id            INT NOT NULL IDENTITY(1,1) PRIMARY KEY CLUSTERED
,   Urn           VARCHAR(128) NOT NULL
,   DispositionUrn VARCHAR(128) NULL REFERENCES Disposition (Urn)
,   ParentId      INT NULL REFERENCES Epc (Id)
,   BizLocationId INT NULL REFERENCES [Location] (Id)
,   ReadPointId   INT NULL REFERENCES [Location] (Id)
,   ProductId     INT NULL REFERENCES Product (Id)
,   LotNumber     NVARCHAR(128) NULL REFERENCES Lot (LotNumber)
,   LastUpdate    DATETIMEOFFSET(7) NULL
);
GO


PRINT 'Populating: Disposition';
DECLARE
  @active VARCHAR(128) = 'urn:epcglobal:cbv:disp:active'
, @destroyed VARCHAR(128) = 'urn:epcglobal:cbv:disp:destroyed'
, @inProgress VARCHAR(128) = 'urn:epcglobal:cbv:disp:in_progress'
, @inTransit VARCHAR(128) = 'urn:epcglobal:cbv:disp:in_transit'
, @recalled VARCHAR(128) = 'urn:epcglobal:cbv:disp:recalled'
, @retailSold VARCHAR(128) = 'urn:epcglobal:cbv:disp:retail_sold'
, @returned VARCHAR(128) = 'urn:epcglobal:cbv:disp:returned'
;
INSERT Disposition
 (Urn,          [Name])
VALUES
 (@active,     N'active')
,(@destroyed,  N'destroyed')
,(@inProgress, N'in_progress')
,(@inTransit,  N'in_transit')
,(@recalled,   N'recalled')
,(@retailSold, N'retail_sold')
,(@returned,   N'returned')
;

PRINT 'Populating: Location';
DECLARE
  @jojaBottling INT = 1
, @jojaLine1 INT = 2
, @jojaAgg INT = 3
, @jojaWrhse INT = 4
, @jojaMart INT = 5
, @silverWrhse INT = 6
, @silver1 INT = 7
, @silver2 INT = 8
, @happyFreight INT = 9
;
INSERT [Location]
 (Id,             Urn,                             [Name],                              IsActive)
VALUES
 (@jojaBottling, 'urn:epc:sgln:950110153.000.1',  N'Joja Bottling Plant',               1)
,(@jojaLine1,    'urn:epc:sgln:950110153.000.2',  N'Joja Bottling Plant, Line 1',       1)
,(@jojaAgg,      'urn:epc:sgln:950110153.000.3',  N'Joja Bottling Plant, Agg Station',  1)
,(@jojaWrhse,    'urn:epc:sgln:950371221.000.0',  N'Joja Warehouse',                    1)
,(@jojaMart,     'urn:epc:sgln:950371221.000.1',  N'Joja Mart',                         1)
,(@silverWrhse,  'urn:epc:sgln:211444444.000.0',  N'Silver Foods Warehouse',            0)
,(@silver1,      'urn:epc:sgln:211500014.000.0',  N'Silver Foods #1',                   0)
,(@silver2,      'urn:epc:sgln:211500029.000.0',  N'Silver Foods #2',                   0)
,(@happyFreight, 'urn:epc:sgln:300100000.000.0',  N'Happy Freight Service',             1)
;

PRINT 'Populating: Product';
DECLARE
  @jojaCola INT = 1
, @jojaColaCase INT = 2
, @jojaDietCola INT = 3
, @jojaDietColaCase INT = 4
, @water INT = 5
, @waterCase INT = 6
;
INSERT Product
 (Id,                 Urn,                                     [Name],                [Weight])
VALUES
 (@jojaCola,         'urn:epc:idpat:sgtin:258643.3704146.*',  N'Joja Cola .5L',        NULL)
,(@jojaColaCase,     'urn:epc:idpat:sgtin:258643.3704200.*',  N'Joja Cola Case',       20.32)
,(@jojaDietCola,     'urn:epc:idpat:sgtin:258643.4821101.*',  N'Joja Diet Cola .5L',   NULL)
,(@jojaDietColaCase, 'urn:epc:idpat:sgtin:258643.4821216.*',  N'Joja Diet Cola Case',  20.40)
,(@water,            'urn:epc:idpat:sgtin:258643.6310000.*',  N'Water .5L',            21.14)
,(@waterCase,        'urn:epc:idpat:sgtin:258643.6310025.*',  N'Water Case',           19.59)
;

PRINT 'Populating: Lot';
DECLARE
  @lot2001a INT = 1
, @lot2002a INT = 2
, @lot2002b INT = 3
, @lot2003a INT = 4
, @lot2003b INT = 5
;
INSERT Lot
 (Id,           LotNumber,    ExpirationDt,   ProductId)
VALUES
 (@lot2001a,  N'LOT 2001a',  '2020-01-31',   @jojaCola)
,(@lot2002a,  N'LOT 2002a',  '2020-02-15',   @jojaCola)
,(@lot2002b,  N'LOT 2002b',  '2020-02-17',   @jojaDietCola)
,(@lot2003a,  N'LOT 2003a',  '2020-03-02',   @jojaCola)
,(@lot2003b,  N'LOT 2003b',  '2020-03-21',   @jojaCola)
;

PRINT 'Populating: Epc';
DECLARE @time DATETIMEOFFSET = '2019-04-01 16:00:00Z';
DECLARE @pallet INT, @case INT;

-- Build up pallet with two cases of Cola and one of Diet Cola
INSERT Epc
 ( Urn,                                    DispositionUrn, ParentId, BizLocationId, ReadPointId, ProductId,        LotNumber,     LastUpdate)
VALUES
 ('urn:epc:id:sscc:258643.11122233344',    @active,        null,    @jojaWrhse,    @jojaWrhse,   null,             null,     @time);
SET @pallet = SCOPE_IDENTITY();

INSERT Epc VALUES
 ('urn:epc:idpat:sgtin:258643.3704200.1',  @active,       @pallet,  @jojaWrhse,    @jojaWrhse,  @jojaColaCase,     null,     @time);
SET @case = SCOPE_IDENTITY();
INSERT Epc VALUES
 ('urn:epc:idpat:sgtin:258643.3704146.1',  @active,       @case,    @jojaWrhse,    @jojaWrhse,  @jojaCola,        @lot2003a, @time)
,('urn:epc:idpat:sgtin:258643.3704146.2',  @active,       @case,    @jojaWrhse,    @jojaWrhse,  @jojaCola,        @lot2003a, @time)
,('urn:epc:idpat:sgtin:258643.3704146.3',  @destroyed,    @case,    @jojaWrhse,    @jojaWrhse,  @jojaCola,        @lot2003a, '2019-04-04 14:10:00Z')
,('urn:epc:idpat:sgtin:258643.3704146.4',  @active,       @case,    @jojaWrhse,    @jojaWrhse,  @jojaCola,        @lot2003a, @time)
;
INSERT Epc VALUES
 ('urn:epc:idpat:sgtin:258643.3704200.2',  @active,       @pallet,  @jojaWrhse,    @jojaWrhse,  @jojaColaCase,     null,     @time);
SET @case = SCOPE_IDENTITY();
INSERT Epc VALUES
 ('urn:epc:idpat:sgtin:258643.3704146.11', @active,       @case,    @jojaWrhse,    @jojaWrhse,  @jojaCola,        @lot2001a, @time)
,('urn:epc:idpat:sgtin:258643.3704146.12', @active,       @case,    @jojaWrhse,    @jojaWrhse,  @jojaCola,        @lot2001a, @time)
,('urn:epc:idpat:sgtin:258643.3704146.13', @active,       @case,    @jojaWrhse,    @jojaWrhse,  @jojaCola,        @lot2001a, @time)
,('urn:epc:idpat:sgtin:258643.3704146.14', @active,       @case,    @jojaWrhse,    @jojaWrhse,  @jojaCola,        @lot2001a, @time)
;
INSERT Epc VALUES
 ('urn:epc:idpat:sgtin:258643.4821216.1',  @active,       @pallet,  @jojaWrhse,    @jojaWrhse,  @jojaDietColaCase, null,     @time);
SET @case = SCOPE_IDENTITY();
INSERT Epc VALUES
 ('urn:epc:idpat:sgtin:258643.4821101.21', @active,       @case,    @jojaWrhse,    @jojaWrhse,  @jojaDietCola,    @lot2002b, @time)
,('urn:epc:idpat:sgtin:258643.4821101.22', @active,       @case,    @jojaWrhse,    @jojaWrhse,  @jojaDietCola,    @lot2002b, @time)
,('urn:epc:idpat:sgtin:258643.4821101.23', @active,       @case,    @jojaWrhse,    @jojaWrhse,  @jojaDietCola,    @lot2002b, @time)
,('urn:epc:idpat:sgtin:258643.4821101.24', @active,       @case,    @jojaWrhse,    @jojaWrhse,  @jojaDietCola,    @lot2002b, @time)
;

-- Some colas were recalled from @lot2003a
SET @time = '2019-06-12 10:11:12Z';
INSERT Epc VALUES
 ('urn:epc:idpat:sgtin:258643.4821101.101', @recalled,     null,     null,         @silver1,    @jojaCola,        @lot2003a, @time)
,('urn:epc:idpat:sgtin:258643.4821101.102', @recalled,     null,     null,         @silver1,    @jojaCola,        @lot2003a, @time)
,('urn:epc:idpat:sgtin:258643.4821101.103', @recalled,     null,     null,         @silver2,    @jojaCola,        @lot2003a, @time)
,('urn:epc:idpat:sgtin:258643.4821101.104', @recalled,     null,     null,         @jojaWrhse,  @jojaCola,        @lot2003a, @time)
;

-- Case of water is being sent to Silver Warehouse
SET @time = '2019-04-29 10:11:12Z';
INSERT Epc VALUES
 ('urn:epc:idpat:sgtin:258643.6310025.200', @inTransit,    null,    @silverWrhse,  @jojaBottling, @waterCase,      null,     @time);
SET @case = SCOPE_IDENTITY();
INSERT Epc VALUES
 ('urn:epc:idpat:sgtin:258643.6310000.201', @inTransit,    null,    @silverWrhse,  @jojaBottling, @water,          null,     @time)
,('urn:epc:idpat:sgtin:258643.6310000.202', @inTransit,    null,    @silverWrhse,  @jojaBottling, @water,          null,     @time)
,('urn:epc:idpat:sgtin:258643.6310000.203', @inTransit,    null,    @silverWrhse,  @jojaBottling, @water,          null,     @time)
,('urn:epc:idpat:sgtin:258643.6310000.204', @inTransit,    null,    @silverWrhse,  @jojaBottling, @water,          null,     @time)
;


GO
CREATE FUNCTION tvf_AllDescendants (
  @parentEpcId INT
)
RETURNS TABLE
AS
RETURN
  WITH ParentCTE AS (
    SELECT
      Id
    , ParentId
    FROM Epc e
    WHERE e.ParentId = @parentEpcId

    UNION ALL

    SELECT
      child.Id
    , child.ParentId
    FROM ParentCTE parent
    INNER JOIN Epc child
      ON child.ParentId = parent.Id
  )

  SELECT
    Id
  FROM ParentCTE;


GO
CREATE FUNCTION tvf_AllAncestors (
  @epcId INT
)
RETURNS TABLE
AS
RETURN
  WITH ChildCTE AS (
    SELECT
      Id
    , ParentId
    FROM Epc e
    WHERE e.id = @epcId AND e.ParentId IS NOT NULL

    UNION ALL

    SELECT
      parent.Id
    , parent.ParentId
    FROM ChildCTE child
    INNER JOIN Epc parent
      ON parent.Id = child.ParentId AND parent.ParentId IS NOT NULL
  )

  SELECT
    ParentId AS Id
  FROM ChildCTE;
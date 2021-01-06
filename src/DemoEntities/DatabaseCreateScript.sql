USE [master]
GO

DROP DATABASE IF EXISTS [GraphqlToTsqlDemoDB]
GO

CREATE DATABASE [GraphqlToTsqlDemoDB]
GO

USE [GraphqlToTsqlDemoDB]
GO

CREATE TABLE Disposition (
    Id            INT NOT NULL PRIMARY KEY CLUSTERED
,   Urn           VARCHAR(128) NOT NULL
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
    Id            INT NOT NULL PRIMARY KEY CLUSTERED
,   LotNumber     NVARCHAR(128) NOT NULL
,   ExpirationDt  DATE NULL
,   ProductId     INT NOT NULL REFERENCES Product (Id)
);

CREATE TABLE Epc (
    Id            INT NOT NULL IDENTITY(1,1) PRIMARY KEY CLUSTERED
,   Urn           VARCHAR(128) NOT NULL
,   DispositionId INT NULL REFERENCES Disposition (Id)
,   ParentId      INT NULL REFERENCES Epc (Id)
,   BizLocationId INT NULL REFERENCES [Location] (Id)
,   ReadPointId   INT NULL REFERENCES [Location] (Id)
,   ProductId     INT NULL REFERENCES Product (Id)
,   LotId         INT NULL REFERENCES Lot (Id)
,   LastUpdate    DATETIMEOFFSET(7) NULL
);
GO


PRINT 'Populating: Disposition';
DECLARE
  @active INT = 1
, @destroyed INT = 2
, @inProgress INT = 3
, @inTransit INT = 4
, @recalled INT = 5
, @retailSold INT = 6
, @returned INT = 7
;
INSERT Disposition VALUES
 (@active,     'urn:epcglobal:cbv:disp:active',       N'active')
,(@destroyed,  'urn:epcglobal:cbv:disp:destroyed',    N'destroyed')
,(@inProgress, 'urn:epcglobal:cbv:disp:in_progress',  N'in_progress')
,(@inTransit,  'urn:epcglobal:cbv:disp:in_transit',   N'in_transit')
,(@recalled,   'urn:epcglobal:cbv:disp:recalled',     N'recalled')
,(@retailSold, 'urn:epcglobal:cbv:disp:retail_sold',  N'retail_sold')
,(@returned,   'urn:epcglobal:cbv:disp:returned',     N'returned')
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
INSERT [Location] VALUES
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
INSERT Product VALUES
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
INSERT Lot VALUES
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
INSERT Epc VALUES ('urn:epc:id:sscc:258643.11122233344', @active, null, @jojaWrhse, @jojaWrhse, null, null, @time);
SET @pallet = SCOPE_IDENTITY();

INSERT Epc VALUES ('urn:epc:idpat:sgtin:258643.3704200.1', @active, @pallet, @jojaWrhse, @jojaWrhse, @jojaColaCase, null, @time);
SET @case = SCOPE_IDENTITY();
INSERT Epc VALUES ('urn:epc:idpat:sgtin:258643.3704146.1', @active, @case, @jojaWrhse, @jojaWrhse, @jojaCola, @lot2003a, @time)
                , ('urn:epc:idpat:sgtin:258643.3704146.2', @active, @case, @jojaWrhse, @jojaWrhse, @jojaCola, @lot2003a, @time)
                , ('urn:epc:idpat:sgtin:258643.3704146.3', @active, @case, @jojaWrhse, @jojaWrhse, @jojaCola, @lot2003a, @time)
                , ('urn:epc:idpat:sgtin:258643.3704146.4', @active, @case, @jojaWrhse, @jojaWrhse, @jojaCola, @lot2003a, @time);

INSERT Epc VALUES ('urn:epc:idpat:sgtin:258643.3704200.2', @active, @pallet, @jojaWrhse, @jojaWrhse, @jojaColaCase, null, @time);
SET @case = SCOPE_IDENTITY();
INSERT Epc VALUES ('urn:epc:idpat:sgtin:258643.3704146.11', @active, @case, @jojaWrhse, @jojaWrhse, @jojaCola, @lot2001a, @time)
                , ('urn:epc:idpat:sgtin:258643.3704146.12', @active, @case, @jojaWrhse, @jojaWrhse, @jojaCola, @lot2001a, @time)
                , ('urn:epc:idpat:sgtin:258643.3704146.13', @active, @case, @jojaWrhse, @jojaWrhse, @jojaCola, @lot2001a, @time)
                , ('urn:epc:idpat:sgtin:258643.3704146.14', @active, @case, @jojaWrhse, @jojaWrhse, @jojaCola, @lot2001a, @time);

INSERT Epc VALUES ('urn:epc:idpat:sgtin:258643.4821216.1', @active, @pallet, @jojaWrhse, @jojaWrhse, @jojaDietColaCase, null, @time);
SET @case = SCOPE_IDENTITY();
INSERT Epc VALUES ('urn:epc:idpat:sgtin:258643.4821101.21', @active, @case, @jojaWrhse, @jojaWrhse, @jojaDietCola, @lot2002b, @time)
                , ('urn:epc:idpat:sgtin:258643.4821101.22', @active, @case, @jojaWrhse, @jojaWrhse, @jojaDietCola, @lot2002b, @time)
                , ('urn:epc:idpat:sgtin:258643.4821101.23', @active, @case, @jojaWrhse, @jojaWrhse, @jojaDietCola, @lot2002b, @time)
                , ('urn:epc:idpat:sgtin:258643.4821101.24', @active, @case, @jojaWrhse, @jojaWrhse, @jojaDietCola, @lot2002b, @time);




--,   Urn           VARCHAR(128) NOT NULL
--,   DispositionId INT NULL REFERENCES Disposition (Id)
--,   ParentId      INT NULL REFERENCES Epc (Id)
--,   BizLocationId INT NULL REFERENCES [Location] (Id)
--,   ReadPointId   INT NULL REFERENCES [Location] (Id)
--,   ProductId     INT NULL REFERENCES Product (Id)
--,   LotId         INT NULL REFERENCES Lot (Id)
--,   LastUpdate    DATETIMEOFFSET(7) NULL
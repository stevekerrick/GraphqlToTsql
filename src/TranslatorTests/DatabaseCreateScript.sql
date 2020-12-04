USE [master]
GO

DROP DATABASE IF EXISTS [GraphqlToTsqlTests]
GO

CREATE DATABASE [GraphqlToTsqlTests]
GO

USE [GraphqlToTsqlTests]
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
);

CREATE TABLE Product (
    Id            INT NOT NULL PRIMARY KEY CLUSTERED
,   Urn           VARCHAR(128) NOT NULL
,   [Name]        NVARCHAR(128) NOT NULL
);

CREATE TABLE Lot (
    Id            INT NOT NULL PRIMARY KEY CLUSTERED
,   ProductId     INT NOT NULL REFERENCES Product (Id)
,   LocationId    INT NULL REFERENCES [Location] (Id)
,   LotNumber     NVARCHAR(128) NOT NULL
,   ExpirationDt  DATE NULL
);

CREATE TABLE Epc (
    Id            INT NOT NULL PRIMARY KEY CLUSTERED
,   Urn           VARCHAR(128) NOT NULL
,   ParentId      INT NULL REFERENCES Epc (Id)
,   DispositionId INT NULL REFERENCES Disposition (Id)
,   BizLocationId INT NULL REFERENCES [Location] (Id)
,   ReadPointId   INT NULL REFERENCES [Location] (Id)
,   ProductId     INT NULL REFERENCES Product (Id)
,   LotId         INT NULL REFERENCES Lot (Id)
,   LastUpdate    DATETIMEOFFSET(7) NULL
);
GO

PRINT 'Populating: Disposition';
INSERT Disposition
 (  Id,  Urn,                                          [Name]) VALUES
 (  1,  'urn:epcglobal:cbv:disp:active',              N'active')
,(  2,  'urn:epcglobal:cbv:disp:container_closed',    N'container_closed')
,(  3,  'urn:epcglobal:cbv:disp:damaged',             N'damaged')
,(  4,  'urn:epcglobal:cbv:disp:destroyed',           N'destroyed')
,(  5,  'urn:epcglobal:cbv:disp:dispensed',           N'dispensed')
,(  6,  'urn:epcglobal:cbv:disp:disposed',            N'disposed')
,(  7,  'urn:epcglobal:cbv:disp:encoded',             N'encoded')
,(  8,  'urn:epcglobal:cbv:disp:expired',             N'expired')
,(  9,  'urn:epcglobal:cbv:disp:in_progress',         N'in_progress')
,( 10,  'urn:epcglobal:cbv:disp:in_transit',          N'in_transit')
,( 11,  'urn:epcglobal:cbv:disp:inactive',            N'inactive')
,( 12,  'urn:epcglobal:cbv:disp:no_pedigree_match',   N'no_pedigree_match')
,( 13,  'urn:epcglobal:cbv:disp:non_sellable_other',  N'non_sellable_other')
,( 14,  'urn:epcglobal:cbv:disp:partially_dispensed', N'partially_dispensed')
,( 15,  'urn:epcglobal:cbv:disp:recalled',            N'recalled')
,( 16,  'urn:epcglobal:cbv:disp:reserved',            N'reserved')
,( 17,  'urn:epcglobal:cbv:disp:retail_sold',         N'retail_sold')
,( 18,  'urn:epcglobal:cbv:disp:returned',            N'returned')
,( 19,  'urn:epcglobal:cbv:disp:stolen',              N'stolen')
,( 20,  'urn:epcglobal:cbv:disp:unknown',             N'unknown');


PRINT 'Populating: Location';
INSERT [Location]
 (  Id,  Urn,                             [Name]) VALUES
 (  1,  'urn:epc:sgln:950110153.000.1',  N'Joja Bottling Plant')
,(  2,  'urn:epc:sgln:950110153.000.2',  N'Joja Bottling Plant, Line 1')
,(  3,  'urn:epc:sgln:950110153.000.3',  N'Joja Bottling Plant, Agg Station')
,(  4,  'urn:epc:sgln:950371221.000.0',  N'Joja Warehouse')
,(  5,  'urn:epc:sgln:950371221.000.1',  N'Joja Mart')
,(  6,  'urn:epc:sgln:211444444.000.0',  N'Silver Foods Warehouse')
,(  7,  'urn:epc:sgln:211500014.000.0',  N'Silver Foods #1')
,(  8,  'urn:epc:sgln:211500029.000.0',  N'Silver Foods #2')
,(  9,  'urn:epc:sgln:300100000.000.0',  N'Happy Freight Service');


PRINT 'Populating: Product';
INSERT Product
 (  Id,  Urn,                             [Name]) VALUES
 (  1,  'urn:epc:sgln:950110153.000.1',  N'Joja Bottling Plant')


GO
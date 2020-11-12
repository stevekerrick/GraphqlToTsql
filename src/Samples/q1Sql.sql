SELECT
  --code
  JSON_QUERY ((
    SELECT
      c1.PureIdentity
      --code.product
    , JSON_QUERY ((
      SELECT
        p.ProductName
      from Product p
      where p.ProductID = c1.ProductID
      for json path, include_null_values, WITHOUT_ARRAY_WRAPPER)) AS product
    FROM Code c1
    where c1.CodeID = 617275
    FOR JSON path, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER)) AS code

--  --codes
--, JSON_QUERY ((
--    SELECT TOP 10
--      c2.PureIdentity
--    FROM Code c2
--    FOR JSON path, INCLUDE_NULL_VALUES)) AS codes
       
FOR JSON path, INCLUDE_NULL_VALUES




declare @codeSet table (
    CodeID int not null
,   ProductID int not null
,   PureIdentity VARCHAR(128) not null
);

insert into @codeSet
select CodeID, ProductID, PureIdentity
from Code
where CodeID = 606843;

-----

declare @productSet table (
    ProductID int not null
,   ProductName nvarchar(200) null
);

insert into @productSet
select Product.ProductID, Product.ProductName
from Product
join @codeSet cs on cs.ProductID = Product.ProductID;

-----

declare @productRefSet table (
    ProductReferenceID int not null
,   ProductID int not null
,   Uri varchar(256) not null
)

insert into @productRefSet
select ProductReference.ProductReferenceID, ProductReference.ProductID, ProductReference.URI
from ProductReference
join @productSet p on ProductReference.ProductID = p.ProductID;


----- wrap up ProductReferences

declare @productRefJsonSet table (
    ProductID int not null
,   productRefJson nvarchar(max)
);
insert into @productRefJsonSet
select
    ProductID
,   (select Uri FOR JSON path, INCLUDE_NULL_VALUES) as prJson
from @productRefSet prSet;

--select * from @productRefJsonSet


----- wrap up Products

declare @productJsonSet table (
    ProductID int not null
,   productJson nvarchar(max)
)
insert @productJsonSet
select
    p.ProductId
,   (select 
        p.ProductName
     ,  json_query(productRefJson) as productReferences
        FOR JSON path, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER) as prJson
from @productSet p
inner join @productRefJsonSet pr ON p.ProductID = pr.ProductID

--select * from @productJsonSet


--- wrap up Codes

declare @codeJsonSet table (
    CodeID int not null
,   codeJson nvarchar(max)
)
insert @codeJsonSet
select
    c.CodeID
,   (select 
        c.CodeID
     ,  c.ProductID
     ,  c.PureIdentity
     ,  json_query(productJson) as product
        FOR JSON path, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER) as pJson
from @codeSet c
inner join @productJsonSet p ON c.ProductID = p.ProductID

select * from @codeJsonSet


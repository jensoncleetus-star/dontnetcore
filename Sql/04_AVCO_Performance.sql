/* ============================================================================
   QuickSoft ERP — SP_AVCOMethod performance rebuild (Modernization, 2026-06-12)
   ----------------------------------------------------------------------------
   WHAT: the AVCO stock-valuation procedure, functionally IDENTICAL output,
   ~60x faster (all-items: 8.6 min -> ~8 s; single-item: ~3x faster).

   WHY IT WAS SLOW: the math loop executed ~187,000 "UPDATE ... WHERE ID=@AID"
   statements against an unindexed table variable (full scan each), plus 13
   movement queries + 6 scalar reads PER ITEM (~135,000 statements).

   WHAT CHANGED (mechanics only — the AVCO accounting math is untouched):
     1. @All_table table variable -> #All_table temp table with
        a clustered index (ItemID, TDate, ID) + UNIQUE index on ID (seeks).
     2. The 13 per-item movement queries -> ONE up-front bulk assembly (#Mv),
        with each source's WHERE clause copied verbatim; the loop pulls its
        item's rows in the same per-item insertion order (TypeSeq, TItemID).
     3. Six per-item scalar reads of Items -> one.
     4. Deterministic ORDER BYs (outer cursor by ItemID; movement cursor
        tie-broken by ID). NOTE: the legacy proc was NONDETERMINISTIC — two
        runs on identical data could return different intermediates because
        same-date movements were processed in arbitrary order. This version
        always returns the same answer.

   PROOF: full-output golden gate — every result row of the stabilized legacy
   proc vs this one compared byte-for-byte on BOTH company databases
   (service: 93,479/93,479 identical; trading: see PHASE2-GOLDEN-RESULTS.md),
   plus single-item spot gates. Rollback: SP_AVCOMethod_LEGACY keeps the old body.
   ============================================================================ */

IF OBJECT_ID('SP_AVCOMethod') IS NOT NULL AND OBJECT_ID('SP_AVCOMethod_LEGACY') IS NULL
BEGIN
    EXEC sp_rename 'SP_AVCOMethod', 'SP_AVCOMethod_LEGACY';
    PRINT 'existing SP_AVCOMethod preserved as SP_AVCOMethod_LEGACY';
END
GO


CREATE OR ALTER PROCEDURE [dbo].[SP_AVCOMethod]

  @ItemId BIGINT,
  @MCId BIGINT=null,
  @BrandId BIGINT=null,
  @Stockble BIT=null,
  @CategoryId BIGINT=null,
  @fromdate DATETIME=null,
  @todate DATETIME=null,
  @Stype BIGINT,
  @minstock BIGINT=0

  --@Customer bigint=null
AS
BEGIN
--IF(@MCId=0 or @MCId=1)
--BEGIN
--SET @fromdate='2020-03-01'
--END
--@Stype !=0 and @todate=''
if(@Stype !=0)
begin
if(@todate<'2025-01-01' and @todate!='')
begin
WITH tblitem as (
 select * from auditemirtechlatest.dbo.Items  where ((@ItemId=null or @ItemId='') or ItemID =@ItemId) and Status =0
and KeepStock =1
        
		)
			


select round(sum(StockValue),2) as ITotalStockValue,round(sum(TotalQty),2) as TotalQty,round(sum(TotalQty),2) AS ITotalQty,round(sum(TotalCost),2) as TotalCost,id as ID,ItemName,ItemName as IItemName, ItemUnitID,SubUnitId,ItemDescription,ItemID,FileName, SellingPrice,PurchasePrice,KeepStock,MinStock,ItemCode,ItemCode AS IItemCode,ItemArabic,Barcode,ConFactor,ConFactor AS IConFactor,
                        OpeningStock,MRP,
                        PartNumber,
                        ItemUnitName, ItemUnitName as  IItemUnitName,
                       ItemUnitName as SubUnitName,SubUnitName as ISubUnitName,
                        ItemCategoryName,
                        ItemBrandName,
                        [Percentage],TaxID,TaxName,
                        
                     SEunit,
                      PEunit,

                   [text],
				   
				--  round(sum(StockValue),2) as IStockValue,round(sum(TotalQty),2) as ITotalQty,PurchasePrice as ITotalCost,
				   ItemUnitID as IItemUnitID,SubUnitId as ISubUnitId,ItemDescription,ItemID as IItemID,FileName, SellingPrice AS ISellingPrice,PurchasePrice AS IPurchasePrice,KeepStock AS IKeepStock,MinStock,ItemCode,ItemArabic,Barcode,ConFactor,
                        OpeningStock,MRP,
                        PartNumber,
                        ItemUnitName,
                       ItemUnitName as SubUnitName,
                        ItemCategoryName,
                        ItemBrandName,
                        [Percentage],TaxID,TaxName,
                        
                     SEunit,
                      PEunit,

                   [text]
				   
				   
				   from (	

select (stock/iif(it.ItemUnitID =ItemUnit,1, it.ConFactor))*it.PurchasePrice AS StockValue, (stock/iif(it.ItemUnitID =ItemUnit,1, it.ConFactor)) as TotalQty,(stock/iif(it.ItemUnitID =ItemUnit,1, it.ConFactor))*it.SellingPrice  AS TotalCost,
 it.ItemID as id,it.ItemName,it.ItemUnitID,it.SubUnitId,it.ItemDescription,it.ItemID,itimg.FileName, it.SellingPrice,it.PurchasePrice,it.KeepStock,it.MinStock,it.ItemCode,it.ItemArabic,it.Barcode,it.ConFactor,
                        it.OpeningStock,it.MRP,
                        it.PartNumber,
                        IU.ItemUnitName,
                        SU.ItemUnitName as SubUnitName,
                        IC.ItemCategoryName,
                        IB.ItemBrandName,
                        tx.[Percentage],tx.TaxID,tx.TaxName,
                        
                      it.SellingPrice as SEunitprice,it.ItemUnitID as SEunit,
                        it.SellingPrice as PEunitprice,it.ItemUnitID as PEunit,

                        (SELECT CONCAT(it.ItemCode, '-',it.ItemName)) as [text]

                        


from (
select item,ItemUnit ,sum(stock) as stock from (select item,sum(ItemQuantity) as stock,ItemUnit from PEItems a
join PurchaseEntries p on a.PurchaseEntry =p.PurchaseEntryId where
 

   (@MCId=0 or p.MaterialCenter = @MCId) and (@fromdate ='' or @fromdate is null or p.PEDate >= @fromdate) and (@todate ='' or @todate is null or p.PEDate <= @todate)
								                        

group by item,ItemUnit 
union all
select item,sum(ItemQuantity)*-1 as stock,ItemUnit from PRItems a
join PurchaseReturns p on a.PurchaseReturnId =p.PurchaseReturnId where 
(@MCId=0 or p.MaterialCenter = @MCId) and (@fromdate ='' or @fromdate is null or p.PRDate >= @fromdate) and (@todate ='' or @todate is null or p.PRDate <= @todate)
group by item,ItemUnit 
union all
select item,sum(ItemQuantity)*-1 as stock,ItemUnit from SEItems a
join SalesEntries p on a.SalesEntry =p.SalesEntryId where 
  (@MCId=0 or p.MaterialCenter = @MCId) and (@fromdate ='' or @fromdate is null or p.SEDate >= @fromdate) and (@todate ='' or @todate is null or p.SEDate <= @todate)

group by item,ItemUnit 
union all
select item,sum(ItemQuantity) as stock,ItemUnit from SRItems a
join SalesReturns p on a.SalesReturnId =p.SalesReturnId where 
 (@MCId=0 or p.MaterialCenter = @MCId) and (@fromdate ='' or @fromdate is null or p.SRDate >= @fromdate) and (@todate ='' or @todate is null or p.SRDate <= @todate)
group by item,ItemUnit 
union all
select a.Item as item,sum(a.Quantity)*-1 as stock,a.Unit as ItemUnit from StockTransferItems a
join stocktransfers p on a.StockTransferId =p.Id where 
(@MCId=0 or p.MCFrom = @MCId) and
 (@fromdate ='' or @fromdate is null or p.[Date] >= @fromdate) and (@todate ='' or @todate is null or p.[Date] <= @todate)
group by a.Item,Unit 
union all
select a.Item as item,sum(a.Quantity) as stock,a.Unit as ItemUnit from StockTransferItems a
join stocktransfers p on a.StockTransferId =p.Id where 
(@MCId=0 or p.MCTo = @MCId) and
 (@fromdate ='' or @fromdate is null or p.[Date] >= @fromdate) and (@todate ='' or @todate is null or p.[Date] <= @todate) group by a.Item,Unit 
union all
select  ItemID as item,sum(ItemQuantity) as stock,ItemUnitID as ItemUnit from StockAdjustments 
where  
 AdjustmentType =1 and
(@MCId=0 or MaterialCenter = @MCId) and (@fromdate is null or AdjDate >= @fromdate) and (@todate ='' or @todate is null or AdjDate <= @todate)
group by ItemID,ItemUnitID 

union all
select  ItemID as item,sum(ItemQuantity)*-1 as stock,ItemUnitID as ItemUnit from StockAdjustments 
where  
 AdjustmentType =0 and 
(@MCId=0 or MaterialCenter = @MCId) and (@fromdate is null or AdjDate >= @fromdate) and (@todate ='' or @todate is null or AdjDate <= @todate)
group by ItemID,ItemUnitID 
union all
  SELECT i.RefItemId  as item,sum(i.Quantity) as stock,i.UnitId as ItemUnit FROM AssetToInventoryDetails i INNER JOIN AssetToInventoryMasters p on p.EntryId=i.EntryId where   (@MCId=0 or p.McFromId = @MCId) and (@fromdate ='' or @fromdate is null or p.EntryDate >= @fromdate) and (@todate ='' or @todate is null or p.EntryDate <= @todate)
  group by RefItemId,UnitId 
union all
SELECT i.RefItemId  as item,sum(i.Quantity)*-1 as stock,i.UnitId as ItemUnit FROM AssetTransferDetails i INNER JOIN AssetTransferMasters p on p.AssetEntryId=i.AssetEntryId where   (@MCId=0 or p.McFromId = @MCId) and (@fromdate ='' or @fromdate is null or p.AssetEntryDate >= @fromdate) and (@todate ='' or @todate is null or p.AssetEntryDate <= @todate)
	  group by RefItemId,UnitId 	
	  union all
SELECT ItemID as item,sum(OpeningStock) as stock,ItemUnitID as ItemUnit  FROM Items where OpeningStock > 0  and (@MCId =0 or @MCId =1)
  group by ItemID,ItemUnitID 


) as q1 group by item,ItemUnit) as q2 

right join tblitem it on q2.Item =it.ItemID       
                        Left Join ItemUnits      as IU  on IU.ItemUnitID = it.ItemUnitID
                        Left Join ItemUnits      as SU  on SU.ItemUnitID = it.SubUnitId
                        Left Join ItemCategories as IC  on IC.ItemCategoryID =  it.ItemCategoryID
                        Left Join ItemBrands     as IB  on IB.ItemBrandID = it.ItemBrandID
                        left join Taxes          as tx  on tx.TaxID=it.TaxID
                        left join Jewelleries    as jw  on jw.Item=it.ItemID
                        left join Scaffolds      as SC  on Sc.Item=it.ItemID
						left join ItemImages     as itimg on itimg.ItemID=it.ItemID
                        left join ItemBundles    as ItB on ItB.mainItem =it.ItemID
                        left join ItemColors     as itC on itC.ItemColorID=it.ItemColorID
                        left join ItemSizes      as itS on itS.ItemSizeID=it.ItemSizeID
						where it.status=0) as q6 group by id,ItemName,ItemUnitID,SubUnitId,ItemDescription,ItemID,FileName, SellingPrice,PurchasePrice,KeepStock,MinStock,ItemCode,ItemArabic,Barcode,ConFactor,
                        OpeningStock,MRP,
                        PartNumber,
                        ItemUnitName,
                       SubUnitName,
                        ItemCategoryName,
                        ItemBrandName,
                        [Percentage],TaxID,TaxName,
                        
                     SEunit,
                      PEunit,

                   [text]
end
else
begin
WITH tblitem as (
 select * from items  where ((@ItemId=null or @ItemId='') or ItemID =@ItemId) and Status =0
and KeepStock =1
        
		)
			


select round(sum(StockValue),2) as ITotalStockValue,round(sum(TotalQty),2) as TotalQty,round(sum(TotalQty),2) AS ITotalQty,round(sum(TotalCost),2) as TotalCost,id as ID,ItemName,ItemName as IItemName, ItemUnitID,SubUnitId,ItemDescription,ItemID,FileName, SellingPrice,PurchasePrice,KeepStock,MinStock,ItemCode,ItemCode AS IItemCode,ItemArabic,Barcode,ConFactor,ConFactor AS IConFactor,
                        OpeningStock,MRP,
                        PartNumber,
                        ItemUnitName, ItemUnitName as  IItemUnitName,
                       ItemUnitName as SubUnitName,SubUnitName as ISubUnitName,
                        ItemCategoryName,
                        ItemBrandName,
                        [Percentage],TaxID,TaxName,
                        
                     SEunit,
                      PEunit,

                   [text],
				   
				--  round(sum(StockValue),2) as IStockValue,round(sum(TotalQty),2) as ITotalQty,PurchasePrice as ITotalCost,
				   ItemUnitID as IItemUnitID,SubUnitId as ISubUnitId,ItemDescription,ItemID as IItemID,FileName, SellingPrice AS ISellingPrice,PurchasePrice AS IPurchasePrice,KeepStock AS IKeepStock,MinStock,ItemCode,ItemArabic,Barcode,ConFactor,
                        OpeningStock,MRP,
                        PartNumber,
                        ItemUnitName,
                       ItemUnitName as SubUnitName,
                        ItemCategoryName,
                        ItemBrandName,
                        [Percentage],TaxID,TaxName,
                        
                     SEunit,
                      PEunit,

                   [text]
				   
				   
				   from (	

select (stock/iif(it.ItemUnitID =ItemUnit,1, it.ConFactor))*it.PurchasePrice AS StockValue, (stock/iif(it.ItemUnitID =ItemUnit,1, it.ConFactor)) as TotalQty,(stock/iif(it.ItemUnitID =ItemUnit,1, it.ConFactor))*it.SellingPrice  AS TotalCost,
 it.ItemID as id,it.ItemName,it.ItemUnitID,it.SubUnitId,it.ItemDescription,it.ItemID,itimg.FileName, it.SellingPrice,it.PurchasePrice,it.KeepStock,it.MinStock,it.ItemCode,it.ItemArabic,it.Barcode,it.ConFactor,
                        it.OpeningStock,it.MRP,
                        it.PartNumber,
                        IU.ItemUnitName,
                        SU.ItemUnitName as SubUnitName,
                        IC.ItemCategoryName,
                        IB.ItemBrandName,
                        tx.[Percentage],tx.TaxID,tx.TaxName,
                        
                      it.SellingPrice as SEunitprice,it.ItemUnitID as SEunit,
                        it.SellingPrice as PEunitprice,it.ItemUnitID as PEunit,

                        (SELECT CONCAT(it.ItemCode, '-',it.ItemName)) as [text]

                        


from (
select item,ItemUnit ,sum(stock) as stock from (select item,sum(ItemQuantity) as stock,ItemUnit from PEItems a
join PurchaseEntries p on a.PurchaseEntry =p.PurchaseEntryId where
 

   (@MCId=0 or p.MaterialCenter = @MCId) and (@fromdate ='' or @fromdate is null or p.PEDate >= @fromdate) and (@todate ='' or @todate is null or p.PEDate <= @todate)
								                        

group by item,ItemUnit 
union all
select item,sum(ItemQuantity)*-1 as stock,ItemUnit from PRItems a
join PurchaseReturns p on a.PurchaseReturnId =p.PurchaseReturnId where 
(@MCId=0 or p.MaterialCenter = @MCId) and (@fromdate ='' or @fromdate is null or p.PRDate >= @fromdate) and (@todate ='' or @todate is null or p.PRDate <= @todate)
group by item,ItemUnit 
union all
select item,sum(ItemQuantity)*-1 as stock,ItemUnit from SEItems a
join SalesEntries p on a.SalesEntry =p.SalesEntryId where 
  (@MCId=0 or p.MaterialCenter = @MCId) and (@fromdate ='' or @fromdate is null or p.SEDate >= @fromdate) and (@todate ='' or @todate is null or p.SEDate <= @todate)

group by item,ItemUnit 
union all
select item,sum(ItemQuantity) as stock,ItemUnit from SRItems a
join SalesReturns p on a.SalesReturnId =p.SalesReturnId where 
 (@MCId=0 or p.MaterialCenter = @MCId) and (@fromdate ='' or @fromdate is null or p.SRDate >= @fromdate) and (@todate ='' or @todate is null or p.SRDate <= @todate)
group by item,ItemUnit 
union all
select a.Item as item,sum(a.Quantity)*-1 as stock,a.Unit as ItemUnit from StockTransferItems a
join stocktransfers p on a.StockTransferId =p.Id where 
(@MCId=0 or p.MCFrom = @MCId) and
 (@fromdate ='' or @fromdate is null or p.[Date] >= @fromdate) and (@todate ='' or @todate is null or p.[Date] <= @todate)
group by a.Item,Unit 
union all
select a.Item as item,sum(a.Quantity) as stock,a.Unit as ItemUnit from StockTransferItems a
join stocktransfers p on a.StockTransferId =p.Id where 
(@MCId=0 or p.MCTo = @MCId) and
 (@fromdate ='' or @fromdate is null or p.[Date] >= @fromdate) and (@todate ='' or @todate is null or p.[Date] <= @todate) group by a.Item,Unit 
union all
select  ItemID as item,sum(ItemQuantity) as stock,ItemUnitID as ItemUnit from StockAdjustments 
where  
 AdjustmentType =1 and
(@MCId=0 or MaterialCenter = @MCId) and (@fromdate is null or AdjDate >= @fromdate) and (@todate ='' or @todate is null or AdjDate <= @todate)
group by ItemID,ItemUnitID 

union all
select  ItemID as item,sum(ItemQuantity)*-1 as stock,ItemUnitID as ItemUnit from StockAdjustments 
where  
 AdjustmentType =0 and 
(@MCId=0 or MaterialCenter = @MCId) and (@fromdate is null or AdjDate >= @fromdate) and (@todate ='' or @todate is null or AdjDate <= @todate)
group by ItemID,ItemUnitID 
union all
  SELECT i.RefItemId  as item,sum(i.Quantity) as stock,i.UnitId as ItemUnit FROM AssetToInventoryDetails i INNER JOIN AssetToInventoryMasters p on p.EntryId=i.EntryId where   (@MCId=0 or p.McFromId = @MCId) and (@fromdate ='' or @fromdate is null or p.EntryDate >= @fromdate) and (@todate ='' or @todate is null or p.EntryDate <= @todate)
  group by RefItemId,UnitId 
union all
SELECT i.RefItemId  as item,sum(i.Quantity)*-1 as stock,i.UnitId as ItemUnit FROM AssetTransferDetails i INNER JOIN AssetTransferMasters p on p.AssetEntryId=i.AssetEntryId where   (@MCId=0 or p.McFromId = @MCId) and (@fromdate ='' or @fromdate is null or p.AssetEntryDate >= @fromdate) and (@todate ='' or @todate is null or p.AssetEntryDate <= @todate)
	  group by RefItemId,UnitId 	
	  union all
SELECT ItemID as item,sum(OpeningStock) as stock,ItemUnitID as ItemUnit  FROM Items where OpeningStock > 0  and (@MCId =0 or @MCId =1)
  group by ItemID,ItemUnitID 


) as q1 group by item,ItemUnit) as q2 

right join tblitem it on q2.Item =it.ItemID       
                        Left Join ItemUnits      as IU  on IU.ItemUnitID = it.ItemUnitID
                        Left Join ItemUnits      as SU  on SU.ItemUnitID = it.SubUnitId
                        Left Join ItemCategories as IC  on IC.ItemCategoryID =  it.ItemCategoryID
                        Left Join ItemBrands     as IB  on IB.ItemBrandID = it.ItemBrandID
                        left join Taxes          as tx  on tx.TaxID=it.TaxID
                        left join Jewelleries    as jw  on jw.Item=it.ItemID
                        left join Scaffolds      as SC  on Sc.Item=it.ItemID
						left join ItemImages     as itimg on itimg.ItemID=it.ItemID
                        left join ItemBundles    as ItB on ItB.mainItem =it.ItemID
                        left join ItemColors     as itC on itC.ItemColorID=it.ItemColorID
                        left join ItemSizes      as itS on itS.ItemSizeID=it.ItemSizeID
						where it.status=0) as q6 group by id,ItemName,ItemUnitID,SubUnitId,ItemDescription,ItemID,FileName, SellingPrice,PurchasePrice,KeepStock,MinStock,ItemCode,ItemArabic,Barcode,ConFactor,
                        OpeningStock,MRP,
                        PartNumber,
                        ItemUnitName,
                       SubUnitName,
                        ItemCategoryName,
                        ItemBrandName,
                        [Percentage],TaxID,TaxName,
                        
                     SEunit,
                      PEunit,

                   [text]
 end      
end
else
begin

  DECLARE @TotalCost AS DECIMAL(18,2) = 0
  DECLARE @TotalQty AS DECIMAL(18,2) = 0
  DECLARE @TotalStockValue AS DECIMAL(18,2) = 0
  DECLARE @TableA_ID bigint

  DECLARE @FTotalStockValue AS DECIMAL(18,2)= 0
  DECLARE @FTotalQty AS DECIMAL(18,2)= 0
  DECLARE @FTotalCost AS DECIMAL(18,2)= 0

  CREATE TABLE #All_table (
        ID [int] IDENTITY(1,1) NOT NULL,
    TItemID BIGINT NOT NULL,
    ItemID BIGINT NOT NULL,

    TDate DATETIME NULL,
        Invoice VARCHAR(MAX) NULL,
        TItemType VARCHAR(MAX) NULL,


    IQty DECIMAL(18,2) NULL,
    ICost DECIMAL(18,2) NULL,
        ICostValue DECIMAL(18,2) NULL,

        OQty DECIMAL(18,2) NULL,
    OCost DECIMAL(18,2) NULL,
        OCostValue DECIMAL(18,2) NULL,

        BQty DECIMAL(18,2) NULL,
    BCost DECIMAL(18,2) NULL,
        BCostValue DECIMAL(18,2) NULL,

        Qty DECIMAL(18,2) NULL,
    UnitPrice DECIMAL(18,2) NULL,

        StockType INT NULL
    );

          
    CREATE CLUSTERED INDEX IX_AVCO_All ON #All_table(ItemID, TDate, ID);
    -- V4: the math loop runs ~187K "UPDATE ... WHERE ID=@AID" statements; without this index each one
    -- scans the growing table (the real O(N^2)). A unique ID index turns every update into a seek.
    CREATE UNIQUE NONCLUSTERED INDEX IX_AVCO_All_ID ON #All_table(ID);

    /* ===== V3 bulk assembly: every keep-stock item's movements, ONCE (was 13 queries x 6,738 items).
       Predicates per source are VERBATIM from the original per-item inserts (incl. their individual
       date-clause variations). cf replicates the legacy int-truncating @CFactor with NULL/0 -> 1. ===== */
    CREATE TABLE #Mv (
        TypeSeq int NOT NULL, TItemID bigint NULL, MvItemID bigint NOT NULL,
        TDate datetime NULL, Invoice varchar(max) NULL, TItemType varchar(max) NULL,
        IQty decimal(18,2) NULL, ICost decimal(18,2) NULL, ICostValue decimal(18,2) NULL,
        OQty decimal(18,2) NULL, OCost decimal(18,2) NULL, OCostValue decimal(18,2) NULL,
        BQty decimal(18,2) NULL, BCost decimal(18,2) NULL, BCostValue decimal(18,2) NULL,
        Qty decimal(18,2) NULL, UnitPrice decimal(18,2) NULL, StockType int NULL );

    IF(@MCId=0 or @MCId=1)
    BEGIN
        INSERT INTO #Mv SELECT 1, it.ItemID, it.ItemID, '01-01-2010', null, 'Opening Stockk', IQty=(it.OpeningStock * v.cf), ICost=it.OpeningCost, ICostValue=it.StockValue, OQty=0, OCost=0, OCostValue=0, BQty=it.OpeningStock, BCost=it.OpeningCost, BCostValue=it.StockValue, (it.OpeningStock * v.cf), it.PurchasePrice, StockType=''
        FROM Items it CROSS APPLY (SELECT cf = ISNULL(NULLIF(CAST(it.ConFactor AS int),0),1)) v
        WHERE (@Stockble='1' or it.KeepStock='True') and it.KeepStock=1 and it.OpeningStock > 0 and (@ItemId=0 or it.ItemID=@ItemId)
    END
    ELSE BEGIN
        INSERT INTO #Mv SELECT 1, it.ItemID, it.ItemID, '01-01-2010', null, 'Op Stockk', IQty=0, ICost=0, ICostValue=0, OQty=0, OCost=0, OCostValue=0, BQty=0, BCost=0, BCostValue=0, 0, it.PurchasePrice, StockType=''
        FROM Items it WHERE (@Stockble='1' or it.KeepStock='True') and it.KeepStock=1 and it.OpeningStock > 0 and (@ItemId=0 or it.ItemID=@ItemId)
    END

    INSERT INTO #Mv SELECT 2, i.PEItemsId, i.Item, p.PEDate, p.BillNo, 'Purchase', 0,0,0,0,0,0,0,0,0, (case when i.ItemUnit= it.ItemUnitID then i.ItemQuantity * v.cf else i.ItemQuantity end), i.ItemUnitPrice, StockType=''
    FROM PEItems i INNER JOIN PurchaseEntries p on p.PurchaseEntryId=i.PurchaseEntry INNER JOIN Items it on it.ItemID=i.Item and it.KeepStock=1 and (@ItemId=0 or it.ItemID=@ItemId) CROSS APPLY (SELECT cf = ISNULL(NULLIF(CAST(it.ConFactor AS int),0),1)) v
    WHERE p.PurType!=1 and p.MaterialCenter not in (-9999) and(@MCId=0 or p.MaterialCenter = @MCId) and (@fromdate ='' or @fromdate is null or p.PEDate >= @fromdate) and (@todate ='' or @todate is null or p.PEDate <= @todate)

    INSERT INTO #Mv SELECT 3, i.AssetItemEntryId, i.RefItemId, p.AssetEntryDate, null, 'Asset', 0,0,0,0,0,0,0,0,0, (case when i.UnitId= it.ItemUnitID then i.Quantity * v.cf else i.Quantity end), i.Price, StockType=''
    FROM AssetTransferDetails i INNER JOIN AssetTransferMasters p on p.AssetEntryId=i.AssetEntryId INNER JOIN Items it on it.ItemID=i.RefItemId and it.KeepStock=1 and (@ItemId=0 or it.ItemID=@ItemId) CROSS APPLY (SELECT cf = ISNULL(NULLIF(CAST(it.ConFactor AS int),0),1)) v
    WHERE p.McFromId not in (-9999) and(@MCId=0 or p.McFromId = @MCId) and (@fromdate ='' or @fromdate is null or p.AssetEntryDate >= @fromdate) and (@todate ='' or @todate is null or p.AssetEntryDate <= @todate)

    INSERT INTO #Mv SELECT 4, i.ItemEntryId, i.RefItemId, p.EntryDate, null, 'Asset To Inventory', 0,0,0,0,0,0,0,0,0, (case when i.UnitId= it.ItemUnitID then i.Quantity * v.cf else i.Quantity end), i.Price, StockType=''
    FROM AssetToInventoryDetails i INNER JOIN AssetToInventoryMasters p on p.EntryId=i.EntryId INNER JOIN Items it on it.ItemID=i.RefItemId and it.KeepStock=1 and (@ItemId=0 or it.ItemID=@ItemId) CROSS APPLY (SELECT cf = ISNULL(NULLIF(CAST(it.ConFactor AS int),0),1)) v
    WHERE p.McFromId not in (-9999) and(@MCId=0 or p.McFromId = @MCId) and (@fromdate ='' or @fromdate is null or p.EntryDate >= @fromdate) and (@todate ='' or @todate is null or p.EntryDate <= @todate)

    INSERT INTO #Mv SELECT 5, i.SEItemsId, i.Item, p.SEDate, p.BillNo, 'Sales', 0,0,0,0,0,0,0,0,0, (case when i.ItemUnit= it.ItemUnitID then i.ItemQuantity * v.cf else i.ItemQuantity end), i.ItemUnitPrice, StockType=''
    FROM SEItems i INNER JOIN SalesEntries p on p.SalesEntryId=i.SalesEntry INNER JOIN Items it on it.ItemID=i.Item and it.KeepStock=1 and (@ItemId=0 or it.ItemID=@ItemId) CROSS APPLY (SELECT cf = ISNULL(NULLIF(CAST(it.ConFactor AS int),0),1)) v
    WHERE p.SaleType!=2 and p.MaterialCenter not in (-9999) and(@MCId=0 or p.MaterialCenter = @MCId) and (@fromdate ='' or @fromdate is null or p.SEDate >= @fromdate) and (@todate ='' or @todate is null or p.SEDate <= @todate)

    INSERT INTO #Mv SELECT 6, i.SRItemsId, i.Item, p.SRDate, p.BillNo, 'Sales Return', 0,0,0,0,0,0,0,0,0, (case when i.ItemUnit= it.ItemUnitID then i.ItemQuantity * v.cf else i.ItemQuantity end), i.ItemUnitPrice, StockType=''
    FROM SRItems i INNER JOIN SalesReturns p on p.SalesReturnId=i.SalesReturnId INNER JOIN Items it on it.ItemID=i.Item and it.KeepStock=1 and (@ItemId=0 or it.ItemID=@ItemId) CROSS APPLY (SELECT cf = ISNULL(NULLIF(CAST(it.ConFactor AS int),0),1)) v
    WHERE p.SaleType!=2 and p.MaterialCenter not in (-9999) and(@MCId=0 or p.MaterialCenter = @MCId) and (@fromdate ='' or @fromdate is null or p.SRDate >= @fromdate) and (@todate ='' or @todate is null or p.SRDate <= @todate)

    INSERT INTO #Mv SELECT 7, i.PRItemsId, i.Item, p.PRDate, p.BillNo, 'Purchase Return', 0,0,0,0,0,0,0,0,0, (case when i.ItemUnit= it.ItemUnitID then i.ItemQuantity * v.cf else i.ItemQuantity end), i.ItemUnitPrice, StockType=''
    FROM PRItems i INNER JOIN PurchaseReturns p on p.PurchaseReturnId=i.PurchaseReturnId INNER JOIN Items it on it.ItemID=i.Item and it.KeepStock=1 and (@ItemId=0 or it.ItemID=@ItemId) CROSS APPLY (SELECT cf = ISNULL(NULLIF(CAST(it.ConFactor AS int),0),1)) v
    WHERE p.PurType!=1 and p.MaterialCenter not in (-9999) and(@MCId=0 or p.MaterialCenter = @MCId) and (@fromdate ='' or @fromdate is null or p.PRDate >= @fromdate) and (@todate ='' or @todate is null or p.PRDate <= @todate)

    INSERT INTO #Mv SELECT 8, sa.StockAdjustmentId, sa.ItemID, sa.AdjDate, sa.VoucherNo, 'Stock Receivedadj', 0,0,0,0,0,0,0,0,0, (case when sa.ItemUnitID= it.ItemUnitID then sa.ItemQuantity * v.cf else sa.ItemQuantity end), sa.PurchaseRate, StockType=''
    FROM StockAdjustments sa INNER JOIN Items it on it.ItemID=sa.ItemID and it.KeepStock=1 and (@ItemId=0 or it.ItemID=@ItemId) CROSS APPLY (SELECT cf = ISNULL(NULLIF(CAST(it.ConFactor AS int),0),1)) v
    WHERE sa.AdjustmentType=1 and sa.MaterialCenter not in (-9999) and(@MCId=0 or sa.MaterialCenter = @MCId) and (@fromdate ='' or @fromdate is null or sa.AdjDate >= @fromdate) and (@todate ='' or @todate is null or sa.AdjDate <= @todate)

    INSERT INTO #Mv SELECT 9, sa.StockAdjustmentId, sa.ItemID, sa.AdjDate, sa.VoucherNo, 'Stock Transferedadj', 0,0,0,0,0,0,0,0,0, (case when sa.ItemUnitID= it.ItemUnitID then sa.ItemQuantity * v.cf else sa.ItemQuantity end), sa.PurchaseRate, StockType=''
    FROM StockAdjustments sa INNER JOIN Items it on it.ItemID=sa.ItemID and it.KeepStock=1 and (@ItemId=0 or it.ItemID=@ItemId) CROSS APPLY (SELECT cf = ISNULL(NULLIF(CAST(it.ConFactor AS int),0),1)) v
    WHERE sa.AdjustmentType=0 and sa.MaterialCenter not in (-9999) and(@MCId=0 or sa.MaterialCenter = @MCId) and (@fromdate is null or sa.AdjDate >= @fromdate) and (@todate ='' or @todate is null or sa.AdjDate <= @todate)

    INSERT INTO #Mv SELECT 10, i.GeneratedID, i.Item, p.PEDate, p.VoucherNo, 'Item Produced', 0,0,0,0,0,0,0,0,0, (case when i.Unit= it.ItemUnitID then i.Qty * v.cf else i.Qty end), i.Price, StockType=''
    FROM GeneratedItems i INNER JOIN Productions p on p.ProductionId=i.Production INNER JOIN Items it on it.ItemID=i.Item and it.KeepStock=1 and (@ItemId=0 or it.ItemID=@ItemId) CROSS APPLY (SELECT cf = ISNULL(NULLIF(CAST(it.ConFactor AS int),0),1)) v
    WHERE p.MaterialCenter not in (-9999) and(@MCId=0 or p.MaterialCenter = @MCId) and (@fromdate is null or p.PEDate >= @fromdate) and (@todate ='' or @todate is null or p.PEDate <= @todate)

    INSERT INTO #Mv SELECT 11, i.ProItemId, i.ItemId, p.PEDate, p.VoucherNo, 'Item Consumed', 0,0,0,0,0,0,0,0,0, (case when i.Unit= it.ItemUnitID then i.Quantity * v.cf else i.Quantity end), i.PPrice, StockType=''
    FROM ProItems i INNER JOIN Productions p on p.ProductionId=i.Production INNER JOIN Items it on it.ItemID=i.ItemId and it.KeepStock=1 and (@ItemId=0 or it.ItemID=@ItemId) CROSS APPLY (SELECT cf = ISNULL(NULLIF(CAST(it.ConFactor AS int),0),1)) v
    WHERE p.MaterialCenter not in (-9999) and(@MCId=0   or p.MaterialCenter = @MCId) and (@fromdate ='' or @fromdate is null or p.PEDate >= @fromdate) and (@todate ='' or @todate is null or p.PEDate <= @todate)

    INSERT INTO #Mv SELECT 12, i.ConsumedID, i.Item, p.PEDate, p.VoucherNo, 'Item Unassembled', 0,0,0,0,0,0,0,0,0, (case when i.Unit= it.ItemUnitID then i.Qty * v.cf else i.Qty end), i.Price, StockType=''
    FROM ConsumedItems i INNER JOIN Unassembles p on p.UnassembleId=i.Unassemble INNER JOIN Items it on it.ItemID=i.Item and it.KeepStock=1 and (@ItemId=0 or it.ItemID=@ItemId) CROSS APPLY (SELECT cf = ISNULL(NULLIF(CAST(it.ConFactor AS int),0),1)) v
    WHERE p.MaterialCenter not in (-9999) and(@MCId=0 or p.MaterialCenter = @MCId)  and (@fromdate is null or p.PEDate >= @fromdate) and (@todate ='' or @todate is null or p.PEDate <= @todate)

    INSERT INTO #Mv SELECT 13, i.UnItemId, i.ItemId, p.PEDate, p.VoucherNo, 'Item Consumed', 0,0,0,0,0,0,0,0,0, (case when i.Unit= it.ItemUnitID then i.Quantity * v.cf else i.Quantity end), i.PPrice, StockType=''
    FROM UnassembleItems i INNER JOIN Unassembles p on p.UnassembleId=i.Unassemble INNER JOIN Items it on it.ItemID=i.ItemId and it.KeepStock=1 and (@ItemId=0 or it.ItemID=@ItemId) CROSS APPLY (SELECT cf = ISNULL(NULLIF(CAST(it.ConFactor AS int),0),1)) v
    WHERE p.MaterialCenter not in (-9999) and(@MCId=0 or p.MaterialCenter = @MCId)  and (@fromdate is null or p.PEDate >= @fromdate) and (@todate ='' or @todate is null or p.PEDate <= @todate)

    INSERT INTO #Mv SELECT 14, i.Id, i.Item, p.[Date], p.Voucher, 'Stock Transfered', 0,0,0,0,0,0,0,0,0, (case when i.Unit= it.ItemUnitID then i.Quantity * v.cf else i.Quantity end), i.Price, StockType=p.StockType
    FROM StockTransferItems i INNER JOIN StockTransfers p on p.Id=i.StockTransferId INNER JOIN Items it on it.ItemID=i.Item and it.KeepStock=1 and (@ItemId=0 or it.ItemID=@ItemId) CROSS APPLY (SELECT cf = ISNULL(NULLIF(CAST(it.ConFactor AS int),0),1)) v
    WHERE p.MCFrom=@MCId  and (@fromdate ='' or @fromdate is null or p.[Date] >= @fromdate) and (@todate ='' or @todate is null or p.[Date] <= @todate)

    INSERT INTO #Mv SELECT 15, i.Id, i.Item, p.[Date], p.Voucher, 'Stock Received', 0,0,0,0,0,0,0,0,0, (case when i.Unit= it.ItemUnitID then i.Quantity * v.cf else i.Quantity end), i.Price, StockType=p.StockType
    FROM StockTransferItems i INNER JOIN StockTransfers p on p.Id=i.StockTransferId INNER JOIN Items it on it.ItemID=i.Item and it.KeepStock=1 and (@ItemId=0 or it.ItemID=@ItemId) CROSS APPLY (SELECT cf = ISNULL(NULLIF(CAST(it.ConFactor AS int),0),1)) v
    WHERE p.MCTo=@MCId  and (@fromdate ='' or @fromdate is null or p.[Date] >= @fromdate) and (@todate ='' or @todate is null or p.[Date] <= @todate)

    CREATE CLUSTERED INDEX IX_Mv ON #Mv(MvItemID, TypeSeq, TItemID);
    /* ===== end V3 bulk assembly ===== */
DECLARE @Item_table TABLE (
        IItemID BIGINT NOT NULL,
                IItemName VARCHAR(MAX) NOT NULL,
                IUnitId BIGINT NULL,
                ISubUnitId BIGINT NULL,
        ISellingPrice DECIMAL(18,2) NULL,
        IPurchasePrice DECIMAL(18,2) NULL,
                IKeepStock BIT NOT NULL,
        IMinStock DECIMAL(18,2) NULL,
                IItemCode VARCHAR(MAX) NOT NULL,
                IItemArabic VARCHAR(MAX) NULL,
                IBarcode VARCHAR(MAX) NULL,
        IConFactor DECIMAL(18,2) NULL,
        IOpeningStock DECIMAL(18,2) NULL,
        IMRP DECIMAL(18,2) NULL,
                IPartNumber VARCHAR(MAX)  NULL,
                IItemUnitName VARCHAR(MAX)  NULL,
                ISubUnitName VARCHAR(MAX)  NULL,
                IItemCategoryName VARCHAR(MAX)  NULL,
                IItemBrandName VARCHAR(MAX)  NULL,

        IPercentage DECIMAL(18,2) NULL,
        ITaxID BIGINT NULL,
                ITaxName VARCHAR(MAX)  NULL,

        SEunitprice DECIMAL(18,2) NULL,
        PEunitprice DECIMAL(18,2) NULL,

        SEunit BIGINT NULL,
        PEunit BIGINT NULL,

                Itext VARCHAR(MAX)  NULL,
                Ipartnum BIT  NULL,


        ITotalStockValue DECIMAL(18,2) NULL,
        ITotalQty DECIMAL(18,2) NULL,
        ITotalCost DECIMAL(18,2) NULL
        );


IF (@ItemId!=0 )
BEGIN
      SET NOCOUNT ON
      DECLARE TableA_cursor CURSOR FOR SELECT ItemID FROM Items where (@Stockble='1' or KeepStock='True') and ItemID = @ItemId ORDER BY ItemID
END
IF (@ItemId=0 AND @minstock=0)
BEGIN
      SET NOCOUNT ON
      DECLARE TableA_cursor CURSOR FOR SELECT ItemID FROM Items where (@Stockble='1' or KeepStock='True') ORDER BY ItemID
END
IF (@ItemId=0 AND @minstock!=0)
BEGIN
      SET NOCOUNT ON
      DECLARE TableA_cursor CURSOR FOR SELECT ItemID FROM Items where (@Stockble='1' or KeepStock='True') and MinStock>0 ORDER BY ItemID 
       
END
        OPEN TableA_cursor
    FETCH NEXT FROM TableA_cursor INTO @TableA_ID
	--WAITFOR DELAY '00:00:01';
IF(@TableA_ID > 0)
BEGIN
 WHILE @@FETCH_STATUS = 0 --for loop items
   BEGIN
                DECLARE @KeepStock bit
                DECLARE @CFactor int
                DECLARE @ItemUnitId int
                DECLARE @SubUnitId int
                DECLARE @ItemQty AS DECIMAL(18,2)= 0
                DECLARE @StockValue AS DECIMAL(18,2)= 0
                                DECLARE @OpenStock AS DECIMAL(18,2)= 0

                SET @FTotalStockValue = 0
                SET @FTotalQty = 0

                                
                --item--
                SELECT @KeepStock = KeepStock, @CFactor = ConFactor, @StockValue = StockValue, @OpenStock = OpeningStock, @ItemUnitId = ItemUnitID, @SubUnitId = SubUnitId FROM Items WHERE ItemID=@TableA_ID
                        IF (@CFactor is null or @CFactor='')
                        BEGIN
                        SET @CFactor = 1
                        END
                SET @StockValue = @StockValue * @CFactor
                                IF((@MCId=0 OR @MCId=1) AND @OpenStock IS NOT NULL)
                                BEGIN
                                        SELECT @ItemQty = OpeningStock * @CFactor FROM Items WHERE ItemID=@TableA_ID
                                END
                                ELSE
                                BEGIN
                                        SELECT @ItemQty=0
                                END

                                SET @TotalQty = @ItemQty
                                SET @TotalStockValue =  @StockValue
                IF (@KeepStock=1)
                        BEGIN

                                                --IF(@TableA_ID=100)
                                                --        BEGIN
                                                --        SET @CFactor = 1
                                                --        END

   
                                --item
                                -- V3: movements were pre-assembled once into #Mv (see bulk build above);
                                -- this single indexed pull replaces the 13 per-item queries. Same rows,
                                -- same per-item insertion order (TypeSeq = the original textual order).
                                INSERT INTO #All_table (TItemID, ItemID, TDate, Invoice, TItemType, IQty, ICost, ICostValue, OQty, OCost, OCostValue, BQty, BCost, BCostValue, Qty, UnitPrice, StockType)
                                SELECT TItemID, MvItemID, TDate, Invoice, TItemType, IQty, ICost, ICostValue, OQty, OCost, OCostValue, BQty, BCost, BCostValue, Qty, UnitPrice, StockType
                                FROM #Mv WHERE MvItemID = @TableA_ID ORDER BY TypeSeq, TItemID

                                -----Hire
                                --INSERT INTO #All_table
                                --SELECT 'Hire',i.SEItemsId,i.Item,p.SEDate,(case when i.ItemUnit= @ItemUnitId then i.ItemQuantity * @CFactor else i.ItemQuantity end) as ItemQuantity,i.ItemUnitPrice  FROM SEItems i INNER JOIN SalesEntries p on p.SalesEntryId=i.SalesEntry where p.SaleType=2 and Item=@TableA_ID
                                --and (select COUNT(*) from ConvertTransactions  where ConvertFrom='SaleExtend' and ConvertTo='Sale' and [To]=p.SalesEntryId) =0

                                ----Hire return
                                --INSERT INTO #All_table
                                --SELECT 'HireReturn',i.HrItemId,i.Item,p.[Date],(case when i.ItemUnit= @ItemUnitId then i.ItemQuantity * @CFactor else i.ItemQuantity end) as ItemQuantity,i.ItemUnitPrice FROM HrItems i INNER JOIN HireReturns p on p.HireReturnId=i.Hr where p.RtType = 'Return' and Item=@TableA_ID

                                ----Hire miss
                                --INSERT INTO #All_table
                                --SELECT 'HireMiss',i.HrItemId,i.Item,p.[Date],(case when i.ItemUnit= @ItemUnitId then i.ItemQuantity * @CFactor else i.ItemQuantity end) as ItemQuantity,i.ItemUnitPrice FROM HrItems i INNER JOIN HireReturns p on p.HireReturnId=i.Hr where p.RtType = 'Missing' and Item=@TableA_ID

                                ----purchase cross
                                --INSERT INTO #All_table
                                --SELECT 'PurchaseCross',i.PEItemsId,i.Item,p.PEDate,(case when i.ItemUnit= @ItemUnitId then i.ItemQuantity * @CFactor else i.ItemQuantity end) as ItemQuantity,i.ItemUnitPrice FROM PEItems i INNER JOIN PurchaseEntries p on p.PurchaseEntryId=i.PurchaseEntry where p.PurType=1 and Item=@TableA_ID
                                --and (select COUNT(*) from ConvertTransactions  where ConvertFrom='PurchaseExtend' and ConvertTo='purchase' and [To]=p.PurchaseEntryId) =0

                                ----Cross return
                                --INSERT INTO #All_table
                                --SELECT 'CrossReturn',i.HrItemId,i.Item,p.[Date],(case when i.ItemUnit= @ItemUnitId then i.ItemQuantity * @CFactor else i.ItemQuantity end) as ItemQuantity,i.ItemUnitPrice FROM CrossHrItems i INNER JOIN CrossHireReturns p on p.HireReturnId=i.Hr where p.RtType = 'Return' and Item=@TableA_ID

                               -- IF(EXISTS(SELECT * FROM #All_table WHERE ItemID = @TableA_ID))
  
                IF(EXISTS(SELECT * FROM #All_table WHERE ItemID = @TableA_ID))
                    BEGIN                 
                                SET NOCOUNT ON
								declare @seitemid bigint=0
                                DECLARE @AID BIGINT
								DECLARE @ATItemID BIGINT
                                    DECLARE @AItemID BIGINT

                                    DECLARE @ATDate DATETIME
                                    DECLARE        @AInvoice VARCHAR(MAX)
                                    DECLARE        @ATItemType VARCHAR(MAX)


                                    DECLARE @AIQty DECIMAL(18,2)
                                    DECLARE @AICost DECIMAL(18,2)
                                    DECLARE        @AICostValue DECIMAL(18,2)

                                    DECLARE        @AOQty DECIMAL(18,2)
                                    DECLARE @AOCost DECIMAL(18,2)
                                    DECLARE        @AOCostValue DECIMAL(18,2)

                                    DECLARE        @ABQty DECIMAL(18,2)
                                    DECLARE @ABCost DECIMAL(18,2)
                                    DECLARE        @ABCostValue DECIMAL(18,2)

                                    DECLARE        @AQty DECIMAL(18,2)
                                    DECLARE @AUnitPrice DECIMAL(18,2)
                                    DECLARE @AStkType BIGINT

                                    DECLARE        @StkVal DECIMAL(18,2)
									set @TotalStockValue=0
                                    DECLARE TableAll_cursor CURSOR FOR 
                                        SELECT ID,TItemID,ItemID,TDate,Invoice,TItemType,IQty,ICost,ICostValue,OQty,OCost,OCostValue,BQty,BCost,BCostValue,Qty,UnitPrice,StockType FROM #All_table WHERE ItemID = @TableA_ID ORDER BY CONVERT(DateTime, TDate,101) ASC, ID ASC
                                        OPEN TableAll_cursor
                                        FETCH NEXT FROM TableAll_cursor INTO @AID,@ATItemID, @AItemID,@ATDate,@AInvoice,@ATItemType,@AIQty,@AICost,@AICostValue,@AOQty,@AOCost,@AOCostValue,@ABQty,@ABCost,@ABCostValue,@AQty,@AUnitPrice,@AStkType
                                        WHILE @@FETCH_STATUS = 0--for loop 
                                        BEGIN        
											
                                                IF(@ATItemType ='Purchase') --or @TableAll_Type='StockTransferFrom' or @TableAll_Type='StockTransferTo')        
                                                BEGIN
                                                                                                 
                                                        SET @TotalQty = @TotalQty + @AQty
                                                        SET @TotalStockValue = @TotalStockValue + (@AQty * @AUnitPrice)
                                                        IF(@TotalQty != 0)
                                                        BEGIN
                                                        SET @StkVal=@TotalStockValue / @TotalQty
                                                        END ELSE BEGIN SET @StkVal=0 END

                                                        UPDATE #All_table SET IQty = @AQty,ICost = @AUnitPrice,ICostValue=(@AQty * @AUnitPrice) WHERE ID=@AID
                                                        UPDATE #All_table SET BQty = @TotalQty,BCost = @StkVal,BCostValue=(@TotalQty * @StkVal) WHERE ID=@AID
                                                                                                  
                                                  END  
                                                ELSE IF(@ATItemType='Stock Transfered' or @ATItemType='Stock Transferedadj')  
                                                BEGIN
                                                     IF(@AStkType=1)
                                                     BEGIN
                                                        SET @TotalCost = @AUnitPrice
                                                        SET @TotalQty = (@TotalQty - @AQty)
                                                        SET @TotalStockValue = (@TotalQty * @TotalCost)
                                                    END
                                                    ELSE
                                                    BEGIN
                                                    IF(@TotalQty != 0)
                                                    BEGIN
                                                    SET @TotalCost = @TotalStockValue / @TotalQty
													 END
                                                    SET @TotalQty = @TotalQty - @AQty
                                                    SET @TotalStockValue = (@TotalQty * @TotalCost)
                                                                                                        
                                       
                                        END

                                        UPDATE #All_table SET OQty = @AQty,OCost = @AUnitPrice,OCostValue=(@AQty * @TotalCost) WHERE ID=@AID
                                        UPDATE #All_table SET BQty = @TotalQty,BCost = @TotalCost,BCostValue=(@TotalQty * @TotalCost) WHERE ID=@AID

                                        END
										  ELSE IF(@ATItemType='Asset To Inventory')  
                                            BEGIN
                                            IF(@TotalQty != 0)
                                                BEGIN
                                                        SET @TotalCost = @TotalStockValue / @TotalQty
														 END
                                                        SET @TotalQty = @TotalQty + @AQty
                                                        SET @TotalStockValue = (@TotalQty * @TotalCost)
                                                       UPDATE #All_table SET IQty = @AQty,ICost = @TotalCost,ICostValue=(@AQty * @TotalCost) WHERE ID=@AID
                                                       UPDATE #All_table SET BQty = @TotalQty,BCost = @TotalCost,BCostValue=(@TotalQty * @TotalCost) WHERE ID=@AID
                                               
                                            END
										ELSE IF(@ATItemType='Stock Received' or @ATItemType='Stock Receivedadj')  
                                                BEGIN
                                                     IF(@AStkType=1)
                                                     BEGIN
                                                        SET @TotalCost = @AUnitPrice
                                                        SET @TotalQty = (@TotalQty + @AQty)*1
                                                        SET @TotalStockValue = (@TotalQty * @TotalCost)
                                                    END
                                                    ELSE
                                                    BEGIN
                                                    IF(@TotalQty != 0)
                                                    BEGIN
                                                    SET @TotalCost = @TotalStockValue / @TotalQty
													 END
                                                    SET @TotalQty = @TotalQty + @AQty
                                                    SET @TotalStockValue = (@TotalQty * @TotalCost)
                                                                                                        
                                       
                                        END

                                        UPDATE #All_table SET IQty = @AQty,ICost = @AUnitPrice,ICostValue=(@AQty * @TotalCost) WHERE ID=@AID
                                        UPDATE #All_table SET BQty = @TotalQty,BCost = @TotalCost,BCostValue=(@TotalQty * @TotalCost) WHERE ID=@AID

                                        END
                                            ELSE IF(@ATItemType='Opening Stockk')  
                                            BEGIN
                                            UPDATE #All_table SET BQty = @ABQty,BCost = @ABCost,BCostValue=BCostValue WHERE ID=@AID
                                            END

											ELSE IF(@ATItemType='Sales Return')  
                                            BEGIN
											select  @seitemid=max(e.SEItemsId)  from #All_table a
												join SRItems b on a.TItemID =b.SRItemsId  
												join SalesReturns c on b.SalesReturnId =c.SalesReturnId
												join SalesEntries d on d.SalesEntryId =c.SalesEntryId 
											    join SEItems e on e.SalesEntry =d.SalesEntryId 
												 and e.Item =b.Item and a.TItemType='Sales Return'
											
												where a.ID=@AID
								if(@seitemid!='')
												BEGIN
												update #All_table set TItemID=@seitemid where ID=@AID
												END
								IF(@TotalQty != 0)
                                                BEGIN
                                                        SET @TotalCost = @TotalStockValue / @TotalQty
														 END
                                                        SET @TotalQty = @TotalQty + @AQty
                                                        SET @TotalStockValue = (@TotalQty * @TotalCost)
                                                       UPDATE #All_table SET IQty = @AQty,ICost = @AUnitPrice,ICostValue=(@AQty * @TotalCost) WHERE ID=@AID
                                                       UPDATE #All_table SET BQty = @TotalQty,BCost = @TotalCost,BCostValue=(@TotalQty * @TotalCost) WHERE ID=@AID
                                               
                                            END
                                        ELSE
                                        BEGIN
                                                IF(@TotalQty != 0)
                                                BEGIN
                                                        SET @TotalCost = @TotalStockValue / @TotalQty
														 END
                                                        SET @TotalQty = @TotalQty - @AQty
                                                        SET @TotalStockValue = (@TotalQty * @TotalCost)
                                                       UPDATE #All_table SET OQty = @AQty,OCost = @AUnitPrice,OCostValue=(@AQty * @TotalCost) WHERE ID=@AID
                                                       UPDATE #All_table SET BQty = @TotalQty,BCost = @TotalCost,BCostValue=(@TotalQty * @TotalCost) WHERE ID=@AID
                                               
                                                                
                                        END        
                                                                                
                                        FETCH NEXT FROM TableAll_cursor INTO @AID,@ATItemID, @AItemID,@ATDate,@AInvoice,@ATItemType,@AIQty,@AICost,@AICostValue,@AOQty,@AOCost,@AOCostValue,@ABQty,@ABCost,@ABCostValue,@AQty,@AUnitPrice,@AStkType                                                                                

                                                                        
                                                        END
                                                        CLOSE TableAll_cursor
                                                        DEALLOCATE TableAll_cursor     
                                                        SET @FTotalQty = @FTotalQty + @TotalQty
                                                        SET @FTotalCost = @FTotalCost + @TotalCost
                                                        SET @FTotalStockValue = @FTotalStockValue + @TotalStockValue
                                        END

                 END

                                --SET @FTotalStockValue = @FTotalStockValue + @TotalStockValue
    --            SET @FTotalQty = @FTotalQty + @TotalQty
    --            IF (@FTotalQty > 0)
    --            BEGIN
    --            SET @TotalCost = @FTotalStockValue / @FTotalQty
    --            END

                                  INSERT INTO @Item_table
                        Select  
                        it.ItemID as id,it.ItemName,it.ItemUnitID,it.SubUnitId, it.SellingPrice,it.PurchasePrice,it.KeepStock,it.MinStock,it.ItemCode,it.ItemArabic,it.Barcode,it.ConFactor,
                        it.OpeningStock,it.MRP,
                        it.PartNumber,
                        IU.ItemUnitName,
                        SU.ItemUnitName as SubUnitName,
                        IC.ItemCategoryName,
                        IB.ItemBrandName,
                        tx.[Percentage],tx.TaxID,tx.TaxName,
                        
                        SE.ItemUnitPrice as SEunitprice,SE.ItemUnit as SEunit,
                        PE.ItemUnitPrice as PEunitprice,PE.ItemUnit as PEunit,

                        (SELECT CONCAT(it.ItemCode, '-', it.ItemName)) as [text],

                        (select ([Status]) from EnableSettings where EnableType='PartNoInItem') as partnum,
                        ((@FTotalQty/it.ConFactor) * (case when @FTotalQty > 0 then it.PurchasePrice else 0 end )) AS StockValue,(@FTotalQty/it.ConFactor) AS TotalQty,case when @FTotalQty > 0 then (it.PurchasePrice) else 0 end AS TotalCost
                        from 
                        items as It
                        Left Join ItemUnits      as IU  on IU.ItemUnitID = it.ItemUnitID
                        Left Join ItemUnits      as SU  on SU.ItemUnitID = it.SubUnitId
                        Left Join ItemCategories as IC  on IC.ItemCategoryID =  it.ItemCategoryID
                        Left Join ItemBrands     as IB  on IB.ItemBrandID = it.ItemBrandID
                        left join Taxes          as tx  on tx.TaxID=it.TaxID
                        left join Jewelleries    as jw  on jw.Item=it.ItemID
                        left join Scaffolds      as SC  on Sc.Item=it.ItemID
                        left join ItemBundles    as ItB on ItB.mainItem =it.ItemID
                        left join ItemColors     as itC on itC.ItemColorID=it.ItemColorID
                        left join ItemSizes      as itS on itS.ItemSizeID=it.ItemSizeID
                        left join PEItems        as PE on PE.Item=it.ItemID and PE.PEItemsId=(select MAX(PEItemsId) from PEItems Pei join PurchaseEntries Purty 
                                 on Pei.PurchaseEntry = Purty.PurchaseEntryId where Pei.Item=it.ItemID and Pei.ItemUnitPrice!=0 and (@MCId=0 or Purty.MaterialCenter = @MCId))
                        left join SEItems        as SE on SE.Item=it.ItemID and SE.SEItemsId=(select MAX(SEItemsId) from SEItems Sei join SalesEntries Salnty 
                                 on Sei.SalesEntry=Salnty.SalesEntryId where Sei.Item=it.ItemID and Sei.ItemUnitPrice != 0 and (@MCId=0 or Salnty.MaterialCenter = @MCId))

                        where It.ItemID=@TableA_ID and (@CategoryId=0 or It.ItemCategoryID = @CategoryId) and (@BrandId=0 or It.ItemBrandID = @BrandId) and (@ItemId=0 or It.ItemID = @ItemId) and (@Stockble='1' or It.KeepStock='True')

         FETCH NEXT FROM TableA_cursor INTO @TableA_ID
   END 


END

CLOSE TableA_cursor
DEALLOCATE TableA_cursor    
IF(@Stype=0)
BEGIN
SELECT * from #All_table order by TDate, ID
END ELSE BEGIN
IF(@MCId=1)
BEGIN
UPDATE @Item_table SET ITotalQty=0,ITotalCost =0,ITotalStockValue =0 where ITotalQty<0
END
SELECT * from @Item_table
END
END
end





































GO
PRINT 'SP_AVCOMethod (fast, deterministic) installed';

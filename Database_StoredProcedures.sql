-- Create a table to store deleted product information
CREATE TABLE [dbo].[ProductAudit] (
    AuditID int IDENTITY(1,1) PRIMARY KEY,
    ProductID int,
    ProductName nvarchar(50),
    DeletedBy nvarchar(100),
    DeletedDate datetime
);
GO

-- =======================================================
-- Trigger: trg_Product_AuditDelete
-- Description: Automatically logs deleted products into the ProductAudit table.
-- =======================================================
CREATE OR ALTER TRIGGER trg_Product_AuditDelete
ON [dbo].[Product]
AFTER DELETE
AS
BEGIN
    -- SET NOCOUNT ON prevents the sending of DONE_IN_PROC messages
    -- to the client for each statement in the trigger.
    SET NOCOUNT ON;

    INSERT INTO [dbo].[ProductAudit] (ProductID, ProductName, DeletedBy, DeletedDate)
    SELECT d.ID, d.Name, SYSTEM_USER, GETDATE()
    FROM deleted d;

    PRINT 'Product deletion logged successfully.';
END;

-- Test: Delete a product that has no orders
DELETE FROM Product WHERE ID = 30;

-- Check the audit table
SELECT * FROM ProductAudit;


-- =======================================================
-- Stored Procedure: sp_ApplyDiscountToNewOrders
-- Description: Iterates through 'New' orders and applies a discount 
-- based on the total order value (derived from the product price).
-- =======================================================
CREATE OR ALTER PROCEDURE sp_ApplyDiscountToNewOrders
AS
BEGIN
    SET NOCOUNT ON;

    -- Variable declarations to hold data from the cursor
    DECLARE @OrderID int;
    DECLARE @ProductPrice decimal(18,2);
    DECLARE @DiscountAmount decimal(18,2);
    
    -- 1. DECLARE THE CURSOR
    -- Select the OrderID and the Price of the associated Product
    -- only for orders that are currently 'New'.
    DECLARE order_cursor CURSOR FOR 
    SELECT o.ID, p.Price
    FROM [Order] o
    INNER JOIN Product p ON o.ProductID = p.ID
    WHERE o.Status = 'New';

    -- 2. OPEN THE CURSOR
    OPEN order_cursor;

    -- 3. FETCH THE FIRST ROW
    -- We load the data into our variables.
    FETCH NEXT FROM order_cursor INTO @OrderID, @ProductPrice;

    -- 4. LOOP THROUGH THE ROWS
    -- @@FETCH_STATUS = 0 means the fetch was successful.
    WHILE @@FETCH_STATUS = 0
    BEGIN
        -- --- BUSINESS LOGIC START ---
        IF @ProductPrice > 100000
            SET @DiscountAmount = @ProductPrice * 0.10;
        ELSE
            SET @DiscountAmount = @ProductPrice * 0.05;

        -- 5. FETCH THE NEXT ROW
        FETCH NEXT FROM order_cursor INTO @OrderID, @ProductPrice;
    END

    -- 6. CLOSE THE CURSOR
    CLOSE order_cursor;
    
    -- 7. DEALLOCATE THE CURSOR
    DEALLOCATE order_cursor;
END;

-- Execute the stored procedure
EXEC sp_ApplyDiscountToNewOrders;
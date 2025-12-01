-- =============================================
-- Fix Tailor Assignment for Existing Orders (SQLite)
-- =============================================
-- This script updates existing StoreOrder records to assign them
-- to the correct tailor based on their product's TailorId.
--
-- IMPORTANT: 
-- - Backup your database before running this script!
-- - This will update ALL store orders to have correct tailor assignments
-- =============================================

-- Update Orders to correct TailorId based on OrderItems
UPDATE Orders
SET 
    TailorId = (
        SELECT p.TailorId 
        FROM OrderItems oi 
        INNER JOIN Products p ON oi.ProductId = p.ProductId 
        WHERE oi.OrderId = Orders.OrderId 
        AND p.TailorId IS NOT NULL
        LIMIT 1
    ),
    CommissionRate = 0.10,
    CommissionAmount = TotalPrice * 0.10
WHERE OrderType = 'StoreOrder'
AND EXISTS (
    SELECT 1 
    FROM OrderItems oi 
    INNER JOIN Products p ON oi.ProductId = p.ProductId 
    WHERE oi.OrderId = Orders.OrderId 
    AND p.TailorId IS NOT NULL
);

-- Update Payment records to match the new TailorId
UPDATE Payment
SET TailorId = (
    SELECT o.TailorId 
    FROM Orders o 
    WHERE o.OrderId = Payment.OrderId
)
WHERE EXISTS (
    SELECT 1 
    FROM Orders o 
    WHERE o.OrderId = Payment.OrderId 
    AND o.OrderType = 'StoreOrder'
);

-- Show summary of changes
SELECT 
    COUNT(*) as TotalOrdersUpdated,
    COUNT(DISTINCT TailorId) as UniqueTailors
FROM Orders
WHERE OrderType = 'StoreOrder'
AND TailorId IS NOT NULL;

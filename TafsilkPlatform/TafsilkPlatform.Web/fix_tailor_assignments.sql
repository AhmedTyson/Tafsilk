-- =============================================
-- Fix Tailor Assignment for Existing Orders
-- =============================================
-- This script updates existing StoreOrder records to assign them
-- to the correct tailor based on their product's TailorId.
--
-- IMPORTANT: 
-- - Backup your database before running this script!
-- - This will update ALL store orders to have correct tailor assignments
-- - Run this only once after deploying the new checkout code
-- =============================================

-- Update Orders to correct TailorId based on OrderItems
-- Note: This handles single-tailor orders. Multi-tailor orders may need manual review.
UPDATE Orders
SET 
    TailorId = (
        SELECT TOP 1 p.TailorId 
        FROM OrderItems oi 
        INNER JOIN Products p ON oi.ProductId = p.ProductId 
        WHERE oi.OrderId = Orders.OrderId 
        AND p.TailorId IS NOT NULL
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
SET TailorId = o.TailorId
FROM Payment p
INNER JOIN Orders o ON p.OrderId = o.OrderId
WHERE o.OrderType = 'StoreOrder';

-- Show summary of changes
SELECT 
    COUNT(*) as TotalOrdersUpdated,
    COUNT(DISTINCT TailorId) as UniqueTailors
FROM Orders
WHERE OrderType = 'StoreOrder'
AND TailorId IS NOT NULL;

PRINT 'Migration completed successfully!';

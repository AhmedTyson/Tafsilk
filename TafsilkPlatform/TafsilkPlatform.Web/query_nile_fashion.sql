-- Query to find Nile Fashion House user information
SELECT 
    u.Email,
    u.PhoneNumber,
    t.FullName,
    t.ShopName,
    t.City,
    t.District,
    r.Name as Role
FROM TailorProfiles t
INNER JOIN Users u ON t.UserId = u.Id
LEFT JOIN Roles r ON u.RoleId = r.Id
WHERE t.ShopName LIKE '%Nile%' OR t.FullName LIKE '%Nile%'
ORDER BY t.CreatedAt DESC;

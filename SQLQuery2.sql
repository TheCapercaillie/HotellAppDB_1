USE HotellAppDB;
GO

SELECT 
    B.*, 
    C.Name
FROM Bookings B
JOIN Customers C ON B.CustomerId = C.Id
WHERE B.CheckInDate >= '2025-04-12'
ORDER BY B.CheckOutDate DESC;
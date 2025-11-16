-- Start transaction, so everything succeeds or nothing does
BEGIN TRANSACTION;

-- Populate the [Category] table (10 categories)
-- Categories with ID 9 and 10 will not have any products assigned.
SET IDENTITY_INSERT [dbo].[Category] ON;
INSERT INTO [dbo].[Category] (ID, [Name])
VALUES
    (1, N'Smartphones and Tablets'),
    (2, N'Laptops and Computers'),
    (3, N'TV and Entertainment'),
    (4, N'Major Home Appliances'),
    (5, N'Small Kitchen Appliances'),
    (6, N'Sports and Fitness'),
    (7, N'Books'),
    (8, N'DIY and Garden'),
    (9, N'Empty Category 1 (no products)'),
    (10, N'Empty Category 2 (no products)');
SET IDENTITY_INSERT [dbo].[Category] OFF;

-- Populate the [Product] table (30 products)
-- The last 5 products (ID: 26-30) will not be included in any orders.
SET IDENTITY_INSERT [dbo].[Product] ON;
INSERT INTO [dbo].[Product] (ID, [Name], Price, Stock, VATPercentage, CategoryID, [Description])
VALUES
    -- Category 1: Smartphones (4 pcs)
    (1, N'Alfa Mobil X100', 120000.00, 50, 27, 1, N'<item><desc>Latest smartphone</desc></item>'),
    (2, N'Béta Tablet T5', 85000.00, 30, 27, 1, N'<item><desc>10-inch display</desc></item>'),
    (3, N'Gamma Okosóra G2', 45000.00, 75, 27, 1, N'<item><desc>Waterproof smartwatch</desc></item>'),
    (4, N'Delta Fülhallgató D1', 22000.00, 150, 27, 1, N'<item><desc>Noise-cancelling, wireless</desc></item>'),
    
    -- Category 2: Laptops (4 pcs)
    (5, N'Omega Laptop Pro 15', 310000.00, 25, 27, 2, N'<item><desc>High-performance laptop</desc></item>'),
    (6, N'Szigma Ultrabook S3', 250000.00, 40, 27, 2, N'<item><desc>Light and thin</desc></item>'),
    (7, N'Pi Gamer PC P9', 450000.00, 15, 27, 2, N'<item><desc>Optimized for video games</desc></item>'),
    (8, N'Epszilon Monitor E27', 65000.00, 60, 27, 2, N'<item><desc>27-inch 4K monitor</desc></item>'),
    
    -- Category 3: TV (4 pcs)
    (9, N'Lambda 55" 4K TV', 180000.00, 35, 27, 3, N'<item><desc>Smart TV, QLED</desc></item>'),
    (10, N'Mű Soundbar M1', 35000.00, 80, 27, 3, N'<item><desc>Surround sound</desc></item>'),
    (11, N'Nű Projektor N7', 95000.00, 20, 27, 3, N'<item><desc>Home cinema projector</desc></item>'),
    (12, N'Zéta Streaming Box', 18000.00, 110, 27, 3, N'<item><desc>Online content player</desc></item>'),
    
    -- Category 4: Major Home Appliances (4 pcs)
    (13, N'Washing Machine "Clean"', 110000.00, 40, 27, 4, N'<item><desc>A+++ energy class</desc></item>'),
    (14, N'Refrigerator "Fresh"', 145000.00, 25, 27, 4, N'<item><desc>Combined, NoFrost</desc></item>'),
    (15, N'Dishwasher "Shiny"', 90000.00, 30, 27, 4, N'<item><desc>12 place settings</desc></item>'),
    (16, N'Vacuum "Dustless"', 55000.00, 60, 27, 4, N'<item><desc>Bagless</desc></item>'),
    
    -- Category 5: Small Kitchen Appliances (3 pcs)
    (17, N'Coffee Maker "WakeUp"', 28000.00, 90, 27, 5, N'<item><desc>Grinds whole beans</desc></item>'),
    (18, N'Blender "Silky"', 15000.00, 120, 27, 5, N'<item><desc>Smoothie maker</desc></item>'),
    (19, N'Kettle "Hot"', 8000.00, 200, 27, 5, N'<item><desc>Fast and quiet</desc></item>'),
    
    -- Category 6: Sports (3 pcs)
    (20, N'Treadmill "Cardio"', 130000.00, 15, 27, 6, N'<item><desc>For home workouts</desc></item>'),
    (21, N'Dumbbell Set', 12000.00, 100, 27, 6, N'<item><desc>2x10kg</desc></item>'),
    (22, N'Yoga Mat "Zen"', 6000.00, 150, 27, 6, N'<item><desc>Non-slip</desc></item>'),
    
    -- Category 7: Books (3 pcs)
    (23, N'The Castle (novel)', 4500.00, 300, 5, 7, N'<item><desc>Classic literature</desc></item>'),
    (24, N'SQL Basics (textbook)', 8900.00, 120, 5, 7, N'<item><desc>For beginners and advanced users</desc></item>'),
    (25, N'Space Travel (sci-fi)', 5200.00, 200, 5, 7, N'<item><desc>Spectacular cover art</desc></item>'),
    
    -- Category 8: DIY (5 pcs) - THESE 5 WILL NOT BE ORDERED
    (26, N'Drill "Strong"', 29000.00, 70, 27, 8, N'<item><desc>Cordless</desc></item>'),
    (27, N'Sander "Smooth"', 18000.00, 50, 27, 8, N'<item><desc>Orbital sander</desc></item>'),
    (28, N'Screwdriver Set', 7000.00, 130, 27, 8, N'<item><desc>With magnetic tips</desc></item>'),
    (29, N'Ladder "High"', 22000.00, 40, 27, 8, N'<item><desc>Aluminum, 5-step</desc></item>'),
    (30, N'Hammer "Big"', 5000.00, 80, 27, 8, N'<item><desc>Fiberglass handle</desc></item>');
SET IDENTITY_INSERT [dbo].[Product] OFF;

-- Populate the [Order] table (50 orders)
-- The orders only apply to products with IDs 1-25.
-- Each product (1-25) has exactly 2 orders.
SET IDENTITY_INSERT [dbo].[Order] ON;
INSERT INTO [dbo].[Order] (ID, [Date], Deadline, [Status], PaymentMethod, ProductID)
VALUES
    -- Round 1 (1 order for each of the 25 products)
    (1, GETDATE()-20, GETDATE()-10, N'Completed', N'Credit Card', 1),
    (2, GETDATE()-19, GETDATE()-9, N'Completed', N'Credit Card', 2),
    (3, GETDATE()-18, GETDATE()-8, N'Completed', N'Cash on Delivery', 3),
    (4, GETDATE()-17, GETDATE()-7, N'Completed', N'Credit Card', 4),
    (5, GETDATE()-16, GETDATE()-6, N'Completed', N'Bank Transfer', 5),
    (6, GETDATE()-15, GETDATE()-5, N'Completed', N'Credit Card', 6),
    (7, GETDATE()-14, GETDATE()-4, N'Completed', N'Cash on Delivery', 7),
    (8, GETDATE()-13, GETDATE()-3, N'Completed', N'Credit Card', 8),
    (9, GETDATE()-12, GETDATE()-2, N'Completed', N'Credit Card', 9),
    (10, GETDATE()-11, GETDATE()-1, N'Completed', N'Bank Transfer', 10),
    (11, GETDATE()-10, GETDATE()+1, N'Shipped', N'Credit Card', 11),
    (12, GETDATE()-9, GETDATE()+2, N'Shipped', N'Cash on Delivery', 12),
    (13, GETDATE()-8, GETDATE()+3, N'Shipped', N'Credit Card', 13),
    (14, GETDATE()-7, GETDATE()+4, N'Shipped', N'Credit Card', 14),
    (15, GETDATE()-6, GETDATE()+5, N'Processing', N'Cash on Delivery', 15),
    (16, GETDATE()-5, GETDATE()+6, N'Processing', N'Credit Card', 16),
    (17, GETDATE()-4, GETDATE()+7, N'Processing', N'Credit Card', 17),
    (18, GETDATE()-3, GETDATE()+8, N'Processing', N'Bank Transfer', 18),
    (19, GETDATE()-2, GETDATE()+9, N'Processing', N'Credit Card', 19),
    (20, GETDATE()-1, GETDATE()+10, N'New', N'Cash on Delivery', 20),
    (21, GETDATE()-1, GETDATE()+11, N'New', N'Credit Card', 21),
    (22, GETDATE()-1, GETDATE()+12, N'New', N'Credit Card', 22),
    (23, GETDATE()-1, GETDATE()+13, N'New', N'Cash on Delivery', 23),
    (24, GETDATE()-1, GETDATE()+14, N'New', N'Credit Card', 24),
    (25, GETDATE()-1, GETDATE()+15, N'New', N'Bank Transfer', 25),
    
    -- Round 2 (1 more order for each of the 25 products)
    (26, GETDATE()-5, GETDATE()+5, N'Completed', N'Credit Card', 1),
    (27, GETDATE()-5, GETDATE()+5, N'Completed', N'Credit Card', 2),
    (28, GETDATE()-4, GETDATE()+6, N'Completed', N'Cash on Delivery', 3),
    (29, GETDATE()-4, GETDATE()+6, N'Completed', N'Credit Card', 4),
    (30, GETDATE()-4, GETDATE()+6, N'Completed', N'Bank Transfer', 5),
    (31, GETDATE()-3, GETDATE()+7, N'Completed', N'Credit Card', 6),
    (32, GETDATE()-3, GETDATE()+7, N'Completed', N'Cash on Delivery', 7),
    (33, GETDATE()-3, GETDATE()+7, N'Completed', N'Credit Card', 8),
    (34, GETDATE()-2, GETDATE()+8, N'Shipped', N'Credit Card', 9),
    (35, GETDATE()-2, GETDATE()+8, N'Shipped', N'Bank Transfer', 10),
    (36, GETDATE()-2, GETDATE()+8, N'Shipped', N'Credit Card', 11),
    (37, GETDATE()-2, GETDATE()+9, N'Shipped', N'Cash on Delivery', 12),
    (38, GETDATE()-2, GETDATE()+9, N'Shipped', N'Credit Card', 13),
    (39, GETDATE()-1, GETDATE()+10, N'Processing', N'Credit Card', 14),
    (40, GETDATE()-1, GETDATE()+10, N'Processing', N'Cash on Delivery', 15),
    (41, GETDATE()-1, GETDATE()+11, N'Processing', N'Credit Card', 16),
    (42, GETDATE()-1, GETDATE()+11, N'New', N'Credit Card', 17),
    (43, GETDATE(), GETDATE()+12, N'New', N'Bank Transfer', 18),
    (44, GETDATE(), GETDATE()+12, N'New', N'Credit Card', 19),
    (45, GETDATE(), GETDATE()+13, N'New', N'Cash on Delivery', 20),
    (46, GETDATE(), GETDATE()+13, N'New', N'Credit Card', 21),
    (47, GETDATE(), GETDATE()+14, N'New', N'Credit Card', 22),
    (48, GETDATE(), GETDATE()+14, N'New', N'Cash on Delivery', 23),
    (49, GETDATE(), GETDATE()+15, N'New', N'Credit Card', 24),
    (50, GETDATE(), GETDATE()+15, N'New', N'Bank Transfer', 25);
SET IDENTITY_INSERT [dbo].[Order] OFF;

COMMIT TRANSACTION;
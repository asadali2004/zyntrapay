-- ============================================
-- Database: RewardsDB
-- Service:  RewardsService (Port 5009)
-- Purpose:  Points, tiers, catalog, redemptions
-- ============================================

CREATE DATABASE RewardsDB;
GO

USE RewardsDB;
GO

CREATE TABLE RewardAccounts (
    Id          INT         PRIMARY KEY IDENTITY(1,1),
    AuthUserId  INT         NOT NULL UNIQUE,
    TotalPoints INT         NOT NULL DEFAULT 0,
    Tier        NVARCHAR(20) NOT NULL DEFAULT 'Silver',
    CreatedAt   DATETIME    NOT NULL DEFAULT GETDATE()
);
GO

CREATE TABLE RewardCatalog (
    Id          INT           PRIMARY KEY IDENTITY(1,1),
    Title       NVARCHAR(150) NOT NULL,
    Description NVARCHAR(300) NULL,
    PointsCost  INT           NOT NULL,
    Stock       INT           NOT NULL DEFAULT -1,
    IsActive    BIT           NOT NULL DEFAULT 1,
    CreatedAt   DATETIME      NOT NULL DEFAULT GETDATE()
);
GO

CREATE TABLE Redemptions (
    Id              INT           PRIMARY KEY IDENTITY(1,1),
    AuthUserId      INT           NOT NULL,
    RewardCatalogId INT           NOT NULL REFERENCES RewardCatalog(Id),
    PointsSpent     INT           NOT NULL,
    RedeemedAt      DATETIME      NOT NULL DEFAULT GETDATE()
);
GO

-- Seed some catalog items
INSERT INTO RewardCatalog (Title, Description, PointsCost, Stock, IsActive)
VALUES
('Amazon Voucher Rs.100',  'Amazon gift voucher worth Rs.100',  100, -1, 1),
('Free Movie Ticket',      'BookMyShow voucher for 1 ticket',   500, 50, 1),
('Cashback Rs.50',         'Rs.50 cashback to your wallet',     200, -1, 1),
('Premium Membership',     '1 month premium access',            1000, 20, 1);
GO
-- ============================================
-- Database: WalletDB
-- Service:  WalletService (Port 5007)
-- Purpose:  Wallet balance and ledger entries
-- ============================================

CREATE DATABASE WalletDB;
GO

USE WalletDB;
GO

CREATE TABLE Wallets (
    Id          INT             PRIMARY KEY IDENTITY(1,1),
    AuthUserId  INT             NOT NULL UNIQUE,
    Balance     DECIMAL(18,2)   NOT NULL DEFAULT 0,
    IsActive    BIT             NOT NULL DEFAULT 1,
    CreatedAt   DATETIME        NOT NULL DEFAULT GETDATE()
);
GO

CREATE TABLE LedgerEntries (
    Id          INT             PRIMARY KEY IDENTITY(1,1),
    WalletId    INT             NOT NULL REFERENCES Wallets(Id),
    Type        NVARCHAR(10)    NOT NULL,         -- CREDIT / DEBIT
    Amount      DECIMAL(18,2)   NOT NULL,
    Description NVARCHAR(200)   NOT NULL,
    ReferenceId NVARCHAR(100)   NULL,             -- e.g. transfer target userId
    CreatedAt   DATETIME        NOT NULL DEFAULT GETDATE()
);
GO
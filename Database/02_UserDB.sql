-- ============================================
-- Database: UserDB
-- Service:  UserService (Port 5005)
-- Purpose:  User profiles and KYC submissions
-- ============================================

CREATE DATABASE UserDB;
GO

USE UserDB;
GO

CREATE TABLE UserProfiles (
    Id          INT PRIMARY KEY IDENTITY(1,1),
    AuthUserId  INT NOT NULL UNIQUE,      -- links to AuthDB.Users.Id
    FullName    NVARCHAR(150) NOT NULL,
    DateOfBirth DATE NOT NULL,
    Address     NVARCHAR(300) NOT NULL,
    City        NVARCHAR(100) NOT NULL,
    State       NVARCHAR(100) NOT NULL,
    PinCode     NVARCHAR(10)  NOT NULL,
    CreatedAt   DATETIME      NOT NULL DEFAULT GETDATE()
);
GO

CREATE TABLE KycSubmissions (
    Id              INT PRIMARY KEY IDENTITY(1,1),
    AuthUserId      INT NOT NULL UNIQUE,
    DocumentType    NVARCHAR(50)  NOT NULL,  -- 'Aadhaar', 'PAN', 'Passport'
    DocumentNumber  NVARCHAR(50)  NOT NULL,
    Status          NVARCHAR(20)  NOT NULL DEFAULT 'Pending', -- Pending, Approved, Rejected
    RejectionReason NVARCHAR(300) NULL,
    SubmittedAt     DATETIME      NOT NULL DEFAULT GETDATE(),
    ReviewedAt      DATETIME      NULL
);
GO
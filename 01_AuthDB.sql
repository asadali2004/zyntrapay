-- ============================================
-- Database: AuthDB
-- Service:  AuthService (Port 5003)
-- Purpose:  Stores user credentials and roles
-- ============================================

CREATE DATABASE AuthDB;
GO

USE AuthDB;
GO

CREATE TABLE Users (
    Id           INT PRIMARY KEY IDENTITY(1,1),
    Email        NVARCHAR(150) NOT NULL UNIQUE,
    PhoneNumber  NVARCHAR(20)  NOT NULL UNIQUE,
    PasswordHash NVARCHAR(300) NOT NULL,
    Role         NVARCHAR(20)  NOT NULL DEFAULT 'User',
    IsActive     BIT           NOT NULL DEFAULT 1,
    CreatedAt    DATETIME      NOT NULL DEFAULT GETDATE()
);
GO
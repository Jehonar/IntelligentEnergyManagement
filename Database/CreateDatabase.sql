-- ============================================================
-- Intelligent Energy Management System
-- Database creation script for SQL Server
-- Run this script in SQL Server Management Studio or sqlcmd
-- ============================================================

USE master;
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = 'IntelligentEnergySystem')
BEGIN
    ALTER DATABASE IntelligentEnergySystem SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE IntelligentEnergySystem;
END
GO

-- Use project data folder to avoid SQL Server compressed default data directory issues
CREATE DATABASE IntelligentEnergySystem
ON PRIMARY (
    NAME = IntelligentEnergySystem,
    FILENAME = 'C:\SQLData\IntelligentEnergySystem.mdf',
    SIZE = 100MB,
    MAXSIZE = UNLIMITED,
    FILEGROWTH = 50MB
)
LOG ON (
    NAME = IntelligentEnergySystem_log,
    FILENAME = 'C:\SQLData\IntelligentEnergySystem_log.ldf',
    SIZE = 50MB,
    MAXSIZE = UNLIMITED,
    FILEGROWTH = 25MB
);
GO

USE IntelligentEnergySystem;
GO

-- ============================================================
-- TABLE: EnergyReadings
-- Stores hourly energy consumption readings per device
-- ============================================================
CREATE TABLE EnergyReadings (
    Id               INT IDENTITY(1,1) PRIMARY KEY,
    ReadingDate      DATE NOT NULL,
    ReadingHour      INT NOT NULL CHECK (ReadingHour BETWEEN 0 AND 23),
    EnergyConsumption DECIMAL(10,4) NOT NULL,  -- kWh
    Temperature      DECIMAL(5,2) NULL,         -- Celsius
    DeviceName       NVARCHAR(100) NOT NULL,
    CreatedAt        DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
GO

CREATE INDEX IX_EnergyReadings_Date ON EnergyReadings (ReadingDate);
CREATE INDEX IX_EnergyReadings_Device ON EnergyReadings (DeviceName);
GO

-- ============================================================
-- TABLE: Predictions
-- Stores AI-generated energy consumption predictions
-- ============================================================
CREATE TABLE Predictions (
    Id                   INT IDENTITY(1,1) PRIMARY KEY,
    PredictionDate       DATE NOT NULL,
    PredictionHour       INT NOT NULL CHECK (PredictionHour BETWEEN 0 AND 23),
    PredictedConsumption DECIMAL(10,4) NOT NULL,  -- kWh
    CreatedAt            DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
GO

-- ============================================================
-- TABLE: Recommendations
-- Stores AI-generated energy saving recommendations
-- ============================================================
CREATE TABLE Recommendations (
    Id                 INT IDENTITY(1,1) PRIMARY KEY,
    PredictionId       INT NULL REFERENCES Predictions(Id),
    Message            NVARCHAR(500) NOT NULL,
    RecommendationType NVARCHAR(50) NOT NULL,   -- HIGH | MODERATE | NORMAL | LOW
    CreatedAt          DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
GO

-- ============================================================
-- SEED DATA
-- Realistic hourly readings for 3 months (Jan–Mar 2026)
-- Devices: HVAC, Lighting, Appliances, Computers
-- Pattern: higher consumption in morning and evening peaks
-- ============================================================

DECLARE @StartDate DATE = '2026-01-01';
DECLARE @EndDate   DATE = '2026-03-31';
DECLARE @CurDate   DATE = @StartDate;
DECLARE @Hour      INT;
DECLARE @Base      DECIMAL(10,4);
DECLARE @Temp      DECIMAL(5,2);
DECLARE @Device    NVARCHAR(100);
DECLARE @DeviceIdx INT;
DECLARE @Devices   TABLE (Idx INT, Name NVARCHAR(100));
INSERT INTO @Devices VALUES (1,'HVAC'),(2,'Lighting'),(3,'Appliances'),(4,'Computers');

WHILE @CurDate <= @EndDate
BEGIN
    SET @Hour = 0;
    WHILE @Hour <= 23
    BEGIN
        -- Temperature varies by month (Jan cold, Mar warmer)
        SET @Temp = 5.0 
            + CASE MONTH(@CurDate) WHEN 1 THEN 0 WHEN 2 THEN 2 ELSE 5 END
            + (CAST(CHECKSUM(NEWID()) AS FLOAT) / 2147483647.0) * 4.0;

        SET @DeviceIdx = 1;
        WHILE @DeviceIdx <= 4
        BEGIN
            SELECT @Device = Name FROM @Devices WHERE Idx = @DeviceIdx;

            -- Base consumption by hour (peak morning 7-9, peak evening 18-21)
            SET @Base = CASE
                WHEN @Hour BETWEEN 0 AND 5  THEN 1.5
                WHEN @Hour BETWEEN 6 AND 8  THEN 4.5
                WHEN @Hour BETWEEN 9 AND 17 THEN 3.0
                WHEN @Hour BETWEEN 18 AND 21 THEN 5.0
                ELSE 2.0
            END;

            -- Device multiplier
            SET @Base = @Base * CASE @Device
                WHEN 'HVAC'       THEN 1.8
                WHEN 'Lighting'   THEN 0.6
                WHEN 'Appliances' THEN 1.0
                WHEN 'Computers'  THEN 0.9
                ELSE 1.0
            END;

            -- Weekend reduction
            IF DATEPART(WEEKDAY, @CurDate) IN (1,7)
                SET @Base = @Base * 0.7;

            -- Random variation ±15%
            SET @Base = @Base * (0.85 + (ABS(CHECKSUM(NEWID())) % 30) / 100.0);

            INSERT INTO EnergyReadings (ReadingDate, ReadingHour, EnergyConsumption, Temperature, DeviceName)
            VALUES (@CurDate, @Hour, ROUND(@Base, 4), @Temp, @Device);

            SET @DeviceIdx = @DeviceIdx + 1;
        END;

        SET @Hour = @Hour + 1;
    END;
    SET @CurDate = DATEADD(DAY, 1, @CurDate);
END;
GO

PRINT 'Database IntelligentEnergySystem created and seeded successfully.';
GO

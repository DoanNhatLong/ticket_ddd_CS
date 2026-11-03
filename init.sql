CREATE DATABASE IF NOT EXISTS `0test1` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE `0test1`;

SET FOREIGN_KEY_CHECKS = 0;
DROP TABLE IF EXISTS Payments;
DROP TABLE IF EXISTS BookingItems;
DROP TABLE IF EXISTS Bookings;
DROP TABLE IF EXISTS Users;
DROP TABLE IF EXISTS TripSeatPrices;
DROP TABLE IF EXISTS Seats;
DROP TABLE IF EXISTS Carriages;
DROP TABLE IF EXISTS TrainTrips;
DROP TABLE IF EXISTS Trains;
DROP TABLE IF EXISTS Routes;
DROP TABLE IF EXISTS Stations;
SET FOREIGN_KEY_CHECKS = 1;

CREATE TABLE Stations (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Code VARCHAR(10) NOT NULL UNIQUE,
    Name VARCHAR(100) NOT NULL,
    City VARCHAR(100) NOT NULL,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB;

CREATE TABLE Routes (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    DepartureStationId INT NOT NULL,
    ArrivalStationId INT NOT NULL,
    DistanceKm DECIMAL(8, 2) NOT NULL,
    CONSTRAINT FK_Routes_DepartureStation FOREIGN KEY (DepartureStationId) REFERENCES Stations(Id) ON DELETE RESTRICT,
    CONSTRAINT FK_Routes_ArrivalStation FOREIGN KEY (ArrivalStationId) REFERENCES Stations(Id) ON DELETE RESTRICT
) ENGINE=InnoDB;

CREATE TABLE Trains (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Code VARCHAR(20) NOT NULL UNIQUE,
    Name VARCHAR(100) NOT NULL,
    TotalCapacity INT NOT NULL,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB;

CREATE TABLE Carriages (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TrainId INT NOT NULL,
    CarriageNumber INT NOT NULL,
    Type VARCHAR(50) NOT NULL,
    TotalSeats INT NOT NULL,
    CONSTRAINT FK_Carriages_Train FOREIGN KEY (TrainId) REFERENCES Trains(Id) ON DELETE CASCADE
) ENGINE=InnoDB;

CREATE TABLE Seats (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    CarriageId INT NOT NULL,
    SeatNumber VARCHAR(10) NOT NULL,
    SeatType VARCHAR(50) NOT NULL,
    CONSTRAINT FK_Seats_Carriage FOREIGN KEY (CarriageId) REFERENCES Carriages(Id) ON DELETE CASCADE
) ENGINE=InnoDB;

CREATE TABLE TrainTrips (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TrainId INT NOT NULL,
    RouteId INT NOT NULL,
    DepartureTime DATETIME NOT NULL,
    ArrivalTime DATETIME NOT NULL,
    Status ENUM('Scheduled', 'Delayed', 'Completed', 'Cancelled') NOT NULL DEFAULT 'Scheduled',
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_TrainTrips_Train FOREIGN KEY (TrainId) REFERENCES Trains(Id) ON DELETE CASCADE,
    CONSTRAINT FK_TrainTrips_Route FOREIGN KEY (RouteId) REFERENCES Routes(Id) ON DELETE RESTRICT
) ENGINE=InnoDB;

CREATE TABLE TripSeatPrices (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TrainTripId INT NOT NULL,
    SeatTypeId VARCHAR(50) NOT NULL,
    Price DECIMAL(10, 2) NOT NULL,
    CONSTRAINT FK_TripSeatPrices_TrainTrip FOREIGN KEY (TrainTripId) REFERENCES TrainTrips(Id) ON DELETE CASCADE
) ENGINE=InnoDB;

CREATE TABLE Users (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Email VARCHAR(255) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    FullName VARCHAR(100) NOT NULL,
    PhoneNumber VARCHAR(20) NOT NULL,
    Role VARCHAR(50) NOT NULL DEFAULT 'Customer',
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB;

CREATE TABLE Bookings (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    BookingCode VARCHAR(50) NOT NULL UNIQUE,
    TotalAmount DECIMAL(12, 2) NOT NULL,
    Status VARCHAR(50) NOT NULL DEFAULT 'Pending',
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    ExpiresAt DATETIME NOT NULL,
    CONSTRAINT FK_Bookings_User FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE RESTRICT
) ENGINE=InnoDB;

CREATE TABLE BookingItems (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    BookingId INT NOT NULL,
    TrainTripId INT NOT NULL,
    SeatId INT NOT NULL,
    PassengerName VARCHAR(100) NOT NULL,
    PassengerIdCard VARCHAR(20) NOT NULL,
    Price DECIMAL(10, 2) NOT NULL,
    CONSTRAINT FK_BookingItems_Booking FOREIGN KEY (BookingId) REFERENCES Bookings(Id) ON DELETE CASCADE,
    CONSTRAINT FK_BookingItems_TrainTrip FOREIGN KEY (TrainTripId) REFERENCES TrainTrips(Id) ON DELETE RESTRICT,
    CONSTRAINT FK_BookingItems_Seat FOREIGN KEY (SeatId) REFERENCES Seats(Id) ON DELETE RESTRICT
) ENGINE=InnoDB;

CREATE TABLE Payments (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    BookingId INT NOT NULL,
    TransactionId VARCHAR(100) NOT NULL UNIQUE,
    Amount DECIMAL(12, 2) NOT NULL,
    PaymentMethod VARCHAR(50) NOT NULL,
    Status VARCHAR(50) NOT NULL DEFAULT 'Pending',
    PaidAt DATETIME NULL,
    CONSTRAINT FK_Payments_Booking FOREIGN KEY (BookingId) REFERENCES Bookings(Id) ON DELETE CASCADE
) ENGINE=InnoDB;

-- 1. Stations (5 bản ghi)
INSERT INTO Stations (Code, Name, City) VALUES
('SGN', 'Ga Sài Gòn', 'Hồ Chí Minh'),
('DAD', 'Ga Đà Nẵng', 'Đà Nẵng'),
('HN', 'Ga Hà Nội', 'Hà Nội'),
('HUE', 'Ga Huế', 'Thừa Thiên Huế'),
('NHA', 'Ga Nha Trang', 'Khánh Hòa');

-- 2. Routes (5 bản ghi)
INSERT INTO Routes (DepartureStationId, ArrivalStationId, DistanceKm) VALUES
(1, 2, 935.00), -- Sài Gòn - Đà Nẵng
(2, 3, 791.00), -- Đà Nẵng - Hà Nội
(1, 4, 688.00), -- Sài Gòn - Huế
(4, 3, 688.00), -- Huế - Hà Nội
(1, 5, 411.00); -- Sài Gòn - Nha Trang

-- 3. Trains (5 bản ghi)
INSERT INTO Trains (Code, Name, TotalCapacity) VALUES
('SE2', 'Tàu Thống Nhất SE2', 500),
('SE7', 'Tàu Thống Nhất SE7', 450),
('SE4', 'Tàu Thống Nhất SE4', 480),
('SE21', 'Tàu Nam Bắc SE21', 400),
('SNT1', 'Tàu Sao Mai SNT1', 420);

-- 4. Carriages (5 bản ghi)
INSERT INTO Carriages (TrainId, CarriageNumber, Type, TotalSeats) VALUES
(1, 1, 'Hard Bed', 64),
(1, 2, 'Soft Seat', 80),
(2, 1, 'Soft Bed', 42),
(3, 1, 'VIP Cabin', 32),
(4, 1, 'Soft Seat', 72);

-- 5. Seats (5 bản ghi)
INSERT INTO Seats (CarriageId, SeatNumber, SeatType) VALUES
(1, 'A01', 'Lower Berth'),
(1, 'A02', 'Middle Berth'),
(2, 'B01', 'Window Seat'),
(2, 'B02', 'Aisle Seat'),
(3, 'C01', 'Upper Berth');

-- 6. TrainTrips (5 bản ghi)
INSERT INTO TrainTrips (TrainId, RouteId, DepartureTime, ArrivalTime, Status) VALUES
(1, 1, '2026-11-10 06:00:00', '2026-11-10 23:30:00', 'Scheduled'),
(2, 2, '2026-11-11 08:00:00', '2026-11-11 22:00:00', 'Scheduled'),
(3, 3, '2026-11-12 07:15:00', '2026-11-12 21:00:00', 'Scheduled'),
(4, 4, '2026-11-13 09:30:00', '2026-11-13 23:00:00', 'Scheduled'),
(5, 5, '2026-11-14 20:00:00', '2026-11-15 05:30:00', 'Scheduled');

-- 7. TripSeatPrices (5 bản ghi)
INSERT INTO TripSeatPrices (TrainTripId, SeatTypeId, Price) VALUES
(1, 'Lower Berth', 1200000.00),
(1, 'Soft Seat', 750000.00),
(2, 'Window Seat', 850000.00),
(3, 'VIP Cabin', 1500000.00),
(4, 'Soft Seat', 700000.00);

-- 8. Users (5 bản ghi)
INSERT INTO Users (Email, PasswordHash, FullName, PhoneNumber, Role) VALUES
('customer1@gmail.com', 'AQAAAAEAACcQAAAAEP...', 'Nguyen Van A', '0912345678', 'Customer'),
('customer2@gmail.com', 'AQAAAAEAACcQAAAAEQ...', 'Tran Thi B', '0987654321', 'Customer'),
('customer3@gmail.com', 'AQAAAAEAACcQAAAAER...', 'Le Van C', '0905123456', 'Customer'),
('staff1@train.com', 'AQAAAAEAACcQAAAAES...', 'Pham Nhan Vien', '0935678901', 'Staff'),
('admin@train.com', 'AQAAAAEAACcQAAAAET...', 'Admin System', '0911223344', 'Admin');

-- 9. Bookings (5 bản ghi)
INSERT INTO Bookings (UserId, BookingCode, TotalAmount, Status, ExpiresAt) VALUES
(1, 'BK20261103001', 1950000.00, 'Pending', '2026-11-03 18:00:00'),
(2, 'BK20261103002', 850000.00, 'Confirmed', '2026-11-03 19:00:00'),
(3, 'BK20261103003', 1500000.00, 'Completed', '2026-11-03 20:00:00'),
(1, 'BK20261103004', 700000.00, 'Cancelled', '2026-11-03 21:00:00'),
(2, 'BK20261103005', 1200000.00, 'Pending', '2026-11-03 22:00:00');

-- 10. BookingItems (5 bản ghi)
INSERT INTO BookingItems (BookingId, TrainTripId, SeatId, PassengerName, PassengerIdCard, Price) VALUES
(1, 1, 1, 'Nguyen Van A', '123456789012', 1200000.00),
(1, 1, 3, 'Nguyen Thi An', '123456789013', 750000.00),
(2, 2, 3, 'Tran Thi B', '098765432109', 850000.00),
(3, 3, 5, 'Le Van C', '044098765432', 1500000.00),
(5, 1, 2, 'Nguyen Van D', '044123456789', 1200000.00);

-- 11. Payments (5 bản ghi)
INSERT INTO Payments (BookingId, TransactionId, Amount, PaymentMethod, Status, PaidAt) VALUES
(1, 'TXN987654321', 1950000.00, 'VNPAY', 'Completed', '2026-11-03 15:00:00'),
(2, 'TXN987654322', 850000.00, 'MOMO', 'Completed', '2026-11-03 15:30:00'),
(3, 'TXN987654323', 1500000.00, 'ZALOPAY', 'Completed', '2026-11-03 16:00:00'),
(4, 'TXN987654324', 700000.00, 'VNPAY', 'Cancelled', NULL),
(5, 'TXN987654325', 1200000.00, 'MOMO', 'Pending', NULL);
CREATE DATABASE RaceDay;
GO

USE RaceDay;
GO

CREATE TABLE Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    Role NVARCHAR(20) NOT NULL CHECK (Role IN ('Organiser', 'Participant')),
    ProfilePictureURL NVARCHAR(500) NULL,
    DateOfBirth DATE NULL,
    DateCreated DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE Events (
    EventID INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    Date DATETIME NOT NULL,
    Location NVARCHAR(200) NOT NULL,
    Distance NVARCHAR(50) NOT NULL,
    EventType NVARCHAR(20) NOT NULL CHECK (EventType IN ('Run', 'Walk', 'Cycle')),
    CreatedByUserID INT NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Events_Users FOREIGN KEY (CreatedByUserID) REFERENCES Users(UserID)
);

CREATE TABLE Categories (
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    EventID INT NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(255) NULL,
    CONSTRAINT FK_Categories_Events FOREIGN KEY (EventID) REFERENCES Events(EventID) ON DELETE CASCADE
);

CREATE TABLE Enrolments (
    EnrolmentID INT IDENTITY(1,1) PRIMARY KEY,
    ParticipantUserID INT NOT NULL,
    EventID INT NOT NULL,
    CategoryID INT NOT NULL,
    EnrolmentDate DATETIME NOT NULL DEFAULT GETDATE(),
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending' CHECK (Status IN ('Pending', 'Confirmed', 'Cancelled')),
    CONSTRAINT FK_Enrolments_Users FOREIGN KEY (ParticipantUserID) REFERENCES Users(UserID),
    CONSTRAINT FK_Enrolments_Events FOREIGN KEY (EventID) REFERENCES Events(EventID),
    CONSTRAINT FK_Enrolments_Categories FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID),
    CONSTRAINT UQ_Enrolment_Participant_Event UNIQUE (ParticipantUserID, EventID)
);

CREATE TABLE Results (
    ResultID INT IDENTITY(1,1) PRIMARY KEY,
    EnrolmentID INT NOT NULL UNIQUE,
    FinishTime TIME(0) NOT NULL,
    FinishPosition INT NOT NULL,
    DateCaptured DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Results_Enrolments FOREIGN KEY (EnrolmentID) REFERENCES Enrolments(EnrolmentID)
);

CREATE TABLE EventImages (
    ImageID INT IDENTITY(1,1) PRIMARY KEY,
    EventID INT NOT NULL,
    ImageURL NVARCHAR(500) NOT NULL,
    IsBanner BIT NOT NULL DEFAULT 0,
    UploadedDate DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_EventImages_Events FOREIGN KEY (EventID) REFERENCES Events(EventID) ON DELETE CASCADE
);

-- Insert Organisers
DECLARE @DummyHash NVARCHAR(255) = 'AF23...DummyHash...';
INSERT INTO Users (FirstName, LastName, Email, PasswordHash, Role, DateOfBirth)
VALUES
    ('Theo', 'Modise', 'theo.modise@raceday.co.za', @DummyHash, 'Organiser', '1985-06-15'),
    ('Lindiwe', 'Nkosi', 'lindiwe.nkosi@raceday.co.za', @DummyHash, 'Organiser', '1990-11-02');
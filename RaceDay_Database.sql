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

 -- Insert Participants
DECLARE @DummyHash NVARCHAR(255) = 'AF23...DummyHash...';
INSERT INTO Users (FirstName, LastName, Email, PasswordHash, Role, DateOfBirth)
VALUES
    ('Sipho', 'Mthembu', 'sipho.runner@gmail.com', @DummyHash, 'Participant', '1995-03-20'),
    ('Zanele', 'Petersen', 'zanele.cycle@yahoo.com', @DummyHash, 'Participant', '1988-07-10'),
    ('Thabo', 'Botha', 'thabo.walker@outlook.com', @DummyHash, 'Participant', '2000-12-05');

    -- Insert Events
INSERT INTO Events (Name, Description, Date, Location, Distance, EventType, CreatedByUserID)
VALUES
    ('Soweto Marathon 2026', 'The iconic 42.2km and 21.1km road race through the streets of Soweto.', '2026-11-15 06:00:00', 'Soweto, Johannesburg', '42.2km', 'Run', (SELECT UserID FROM Users WHERE Email = 'theo.modise@raceday.co.za')),
    ('Cape Town Cycle Tour 2026', 'The world''s largest timed cycling event, a stunning 109km route around the Cape Peninsula.', '2026-03-08 07:00:00', 'Cape Town', '109km', 'Cycle', (SELECT UserID FROM Users WHERE Email = 'lindiwe.nkosi@raceday.co.za')),
    ('Durban City Walk 2026', 'A scenic 10km walk along the Durban beachfront and through the city center.', '2026-09-20 08:00:00', 'Durban', '10km', 'Walk', (SELECT UserID FROM Users WHERE Email = 'theo.modise@raceday.co.za'));

    -- Insert Categories for Soweto Marathon
DECLARE @SowetoEventID INT = (SELECT EventID FROM Events WHERE Name = 'Soweto Marathon 2026');
INSERT INTO Categories (EventID, Name, Description) VALUES
    (@SowetoEventID, '42.2km Open', 'Full marathon for participants of all ages (16+)'),
    (@SowetoEventID, '21.1km Open', 'Half marathon for participants of all ages (16+)'),
    (@SowetoEventID, '42.2km Junior', 'Full marathon for participants aged 18-25'),
    (@SowetoEventID, '42.2km Veteran', 'Full marathon for participants aged 50+');

    -- Insert Categories for Cape Town Cycle Tour
DECLARE @CTCycleEventID INT = (SELECT EventID FROM Events WHERE Name = 'Cape Town Cycle Tour 2026');
INSERT INTO Categories (EventID, Name, Description) VALUES
    (@CTCycleEventID, '109km Elite', 'Elite category with a competitive start group'),
    (@CTCycleEventID, '109km Open', 'Open category for cyclists of all abilities'),
    (@CTCycleEventID, '109km Tandem', 'Category for tandem bicycle teams');

    -- Insert Categories for Durban City Walk
DECLARE @DurbanEventID INT = (SELECT EventID FROM Events WHERE Name = 'Durban City Walk 2026');
INSERT INTO Categories (EventID, Name, Description) VALUES
    (@DurbanEventID, '10km Walk', 'The main 10km walking event'),
    (@DurbanEventID, '5km Fun Walk', 'A shorter, family-friendly 5km walking route');
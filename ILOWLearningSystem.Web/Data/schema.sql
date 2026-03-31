CREATE DATABASE IF NOT EXISTS `ilow_learning_system`
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;

USE `ilow_learning_system`;

CREATE TABLE IF NOT EXISTS `Users` (
  `UserId` INT NOT NULL AUTO_INCREMENT,
  `FullName` VARCHAR(200) NOT NULL,
  `Email` VARCHAR(320) NOT NULL,
  `Password` VARCHAR(500) NOT NULL,
  `Role` VARCHAR(50) NOT NULL,
  `CreatedAt` DATETIME(6) NOT NULL,
  PRIMARY KEY (`UserId`),
  UNIQUE KEY `UX_Users_Email` (`Email`)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS `Courses` (
  `CourseId` INT NOT NULL AUTO_INCREMENT,
  `Title` VARCHAR(200) NOT NULL,
  `Description` VARCHAR(2000) NULL,
  `Category` VARCHAR(100) NULL,
  `LecturerName` VARCHAR(200) NULL,
  `ImagePath` VARCHAR(1000) NULL,
  `CreatedAt` DATETIME(6) NOT NULL,
  PRIMARY KEY (`CourseId`)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS `Enrollments` (
  `EnrollmentId` INT NOT NULL AUTO_INCREMENT,
  `UserId` INT NOT NULL,
  `CourseId` INT NOT NULL,
  `EnrolledAt` DATETIME(6) NOT NULL,
  `Status` VARCHAR(50) NOT NULL,
  PRIMARY KEY (`EnrollmentId`),
  UNIQUE KEY `UX_Enrollments_User_Course` (`UserId`, `CourseId`),
  CONSTRAINT `FK_Enrollments_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`UserId`) ON DELETE CASCADE,
  CONSTRAINT `FK_Enrollments_Courses_CourseId` FOREIGN KEY (`CourseId`) REFERENCES `Courses` (`CourseId`) ON DELETE CASCADE
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS `Lessons` (
  `LessonId` INT NOT NULL AUTO_INCREMENT,
  `CourseId` INT NOT NULL,
  `Title` VARCHAR(200) NOT NULL,
  `Content` LONGTEXT NULL,
  `SlidePath` VARCHAR(1000) NULL,
  `VideoPath` VARCHAR(1000) NULL,
  `CreatedAt` DATETIME(6) NOT NULL,
  PRIMARY KEY (`LessonId`),
  KEY `IX_Lessons_CourseId` (`CourseId`),
  CONSTRAINT `FK_Lessons_Courses_CourseId` FOREIGN KEY (`CourseId`) REFERENCES `Courses` (`CourseId`) ON DELETE CASCADE
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS `Assignments` (
  `AssignmentId` INT NOT NULL AUTO_INCREMENT,
  `CourseId` INT NOT NULL,
  `Title` VARCHAR(200) NOT NULL,
  `Description` VARCHAR(4000) NULL,
  `DueDate` DATETIME(6) NULL,
  `TotalMarks` INT NOT NULL,
  `CreatedAt` DATETIME(6) NOT NULL,
  PRIMARY KEY (`AssignmentId`),
  KEY `IX_Assignments_CourseId` (`CourseId`),
  CONSTRAINT `FK_Assignments_Courses_CourseId` FOREIGN KEY (`CourseId`) REFERENCES `Courses` (`CourseId`) ON DELETE CASCADE
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS `Submissions` (
  `SubmissionId` INT NOT NULL AUTO_INCREMENT,
  `AssignmentId` INT NOT NULL,
  `UserId` INT NOT NULL,
  `FilePath` VARCHAR(1000) NULL,
  `SubmittedAt` DATETIME(6) NOT NULL,
  `Status` VARCHAR(50) NOT NULL,
  `Score` INT NULL,
  `Feedback` VARCHAR(2000) NULL,
  PRIMARY KEY (`SubmissionId`),
  KEY `IX_Submissions_AssignmentId` (`AssignmentId`),
  KEY `IX_Submissions_UserId` (`UserId`),
  CONSTRAINT `FK_Submissions_Assignments_AssignmentId` FOREIGN KEY (`AssignmentId`) REFERENCES `Assignments` (`AssignmentId`) ON DELETE CASCADE,
  CONSTRAINT `FK_Submissions_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`UserId`) ON DELETE CASCADE
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS `Flashcards` (
  `FlashcardId` INT NOT NULL AUTO_INCREMENT,
  `LessonId` INT NOT NULL,
  `Question` VARCHAR(2000) NOT NULL,
  `Answer` VARCHAR(2000) NOT NULL,
  `DifficultyLevel` INT NOT NULL,
  `CreatedAt` DATETIME(6) NOT NULL,
  PRIMARY KEY (`FlashcardId`),
  KEY `IX_Flashcards_LessonId` (`LessonId`),
  CONSTRAINT `FK_Flashcards_Lessons_LessonId` FOREIGN KEY (`LessonId`) REFERENCES `Lessons` (`LessonId`) ON DELETE CASCADE
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS `DailyTasks` (
  `TaskId` INT NOT NULL AUTO_INCREMENT,
  `UserId` INT NOT NULL,
  `Title` VARCHAR(200) NOT NULL,
  `Description` VARCHAR(2000) NULL,
  `DueDate` DATETIME(6) NULL,
  `Status` VARCHAR(50) NOT NULL,
  `CreatedAt` DATETIME(6) NOT NULL,
  PRIMARY KEY (`TaskId`),
  KEY `IX_DailyTasks_UserId` (`UserId`),
  CONSTRAINT `FK_DailyTasks_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`UserId`) ON DELETE CASCADE
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS `StressRecords` (
  `StressRecordId` INT NOT NULL AUTO_INCREMENT,
  `UserId` INT NOT NULL,
  `StressLevel` INT NOT NULL,
  `Note` VARCHAR(2000) NULL,
  `RecordedAt` DATETIME(6) NOT NULL,
  PRIMARY KEY (`StressRecordId`),
  KEY `IX_StressRecords_UserId` (`UserId`),
  CONSTRAINT `FK_StressRecords_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`UserId`) ON DELETE CASCADE
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS `Announcements` (
  `AnnouncementId` INT NOT NULL AUTO_INCREMENT,
  `Title` VARCHAR(200) NOT NULL,
  `Content` VARCHAR(4000) NOT NULL,
  `CreatedBy` INT NOT NULL,
  `CreatedAt` DATETIME(6) NOT NULL,
  PRIMARY KEY (`AnnouncementId`),
  KEY `IX_Announcements_CreatedBy` (`CreatedBy`),
  CONSTRAINT `FK_Announcements_Users_CreatedBy` FOREIGN KEY (`CreatedBy`) REFERENCES `Users` (`UserId`) ON DELETE RESTRICT
) ENGINE=InnoDB;

SELECT (SELECT COUNT(*) FROM Artists)            AS Artists,
       (SELECT COUNT(*) FROM Dsps)               AS Dsps,
       (SELECT COUNT(*) FROM Tracks)             AS Tracks,
       (SELECT COUNT(*) FROM TrackDistributions) AS Distributions;
-- expected: 4, 3, 11, 14

SELECT Status, COUNT(*) AS Total FROM Tracks GROUP BY Status;
-- expected: Draft 3, Submitted 5, Distributed 3

SELECT t.Title, a.Name AS Artist, t.Genre, t.Status, t.Isrc
FROM Tracks t JOIN Artists a ON a.Id = t.ArtistId
ORDER BY t.ReleaseDate;


-- 1. Duplicate ISRC -> error 2601, unique index 'UX_Tracks_Isrc'
INSERT INTO Tracks (Id, Title, ArtistId, Isrc, ReleaseDate, Genre, Status)
SELECT NEWID(), 'Dup', ArtistId, Isrc, ReleaseDate, Genre, 'Draft'
FROM Tracks WHERE Title = 'Sahar El Leil';

-- 2. Invalid status -> check constraint 'CK_Tracks_Status'
UPDATE Tracks SET Status = 'Banana' WHERE Title = 'Beirut Lullaby';

-- 3. Same DSP twice for one track -> unique index 'UX_TrackDistributions_TrackId_DspId'
INSERT INTO TrackDistributions (Id, TrackId, DspId, SubmittedAt, Status)
SELECT TOP (1) NEWID(), TrackId, DspId, SYSDATETIMEOFFSET(), 'Pending' FROM TrackDistributions;

-- 4. Deleting an artist who has tracks -> FK violation (Restrict)
DELETE FROM Artists WHERE Name = 'Nour El-Sayed';
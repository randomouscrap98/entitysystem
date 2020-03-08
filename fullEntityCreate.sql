BEGIN TRANSACTION;
CREATE TABLE IF NOT EXISTS "EntityValues" (
	"id"	INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	"createDate"	TEXT NOT NULL,
	"entityId"	INTEGER NOT NULL,
	"key"	TEXT COLLATE NOCASE,
	"value"	TEXT COLLATE NOCASE
);
CREATE TABLE IF NOT EXISTS "EntityRelations" (
	"id"	INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	"createDate"	TEXT NOT NULL,
	"entityId1"	INTEGER NOT NULL,
	"entityId2"	INTEGER NOT NULL,
	"type"	TEXT COLLATE NOCASE,
	"value"	TEXT COLLATE NOCASE
);
CREATE TABLE IF NOT EXISTS "Entities" (
	"id"	INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	"createDate"	TEXT NOT NULL,
	"type"	TEXT COLLATE NOCASE,
	"name"	TEXT COLLATE NOCASE,
	"content"	TEXT
);
CREATE INDEX IF NOT EXISTS "entityRelationId1Index" ON "EntityRelations" (
	"entityId1"
);
CREATE INDEX IF NOT EXISTS "entityRelationId2Index" ON "EntityRelations" (
	"entityId2"
);
CREATE INDEX IF NOT EXISTS "entityRelationTypeIndex" ON "EntityRelations" (
	"type"
);
CREATE INDEX IF NOT EXISTS "entityValueEntityIdIndex" ON "EntityValues" (
	"entityId"
);
CREATE INDEX IF NOT EXISTS "entityValueKeyIndex" ON "EntityValues" (
	"key"
);
CREATE INDEX IF NOT EXISTS "entityValueValueIndex" ON "EntityValues" (
	"value"
);
CREATE INDEX IF NOT EXISTS "entityTypeIndex" ON "Entities" (
	"type"
);
CREATE INDEX IF NOT EXISTS "entityNameIndex" ON "Entities" (
	"name"
);
COMMIT;

BEGIN TRANSACTION;
CREATE TABLE IF NOT EXISTS "EntityValues" (
	"id"	INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	"createDate"	TEXT,
	"entityId"	INTEGER NOT NULL,
	"key"	TEXT COLLATE NOCASE,
	"value"	TEXT COLLATE NOCASE
);
CREATE TABLE IF NOT EXISTS "EntityRelations" (
	"id"	INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	"createDate"	TEXT,
	"entityId1"	INTEGER NOT NULL,
	"entityId2"	INTEGER NOT NULL,
	"type"	TEXT COLLATE NOCASE,
	"value"	TEXT COLLATE NOCASE
);
CREATE TABLE IF NOT EXISTS "Entities" (
	"id"	INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	"createDate"	TEXT,
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
--This index is for quickly finding values for a given entity (ie finding 
--all fields for a user or all fields for a content)
CREATE INDEX IF NOT EXISTS "entityValueEntityIdIndex" ON "EntityValues" (
	"entityId"
);
--This index is for quickly searching through values. Value table is MAINLY
--for storing... values, so chances are you will want to search through it.
--Notice secondary index "key"; most value lookups will be finding both key
--AND value: like finding keywords will be value="thekeyword" and key="keyword".
--Adding 'key' means it doesn't have to go index the table to find the key. 
--For values with MANY mismatched keys, this could have a huge impact. For 
--valaues that are mostly within the desired key, this won't have ANY impact,
--since we still have to go to the table to get the rest of the data for the object
CREATE INDEX IF NOT EXISTS "entityValueValueKeyIndex" ON "EntityValues" (
	"value", "key"
);
CREATE INDEX IF NOT EXISTS "entityTypeIndex" ON "Entities" (
	"type"
);
CREATE INDEX IF NOT EXISTS "entityNameIndex" ON "Entities" (
	"name"
);
COMMIT;

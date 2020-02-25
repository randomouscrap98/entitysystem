using System;

namespace entitysystem
{
    public class Entity
    {
        public long id {get;set;}
        public string name {get;set;}
        public string content {get;set;}
        public DateTime createDate {get;set;}
        public string type {get;set;}
    }

    public class EntityRelation
    {
        public long id {get;set;}
        public long entityId1 {get;set;}
        public long entityId2 {get;set;}
        public string type {get;set;}
        public string value {get;set;}
        public DateTime createDate {get;set;}
    }

    public class EntityValue
    {
        public long id {get;set;}
        public long entityId {get;set;}
        public string key {get;set;}
        public string value{get;set;}
        public DateTime createDate {get;set;}
    }
}
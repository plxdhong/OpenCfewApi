using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CFEW.Shared
{
    public class UserDetail
    {
        [BsonId]
        public string Id { get; set; }
        public string Uuid {  get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Qq { get; set; }
        public string Wechat { get; set; }
        public DateTime Created { get; set; }
    }
}

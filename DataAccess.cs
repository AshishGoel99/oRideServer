using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System.Collections.Generic;
using oServer.DbModels;

namespace oServer
{
    public class DataAccess
    {
        MongoClient _client;
        MongoServer _server;
        MongoDatabase _db;

        public DataAccess()
        {
            _client = new MongoClient("mongodb://localhost:27017");
            _server = _client.GetServer();
            _db = _server.GetDatabase("oRideDb");
        }

        private string GetCollectionName<T>()
        {
            if (typeof(T) == typeof(Ride))
                return "Ride";
            else if (typeof(T) == typeof(User))
                return "User";
            else if (typeof(T) == typeof(Location))
                return "Location";

            return string.Empty;
        }

        public IEnumerable<T> GetAll<T>()
        {
            var col = GetCollectionName<T>();
            return _db.GetCollection<T>(col).FindAll();
        }


        public T GetItem<T>(ObjectId id) where T : Base
        {
            var col = GetCollectionName<T>();
            var res = Query<T>.EQ(p => p.Id, id);
            return _db.GetCollection<T>(col).FindOne(res);
        }

        public T CreateItem<T>(T p)
        {
            var col = GetCollectionName<T>();
            _db.GetCollection<T>(col).Save(p);
            return p;
        }

        public void UpdateItem<T>(ObjectId id, T p) where T : Base
        {
            p.Id = id;
            var col = GetCollectionName<T>();
            var res = Query<T>.EQ(pd => pd.Id, id);
            var operation = Update<T>.Replace(p);
            _db.GetCollection<T>(col).Update(res, operation);
        }
        public void RemoveItem<T>(ObjectId id) where T : Base
        {
            var res = Query<T>.EQ(e => e.Id, id);
            var col = GetCollectionName<T>();
            var operation = _db.GetCollection<T>(col).Remove(res);
        }
    }
}
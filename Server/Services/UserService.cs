using CFEW.Server.Models;
using CFEW.Shared;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CFEW.Server.Services;
public class UserService
{
    private readonly IMongoCollection<UserDetail> _users;
    public UserService(IXrayUsersstoreDatabaseSettings settings)
    {
        var client = new MongoClient(settings.ConnectionString);
        var database = client.GetDatabase(settings.DatabaseName);

        _users=database.GetCollection<UserDetail>(settings.XrayUsersCollectionName);
    }

    public List<UserDetail> Get() =>
        _users.Find(user => true).ToList();

    public UserDetail Get(string id) =>
        _users.Find<UserDetail>(user => user.Id==id).FirstOrDefault();

    public UserDetail GetFormEmail(string Email) =>
    _users.Find<UserDetail>(user => user.Email == Email).FirstOrDefault();

    public async Task<UserDetail> CreateAsync(UserDetail user)
    {
        await _users.InsertOneAsync(user);
        return user;
    }

    public void Update(string id, UserDetail userIn) =>
        _users.ReplaceOne(user => user.Id == id, userIn);

    public void Delete(string id) =>
        _users.DeleteOne(user => user.Id == id);

    public void Delete(UserDetail userIn) =>
        _users.DeleteOne(user => user.Id == userIn.Id);
}

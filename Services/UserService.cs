using Chatup.Entities;
using MongoDB.Driver;

public class UserService
{
    private readonly IMongoCollection<User> _users;

    public UserService(IMongoDatabase db)
    {
        _users = db.GetCollection<User>("Users");
    }

    public async Task UpdateUserOnlineStatusAsync(string userId, bool isOnline)
    {
        var update = Builders<User>.Update
            .Set(u => u.IsOnline, isOnline)
            .Set(u => u.LastSeen, DateTime.UtcNow);

        await _users.UpdateOneAsync(u => u.Id == userId, update);
    }
}
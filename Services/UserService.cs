using Chatup.DTOs;
using Chatup.Entities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Chatup.Services;

public class UserService
{
    private readonly IMongoCollection<User> _users;

    public UserService(IMongoDatabase database)
    {
        _users = database.GetCollection<User>("Users");
    }

    public async Task<UserResponse?> GetUserByIdAsync(string userId)
    {
        var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
        if (user == null) return null;
        
        return new UserResponse(user.Id, user.PhoneNumber, user.Nickname, user.About, user.AvatarUrl);
    }

    public async Task UpdateProfileAsync(string userId, UpdateProfileRequest request)
    {
        var update = Builders<User>.Update
            .Set(u => u.Nickname, request.Nickname)
            .Set(u => u.About, request.About)
            .Set(u => u.AvatarUrl, request.AvatarUrl);

        await _users.UpdateOneAsync(u => u.Id == userId, update);
    }

    public async Task<(bool Success, string Message)> AddContactAsync(string userId, string contactPhoneNumber)
    {
        var contactUser = await _users.Find(u => u.PhoneNumber == contactPhoneNumber).FirstOrDefaultAsync();
        
        if (contactUser == null) return (false, "User with this phone number not found.");
        if (contactUser.Id == userId) return (false, "You cannot add yourself as a contact.");

        // add contact ID to the list if it's not already there
        var update = Builders<User>.Update.AddToSet(u => u.ContactIds, contactUser.Id);
        await _users.UpdateOneAsync(u => u.Id == userId, update);

        return (true, "Contact added successfully.");
    }

    public async Task<List<UserResponse>> GetContactsAsync(string userId)
    {
        var me = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
        if (me == null || me.ContactIds.Count == 0) return new List<UserResponse>();

        // find all users whose IDs are in my ContactIds list
        var filter = Builders<User>.Filter.In(u => u.Id, me.ContactIds);
        var contacts = await _users.Find(filter).ToListAsync();

        return contacts.Select(u => new UserResponse(u.Id, u.PhoneNumber, u.Nickname, u.About, u.AvatarUrl)).ToList();
    }

    public async Task<(bool Success, string Message)> BlockUserAsync(string userId, string blockPhoneNumber)
    {
        var userToBlock = await _users.Find(u => u.PhoneNumber == blockPhoneNumber).FirstOrDefaultAsync();
        
        if (userToBlock == null) return (false, "User not found.");
        if (userToBlock.Id == userId) return (false, "You cannot block yourself.");

        var update = Builders<User>.Update.AddToSet(u => u.BlockedUserIds, userToBlock.Id);
        await _users.UpdateOneAsync(u => u.Id == userId, update);

        return (true, "User blocked successfully.");
    }

    public async Task<List<UserResponse>> GetBlockedUsersAsync(string userId)
    {
        var me = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
        if (me == null || me.BlockedUserIds.Count == 0) return new List<UserResponse>();

        var filter = Builders<User>.Filter.In(u => u.Id, me.BlockedUserIds);
        var blockedUsers = await _users.Find(filter).ToListAsync();

        return blockedUsers.Select(u => new UserResponse(u.Id, u.PhoneNumber, u.Nickname, u.About, u.AvatarUrl)).ToList();
    }

    public async Task<(bool Success, string Message)> UnblockUserAsync(string userId, string unblockPhoneNumber)
    {
        var userToUnblock = await _users.Find(u => u.PhoneNumber == unblockPhoneNumber).FirstOrDefaultAsync();
        if (userToUnblock == null) return (false, "User not found.");

        var update = Builders<User>.Update.Pull(u => u.BlockedUserIds, userToUnblock.Id);
        await _users.UpdateOneAsync(u => u.Id == userId, update);

        return (true, "User unblocked successfully.");
    }
    
    public async Task<List<UserResponse>> SearchUsersAsync(string query, string currentUserId)
    {
        var filter = Builders<User>.Filter.And(
            Builders<User>.Filter.Ne(u => u.Id, currentUserId),
            Builders<User>.Filter.Or(
                Builders<User>.Filter.Regex(u => u.PhoneNumber, new BsonRegularExpression(query, "i")),
                Builders<User>.Filter.Regex(u => u.Nickname, new BsonRegularExpression(query, "i"))
            )
        );

        var users = await _users.Find(filter).Limit(20).ToListAsync();
        return users.Select(u => new UserResponse(u.Id, u.PhoneNumber, u.Nickname, u.About, u.AvatarUrl)).ToList();
    }
}
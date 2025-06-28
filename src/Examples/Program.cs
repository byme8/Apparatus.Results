using Apparatus.Results;

// Sync example
var (user, error) = GetUserById(1);
if (error)
{
    if (error is InvalidUserId invalidUserId)
    {
        Console.WriteLine($"Invalid user ID: {invalidUserId.Id}");
        return;
    }

    Console.WriteLine($"{error.Code}: {error.Message}");
}
else
{
    Console.WriteLine($"Sync - User ID: {user.Id}, Name: {user.Name}");
}

// Async example
var (asyncUser, asyncError) = await GetUserByIdAsync(2);
if (asyncError)
{
    Console.WriteLine($"Async error: {asyncError.Code}: {asyncError.Message}");
}
else
{
    Console.WriteLine($"Async - User ID: {asyncUser.Id}, Name: {asyncUser.Name}");
}

// Async chaining example
var result = await GetUserByIdAsync(3)
    .Select(u => $"Hello, {u.Name}!")
    .Do(async greeting => 
    {
        await Task.Delay(10);
        Console.WriteLine($"Async chaining: {greeting}");
    });

Result<User> GetUserById(int id)
{
    if (id <= 0)
    {
        return new InvalidUserId(id);
    }

    var user = new User(id, "John Doe");
    return user;
}

async Task<Result<User>> GetUserByIdAsync(int id)
{
    await Task.Delay(50); // Simulate async work
    
    if (id <= 0)
    {
        return new InvalidUserId(id);
    }

    var user = new User(id, $"Async User {id}");
    return user;
}

record User(int Id, string Name);
record InvalidUserId(int Id) 
    : Error("InvalidUserId", $"The user ID {Id} is invalid.");

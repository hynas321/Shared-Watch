namespace WebApi.Application.Services;
public interface IRoomPasswordService
{
    string Hash(string password);
    bool Verify(string password, string storedPassword);
}

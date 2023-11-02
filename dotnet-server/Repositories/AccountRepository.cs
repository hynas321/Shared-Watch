using Dapper;
using Microsoft.Data.Sqlite;

public class AccountRepository {
    private readonly string connectionString;

    public AccountRepository(IConfiguration configuration) {
        connectionString = configuration.GetConnectionString("DefaultConnection");

        using (SqliteConnection db = new SqliteConnection(connectionString))
        {
            string createTableQuery = $"CREATE TABLE IF NOT EXISTS Account (Username, Email, Password)";

            db.Open();
            db.Execute(createTableQuery);
        } 
    }

    public bool Insert(Account account)
    {
        using (SqliteConnection db = new SqliteConnection(connectionString))
        {
            string insertQuery = "INSERT OR IGNORE INTO Account (Username, Email, Password) " +
                "VALUES (@Username, @Email, @Password)";

            db.Open();

            int rowsAffected = db.Execute(insertQuery, account);

            return rowsAffected > 0;
        }
    }

    public bool Delete(string username)
    {
        using (SqliteConnection db = new SqliteConnection(connectionString))
        {
            string deleteQuery = "DELETE FROM Account WHERE Username = @Username";
            object parameters = new { Username = username };

            db.Open();

            int rowsAffected = db.Execute(deleteQuery, parameters);

            return rowsAffected > 0;
        }
    }

    public Account Get(string username)
    {
        using (SqliteConnection db = new SqliteConnection(connectionString))
        {
            string getQuery = "SELECT * FROM Account WHERE Username = @Username";
            object parameters = new { Username = "string1" };

            db.Open();

            return db.QueryFirstOrDefault<Account>(getQuery, parameters) ?? null;
        }
    }

    public IEnumerable<Account> GetAll()
    {
        using (SqliteConnection db = new SqliteConnection(connectionString))
        {
            string getAllQuery = "SELECT * FROM Account";

            db.Open();

            return db.Query<Account>(getAllQuery);
        }
    }
}
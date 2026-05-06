using Google.Protobuf.WellKnownTypes;
using Konscious.Security.Cryptography;
using Microsoft.Data.SqlClient;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Security.Cryptography;
using System.Text;


namespace Domain
{
    internal class Helpers
    {

        public class SP
        {
            public class User
            {
                public struct GetUserByID
                {
                    public const string Name = "GetUserByID";

                    public struct Param
                    {
                        public const string ID = "ID";
                        public const string statusFlag = "statusFlag";

                    }
                }

                public struct GetUserByIdAndGuid
                {
                    public const string Name = "GetUserByIdAndGuid";

                    public struct Param
                    {
                        public const string ID = "ID";
                        public const string guid = "guid";
                        public const string statusFlag = "statusFlag";

                    }
                }

                public struct GetUserByEmail
                {
                    public const string Name = "GetUserByEmail";

                    public struct Param
                    {
                        public const string email = "email";
                        public const string statusFlag = "statusFlag";

                    }
                }

                public struct GetUserByEmailAndPassword
                {
                    public const string Name = "GetUserByEmailAndPassword";

                    public struct Param
                    {
                        public const string email = "email";
                        public const string password = "password";
                        public const string statusFlag = "statusFlag";

                    }
                }

                public struct InsertUser
                {
                    public const string Name = "InsertUser";

                    public struct Param
                    {

                        public const string first_name = "first_name";
                        public const string last_name = "last_name";
                        public const string display_name = "display_name";
                        public const string bio = "bio";
                        public const string gender = "gender";
                        public const string birth_date = "birth_date";
                        public const string request = "request";
                        public const string entity = "entity";
                        public const string created_at = "created_at";
                        public const string updated_at = "updated_at";
                        public const string status = "status";
                        public const string updated_by = "updated_by";



                    }

                }

                public struct UpdateUserByID
                {
                    public const string Name = "GetUserByID";

                    public struct Param
                    {
                        public const string ID = "ID";
                        public const string guid = "guid";

                        public const string first_name = "first_name";
                        public const string last_name = "last_name";
                        public const string display_name = "display_name";
                        public const string bio = "bio";
                        public const string gender = "gender";
                        public const string birth_date = "birth_date";
                        public const string request = "request";
                        public const string entity = "entity";
                        public const string created_at = "created_at";
                        public const string updated_at = "updated_at";
                        public const string status = "status";
                        public const string updated_by = "updated_by";
                    }
                }

                public struct UpdateUserByID_Or_Guid
                {
                    public const string Name = "UpdateUserByID_Or_Guid";

                    public struct Param
                    {
                        public const string ID = "ID";
                        public const string guid = "guid";

                    }
                }

                public struct DeleteUserByID
                {
                    public const string Name = "DeleteUserByID";

                    public struct Param
                    {
                        public const string ID = "ID";

                    }
                }

                public struct DeleteUserByID_Or_Guid
                {
                    public const string Name = "DeleteUserByID_Or_Guid";

                    public struct Param
                    {
                        public const string ID = "ID";
                        public const string guid = "guid";

                    }
                }

            }

            public class Address
            {

                public class GetAddressByID
                {
                    public const string Name = "GetAddressByID";

                    public class Param
                    {
                        public const string ID = "ID";
                    }
                }

                public class GetAddressByIDandGuid
                {
                    public const string Name = "GetAddressByIDandGuid";

                    public class Param
                    {
                        public const string ID = "ID";
                        public const string guid = "guid";

                    }
                }

                public class GetAddressByEmailAndPassword
                {
                    public const string Name = "GetAddressByEmailAndPassword";

                    public class Param
                    {
                        public const string email = "email";
                        public const string password = "password";
                    }
                }

                public class InsertAddress
                {
                    public const string Name = "InsertAddress";

                    public class Param
                    {

                        public const string user_id = "user_id";
                        public const string user_guid = "user_guid";
                        public const string country = "country";
                        public const string city = "city";
                        public const string district = "district";
                        public const string street = "street";
                        public const string building = "building";
                        public const string apartment = "apartment";
                        public const string postal_code = "postal_code";
                        public const string location = "location";
                        public const string created_at = "created_at";
                        public const string updated_at = "updated_at";
                        public const string status = "status";



                    }

                }

                public class UpdateAddressByID
                {
                    public const string Name = "UpdateAddressByID";

                    public class Param
                    {
                        public const string ID = "ID";
                    }
                }

                public class UpdateAddressByIDorGuid
                {
                    public const string Name = "UpdateAddressByIDorGuid";

                    public class Param
                    {
                        public const string ID = "ID";
                        public const string guid = "guid";

                    }
                }


            }

            public class Phones
            {

            }

            public class Emails
            {
                
            }

            public class Passwords
            {

            }


            

            

                








            


        }
        public class Flags
        {
            public class User
            {

                [Flags]
                public enum StatusFlags : ulong
                {
                    None = 0,
                    Active = 1UL << 0,
                    Deleted = 1UL << 1,
                    Primary = 1UL << 2
                }

            }

            public class Address
            {

                [Flags]
                public enum StatusFlags : ulong
                {
                    None = 0,
                    Active = 1UL << 0,
                    Deleted = 1UL << 1,
                    Primary = 1UL << 2
                }

            }

            public class Phone
            {

                [Flags]
                public enum StatusFlags : ulong
                {
                    None = 0,
                    Active = 1UL << 0,
                    Deleted = 1UL << 1,
                    Primary = 1UL << 2
                }

            }

            public class Email
            {

                [Flags]
                public enum StatusFlags : ulong
                {
                    None = 0,
                    Active = 1UL << 0,
                    Deleted = 1UL << 1,
                    Primary = 1UL << 2
                }

            }

            public class Password
            {

                [Flags]
                public enum StatusFlags : ulong
                {
                    None = 0,
                    Active = 1UL << 0,
                    Deleted = 1UL << 1,
                    Primary = 1UL << 2
                }

            }

            public class Media
            {

                [Flags]
                public enum StatusFlags : ulong
                {
                    None = 0,
                    Active = 1UL << 0,
                    Deleted = 1UL << 1,
                    Primary = 1UL << 2
                }

            }

            public class Post
            {

                [Flags]
                public enum StatusFlags : ulong
                {
                    None = 0,
                    Active = 1UL << 0,
                    Deleted = 1UL << 1,
                    Primary = 1UL << 2
                }

            }

            public class Album
            {

                [Flags]
                public enum StatusFlags : ulong
                {
                    None = 0,
                    Active = 1UL << 0,
                    Deleted = 1UL << 1,
                    Primary = 1UL << 2
                }

            }
        }


        public class SQLHelpers
        {

            public static char SqlParameterPrefix(DbConnection? connection)
            {
                if (connection == null)
                    throw new ArgumentNullException("The Connection is NULL!");
                return connection is OracleConnection ? ':' : '@';
            }

            public static DbParameter CreateParam(DbCommand command, string name, object? value, DbType type)
            {
                DbParameter parameter = command.CreateParameter();
                parameter.ParameterName = Helpers.SQLHelpers.SqlParameterPrefix(command.Connection) + name;
                parameter.Value = value ?? DBNull.Value;
                parameter.DbType = type;
                return parameter;
            }

            public static void AddParamIfNotNull(DbCommand command, string name, object? value, DbType type)
            {
                if (value is null)
                    return;

                var parameter = command.CreateParameter();
                parameter.ParameterName = Helpers.SQLHelpers.SqlParameterPrefix(command.Connection) + name;
                parameter.Value = value;
                parameter.DbType = type;
                command.Parameters.Add(parameter);
            }

        }


        //*************************************************************************************//

        public static async Task<byte[]> SafeGetAsync(int ordinal, DbDataReader reader, CancellationToken ct = default!)
        {

            MemoryStream destination = new MemoryStream();
            await using Stream source = reader.GetStream(ordinal); // stream from DB
            var buffer = ArrayPool<byte>.Shared.Rent(81920);
            try
            {
                int read;
                while ((read = await source.ReadAsync(buffer.AsMemory(0, 81920), ct).ConfigureAwait(false)) > 0)
                {
                    await destination.WriteAsync(buffer.AsMemory(0, read), ct).ConfigureAwait(false);
                }
                //await destination.FlushAsync(ct).ConfigureAwait(false);
                Console.WriteLine(destination.Length.ToString());

                return destination.ToArray(); ;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                //return (T)(object)Array.Empty<byte>();
                return destination.ToArray();

            }


        }

        public static T SafeGet<T>(DbDataReader reader, string columnName, T defaultValue = default!)
        {
            // 1️⃣ تحقق من وجود العمود
            int ordinal;

            try
            {
                ordinal = reader.GetOrdinal(columnName);
            }
            catch (IndexOutOfRangeException indexOutOfRangeEx)
            {
                return defaultValue;
            }


            if (reader.IsDBNull(ordinal) && typeof(T) == typeof(byte[]))
                return defaultValue;

            try
            {

                if (typeof(T) == typeof(byte[]) || typeof(T) == typeof(Stream))
                {

                    MemoryStream ms = new();
                    const int chunkSize = 8192;
                    long offset = 0;
                    byte[] chunk = new byte[chunkSize];
                    long bytesRead = 0;
                    while ((bytesRead = reader.GetBytes(ordinal, offset, chunk, 0, chunkSize)) > 0)
                    {

                        ms.Write(chunk, 0, (int)bytesRead);
                        offset += bytesRead;

                    }
                    ms.Position = 0;
                    return (T)(object)ms.ToArray();


                    /*byte[] i = SafeGetAsync(ordinal, reader).GetAwaiter().GetResult();
                    Console.WriteLine($"[SafeGet] Reading bytes: Offset = {i}, The I is = {JsonSerializer.Serialize(i)}");
                    Console.ReadKey();
                    return (T)(object)i.ToArray();*/

                }

                // ✅ محاولة القراءة المباشرة لأي نوع آخر (int, string, bool, double, Guid, DateTime, ...)
                return reader.GetFieldValue<T>(ordinal);

            }
            catch (InvalidCastException)
            {
                // 🔁 محاولة تحويل ذكي عند اختلاف الأنواع (مثل decimal → double)

                //if (typeof(T) == typeof(decimal))
                //    return defaultValue;

                try
                {
                    object? val = reader.GetValue(ordinal);
                    if (val is not null)
                    {
                        return (T)Convert.ChangeType(val, typeof(T));
                    }
                }
                catch
                {
                    // تجاهل التحويلات الفاشلة
                }
                return defaultValue;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SafeGet Warning] Column: {columnName}, Error: {ex.Message}");
                return defaultValue;
            }
        }

        public static Entities.User CreateUserFromReader(DbDataReader reader)
        {
            Entities.User user = new Entities.User();
            return new Entities.User(

                // ---------- User ----------
                id: SafeGet(reader, nameof(user.ID), 0L),
                guid: SafeGet(reader, nameof(user.GUID), Guid.Empty),

                firstName: SafeGet(reader, nameof(user.FirstName), string.Empty),
                lastName: SafeGet(reader, nameof(user.LastName), string.Empty),
                displayName:SafeGet(reader, nameof(user.DisplayName), string.Empty),
                bio: SafeGet(reader, nameof(user.Bio), string.Empty),

                gender: SafeGet(reader, nameof(user.Gender), 0L),
                birthDate: SafeGet(reader, nameof(user.BirthDate), DateTime.MinValue),

                request: SafeGet(reader, nameof(user.Request), 0L),
                entity: SafeGet(reader, nameof(user.Entity), 0L),

                createdAt: SafeGet(reader, nameof(user.CreatedAt), DateTime.MinValue),
                updatedAt: SafeGet(reader, nameof(user.UpdatedAt), DateTime.MinValue),
                status: SafeGet(reader, nameof(user.Status), 0L),
                updatedBy: SafeGet(reader, nameof(user.UpdatedAt), string.Empty)

            );
        }

        public static void PrintUserInfo(Entities.User user)
        {

            Console.WriteLine($@"
===================================================================================
                                  USER {user.ID} DETAILS
===================================================================================

USER_ID                  : {user.ID}
USER_GUID                : {user.GUID}

FIRST_NAME               : {user.FirstName?.ToUpper() ?? ""}
LAST_NAME                : {user.LastName?.ToUpper() ?? ""}
DISPLAY_NAME             : {user.DisplayName?.ToUpper() ?? ""}
BIO                      : {user.Bio?.ToUpper() ?? ""}

GENDER                   : 
BIRTH_DATE               : {user.BirthDate:yyyy-MM-dd}
AGE                      : {user.Age}

REQUEST                  : {user.Request}
ENTITY                   : {user.Entity}

CREATED_AT               : {user.CreatedAt:yyyy-MM-dd HH:mm:ss}
UPDATED_AT               : {user.UpdatedAt:yyyy-MM-dd HH:mm:ss}
USER_STATUS              : 
USER_UPDATED_BY          : {user.UpdatedBy}

============             =                                         ================                   
===================================================================================
");
        }



        internal class PasswordRepository
        {
            protected static class clsHashArgon2
            {

                public const int DefaultSaltSize = 16;
                public const int DefaultHashSize = 64;
                public const int DefaultIterations = 3;
                public const int DefaultMemoryKB = 64 * 1024;
                public const int DefaultParallelism = 3;

                public static byte[] GenerateSalt(int size = DefaultSaltSize)
                {
                    byte[] salt = new byte[size];
                    using (var createRandomSalt = RandomNumberGenerator.Create())
                    {
                        createRandomSalt.GetBytes(salt);
                    }
                    Console.WriteLine(salt);
                    return salt;
                }

                public static byte[] HashPasswordArgon2id(string password, byte[] salt, int iterations = DefaultIterations, int memoryKb = DefaultMemoryKB, int parallelism = DefaultParallelism, int hashLength = DefaultHashSize)
                {

                    if (password is null) throw new ArgumentNullException(nameof(password));
                    if (salt is null) throw new ArgumentNullException(nameof(salt));

                    var passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);
                    var argon = new Argon2id(passwordBytes)
                    {
                        Salt = salt,
                        DegreeOfParallelism = parallelism,
                        Iterations = iterations,
                        MemorySize = memoryKb
                    };

                    return argon.GetBytes(hashLength); // returns hashLength bytes

                }

                public static bool VerifyPasswordArgon2id(string password, byte[] passwordHash, byte[] salt, int iterations, int memoryKb, int parallelism)
                {
                    var computedPasswordHash = HashPasswordArgon2id(password, salt, iterations, memoryKb, parallelism, passwordHash.Length);
                    return CryptographicOperations.FixedTimeEquals(computedPasswordHash, passwordHash);
                }

            }

            public class clsPasswordOperations
            {

                public async Task<(App.ClsApplication?, bool)> VerifyUserDetails_SQLS(string email, string enteredPassword, CancellationToken ct = default)
                {

                    if (string.IsNullOrEmpty(email)) { return (null, false); }

                    string? SelectQuery_findMaxRecordsByEmail = App.SQLQueryList("SelectQuery_findMaxRecordsByEmail");

                    try
                    {

                        await using DbConnection connection = App.CreateConnection();
                        await connection.OpenAsync(ct).ConfigureAwait(false);
                        await using var command = connection.CreateCommand();

                        command.CommandText = SelectQuery_findMaxRecordsByEmail;
                        command.CommandType = CommandType.Text;
                        command.CommandTimeout = App.CRUD_DefaultDbTimeout;

                        char prefix = App.SqlParameterPrefix(connection);

                        var parameter1 = command.CreateParameter();
                        parameter1.ParameterName = prefix + "Email";
                        parameter1.Value = email.Trim();
                        parameter1.DbType = DbType.String;

                        var parameter2 = command.CreateParameter();
                        parameter2.ParameterName = prefix + "EmailPrimaryFlag";
                        parameter2.Value = App.EmailPrimaryFlag;
                        parameter2.DbType = DbType.Int64;

                        var parameter3 = command.CreateParameter();
                        parameter3.ParameterName = prefix + "PasswordPrimaryFlag";
                        parameter3.Value = App.PasswordPrimaryFlag;
                        parameter3.DbType = DbType.Int64;

                        var parameter4 = command.CreateParameter();
                        parameter4.ParameterName = prefix + "PhonePrimaryFlag";
                        parameter4.Value = App.PhonePrimaryFlag;
                        parameter4.DbType = DbType.Int64;

                        command.Parameters.Add(parameter1);
                        command.Parameters.Add(parameter2);
                        command.Parameters.Add(parameter3);
                        command.Parameters.Add(parameter4);

                        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, ct).ConfigureAwait(false);

                        byte[] storedHash;
                        byte[] storedSalt;
                        int iterations;
                        int memoryKb;
                        int parallelism;

                        if (!reader.HasRows) { return (null, false); }
                        App.ClsApplication? obj;
                        if (await reader.ReadAsync(ct).ConfigureAwait(false))
                        {
                            storedHash = await reader.GetFieldValueAsync<byte[]>(reader.GetOrdinal(App.HashedPassword_MapName), ct).ConfigureAwait(false);
                            storedSalt = await reader.GetFieldValueAsync<byte[]>(reader.GetOrdinal(App.PasswordSalt_MapName), ct).ConfigureAwait(false);
                            iterations = reader.GetInt32(reader.GetOrdinal(App.Iterations_MapName));
                            memoryKb = reader.GetInt32(reader.GetOrdinal(App.MemoryKb_MapName));
                            parallelism = reader.GetInt32(reader.GetOrdinal(App.Parallelism_MapName));

                            obj = App.ClsApplication.CreateApplicationFromReader2(reader);
                            if (!clsHashArgon2.VerifyPasswordArgon2id(enteredPassword, storedHash, storedSalt, iterations, memoryKb, parallelism))
                            {
                                return (obj, false);
                            }

                            App.ClsApplication.PrintClassInfo(obj);

                            return (obj, true);
                        }

                        return (null, true);

                    }
                    catch (DbException ex)
                    {
                        Console.WriteLine(ex);
                        Console.WriteLine("Database unreachable. Please try again later.");
                        return (null, false);
                    }


                }

                public async Task<(App.ClsApplication?, bool)> VerifyUserDetails(string email, string enteredPassword, CancellationToken ct = default)
                {

                    if (string.IsNullOrEmpty(email)) { return (null, false); }

                    string SelectQuery_findRecordsByEmail = @$"
SELECT 
u.{App.UserID_DBName} AS {App.UserID_MapName},
u.{App.UserGUID_DBName} AS {App.UserGUID_MapName},
u.{App.FirstName_DBName} AS {App.FirstName_MapName},
u.{App.LastName_DBName} AS {App.LastName_MapName},
u.{App.Gender_DBName} AS {App.Gender_MapName},
u.{App.BirthDate_DBName} AS {App.BirthDate_MapName},
u.{App.Age_DBName} AS {App.Age_MapName},
u.{App.UserCreatedAt_DBName} AS {App.UserCreatedAt_MapName},
u.{App.UserUpdatedAt_DBName} AS {App.UserUpdatedAt_MapName},
u.{App.UserStatus_DBName} AS {App.UserStatus_MapName},
u.{App.UserUpdatedBy_DBName} AS {App.UserUpdatedBy_MapName},

e.{App.EmailID_DBName} AS {App.EmailID_MapName},
e.{App.EmailGUID_DBName} AS {App.EmailGUID_MapName},
e.{App.Email_DBName} AS {App.Email_MapName},
e.{App.EmailUserID_DBName} AS {App.EmailUserID_MapName},
e.{App.EmailCreatedAt_DBName} AS {App.EmailCreatedAt_MapName},
e.{App.EmailUpdatedAt_DBName} AS {App.EmailUpdatedAt_MapName},
e.{App.EmailStatus_DBName} AS {App.EmailStatus_MapName},

ps.{App.PasswordID_DBName} AS {App.PasswordID_MapName},
ps.{App.PasswordGUID_DBName} AS {App.PasswordGUID_MapName},
ps.{App.HashedPassword_DBName} AS {App.HashedPassword_MapName},
ps.{App.PasswordSalt_DBName} AS {App.PasswordSalt_MapName},
ps.{App.Iterations_DBName} AS {App.Iterations_MapName},
ps.{App.MemoryKb_DBName} AS {App.MemoryKb_MapName},
ps.{App.Parallelism_DBName} AS {App.Parallelism_MapName},
ps.{App.PasswordUserID_DBName} AS {App.PasswordUserID_MapName},
ps.{App.PasswordCreatedAt_DBName} AS {App.PasswordCreatedAt_MapName},
ps.{App.PasswordUpdatedAt_DBName} AS {App.PasswordUpdatedAt_MapName},
ps.{App.PasswordStatus_DBName} AS {App.PasswordStatus_MapName},

ph.{App.PhoneID_DBName} AS {App.PhoneID_MapName},
ph.{App.PhoneGUID_DBName} AS {App.PhoneGUID_MapName},
ph.{App.Phone_DBName} AS {App.Phone_MapName},
ph.{App.PhoneUserID_DBName} AS {App.PhoneUserID_MapName},
ph.{App.PhoneCreatedAt_DBName} AS {App.PhoneCreatedAt_MapName},
ph.{App.PhoneUpdatedAt_DBName} AS {App.PhoneUpdatedAt_MapName},
ph.{App.PhoneStatus_DBName} AS {App.PhoneStatus_MapName}

FROM {App.Users_Table_DBName} u
INNER JOIN {App.Emails_Table_DBName} e 
    ON u.{App.UserID_DBName} = e.{App.EmailUserID_DBName}
INNER JOIN {App.Passwords_Table_DBName} ps 
    ON u.{App.UserID_DBName} = ps.{App.PasswordUserID_DBName}
INNER JOIN {App.Phones_Table_DBName} ph 
    ON u.{App.UserID_DBName} = ph.{App.PhoneUserID_DBName}

WHERE e.{App.Email_DBName} = @Email
AND (e.{App.EmailStatus_DBName} & @EmailPrimaryFlag) = @EmailPrimaryFlag
AND (ps.{App.PasswordStatus_DBName} & @PasswordPrimaryFlag) = @PasswordPrimaryFlag
AND (ph.{App.PhoneStatus_DBName} & @PhonePrimaryFlag) = @PhonePrimaryFlag;
";

                    try
                    {

                        using SqlConnection connection1 = App.CreateConnectionSqlServer();

                        await connection1.OpenAsync(ct).ConfigureAwait(false);
                        SqlCommand command1 = new SqlCommand(SelectQuery_findRecordsByEmail, connection1);

                        command1.Parameters.Add("@Email", SqlDbType.NVarChar).Value = email.Trim();
                        command1.Parameters.Add("@EmailPrimaryFlag", SqlDbType.BigInt).Value = App.EmailPrimaryFlag;
                        command1.Parameters.Add("@PasswordPrimaryFlag", SqlDbType.BigInt).Value = App.PasswordPrimaryFlag;
                        command1.Parameters.Add("@PhonePrimaryFlag", SqlDbType.BigInt).Value = App.PhonePrimaryFlag;
                        command1.CommandTimeout = App.CRUD_DefaultDbTimeout;
                        var reader = await command1.ExecuteReaderAsync(ct).ConfigureAwait(false);

                        byte[] storedHash;
                        byte[] storedSalt;
                        int iterations;
                        int memoryKb;
                        int parallelism;

                        if (!reader.HasRows) { return (null, false); }
                        App.ClsApplication? obj;
                        if (await reader.ReadAsync(ct).ConfigureAwait(false))
                        {
                            storedHash = await reader.GetFieldValueAsync<byte[]>(reader.GetOrdinal(App.HashedPassword_MapName), ct);
                            storedSalt = await reader.GetFieldValueAsync<byte[]>(reader.GetOrdinal(App.PasswordSalt_MapName), ct);
                            iterations = reader.GetInt32(reader.GetOrdinal(App.Iterations_MapName));
                            memoryKb = reader.GetInt32(reader.GetOrdinal(App.MemoryKb_MapName));
                            parallelism = reader.GetInt32(reader.GetOrdinal(App.Parallelism_MapName));

                            obj = App.ClsApplication.CreateApplicationFromReader(reader);
                            if (!clsHashArgon2.VerifyPasswordArgon2id(enteredPassword, storedHash, storedSalt, iterations, memoryKb, parallelism))
                            {
                                return (obj, false);
                            }

                            App.ClsApplication.PrintClassInfo(obj);

                            return (obj, true);

                        }

                        return (null, true);

                    }
                    catch (DbException ex)
                    {
                        Console.WriteLine(ex);
                        Console.WriteLine("Database unreachable. Please try again later.");
                        return (null, false);
                    }

                }

                public static bool VerifyPassword(string enteredPassword, App.ClsApplication obj)
                {
                    if (obj == null || obj.HashedPassword == null || obj.PasswordSalt == null || obj.Iterations == null || obj.MemoryKb == null || obj.Parallelism == null)
                    {
                        return false;
                    }

                    byte[] storedHash = (byte[])obj.HashedPassword;
                    byte[] storedSalt = (byte[])obj.PasswordSalt;
                    int iterations = (int)obj.Iterations;
                    int memoryKb = (int)obj.MemoryKb;
                    int parallelism = (int)obj.Parallelism;

                    return clsHashArgon2.VerifyPasswordArgon2id(enteredPassword, storedHash, storedSalt, iterations, memoryKb, parallelism);
                }

                public static async Task<bool> CreateUserPasswordByID(string plainPassword, long? UserId, Guid? UserGuid, CancellationToken ct = default, int size = clsHashArgon2.DefaultSaltSize, int iterations = clsHashArgon2.DefaultIterations, int memoryKb = clsHashArgon2.DefaultMemoryKB, int parallelism = clsHashArgon2.DefaultParallelism, int hashLength = clsHashArgon2.DefaultHashSize)
                {
                    if (plainPassword.Length < 4) { throw new Exception("The password is invalid!"); }
                    if (UserId <= 0 || UserId == null || UserGuid == Guid.Empty || UserGuid == null) { throw new ArgumentNullException("The ID or GUID is invalid!"); }

                    string InsertQuery_InsertNewPassword = @$"
INSERT INTO {App.Passwords_Table_DBName}
(
    {App.PasswordGUID_DBName},
    {App.UserID_DBName},
    {App.UserGUID_DBName},
    {App.HashedPassword_DBName},
    {App.PasswordSalt_DBName},
    {App.Iterations_DBName},
    {App.MemoryKb_DBName},
    {App.Parallelism_DBName},
    {App.PasswordCreatedAt_DBName},
    {App.PasswordUpdatedAt_DBName},
    {App.PasswordStatus_DBName}
)
VALUES
(
    @{App.PasswordGUID_MapName},
    @{App.PasswordUserID_MapName},
    @{App.PasswordUserGUID_MapName},
    @{App.HashedPassword_MapName},
    @{App.PasswordSalt_MapName},
    @{App.Iterations_MapName},
    @{App.MemoryKb_MapName},
    @{App.Parallelism_MapName},
    @{App.PasswordCreatedAt_MapName},
    @{App.PasswordUpdatedAt_MapName},
    @{App.PasswordStatus_MapName}
);";


                    Guid PasswordGuid = Guid.NewGuid();
                    byte[] byteSalt = clsHashArgon2.GenerateSalt(size);
                    byte[] bytePassword = clsHashArgon2.HashPasswordArgon2id(plainPassword, byteSalt, iterations, memoryKb, parallelism, hashLength);
                    try
                    {
                        await using DbConnection connection = App.CreateConnection();
                        await connection.OpenAsync(ct).ConfigureAwait(false);
                        await using var command = connection.CreateCommand();

                        command.CommandText = InsertQuery_InsertNewPassword;
                        command.CommandType = CommandType.Text;
                        command.CommandTimeout = App.CRUD_DefaultDbTimeout;

                        char prefix = App.SqlParameterPrefix(connection);

                        DbParameter AddParam(string name, object? value, DbType type)
                        {
                            var parameter1 = command.CreateParameter();
                            parameter1.ParameterName = prefix + name;
                            parameter1.Value = value ?? DBNull.Value;
                            parameter1.DbType = type;
                            return parameter1;
                        }

                        command.Parameters.Add(AddParam("Guid", PasswordGuid, DbType.Guid));
                        command.Parameters.Add(AddParam("UserId", UserId, DbType.Int64));
                        command.Parameters.Add(AddParam("Guid", UserGuid, DbType.Guid));
                        command.Parameters.Add(AddParam("HashedPassword", bytePassword, DbType.Binary));
                        command.Parameters.Add(AddParam("PasswordSalt", byteSalt, DbType.Binary));
                        command.Parameters.Add(AddParam("Iterations", clsHashArgon2.DefaultIterations, DbType.Int32));
                        command.Parameters.Add(AddParam("Memory_Kb", clsHashArgon2.DefaultMemoryKB, DbType.Int32));
                        command.Parameters.Add(AddParam("Parallelism", clsHashArgon2.DefaultParallelism, DbType.Int32));
                        command.Parameters.Add(AddParam("CreatedAt", DateTimeOffset.UtcNow, DbType.DateTimeOffset));
                        command.Parameters.Add(AddParam("UpdatedAt", DateTimeOffset.UtcNow, DbType.DateTimeOffset));
                        command.Parameters.Add(AddParam("Status", 2, DbType.Int64));

                        int row = await command.ExecuteNonQueryAsync(ct).ConfigureAwait(false);

                        if (row <= 0) { return false; }
                        else { return true; }

                    }
                    catch (DbException ex)
                    {
                        Console.WriteLine(ex);
                        Console.WriteLine("Database unreachable. Please try again later.");
                        return false;
                    }


                }

            }
        
        }



    }
}

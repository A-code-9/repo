using Dapper;
using FileSignatures;
using Konscious.Security.Cryptography;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using Oracle.ManagedDataAccess.Client;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Domain
{

    class App
    {
        

        private static string? SQLServerPassword = Environment.GetEnvironmentVariable("SQLPASSWORD");

        private static readonly string DatabaseName = "ABDELRHMAN_PROJECT";
        private readonly string PrimaryDeleteSiuation = "1";
        private readonly string PrimarySearchSiuation = "1";
        private static readonly string SQLConnectionString = $"Data Source = localhost,2004; Initial Catalog = {DatabaseName}; User ID = SA; Password = {SQLServerPassword}; Encrypt=True;TrustServerCertificate=True;Connection timeout = 15;";

        //public ProgramRepository(IConfiguration configuration)
        //{
        //    SQLServerPassword = configuration.GetConnectionString("SQLPASSWORD");
        //}
        public static SqlConnection CreateConnectionSqlServer(string dbType = "SqlServer")
        {
            return new SqlConnection(SQLConnectionString);
        }

        public static DbConnection CreateConnection(string dbType = "SqlServer")
        {
            switch (dbType)
            {
                case "SqlServer":
                    return new SqlConnection(SQLConnectionString);
                case "Postgres":
                    return new NpgsqlConnection(SQLConnectionString);
                /*case "MySql":
                    return new MySqlConnection(SQLConnectionString);*/
                case "Oracle":
                    return new OracleConnection(SQLConnectionString);
                default:
                    throw new ArgumentException("Unsupported DB type");
            }
        }

        public static char SqlParameterPrefix(DbConnection connection)
        {
            return connection is OracleConnection ? ':' : '@';
        }

        public static string? SQLQueryList(string? queryString)
        {
            switch (queryString)
            {
                case "SelectQuery_findMinRecordsByEmail":
                    return @$"SELECT
-- USERS
    us.{UserID_DBName} AS {UserID_MapName},
    us.{UserGUID_DBName} AS {UserGUID_MapName},
    us.{FirstName_DBName} AS {FirstName_MapName},
    us.{LastName_DBName} AS {LastName_MapName},
    us.{Gender_DBName} AS {Gender_MapName},
    us.{BirthDate_DBName} AS {BirthDate_MapName},
    us.{Age_DBName} AS {Age_MapName},
    us.{UserCreatedAt_DBName} AS {UserCreatedAt_MapName},
    us.{UserUpdatedAt_DBName} AS {UserUpdatedAt_MapName},
    us.{UserStatus_DBName} AS {UserStatus_MapName},
    us.{UserUpdatedBy_DBName} AS {UserUpdatedBy_MapName}
    FROM {Users_Table_DBName} us INNER JOIN {Emails_Table_DBName} em ON us.{UserID_DBName} = em.{EmailUserID_DBName}
    WHERE em.{Email_DBName} = @Email
    AND (us.{UserStatus_DBName} & @UserDeletedFlag) = 0
";


                case "SelectQuery_findMaxRecordsByEmail":
                    return @$"SELECT  
-- USERS
    us.{UserID_DBName} AS {UserID_MapName},
    us.{UserGUID_DBName} AS {UserGUID_MapName},
    us.{FirstName_DBName} AS {FirstName_MapName},
    us.{LastName_DBName} AS {LastName_MapName},
    us.{Gender_DBName} AS {Gender_MapName},
    us.{BirthDate_DBName} AS {BirthDate_MapName},
    us.{Age_DBName} AS {Age_MapName},
    us.{UserCreatedAt_DBName} AS {UserCreatedAt_MapName},
    us.{UserUpdatedAt_DBName} AS {UserUpdatedAt_MapName},
    us.{UserStatus_DBName} AS {UserStatus_MapName},
    us.{UserUpdatedBy_DBName} AS {UserUpdatedBy_MapName},

    -- ADDRESSES
    ad.{AddressID_DBName} AS {AddressID_MapName},
    ad.{AddressGUID_DBName} AS {AddressGUID_MapName},
    ad.{AddressUserID_DBName} AS {AddressUserID_MapName},
    ad.{AddressUserGUID_DBName} AS {AddressUserGUID_MapName},
    ad.{AddressCountry_DBName} AS {AddressCountry_MapName},
    ad.{AddressCity_DBName} AS {AddressCity_MapName},
    ad.{AddressDistrict_DBName} AS {AddressDistrict_MapName},
    ad.{AddressStreet_DBName} AS {AddressStreet_MapName},
    ad.{AddressBuilding_DBName} AS {AddressBuilding_MapName},
    ad.{AddressApartment_DBName} AS {AddressApartment_MapName},
    ad.{AddressPostalCode_DBName} AS {AddressPostalCode_MapName},
    ad.{AddressLatitude_DBName} AS {AddressLatitude_MapName},
    ad.{AddressLongitude_DBName} AS {AddressLongitude_MapName},
    ad.{AddressCreatedAt_DBName} AS {AddressCreatedAt_MapName},
    ad.{AddressUpdatedAt_DBName} AS {AddressUpdatedAt_MapName},
    ad.{AddressStatus_DBName} AS {AddressStatus_MapName},

    -- PHONES
    ph.{PhoneID_DBName} AS {PhoneID_MapName},
    ph.{PhoneGUID_DBName} AS {PhoneGUID_MapName},
    ph.{PhoneUserID_DBName} AS {PhoneUserID_MapName},
    ph.{PhoneUserGUID_DBName} AS {PhoneUserGUID_MapName},
    ph.{Phone_DBName} AS {Phone_MapName},
    ph.{PhoneCreatedAt_DBName} AS {PhoneCreatedAt_MapName},
    ph.{PhoneUpdatedAt_DBName} AS {PhoneUpdatedAt_MapName},
    ph.{PhoneStatus_DBName} AS {PhoneStatus_MapName},

    -- EMAILS
    em.{EmailID_DBName} AS {EmailID_MapName},
    em.{EmailGUID_DBName} AS {EmailGUID_MapName},
    em.{EmailUserID_DBName} AS {EmailUserID_MapName},
    em.{EmailUserGUID_DBName} AS {EmailUserGUID_MapName},
    em.{Email_DBName} AS {Email_MapName},
    em.{EmailCreatedAt_DBName} AS {EmailCreatedAt_MapName},
    em.{EmailUpdatedAt_DBName} AS {EmailUpdatedAt_MapName},
    em.{EmailStatus_DBName} AS {EmailStatus_MapName},

    -- PASSWORDS
    ps.{PasswordID_DBName} AS {PasswordID_MapName},
    ps.{PasswordGUID_DBName} AS {PasswordGUID_MapName},
    ps.{PasswordUserID_DBName} AS {PasswordUserID_MapName},
    ps.{PasswordUserGUID_DBName} AS {PasswordUserGUID_MapName},
    ps.{HashedPassword_DBName} AS {HashedPassword_MapName},
    ps.{PasswordSalt_DBName} AS {PasswordSalt_MapName},
    ps.{Iterations_DBName} AS {Iterations_MapName},
    ps.{MemoryKb_DBName} AS {MemoryKb_MapName},
    ps.{Parallelism_DBName} AS {Parallelism_MapName},
    ps.{PasswordCreatedAt_DBName} AS {PasswordCreatedAt_MapName},
    ps.{PasswordUpdatedAt_DBName} AS {PasswordUpdatedAt_MapName},
    ps.{PasswordStatus_DBName} AS {PasswordStatus_MapName}

    FROM {Users_Table_DBName} us INNER JOIN {Addresses_Table_DBName} ad ON us.{UserID_DBName} = ad.{AddressUserID_DBName}
    INNER JOIN {Phones_Table_DBName} ph ON us.{UserID_DBName} = ph.{PhoneUserID_DBName} 
    INNER JOIN {Emails_Table_DBName} em ON us.{UserID_DBName} = em.{EmailUserID_DBName} 
    INNER JOIN {Passwords_Table_DBName} ps ON us.{UserID_DBName} = ps.{PasswordUserID_DBName} 
WHERE
em.{Email_DBName} = @Email
    AND (us.{UserStatus_DBName} & @UserDeletedFlag) = 0
    AND (ad.{AddressStatus_DBName} & @AddressPrimaryFlag) = @AddressPrimaryFlag
    AND (ph.{PhoneStatus_DBName} & @PhonePrimaryFlag) = @PhonePrimaryFlag
    AND (em.{EmailStatus_DBName} & @EmailPrimaryFlag) = @EmailPrimaryFlag
    AND (ps.{PasswordStatus_DBName} & @PasswordPrimaryFlag) = @PasswordPrimaryFlag;";


                case "SelectQuery_findMaxRecordsByID":
                    return @$"SELECT  
-- USERS
    us.{UserID_DBName} AS {UserID_MapName},
    us.{UserGUID_DBName} AS {UserGUID_MapName},
    us.{FirstName_DBName} AS {FirstName_MapName},
    us.{LastName_DBName} AS {LastName_MapName},
    us.{Gender_DBName} AS {Gender_MapName},
    us.{BirthDate_DBName} AS {BirthDate_MapName},
    us.{Age_DBName} AS {Age_MapName},
    us.{UserCreatedAt_DBName} AS {UserCreatedAt_MapName},
    us.{UserUpdatedAt_DBName} AS {UserUpdatedAt_MapName},
    us.{UserStatus_DBName} AS {UserStatus_MapName},
    us.{UserUpdatedBy_DBName} AS {UserUpdatedBy_MapName},

    -- ADDRESSES
    ad.{AddressID_DBName} AS {AddressID_MapName},
    ad.{AddressGUID_DBName} AS {AddressGUID_MapName},
    ad.{AddressUserID_DBName} AS {AddressUserID_MapName},
    ad.{AddressUserGUID_DBName} AS {AddressUserGUID_MapName},
    ad.{AddressCountry_DBName} AS {AddressCountry_MapName},
    ad.{AddressCity_DBName} AS {AddressCity_MapName},
    ad.{AddressDistrict_DBName} AS {AddressDistrict_MapName},
    ad.{AddressStreet_DBName} AS {AddressStreet_MapName},
    ad.{AddressBuilding_DBName} AS {AddressBuilding_MapName},
    ad.{AddressApartment_DBName} AS {AddressApartment_MapName},
    ad.{AddressPostalCode_DBName} AS {AddressPostalCode_MapName},
    ad.{AddressLatitude_DBName} AS {AddressLatitude_MapName},
    ad.{AddressLongitude_DBName} AS {AddressLongitude_MapName},
    ad.{AddressCreatedAt_DBName} AS {AddressCreatedAt_MapName},
    ad.{AddressUpdatedAt_DBName} AS {AddressUpdatedAt_MapName},
    ad.{AddressStatus_DBName} AS {AddressStatus_MapName},

    -- PHONES
    ph.{PhoneID_DBName} AS {PhoneID_MapName},
    ph.{PhoneGUID_DBName} AS {PhoneGUID_MapName},
    ph.{PhoneUserID_DBName} AS {PhoneUserID_MapName},
    ph.{PhoneUserGUID_DBName} AS {PhoneUserGUID_MapName},
    ph.{Phone_DBName} AS {Phone_MapName},
    ph.{PhoneCreatedAt_DBName} AS {PhoneCreatedAt_MapName},
    ph.{PhoneUpdatedAt_DBName} AS {PhoneUpdatedAt_MapName},
    ph.{PhoneStatus_DBName} AS {PhoneStatus_MapName},

    -- EMAILS
    em.{EmailID_DBName} AS {EmailID_MapName},
    em.{EmailGUID_DBName} AS {EmailGUID_MapName},
    em.{EmailUserID_DBName} AS {EmailUserID_MapName},
    em.{EmailUserGUID_DBName} AS {EmailUserGUID_MapName},
    em.{Email_DBName} AS {Email_MapName},
    em.{EmailCreatedAt_DBName} AS {EmailCreatedAt_MapName},
    em.{EmailUpdatedAt_DBName} AS {EmailUpdatedAt_MapName},
    em.{EmailStatus_DBName} AS {EmailStatus_MapName},

    -- PASSWORDS
    ps.{PasswordID_DBName} AS {PasswordID_MapName},
    ps.{PasswordGUID_DBName} AS {PasswordGUID_MapName},
    ps.{PasswordUserID_DBName} AS {PasswordUserID_MapName},
    ps.{PasswordUserGUID_DBName} AS {PasswordUserGUID_MapName},
    ps.{HashedPassword_DBName} AS {HashedPassword_MapName},
    ps.{PasswordSalt_DBName} AS {PasswordSalt_MapName},
    ps.{Iterations_DBName} AS {Iterations_MapName},
    ps.{MemoryKb_DBName} AS {MemoryKb_MapName},
    ps.{Parallelism_DBName} AS {Parallelism_MapName},
    ps.{PasswordCreatedAt_DBName} AS {PasswordCreatedAt_MapName},
    ps.{PasswordUpdatedAt_DBName} AS {PasswordUpdatedAt_MapName},
    ps.{PasswordStatus_DBName} AS {PasswordStatus_MapName}

    FROM {Users_Table_DBName} us INNER JOIN {Addresses_Table_DBName} ad ON us.{UserID_DBName} = ad.{AddressUserID_DBName}
    INNER JOIN {Phones_Table_DBName} ph ON us.{UserID_DBName} = ph.{PhoneUserID_DBName} 
    INNER JOIN {Emails_Table_DBName} em ON us.{UserID_DBName} = em.{EmailUserID_DBName} 
    INNER JOIN {Passwords_Table_DBName} ps ON us.{UserID_DBName} = ps.{PasswordUserID_DBName} 
WHERE
em.{UserID_DBName} = @UserID
    AND (us.{UserStatus_DBName} & @UserDeletedFlag) = 0
    AND (ad.{AddressStatus_DBName} & @AddressPrimaryFlag) = @AddressPrimaryFlag
    AND (ph.{PhoneStatus_DBName} & @PhonePrimaryFlag) = @PhonePrimaryFlag
    AND (em.{EmailStatus_DBName} & @EmailPrimaryFlag) = @EmailPrimaryFlag
    AND (ps.{PasswordStatus_DBName} & @PasswordPrimaryFlag) = @PasswordPrimaryFlag;";


                case "SelectQuery_findBigMaxRecordsByID":
                    return @$"
SELECT 
    -- USERS
    us.{UserID_DBName} AS {UserID_MapName},
    us.{UserGUID_DBName} AS {UserGUID_MapName},
    us.{FirstName_DBName} AS {FirstName_MapName},
    us.{LastName_DBName} AS {LastName_MapName},
    us.{Gender_DBName} AS {Gender_MapName},
    us.{BirthDate_DBName} AS {BirthDate_MapName},
    us.{Age_DBName} AS {Age_MapName},
    us.{UserCreatedAt_DBName} AS {UserCreatedAt_MapName},
    us.{UserUpdatedAt_DBName} AS {UserUpdatedAt_MapName},
    us.{UserStatus_DBName} AS {UserStatus_MapName},
    us.{UserUpdatedBy_DBName} AS {UserUpdatedBy_MapName},

    -- ADDRESSES
    ad.{AddressID_DBName} AS {AddressID_MapName},
    ad.{AddressGUID_DBName} AS {AddressGUID_MapName},
    ad.{AddressUserID_DBName} AS {AddressUserID_MapName},
    ad.{AddressUserGUID_DBName} AS {AddressUserGUID_MapName},
    ad.{AddressCountry_DBName} AS {AddressCountry_MapName},
    ad.{AddressCity_DBName} AS {AddressCity_MapName},
    ad.{AddressDistrict_DBName} AS {AddressDistrict_MapName},
    ad.{AddressStreet_DBName} AS {AddressStreet_MapName},
    ad.{AddressBuilding_DBName} AS {AddressBuilding_MapName},
    ad.{AddressApartment_DBName} AS {AddressApartment_MapName},
    ad.{AddressPostalCode_DBName} AS {AddressPostalCode_MapName},
    ad.{AddressLatitude_DBName} AS {AddressLatitude_MapName},
    ad.{AddressLongitude_DBName} AS {AddressLongitude_MapName},
    ad.{AddressCreatedAt_DBName} AS {AddressCreatedAt_MapName},
    ad.{AddressUpdatedAt_DBName} AS {AddressUpdatedAt_MapName},
    ad.{AddressStatus_DBName} AS {AddressStatus_MapName},

    -- PHONES
    ph.{PhoneID_DBName} AS {PhoneID_MapName},
    ph.{PhoneGUID_DBName} AS {PhoneGUID_MapName},
    ph.{PhoneUserID_DBName} AS {PhoneUserID_MapName},
    ph.{PhoneUserGUID_DBName} AS {PhoneUserGUID_MapName},
    ph.{Phone_DBName} AS {Phone_MapName},
    ph.{PhoneCreatedAt_DBName} AS {PhoneCreatedAt_MapName},
    ph.{PhoneUpdatedAt_DBName} AS {PhoneUpdatedAt_MapName},
    ph.{PhoneStatus_DBName} AS {PhoneStatus_MapName},

    -- EMAILS
    em.{EmailID_DBName} AS {EmailID_MapName},
    em.{EmailGUID_DBName} AS {EmailGUID_MapName},
    em.{EmailUserID_DBName} AS {EmailUserID_MapName},
    em.{EmailUserGUID_DBName} AS {EmailUserGUID_MapName},
    em.{Email_DBName} AS {Email_MapName},
    em.{EmailCreatedAt_DBName} AS {EmailCreatedAt_MapName},
    em.{EmailUpdatedAt_DBName} AS {EmailUpdatedAt_MapName},
    em.{EmailStatus_DBName} AS {EmailStatus_MapName},

    -- PASSWORDS
    ps.{PasswordID_DBName} AS {PasswordID_MapName},
    ps.{PasswordGUID_DBName} AS {PasswordGUID_MapName},
    ps.{PasswordUserID_DBName} AS {PasswordUserID_MapName},
    ps.{PasswordUserGUID_DBName} AS {PasswordUserGUID_MapName},
    ps.{HashedPassword_DBName} AS {HashedPassword_MapName},
    ps.{PasswordSalt_DBName} AS {PasswordSalt_MapName},
    ps.{Iterations_DBName} AS {Iterations_MapName},
    ps.{MemoryKb_DBName} AS {MemoryKb_MapName},
    ps.{Parallelism_DBName} AS {Parallelism_MapName},
    ps.{PasswordCreatedAt_DBName} AS {PasswordCreatedAt_MapName},
    ps.{PasswordUpdatedAt_DBName} AS {PasswordUpdatedAt_MapName},
    ps.{PasswordStatus_DBName} AS {PasswordStatus_MapName},

    -- BYTE FILES
    bf.{ByteFiles_ID_DBName} AS {ByteFiles_ID_MapName},
    bf.{ByteFiles_GUID_DBName} AS {ByteFiles_GUID_MapName},
    bf.{ByteFiles_OwnerID_DBName} AS {ByteFiles_OwnerID_MapName},
    bf.{ByteFiles_OwnerGUID_DBName} AS {ByteFiles_OwnerGUID_MapName},
    bf.{ByteFiles_AlbumID_DBName} AS {ByteFiles_AlbumID_MapName},
    bf.{ByteFiles_AlbumGUID_DBName} AS {ByteFiles_AlbumGUID_MapName},
    bf.{ByteFiles_PostID_DBName} AS {ByteFiles_PostID_MapName},
    bf.{ByteFiles_PostGUID_DBName} AS {ByteFiles_PostGUID_MapName},
    bf.{ByteFiles_FileName_DBName} AS {ByteFiles_FileName_MapName},
    bf.{ByteFiles_FileData_DBName} AS {ByteFiles_FileData_MapName},
    bf.{ByteFiles_Thumbnail_DBName} AS {ByteFiles_Thumbnail_MapName},
    bf.{ByteFiles_Width_DBName} AS {ByteFiles_Width_MapName},
    bf.{ByteFiles_Height_DBName} AS {ByteFiles_Height_MapName},
    bf.{ByteFiles_SizeByte_DBName} AS {ByteFiles_SizeByte_MapName},
    bf.{ByteFiles_MimeType_DBName} AS {ByteFiles_MimeType_MapName},
    bf.{ByteFiles_Path_DBName} AS {ByteFiles_Path_MapName},
    bf.{ByteFiles_Privacy_DBName} AS {ByteFiles_Privacy_MapName},
    bf.{ByteFiles_StorageBucket_DBName} AS {ByteFiles_StorageBucket_MapName},
    bf.{ByteFiles_StorageKey_DBName} AS {ByteFiles_StorageKey_MapName},
    bf.{ByteFiles_CheckHash_DBName} AS {ByteFiles_CheckHash_MapName},
    bf.{ByteFiles_PerceptHash_DBName} AS {ByteFiles_PerceptHash_MapName},
    bf.{ByteFiles_ExifDetails_DBName} AS {ByteFiles_ExifDetails_MapName},
    bf.{ByteFiles_Derivatives_DBName} AS {ByteFiles_Derivatives_MapName},
    bf.{ByteFiles_EmbeddingRef_DBName} AS {ByteFiles_EmbeddingRef_MapName},
    bf.{ByteFiles_AccessCount_DBName} AS {ByteFiles_AccessCount_MapName},
    bf.{ByteFiles_LastServed_DBName} AS {ByteFiles_LastServed_MapName},
    bf.{ByteFiles_CreatedAt_DBName} AS {ByteFiles_CreatedAt_MapName},
    bf.{ByteFiles_UpdatedAt_DBName} AS {ByteFiles_UpdatedAt_MapName},
    bf.{ByteFiles_Status_DBName} AS {ByteFiles_Status_MapName}

FROM {Users_Table_DBName} us
    LEFT JOIN {Addresses_Table_DBName} ad ON us.{UserID_DBName} = ad.{AddressUserID_DBName}
    LEFT JOIN {Phones_Table_DBName} ph ON us.{UserID_DBName} = ph.{PhoneUserID_DBName}
    LEFT JOIN {Emails_Table_DBName} em  ON us.{UserID_DBName} = em.{EmailUserID_DBName}
    LEFT JOIN {Passwords_Table_DBName} ps ON us.{UserID_DBName} = ps.{PasswordUserID_DBName}
    LEFT JOIN {ByteFiles_Table_DBName} bf ON us.{UserID_DBName} = bf.{ByteFiles_OwnerID_DBName}

WHERE 
    us.{UserID_DBName} = @UserID
    AND (us.{UserStatus_DBName} & @UserDeletedFlag) = 0
    AND (ad.{AddressStatus_DBName} & @AddressPrimaryFlag) = @AddressPrimaryFlag
    AND (ph.{PhoneStatus_DBName} & @PhonePrimaryFlag) = @PhonePrimaryFlag
    AND (em.{EmailStatus_DBName} & @EmailPrimaryFlag) = @EmailPrimaryFlag
    AND (ps.{PasswordStatus_DBName} & @PasswordPrimaryFlag) = @PasswordPrimaryFlag

;";

            }
            return null;
        }

        public const int CRUD_DefaultDbTimeout = 30;
        public const int ADMIN_DefaultDbTimeout = 60;
        public const int HEAVY_DefaultDbTimeout = 100;
        public const int MOST_DefaultDbTimeout = 300;

        public const string UserID_MapName = nameof(ClsApplication.UserID);
        public const string UserGUID_MapName = nameof(ClsApplication.UserGUID);
        public const string FirstName_MapName = nameof(ClsApplication.FirstName);
        public const string LastName_MapName = nameof(ClsApplication.LastName);
        public const string Gender_MapName = nameof(ClsApplication.Gender);
        public const string BirthDate_MapName = nameof(ClsApplication.BirthDate);
        public const string Age_MapName = nameof(ClsApplication.Age);
        public const string UserCreatedAt_MapName = nameof(ClsApplication.UserCreatedAt);
        public const string UserUpdatedAt_MapName = nameof(ClsApplication.UserUpdatedAt);
        public const string UserStatus_MapName = nameof(ClsApplication.UserStatus);
        public const string UserUpdatedBy_MapName = nameof(ClsApplication.UserUpdatedBy);

        // ADDRESSES

        public const string AddressID_MapName = nameof(ClsApplication.AddressID);
        public const string AddressGUID_MapName = nameof(ClsApplication.AddressGUID);
        public const string AddressUserID_MapName = nameof(ClsApplication.AddressUserID);
        public const string AddressUserGUID_MapName = nameof(ClsApplication.AddressUserGUID);
        public const string AddressCountry_MapName = nameof(ClsApplication.AddressCountry);
        public const string AddressCity_MapName = nameof(ClsApplication.AddressCity);
        public const string AddressDistrict_MapName = nameof(ClsApplication.AddressDistrict);
        public const string AddressStreet_MapName = nameof(ClsApplication.AddressStreet);
        public const string AddressBuilding_MapName = nameof(ClsApplication.AddressBuilding);
        public const string AddressApartment_MapName = nameof(ClsApplication.AddressApartment);
        public const string AddressPostalCode_MapName = nameof(ClsApplication.AddressPostalCode);
        public const string AddressLatitude_MapName = nameof(ClsApplication.AddressLatitude);
        public const string AddressLongitude_MapName = nameof(ClsApplication.AddressLongitude);
        public const string AddressCreatedAt_MapName = nameof(ClsApplication.AddressCreatedAt);
        public const string AddressUpdatedAt_MapName = nameof(ClsApplication.AddressUpdatedAt);
        public const string AddressStatus_MapName = nameof(ClsApplication.AddressStatus);

        //PHONES

        public const string PhoneID_MapName = nameof(ClsApplication.PhoneID);
        public const string PhoneGUID_MapName = nameof(ClsApplication.PhoneGUID);
        public const string PhoneUserID_MapName = nameof(ClsApplication.PhoneUserID);
        public const string PhoneUserGUID_MapName = nameof(ClsApplication.PhoneUserGUID);
        public const string Phone_MapName = nameof(ClsApplication.Phone);
        public const string PhoneCreatedAt_MapName = nameof(ClsApplication.PhoneCreatedAt);
        public const string PhoneUpdatedAt_MapName = nameof(ClsApplication.PhoneUpdatedAt);
        public const string PhoneStatus_MapName = nameof(ClsApplication.PhoneStatus);


        //EMAILS

        public const string EmailID_MapName = nameof(ClsApplication.EmailID);
        public const string EmailGUID_MapName = nameof(ClsApplication.EmailGUID);
        public const string EmailUserID_MapName = nameof(ClsApplication.EmailUserID);
        public const string EmailUserGUID_MapName = nameof(ClsApplication.EmailUserGUID);
        public const string Email_MapName = nameof(ClsApplication.Email);
        public const string EmailCreatedAt_MapName = nameof(ClsApplication.EmailCreatedAt);
        public const string EmailUpdatedAt_MapName = nameof(ClsApplication.EmailUpdatedAt);
        public const string EmailStatus_MapName = nameof(ClsApplication.EmailStatus);


        //PASSWORD

        public const string PasswordID_MapName = nameof(ClsApplication.PasswordID);
        public const string PasswordGUID_MapName = nameof(ClsApplication.PasswordGUID);
        public const string PasswordUserID_MapName = nameof(ClsApplication.PasswordUserID);
        public const string PasswordUserGUID_MapName = nameof(ClsApplication.PasswordUserGUID);
        public const string HashedPassword_MapName = nameof(ClsApplication.HashedPassword);
        public const string PasswordSalt_MapName = nameof(ClsApplication.PasswordSalt);
        public const string Iterations_MapName = nameof(ClsApplication.Iterations);
        public const string MemoryKb_MapName = nameof(ClsApplication.MemoryKb);
        public const string Parallelism_MapName = nameof(ClsApplication.Parallelism);
        public const string PasswordCreatedAt_MapName = nameof(ClsApplication.PasswordCreatedAt);
        public const string PasswordUpdatedAt_MapName = nameof(ClsApplication.PasswordUpdatedAt);
        public const string PasswordStatus_MapName = nameof(ClsApplication.PasswordStatus);

        //BYTES FILES

        public const string ByteFiles_ID_MapName = nameof(ClsApplication.ByteFileID);
        public const string ByteFiles_GUID_MapName = nameof(ClsApplication.ByteFileGUID);
        public const string ByteFiles_OwnerID_MapName = nameof(ClsApplication.ByteFileOwnerID);
        public const string ByteFiles_OwnerGUID_MapName = nameof(ClsApplication.ByteFileOwnerGUID);
        public const string ByteFiles_AlbumID_MapName = nameof(ClsApplication.ByteFileAlbumID);
        public const string ByteFiles_AlbumGUID_MapName = nameof(ClsApplication.ByteFileAlbumGUID);
        public const string ByteFiles_PostID_MapName = nameof(ClsApplication.ByteFilePostID);
        public const string ByteFiles_PostGUID_MapName = nameof(ClsApplication.ByteFilePostGUID);
        public const string ByteFiles_FileName_MapName = nameof(ClsApplication.ByteFileFileName);
        public const string ByteFiles_FileData_MapName = nameof(ClsApplication.ByteFileFileData);
        public const string ByteFiles_Thumbnail_MapName = nameof(ClsApplication.ByteFileThumbnail);
        public const string ByteFiles_Width_MapName = nameof(ClsApplication.ByteFileWidth);
        public const string ByteFiles_Height_MapName = nameof(ClsApplication.ByteFileHeight);
        public const string ByteFiles_SizeByte_MapName = nameof(ClsApplication.ByteFileSizeByte);
        public const string ByteFiles_MimeType_MapName = nameof(ClsApplication.ByteFileMimeType);
        public const string ByteFiles_Path_MapName = nameof(ClsApplication.ByteFilePath);
        public const string ByteFiles_Privacy_MapName = nameof(ClsApplication.ByteFilePrivacy);
        public const string ByteFiles_StorageBucket_MapName = nameof(ClsApplication.ByteFileStorageBucket);
        public const string ByteFiles_StorageKey_MapName = nameof(ClsApplication.ByteFileStorageKey);
        public const string ByteFiles_CheckHash_MapName = nameof(ClsApplication.ByteFileCheckHash);
        public const string ByteFiles_PerceptHash_MapName = nameof(ClsApplication.ByteFilePerceptHash);
        public const string ByteFiles_ExifDetails_MapName = nameof(ClsApplication.ByteFileExifDetails);
        public const string ByteFiles_Derivatives_MapName = nameof(ClsApplication.ByteFileDerivatives);
        public const string ByteFiles_EmbeddingRef_MapName = nameof(ClsApplication.ByteFileEmbeddingRef);
        public const string ByteFiles_AccessCount_MapName = nameof(ClsApplication.ByteFileAccessCount);
        public const string ByteFiles_LastServed_MapName = nameof(ClsApplication.ByteFileLastServed);
        public const string ByteFiles_CreatedAt_MapName = nameof(ClsApplication.ByteFileCreatedAt);
        public const string ByteFiles_UpdatedAt_MapName = nameof(ClsApplication.ByteFileUpdatedAt);
        public const string ByteFiles_Status_MapName = nameof(ClsApplication.ByteFileStatus);


        ///////////////////////////////////////////////////////////////


        //USERS

        public const string Users_Table_DBName = "Users";

        public const string UserID_DBName = "ID";
        public const string UserGUID_DBName = "guid";
        public const string FirstName_DBName = "first_name";
        public const string LastName_DBName = "last_name";
        public const string Gender_DBName = "gender";
        public const string BirthDate_DBName = "birth_date";
        public const string Age_DBName = "age";
        public const string UserCreatedAt_DBName = "created_at";
        public const string UserUpdatedAt_DBName = "updated_at";
        public const string UserStatus_DBName = "status";
        public const string UserUpdatedBy_DBName = "updated_by";

        // ADDRESSES - DATABASE NAMES

        public const string Addresses_Table_DBName = "Addresses";

        public const string AddressID_DBName = "ID";
        public const string AddressGUID_DBName = "guid";
        public const string AddressUserID_DBName = "user_id";
        public const string AddressUserGUID_DBName = "user_guid";
        public const string AddressCountry_DBName = "country";
        public const string AddressCity_DBName = "city";
        public const string AddressDistrict_DBName = "district";
        public const string AddressStreet_DBName = "street";
        public const string AddressBuilding_DBName = "building";
        public const string AddressApartment_DBName = "apartment";
        public const string AddressPostalCode_DBName = "postal_code";
        public const string AddressLatitude_DBName = "latitude";
        public const string AddressLongitude_DBName = "longitude";
        public const string AddressCreatedAt_DBName = "created_at";
        public const string AddressUpdatedAt_DBName = "updated_at";
        public const string AddressStatus_DBName = "status";


        //PHONES

        public const string Phones_Table_DBName = "Phones";

        public const string PhoneID_DBName = "ID";
        public const string PhoneGUID_DBName = "guid";
        public const string PhoneUserID_DBName = "user_id";
        public const string PhoneUserGUID_DBName = "user_guid";
        public const string Phone_DBName = "phone_number";
        public const string PhoneCreatedAt_DBName = "created_at";
        public const string PhoneUpdatedAt_DBName = "updated_at";
        public const string PhoneStatus_DBName = "status";

        //EMAILS

        public const string Emails_Table_DBName = "Emails";

        public const string EmailID_DBName = "ID";
        public const string EmailGUID_DBName = "guid";
        public const string EmailUserID_DBName = "user_id";
        public const string EmailUserGUID_DBName = "user_guid";
        public const string Email_DBName = "email";
        public const string EmailCreatedAt_DBName = "created_at";
        public const string EmailUpdatedAt_DBName = "updated_at";
        public const string EmailStatus_DBName = "status";


        //PASSWORDS

        public const string Passwords_Table_DBName = "Passwords";

        public const string PasswordID_DBName = "ID";
        public const string PasswordGUID_DBName = "guid";
        public const string PasswordUserID_DBName = "user_id";
        public const string PasswordUserGUID_DBName = "user_guid";
        public const string HashedPassword_DBName = "hashed_password";
        public const string PasswordSalt_DBName = "password_salt";
        public const string Iterations_DBName = "iterations";
        public const string MemoryKb_DBName = "memory_kb";
        public const string Parallelism_DBName = "parallelism";
        public const string PasswordCreatedAt_DBName = "created_at";
        public const string PasswordUpdatedAt_DBName = "updated_at";
        public const string PasswordStatus_DBName = "status";

        //BYTE FILES

        public const string ByteFiles_Table_DBName = "ByteFiles";

        public const string ByteFiles_ID_DBName = "ID";
        public const string ByteFiles_GUID_DBName = "guid";
        public const string ByteFiles_OwnerID_DBName = "owner_id";
        public const string ByteFiles_OwnerGUID_DBName = "owner_guid";
        public const string ByteFiles_AlbumID_DBName = "album_id";
        public const string ByteFiles_AlbumGUID_DBName = "album_guid";
        public const string ByteFiles_PostID_DBName = "post_id";
        public const string ByteFiles_PostGUID_DBName = "post_guid";
        public const string ByteFiles_FileName_DBName = "file_name";
        public const string ByteFiles_FileData_DBName = "file_data";
        public const string ByteFiles_Thumbnail_DBName = "thumbnail";
        public const string ByteFiles_Width_DBName = "width";
        public const string ByteFiles_Height_DBName = "height";
        public const string ByteFiles_SizeByte_DBName = "size_byte";
        public const string ByteFiles_MimeType_DBName = "mime_type";
        public const string ByteFiles_Path_DBName = "path";
        public const string ByteFiles_Privacy_DBName = "privacy";
        public const string ByteFiles_StorageBucket_DBName = "storage_bucket";
        public const string ByteFiles_StorageKey_DBName = "storage_key";
        public const string ByteFiles_CheckHash_DBName = "check_hash";
        public const string ByteFiles_PerceptHash_DBName = "percept_hash";
        public const string ByteFiles_ExifDetails_DBName = "exif_details";
        public const string ByteFiles_Derivatives_DBName = "derivatives";
        public const string ByteFiles_EmbeddingRef_DBName = "embedding_ref";
        public const string ByteFiles_AccessCount_DBName = "access_count";
        public const string ByteFiles_LastServed_DBName = "last_served";
        public const string ByteFiles_CreatedAt_DBName = "created_at";
        public const string ByteFiles_UpdatedAt_DBName = "updated_at";
        public const string ByteFiles_Status_DBName = "status";


        //FLAGS/PONS/PRIVACY/CHECKS
        //IsDELET
        public const long UserDeletedFlag = 1;
        public const long AddressDeletedFlag = 1;
        public const long PhoneDeletedFlag = 1;
        public const long EmailDeletedFlag = 1;
        public const long PasswordDeletedFlag = 1;
        public const long ByteFileDeletedFlag = 1;

        //IsPRIMA
        public const long UserPrimaryFlag = 2;
        public const long AddressPrimaryFlag = 2;
        public const long PhonePrimaryFlag = 2;
        public const long EmailPrimaryFlag = 2;
        public const long PasswordPrimaryFlag = 2;
        public const long ByteFilePrimaryFlag = 2;

        ServiceCollection builder = new ServiceCollection();

        internal class ClsApplication
        {

            //USERS

            private long _user_ID;
            private Guid _user_GUID;
            private string _first_name;
            private string _last_name;
            private string _gender;
            private DateTime _birth_date;
            private int _age;
            private DateTimeOffset _user_created_at;
            private DateTimeOffset _user_updated_at;
            private long _user_status;
            private string _updated_by;

            // ========== ADDRESSES ==========
            private long _address_ID;
            private Guid _address_GUID;
            private long _address_user_id;
            private Guid _address_user_guid;
            private string _address_country;
            private string _address_city;
            private string _address_district;
            private string _address_street;
            private string _address_building;
            private string _address_apartment;
            private string _address_postal_code;
            private decimal _address_latitude;
            private decimal _address_longitude;
            private DateTimeOffset _address_created_at;
            private DateTimeOffset _address_updated_at;
            private long _address_status;

            private long _phone_ID;
            private Guid _phone_GUID;
            private long _phone_user_id;
            private Guid _phone_user_guid;
            private string _phone_number;
            private DateTimeOffset _phone_created_at;
            private DateTimeOffset _phone_updated_at;
            private long _phone_status;

            private long _email_ID;
            private Guid _email_GUID;
            private long _email_user_id;
            private Guid _email_user_guid;
            private string _email;
            private DateTimeOffset _email_created_at;
            private DateTimeOffset _email_updated_at;
            private long _email_status;

            private long _password_ID;
            private Guid _password_GUID;
            private long _password_user_id;
            private Guid _password_user_guid;
            private byte[] _hashed_password;
            private byte[] _password_salt;
            private int _iterations;
            private int _memory_kb;
            private int _parallelism;
            private DateTimeOffset _password_created_at;
            private DateTimeOffset _password_updated_at;
            private long _password_status;


            private long _bytefile_ID;
            private Guid _bytefile_GUID;
            private long _bytefile_owner_id;
            private Guid _bytefile_owner_guid;
            private long _bytefile_album_id;
            private Guid _bytefile_album_guid;
            private long _bytefile_post_id;
            private Guid _bytefile_post_guid;
            private string _bytefile_file_name;
            private byte[] _bytefile_file_data;
            private byte[] _bytefile_thumbnail;
            private int _bytefile_width;
            private int _bytefile_height;
            private long _bytefile_size_byte;
            private string _bytefile_mime_type;
            private string _bytefile_path;
            private long _bytefile_privacy;
            private string _bytefile_storage_bucket;
            private string _bytefile_storage_key;
            private byte[] _bytefile_check_hash;
            private long _bytefile_percept_hash;
            private string _bytefile_exif_details;
            private string _bytefile_derivatives;
            private string _bytefile_embedding_ref;
            private long _bytefile_access_count;
            private DateTimeOffset _bytefile_last_served;
            private DateTimeOffset _bytefile_created_at;
            private DateTimeOffset _bytefile_updated_at;
            private long _bytefile_status;

            // USER_ID
            public long UserID { set { _user_ID = value; } get { return _user_ID; } }
            public Guid UserGUID { set { _user_GUID = value; } get { return _user_GUID; } }
            public string FirstName { set { _first_name = value; } get { return _first_name; } }
            public string LastName { set { _last_name = value; } get { return _last_name; } }
            public string Gender { set { _gender = value; } get { return _gender; } }
            public DateTime BirthDate { set { _birth_date = value; } get { return _birth_date; } }
            public int Age { set { _age = value; } get { return _age; } }
            public DateTimeOffset UserCreatedAt { set { _user_created_at = value; } get { return _user_created_at; } }
            public DateTimeOffset UserUpdatedAt { set { _user_updated_at = value; } get { return _user_updated_at; } }
            public long UserStatus { set { _user_status = value; } get { return _user_status; } }
            public string UserUpdatedBy { set { _updated_by = value; } get { return _updated_by; } }

            // ADDRESSES

            // ========== ADDRESSES ==========

            public long AddressID { set { _address_ID = value; } get { return _address_ID; } }
            public Guid AddressGUID { set { _address_GUID = value; } get { return _address_GUID; } }
            public long AddressUserID { set { _address_user_id = value; } get { return _address_user_id; } }
            public Guid AddressUserGUID { set { _address_user_guid = value; } get { return _address_user_guid; } }
            public string AddressCountry { set { _address_country = value; } get { return _address_country; } }
            public string AddressCity { set { _address_city = value; } get { return _address_city; } }
            public string AddressDistrict { set { _address_district = value; } get { return _address_district; } }
            public string AddressStreet { set { _address_street = value; } get { return _address_street; } }
            public string AddressBuilding { set { _address_building = value; } get { return _address_building; } }
            public string AddressApartment { set { _address_apartment = value; } get { return _address_apartment; } }
            public string AddressPostalCode { set { _address_postal_code = value; } get { return _address_postal_code; } }
            public decimal AddressLatitude { set { _address_latitude = value; } get { return _address_latitude; } }
            public decimal AddressLongitude { set { _address_longitude = value; } get { return _address_longitude; } }
            public DateTimeOffset AddressCreatedAt { set { _address_created_at = value; } get { return _address_created_at; } }
            public DateTimeOffset AddressUpdatedAt { set { _address_updated_at = value; } get { return _address_updated_at; } }
            public long AddressStatus { set { _address_status = value; } get { return _address_status; } }


            // PHONE_ID
            public long PhoneID { set { _phone_ID = value; } get { return _phone_ID; } }
            public Guid PhoneGUID { set { _phone_GUID = value; } get { return _phone_GUID; } }
            public long PhoneUserID { set { _phone_user_id = value; } get { return _phone_user_id; } }
            public Guid PhoneUserGUID { set { _phone_user_guid = value; } get { return _phone_user_guid; } }
            public string Phone { set { _phone_number = value; } get { return _phone_number; } }
            public DateTimeOffset PhoneCreatedAt { set { _phone_created_at = value; } get { return _phone_updated_at; } }
            public DateTimeOffset PhoneUpdatedAt { set { _phone_updated_at = value; } get { return _phone_updated_at; } }
            public long PhoneStatus { set { _phone_status = value; } get { return _phone_status; } }

            // EMAIL_ID
            public long EmailID { set { _email_ID = value; } get { return _email_ID; } }
            public Guid EmailGUID { set { _email_GUID = value; } get { return _email_GUID; } }
            public long EmailUserID { set { _email_user_id = value; } get { return _email_user_id; } }
            public Guid EmailUserGUID { set { _email_user_guid = value; } get { return _email_user_guid; } }
            public string Email { set { _email = value; } get { return _email; } }
            public DateTimeOffset EmailCreatedAt { set { _email_created_at = value; } get { return _email_created_at; } }
            public DateTimeOffset EmailUpdatedAt { set { _email_updated_at = value; } get { return _email_updated_at; } }
            public long EmailStatus { set { _email_status = value; } get { return _email_status; } }

            // PASSWORD_ID
            public long PasswordID { set { _password_ID = value; } get { return _password_ID; } }
            public Guid PasswordGUID { set { _password_GUID = value; } get { return _password_GUID; } }
            public long PasswordUserID { set { _password_user_id = value; } get { return _password_user_id; } }
            public Guid PasswordUserGUID { set { _password_user_guid = value; } get { return _password_user_guid; } }
            public byte[] HashedPassword { set { _hashed_password = value; } get { return _hashed_password; } }
            public byte[] PasswordSalt { set { _password_salt = value; } get { return _password_salt; } }
            public int Iterations { set { _iterations = value; } get { return _iterations; } }
            public int MemoryKb { set { _memory_kb = value; } get { return _memory_kb; } }
            public int Parallelism { set { _parallelism = value; } get { return _parallelism; } }
            public DateTimeOffset PasswordCreatedAt { set { _password_created_at = value; } get { return _password_created_at; } }
            public DateTimeOffset PasswordUpdatedAt { set { _password_updated_at = value; } get { return _password_updated_at; } }
            public long PasswordStatus { set { _password_status = value; } get { return _password_status; } }


            public long ByteFileID { set { _bytefile_ID = value; } get { return _bytefile_ID; } }
            public Guid ByteFileGUID { set { _bytefile_GUID = value; } get { return _bytefile_GUID; } }
            public long ByteFileOwnerID { set { _bytefile_owner_id = value; } get { return _bytefile_owner_id; } }
            public Guid ByteFileOwnerGUID { set { _bytefile_owner_guid = value; } get { return _bytefile_owner_guid; } }
            public long ByteFileAlbumID { set { _bytefile_album_id = value; } get { return _bytefile_album_id; } }
            public Guid ByteFileAlbumGUID { set { _bytefile_album_guid = value; } get { return _bytefile_album_guid; } }
            public long ByteFilePostID { set { _bytefile_post_id = value; } get { return _bytefile_post_id; } }
            public Guid ByteFilePostGUID { set { _bytefile_post_guid = value; } get { return _bytefile_post_guid; } }
            public string ByteFileFileName { set { _bytefile_file_name = value; } get { return _bytefile_file_name; } }
            public byte[] ByteFileFileData { set { _bytefile_file_data = value; } get { return _bytefile_file_data; } }
            public byte[] ByteFileThumbnail { set { _bytefile_thumbnail = value; } get { return _bytefile_thumbnail; } }
            public int ByteFileWidth { set { _bytefile_width = value; } get { return _bytefile_width; } }
            public int ByteFileHeight { set { _bytefile_height = value; } get { return _bytefile_height; } }
            public long ByteFileSizeByte { set { _bytefile_size_byte = value; } get { return _bytefile_size_byte; } }
            public string ByteFileMimeType { set { _bytefile_mime_type = value; } get { return _bytefile_mime_type; } }
            public string ByteFilePath { set { _bytefile_path = value; } get { return _bytefile_path; } }
            public long ByteFilePrivacy { set { _bytefile_privacy = value; } get { return _bytefile_privacy; } }
            public string ByteFileStorageBucket { set { _bytefile_storage_bucket = value; } get { return _bytefile_storage_bucket; } }
            public string ByteFileStorageKey { set { _bytefile_storage_key = value; } get { return _bytefile_storage_key; } }
            public byte[] ByteFileCheckHash { set { _bytefile_check_hash = value; } get { return _bytefile_check_hash; } }
            public long ByteFilePerceptHash { set { _bytefile_percept_hash = value; } get { return _bytefile_percept_hash; } }
            public string ByteFileExifDetails { set { _bytefile_exif_details = value; } get { return _bytefile_exif_details; } }
            public string ByteFileDerivatives { set { _bytefile_derivatives = value; } get { return _bytefile_derivatives; } }
            public string ByteFileEmbeddingRef { set { _bytefile_embedding_ref = value; } get { return _bytefile_embedding_ref; } }
            public long ByteFileAccessCount { set { _bytefile_access_count = value; } get { return _bytefile_access_count; } }
            public DateTimeOffset ByteFileLastServed { set { _bytefile_last_served = value; } get { return _bytefile_last_served; } }
            public DateTimeOffset ByteFileCreatedAt { set { _bytefile_created_at = value; } get { return _bytefile_created_at; } }
            public DateTimeOffset ByteFileUpdatedAt { set { _bytefile_updated_at = value; } get { return _bytefile_updated_at; } }
            public long ByteFileStatus { set { _bytefile_status = value; } get { return _bytefile_status; } }


            private ClsApplication()
            {

            }

            private ClsApplication(
    // ---------- USER ----------
    long UserID, Guid UserGUID, string FirstName, string LastName, string Gender,
    DateTime BirthDate, int Age, DateTimeOffset UserCreatedAt, DateTimeOffset UserUpdatedAt,
    long UserStatus, string UserUpdatedBy)
            {
                // ---------- USER ----------
                _user_ID = UserID;
                _user_GUID = UserGUID;
                _first_name = FirstName;
                _last_name = LastName;
                _gender = Gender;
                _birth_date = BirthDate;
                _age = Age;
                _user_created_at = UserCreatedAt;
                _user_updated_at = UserUpdatedAt;
                _user_status = UserStatus;
                _updated_by = UserUpdatedBy;

            }


            private ClsApplication(
    // ---------- USER ----------
    long UserID, Guid UserGUID, string FirstName, string LastName, string Gender,
    DateTime BirthDate, int Age, DateTimeOffset UserCreatedAt, DateTimeOffset UserUpdatedAt,
    long UserStatus, string UserUpdatedBy,

    // ---------- ADDRESS ----------
    long AddressID, Guid AddressGUID, long AddressUserID, Guid AddressUserGUID,
    string AddressCountry, string AddressCity, string AddressDistrict, string AddressStreet,
    string AddressBuilding, string AddressApartment, string AddressPostalCode,
    decimal AddressLatitude, decimal AddressLongitude,
    DateTimeOffset AddressCreatedAt, DateTimeOffset AddressUpdatedAt, long AddressStatus,

    // ---------- PHONE ----------
    long PhoneID, Guid PhoneGUID, long PhoneUserID, Guid PhoneUserGUID,
    string Phone, DateTimeOffset PhoneCreatedAt, DateTimeOffset PhoneUpdatedAt, long PhoneStatus,

    // ---------- EMAIL ----------
    long EmailID, Guid EmailGUID, long EmailUserID, Guid EmailUserGUID,
    string Email, DateTimeOffset EmailCreatedAt, DateTimeOffset EmailUpdatedAt, long EmailStatus,

    // ---------- PASSWORD ----------
    long PasswordID, Guid PasswordGUID, long PasswordUserID, Guid PasswordUserGUID,
    byte[] HashedPassword, byte[] PasswordSalt, int Iterations, int MemoryKb, int Parallelism,
    DateTimeOffset PasswordCreatedAt, DateTimeOffset PasswordUpdatedAt, long PasswordStatus)
            {

                // ---------- USER ----------
                _user_ID = UserID;
                _user_GUID = UserGUID;
                _first_name = FirstName;
                _last_name = LastName;
                _gender = Gender;
                _birth_date = BirthDate;
                _age = Age;
                _user_created_at = UserCreatedAt;
                _user_updated_at = UserUpdatedAt;
                _user_status = UserStatus;
                _updated_by = UserUpdatedBy;

                // ---------- ADDRESS ----------
                _address_ID = AddressID;
                _address_GUID = AddressGUID;
                _address_user_id = AddressUserID;
                _address_user_guid = AddressUserGUID;
                _address_country = AddressCountry;
                _address_city = AddressCity;
                _address_district = AddressDistrict;
                _address_street = AddressStreet;
                _address_building = AddressBuilding;
                _address_apartment = AddressApartment;
                _address_postal_code = AddressPostalCode;
                _address_latitude = AddressLatitude;
                _address_longitude = AddressLongitude;
                _address_created_at = AddressCreatedAt;
                _address_updated_at = AddressUpdatedAt;
                _address_status = AddressStatus;

                // ---------- EMAIL ----------
                _email_ID = EmailID;
                _email_GUID = EmailGUID;
                _email_user_id = EmailUserID;
                _email_user_guid = EmailUserGUID;
                _email = Email;
                _email_created_at = EmailCreatedAt;
                _email_updated_at = EmailUpdatedAt;
                _email_status = EmailStatus;

                // ---------- PASSWORD ----------
                _password_ID = PasswordID;
                _password_GUID = PasswordGUID;
                _password_user_id = PasswordUserID;
                _password_user_guid = PasswordUserGUID;
                _hashed_password = HashedPassword;
                _password_salt = PasswordSalt;
                _iterations = Iterations;
                _memory_kb = MemoryKb;
                _parallelism = Parallelism;
                _password_created_at = PasswordCreatedAt;
                _password_updated_at = PasswordUpdatedAt;
                _password_status = PasswordStatus;

                // ---------- PHONE ----------
                _phone_ID = PhoneID;
                _phone_GUID = PhoneGUID;
                _phone_user_id = PhoneUserID;
                _phone_user_guid = PhoneUserGUID;
                _phone_number = Phone;
                _phone_created_at = PhoneCreatedAt;
                _phone_updated_at = PhoneUpdatedAt;
                _phone_status = PhoneStatus;

            }




            private ClsApplication(
    // ---------- USER ----------
    long UserID, Guid UserGUID, string FirstName, string LastName, string Gender,
    DateTime BirthDate, int Age, DateTimeOffset UserCreatedAt, DateTimeOffset UserUpdatedAt,
    long UserStatus, string UserUpdatedBy,

    // ---------- ADDRESS ----------
    long AddressID, Guid AddressGUID, long AddressUserID, Guid AddressUserGUID,
    string AddressCountry, string AddressCity, string AddressDistrict, string AddressStreet,
    string AddressBuilding, string AddressApartment, string AddressPostalCode,
    decimal AddressLatitude, decimal AddressLongitude,
    DateTimeOffset AddressCreatedAt, DateTimeOffset AddressUpdatedAt, long AddressStatus,

    // ---------- PHONE ----------
    long PhoneID, Guid PhoneGUID, long PhoneUserID, Guid PhoneUserGUID,
    string Phone, DateTimeOffset PhoneCreatedAt, DateTimeOffset PhoneUpdatedAt, long PhoneStatus,

    // ---------- EMAIL ----------
    long EmailID, Guid EmailGUID, long EmailUserID, Guid EmailUserGUID,
    string Email, DateTimeOffset EmailCreatedAt, DateTimeOffset EmailUpdatedAt, long EmailStatus,

    // ---------- PASSWORD ----------
    long PasswordID, Guid PasswordGUID, long PasswordUserID, Guid PasswordUserGUID,
    byte[] HashedPassword, byte[] PasswordSalt, int Iterations, int MemoryKb, int Parallelism,
    DateTimeOffset PasswordCreatedAt, DateTimeOffset PasswordUpdatedAt, long PasswordStatus,

    // ---------- BYTEFILE ----------
    long ByteFileID, Guid ByteFileGUID, long ByteFileOwnerID, Guid ByteFileOwnerGUID,
    long ByteFileAlbumID, Guid ByteFileAlbumGUID, long ByteFilePostID, Guid ByteFilePostGUID,
    string ByteFileFileName, byte[] ByteFileFileData, byte[] ByteFileThumbnail,
    int ByteFileWidth, int ByteFileHeight, long ByteFileSizeByte,
    string ByteFileMimeType, string ByteFilePath, long ByteFilePrivacy,
    string ByteFileStorageBucket, string ByteFileStorageKey,
    byte[] ByteFileCheckHash, long ByteFilePerceptHash,
    string ByteFileExifDetails, string ByteFileDerivatives, string ByteFileEmbeddingRef,
    long ByteFileAccessCount, DateTimeOffset ByteFileLastServed,
    DateTimeOffset ByteFileCreatedAt, DateTimeOffset ByteFileUpdatedAt, long ByteFileStatus
    )
            {
                // ---------- USER ----------
                _user_ID = UserID;
                _user_GUID = UserGUID;
                _first_name = FirstName;
                _last_name = LastName;
                _gender = Gender;
                _birth_date = BirthDate;
                _age = Age;
                _user_created_at = UserCreatedAt;
                _user_updated_at = UserUpdatedAt;
                _user_status = UserStatus;
                _updated_by = UserUpdatedBy;

                // ---------- ADDRESS ----------
                _address_ID = AddressID;
                _address_GUID = AddressGUID;
                _address_user_id = AddressUserID;
                _address_user_guid = AddressUserGUID;
                _address_country = AddressCountry;
                _address_city = AddressCity;
                _address_district = AddressDistrict;
                _address_street = AddressStreet;
                _address_building = AddressBuilding;
                _address_apartment = AddressApartment;
                _address_postal_code = AddressPostalCode;
                _address_latitude = AddressLatitude;
                _address_longitude = AddressLongitude;
                _address_created_at = AddressCreatedAt;
                _address_updated_at = AddressUpdatedAt;
                _address_status = AddressStatus;

                // ---------- EMAIL ----------
                _email_ID = EmailID;
                _email_GUID = EmailGUID;
                _email_user_id = EmailUserID;
                _email_user_guid = EmailUserGUID;
                _email = Email;
                _email_created_at = EmailCreatedAt;
                _email_updated_at = EmailUpdatedAt;
                _email_status = EmailStatus;

                // ---------- PASSWORD ----------
                _password_ID = PasswordID;
                _password_GUID = PasswordGUID;
                _password_user_id = PasswordUserID;
                _password_user_guid = PasswordUserGUID;
                _hashed_password = HashedPassword;
                _password_salt = PasswordSalt;
                _iterations = Iterations;
                _memory_kb = MemoryKb;
                _parallelism = Parallelism;
                _password_created_at = PasswordCreatedAt;
                _password_updated_at = PasswordUpdatedAt;
                _password_status = PasswordStatus;

                // ---------- PHONE ----------
                _phone_ID = PhoneID;
                _phone_GUID = PhoneGUID;
                _phone_user_id = PhoneUserID;
                _phone_user_guid = PhoneUserGUID;
                _phone_number = Phone;
                _phone_created_at = PhoneCreatedAt;
                _phone_updated_at = PhoneUpdatedAt;
                _phone_status = PhoneStatus;

                // ---------- BYTEFILE ----------
                _bytefile_ID = ByteFileID;
                _bytefile_GUID = ByteFileGUID;
                _bytefile_owner_id = ByteFileOwnerID;
                _bytefile_owner_guid = ByteFileOwnerGUID;
                _bytefile_album_id = ByteFileAlbumID;
                _bytefile_album_guid = ByteFileAlbumGUID;
                _bytefile_post_id = ByteFilePostID;
                _bytefile_post_guid = ByteFilePostGUID;
                _bytefile_file_name = ByteFileFileName;
                _bytefile_file_data = ByteFileFileData;
                _bytefile_thumbnail = ByteFileThumbnail;
                _bytefile_width = ByteFileWidth;
                _bytefile_height = ByteFileHeight;
                _bytefile_size_byte = ByteFileSizeByte;
                _bytefile_mime_type = ByteFileMimeType;
                _bytefile_path = ByteFilePath;
                _bytefile_privacy = ByteFilePrivacy;
                _bytefile_storage_bucket = ByteFileStorageBucket;
                _bytefile_storage_key = ByteFileStorageKey;
                _bytefile_check_hash = ByteFileCheckHash;
                _bytefile_percept_hash = ByteFilePerceptHash;
                _bytefile_exif_details = ByteFileExifDetails;
                _bytefile_derivatives = ByteFileDerivatives;
                _bytefile_embedding_ref = ByteFileEmbeddingRef;
                _bytefile_access_count = ByteFileAccessCount;
                _bytefile_last_served = ByteFileLastServed;
                _bytefile_created_at = ByteFileCreatedAt;
                _bytefile_updated_at = ByteFileUpdatedAt;
                _bytefile_status = ByteFileStatus;
            }


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
                catch (IndexOutOfRangeException)
                {
                    // العمود غير موجود في النتائج
                    return defaultValue;
                }

                // 2️⃣ تحقق من القيمة الفعلية
                if (reader.IsDBNull(ordinal) && typeof(T) == typeof(byte[]))
                    return defaultValue;

                try
                {

                    if (typeof(T) == typeof(byte[]))
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
                        return (T)(object)ms.ToArray();


                        /*byte[] i = SafeGetAsync(ordinal, reader).GetAwaiter().GetResult();
                        Console.WriteLine($"[SafeGet] Reading bytes: Offset = {i}, The I is = {JsonSerializer.Serialize(i)}");
                        Console.ReadKey();
                        return (T)(object)i.ToArray();*/

                    }

                    if (typeof(T) == typeof(Stream))
                    {
                        MemoryStream ms = new();
                        const int chunkSize = 8192;
                        long offset = 0;
                        byte[] chunk = new byte[chunkSize];
                        long bytesRead;

                        while ((bytesRead = reader.GetBytes(ordinal, offset, chunk, 0, chunkSize)) > 0)
                        {
                            ms.Write(chunk, 0, (int)bytesRead);
                            offset += bytesRead;
                        }

                        ms.Position = 0;
                        return (T)(object)ms.ToArray();
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
            
            public static T SafeGet1<T>(DbDataReader reader, string columnName, T defaultValue = default!)
            {
                int ordinal;

                try
                {
                    ordinal = reader.GetOrdinal(columnName);
                }
                catch (IndexOutOfRangeException)
                {
                    // العمود غير موجود في النتائج
                    return defaultValue;
                }

                if (reader.IsDBNull(ordinal))
                    return defaultValue;

                try
                {
                    return reader.GetFieldValue<T>(ordinal);
                }
                catch
                {
                    // إذا كان النوع مختلف (مثلاً DECIMAL بينما نقرأه كـ double)
                    object? val = reader.GetValue(ordinal);
                    if (val is T casted) return casted;
                    return defaultValue;
                }
            }
            public static ClsApplication CreateApplicationFromReader(DbDataReader reader)
            {
                return new ClsApplication(

                    // ---------- User ----------
                    UserID: SafeGet(reader, UserID_MapName, 0L),
                    UserGUID: SafeGet(reader, UserGUID_MapName, Guid.Empty),
                    FirstName: SafeGet(reader, FirstName_MapName, string.Empty),
                    LastName: SafeGet(reader, LastName_MapName, string.Empty),
                    Gender: SafeGet(reader, Gender_MapName, string.Empty),
                    BirthDate: SafeGet(reader, BirthDate_MapName, DateTime.MinValue),
                    Age: SafeGet(reader, Age_MapName, 0),
                    UserCreatedAt: SafeGet(reader, UserCreatedAt_MapName, DateTimeOffset.MinValue),
                    UserUpdatedAt: SafeGet(reader, UserUpdatedAt_MapName, DateTimeOffset.MinValue),
                    UserStatus: SafeGet(reader, UserStatus_MapName, 0L),
                    UserUpdatedBy: SafeGet(reader, UserUpdatedBy_MapName, string.Empty)

                );
            }
            public static ClsApplication CreateApplicationFromReader2(DbDataReader reader)
            {
                return new ClsApplication(

                    // ---------- User ----------
                    UserID: SafeGet(reader, UserID_MapName, 0L),
                    UserGUID: SafeGet(reader, UserGUID_MapName, Guid.Empty),
                    FirstName: SafeGet(reader, FirstName_MapName, string.Empty),
                    LastName: SafeGet(reader, LastName_MapName, string.Empty),
                    Gender: SafeGet(reader, Gender_MapName, string.Empty),
                    BirthDate: SafeGet(reader, BirthDate_MapName, DateTime.MinValue),
                    Age: SafeGet(reader, Age_MapName, 0),
                    UserCreatedAt: SafeGet(reader, UserCreatedAt_MapName, DateTimeOffset.MinValue),
                    UserUpdatedAt: SafeGet(reader, UserUpdatedAt_MapName, DateTimeOffset.MinValue),
                    UserStatus: SafeGet(reader, UserStatus_MapName, 0L),
                    UserUpdatedBy: SafeGet(reader, UserUpdatedBy_MapName, string.Empty),

                    // ---------- Address ----------
                    AddressID: SafeGet(reader, AddressID_MapName, 0L),
                    AddressGUID: SafeGet(reader, AddressGUID_MapName, Guid.Empty),
                    AddressUserID: SafeGet(reader, AddressUserID_MapName, 0L),
                    AddressUserGUID: SafeGet(reader, AddressUserGUID_MapName, Guid.Empty),
                    AddressCountry: SafeGet(reader, AddressCountry_MapName, string.Empty),
                    AddressCity: SafeGet(reader, AddressCity_MapName, string.Empty),
                    AddressDistrict: SafeGet(reader, AddressDistrict_MapName, string.Empty),
                    AddressStreet: SafeGet(reader, AddressStreet_MapName, string.Empty),
                    AddressBuilding: SafeGet(reader, AddressBuilding_MapName, string.Empty),
                    AddressApartment: SafeGet(reader, AddressApartment_MapName, string.Empty),
                    AddressPostalCode: SafeGet(reader, AddressPostalCode_MapName, string.Empty),
                    AddressLatitude: SafeGet(reader, AddressLatitude_MapName, 0m),
                    AddressLongitude: SafeGet(reader, AddressLongitude_MapName, 0m),
                    AddressCreatedAt: SafeGet(reader, AddressCreatedAt_MapName, DateTimeOffset.MinValue),
                    AddressUpdatedAt: SafeGet(reader, AddressUpdatedAt_MapName, DateTimeOffset.MinValue),
                    AddressStatus: SafeGet(reader, AddressStatus_MapName, 0L),

                    // ---------- Phone ----------
                    PhoneID: SafeGet(reader, PhoneID_MapName, 0L),
                    PhoneGUID: SafeGet(reader, PhoneGUID_MapName, Guid.Empty),
                    PhoneUserID: SafeGet(reader, PhoneUserID_MapName, 0L),
                    PhoneUserGUID: SafeGet(reader, PhoneUserGUID_MapName, Guid.Empty),
                    Phone: SafeGet(reader, Phone_MapName, string.Empty),
                    PhoneCreatedAt: SafeGet(reader, PhoneCreatedAt_MapName, DateTimeOffset.MinValue),
                    PhoneUpdatedAt: SafeGet(reader, PhoneUpdatedAt_MapName, DateTimeOffset.MinValue),
                    PhoneStatus: SafeGet(reader, PhoneStatus_MapName, 0L),

                    // ---------- Email ----------
                    EmailID: SafeGet(reader, EmailID_MapName, 0L),
                    EmailGUID: SafeGet(reader, EmailGUID_MapName, Guid.Empty),
                    EmailUserID: SafeGet(reader, EmailUserID_MapName, 0L),
                    EmailUserGUID: SafeGet(reader, EmailUserGUID_MapName, Guid.Empty),
                    Email: SafeGet(reader, Email_MapName, string.Empty),
                    EmailCreatedAt: SafeGet(reader, EmailCreatedAt_MapName, DateTimeOffset.MinValue),
                    EmailUpdatedAt: SafeGet(reader, EmailUpdatedAt_MapName, DateTimeOffset.MinValue),
                    EmailStatus: SafeGet(reader, EmailStatus_MapName, 0L),

                    // ---------- Password ----------
                    PasswordID: SafeGet(reader, PasswordID_MapName, 0L),
                    PasswordGUID: SafeGet(reader, PasswordGUID_MapName, Guid.Empty),
                    PasswordUserID: SafeGet(reader, PasswordUserID_MapName, 0L),
                    PasswordUserGUID: SafeGet(reader, PasswordUserGUID_MapName, Guid.Empty),
                    HashedPassword: SafeGet(reader, HashedPassword_MapName, Array.Empty<byte>()),
                    PasswordSalt: SafeGet(reader, PasswordSalt_MapName, Array.Empty<byte>()),
                    Iterations: SafeGet(reader, Iterations_MapName, 0),
                    MemoryKb: SafeGet(reader, MemoryKb_MapName, 0),
                    Parallelism: SafeGet(reader, Parallelism_MapName, 0),
                    PasswordCreatedAt: SafeGet(reader, PasswordCreatedAt_MapName, DateTimeOffset.MinValue),
                    PasswordUpdatedAt: SafeGet(reader, PasswordUpdatedAt_MapName, DateTimeOffset.MinValue),
                    PasswordStatus: SafeGet(reader, PasswordStatus_MapName, 0L)
                );
            }
            public static ClsApplication CreateApplicationFromReader3(DbDataReader reader)
            {
                return new ClsApplication(

                    // ---------- User ----------
                    UserID: SafeGet(reader, UserID_MapName, 0L),
                    UserGUID: SafeGet(reader, UserGUID_MapName, Guid.Empty),
                    FirstName: SafeGet(reader, FirstName_MapName, string.Empty),
                    LastName: SafeGet(reader, LastName_MapName, string.Empty),
                    Gender: SafeGet(reader, Gender_MapName, string.Empty),
                    BirthDate: SafeGet(reader, BirthDate_MapName, DateTime.MinValue),
                    Age: SafeGet(reader, Age_MapName, 0),
                    UserCreatedAt: SafeGet(reader, UserCreatedAt_MapName, DateTimeOffset.MinValue),
                    UserUpdatedAt: SafeGet(reader, UserUpdatedAt_MapName, DateTimeOffset.MinValue),
                    UserStatus: SafeGet(reader, UserStatus_MapName, 0L),
                    UserUpdatedBy: SafeGet(reader, UserUpdatedBy_MapName, string.Empty),

                    // ---------- Address ----------
                    AddressID: SafeGet(reader, AddressID_MapName, 0L),
                    AddressGUID: SafeGet(reader, AddressGUID_MapName, Guid.Empty),
                    AddressUserID: SafeGet(reader, AddressUserID_MapName, 0L),
                    AddressUserGUID: SafeGet(reader, AddressUserGUID_MapName, Guid.Empty),
                    AddressCountry: SafeGet(reader, AddressCountry_MapName, string.Empty),
                    AddressCity: SafeGet(reader, AddressCity_MapName, string.Empty),
                    AddressDistrict: SafeGet(reader, AddressDistrict_MapName, string.Empty),
                    AddressStreet: SafeGet(reader, AddressStreet_MapName, string.Empty),
                    AddressBuilding: SafeGet(reader, AddressBuilding_MapName, string.Empty),
                    AddressApartment: SafeGet(reader, AddressApartment_MapName, string.Empty),
                    AddressPostalCode: SafeGet(reader, AddressPostalCode_MapName, string.Empty),
                    AddressLatitude: SafeGet(reader, AddressLatitude_MapName, 0m),
                    AddressLongitude: SafeGet(reader, AddressLongitude_MapName, 0m),
                    AddressCreatedAt: SafeGet(reader, AddressCreatedAt_MapName, DateTimeOffset.MinValue),
                    AddressUpdatedAt: SafeGet(reader, AddressUpdatedAt_MapName, DateTimeOffset.MinValue),
                    AddressStatus: SafeGet(reader, AddressStatus_MapName, 0L),

                    // ---------- Phone ----------
                    PhoneID: SafeGet(reader, PhoneID_MapName, 0L),
                    PhoneGUID: SafeGet(reader, PhoneGUID_MapName, Guid.Empty),
                    PhoneUserID: SafeGet(reader, PhoneUserID_MapName, 0L),
                    PhoneUserGUID: SafeGet(reader, PhoneUserGUID_MapName, Guid.Empty),
                    Phone: SafeGet(reader, Phone_MapName, string.Empty),
                    PhoneCreatedAt: SafeGet(reader, PhoneCreatedAt_MapName, DateTimeOffset.MinValue),
                    PhoneUpdatedAt: SafeGet(reader, PhoneUpdatedAt_MapName, DateTimeOffset.MinValue),
                    PhoneStatus: SafeGet(reader, PhoneStatus_MapName, 0L),

                    // ---------- Email ----------
                    EmailID: SafeGet(reader, EmailID_MapName, 0L),
                    EmailGUID: SafeGet(reader, EmailGUID_MapName, Guid.Empty),
                    EmailUserID: SafeGet(reader, EmailUserID_MapName, 0L),
                    EmailUserGUID: SafeGet(reader, EmailUserGUID_MapName, Guid.Empty),
                    Email: SafeGet(reader, Email_MapName, string.Empty),
                    EmailCreatedAt: SafeGet(reader, EmailCreatedAt_MapName, DateTimeOffset.MinValue),
                    EmailUpdatedAt: SafeGet(reader, EmailUpdatedAt_MapName, DateTimeOffset.MinValue),
                    EmailStatus: SafeGet(reader, EmailStatus_MapName, 0L),

                    // ---------- Password ----------
                    PasswordID: SafeGet(reader, PasswordID_MapName, 0L),
                    PasswordGUID: SafeGet(reader, PasswordGUID_MapName, Guid.Empty),
                    PasswordUserID: SafeGet(reader, PasswordUserID_MapName, 0L),
                    PasswordUserGUID: SafeGet(reader, PasswordUserGUID_MapName, Guid.Empty),
                    HashedPassword: SafeGet(reader, HashedPassword_MapName, Array.Empty<byte>()),
                    PasswordSalt: SafeGet(reader, PasswordSalt_MapName, Array.Empty<byte>()),
                    Iterations: SafeGet(reader, Iterations_MapName, 0),
                    MemoryKb: SafeGet(reader, MemoryKb_MapName, 0),
                    Parallelism: SafeGet(reader, Parallelism_MapName, 0),
                    PasswordCreatedAt: SafeGet(reader, PasswordCreatedAt_MapName, DateTimeOffset.MinValue),
                    PasswordUpdatedAt: SafeGet(reader, PasswordUpdatedAt_MapName, DateTimeOffset.MinValue),
                    PasswordStatus: SafeGet(reader, PasswordStatus_MapName, 0L),

                    // ---------- ByteFile ----------
                    ByteFileID: SafeGet(reader, ByteFiles_ID_MapName, 0L),
                    ByteFileGUID: SafeGet(reader, ByteFiles_GUID_MapName, Guid.Empty),
                    ByteFileOwnerID: SafeGet(reader, ByteFiles_OwnerID_MapName, 0L),
                    ByteFileOwnerGUID: SafeGet(reader, ByteFiles_OwnerGUID_MapName, Guid.Empty),
                    ByteFileAlbumID: SafeGet(reader, ByteFiles_AlbumID_MapName, 0L),
                    ByteFileAlbumGUID: SafeGet(reader, ByteFiles_AlbumGUID_MapName, Guid.Empty),
                    ByteFilePostID: SafeGet(reader, ByteFiles_PostID_MapName, 0L),
                    ByteFilePostGUID: SafeGet(reader, ByteFiles_PostGUID_MapName, Guid.Empty),
                    ByteFileFileName: SafeGet(reader, ByteFiles_FileName_MapName, string.Empty),
                    ByteFileFileData: SafeGet(reader, ByteFiles_FileData_MapName, Array.Empty<byte>()),
                    ByteFileThumbnail: SafeGet(reader, ByteFiles_Thumbnail_MapName, Array.Empty<byte>()),
                    ByteFileWidth: SafeGet(reader, ByteFiles_Width_MapName, 0),
                    ByteFileHeight: SafeGet(reader, ByteFiles_Height_MapName, 0),
                    ByteFileSizeByte: SafeGet(reader, ByteFiles_SizeByte_MapName, 0L),
                    ByteFileMimeType: SafeGet(reader, ByteFiles_MimeType_MapName, string.Empty),
                    ByteFilePath: SafeGet(reader, ByteFiles_Path_MapName, string.Empty),
                    ByteFilePrivacy: SafeGet(reader, ByteFiles_Privacy_MapName, 0L),
                    ByteFileStorageBucket: SafeGet(reader, ByteFiles_StorageBucket_MapName, string.Empty),
                    ByteFileStorageKey: SafeGet(reader, ByteFiles_StorageKey_MapName, string.Empty),
                    ByteFileCheckHash: SafeGet(reader, ByteFiles_CheckHash_MapName, Array.Empty<byte>()),
                    ByteFilePerceptHash: SafeGet(reader, ByteFiles_PerceptHash_MapName, 0L),
                    ByteFileExifDetails: SafeGet(reader, ByteFiles_ExifDetails_MapName, string.Empty),
                    ByteFileDerivatives: SafeGet(reader, ByteFiles_Derivatives_MapName, string.Empty),
                    ByteFileEmbeddingRef: SafeGet(reader, ByteFiles_EmbeddingRef_MapName, string.Empty),
                    ByteFileAccessCount: SafeGet(reader, ByteFiles_AccessCount_MapName, 0L),
                    ByteFileLastServed: SafeGet(reader, ByteFiles_LastServed_MapName, DateTimeOffset.MinValue),
                    ByteFileCreatedAt: SafeGet(reader, ByteFiles_CreatedAt_MapName, DateTimeOffset.MinValue),
                    ByteFileUpdatedAt: SafeGet(reader, ByteFiles_UpdatedAt_MapName, DateTimeOffset.MinValue),
                    ByteFileStatus: SafeGet(reader, ByteFiles_Status_MapName, 0L)
                );
            }
            
            public static void PrintClassInfo(ClsApplication obj)
            {
                // ======= Status and Flags =======
                string gender = obj.Gender == "M" ? "MALE" : obj.Gender == "F" ? "FEMALE" : "UNKNOWN";

                string userState = (obj.UserStatus & UserDeletedFlag) == 0 ? "ACTIVE" : "DELETED";

                Console.WriteLine($@"
===================================================================================
                                  USER DETAILS
===================================================================================

USER_ID                  : {obj.UserID}
USER_GUID                : {obj.UserGUID}
FIRST_NAME               : {obj.FirstName?.ToUpper() ?? ""}
LAST_NAME                : {obj.LastName?.ToUpper() ?? ""}
GENDER                   : {gender}
BIRTH_DATE               : {obj.BirthDate:yyyy-MM-dd}
AGE                      : {obj.Age}
CREATED_AT               : {obj.UserCreatedAt:yyyy-MM-dd HH:mm:ss}
UPDATED_AT               : {obj.UserUpdatedAt:yyyy-MM-dd HH:mm:ss}
USER_STATUS              : {userState}

============             =                                         ================
                             END OF USER {obj.UserID} REPORT
===================================================================================
");
            }      
            public static void PrintClassInfo2(ClsApplication obj)
            {
                // ======= Status and Flags =======
                string gender = obj.Gender == "M" ? "MALE" : obj.Gender == "F" ? "FEMALE" : "UNKNOWN";

                string userState = (obj.UserStatus & UserDeletedFlag) == 0 ? "ACTIVE" : "DELETED";
                string emailState = (obj.EmailStatus & EmailDeletedFlag) == 0 ? "ACTIVE" : "DELETED";
                string passwordState = (obj.PasswordStatus & PasswordDeletedFlag) == 0 ? "ACTIVE" : "DELETED";
                string phoneState = (obj.PhoneStatus & PhoneDeletedFlag) == 0 ? "ACTIVE" : "DELETED";
                string addressState = (obj.AddressStatus & AddressDeletedFlag) == 0 ? "ACTIVE" : "DELETED";

                string addressPrimary = (obj.AddressStatus & AddressPrimaryFlag) == AddressPrimaryFlag ? "YES" : "NO";
                string phonePrimary = (obj.PhoneStatus & PhonePrimaryFlag) == PhonePrimaryFlag ? "YES" : "NO";
                string emailPrimary = (obj.EmailStatus & EmailPrimaryFlag) == EmailPrimaryFlag ? "YES" : "NO";
                string passwordPrimary = (obj.PasswordStatus & PasswordPrimaryFlag) == PasswordPrimaryFlag ? "YES" : "NO";

                Console.WriteLine($@"
===================================================================================
                                  USER DETAILS
===================================================================================

USER_ID                  : {obj.UserID}
USER_GUID                : {obj.UserGUID}
FIRST_NAME               : {obj.FirstName?.ToUpper() ?? ""}
LAST_NAME                : {obj.LastName?.ToUpper() ?? ""}
GENDER                   : {gender}
BIRTH_DATE               : {obj.BirthDate:yyyy-MM-dd}
AGE                      : {obj.Age}
CREATED_AT               : {obj.UserCreatedAt:yyyy-MM-dd HH:mm:ss}
UPDATED_AT               : {obj.UserUpdatedAt:yyyy-MM-dd HH:mm:ss}
USER_STATUS              : {userState}

-----------------------------------------------------------------------------------
                                ADDRESS DETAILS
-----------------------------------------------------------------------------------

ADDRESS_ID               : {obj.AddressID}
ADDRESS_GUID             : {obj.AddressGUID}
ADDRESS_USER_ID          : {obj.AddressUserID}
ADDRESS_USER_GUID        : {obj.AddressUserGUID}
COUNTRY                  : {obj.AddressCountry}
CITY                     : {obj.AddressCity}
DISTRICT                 : {obj.AddressDistrict}
STREET                   : {obj.AddressStreet}
BUILDING                 : {obj.AddressBuilding}
APARTMENT                : {obj.AddressApartment}
POSTAL_CODE              : {obj.AddressPostalCode}
LATITUDE                 : {obj.AddressLatitude}
LONGITUDE                : {obj.AddressLongitude}
CREATED_AT               : {obj.AddressCreatedAt:yyyy-MM-dd HH:mm:ss}
UPDATED_AT               : {obj.AddressUpdatedAt:yyyy-MM-dd HH:mm:ss}
IS_PRIMARY               : {addressPrimary}
ADDRESS_STATUS           : {addressState}

-----------------------------------------------------------------------------------
                                 PHONE DETAILS
-----------------------------------------------------------------------------------

PHONE_ID                 : {obj.PhoneID}
PHONE_GUID               : {obj.PhoneGUID}
PHONE_USER_ID            : {obj.PhoneUserID}
PHONE_USER_GUID          : {obj.PhoneUserGUID}
PHONE_NUMBER             : {obj.Phone}
CREATED_AT               : {obj.PhoneCreatedAt:yyyy-MM-dd HH:mm:ss}
UPDATED_AT               : {obj.PhoneUpdatedAt:yyyy-MM-dd HH:mm:ss}
IS_PRIMARY               : {phonePrimary}
PHONE_STATUS             : {phoneState}

-----------------------------------------------------------------------------------
                                 EMAIL DETAILS
-----------------------------------------------------------------------------------

EMAIL_ID                 : {obj.EmailID}
EMAIL_GUID               : {obj.EmailGUID}
EMAIL_USER_ID            : {obj.EmailUserID}
EMAIL_USER_GUID          : {obj.EmailUserGUID}
EMAIL                    : {obj.Email?.ToUpper() ?? ""}
CREATED_AT               : {obj.EmailCreatedAt:yyyy-MM-dd HH:mm:ss}
UPDATED_AT               : {obj.EmailUpdatedAt:yyyy-MM-dd HH:mm:ss}
IS_PRIMARY               : {emailPrimary}
EMAIL_STATUS             : {emailState}

-----------------------------------------------------------------------------------
                               PASSWORD DETAILS
-----------------------------------------------------------------------------------

PASSWORD_ID              : {obj.PasswordID}
PASSWORD_GUID            : {obj.PasswordGUID}
USER_ID                  : {obj.PasswordUserID}
USER_GUID                : {obj.PasswordUserGUID}
HASHED_PASSWORD          : {(obj.HashedPassword != null ? Convert.ToBase64String(obj.HashedPassword) : "NULL")}
PASSWORD_SALT            : {(obj.PasswordSalt != null ? Convert.ToBase64String(obj.PasswordSalt) : "NULL")}
ITERATIONS               : {obj.Iterations}
MEMORY_KB                : {obj.MemoryKb}
PARALLELISM              : {obj.Parallelism}
CREATED_AT               : {obj.PasswordCreatedAt:yyyy-MM-dd HH:mm:ss}
UPDATED_AT               : {obj.PasswordUpdatedAt:yyyy-MM-dd HH:mm:ss}
IS_PRIMARY               : {passwordPrimary}
PASSWORD_STATUS          : {passwordState}

===================================================================================
                             END OF USER {obj.UserID} REPORT
===================================================================================
");
            }
            public static void PrintClassInfo3(ClsApplication obj)
            {
                // ======= Status and Flags =======
                string gender = obj.Gender == "M" ? "MALE" : obj.Gender == "F" ? "FEMALE" : "UNKNOWN";

                string userState = (obj.UserStatus & UserDeletedFlag) == 0 ? "ACTIVE" : "DELETED";
                string emailState = (obj.EmailStatus & EmailDeletedFlag) == 0 ? "ACTIVE" : "DELETED";
                string passwordState = (obj.PasswordStatus & PasswordDeletedFlag) == 0 ? "ACTIVE" : "DELETED";
                string phoneState = (obj.PhoneStatus & PhoneDeletedFlag) == 0 ? "ACTIVE" : "DELETED";
                string addressState = (obj.AddressStatus & AddressDeletedFlag) == 0 ? "ACTIVE" : "DELETED";
                string byteFileState = (obj.ByteFileStatus & ByteFileDeletedFlag) == 0 ? "ACTIVE" : "DELETED";

                string addressPrimary = (obj.AddressStatus & AddressPrimaryFlag) == AddressPrimaryFlag ? "YES" : "NO";
                string phonePrimary = (obj.PhoneStatus & PhonePrimaryFlag) == PhonePrimaryFlag ? "YES" : "NO";
                string emailPrimary = (obj.EmailStatus & EmailPrimaryFlag) == EmailPrimaryFlag ? "YES" : "NO";
                string passwordPrimary = (obj.PasswordStatus & PasswordPrimaryFlag) == PasswordPrimaryFlag ? "YES" : "NO";

                Console.WriteLine($@"
===================================================================================
                                  USER DETAILS
===================================================================================

USER_ID                  : {obj.UserID}
USER_GUID                : {obj.UserGUID}
FIRST_NAME               : {obj.FirstName?.ToUpper() ?? ""}
LAST_NAME                : {obj.LastName?.ToUpper() ?? ""}
GENDER                   : {gender}
BIRTH_DATE               : {obj.BirthDate:yyyy-MM-dd}
AGE                      : {obj.Age}
CREATED_AT               : {obj.UserCreatedAt:yyyy-MM-dd HH:mm:ss}
UPDATED_AT               : {obj.UserUpdatedAt:yyyy-MM-dd HH:mm:ss}
USER_STATUS              : {userState}

-----------------------------------------------------------------------------------
                                ADDRESS DETAILS
-----------------------------------------------------------------------------------

ADDRESS_ID               : {obj.AddressID}
ADDRESS_GUID             : {obj.AddressGUID}
ADDRESS_USER_ID          : {obj.AddressUserID}
ADDRESS_USER_GUID        : {obj.AddressUserGUID}
COUNTRY                  : {obj.AddressCountry}
CITY                     : {obj.AddressCity}
DISTRICT                 : {obj.AddressDistrict}
STREET                   : {obj.AddressStreet}
BUILDING                 : {obj.AddressBuilding}
APARTMENT                : {obj.AddressApartment}
POSTAL_CODE              : {obj.AddressPostalCode}
LATITUDE                 : {obj.AddressLatitude}
LONGITUDE                : {obj.AddressLongitude}
CREATED_AT               : {obj.AddressCreatedAt:yyyy-MM-dd HH:mm:ss}
UPDATED_AT               : {obj.AddressUpdatedAt:yyyy-MM-dd HH:mm:ss}
IS_PRIMARY               : {addressPrimary}
ADDRESS_STATUS           : {addressState}

-----------------------------------------------------------------------------------
                                 PHONE DETAILS
-----------------------------------------------------------------------------------

PHONE_ID                 : {obj.PhoneID}
PHONE_GUID               : {obj.PhoneGUID}
PHONE_USER_ID            : {obj.PhoneUserID}
PHONE_USER_GUID          : {obj.PhoneUserGUID}
PHONE_NUMBER             : {obj.Phone}
CREATED_AT               : {obj.PhoneCreatedAt:yyyy-MM-dd HH:mm:ss}
UPDATED_AT               : {obj.PhoneUpdatedAt:yyyy-MM-dd HH:mm:ss}
IS_PRIMARY               : {phonePrimary}
PHONE_STATUS             : {phoneState}

-----------------------------------------------------------------------------------
                                 EMAIL DETAILS
-----------------------------------------------------------------------------------

EMAIL_ID                 : {obj.EmailID}
EMAIL_GUID               : {obj.EmailGUID}
EMAIL_USER_ID            : {obj.EmailUserID}
EMAIL_USER_GUID          : {obj.EmailUserGUID}
EMAIL                    : {obj.Email?.ToUpper() ?? ""}
CREATED_AT               : {obj.EmailCreatedAt:yyyy-MM-dd HH:mm:ss}
UPDATED_AT               : {obj.EmailUpdatedAt:yyyy-MM-dd HH:mm:ss}
IS_PRIMARY               : {emailPrimary}
EMAIL_STATUS             : {emailState}

-----------------------------------------------------------------------------------
                               PASSWORD DETAILS
-----------------------------------------------------------------------------------

PASSWORD_ID              : {obj.PasswordID}
PASSWORD_GUID            : {obj.PasswordGUID}
USER_ID                  : {obj.PasswordUserID}
USER_GUID                : {obj.PasswordUserGUID}
HASHED_PASSWORD          : {(obj.HashedPassword != null ? Convert.ToBase64String(obj.HashedPassword) : "NULL")}
PASSWORD_SALT            : {(obj.PasswordSalt != null ? Convert.ToBase64String(obj.PasswordSalt) : "NULL")}
ITERATIONS               : {obj.Iterations}
MEMORY_KB                : {obj.MemoryKb}
PARALLELISM              : {obj.Parallelism}
CREATED_AT               : {obj.PasswordCreatedAt:yyyy-MM-dd HH:mm:ss}
UPDATED_AT               : {obj.PasswordUpdatedAt:yyyy-MM-dd HH:mm:ss}
IS_PRIMARY               : {passwordPrimary}
PASSWORD_STATUS          : {passwordState}

-----------------------------------------------------------------------------------
                               BYTE FILE DETAILS
-----------------------------------------------------------------------------------

BYTEFILE_ID              : {obj.ByteFileID}
BYTEFILE_GUID            : {obj.ByteFileGUID}
OWNER_ID                 : {obj.ByteFileOwnerID}
OWNER_GUID               : {obj.ByteFileOwnerGUID}
ALBUM_ID                 : {obj.ByteFileAlbumID}
ALBUM_GUID               : {obj.ByteFileAlbumGUID}
POST_ID                  : {obj.ByteFilePostID}
POST_GUID                : {obj.ByteFilePostGUID}
FILE_NAME                : {obj.ByteFileFileName}
FILE_DATA                : {(obj.ByteFileFileData != null ? Convert.ToBase64String(obj.ByteFileFileData, 1, 1000) : "NULL")}
FILE_THUMBNAIL           : {(obj.ByteFileThumbnail != null ? Convert.ToBase64String(obj.ByteFileThumbnail).Length : "NULL")}
FILE_MIME_TYPE           : {obj.ByteFileMimeType}
FILE_SIZE_BYTES          : {obj.ByteFileSizeByte}
FILE_WIDTH               : {obj.ByteFileWidth}
FILE_HEIGHT              : {obj.ByteFileHeight}
STORAGE_BUCKET           : {obj.ByteFileStorageBucket}
STORAGE_KEY              : {obj.ByteFileStorageKey}
FILE_PATH                : {obj.ByteFilePath}
PRIVACY_LEVEL            : {obj.ByteFilePrivacy}
ACCESS_COUNT             : {obj.ByteFileAccessCount}
LAST_SERVED              : {obj.ByteFileLastServed:yyyy-MM-dd HH:mm:ss}
CREATED_AT               : {obj.ByteFileCreatedAt:yyyy-MM-dd HH:mm:ss}
UPDATED_AT               : {obj.ByteFileUpdatedAt:yyyy-MM-dd HH:mm:ss}
BYTEFILE_STATUS          : {byteFileState}

===================================================================================
                             END OF USER {obj.UserID} REPORT
===================================================================================
");
            }

            //////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////////////
            /// <summary>
            /// HASH_PASSWORD_SEARCH_VERIFY_USER
            /// </summary>
            ////////////////////////////////////////////////////////////////////////
            protected static class clsHashArgon2
            {

                public const int DefaultSaltSize = 16;
                public const int DefaultHashSize = 64;
                public const int DefaultIterations = 3;
                public const int DefaultMemoryKB = 64 * 1024;
                public const int DefaultParallelism = 2;

                public static byte[] GenerateSalt(int size = DefaultSaltSize)
                {
                    byte[] salt = new byte[size];
                    using (var createRandomSalt = RandomNumberGenerator.Create())
                    {
                        createRandomSalt.GetBytes(salt);
                    }

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

                public async Task<(ClsApplication?, bool)> VerifyUserDetails_SQLS(string email, string enteredPassword, CancellationToken ct = default)
                {
                    if (string.IsNullOrEmpty(email)) { return (null, false); }

                    string? SelectQuery_findMaxRecordsByEmail = SQLQueryList("SelectQuery_findMaxRecordsByEmail");

                    try
                    {

                        await using DbConnection connection = CreateConnection();
                        await connection.OpenAsync(ct).ConfigureAwait(false);
                        await using var command = connection.CreateCommand();

                        command.CommandText = SelectQuery_findMaxRecordsByEmail;
                        command.CommandType = CommandType.Text;
                        command.CommandTimeout = CRUD_DefaultDbTimeout;

                        char prefix = SqlParameterPrefix(connection);

                        var parameter1 = command.CreateParameter();
                        parameter1.ParameterName = prefix + "Email";
                        parameter1.Value = email.Trim();
                        parameter1.DbType = DbType.String;

                        var parameter2 = command.CreateParameter();
                        parameter2.ParameterName = prefix + "EmailPrimaryFlag";
                        parameter2.Value = EmailPrimaryFlag;
                        parameter2.DbType = DbType.Int64;

                        var parameter3 = command.CreateParameter();
                        parameter3.ParameterName = prefix + "PasswordPrimaryFlag";
                        parameter3.Value = PasswordPrimaryFlag;
                        parameter3.DbType = DbType.Int64;

                        var parameter4 = command.CreateParameter();
                        parameter4.ParameterName = prefix + "PhonePrimaryFlag";
                        parameter4.Value = PhonePrimaryFlag;
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
                        ClsApplication? obj;
                        if (await reader.ReadAsync(ct).ConfigureAwait(false))
                        {
                            storedHash = await reader.GetFieldValueAsync<byte[]>(reader.GetOrdinal(HashedPassword_MapName), ct).ConfigureAwait(false);
                            storedSalt = await reader.GetFieldValueAsync<byte[]>(reader.GetOrdinal(PasswordSalt_MapName), ct).ConfigureAwait(false);
                            iterations = reader.GetInt32(reader.GetOrdinal(Iterations_MapName));
                            memoryKb = reader.GetInt32(reader.GetOrdinal(MemoryKb_MapName));
                            parallelism = reader.GetInt32(reader.GetOrdinal(Parallelism_MapName));

                            obj = CreateApplicationFromReader2(reader);
                            if (!clsHashArgon2.VerifyPasswordArgon2id(enteredPassword, storedHash, storedSalt, iterations, memoryKb, parallelism))
                            {
                                return (obj, false);
                            }

                            PrintClassInfo(obj);

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

                public async Task<(ClsApplication?, bool)> VerifyUserDetails(string email, string enteredPassword, CancellationToken ct = default)
                {

                    if (string.IsNullOrEmpty(email)) { return (null, false); }

                    string SelectQuery_findRecordsByEmail = @$"SELECT u.{UserID_DBName} AS {UserID_MapName}, u.{UserGUID_DBName} AS {UserGUID_MapName}, u.{FirstName_DBName} AS {FirstName_MapName}, u.{LastName_DBName} AS {LastName_MapName}, u.{Gender_DBName} AS {Gender_MapName}, u.{BirthDate_DBName} AS {BirthDate_MapName}, u.{Age_DBName} AS {Age_MapName}, u.{UserCreatedAt_DBName} AS {UserCreatedAt_MapName}, u.{UserUpdatedAt_DBName} AS {UserUpdatedAt_MapName}, u.{UserStatus_DBName} AS {UserStatus_MapName}, u.{UserUpdatedBy_DBName} AS {UserUpdatedBy_MapName},
                e.{EmailID_DBName} AS {EmailID_MapName}, e.{EmailGUID_DBName} AS {EmailGUID_MapName}, e.{Email_DBName} AS {Email_MapName},e.{EmailUserID_DBName} AS {EmailUserID_MapName},e.{EmailCreatedAt_DBName} AS {EmailCreatedAt_MapName},e.{EmailUpdatedAt_DBName} AS {EmailUpdatedAt_MapName},e.{EmailStatus_DBName} AS {EmailStatus_MapName},                
                ps.{PasswordID_DBName} AS {PasswordID_MapName}, ps.{PasswordGUID_DBName} AS {PasswordGUID_MapName},ps.{HashedPassword_DBName} AS {HashedPassword_MapName},ps.{PasswordSalt_DBName} AS {PasswordSalt_MapName},ps.{Iterations_DBName} AS {Iterations_MapName},ps.{MemoryKb_DBName} AS {MemoryKb_MapName},ps.{Parallelism_DBName} AS {Parallelism_MapName},ps.{PasswordUserID_DBName} AS {PasswordUserID_MapName},ps.{PasswordCreatedAt_DBName} AS {PasswordCreatedAt_MapName},ps.{PasswordUpdatedAt_DBName} AS {PasswordUpdatedAt_MapName},ps.{PasswordStatus_DBName} AS {PasswordStatus_MapName},
                ph.{PhoneID_DBName} AS {PhoneID_MapName}, ph.{PhoneGUID_DBName} AS {PhoneGUID_MapName},ph.{Phone_DBName} AS {Phone_MapName},ph.{PhoneUserID_DBName} AS {PhoneUserID_MapName},ph.{PhoneCreatedAt_DBName} AS {PhoneCreatedAt_MapName},ph.{PhoneUpdatedAt_DBName} AS {PhoneUpdatedAt_MapName},ph.{PhoneStatus_DBName} AS {PhoneStatus_MapName} 
                FROM {Users_Table_DBName} u INNER JOIN {Emails_Table_DBName} e ON u.{UserID_DBName} = e.{EmailUserID_DBName} INNER JOIN {Passwords_Table_DBName} ps ON u.{UserID_DBName} = ps.{PasswordUserID_DBName} INNER JOIN {Phones_Table_DBName} ph ON u.{UserID_DBName} = ph.{PhoneUserID_DBName} WHERE e.{Email_DBName} = @Email AND (e.{EmailStatus_DBName} & @EmailPrimaryFlag) = @EmailPrimaryFlag AND (ps.{PasswordStatus_DBName} & @PasswordPrimaryFlag) = @PasswordPrimaryFlag AND (ph.{PhoneStatus_DBName} & @PhonePrimaryFlag) = @PhonePrimaryFlag;";

                    try
                    {

                        using SqlConnection connection1 = CreateConnectionSqlServer();

                        await connection1.OpenAsync(ct).ConfigureAwait(false);
                        SqlCommand command1 = new SqlCommand(SelectQuery_findRecordsByEmail, connection1);

                        command1.Parameters.Add("@Email", SqlDbType.NVarChar).Value = email.Trim();
                        command1.Parameters.Add("@EmailPrimaryFlag", SqlDbType.BigInt).Value = EmailPrimaryFlag;
                        command1.Parameters.Add("@PasswordPrimaryFlag", SqlDbType.BigInt).Value = PasswordPrimaryFlag;
                        command1.Parameters.Add("@PhonePrimaryFlag", SqlDbType.BigInt).Value = PhonePrimaryFlag;
                        command1.CommandTimeout = CRUD_DefaultDbTimeout;
                        var reader = await command1.ExecuteReaderAsync(ct).ConfigureAwait(false);

                        byte[] storedHash;
                        byte[] storedSalt;
                        int iterations;
                        int memoryKb;
                        int parallelism;

                        if (!reader.HasRows) { return (null, false); }
                        ClsApplication? obj;
                        if (await reader.ReadAsync(ct).ConfigureAwait(false))
                        {
                            storedHash = await reader.GetFieldValueAsync<byte[]>(reader.GetOrdinal(HashedPassword_MapName), ct);
                            storedSalt = await reader.GetFieldValueAsync<byte[]>(reader.GetOrdinal(PasswordSalt_MapName), ct);
                            iterations = reader.GetInt32(reader.GetOrdinal(Iterations_MapName));
                            memoryKb = reader.GetInt32(reader.GetOrdinal(MemoryKb_MapName));
                            parallelism = reader.GetInt32(reader.GetOrdinal(Parallelism_MapName));

                            obj = CreateApplicationFromReader(reader);
                            if (!clsHashArgon2.VerifyPasswordArgon2id(enteredPassword, storedHash, storedSalt, iterations, memoryKb, parallelism))
                            {
                                return (obj, false);
                            }

                            PrintClassInfo(obj);

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

                public static bool VerifyPassword(string enteredPassword, ClsApplication obj)
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

                    string InsertQuery_InsertNewPassword = @$"INSERT INTO {Passwords_Table_DBName} ({PasswordGUID_DBName}, {UserID_DBName}, {UserGUID_DBName}, {HashedPassword_DBName}, {PasswordSalt_DBName}, {Iterations_DBName},
                    {MemoryKb_DBName}, {Parallelism_DBName}, {PasswordCreatedAt_DBName}, {PasswordUpdatedAt_DBName}, {PasswordStatus_DBName})
                    VALUES (@{PasswordGUID_MapName}, @{PasswordUserID_MapName}, @{PasswordUserGUID_MapName}, @{HashedPassword_MapName}, @{PasswordSalt_MapName}, @{Iterations_MapName}, @{MemoryKb_MapName}, @{Parallelism_MapName},
                    @{PasswordCreatedAt_MapName}, @{PasswordUpdatedAt_MapName}, @{PasswordStatus_MapName});";

                    Guid PasswordGuid = Guid.NewGuid();
                    byte[] byteSalt = clsHashArgon2.GenerateSalt(size);
                    byte[] bytePassword = clsHashArgon2.HashPasswordArgon2id(plainPassword, byteSalt, iterations, memoryKb, parallelism, hashLength);
                    try
                    {
                        await using DbConnection connection = CreateConnection();
                        await connection.OpenAsync(ct).ConfigureAwait(false);
                        await using var command = connection.CreateCommand();

                        command.CommandText = InsertQuery_InsertNewPassword;
                        command.CommandType = CommandType.Text;
                        command.CommandTimeout = CRUD_DefaultDbTimeout;

                        char prefix = SqlParameterPrefix(connection);

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

            ///////////////////////////////////////////////////////////////////////////////
            /// <summary>
            /// //////////////////////////////////////////////////////////
            /// 


            // NOTE: This file converts the original MediaInspectors to an async-first, high-performance, production-ready variant.
            // Key design choices made for performance & safety:
            // - Async APIs for all IO-bound operations (ReadAllBytesAsync + SaveAsync + ffprobe async read)
            // - CancellationToken support throughout
            // - Concurrency throttling via an internal SemaphoreSlim (configurable)
            // - Memory-friendly thumbnail handling (returns byte[]; caller can decide to persist/stream)
            // - Compute-heavy tasks are run on ThreadPool via Task.Run to avoid blocking
            // - Optional ILogger for structured logging (no Console.WriteLine)
            // - Bounded cache for MIME detection using MemoryCache

            private static readonly FileFormatInspector _inspector = new FileFormatInspector();
            private static readonly ConcurrentDictionary<string, string> _cache = new();

            public record MediaMetadata
            {
                public long SizeBytes { get; init; }
                public string? MimeType { get; init; } = "";
                public string ChecksumSha256 { get; init; } = "";
                public string ExifJson { get; init; } = "{}";
                public byte[] ThumbnailJpeg { get; init; } = Array.Empty<byte>();
                public ulong AHash { get; init; }
                public int Width { get; init; }
                public int Height { get; init; }
            }

            public record VideoMetadata
            {
                public double? DurationSeconds { get; init; }
                public int? Width { get; init; }
                public int? Height { get; init; }
                public string? FormatName { get; init; }
                public string? Codec { get; init; }
                public long? Bitrate { get; init; }
                public string? RawJson { get; init; }
            }

            public static class MediaInspectors
            {
                private const long MAX_FILE_BYTES = 150 * 1024 * 1024;

                // Throttle parallel image decodes to avoid OOM under high concurrency (tweak as needed)
                private static readonly SemaphoreSlim _decodeSemaphore = new SemaphoreSlim(Environment.ProcessorCount);

                // lightweight in-memory cache for mime detection
                private static readonly MemoryCache _mimeCache = new(new MemoryCacheOptions { SizeLimit = 1024 * 1024 });

                // Optional external inspector (kept from original code)
                private static readonly FileFormatInspector _inspector = new FileFormatInspector();

                // ---------- Public helpers (Async-first) ----------

                public static async Task<MediaMetadata> AnalyzeImageFileAsync(string filePath, int thumbnailMaxSize = 300, CancellationToken ct = default, Microsoft.Extensions.Logging.ILogger? logger = null)
                {
                    if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));
                    if (!File.Exists(filePath)) throw new FileNotFoundException(filePath);

                    ct.ThrowIfCancellationRequested();

                    var bytes = await File.ReadAllBytesAsync(filePath, ct).ConfigureAwait(false);

                    if (bytes.LongLength == 0) throw new ArgumentException("File is invalid");
                    if (bytes.LongLength > MAX_FILE_BYTES) throw new ArgumentException("File exceeds maximum size");

                    return await AnalyzeImageBytesAsync(bytes, filePath, thumbnailMaxSize, ct, logger).ConfigureAwait(false);
                }

                // Synchronous convenience wrapper (keeps original name but calls Async under the hood)
                public static MediaMetadata AnalyzeImageFile_NonAsync(string filePath, int thumbnailMaxSize = 300)
                {
                    // Keep compatibility but run the async pipeline synchronously (not recommended under high-load)
                    return AnalyzeImageFileAsync(filePath, thumbnailMaxSize, CancellationToken.None, logger: null).GetAwaiter().GetResult();
                }

                public static async Task<MediaMetadata> AnalyzeImageBytesAsync(byte[] fileBytes, string? fileNameHint = null, int thumbnailMaxSize = 300, CancellationToken ct = default, Microsoft.Extensions.Logging.ILogger? logger = null)
                {
                    if (fileBytes is null || fileBytes.Length == 0) throw new ArgumentException("fileBytes cannot be null or empty", nameof(fileBytes));
                    if (fileBytes.LongLength > MAX_FILE_BYTES) throw new ArgumentException($"File too large (> {MAX_FILE_BYTES / 1024 / 1024}MB)", nameof(fileBytes));

                    var result = new MediaMetadata
                    {
                        SizeBytes = fileBytes.LongLength,
                        MimeType = clsMimeType.DetectMimeTypeEnhanced(fileBytes, fileNameHint, true) ?? string.Empty,
                        ChecksumSha256 = ComputeSha256HexChecksum(fileBytes),
                        ExifJson = "{}",
                        ThumbnailJpeg = Array.Empty<byte>(),
                        AHash = 0,
                        Width = 0,
                        Height = 0
                    };

                    // Decode + thumbnail + aHash should be throttled to avoid OOM
                    await _decodeSemaphore.WaitAsync(ct).ConfigureAwait(false);
                    try
                    {
                        // Offload image decode & CPU-bound compute to ThreadPool to avoid blocking calling sync-context
                        var decodeTask = Task.Run(async () =>
                        {
                            using var ms = new MemoryStream(fileBytes, writable: false);
                            using var image = SixLabors.ImageSharp.Image.Load<Rgba32>(ms);

                            var meta = new MediaMetadata
                            {
                                Width = image.Width,
                                Height = image.Height
                            };

                            // Create thumbnail (clone + resize) but keep clones minimal
                            using var thumb = image.Clone(ctx => ctx.Resize(new ResizeOptions
                            {
                                Mode = ResizeMode.Max,
                                Size = new Size(thumbnailMaxSize, thumbnailMaxSize),
                                Sampler = KnownResamplers.Lanczos3
                            }));

                            // Save thumbnail to byte[] synchronously inside Task.Run (fast and avoids blocking caller)
                            byte[] thumbBytes;
                            using (var tms = new MemoryStream())
                            {
                                thumb.Save(tms, new JpegEncoder { Quality = 90 });
                                thumbBytes = tms.ToArray();
                            }

                            //byte[] thumbnailBytes = await SaveJpegToMemoryAsync(thumb, 90, ct);
                            // Compute aHash on a small 8x8 clone (CPU-bound)
                            using var smallForHash = image.Clone(ctx => ctx.Resize(new ResizeOptions { Size = new Size(8, 8), Mode = ResizeMode.Stretch }));
                            var aHash = ComputeAverageHashHybrid_InMemory(smallForHash);

                            return (thumbBytes, aHash, meta.Width, meta.Height);
                        }, ct);

                        var (thumbBytesResult, aHashResult, width, height) = await decodeTask.ConfigureAwait(false);

                        result = result with { ThumbnailJpeg = thumbBytesResult, AHash = aHashResult, Width = width, Height = height };
                    }
                    catch (OperationCanceledException) { throw; }
                    catch (Exception ex)
                    {
                        logger?.LogWarning(ex, "Image decode failed");
                    }
                    finally
                    {
                        _decodeSemaphore.Release();
                    }

                    // EXIF extraction (fast, but keep outside of decode critical path) — run in Task.Run because the library is sync
                    try
                    {
                        var exifDict = await Task.Run(() =>
                        {
                            using var msExif = new MemoryStream(fileBytes, writable: false);
                            var directories = ImageMetadataReader.ReadMetadata(msExif);
                            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                            int added = 0;
                            const int maxTags = 2000;

                            foreach (var d in directories)
                            {
                                foreach (var t in d.Tags)
                                {
                                    if (added++ >= maxTags) break;

                                    string key = $"{d.Name}:{t.Name}";
                                    string val = t.Description ?? string.Empty;
                                    if (val.Length > 2000) val = val[..2000] + "...";
                                    // Optionally filter sensitive tags (GPS) here if required
                                    dict[key] = val;
                                }
                                if (added >= maxTags) break;
                            }

                            return JsonSerializer.Serialize(dict);
                        }, ct).ConfigureAwait(false);

                        result = result with { ExifJson = exifDict };
                    }
                    catch (OperationCanceledException) { throw; }
                    catch (Exception ex)
                    {
                        logger?.LogWarning(ex, "EXIF extraction failed");
                    }

                    return result;
                }

                public interface IVideoAnalyzer
                {
                    Task<VideoMetadata> AnalyzeAsync(string filePath);

                }


                // ---------- Video metadata via ffprobe (Async improved) ----------
                public static async Task<VideoMetadata> AnalyzeVideoFileWithFfprobeAsync(string filePath, int ffprobeTimeoutMs = 15000, CancellationToken ct = default, Microsoft.Extensions.Logging.ILogger? logger = null)
                {
                    if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));
                    if (!File.Exists(filePath)) throw new FileNotFoundException(filePath);

                    ct.ThrowIfCancellationRequested();

                    var os = Environment.OSVersion.Platform.ToString().ToLower();
                    string ffmpegFolder = System.IO.Path.Combine(AppContext.BaseDirectory, "ffmpeg");

                    string ffprobePath;
                    if (os.Contains("win"))
                        ffprobePath = System.IO.Path.Combine(ffmpegFolder, "windows", "ffprobe.exe");
                    else if (os.Contains("unix") || os.Contains("linux"))
                        ffprobePath = System.IO.Path.Combine(ffmpegFolder, "linux", "ffprobe");
                    else if (os.Contains("mac"))
                        ffprobePath = System.IO.Path.Combine(ffmpegFolder, "mac", "ffprobe");
                    else
                        throw new PlatformNotSupportedException("Unsupported OS");


                    var startInfo = new ProcessStartInfo
                    {
                        FileName = ffprobePath,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    startInfo.ArgumentList.Add("-v");
                    startInfo.ArgumentList.Add("quiet");
                    startInfo.ArgumentList.Add("-print_format");
                    startInfo.ArgumentList.Add("json");
                    startInfo.ArgumentList.Add("-show_format");
                    startInfo.ArgumentList.Add("-show_streams");
                    startInfo.ArgumentList.Add(filePath);

                    //IVideoAnalyzer analyzer = new FfprobeAnalyzer();


                    using var proc = Process.Start(startInfo) ?? throw new InvalidOperationException("ffprobe not found or could not start.");

                    var outputTask = proc.StandardOutput.ReadToEndAsync();
                    var errTask = proc.StandardError.ReadToEndAsync();

                    var completed = await Task.WhenAny(outputTask, Task.Delay(ffprobeTimeoutMs, ct)).ConfigureAwait(false);
                    if (completed != outputTask)
                    {
                        try { proc.Kill(); } catch { }
                        throw new TimeoutException("ffprobe timed out");
                    }

                    var json = await outputTask.ConfigureAwait(false);
                    var err = await errTask.ConfigureAwait(false);

                    if (string.IsNullOrWhiteSpace(json))
                    {
                        logger?.LogWarning("ffprobe returned empty json. stderr={stderr}", err);
                        throw new InvalidOperationException("ffprobe returned empty response");
                    }

                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    var res = new VideoMetadata { RawJson = json };

                    if (root.TryGetProperty("format", out var fmt))
                    {
                        if (fmt.TryGetProperty("duration", out var durationEl) && durationEl.ValueKind == JsonValueKind.String)
                        {
                            if (double.TryParse(durationEl.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var dur))
                                res = res with { DurationSeconds = dur };
                        }
                        if (fmt.TryGetProperty("format_name", out var fName)) res = res with { FormatName = fName.GetString() };
                        if (fmt.TryGetProperty("bit_rate", out var br) && br.ValueKind == JsonValueKind.String && long.TryParse(br.GetString(), out var brv))
                            res = res with { Bitrate = brv };
                    }

                    if (root.TryGetProperty("streams", out var streams) && streams.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var s in streams.EnumerateArray())
                        {
                            if (s.TryGetProperty("codec_type", out var ctEl) && ctEl.GetString() == "video")
                            {
                                string? codec = s.TryGetProperty("codec_name", out var cn) ? cn.GetString() : null;
                                int? w = s.TryGetProperty("width", out var wEl) && wEl.TryGetInt32(out var wi) ? wi : (int?)null;
                                int? h = s.TryGetProperty("height", out var hEl) && hEl.TryGetInt32(out var hi) ? hi : (int?)null;

                                res = res with { Codec = codec, Width = w, Height = h };
                                break;
                            }
                        }
                    }

                    return res;
                }

                // ---------- Utilities ----------

                private static string ComputeSha256HexChecksum(byte[] bytes)
                {
                    using var sha = SHA256.Create();
                    var hash = sha.ComputeHash(bytes);
                    return Convert.ToHexString(hash).ToLowerInvariant();
                }

                // Async-friendly jpeg save that RETURNS byte[] (preferred for async flows)
                private static async Task<byte[]> SaveJpegToMemoryAsync(Image<Rgba32> image, int quality = 90, CancellationToken ct = default)
                {
                    using var ms = new MemoryStream();
                    var encoder = new JpegEncoder { Quality = quality };
                    await image.SaveAsync(ms, encoder, ct).ConfigureAwait(false);
                    return ms.ToArray();
                }

                // aHash implementation (kept optimized) - made internal and synchronous; callers run it inside Task.Run
                private static ulong ComputeAverageHashHybrid_InMemory(Image<Rgba32> source)
                {

                    const int size = 8;
                    using var image = source.CloneAs<Rgba32>();

                    double sum = 0;
                    int total = size * size;
                    bool usedDangerous = false;
                    bool usedProcess = false;

                    try
                    {
                        for (int y = 0; y < size; y++)
                        {
                            var span = image.DangerousGetPixelRowMemory(y).Span;
                            for (int x = 0; x < size; x++)
                                sum += 0.299 * span[x].R + 0.587 * span[x].G + 0.114 * span[x].B;
                        }
                        usedDangerous = true;
                    }
                    catch
                    {
                        try
                        {
                            image.ProcessPixelRows(accessor =>
                            {
                                for (int y = 0; y < size; y++)
                                {
                                    var rowSpan = accessor.GetRowSpan(y);
                                    for (int x = 0; x < size; x++)
                                        sum += 0.299 * rowSpan[x].R + 0.587 * rowSpan[x].G + 0.114 * rowSpan[x].B;
                                }
                            });
                            usedProcess = true;
                        }

                        catch
                        {
                            for (int y = 0; y < size; y++)
                                for (int x = 0; x < size; x++)
                                {
                                    var px = image[x, y];
                                    sum += 0.299 * px.R + 0.587 * px.G + 0.114 * px.B;
                                }
                        }
                    }

                    double avg = sum / total;
                    ulong hash = 0;

                    if (usedDangerous)
                    {
                        int bit = 0;
                        for (int y = 0; y < size; y++)
                        {
                            var span = image.DangerousGetPixelRowMemory(y).Span;
                            for (int x = 0; x < size; x++, bit++)
                            {
                                double lum = 0.299 * span[x].R + 0.587 * span[x].G + 0.114 * span[x].B;
                                if (lum >= avg) hash |= (1UL << bit);
                            }
                        }
                        return hash;
                    }

                    if (usedProcess)
                    {
                        int bit = 0;
                        image.ProcessPixelRows(accessor =>
                        {
                            for (int y = 0; y < size; y++)
                            {
                                var rowSpan = accessor.GetRowSpan(y);
                                for (int x = 0; x < size; x++, bit++)
                                {
                                    double lum = 0.299 * rowSpan[x].R + 0.587 * rowSpan[x].G + 0.114 * rowSpan[x].B;
                                    if (lum >= avg) hash |= (1UL << bit);
                                }
                            }
                        });
                        return hash;
                    }

                    int bit2 = 0;
                    for (int y = 0; y < size; y++)

                        for (int x = 0; x < size; x++, bit2++)
                        {
                            var px = image[x, y];
                            double lum = 0.299 * px.R + 0.587 * px.G + 0.114 * px.B;
                            if (lum >= avg) hash |= (1UL << bit2);
                        }
                    return hash;

                }



                // ---------- MIME detection (kept robust but async-friendly caching)
                private static class clsMimeType
                {
                    private const int SAMPLE_HIGH = 32;
                    private const int SAMPLE_LOW = 16;

                    public static string? DetectMimeTypeEnhanced(byte[] fileBytes, string? fileNameHint = null, bool highAccuracy = false)
                    {
                        if (fileBytes == null || fileBytes.Length == 0) return "application/octet-stream";
                        if (fileBytes.LongLength > MAX_FILE_BYTES) throw new ArgumentException("file too large");

                        int sampleLen = highAccuracy ? SAMPLE_HIGH : SAMPLE_LOW;
                        sampleLen = Math.Min(sampleLen, fileBytes.Length);

                        var prefixHash = SHA256.HashData(fileBytes.AsSpan(0, sampleLen));
                        string shortHash = Convert.ToBase64String(prefixHash, 0, Math.Min(12, prefixHash.Length));
                        string cacheKey = (highAccuracy ? "H_" : "L_") + shortHash + "_" + fileBytes.Length;

                        if (_mimeCache.TryGetValue(cacheKey, out string cached)) return cached;

                        string? result = QuickMagicCheck(fileBytes);

                        if (string.IsNullOrEmpty(result))
                        {
                            try
                            {
                                using var ms = new MemoryStream(fileBytes, writable: false);
                                var format = _inspector?.DetermineFileFormat(ms);
                                result = format?.MediaType;
                            }
                            catch { result = null; }
                        }

                        if (string.IsNullOrEmpty(result)) result = FtypScan(fileBytes);
                        if (string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(fileNameHint)) result = ExtensionLookup(fileNameHint);
                        if (string.IsNullOrEmpty(result)) result = "application/octet-stream";
                        if (highAccuracy) result = ValidateMimeType(fileBytes, result);

                        // cache with a small size cost estimate
                        _mimeCache.Set(cacheKey, result, new MemoryCacheEntryOptions { Size = 1, SlidingExpiration = TimeSpan.FromMinutes(30) });

                        return result;
                    }

                    // helpers (identical to original fast checks)
                    private static string? QuickMagicCheck(byte[] b)
                    {
                        if (b.Length >= 2 && b[0] == 0xFF && b[1] == 0xD8) return "image/jpeg";
                        if (b.Length >= 8 && b[0] == 0x89 && b[1] == 0x50 && b[2] == 0x4E && b[3] == 0x47) return "image/png";
                        if (b.Length >= 3 && b[0] == 0x47 && b[1] == 0x49 && b[2] == 0x46) return "image/gif";
                        if (b.Length >= 12 && b[0] == 0x52 && b[1] == 0x49 && b[2] == 0x46 && b[3] == 0x46
                            && b[8] == 0x57 && b[9] == 0x45 && b[10] == 0x42 && b[11] == 0x50) return "image/webp";
                        if (b.Length >= 4 && b[0] == (byte)'P' && b[1] == (byte)'K') return "application/zip";
                        if (b.Length >= 4 && b[0] == (byte)'%' && b[1] == (byte)'P' && b[2] == (byte)'D' && b[3] == (byte)'F') return "application/pdf";
                        if (b.Length >= 2 && b[0] == (byte)'B' && b[1] == (byte)'M') return "image/bmp";
                        if (b.Length >= 4 && ((b[0] == 0x49 && b[1] == 0x49 && b[2] == 0x2A && b[3] == 0x00) ||
                                              (b[0] == 0x4D && b[1] == 0x4D && b[2] == 0x00 && b[3] == 0x2A))) return "image/tiff";
                        if (b.Length >= 2 && b[0] == 0x4D && b[1] == 0x5A) return "application/x-msdownload";
                        if (b.Length >= 12 && LooksLikeMp4(b)) return "video/mp4";
                        return null;
                    }

                    private static string? FtypScan(byte[] b)
                    {
                        if (b.Length >= 4)
                        {
                            if (b[0] == '<' || (b[0] == 0xEF && b[1] == 0xBB && b[2] == 0xBF && b[3] == '<'))
                            {
                                var head = Encoding.UTF8.GetString(b, 0, Math.Min(512, b.Length)).ToLowerInvariant();
                                if (head.Contains("<svg")) return "image/svg+xml";
                            }
                        }

                        int scan = Math.Min(256, b.Length);
                        for (int i = 0; i + 3 < scan; i++)
                        {
                            if (b[i] == (byte)'f' && b[i + 1] == (byte)'t' && b[i + 2] == (byte)'y' && b[i + 3] == (byte)'p')
                            {
                                int brandPos = i + 4;
                                if (brandPos + 3 < scan)
                                {
                                    string brand = Encoding.ASCII.GetString(b, brandPos, Math.Min(4, scan - brandPos)).Trim('\0').ToLowerInvariant();
                                    var mp4Brands = new[] { "isom", "iso2", "mp41", "mp42", "avc1", "dash", "iso6", "mif1", "msf1" };
                                    var heifBrands = new[] { "heic", "heix", "hevc", "hevx", "mif1", "msf1" };

                                    if (mp4Brands.Contains(brand)) return "video/mp4";
                                    if (heifBrands.Contains(brand)) return "image/heif";
                                    if (brand == "qt  " || brand == "pnot") return "video/quicktime";
                                    return "video/mp4";
                                }
                            }
                        }
                        return null;
                    }

                    private static bool LooksLikeMp4(byte[] b)
                    {
                        if (b.Length >= 12 && b[4] == (byte)'f' && b[5] == (byte)'t' && b[6] == (byte)'y' && b[7] == (byte)'p') return true;
                        return FtypScan(b) != null;
                    }

                    private static string? ExtensionLookup(string fileNameHint)
                    {
                        try
                        {
                            var ext = System.IO.Path.GetExtension(fileNameHint)?.ToLowerInvariant() ?? string.Empty;
                            return ext switch
                            {
                                ".jpg" or ".jpeg" => "image/jpeg",
                                ".png" => "image/png",
                                ".gif" => "image/gif",
                                ".webp" => "image/webp",
                                ".bmp" => "image/bmp",
                                ".tif" or ".tiff" => "image/tiff",
                                ".heic" or ".heif" => "image/heif",
                                ".mp4" => "video/mp4",
                                ".mov" => "video/quicktime",
                                ".mkv" => "video/x-matroska",
                                ".avi" => "video/x-msvideo",
                                ".mp3" => "audio/mpeg",
                                ".wav" => "audio/wave",
                                ".pdf" => "application/pdf",
                                ".zip" => "application/zip",
                                _ => null
                            };
                        }
                        catch { return null; }
                    }

                    private static string ValidateMimeType(byte[] bytes, string mime)
                    {
                        if (string.IsNullOrEmpty(mime)) return "application/octet-stream";
                        mime = mime.ToLowerInvariant();
                        try
                        {
                            if (mime == "image/jpeg" && (bytes.Length < 2 || bytes[0] != 0xFF || bytes[1] != 0xD8)) return "application/octet-stream";
                            if (mime == "image/png" && (bytes.Length < 8 || bytes[0] != 0x89 || bytes[1] != 0x50 || bytes[2] != 0x4E || bytes[3] != 0x47)) return "application/octet-stream";
                            if (mime == "image/gif" && (bytes.Length < 3 || bytes[0] != 0x47 || bytes[1] != 0x49 || bytes[2] != 0x46)) return "application/octet-stream";
                            if (mime == "image/webp" && (bytes.Length < 12 || !(bytes[0] == 0x52 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x46))) return "application/octet-stream";
                            if (mime.StartsWith("video/") && !string.IsNullOrEmpty(mime))
                            {
                                if (mime == "video/mp4" && !LooksLikeMp4(bytes)) return "application/octet-stream";
                            }
                            if (mime == "application/pdf" && (bytes.Length < 4 || bytes[0] != (byte)'%' || bytes[1] != (byte)'P' || bytes[2] != (byte)'D' || bytes[3] != (byte)'F'))
                                return "application/octet-stream";
                        }
                        catch { return "application/octet-stream"; }
                        return mime;
                    }
                }


            }

            class clsExif
            {

                public class ExifOptions
                {
                    public int MaxTagCount { get; set; } = 1000;
                    public int MaxStringLength { get; set; } = 2000;
                    public bool TrimValues { get; set; } = true;
                }

                public class ExifDataResults
                {
                    public Dictionary<string, string> Tags { get; set; } = new();
                    public string CameraMake { get; set; } = "";
                    public string CameraModel { get; set; } = "";
                    public DateTime? DateTimeOriginal { get; set; }
                    public double? Latitude { get; set; }
                    public double? Longitude { get; set; }

                    public string ToJson() => JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                }

                public static class ExifReader
                {
                    /// <summary>
                    /// Robust EXIF extractor for .NET 8.
                    /// Input: image bytes (byte[]).
                    /// Output: ExifResult with tags + common fields.
                    /// </summary>
                    public static ExifDataResults ExtractExif(byte[] fileBytes, ExifOptions? options = null)
                    {
                        options ??= new ExifOptions();
                        var result = new ExifDataResults();

                        if (fileBytes == null || fileBytes.Length < 8)
                            return result;

                        if (!IsItLooksLikeImage(fileBytes))
                            return result;

                        try
                        {
                            using var ms = new MemoryStream(fileBytes, writable: false);

                            // NOTE: return type is IReadOnlyList<MetadataExtractor.Directory>, alias used above to avoid conflict
                            IReadOnlyList<MetadataExtractor.Directory> directories = ImageMetadataReader.ReadMetadata(ms);

                            // collect tags safely
                            int added = 0;
                            foreach (var dir in directories)
                            {
                                foreach (var tag in dir.Tags)
                                {
                                    if (added >= options.MaxTagCount) break;

                                    string key = $"{dir.Name}:{tag.Name}";
                                    string value = tag.Description ?? string.Empty;

                                    if (options.TrimValues) value = value.Trim();

                                    if (value.Length > options.MaxStringLength)
                                        value = value.Substring(0, options.MaxStringLength) + "...";

                                    if (!string.IsNullOrEmpty(value) && !result.Tags.ContainsKey(key))
                                    {
                                        result.Tags[key] = value;
                                        added++;
                                    }
                                }

                                if (added >= options.MaxTagCount) break;
                            }

                            // camera make/model (search common directories)
                            result.CameraMake = GetFirstTagValue(directories, ExifDirectoryBase.TagMake);
                            result.CameraModel = GetFirstTagValue(directories, ExifDirectoryBase.TagModel);

                            // DateTimeOriginal (as string then parse)
                            string? dateStr = GetFirstTagValue(directories, ExifDirectoryBase.TagDateTimeOriginal);
                            if (!string.IsNullOrEmpty(dateStr) && TryParseExifDate(dateStr, out var dt))
                                result.DateTimeOriginal = dt;

                            // GPS: try rational arrays first, then fallback to textual parse
                            var gpsDir = directories.OfType<GpsDirectory>().FirstOrDefault();
                            if (gpsDir != null)
                            {
                                double? lat = null, lon = null;

                                // try rational arrays
                                try
                                {
                                    if (gpsDir.ContainsTag(GpsDirectory.TagLatitude))
                                    {
                                        var latR = gpsDir.GetRationalArray(GpsDirectory.TagLatitude);
                                        var latRef = gpsDir.GetDescription(GpsDirectory.TagLatitudeRef);
                                        lat = ConvertRationalArrayToDegrees(latR, latRef);
                                    }
                                }
                                catch
                                {
                                    lat = null;
                                }

                                try
                                {
                                    if (gpsDir.ContainsTag(GpsDirectory.TagLongitude))
                                    {
                                        var lonR = gpsDir.GetRationalArray(GpsDirectory.TagLongitude);
                                        var lonRef = gpsDir.GetDescription(GpsDirectory.TagLongitudeRef);
                                        lon = ConvertRationalArrayToDegrees(lonR, lonRef);
                                    }
                                }
                                catch
                                {
                                    lon = null;
                                }

                                // fallback: parse textual descriptions like "30° 3' 20.00\" N"
                                if (lat == null)
                                {
                                    var latDesc = gpsDir.GetDescription(GpsDirectory.TagLatitude);
                                    var latRefDesc = gpsDir.GetDescription(GpsDirectory.TagLatitudeRef);
                                    lat = ParseDmsString(latDesc, latRefDesc);
                                }

                                if (lon == null)
                                {
                                    var lonDesc = gpsDir.GetDescription(GpsDirectory.TagLongitude);
                                    var lonRefDesc = gpsDir.GetDescription(GpsDirectory.TagLongitudeRef);
                                    lon = ParseDmsString(lonDesc, lonRefDesc);
                                }

                                result.Latitude = lat;
                                result.Longitude = lon;
                            }

                            return result;
                        }
                        catch
                        {
                            // Safe fallback on any parsing error
                            return result;
                        }
                    }

                    // ---------------- helpers ----------------

                    // get first occurrence of a tag (search common EXIF dirs first)
                    private static string? GetFirstTagValue(IReadOnlyList<MetadataExtractor.Directory> dirs, int tagType)
                    {
                        var d1 = dirs.OfType<ExifIfd0Directory>().FirstOrDefault();
                        if (d1 != null && d1.ContainsTag(tagType))
                            return d1.GetDescription(tagType);

                        var d2 = dirs.OfType<ExifSubIfdDirectory>().FirstOrDefault();
                        if (d2 != null && d2.ContainsTag(tagType))
                            return d2.GetDescription(tagType);

                        foreach (var d in dirs)
                        {
                            if (d.ContainsTag(tagType))
                            {
                                var desc = d.GetDescription(tagType);
                                if (!string.IsNullOrEmpty(desc)) return desc;
                            }
                        }
                        return null;
                    }

                    // parse typical EXIF datetime "yyyy:MM:dd HH:mm:ss"
                    private static bool TryParseExifDate(string dateStr, out DateTime dt)
                    {
                        dt = default;
                        if (string.IsNullOrWhiteSpace(dateStr)) return false;
                        string[] formats = { "yyyy:MM:dd HH:mm:ss", "yyyy:MM:dd HH:mm:ss\0", "yyyy-MM-dd HH:mm:ss", "yyyy-MM-ddTHH:mm:ss" };
                        return DateTime.TryParseExact(dateStr.Trim(), formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeLocal, out dt);
                    }

                    // convert Rational[] (degrees/minutes/seconds) + ref ("N"/"S"/"E"/"W") to signed degrees
                    private static double? ConvertRationalArrayToDegrees(MetadataExtractor.Rational[] arr, string reference)
                    {
                        if (arr == null || arr.Length < 3) return null;
                        try
                        {
                            double d = arr[0].ToDouble();
                            double m = arr[1].ToDouble();
                            double s = arr[2].ToDouble();
                            double coord = d + (m / 60.0) + (s / 3600.0);
                            if (!string.IsNullOrEmpty(reference) && (reference.StartsWith("S", StringComparison.OrdinalIgnoreCase) || reference.StartsWith("W", StringComparison.OrdinalIgnoreCase)))
                                coord = -coord;
                            return Math.Round(coord, 7);
                        }
                        catch
                        {
                            return null;
                        }
                    }

                    // parse textual DMS fallback e.g. "30° 3' 20.00\""
                    private static double? ParseDmsString(string dmsText, string reference)
                    {
                        if (string.IsNullOrWhiteSpace(dmsText)) return null;

                        var rx = new Regex(@"(?<deg>[-+]?\d+(?:\.\d+)?)\D+(?<min>\d+(?:\.\d+)?)\D+(?<sec>\d+(?:\.\d+)?)", RegexOptions.Compiled);
                        var m = rx.Match(dmsText);
                        if (m.Success)
                        {
                            if (double.TryParse(m.Groups["deg"].Value, out var deg) &&
                                double.TryParse(m.Groups["min"].Value, out var min) &&
                                double.TryParse(m.Groups["sec"].Value, out var sec))
                            {
                                double coord = deg + (min / 60.0) + (sec / 3600.0);
                                if (!string.IsNullOrEmpty(reference) && (reference.StartsWith("S", StringComparison.OrdinalIgnoreCase) || reference.StartsWith("W", StringComparison.OrdinalIgnoreCase)))
                                    coord = -coord;
                                return Math.Round(coord, 7);
                            }
                        }

                        // try parse simple float
                        if (double.TryParse(dmsText, out var simple))
                            return simple;

                        return null;
                    }

                    private static bool IsItLooksLikeImage(byte[] b)
                    {
                        if (b.Length < 4) return false;
                        // JPEG
                        if (b[0] == 0xFF && b[1] == 0xD8) return true;
                        // PNG
                        if (b.Length >= 8 && b[0] == 0x89 && b[1] == 0x50 && b[2] == 0x4E && b[3] == 0x47) return true;
                        // GIF
                        if (b[0] == 0x47 && b[1] == 0x49 && b[2] == 0x46) return true;
                        // WebP (RIFF....WEBP)
                        if (b.Length >= 12 && b[0] == 0x52 && b[1] == 0x49 && b[2] == 0x46 && b[3] == 0x46
                            && b[8] == 0x57 && b[9] == 0x45 && b[10] == 0x42 && b[11] == 0x50) return true;
                        // TIFF little/big
                        if ((b[0] == 0x49 && b[1] == 0x49 && b[2] == 0x2A && b[3] == 0x00) ||
                            (b[0] == 0x4D && b[1] == 0x4D && b[2] == 0x00 && b[3] == 0x2A)) return true;

                        return false;
                    }
                }


            }

            ///////////kljhvfbgnhkjlkjnhbgvfd//////////////

            public static class MediaInspectors_2
            {

                const long MAX_FILE_BYTES = 150 * 1024 * 1024;

                // Throttle parallel image decodes to avoid OOM under high concurrency (tweak as needed)
                private static readonly SemaphoreSlim _decodeSemaphore = new SemaphoreSlim(Environment.ProcessorCount);

                // lightweight in-memory cache for mime detection
                private static readonly MemoryCache _mimeCache = new(new MemoryCacheOptions { SizeLimit = 1024 * 1024 });

                // Optional external inspector (kept from original code)
                private static readonly FileFormatInspector _inspector = new FileFormatInspector();

                public static MediaMetadata AnalyzeImageBytes(byte[] fileBytes, string? fileNameHint = null, int thumbnailMaxSize = 300)
                {
                    // ✅ تحقق أولي من البيانات
                    if (fileBytes is null || fileBytes.Length == 0)
                        throw new ArgumentException("fileBytes cannot be null or empty", nameof(fileBytes));

                    if (fileBytes.LongLength > MAX_FILE_BYTES)
                        throw new ArgumentException($"File too large (> {MAX_FILE_BYTES / 1024 / 1024}MB)", nameof(fileBytes));

                    // ✅ كائن النتائج المبدئي
                    var result = new MediaMetadata
                    {
                        SizeBytes = fileBytes.LongLength,
                        MimeType = clsMimeType.DetectMimeTypeEnhanced(fileBytes, fileNameHint, true) ?? "",
                        ChecksumSha256 = ComputeSha256HexChecksum(fileBytes),
                        ExifJson = "{}",
                        ThumbnailJpeg = Array.Empty<byte>(),
                        AHash = 0,
                        Width = 0,
                        Height = 0
                    };

                    // ✅  تحليل الصورة وإنشاء الصورة المصغرة
                    try
                    {
                        using var ms = new MemoryStream(fileBytes, writable: false);
                        using var image = SixLabors.ImageSharp.Image.Load<Rgba32>(ms); // ✅ بدون out IImageFormat

                        // حفظ الأبعاد
                        result = result with
                        {
                            Width = image.Width,
                            Height = image.Height
                        };

                        // ✅ إنشاء الصورة المصغّرة بأداء عالي وجودة ممتازة
                        using var thumb = image.Clone(ctx => ctx.Resize(new ResizeOptions
                        {
                            Mode = ResizeMode.Max,
                            Size = new Size(thumbnailMaxSize, thumbnailMaxSize),
                            Sampler = KnownResamplers.Lanczos3
                        }));

                        //using var msThumb = new MemoryStream();
                        //thumb.Save(msThumb, new JpegEncoder { Quality = 90 });
                        //result = result with { ThumbnailJpeg = msThumb.ToArray() };

                        //// ✅ توليد Average Hash (بصمة بصرية)
                        //result = result with { AHash = ComputeAverageHashFromImage(image) };
                        awaitSaveJpegToMemory(thumb, out var thumbBytes, quality: 90);
                        result = result with { ThumbnailJpeg = thumbBytes };

                        // compute aHash using HYBRID method:
                        // For best performance, compute on a small 8x8 clone
                        using var smallForHash = image.Clone(ctx => ctx.Resize(new ResizeOptions { Size = new Size(8, 8), Mode = ResizeMode.Stretch }));
                        var aHash = ComputeAverageHashHybrid(smallForHash);
                        result = result with { AHash = aHash };
                    }
                    catch (Exception ex)
                    {
                        // ⚠️ لا توقف التنفيذ — سجل الخطأ فقط
                        Console.Error.WriteLine($"[WARN] Image decode failed: {ex.Message}");
                    }

                    // ✅ استخراج بيانات EXIF باستخدام MetadataExtractor
                    try
                    {
                        using var msExif = new MemoryStream(fileBytes, writable: false);
                        var directories = ImageMetadataReader.ReadMetadata(msExif);
                        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                        int added = 0;
                        const int maxTags = 2000;

                        foreach (var d in directories)
                        {
                            foreach (var t in d.Tags)
                            {
                                if (added++ >= maxTags) break;

                                string key = $"{d.Name}:{t.Name}";
                                string val = t.Description ?? string.Empty;
                                if (val.Length > 2000) val = val[..2000] + "...";
                                dict[key] = val;
                            }
                            if (added >= maxTags) break;
                        }

                        result = result with { ExifJson = JsonSerializer.Serialize(dict) };
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"[WARN] EXIF extraction failed: {ex.Message}");
                    }

                    return result;
                }
                // ---------- Video metadata via ffprobe (recommended) ----------
                // Requires ffprobe in PATH. Cross-platform if ffprobe installed.

                // ---------- Utilities ----------


                // ---------------- SHA256 helper (recommended)
                private static string ComputeSha256HexChecksum(byte[] bytes)
                {
                    using var sha = SHA256.Create();
                    var hash = sha.ComputeHash(bytes);
                    return Convert.ToHexString(hash).ToLowerInvariant(); // efficient
                }

                // ---------------- JPEG save helper (synchronous memory write)
                private static void awaitSaveJpegToMemory(Image<Rgba32> image, out byte[] bytes, int quality = 90)
                {
                    using var ms = new MemoryStream();
                    image.Save(ms, new JpegEncoder { Quality = quality });
                    bytes = ms.ToArray();
                }

                private static async Task SaveJpegToMemoryAsync(Image<Rgba32> image, MemoryStream ms, int quality = 90, CancellationToken ct = default)
                {
                    // ImageSharp supports async saving
                    var encoder = new JpegEncoder { Quality = quality };
                    await image.SaveAsync(ms, encoder, ct).ConfigureAwait(false);
                    ms.Position = 0;
                }

                private static ulong ComputeAverageHashHybrid(Image<Rgba32> smallImage)
                {
                    // smallImage is expected to be a small clone (e.g., 8x8). This function DOES NOT dispose it.
                    // We'll operate inside the lifetime of smallImage (caller must ensure disposal).
                    const int size = 8;
                    if (smallImage.Width < size || smallImage.Height < size)
                    {
                        // fallback: resize if needed
                        using var tmp = smallImage.Clone(ctx => ctx.Resize(new ResizeOptions { Size = new Size(size, size), Mode = ResizeMode.Stretch }));
                        return ComputeAverageHashHybrid_InMemory(tmp);
                    }

                    return ComputeAverageHashHybrid_InMemory(smallImage);
                }

                private static ulong ComputeAverageHashHybrid_InMemory(Image<Rgba32> source)
                {
                    const int size = 8;
                    using var image = source.CloneAs<Rgba32>(); // يضمن تنسيق ثابت آمن وسريع

                    double sum = 0;
                    int total = size * size;
                    bool usedDangerous = false;
                    bool usedProcess = false;

                    // PASS 1: average luminance
                    try
                    {
                        for (int y = 0; y < size; y++)
                        {
                            var span = image.DangerousGetPixelRowMemory(y).Span;
                            for (int x = 0; x < size; x++)
                                sum += 0.299 * span[x].R + 0.587 * span[x].G + 0.114 * span[x].B;
                        }
                        usedDangerous = true;
                    }
                    catch
                    {
                        try
                        {
                            image.ProcessPixelRows(accessor =>
                            {
                                for (int y = 0; y < size; y++)
                                {
                                    var rowSpan = accessor.GetRowSpan(y);
                                    for (int x = 0; x < size; x++)
                                        sum += 0.299 * rowSpan[x].R + 0.587 * rowSpan[x].G + 0.114 * rowSpan[x].B;
                                }
                            });
                            usedProcess = true;
                        }
                        catch
                        {
                            for (int y = 0; y < size; y++)
                                for (int x = 0; x < size; x++)
                                {
                                    var px = image[x, y];
                                    sum += 0.299 * px.R + 0.587 * px.G + 0.114 * px.B;
                                }
                        }
                    }

                    double avg = sum / total;
                    ulong hash = 0;

                    // PASS 2: build hash
                    if (usedDangerous)
                    {
                        int bit = 0;
                        for (int y = 0; y < size; y++)
                        {
                            var span = image.DangerousGetPixelRowMemory(y).Span;
                            for (int x = 0; x < size; x++, bit++)
                            {
                                double lum = 0.299 * span[x].R + 0.587 * span[x].G + 0.114 * span[x].B;
                                if (lum >= avg) hash |= (1UL << bit);
                            }
                        }
                        return hash;
                    }

                    if (usedProcess)
                    {
                        int bit = 0;
                        image.ProcessPixelRows(accessor =>
                        {
                            for (int y = 0; y < size; y++)
                            {
                                var rowSpan = accessor.GetRowSpan(y);
                                for (int x = 0; x < size; x++, bit++)
                                {
                                    double lum = 0.299 * rowSpan[x].R + 0.587 * rowSpan[x].G + 0.114 * rowSpan[x].B;
                                    if (lum >= avg) hash |= (1UL << bit);
                                }
                            }
                        });
                        return hash;
                    }

                    // fallback indexer path
                    {
                        int bit = 0;
                        for (int y = 0; y < size; y++)
                            for (int x = 0; x < size; x++, bit++)
                            {
                                var px = image[x, y];
                                double lum = 0.299 * px.R + 0.587 * px.G + 0.114 * px.B;
                                if (lum >= avg) hash |= (1UL << bit);
                            }
                    }
                    return hash;
                }

                /*private static double Luminance(Rgba32 px) => (0.299 * px.R + 0.587 * px.G + 0.114 * px.B);

                private static ulong ComputeAverageHashHybrid_InMemory2(Image<Rgba32> image)
                {
                    const int size = 8;
                    // PASS 1: compute average luminance
                    double sum = 0;
                    int total = size * size;
                    bool usedDangerous = false;
                    bool usedProcess = false;

                    // Try dangerous fast path
                    try
                    {
                        for (int y = 0; y < size; y++)
                        {
                            // DangerousGetPixelRowMemory may throw if not supported; catch below
                            Memory<Rgba32> memRow = image.DangerousGetPixelRowMemory(y);
                            var span = memRow.Span; // quick access
                            for (int x = 0; x < size; x++)
                            {
                                var px = span[x];
                                sum += Luminance(px);
                            }
                        }
                        usedDangerous = true;
                    }
                    catch
                    {
                        sum = 0;
                        // Try ProcessPixelRows (safe & fast)
                        try
                        {
                            usedProcess = true;
                            image.ProcessPixelRows(accessor =>
                            {
                                for (int y = 0; y < size; y++)
                                {
                                    var rowSpan = accessor.GetRowSpan(y);
                                    for (int x = 0; x < size; x++)
                                    {
                                        var px = rowSpan[x];
                                        sum += Luminance(px);
                                    }
                                }
                            });
                        }
                        catch
                        {
                            // final fallback: indexer
                            sum = 0;
                            for (int y = 0; y < size; y++)
                                for (int x = 0; x < size; x++)
                                {
                                    var px = image[x, y];
                                    sum += Luminance(px);
                                }
                        }
                    }

                    double avg = sum / total;

                    // PASS 2: generate 64-bit hash
                    ulong hash = 0;
                    int bit = 0;

                    if (usedDangerous)
                    {
                        for (int y = 0; y < size; y++)
                        {
                            var span = image.DangerousGetPixelRowMemory(y).Span;
                            for (int x = 0; x < size; x++)
                            {
                                var lum = Luminance(span[x]);
                                if (lum >= avg) hash |= (1UL << bit);
                                bit++;
                            }
                        }
                        return hash;
                    }

                    if (usedProcess)
                    {
                        image.ProcessPixelRows(accessor =>
                        {
                            for (int y = 0; y < size; y++)
                            {
                                var rowSpan = accessor.GetRowSpan(y);
                                for (int x = 0; x < size; x++)
                                {
                                    var lum = Luminance(rowSpan[x]);
                                    if (lum >= avg) hash |= (1UL << bit);
                                    bit++;
                                }
                            }
                        });
                        return hash;
                    }



                    // fallback indexer path
                    for (int y = 0; y < size; y++)
                        for (int x = 0; x < size; x++)
                        {
                            var lum = Luminance(image[x, y]);
                            if (lum >= avg) hash |= (1UL << bit);
                            bit++;
                        }
                    return hash;
                }


                // Average hash (aHash) implementation (64-bit)
                // Steps: convert image to grayscale 8x8, compute mean, set bits.

                private static ulong ComputeAverageHashFromImageDangerous(Image<Rgba32> image)
                {


                    const int size = 8;
                    // أصنع نسخة صغيرة 8x8 (لا بد من clone لأننا لا نريد تغيير الأصل)
                    using var small = image.Clone(ctx => ctx.Resize(new ResizeOptions { Size = new Size(size, size), Mode = ResizeMode.Stretch }));

                    // 1) حساب المتوسط بدون تخصيص مصفوفة:
                    double sum = 0;
                    int total = size * size;

                    // حاول الحصول على صفوف الذاكرة الخطية (الأسرع)
                    bool usedDangerous = false;
                    try
                    {
                        for (int y = 0; y < size; y++)
                        {
                            Memory<Rgba32> rowMem = small.DangerousGetPixelRowMemory(y); // سريع جداً
                            var span = rowMem.Span;
                            for (int x = 0; x < size; x++)
                            {
                                var px = span[x];
                                sum += (0.299 * px.R + 0.587 * px.G + 0.114 * px.B);
                            }
                        }
                        usedDangerous = true;
                    }
                    catch
                    {
                        // لو فشل DangerousGetPixelRowMemory (نادر)، نستخدم الفهرس كـ fallback
                        sum = 0;
                        for (int y = 0; y < size; y++)
                            for (int x = 0; x < size; x++)
                            {
                                var px = small[x, y];
                                sum += (0.299 * px.R + 0.587 * px.G + 0.114 * px.B);
                            }
                    }

                    double avg = sum / total;

                    // 2) توليد الهاش (مرّة ثانية) باستخدام نفس الأسلوب لـ minimal allocations
                    ulong hash = 0;
                    int bit = 0;
                    if (usedDangerous)
                    {
                        for (int y = 0; y < size; y++)
                        {
                            var span = small.DangerousGetPixelRowMemory(y).Span;
                            for (int x = 0; x < size; x++)
                            {
                                var px = span[x];
                                double lum = (0.299 * px.R + 0.587 * px.G + 0.114 * px.B);
                                if (lum >= avg) hash |= (1UL << bit);
                                bit++;
                            }
                        }
                    }
                    else
                    {
                        for (int y = 0; y < size; y++)
                            for (int x = 0; x < size; x++)
                            {
                                var px = small[x, y];
                                double lum = (0.299 * px.R + 0.587 * px.G + 0.114 * px.B);
                                if (lum >= avg) hash |= (1UL << bit);
                                bit++;
                            }
                    }

                    return hash;
                }
                private static ulong ComputeAverageHashFromImageSafe(Image<Rgba32> image)
                {
                    const int size = 8;
                    using var small = image.Clone(ctx => ctx.Resize(new ResizeOptions { Size = new Size(size, size), Mode = ResizeMode.Stretch }));
                    double sum = 0;
                    // ProcessPixelRows ensures safety and gives fast access to spans
                    small.ProcessPixelRows(accessor =>
                    {
                        for (int y = 0; y < size; y++)
                        {
                            var row = accessor.GetRowSpan(y);
                            for (int x = 0; x < size; x++)
                            {
                                var px = row[x];
                                sum += (0.299 * px.R + 0.587 * px.G + 0.114 * px.B);
                            }
                        }
                    });
                    double avg = sum / (size * size);
                    ulong hash = 0; int bit = 0;
                    small.ProcessPixelRows(accessor =>
                    {
                        for (int y = 0; y < size; y++)
                        {
                            var row = accessor.GetRowSpan(y);
                            for (int x = 0; x < size; x++)
                            {
                                var px = row[x];
                                var lum = (0.299 * px.R + 0.587 * px.G + 0.114 * px.B);
                                if (lum >= avg) hash |= (1UL << bit);
                                bit++;
                            }
                        }
                    });
                    return hash;
                }
                private static ulong ComputeAverageHashFromImageOld(Image<Rgba32> image)
                {
                    try
                    {

                        // Resize to 8x8 grayscale
                        using var small = image.Clone(ctx => ctx.Resize(new ResizeOptions { Size = new Size(8, 8), Mode = ResizeMode.Stretch }));
                        double[] gray = new double[64];
                        int i = 0;
                        for (int y = 0; y < 8; y++)
                        {
                            for (int x = 0; x < 8; x++)
                            {
                                var px = small[x, y];
                                // luminance approx
                                double lum = 0.299 * px.R + 0.587 * px.G + 0.114 * px.B;
                                gray[i++] = lum;
                            }
                        }
                        double avg = gray.Average();
                        ulong hash = 0;
                        for (int k = 0; k < 64; k++)
                        {
                            if (gray[k] >= avg) hash |= (1UL << k);
                        }
                        return hash;
                    }
                    catch
                    {
                        return 0UL;
                    }
                }*/

                public static async Task<VideoMetadata> AnalyzeVideoFileWithFfprobe(string filePath, int ffprobeTimeoutMs = 15000)
                {
                    if (!File.Exists(filePath))
                    {
                        throw new FileNotFoundException(filePath);
                    }


                    // call ffprobe:
                    // ffprobe -v quiet -print_format json -show_format -show_streams "filePath"

                    var startInfo = new ProcessStartInfo { FileName = "ffprobe", RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false, CreateNoWindow = true };
                    startInfo.ArgumentList.Add("-v"); startInfo.ArgumentList.Add("quiet");
                    startInfo.ArgumentList.Add("-print_format"); startInfo.ArgumentList.Add("json");
                    startInfo.ArgumentList.Add("-show_format"); startInfo.ArgumentList.Add("-show_streams");
                    startInfo.ArgumentList.Add(filePath);



                    using var proc = Process.Start(startInfo) ?? throw new InvalidOperationException("ffprobe not found or could not start.");
                    var outputTask = proc.StandardOutput.ReadToEndAsync();
                    var errTask = proc.StandardError.ReadToEndAsync();

                    if (await Task.WhenAny(outputTask, Task.Delay(ffprobeTimeoutMs)) != outputTask)
                    {
                        try { proc.Kill(); } catch { }
                        throw new TimeoutException("ffprobe timed out");
                    }
                    string json = await outputTask;

                    string err = await errTask;
                    // continue parsing...


                    // parse JSON (structure: { streams: [...], format: { ... } })
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    var res = new VideoMetadata { RawJson = json };

                    // format section
                    if (root.TryGetProperty("format", out var fmt))
                    {
                        if (fmt.TryGetProperty("duration", out var durationEl) && durationEl.ValueKind == JsonValueKind.String)
                        {
                            if (double.TryParse(durationEl.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var dur))
                                res = res with { DurationSeconds = dur };
                        }
                        if (fmt.TryGetProperty("format_name", out var fName)) res = res with { FormatName = fName.GetString() };
                        if (fmt.TryGetProperty("bit_rate", out var br) && br.ValueKind == JsonValueKind.String && long.TryParse(br.GetString(), out var brv))
                            res = res with { Bitrate = brv };
                    }

                    // streams: find first video stream
                    if (root.TryGetProperty("streams", out var streams) && streams.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var s in streams.EnumerateArray())
                        {
                            if (s.TryGetProperty("codec_type", out var ct) && ct.GetString() == "video")
                            {
                                string? codec = s.GetProperty("codec_name").GetString();
                                int? w = s.TryGetProperty("width", out var wEl) && wEl.TryGetInt32(out var wi) ? wi : (int?)null;
                                int? h = s.TryGetProperty("height", out var hEl) && hEl.TryGetInt32(out var hi) ? hi : (int?)null;

                                res = res with { Codec = codec, Width = w, Height = h };
                                break;
                            }
                        }
                    }

                    return res;


                }

                private static class clsMimeType
                {
                    // bounded cache helpers (approximate FIFO eviction)
                    private const int MIME_CACHE_CAPACITY = 5000;
                    private static readonly ConcurrentQueue<string> _mimeCacheQueue = new();

                    // _cache already exists (ConcurrentDictionary<string,string>)
                    // if you prefer a true LRU, replace this with MemoryCache or a dedicated LRU structure.

                    /// <summary>
                    /// Robust mime detector:
                    /// - fast magic bytes checks
                    /// - ftyp/mp4/heic detection by scanning first bytes
                    /// - inspector fallback (_inspector.DetermineFileFormat)
                    /// - extension fallback (lowest priority)
                    /// - optional "highAccuracy" extra validation of signatures
                    /// - bounded cache keyed by hash of first N bytes + length
                    /// </summary>
                    public static string? DetectMimeTypeEnhanced(byte[] fileBytes, string? fileNameHint = null, bool highAccuracy = false)
                    {

                        if (fileBytes == null || fileBytes.Length == 0)
                        {
                            return "application/octet-stream";
                        }

                        if (fileBytes.LongLength > MAX_FILE_BYTES)
                        {
                            throw new ArgumentException("file too large");
                        }

                        // --- 1) cache key: SHA256 of the first N bytes + file length + accuracy flag
                        int sampleLen = highAccuracy ? 32 : 16;
                        sampleLen = Math.Min(sampleLen, fileBytes.Length);
                        byte[] prefixHash = SHA256.HashData(fileBytes.AsSpan(0, sampleLen));
                        // shorten to 12 bytes to keep key small
                        string shortHash = Convert.ToBase64String(prefixHash, 0, Math.Min(12, prefixHash.Length));
                        string cacheKey = (highAccuracy ? "H_" : "L_") + shortHash + "_" + fileBytes.Length;

                        if (_cache.TryGetValue(cacheKey, out var cached)) return cached;

                        // --- 2) fast magic checks (very cheap)
                        var fast = QuickMagicCheck(fileBytes);
                        string? result = fast;

                        // --- 3) try inspector if not found by fast path
                        if (string.IsNullOrEmpty(result))
                        {
                            try
                            {
                                using var ms = new MemoryStream(fileBytes, writable: false);
                                var format = _inspector?.DetermineFileFormat(ms);
                                // format may be null; many inspector libs expose MediaType or MimeType property
                                result = format?.MediaType;
                            }
                            catch
                            {
                                // inspector failed — ignore, we'll try other fallbacks
                                result = null;
                            }
                        }

                        // --- 4) heuristic ftyp scan (more flexible detection for MP4/HEIC/ISO-based)
                        if (string.IsNullOrEmpty(result))
                            result = FtypScan(fileBytes);

                        // --- 5) extension fallback (least trusted)
                        if (string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(fileNameHint))
                            result = ExtensionLookup(fileNameHint);

                        // default
                        if (string.IsNullOrEmpty(result))
                            result = "application/octet-stream";

                        // --- 6) optional strict validation
                        if (highAccuracy)
                            result = ValidateMimeType(fileBytes, result);

                        // --- 7) add to bounded cache
                        AddToCacheBounded(cacheKey, result);

                        return result;
                    }

                    // ---------- helpers ----------

                    private static void AddToCacheBounded(string key, string value)
                    {
                        if (string.IsNullOrEmpty(key)) return;
                        if (_cache.TryAdd(key, value))
                        {
                            _mimeCacheQueue.Enqueue(key);
                            // evict if over capacity (approximate FIFO)
                            while (_mimeCacheQueue.Count > MIME_CACHE_CAPACITY)
                            {
                                if (_mimeCacheQueue.TryDequeue(out var old))
                                {
                                    _cache.TryRemove(old, out _);
                                }
                                else break;
                            }
                        }
                    }

                    private static string? QuickMagicCheck(byte[] b)
                    {
                        // requires few bytes, very fast
                        if (b.Length >= 2 && b[0] == 0xFF && b[1] == 0xD8) return "image/jpeg";
                        if (b.Length >= 8 && b[0] == 0x89 && b[1] == 0x50 && b[2] == 0x4E && b[3] == 0x47) return "image/png";
                        if (b.Length >= 3 && b[0] == 0x47 && b[1] == 0x49 && b[2] == 0x46) return "image/gif";
                        if (b.Length >= 12 && b[0] == 0x52 && b[1] == 0x49 && b[2] == 0x46 && b[3] == 0x46
                            && b[8] == 0x57 && b[9] == 0x45 && b[10] == 0x42 && b[11] == 0x50) return "image/webp";
                        if (b.Length >= 4 && b[0] == (byte)'P' && b[1] == (byte)'K') return "application/zip"; // office, docx, xlsx, pptx, zip
                        if (b.Length >= 4 && b[0] == (byte)'%' && b[1] == (byte)'P' && b[2] == (byte)'D' && b[3] == (byte)'F') return "application/pdf";
                        if (b.Length >= 2 && b[0] == (byte)'B' && b[1] == (byte)'M') return "image/bmp";
                        // TIFF (II*, MM*)
                        if (b.Length >= 4 && ((b[0] == 0x49 && b[1] == 0x49 && b[2] == 0x2A && b[3] == 0x00) ||
                                              (b[0] == 0x4D && b[1] == 0x4D && b[2] == 0x00 && b[3] == 0x2A))) return "image/tiff";
                        // Windows executable
                        if (b.Length >= 2 && b[0] == 0x4D && b[1] == 0x5A) return "application/x-msdownload";
                        // audio/video simple hints (not exhaustive)
                        if (b.Length >= 12 && LooksLikeMp4(b)) return "video/mp4";
                        return null;
                    }

                    // scan first N bytes for 'ftyp' and common brands (mp4/heic/heif)
                    private static string? FtypScan(byte[] b)
                    {

                        if (b.Length >= 4)
                        {
                            // فحص إذا كان الملف يبدأ بـ XML عادي أو UTF-8 BOM + XML
                            if (b[0] == '<' || (b[0] == 0xEF && b[1] == 0xBB && b[2] == 0xBF && b[3] == '<'))
                            {
                                var head = Encoding.UTF8.GetString(b, 0, Math.Min(512, b.Length)).ToLowerInvariant();
                                if (head.Contains("<svg")) return "image/svg+xml";
                            }
                        }

                        int scan = Math.Min(256, b.Length);
                        for (int i = 0; i + 3 < scan; i++)
                        {
                            if (b[i] == (byte)'f' && b[i + 1] == (byte)'t' && b[i + 2] == (byte)'y' && b[i + 3] == (byte)'p')
                            {
                                // brand is typically at i+4..i+7 or i+8..i+11 depending on file
                                int brandPos = i + 4;
                                if (brandPos + 3 < scan)
                                {
                                    string brand = Encoding.ASCII.GetString(b, brandPos, Math.Min(4, scan - brandPos));
                                    brand = brand.Trim('\0').ToLowerInvariant();
                                    // common mp4/isobmff brands
                                    var mp4Brands = new[] { "isom", "iso2", "mp41", "mp42", "avc1", "dash", "iso6", "mif1", "msf1" };
                                    var heifBrands = new[] { "heic", "heix", "hevc", "hevx", "mif1", "msf1" };

                                    if (mp4Brands.Contains(brand)) return "video/mp4";
                                    if (heifBrands.Contains(brand)) return "image/heif"; // heic/heif family
                                                                                         // quicktime brand
                                    if (brand == "qt  " || brand == "pnot") return "video/quicktime";
                                    // fallback to generic mp4 if 'ftyp' present
                                    return "video/mp4";
                                }
                            }

                        }
                        return null;

                    }

                    private static bool LooksLikeMp4(byte[] b)
                    {
                        // check offset 4..7 for 'ftyp' (most common) or scan first 256 bytes
                        if (b.Length >= 12 && b[4] == (byte)'f' && b[5] == (byte)'t' && b[6] == (byte)'y' && b[7] == (byte)'p') return true;
                        // fallback scanning:
                        return FtypScan(b) != null;
                    }

                    private static string? ExtensionLookup(string fileNameHint)
                    {
                        try
                        {
                            var ext = System.IO.Path.GetExtension(fileNameHint)?.ToLowerInvariant() ?? string.Empty;
                            return ext switch
                            {
                                ".jpg" or ".jpeg" => "image/jpeg",
                                ".png" => "image/png",
                                ".gif" => "image/gif",
                                ".webp" => "image/webp",
                                ".bmp" => "image/bmp",
                                ".tif" or ".tiff" => "image/tiff",
                                ".heic" or ".heif" => "image/heif",
                                ".mp4" => "video/mp4",
                                ".mov" => "video/quicktime",
                                ".mkv" => "video/x-matroska",
                                ".avi" => "video/x-msvideo",
                                ".mp3" => "audio/mpeg",
                                ".wav" => "audio/wave",
                                ".pdf" => "application/pdf",
                                ".zip" => "application/zip",
                                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                                _ => null
                            };
                        }
                        catch
                        {
                            return null;
                        }
                    }

                    /// <summary>
                    /// Validates some critical MIME types by re-checking magic bytes. Returns the original mime if ok; otherwise application/octet-stream
                    /// (you can extend this with more signatures as needed).
                    /// </summary>
                    private static string ValidateMimeType(byte[] bytes, string mime)
                    {
                        if (string.IsNullOrEmpty(mime)) return "application/octet-stream";

                        mime = mime.ToLowerInvariant();

                        try
                        {
                            if (mime == "image/jpeg" && (bytes.Length < 2 || bytes[0] != 0xFF || bytes[1] != 0xD8))
                                return "application/octet-stream";

                            if (mime == "image/png" && (bytes.Length < 8 || bytes[0] != 0x89 || bytes[1] != 0x50 || bytes[2] != 0x4E || bytes[3] != 0x47))
                                return "application/octet-stream";

                            if (mime == "image/gif" && (bytes.Length < 3 || bytes[0] != 0x47 || bytes[1] != 0x49 || bytes[2] != 0x46))
                                return "application/octet-stream";

                            if (mime == "image/webp" && (bytes.Length < 12 || !(bytes[0] == 0x52 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x46)))
                                return "application/octet-stream";

                            if (mime.StartsWith("video/") && !string.IsNullOrEmpty(mime))
                            {
                                // ensure MP4-ish signature if mp4
                                if (mime == "video/mp4" && !LooksLikeMp4(bytes))
                                    return "application/octet-stream";
                            }

                            if (mime == "application/pdf" && (bytes.Length < 4 || bytes[0] != (byte)'%' || bytes[1] != (byte)'P' || bytes[2] != (byte)'D' || bytes[3] != (byte)'F'))
                                return "application/octet-stream";
                        }
                        catch
                        {
                            return "application/octet-stream";
                        }

                        return mime;
                    }
                }

            }


            /////////////////////////////////////////////////////////////////

            //[httpGet]

            public static async Task<(ClsApplication?, bool)> FindUser(string? email, CancellationToken ct = default)
            {

                if (string.IsNullOrEmpty(email)) { return (null, false); }


                string? SelectQuery_findMinRecordsByEmail = SQLQueryList("SelectQuery_findMinRecordsByEmail");


                try
                {

                    await using DbConnection connection = CreateConnection();
                    await connection.OpenAsync(ct).ConfigureAwait(false);
                    await using var command = connection.CreateCommand();

                    command.CommandText = SelectQuery_findMinRecordsByEmail;
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = CRUD_DefaultDbTimeout;

                    char prefix = SqlParameterPrefix(connection);

                    DbParameter AddParam(string name, object? value, DbType type)
                    {
                        var parameter1 = command.CreateParameter();
                        parameter1.ParameterName = prefix + name;
                        parameter1.Value = value ?? DBNull.Value;
                        parameter1.DbType = type;
                        return parameter1;
                    }

                    command.Parameters.Add(AddParam("Email", email.Trim(), DbType.String));
                    command.Parameters.Add(AddParam("UserDeletedFlag", UserDeletedFlag, DbType.Int64));



                    DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, ct).ConfigureAwait(false);
                    ClsApplication? Obj = null;
                    if (!reader.HasRows)
                    {

                        Obj = CreateApplicationFromReader(reader);
                        PrintClassInfo(Obj);
                        return (Obj, false);
                    }

                    return (Obj, true);


                }
                catch (DbException ex)
                {
                    Console.WriteLine(ex);
                    Console.WriteLine("Database unreachable. Please try again later.");
                    return (null, false);
                }

            }

            public static async Task<(ClsApplication?, bool)> FindUserAllData(string? email, CancellationToken ct = default)
            {

                if (string.IsNullOrEmpty(email)) { throw new ArgumentException("The email is invalid!"); }


                string? SelectQuery_findMaxRecordsByEmail = SQLQueryList("SelectQuery_findMaxRecordsByEmail");


                try
                {

                    await using DbConnection connection = CreateConnection();
                    await connection.OpenAsync(ct).ConfigureAwait(false);
                    await using var command = connection.CreateCommand();

                    command.CommandText = SelectQuery_findMaxRecordsByEmail;
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = CRUD_DefaultDbTimeout;

                    char prefix = SqlParameterPrefix(connection);

                    DbParameter AddParam(string name, object? value, DbType type)
                    {
                        var parameter1 = command.CreateParameter();
                        parameter1.ParameterName = prefix + name;
                        parameter1.Value = value ?? DBNull.Value;
                        parameter1.DbType = type;
                        return parameter1;
                    }

                    command.Parameters.Add(AddParam("Email", email.Trim(), DbType.String));
                    command.Parameters.Add(AddParam("UserDeletedFlag", UserDeletedFlag, DbType.Int64));
                    command.Parameters.Add(AddParam("AddressPrimaryFlag", AddressPrimaryFlag, DbType.Int64));
                    command.Parameters.Add(AddParam("PhonePrimaryFlag", PhonePrimaryFlag, DbType.Int64));
                    command.Parameters.Add(AddParam("EmailPrimaryFlag", EmailPrimaryFlag, DbType.Int64));
                    command.Parameters.Add(AddParam("PasswordPrimaryFlag", PasswordPrimaryFlag, DbType.Int64));


                    DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, ct).ConfigureAwait(false);

                    if (!reader.HasRows) { return (null, false); }
                    ClsApplication? Obj = null;
                    if (await reader.ReadAsync(ct).ConfigureAwait(false))
                    {

                        Obj = CreateApplicationFromReader2(reader);
                        PrintClassInfo2(Obj);

                        return (Obj, true);
                    }

                    return (Obj, true);


                }
                catch (DbException ex)
                {
                    Console.WriteLine(ex);
                    Console.WriteLine("Database unreachable. Please try again later.");
                    return (null, false);
                }

            }

            public static async Task<(ClsApplication?, bool)> FindUser(long? ID, CancellationToken ct = default)
            {

                if (ID < 1 || ID > 9000000000000000 || ID == null) { throw new ArgumentOutOfRangeException("The ID is out if range!"); }


                string? SelectQuery_findMinRecordsByID = SQLQueryList("SelectQuery_findMinRecordsByID");


                try
                {

                    await using DbConnection connection = CreateConnection();
                    await connection.OpenAsync(ct).ConfigureAwait(false);
                    await using var command = connection.CreateCommand();

                    command.CommandText = SelectQuery_findMinRecordsByID;
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = CRUD_DefaultDbTimeout;

                    char prefix = SqlParameterPrefix(connection);

                    DbParameter AddParam(string name, object? value, DbType type)
                    {
                        var parameter1 = command.CreateParameter();
                        parameter1.ParameterName = prefix + name;
                        parameter1.Value = value ?? DBNull.Value;
                        parameter1.DbType = type;
                        return parameter1;
                    }

                    command.Parameters.Add(AddParam("UserID", ID, DbType.Int64));
                    command.Parameters.Add(AddParam("UserDeletedFlag", UserDeletedFlag, DbType.Int64));

                    DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, ct).ConfigureAwait(false);
                    if (!reader.HasRows) { return (null, false); }
                    ClsApplication? Obj = null;
                    if (await reader.ReadAsync(ct).ConfigureAwait(false))
                    {

                        Obj = CreateApplicationFromReader2(reader);
                        PrintClassInfo2(Obj);

                        return (Obj, true);
                    }

                    return (Obj, true);

                }
                catch (DbException ex)
                {
                    Console.WriteLine(ex);
                    Console.WriteLine("Database unreachable. Please try again later.");
                    return (null, false);
                }

            }

            public static async Task<(ClsApplication?, bool)> FindUserAllData(long? ID, CancellationToken ct = default)
            {

                if (ID < 1 || ID > 9000000000000000 || ID == null) { throw new ArgumentOutOfRangeException("The ID is out if range!"); }

                string? SelectQuery_findMaxRecordsByID = SQLQueryList("SelectQuery_findMaxRecordsByID");

                try
                {

                    await using DbConnection connection = CreateConnection();
                    await connection.OpenAsync(ct).ConfigureAwait(false);
                    await using var command = connection.CreateCommand();

                    command.CommandText = SelectQuery_findMaxRecordsByID;
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = CRUD_DefaultDbTimeout;

                    char prefix = SqlParameterPrefix(connection);

                    DbParameter AddParam(string name, object? value, DbType type)
                    {
                        var parameter1 = command.CreateParameter();
                        parameter1.ParameterName = prefix + name;
                        parameter1.Value = value ?? DBNull.Value;
                        parameter1.DbType = type;
                        return parameter1;
                    }

                    command.Parameters.Add(AddParam("UserID", ID, DbType.Int64));
                    command.Parameters.Add(AddParam("UserDeletedFlag", UserDeletedFlag, DbType.Int64));
                    command.Parameters.Add(AddParam("AddressPrimaryFlag", AddressPrimaryFlag, DbType.Int64));
                    command.Parameters.Add(AddParam("PhonePrimaryFlag", PhonePrimaryFlag, DbType.Int64));
                    command.Parameters.Add(AddParam("EmailPrimaryFlag", EmailPrimaryFlag, DbType.Int64));
                    command.Parameters.Add(AddParam("PasswordPrimaryFlag", PasswordPrimaryFlag, DbType.Int64));

                    await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, ct).ConfigureAwait(false);

                    if (!reader.HasRows) { return (null, false); }
                    ClsApplication? Obj = null;
                    if (await reader.ReadAsync(ct).ConfigureAwait(false))
                    {

                        Obj = CreateApplicationFromReader3(reader);
                        string g = JsonSerializer.Serialize(Obj);
                        Console.WriteLine(g);
                        //PrintClassInfo3(Obj);

                        return (Obj, true);
                    }

                    return (Obj, true);

                }
                catch (DbException ex)
                {
                    Console.WriteLine(ex);
                    Console.WriteLine("Database unreachable. Please try again later.");
                    return (null, false);
                }


            }

            public static async Task<(ClsApplication?, bool)> FindUserAllDataBigMax(long? ID, CancellationToken ct = default)
            {

                if (ID < 1 || ID > 9000000000000000 || ID == null) { throw new ArgumentOutOfRangeException("The ID is out if range!"); }

                string? SelectQuery_findBigMaxRecordsByID = SQLQueryList("SelectQuery_findBigMaxRecordsByID");

                try
                {

                    await using DbConnection connection = CreateConnection();
                    await connection.OpenAsync(ct).ConfigureAwait(false);
                    await using var command = connection.CreateCommand();

                    command.CommandText = SelectQuery_findBigMaxRecordsByID;
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = CRUD_DefaultDbTimeout;

                    char prefix = SqlParameterPrefix(connection);

                    DbParameter AddParam(string name, object? value, DbType type)
                    {
                        var parameter1 = command.CreateParameter();
                        parameter1.ParameterName = prefix + name;
                        parameter1.Value = value ?? DBNull.Value;
                        parameter1.DbType = type;
                        return parameter1;
                    }

                    command.Parameters.Add(AddParam("UserID", ID, DbType.Int64));
                    command.Parameters.Add(AddParam("UserDeletedFlag", UserDeletedFlag, DbType.Int64));
                    command.Parameters.Add(AddParam("AddressPrimaryFlag", AddressPrimaryFlag, DbType.Int64));
                    command.Parameters.Add(AddParam("PhonePrimaryFlag", PhonePrimaryFlag, DbType.Int64));
                    command.Parameters.Add(AddParam("EmailPrimaryFlag", EmailPrimaryFlag, DbType.Int64));
                    command.Parameters.Add(AddParam("PasswordPrimaryFlag", PasswordPrimaryFlag, DbType.Int64));

                    await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess | CommandBehavior.SingleRow, ct).ConfigureAwait(false);

                    if (!reader.HasRows) { return (null, false); }
                    ClsApplication? Obj = null;
                    if (await reader.ReadAsync(ct).ConfigureAwait(false))
                    {

                        /*const int bufferSize = 81920;
                        byte[] buffer = new byte[bufferSize];
                        long offset = 0;
                        long readBytes = 0;

                        using Stream stream = new MemoryStream();

                        while ((readBytes = reader.GetBytes(reader.GetOrdinal(ByteFiles_Thumbnail_MapName), offset, buffer, 0, bufferSize))>0)
                        {
                            await stream.WriteAsync(buffer, 0, (int)readBytes);
                            offset = offset + readBytes;
                        }

                        FileStream fileStream = File.Create("C:\\Users\\abdal\\mangadesk\\PROGRAMMING\\The_Damn.mp4");
                        stream.Position = 0;
                        stream.CopyTo(fileStream);*/

                        Obj = CreateApplicationFromReader3(reader);
                        //PrintClassInfo3(Obj);
                        /*if(Obj.ByteFileThumbnail != null)
                        {
                            Console.WriteLine("The Code is: " + Obj.ByteFileThumbnail.Length);

                            Console.WriteLine("The Code is: " + Encoding.UTF8.GetString(Obj.ByteFileThumbnail, 1, 1000));
                            Console.WriteLine("The Code is: " + BitConverter.ToString(Obj.ByteFileThumbnail, 1, 1000));
                            Console.WriteLine("The Code is: " + Convert.ToBase64String(Obj.ByteFileThumbnail, 1, 1000));

                        }*/
                        return (Obj, true);
                    }

                    await using var byteReader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess, ct).ConfigureAwait(false);

                    if (!await byteReader.ReadAsync(ct).ConfigureAwait(false))
                        return (Obj, true); // الصف غير موجود


                    return (Obj, true);

                }
                catch (DbException ex)
                {
                    Console.WriteLine(ex);
                    Console.WriteLine("Database unreachable. Please try again later.");
                    return (null, false);
                }


            }

            public static async Task<byte[]> LoadFileBytesAsync(
    DbConnection connection,
    string tableName,
    string columnName,
    string idColumnName,
    object idValue,
    CancellationToken ct = default)
            {
                // التحقق من الاتصال
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync(ct).ConfigureAwait(false);

                await using var command = connection.CreateCommand();
                command.CommandText = $"SELECT {columnName} FROM {tableName} WHERE {idColumnName} = @id";
                command.CommandType = CommandType.Text;
                command.CommandTimeout = 120;

                // إضافة المعامل
                var param = command.CreateParameter();
                param.ParameterName = "@id";
                param.Value = idValue ?? DBNull.Value;
                command.Parameters.Add(param);

                // تنفيذ القراءة
                await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess, ct)
                    .ConfigureAwait(false);

                if (!await reader.ReadAsync(ct).ConfigureAwait(false))
                    return Array.Empty<byte>(); // الصف غير موجود

                const int bufferSize = 81920; // 80KB buffer
                long offset = 0;
                byte[] buffer = new byte[bufferSize];
                using var memoryStream = new MemoryStream();

                int ordinal = reader.GetOrdinal(columnName);

                long bytesRead;
                while ((bytesRead = reader.GetBytes(ordinal, offset, buffer, 0, buffer.Length)) > 0)
                {
                    await memoryStream.WriteAsync(buffer.AsMemory(0, (int)bytesRead), ct).ConfigureAwait(false);
                    offset += bytesRead;
                }
                FileStream fileStream = File.Create("C:\\Users\\abdal\\mangadesk\\PROGRAMMING\\The_Damn.mp4");
                memoryStream.Position = 0;
                memoryStream.CopyTo(fileStream);
                return memoryStream.ToArray(); // هنا الملف بالكامل كـ byte[]
            }


            public static async Task<byte> UpdateUser(long? UserID = -1, string FirstName = "", string LastName = "", char Gender = '0', int BirthDateYear = -1, byte BirthDateMonth = 0, byte BirthDateDay = 0,
            long UserStatus = -1, string? UpdatedBy = null)
            {

                if (UserID < 1 || UserID == null) { return 0; }

                DateTime BirthDate = new DateTime();

                try
                {
                    using DbConnection connection = new SqlConnection(SQLConnectionString);

                    await connection.OpenAsync().ConfigureAwait(false);

                    var updates = new List<string>();
                    var parameters = new DynamicParameters();


                    if (FirstName != "")
                    {
                        updates.Add($"{FirstName_DBName} = @firstName");
                        parameters.Add("@firstName", FirstName, DbType.String);
                    }

                    if (LastName != "")
                    {
                        updates.Add($"{LastName_DBName} = @lastName");
                        parameters.Add("@lastName", LastName, DbType.String);
                    }

                    if (Gender != '0')
                    {
                        updates.Add($"{Gender_DBName} = @gender");
                        parameters.Add("@gender", Gender, DbType.StringFixedLength);
                    }

                    if ((BirthDateYear != -1) && (BirthDateMonth != 0) && (BirthDateDay != 0))
                    {
                        BirthDate = new DateTime(BirthDateYear, BirthDateMonth, BirthDateDay);
                        updates.Add($"{BirthDate_DBName} = @birthDate");
                        parameters.Add("@birthDate", BirthDate, DbType.DateTime);
                    }

                    if (UserStatus != -1 && UserStatus >= 0)
                    {
                        updates.Add($"{UserStatus_DBName} = @userStatus");
                        parameters.Add("@userStatus", UserStatus, DbType.Int64);
                    }

                    if (UpdatedBy != null)
                    {
                        updates.Add($"{UserUpdatedBy_DBName} = @userUpdatedBy");
                        parameters.Add("@userUpdatedBy", UpdatedBy, DbType.String);
                    }


                    if (updates.Count <= 0)
                    { return 0; }

                    DateTimeOffset UserUpdatedAt = DateTimeOffset.Now;
                    updates.Add($"{UserUpdatedAt_DBName} = @userLastUpdate");
                    parameters.Add("@userLastUpdate", UserUpdatedAt, DbType.DateTimeOffset);

                    string UpdateQuery_updateUserData = @$"UPDATE {Users_Table_DBName} SET {string.Join(",", updates)} WHERE {UserID_DBName} = @Id";
                    parameters.Add("@Id", UserID);

                    int rows = await connection.ExecuteAsync(UpdateQuery_updateUserData, parameters).ConfigureAwait(false);

                    if (rows > 0)
                    {
                        (ClsApplication? app, bool result) = await ClsApplication.FindUser(UserID).ConfigureAwait(false);
                        return 1;
                    }
                    else
                    {
                        return 2;
                    }

                }
                catch (SqlException ex)
                {
                    Console.WriteLine(ex);
                    Console.WriteLine($"Database unreachable.. Please try again later : {ex}");
                    return 0;
                }

            }

            public static async Task<byte> UpdateEmail(long? EmailID = -1, long EmailStatus = -1)
            {

                if (EmailID < 1 || EmailID == null) { return 0; }

                try
                {
                    using IDbConnection connection = new SqlConnection(SQLConnectionString);

                    await ((DbConnection)connection).OpenAsync().ConfigureAwait(false);

                    var updates = new List<string>();
                    var parameters = new DynamicParameters();

                    if (EmailStatus != -1 && EmailStatus >= 0)
                    {
                        updates.Add($"{EmailStatus_DBName} = @emailStatus");
                        parameters.Add("@emailStatus", EmailStatus, DbType.Int64);
                    }

                    if (updates.Count <= 0)
                    { return 0; }

                    DateTimeOffset EmailUpdatedAt = DateTimeOffset.Now;

                    updates.Add($"{EmailUpdatedAt_DBName} = @emailUpdatedAt");
                    parameters.Add("@emailUpdatedAt", EmailUpdatedAt, DbType.DateTimeOffset);

                    string UpdateQuery_updateEmailData = @$"UPDATE {Emails_Table_DBName} SET {string.Join(",", updates)} WHERE {EmailID_DBName} = @emailID";
                    parameters.Add("@emailID", EmailID);

                    int rows = await connection.ExecuteAsync(UpdateQuery_updateEmailData, parameters).ConfigureAwait(false);

                    if (rows > 0)
                    {
                        (ClsApplication? app, bool result) = await ClsApplication.FindUserAllData(EmailID).ConfigureAwait(false);
                        return 1;
                    }
                    else
                    {
                        return 2;
                    }

                }
                catch (SqlException ex)
                {
                    Console.WriteLine(ex);
                    Console.WriteLine("Database unreachable. Please try again later.");
                    return 0;
                }

            }

            public static async Task<byte> UpdatePassword(long? PasswordID = -1, long PasswordStatus = -1)
            {

                if (PasswordID < 1 || PasswordID == null) { return 0; }

                try
                {
                    using IDbConnection connection = new SqlConnection(SQLConnectionString);

                    await ((DbConnection)connection).OpenAsync().ConfigureAwait(false);

                    var updates = new List<string>();
                    var parameters = new DynamicParameters();

                    if (PasswordStatus != -1 && PasswordStatus >= 0)
                    {
                        updates.Add($"{PasswordStatus_DBName} = @passwordStatus");
                        parameters.Add("@passwordStatus", PasswordStatus, DbType.Int64);
                    }

                    if (updates.Count <= 0)
                    { return 0; }

                    DateTimeOffset PasswordUpdatedAt = DateTimeOffset.Now;

                    updates.Add($"{PasswordUpdatedAt_DBName} = @passwordUpdatedAt");
                    parameters.Add("@passwordUpdatedAt", PasswordUpdatedAt, DbType.DateTimeOffset);

                    string UpdateQuery_updatePasswordData = @$"UPDATE {Passwords_Table_DBName} SET {string.Join(",", updates)} WHERE {PasswordID_DBName} = @passwordID";
                    parameters.Add("@passwordID", PasswordID);

                    int rows = await connection.ExecuteAsync(UpdateQuery_updatePasswordData, parameters).ConfigureAwait(false);

                    if (rows > 0)
                    {
                        (ClsApplication? app, bool result) = await ClsApplication.FindUserAllData(PasswordID).ConfigureAwait(false);
                        return 1;
                    }
                    else
                    {
                        return 2;
                    }

                }
                catch (SqlException ex)
                {
                    Console.WriteLine(ex);
                    Console.WriteLine("Database unreachable. Please try again later.");
                    return 0;
                }

            }

            public static async Task<byte> UpdatePhone(long? PhoneID = -1, long PhoneStatus = -1)
            {

                if (PhoneID < 1 || PhoneID == null) { return 0; }

                try
                {
                    using IDbConnection connection = new SqlConnection(SQLConnectionString);

                    await ((DbConnection)connection).OpenAsync().ConfigureAwait(false);

                    var updates = new List<string>();
                    var parameters = new DynamicParameters();

                    if (PhoneStatus != -1 && PhoneStatus >= 0)
                    {
                        updates.Add($"{PhoneStatus_DBName} = @PhoneStatus");
                        parameters.Add("@PhoneStatus", PhoneStatus, DbType.Int64);
                    }

                    if (updates.Count <= 0)
                    { return 0; }

                    DateTimeOffset PhoneUpdatedAt = DateTimeOffset.Now;

                    updates.Add($"{PhoneUpdatedAt_DBName} = @phoneUpdatedAt");
                    parameters.Add("@phoneUpdatedAt", PhoneUpdatedAt, DbType.DateTimeOffset);

                    string UpdateQuery_updatePhoneData = @$"UPDATE {Phones_Table_DBName} SET {string.Join(",", updates)} WHERE {PhoneID_DBName} = @phoneID";
                    parameters.Add("@phoneID", PhoneID);

                    int rows = await connection.ExecuteAsync(UpdateQuery_updatePhoneData, parameters).ConfigureAwait(false);

                    if (rows > 0)
                    {
                        (ClsApplication? app, bool result) = await ClsApplication.FindUserAllData(PhoneID).ConfigureAwait(false);
                        return 1;
                    }
                    else
                    {
                        return 2;
                    }

                }
                catch (SqlException ex)
                {
                    Console.WriteLine(ex);
                    Console.WriteLine("Database unreachable. Please try again later.");
                    return 0;
                }

            }

            public string h()
            {
                return "";
            }
            enum enString { one = 1, two = 2 }

            public static async Task<byte> UpdateUserData(long UserID = -1, string? FirstName = "", string? LastName = "", char gender = '0', int birthDateYear = -1, byte birthDateMonth = 0, byte birthDateDay = 0)
            {

                byte update_status = 0;
                string? answer;

                if (UserID == -1)
                {

                    do
                    {
                        do
                        {

                            Console.WriteLine(@$"WRITE THE ""USER ID"" : ");
                            UserID = GetPositiveLongInteger();

                        } while (UserID < 1 || UserID > 1800000000000);

                    } while (ClsApplication.FindUser(UserID) == null);

                    do
                    {

                        Console.WriteLine(@$"DO YOU WANT TO UPDATE THE ""FIRST NAME"" ?(YES/NO).. ");
                        answer = Console.ReadLine()?.Trim().ToUpper();

                    } while ((answer != "YES") && (answer != "NO"));
                    if (answer == "YES")
                    {
                        do
                        {
                            Console.WriteLine(@$"WRITE THE ""FIRST NAME"" : ");
                            FirstName = Console.ReadLine()?.Trim().ToUpper();

                        } while (FirstName == "");

                    }

                    do
                    {
                        Console.WriteLine(@$"DO YOU WANT TO UPDATE THE ""LAST NAME"" ?(YES/NO).. ");
                        answer = Console.ReadLine()?.Trim().ToUpper();

                    } while (answer != "YES" && answer != "NO");
                    if (answer == "YES")
                    {

                        do
                        {
                            Console.WriteLine(@$"WRITE THE ""LAST NAME"" : ");
                            LastName = Console.ReadLine()?.Trim().ToUpper();

                        } while (LastName == "");
                    }

                    do
                    {
                        Console.WriteLine(@$"DO YOU WANT TO UPDATE THE ""GENDER"" ?(YES/NO).. ");
                        answer = Console.ReadLine()?.Trim().ToUpper();
                    } while (answer != "YES" && answer != "NO");
                    if (answer == "YES")
                    {

                        do
                        {
                            Console.WriteLine(@$"WRITE THE ""GENDER"" :(MALE/FEMALE) ");
                            answer = Console.ReadLine()?.Trim().ToUpper();

                        } while (answer != "MALE" && answer != "FEMALE");

                        if (answer == "MALE")
                        {
                            gender = 'M';
                        }
                        else
                        {
                            gender = 'F';
                        }

                    }

                    do
                    {
                        Console.WriteLine(@$"DO YOU WANT TO UPDATE THE ""BIRTH DATE"" ?(YES/NO).. ");
                        answer = Console.ReadLine()?.Trim().ToUpper();

                    } while (answer != "YES" && answer != "NO");
                    if (answer == "YES")
                    {

                        do
                        {
                            Console.WriteLine(@$"WRITE THE ""BIRTH YEAR"" : ");
                            birthDateYear = GetPositiveInteger();

                        } while (birthDateYear < 1900 || birthDateYear > 2999);

                        do
                        {
                            Console.WriteLine(@$"WRITE THE ""BIRTH MONTH"" : ");
                            birthDateMonth = GetPositiveByte();

                        } while (birthDateMonth < 1 || birthDateMonth > 12);

                        byte DaysOfMonth = 0;

                        if (birthDateMonth == 1 || birthDateMonth == 3 || birthDateMonth == 5 || birthDateMonth == 7 || birthDateMonth == 8 || birthDateMonth == 10 || birthDateMonth == 12)
                        {
                            DaysOfMonth = 31;
                        }
                        else if (birthDateMonth == 2)
                        {

                            if (DateTime.IsLeapYear(birthDateYear))
                            {
                                DaysOfMonth = 29;
                            }
                            else
                            {
                                DaysOfMonth = 28;
                            }

                        }
                        else
                        {
                            DaysOfMonth = 30;
                        }
                        do
                        {

                            Console.WriteLine(@$"WRITE THE ""BIRTH DAY"" : ");
                            birthDateDay = GetPositiveByte();


                        } while (birthDateDay < 1 || birthDateDay > DaysOfMonth);


                    }

                    do
                    {
                        Console.WriteLine(@$"*CONFIRM UPDATES : (YES/NO).. ");
                        answer = Console.ReadLine()?.Trim().ToUpper();

                    } while (answer != "YES" && answer != "NO");

                    if (answer == "YES")
                    {

                        update_status = await UpdateUser(UserID, FirstName, LastName, gender, birthDateYear, birthDateMonth, birthDateDay).ConfigureAwait(false);
                    }

                    if (update_status == 1) { return 1; }
                    if (update_status == 0) { return 0; }

                }
                else if (UserID != -1)
                {
                    if (UserID < 1) { return 0; }
                    update_status = await UpdateUser(UserID, FirstName, LastName, gender, birthDateYear, birthDateMonth, birthDateDay, -1, FirstName).ConfigureAwait(false);

                    if (update_status == 1) { return 1; }
                    if (update_status == 0) { return 0; }
                }

                return 3;

            }

            public static async Task<byte> DeleteUserPermanently(long Id = 0)
            {

                if (Id < 0) { return 0; }

                string answer = "";

                string SelectQuery_findRecordByID = @$"DELETE FROM {Users_Table_DBName} WHERE {UserID_DBName} = @Id;";


                if (Id > 0)
                {

                    try
                    {
                        using IDbConnection connection = new SqlConnection(SQLConnectionString);
                        await ((DbConnection)connection).OpenAsync().ConfigureAwait(false);
                        DynamicParameters parameters = new DynamicParameters();
                        parameters.Add("@Id", Id);
                        int row = await connection.ExecuteAsync(SelectQuery_findRecordByID, parameters).ConfigureAwait(false);

                        if (row < 1) { return 0; }
                        if (row > 0)
                        {
                            Console.WriteLine($"DONE SUCCESSFULLY.. THE USER NUMBER {Id} HAS BEEN DELETED PERMANENTLY...");
                            return 1;
                        }

                    }
                    catch (SqlException ex)
                    {

                        Console.WriteLine("Database unreachable. Please try again later.");
                        return 0;
                    }
                    return 3;

                }
                else if (Id == 0)
                {


                    do
                    {
                        do
                        {

                            Console.WriteLine(@$"WRITE THE ""USER ID"" : ");
                            Id = GetPositiveLongInteger();

                        } while (Id < 1 || Id > 18000000000000);

                    } while (ClsApplication.FindUser(Id) == null);
                    do
                    {

                        Console.WriteLine(@$"DO YOU WANT TO DELETE THE ""USER"" PERMANENTLY ?(YES/NO).. ");
                        answer = Console.ReadLine().Trim().ToUpper();

                    } while (answer != "YES" && answer != "NO");
                    if (answer == "YES")
                    {
                        do
                        {
                            Console.WriteLine(@$" *ARE YOU SURE YOU WANT TO DELETE THIS ""USER"" PERMANENTLY !!(YES/NO).. ");
                            answer = Console.ReadLine().Trim().ToUpper();

                        } while (answer != "YES" && answer != "NO");
                        if (answer == "YES")
                        {
                            try
                            {
                                using IDbConnection connection = new SqlConnection(SQLConnectionString);

                                await ((DbConnection)connection).OpenAsync().ConfigureAwait(false);

                                DynamicParameters parameters = new DynamicParameters();
                                parameters.Add("@Id", Id);
                                int row = await connection.ExecuteAsync(SelectQuery_findRecordByID, parameters).ConfigureAwait(false);

                                if (row < 1) { return 0; }
                                if (row > 0)
                                {
                                    Console.WriteLine($"DONE SUCCESSFULLY.. THE USER NUMBER {Id} HAS BEEN DELETED PERMANENTLY...");
                                    return 1;
                                }
                            }
                            catch (SqlException ex)
                            {

                                Console.WriteLine("Database unreachable. Please try again later.");
                                return 0;
                            }
                            return 3;
                        }

                    }

                }

                return 9;

            }

            public static async Task<int> DeleteUser(long Id = 0)
            {

                if (Id < 0) { return 0; }


                string answer = "";

                string UpdateQuery_UpdateRecordByID = @$"UPDATE {Users_Table_DBName} SET {UserStatus_DBName} = 1 WHERE {UserID_DBName} = @Id;";

                if (Id > 0)
                {

                    try
                    {
                        using IDbConnection connection = new SqlConnection(SQLConnectionString);

                        await ((DbConnection)connection).OpenAsync().ConfigureAwait(false);

                        DynamicParameters parameters = new DynamicParameters();
                        parameters.Add("@Id", Id);
                        int row = await connection.ExecuteAsync(UpdateQuery_UpdateRecordByID, parameters).ConfigureAwait(false);

                        if (row < 1) { return 0; }
                        if (row > 0)
                        {
                            Console.WriteLine($"DONE SUCCESSFULLY.. THE USER NUMBER {Id} HAS BEEN DELETED ...");
                            return 1;
                        }
                    }
                    catch (SqlException ex)
                    {

                        Console.WriteLine("Database unreachable. Please try again later.");
                        return -1;
                    }
                    return -3;

                }
                else if (Id == 0)
                {

                    do
                    {
                        do
                        {

                            Console.WriteLine(@$"WRITE THE ""USER ID"" : ");
                            Id = GetPositiveLongInteger();

                        } while (Id < 1 || Id > 18000000000000);

                    } while (ClsApplication.FindUser(Id) == null);
                    do
                    {

                        Console.WriteLine(@$"DO YOU WANT TO DELETE THE ""USER"" ?(YES/NO).. ");
                        answer = Console.ReadLine().Trim().ToUpper();

                    } while (answer != "YES" && answer != "NO");
                    if (answer == "YES")
                    {
                        do
                        {
                            Console.WriteLine(@$" *ARE YOU SURE YOU WANT TO DELETE THIS ""USER"" !!(YES/NO).. ");
                            answer = Console.ReadLine().Trim().ToUpper();

                        } while (answer != "YES" && answer != "NO");
                        if (answer == "YES")
                        {
                            try
                            {

                                {
                                    using IDbConnection connection = new SqlConnection(SQLConnectionString);
                                    await ((DbConnection)connection).OpenAsync().ConfigureAwait(false);

                                    DynamicParameters parameters = new DynamicParameters();
                                    parameters.Add("@Id", Id);
                                    int row = await connection.ExecuteAsync(UpdateQuery_UpdateRecordByID, parameters).ConfigureAwait(false);

                                    if (row < 1) { return 0; }
                                    if (row > 0)
                                    {
                                        Console.WriteLine($"DONE SUCCESSFULLY.. THE USER NUMBER {Id} HAS BEEN DELETED ...");
                                        return 1;
                                    }

                                }
                            }
                            catch (SqlException ex)
                            {

                                Console.WriteLine("Database unreachable. Please try again later.");
                                return -1;
                            }
                            return -3;
                        }

                    }

                }

                return -3;

            }

            public static long GetPositiveLongInteger(string prompt = "@$\"WRITE THE \"\"USER ID\"\" : \"", string warning = "WRONG ENTERY!.. PLEASE WRITE POSITIVE NUMBERS ONLY ")
            {
                while (true)
                {
                    //Console.Write(prompt);
                    string input = Console.ReadLine();

                    if (long.TryParse(input, out long number) && number > 0)
                    {
                        return number;
                    }
                    Console.WriteLine(warning);
                }
            }

            public static int GetPositiveInteger(string prompt = "@$\"WRITE THE \"\"USER ID\"\" : \"", string warning = "WRONG ENTERY!.. PLEASE WRITE POSITIVE NUMBERS ONLY ")
            {
                while (true)
                {
                    //Console.Write(prompt);
                    string input = Console.ReadLine();

                    if (int.TryParse(input, out int number) && number > 0)
                    {
                        return number;
                    }
                    Console.WriteLine(warning);
                }
            }

            public static byte GetPositiveByte(string prompt = "@$\"WRITE THE \"\"USER ID\"\" : \"", string warning = "WRONG ENTERY!.. PLEASE WRITE POSITIVE NUMBERS ONLY ")
            {
                while (true)
                {
                    //Console.Write(prompt);
                    string input = Console.ReadLine();

                    if (byte.TryParse(input, out byte number) && number > 0)
                    {
                        return number;
                    }
                    Console.WriteLine(warning);
                }
            }

            public static async Task<int> SavePhoto(string imagePath, string imageName, long Id)
            {

                byte[] imageData = File.ReadAllBytes(imagePath);

                string SQLServerPassword = Environment.GetEnvironmentVariable("SQLPASSWORD");
                string DatabaseName = "ABDELRHMAN_PROJECT";
                string MainTableName = "ProfileImages";
                string JoinTable_right = "Users";
                string SQLConnectionString = $"Data Source = localhost,2004; Initial Catalog = {DatabaseName}; User ID = SA; Password = {SQLServerPassword}; Encrypt=True;TrustServerCertificate=True;Connection timeout = 3;";
                string ProfileImageID_DBName = "ID";
                string Query_InsertImage = @$"INSERT INTO {MainTableName} (guid_id, name, file_data, height, width, size_byte, type, path, owner_id, privacy, storage_bucket, storage_key, check_hash, percept_hash, exif_json_details, album_id, post_id, created_at, updated_at, status ) 
                     VALUES (guid, name, file_data, height, width, size_byte, type, path, owner_id, privacy, storage_bucket, storage_key, check_hash, percept_hash, exif_json_details, album_id, post_id, created_at, updated_at, status ) WHERE {ProfileImageID_DBName} = @Id;";


                try
                {
                    using IDbConnection connection = new SqlConnection(SQLConnectionString);
                    await ((DbConnection)connection).OpenAsync().ConfigureAwait(false);
                    DynamicParameters parameters = new DynamicParameters();
                    parameters.Add("@Id", Id);
                    parameters.Add("@ImageName", imageName);
                    parameters.Add("@ImageData", imageData);
                    parameters.Add("@ImageSize", imageData.Length);
                    parameters.Add("@ImageSize", imageData.Length);
                    parameters.Add("@ImageSize", imageData.Length);
                    parameters.Add("@ImageSize", imageData.Length);
                    parameters.Add("@ImageSize", imageData.Length);
                    parameters.Add("@ImageSize", imageData.Length);
                    parameters.Add("@ImageSize", imageData.Length);
                    parameters.Add("@ImageSize", imageData.Length);
                    parameters.Add("@ImageSize", imageData.Length);
                    parameters.Add("@ImageSize", imageData.Length);
                    parameters.Add("@ImageSize", imageData.Length);
                    parameters.Add("@ImageSize", imageData.Length);
                    parameters.Add("@ImageSize", imageData.Length);
                    parameters.Add("@ImageSize", imageData.Length);


                    long row = await connection.ExecuteAsync(Query_InsertImage, parameters);

                    if (row < 1) { return 0; }
                    if (row > 0)
                    {
                        Console.WriteLine($"DONE SUCCESSFULLY.. THE USER NUMBER {Id} HAS BEEN DELETED PERMANENTLY...");
                        return 1;
                    }

                }
                catch (SqlException ex)
                {

                    Console.WriteLine("Database unreachable. Please try again later.");
                    return -1;
                }
                return -3;

            }

            // <summary>
            /// تسجيل خطأ في سجل النظام (يمكن استبداله بـ ILogger أو Serilog)
            /// </summary>
            public static void LogError(string message, Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ERROR] {message}: {ex.Message}");
                Console.ResetColor();
            }

            /// <summary>
            /// تسجيل معلومات عامة (يمكن استبداله بـ ILogger)
            /// </summary>
            public static void LogInfo(string message)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[INFO] {message}");
                Console.ResetColor();
            }

            public static async Task<int> SaveByteFile(string FilePath, string fileName, long? userID, Guid? userGuid, long? albumID = null, Guid? albumGuid = null, long? postID = null, Guid? postGuid = null,

            CancellationToken ct = default, string storageBucket = "mypro-asia-media1-1")
            {
                if (string.IsNullOrWhiteSpace(FilePath)) throw new ArgumentException("FilePath empty");
                if (!File.Exists(FilePath)) throw new FileNotFoundException(FilePath);
                if (userID == null || userID < 1) throw new ArgumentException("userID incorrect");
                if (userGuid == null || userGuid == Guid.Empty)
                    throw new ArgumentException("userGuid cannot be null or empty", nameof(userGuid));
                if (albumID != null && albumID < 1) throw new ArgumentException("albumID incorrect");
                if (albumGuid != null && albumGuid == Guid.Empty)
                    throw new ArgumentException("albumGuid cannot be empty", nameof(albumGuid));
                if (postID != null && postID < 1) throw new ArgumentException("postID incorrect");
                if (postGuid != null && postGuid == Guid.Empty)
                    throw new ArgumentException("postGuid cannot be empty", nameof(postGuid));
                if (string.IsNullOrEmpty(fileName))
                {
                    fileName = System.IO.Path.GetFileName(FilePath) ?? throw new ArgumentException("This file has no name that's means it is a Virus!");
                }

                //byte[] fileByteData = await File.ReadAllBytesAsync(FilePath).ConfigureAwait(false);
                const long MAX_FILE_BYTES = 150L * 1024 * 1024; // 150MB safety limit (تستطيع تعديلها)
                                                                //if (fileByteData.LongLength > MAX_FILE_BYTES) throw new ArgumentException("file too large");

                byte[] fileBytes;
                int width = 0;
                int height = 0;
                long fileSize;
                string mimeType = "application/octet-stream";
                string? exifJson = "{}";
                byte[]? thumbnailBytes = Array.Empty<byte>();
                ulong aHash = 0UL; // perceptual hash
                byte[] sha256Bytes; // 32 bytes
                long perceptHashSigned = 0L; // we will fill if image
                string derivativesJson = "{}";
                ; // مثال: اسم الـ bucket أو container
                string storageKey = $"{DateTimeOffset.UtcNow:yyyy/MM}/{Guid.NewGuid():N}{System.IO.Path.GetExtension(fileName)?.ToLowerInvariant()}";
                long privacy = 0; // default privacy (0=public, 1=friends, 2=private) — غيرها حسب نظامك
                string? embeddingReference = null;
                long accessCount = 0;
                DateTimeOffset? lastServed = null;
                DateTimeOffset createdAt = DateTimeOffset.UtcNow;
                DateTimeOffset updatedAt = DateTimeOffset.UtcNow;
                long status = 2;

                bool isImage = false;
                bool isVideo = false;
                VideoMetadata? videoMeta = null;

                await using FileStream? fileSTREAM = new(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
                if (fileSTREAM == null || fileSTREAM.Length == 0)
                    throw new ArgumentException("FilePath is Invalid");
                fileSize = fileSTREAM.Length;
                if (fileSize > MAX_FILE_BYTES)
                    throw new ArgumentException("File exceeds maximum size (150MB)");

                fileSTREAM.Position = 0;
                using var sha256 = SHA256.Create();
                sha256Bytes = await sha256.ComputeHashAsync(fileSTREAM, ct).ConfigureAwait(false);
                fileSTREAM.Position = 0; // reset to start for next operations

                fileBytes = new byte[fileSize];
                await fileSTREAM.ReadAsync(fileBytes, ct);
                Console.WriteLine("File read into byte array the size is: " + fileBytes.Length);
                try
                {
                    MemoryStream streamCheck = new MemoryStream(fileBytes, 0, 256);
                    try
                    {
                        var fileInfo = await SixLabors.ImageSharp.Image.IdentifyAsync(streamCheck);
                        if (fileInfo != null)
                        {

                            isImage = true;
                            mimeType = fileInfo.Metadata.DecodedImageFormat?.DefaultMimeType ?? "image/*";
                            width = fileInfo.Width;
                            height = fileInfo.Height;
                        }

                    }
                    catch
                    {
                        //fileSTREAM.Position = 0;
                        //byte[] header = new byte[Math.Min(256, fileSize)];
                        //await fileSTREAM.ReadExactlyAsync(header, ct);
                        byte[] header = streamCheck.ToArray();

                        for (int i = 0; i < header.Length - 4; i++)
                        {

                            if (header[i] == 'f' && header[i + 1] == 't' && header[i + 2] == 'y' && header[i + 3] == 'p')
                            {
                                isVideo = true;
                                mimeType = "video/mp4";
                                break;

                            }
                        }
                    }

                }
                catch { }


                // 2) إن كانت صورة - استعمل AnalyzeImageBytes (تستخرج thumbnail, exif, aHash...)
                if (isImage)
                {
                    Console.WriteLine("This is an Image File");
                    try
                    {

                        fileSTREAM.Position = 0;
                        fileBytes = new byte[fileSize];
                        await fileSTREAM.ReadAsync(fileBytes, ct);

                        var meta = await MediaInspectors.AnalyzeImageBytesAsync(fileBytes, fileNameHint: fileName, thumbnailMaxSize: 300);
                        width = meta.Width;
                        height = meta.Height;
                        mimeType = meta.MimeType ?? mimeType;
                        exifJson = meta.ExifJson ?? "{}";
                        thumbnailBytes = meta.ThumbnailJpeg ?? Array.Empty<byte>();
                        aHash = meta.AHash;
                        perceptHashSigned = unchecked((long)aHash);

                        var derivativesObj = new
                        {
                            thumbnail_size = thumbnailBytes?.LongLength ?? 0,
                            thumbnail_encoding = (thumbnailBytes != null && thumbnailBytes.Length > 0) ? "jpeg-base64" : null,
                            thumbnail_embedded = (thumbnailBytes != null && thumbnailBytes.Length <= 100 * 1024) ? Convert.ToBase64String(thumbnailBytes) : null,
                            generated_at_utc = DateTime.UtcNow
                        };
                        derivativesJson = JsonSerializer.Serialize(derivativesObj);
                    }
                    catch
                    {
                        try
                        {
                            var ex = clsExif.ExifReader.ExtractExif(fileBytes);
                            exifJson = ex.ToJson() ?? "{}";
                        }
                        catch (Exception ex)
                        {
                            LogError("Image analysis failed", ex);
                        }

                    }
                }
                else if (isVideo)
                {
                    // 3) إن كان فيديو - استعمل ffprobe لتحليل (يجب أن يتوفر ffprobe في PATH)
                    try
                    {
                        // وظيفة AnalyzeVideoFileWithFfprobe هي async
                        videoMeta = await MediaInspectors.AnalyzeVideoFileWithFfprobeAsync(FilePath).ConfigureAwait(false);
                        if (videoMeta != null)
                        {
                            mimeType = videoMeta.FormatName != null ? $"video/{videoMeta.FormatName.Split(',')[0]}" : mimeType;
                            width = videoMeta.Width ?? 0;
                            height = videoMeta.Height ?? 0;

                            var derivativesObj = new
                            {
                                video_duration_seconds = videoMeta.DurationSeconds,
                                video_codec = videoMeta.Codec,
                                generated_at_utc = DateTime.UtcNow
                            };

                            derivativesJson = JsonSerializer.Serialize(derivativesObj);

                        }
                    }
                    catch (Exception ex)
                    {
                        // ignore ffprobe failures (server قد لا يحتويه)
                        LogError("Video analysis failed", ex);
                    }
                }
                else
                {
                    try
                    {
                        fileSTREAM.Position = 0;
                        fileBytes = new byte[fileSize];
                        await fileSTREAM.ReadAsync(fileBytes, ct);

                        var detected = SixLabors.ImageSharp.Image.DetectFormat(fileBytes);
                        if (detected != null)
                        {
                            mimeType = detected.DefaultMimeType ?? mimeType;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError("Image analysis failed, We Cannot Analize your file, May be you are Hacker and it's a Virus", ex);
                        return 0;
                    }
                }

                // 4) إعداد قيم إضافية (storage path/keys)
                // (في تطبيق حقيقي عادة تقوم برفع الملف إلى Object Storage مثل S3/GCS/Azure وتهيء storageBucket & storageKey)
                // هنا نستخدم مكان تخزين افتراضي (يمكنك تغييره)
                // storageKey تمّ توليده أعلاه، لكن نحدثه ليحتوي اسم الملف الأصلي

                // 5) بناء INSERT إلى قاعدة البيانات

                string Query_InsertByteFile = $@"
INSERT INTO {ByteFiles_Table_DBName}
(
    {ByteFiles_GUID_DBName}, {ByteFiles_OwnerID_DBName},  {ByteFiles_OwnerGUID_DBName},
    {ByteFiles_FileName_DBName}, {ByteFiles_FileData_DBName}, {ByteFiles_Thumbnail_DBName},
    {ByteFiles_Width_DBName}, {ByteFiles_Height_DBName}, {ByteFiles_SizeByte_DBName}, 
    {ByteFiles_MimeType_DBName}, {ByteFiles_Path_DBName}, {ByteFiles_Privacy_DBName},
    {ByteFiles_StorageBucket_DBName}, {ByteFiles_StorageKey_DBName}, {ByteFiles_CheckHash_DBName},
    {ByteFiles_PerceptHash_DBName}, {ByteFiles_ExifDetails_DBName}, {ByteFiles_Derivatives_DBName},
    {ByteFiles_EmbeddingRef_DBName}, {ByteFiles_AccessCount_DBName}, {ByteFiles_LastServed_DBName},
    {ByteFiles_AlbumID_DBName}, {ByteFiles_AlbumGUID_DBName}, {ByteFiles_PostID_DBName},
    {ByteFiles_PostGUID_DBName}, {ByteFiles_CreatedAt_DBName}, {ByteFiles_UpdatedAt_DBName}, 
    {ByteFiles_Status_DBName}
)
VALUES
(
    @{ByteFiles_GUID_MapName}, @{ByteFiles_OwnerID_MapName}, @{ByteFiles_OwnerGUID_MapName}, @{ByteFiles_FileName_MapName},
    @{ByteFiles_FileData_MapName}, @{ByteFiles_Thumbnail_MapName}, @{ByteFiles_Width_MapName}, @{ByteFiles_Height_MapName}, 
    @{ByteFiles_SizeByte_MapName}, @{ByteFiles_MimeType_MapName}, @{ByteFiles_Path_MapName},  @{ByteFiles_Privacy_MapName},
    @{ByteFiles_StorageBucket_MapName}, @{ByteFiles_StorageKey_MapName}, @{ByteFiles_CheckHash_MapName}, @{ByteFiles_PerceptHash_MapName},
    @{ByteFiles_ExifDetails_MapName}, @{ByteFiles_Derivatives_MapName}, @{ByteFiles_EmbeddingRef_MapName}, @{ByteFiles_AccessCount_MapName},
    @{ByteFiles_LastServed_MapName}, @{ByteFiles_AlbumID_MapName}, @{ByteFiles_AlbumGUID_MapName}, @{ByteFiles_PostID_MapName}, 
    @{ByteFiles_PostGUID_MapName},@{ByteFiles_CreatedAt_MapName}, @{ByteFiles_UpdatedAt_MapName},  @{ByteFiles_Status_MapName}
);
";


                Guid fileGuid = Guid.NewGuid();

                try
                {
                    await using DbConnection connection = CreateConnection();
                    await connection.OpenAsync(ct).ConfigureAwait(false);
                    await using var command = connection.CreateCommand();

                    command.CommandText = Query_InsertByteFile;
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = CRUD_DefaultDbTimeout;

                    char prefix = SqlParameterPrefix(connection);


                    DbParameter AddParam(string name, object? value, DbType type)
                    {
                        var parameter1 = command.CreateParameter();
                        parameter1.ParameterName = prefix + name;
                        parameter1.Value = value ?? DBNull.Value;
                        parameter1.DbType = type;
                        return parameter1;
                    }

                    command.Parameters.Add(AddParam(ByteFiles_GUID_MapName, fileGuid, DbType.Guid));
                    command.Parameters.Add(AddParam(ByteFiles_OwnerID_MapName, userID, DbType.Int64));
                    command.Parameters.Add(AddParam(ByteFiles_OwnerGUID_MapName, userGuid, DbType.Guid));
                    command.Parameters.Add(AddParam(ByteFiles_FileName_MapName, fileName, DbType.String));
                    command.Parameters.Add(AddParam(ByteFiles_FileData_MapName, fileBytes, DbType.Binary));
                    command.Parameters.Add(AddParam(ByteFiles_Thumbnail_MapName, thumbnailBytes, DbType.Binary));
                    command.Parameters.Add(AddParam(ByteFiles_Height_MapName, height, DbType.Int32));
                    command.Parameters.Add(AddParam(ByteFiles_Width_MapName, width, DbType.Int32));
                    command.Parameters.Add(AddParam(ByteFiles_SizeByte_MapName, fileSize, DbType.Int64));
                    command.Parameters.Add(AddParam(ByteFiles_MimeType_MapName, mimeType, DbType.String));
                    command.Parameters.Add(AddParam(ByteFiles_Path_MapName, FilePath, DbType.String));
                    command.Parameters.Add(AddParam(ByteFiles_Privacy_MapName, privacy, DbType.Int64));
                    command.Parameters.Add(AddParam(ByteFiles_StorageBucket_MapName, storageBucket, DbType.String));
                    command.Parameters.Add(AddParam(ByteFiles_StorageKey_MapName, storageKey, DbType.String));
                    command.Parameters.Add(AddParam(ByteFiles_CheckHash_MapName, sha256Bytes, DbType.Binary));
                    command.Parameters.Add(AddParam(ByteFiles_PerceptHash_MapName, perceptHashSigned, DbType.Int64));
                    command.Parameters.Add(AddParam(ByteFiles_ExifDetails_MapName, exifJson, DbType.String));
                    command.Parameters.Add(AddParam(ByteFiles_AlbumID_MapName, albumID, DbType.Int64));
                    command.Parameters.Add(AddParam(ByteFiles_AlbumGUID_MapName, albumGuid, DbType.Guid));
                    command.Parameters.Add(AddParam(ByteFiles_PostID_MapName, postID, DbType.Int64));
                    command.Parameters.Add(AddParam(ByteFiles_PostGUID_MapName, postGuid, DbType.Guid));
                    command.Parameters.Add(AddParam(ByteFiles_Derivatives_MapName, derivativesJson, DbType.String));
                    command.Parameters.Add(AddParam(ByteFiles_EmbeddingRef_MapName, embeddingReference, DbType.String));
                    command.Parameters.Add(AddParam(ByteFiles_AccessCount_MapName, accessCount, DbType.Int64));
                    command.Parameters.Add(AddParam(ByteFiles_LastServed_MapName, lastServed, DbType.DateTimeOffset));
                    command.Parameters.Add(AddParam(ByteFiles_CreatedAt_MapName, createdAt, DbType.DateTimeOffset));
                    command.Parameters.Add(AddParam(ByteFiles_UpdatedAt_MapName, updatedAt, DbType.DateTimeOffset));
                    command.Parameters.Add(AddParam(ByteFiles_Status_MapName, status, DbType.Int64));

                    int rows = await command.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                    if (rows > 0)
                    {


                        return 1;
                    }
                    else
                    {
                        return 0;
                    }


                }
                catch (DbException ex)
                {
                    Console.WriteLine("Database unreachable. " + ex.Message);
                    return -1;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unexpected error: " + ex.Message);
                    return -3;

                }
            }


            public static void logger(string message)
            {
                var builder = new ServiceCollection();
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.Console()
                    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
                    .CreateLogger();
            }
        }


    }


}

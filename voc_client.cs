//Request library
using System;
using System.Net;
using System.IO;
using System.Data;

//certificates
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Collections.Generic;

//json
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;
//sqllite
#if __MonoCS__
    using Mono.Data.Sqlite;
    //using SQLiteCommand =     Mono.Data.Sqlite.SqliteCommand;
    //using SQLiteConnection =  Mono.Data.Sqlite.SqliteConnection;
    //using SQLiteException =   Mono.Data.Sqlite.SqliteException;
    //using SQLiteParameter =   Mono.Data.Sqlite.SqliteParameter;
    //using SQLiteTransaction = Mono.Data.Sqlite.SqliteTransaction;
#else
    using System.Data.SQLite;
#endif



public class VocSyncRequestClient
{
    public static bool Validator (object sender, X509Certificate certificate, X509Chain chain,
                                      SslPolicyErrors sslPolicyErrors)
    {
        return true;
    }

    public static string Get(string url, bool verify=true)
    {

        string resp = string.Empty;

        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        request.AutomaticDecompression = DecompressionMethods.GZip;

        if(!verify){
            // Ignore certificates for now muhahaha
            ServicePointManager.ServerCertificateValidationCallback = Validator;
        }

        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
        using (Stream stream = response.GetResponseStream())
        using (StreamReader reader = new StreamReader(stream))
        {
                resp = reader.ReadToEnd();
        }
        return resp;
    }

    public static string Post(string url, string json_str, bool verify=true)
    {

        string resp = string.Empty;

        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        request.AutomaticDecompression = DecompressionMethods.GZip;
        request.ContentType = "application/json";
        request.Method = "POST";

        if(json_str != ""){
            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(json_str);
                streamWriter.Flush();
                streamWriter.Close();
            }
        }

        if(!verify){
            // Ignore certificates for now muhahaha
            ServicePointManager.ServerCertificateValidationCallback = Validator;
        }

        try
        {
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                    resp = reader.ReadToEnd();
            }
        }
        catch (WebException ex){
            using (var stream = ex.Response.GetResponseStream())
            using (var reader = new StreamReader(stream))
            {
                resp = reader.ReadToEnd();
            }
        }
        return resp;
    }
}

// Database Library to sqlite 
public class DatabaseLib
{
    private IDbConnection  dbconn;

    public DatabaseLib()
    {
        const string connectionString = "URI=file:SqliteTest.db";
        this.dbconn = new SqliteConnection(connectionString);
        this.dbconn.Open();
    }

    ~DatabaseLib()
    {
        // clean up
        dbconn.Close();
    }

    public bool execute_query(IDbConnection dbcon, string sql)
    {
        IDbCommand dbcmd = this.dbconn.CreateCommand();
        dbcmd.CommandText = sql;
        dbcmd.ExecuteNonQuery();
        dbcmd.Dispose();
        return true;
    }

    public void create_tables()
    {

        string sql = "create table if not exists voc_user" +
                          "(userid text, password text," +
                          "device_id text, platform text," +
                          "device_type text, access_token text," +
                          "refresh_token text, voc_id text," +
                          "congestion_detection text, ads_frequency text," +
                          "daily_quota integer, daily_manifest integer," +
                          "daily_download_wifi integer, daily_download_cellular integer," +
                          "congestion text, sdk_capabilities text," +
                          "max_content_duration integer, play_ads text," +
                          "skip_policy_first_time text, tod_policy text," +
                          "token_expiration integer, server text," +
                          "server_state text, my_row integer primary key autoincrement)";
        this.execute_query(dbconn, sql);

        sql =  "create table if not exists provider " +
                        " (name text unique, contentprovider text, subscribed integer)";
        this.execute_query(dbconn, sql);


        sql = "create table if not exists category (name text unique,subscribed integer)";
        this.execute_query(dbconn, sql);

        sql = "create table if not exists uuid_table (uuid text)";
        this.execute_query(dbconn, sql);
        
        sql = "create table if not exists playing (unique_Id text,timestamp DATETIME DEFAULT CURRENT_TIMESTAMP)";
        this.execute_query(dbconn, sql);

        sql = "create table if not exists content_status " + 
                " (download_time text,download length integer,download_duration real,eviction_info text,user_rating int,unique_id text, my_row integer primary key autoincrement)";
        this.execute_query(dbconn, sql);
            
        sql = "create table if not exists consumption_status (watch_time int,watchstart integer,watchend int,my_row integer primary key autoincrement)";
        this.execute_query(dbconn, sql);

        sql = " create table if not exists ad_consumption_status (adurl text,duration int, starttime integer,stopposition int, clicked int,unique_id text, my_row integer primary key autoincrement)";
        this.execute_query(dbconn, sql);

        sql = " create table if not exists cache_manifest " +
            "( local_file text, local_thumbnail text, " +
            " local_info text, video_size integer, " +
            " thumbnail_size integer, download_date integer, " +
            " content_provider text, category text, " +
            " unique_id text, summary text, " +
            " title text, duration integer, " +
            " timestamp integer, sdk_metadata text, " +
            " streams text,   ad_server_url text, " +
            " tags text, priority integer, " +
            " object_type text, thumb_attrs text, " +
            " object_attrs text, children text, " +
            " policy_name text, key_server_url text, " +
            " save integer default 0, my_row integer primary key autoincrement)";
        this.execute_query(dbconn, sql);
    } 
   
    public void AddParameter (IDbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }
 
    public void addVocUser(RegResp reg)
    {
        string query = "INSERT INTO voc_user( voc_id, access_token, refresh_token, " +
                        "daily_download_wifi)" +
                        "values(@vocId, @accessToken, @refreshToken, @dailyDownloadWifi)";

        IDbCommand myCommand = this.dbconn.CreateCommand();
        myCommand.CommandText = query;
        this.AddParameter( myCommand, "@vocId", reg.VocId);
        this.AddParameter( myCommand, "@accessToken", reg.AccessToken);
        this.AddParameter( myCommand, "@refreshToken", reg.RefreshToken);
        this.AddParameter( myCommand, "@dailyDownloadWifi", reg.DailyDownloadWifi);
        myCommand.ExecuteNonQuery();
    }

    public RegResp getVocUser()
    {

        string query = "SELECT voc_id, access_token, refresh_token, daily_download_wifi FROM voc_user";
        IDbCommand myCommand = this.dbconn.CreateCommand();
        myCommand.CommandText = query;
        RegResp body = new RegResp();
        using (var dataReader = myCommand.ExecuteReader())
        {

            if ( dataReader.Read())
            {
                body.VocId = dataReader.GetString(0);
                body.AccessToken = dataReader.GetString(1);
            }
            else
            {
                return null;
            }
            
        }
        return body;

    }

}

//TODO should this be private?
public class ServerState
{
    public string SchemaName;
    public string TenantId; 
    
    public ServerState(string schema, string tenantId){
        this.SchemaName = schema;
        this.TenantId = tenantId;
    }
}

// Class for json for Registration Request
public class RegBody
{
    public ServerState ServerState;
    public string Platform;
    public string DeviceId;
    public string PushToken;
    public string DeviceType;
    public string PublicKey;
    public string Version;

    public RegBody(ServerState s, string pk)
    {
        this.ServerState = s;
        this.PublicKey = pk;
        this.DeviceId = System.Guid.NewGuid().ToString();
        this.Platform = "MonoClient";
    }
}

// Class to store the response json from Registration 
public class RegResp
{
    public string VocId;
    public string RefreshToken;
    public string AccessToken;
    public int? DailyDownloadWifi;
    public bool? PlayAds;
    public bool? SkipPolicyFirstTime;
}

// Main class for the VoClient sdk
public class VocClient 
{
    public DatabaseLib dblib;
    private  ServerState serverState;

    public string VocHost {get; set;}
    public string PublicKey;

    public VocClient(string host)
    {
        this.dblib = new DatabaseLib();
        dblib.create_tables();
        this.VocHost = host;
    }

    public bool Register(string schema, string tenantId, string publicKey)
    {
        this.PublicKey = publicKey;
        this.serverState = new ServerState(schema, tenantId);
        RegBody r = new RegBody(this.serverState, publicKey);

        string url = string.Format("https://{0}/Anaina/v0/Register", this.VocHost);

        JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
        string json_body = JsonConvert.SerializeObject(r, Formatting.Indented, jsonSerializerSettings); 
        Console.WriteLine(url);
        Console.WriteLine(json_body);
        string resp = VocSyncRequestClient.Post(url, json_body, verify:false); 
        Console.WriteLine(resp);

        RegResp deserializedResp = JsonConvert.DeserializeObject<RegResp>(resp);     
        Console.WriteLine(deserializedResp.VocId);
        Console.WriteLine(deserializedResp.RefreshToken);
        Console.WriteLine(deserializedResp.AccessToken);
        this.dblib.addVocUser(deserializedResp);
        return true;    
    }

    public JToken DownloadManifest()
    {
        string url = string.Format("https://{0}/Anaina/v0/Download-Manifest", this.VocHost);

        var vocUser = dblib.getVocUser();
        if(vocUser != null)
        {
            JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            var json_body = JsonConvert.SerializeObject(vocUser, jsonSerializerSettings);

            Console.WriteLine("vocId: " + json_body);
            Console.WriteLine("Url: " + url);

            string resp = VocSyncRequestClient.Post(url, json_body, verify:false);
            if (resp == "")
            {
                Console.WriteLine("Empty Manifest");
            }
            else 
            {
                try 
                {
                   return JToken.Parse(resp);
                }
                catch 
                {
                   Console.WriteLine("Failure parsing response to JSON");     
                }       
            }              
        }
        else 
        {
            Console.WriteLine("No voc_user record, registration needed");
        }

        return null;
    }


    static public void Main(string[] args)
    {
        Console.WriteLine ("Hello Mono World");
        WebClient wb = new WebClient(); 
        wb.DownloadFile("http://humanstxt.org/humans.txt", "foo.txt");

        if(args.Length > 0)
        {
            if(args[0] == "register"){
                string host = args[1];
                string schema = args[2];
                string publicKey = args[3];
   
                VocClient vc = new VocClient(host);
                vc.Register(schema, "", publicKey);
            } 
            else if (args[0] == "download-manifest")
            {
                string host = args[1];
                string schema = args[2];

                VocClient vc = new VocClient(host);
                JToken jsonManifest = vc.DownloadManifest();
                Console.WriteLine("Manifest: " + jsonManifest );
            }   
            else
            {
                Console.WriteLine ("Command not supported");
            }
        } else
        {
            Console.WriteLine ("No Command given");
        }

//            Console.WriteLine(resp);
//            Dictionary<string, dynamic> dictionary = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(resp);
//            foreach (KeyValuePair<string, dynamic> kvp in dictionary)
//            {
//                Console.WriteLine(string.Format(" {0}: {1}", kvp.Key, kvp.Value));
//            }
//        }
    }

}
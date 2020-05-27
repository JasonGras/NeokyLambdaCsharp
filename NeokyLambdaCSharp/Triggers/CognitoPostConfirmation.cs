using System;
using System.IO;
using System.Collections.Generic;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using NeokyLambdaCSharp.Models;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using System.Threading.Tasks;
//using Amazon.S3;
//using Amazon.S3.Model;

namespace NeokyLambdaCSharp.Triggers
{
    public class CognitoPostConfirmation {

        //--- Fields ---
        private readonly static AmazonDynamoDBClient client = new AmazonDynamoDBClient();
        private readonly IAmazonCognitoIdentityProvider _cognitoIdentityProviderClient;
        private readonly string _prefix;
        private string _usersBucketName;
        private static DateTime aDate;
        private static string TableName = "Neoky_clients";

        //--- Constructors ---
        public CognitoPostConfirmation(string prefix)
        {
            //_s3Client = new AmazonS3Client();
            aDate = DateTime.UtcNow.AddHours(2); // Viser l'heure en cours en France
            _cognitoIdentityProviderClient = new AmazonCognitoIdentityProviderClient();
            _prefix = prefix;
        }

        //--- Methods ---
        public PostConfirmationBase FunctionHandler(PostConfirmationBase cognitoPostConfirmationEvent, ILambdaContext context)
        {
            //## LEVEL 5 - Add Post Confirmation Workflow Trigger
            LambdaLogger.Log("LEVEL 5 - Add Post Confirmation Workflow Trigger");

            //Search If User Already Exists
            //int resultGetData = GetDataAsync(cognitoPostConfirmationEvent, context).Result;
            //context.Logger.LogLine("Does user Exist Result is : " + resultGetData);

            //if (resultGetData > 0){
                //This user sub already exist in DynamoDB
            //    context.Logger.LogLine("This user Already exist in DynamoDB");
                
            //}
            //else
            //{
                // It's a New user coz Result = 0 !
            //    context.Logger.LogLine("New User Detected by GetDataAsync !");
                // Put The User Confirmed in Dyname Database
                string resultPutData = PutDataAsync(cognitoPostConfirmationEvent, context).Result;
                context.Logger.LogLine("FunctionHandler | PutData Status : " + resultPutData);
            //}           

            return cognitoPostConfirmationEvent;
        }

        private static async Task<string> PutDataAsync(PostConfirmationBase cognitoEvent, ILambdaContext context1)
        {
            Table table = Table.LoadTable(client, TableName);
            try
            {
                var book = new Document();
                book["client_sub"] = cognitoEvent.Request.UserAttributes["sub"];
                book["username"] = cognitoEvent.UserName;
                book["email"] = cognitoEvent.Request.UserAttributes["email"];
                book["account_statut"] = cognitoEvent.Request.UserAttributes["cognito:user_status"];
                book["level"] = "1";
                book["level_xp"] = "1";
                book["Created"] = aDate.ToString("yyyy-MM-ddTHH:mm:ssZ");
                book["Modified"] = aDate.ToString("yyyy-MM-ddTHH:mm:ssZ");

                Document x = await table.PutItemAsync(book);
                context1.Logger.LogLine("PutDataAsync | Success");
                return "success";
            }
            catch (Exception ex)
            {
                context1.Logger.LogLine("PutDataAsync | Exception | "+ex);
                return "failed";
            }
        }

        private static async Task<int> GetDataAsync(PostConfirmationBase cognitoEvent, ILambdaContext context1)
        {
            Table table = Table.LoadTable(client, TableName);

            ScanFilter scanFilter = new ScanFilter();
            
            try
            {
                scanFilter.AddCondition("client_sub", ScanOperator.Equal, cognitoEvent.Request.UserAttributes["sub"]); // 
                Search search = table.Scan(scanFilter);
                //context1.Logger.LogLine(search.Count.ToString());
                return search.Count;
            }        
            catch (Exception ex)
            {
                context1.Logger.LogLine("PutDataAsync | Exception | "+ex);
                return 1;
            }
        }
    }
}
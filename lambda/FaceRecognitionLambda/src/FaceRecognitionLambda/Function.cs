using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using System.Text.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace FaceRecognitionLambda;

public class Function
{
    private readonly IAmazonS3 S3Client;
    private readonly IAmazonRekognition RekognitionClient;

    public Function()
    {
        S3Client = new AmazonS3Client();
        RekognitionClient = new AmazonRekognitionClient();
    }

    public Function(IAmazonS3 s3Client, IAmazonRekognition rekognitionClient)
    {
        this.S3Client = s3Client;
        this.RekognitionClient = rekognitionClient;
    }

    public async Task FunctionHandler(S3Event evnt, ILambdaContext context)
    {
        var eventRecords = evnt.Records ?? new List<S3Event.S3EventNotificationRecord>();
        foreach (var record in eventRecords)
        {
            var s3Event = record.S3;
            if (s3Event == null)
                continue;

            try
            {
                var request = new RecognizeCelebritiesRequest
                {
                    Image = new Image
                    {
                        S3Object = new Amazon.Rekognition.Model.S3Object
                        {
                            Bucket = s3Event.Bucket.Name,
                            Name = s3Event.Object.Key
                        }
                    }
                };

                var result = await RekognitionClient.RecognizeCelebritiesAsync(request);

                var json = JsonSerializer.Serialize(result, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                var putRequest = new PutObjectRequest
                {
                    BucketName = "DEIN-OUT-BUCKET-NAME", // OutBucket Name
                    Key = s3Event.Object.Key + ".json",
                    ContentBody = json
                };

                await S3Client.PutObjectAsync(putRequest);

                context.Logger.LogInformation("Gesichtserkennung abgeschlossen.");
            }
            catch (Exception e)
            {
                context.Logger.LogError("Fehler bei der Verarbeitung:");
                context.Logger.LogError(e.Message);
                throw;
            }
        }
    }
}

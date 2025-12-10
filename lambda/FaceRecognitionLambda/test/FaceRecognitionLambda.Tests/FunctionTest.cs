using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.Lambda.TestUtilities;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using Moq;
using Xunit;

namespace FaceRecognitionLambda.Tests;

public class FunctionTest
{
    [Fact]
    public async Task TestS3EventLambdaFunction()
    {
        var mockS3Client = new Mock<IAmazonS3>();
        var getObjectMetadataResponse = new GetObjectMetadataResponse();
        getObjectMetadataResponse.Headers.ContentType = "text/plain";

        mockS3Client
            .Setup(x => x.GetObjectMetadataAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(getObjectMetadataResponse));

        var mockRekognitionClient = new Mock<IAmazonRekognition>();
        mockRekognitionClient
            .Setup(x => x.RecognizeCelebritiesAsync(
                It.IsAny<RecognizeCelebritiesRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RecognizeCelebritiesResponse());

        var s3Event = new S3Event
        {
            Records = new List<S3Event.S3EventNotificationRecord>
            {
                new S3Event.S3EventNotificationRecord
                {
                    S3 = new S3Event.S3Entity
                    {
                        Bucket = new S3Event.S3BucketEntity { Name = "s3-bucket" },
                        Object = new S3Event.S3ObjectEntity { Key = "text.txt" }
                    }
                }
            }
        };

        ILambdaLogger testLambdaLogger = new TestLambdaLogger();
        var testLambdaContext = new TestLambdaContext
        {
            Logger = testLambdaLogger
        };

        var function = new Function(
            mockS3Client.Object,
            mockRekognitionClient.Object
        );

        await function.FunctionHandler(s3Event, testLambdaContext);

        Assert.Equal("text/plain", ((TestLambdaLogger)testLambdaLogger).Buffer.ToString().Trim());
    }
}

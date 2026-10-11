using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using System.Text.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace OrderApi;

public class Order
{
    public string Id { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string Product { get; set; } = "";
    public int Quantity { get; set; }
    public string Status { get; set; } = "New";
    public DateTime CreatedAt { get; set; }
}

public class CreateOrderRequest
{
    public string? CustomerName { get; set; }
    public string? Product { get; set; }
    public int Quantity { get; set; }
}

public class UpdateStatusRequest
{
    public string? Status { get; set; }
}

public class Function
{
    private static readonly string[] AllowedStatuses = { "New", "Processing", "Completed", "Cancelled" };
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IAmazonS3 _s3 = new AmazonS3Client();
    private readonly IAmazonSQS _sqs = new AmazonSQSClient();
    private readonly string _bucket = Environment.GetEnvironmentVariable("BUCKET_NAME") ?? "";
    private readonly string _queueUrl = Environment.GetEnvironmentVariable("QUEUE_URL") ?? "";

    public async Task<APIGatewayHttpApiV2ProxyResponse> Handler(
        APIGatewayHttpApiV2ProxyRequest request,
        ILambdaContext context)
    {
        try
        {
            switch (request.RouteKey)
            {
                case "POST /orders":
                    return await CreateOrder(request);

                case "GET /orders/{id}":
                    return await GetOrder(request);

                case "PUT /orders/{id}/status":
                    return await UpdateStatus(request);

                default:
                    return Response(404, new { error = "Маршрут не знайдено" });
            }
        }
        catch (JsonException)
        {
            return Response(400, new { error = "Некоректний JSON" });
        }
        catch (Exception ex)
        {
            context.Logger.LogError(ex.ToString());
            return Response(500, new { error = "Внутрішня помилка сервера" });
        }
    }

    private async Task<APIGatewayHttpApiV2ProxyResponse> CreateOrder(APIGatewayHttpApiV2ProxyRequest request)
    {
        var input = JsonSerializer.Deserialize<CreateOrderRequest>(request.Body ?? "", JsonOptions);

        if (input == null ||
            string.IsNullOrWhiteSpace(input.CustomerName) ||
            string.IsNullOrWhiteSpace(input.Product) ||
            input.Quantity <= 0)
        {
            return Response(400, new { error = "Потрібні customerName, product і quantity > 0" });
        }

        var order = new Order
        {
            Id = Guid.NewGuid().ToString(),
            CustomerName = input.CustomerName,
            Product = input.Product,
            Quantity = input.Quantity,
            Status = "New",
            CreatedAt = DateTime.UtcNow
        };

        await SaveOrder(order);

        // Повідомлення в SQS після створення замовлення
        await _sqs.SendMessageAsync(new SendMessageRequest
        {
            QueueUrl = _queueUrl,
            MessageBody = JsonSerializer.Serialize(new
            {
                orderId = order.Id,
                customerName = order.CustomerName,
                product = order.Product,
                quantity = order.Quantity
            }, JsonOptions)
        });

        return Response(201, order);
    }

    private async Task<APIGatewayHttpApiV2ProxyResponse> GetOrder(APIGatewayHttpApiV2ProxyRequest request)
    {
        var id = request.PathParameters["id"];
        var order = await LoadOrder(id);

        return order == null
            ? Response(404, new { error = "Замовлення не знайдено" })
            : Response(200, order);
    }

    private async Task<APIGatewayHttpApiV2ProxyResponse> UpdateStatus(APIGatewayHttpApiV2ProxyRequest request)
    {
        var id = request.PathParameters["id"];
        var input = JsonSerializer.Deserialize<UpdateStatusRequest>(request.Body ?? "", JsonOptions);

        if (input == null || !AllowedStatuses.Contains(input.Status))
        {
            return Response(400, new
            {
                error = $"Статус має бути одним із: {string.Join(", ", AllowedStatuses)}"
            });
        }

        var order = await LoadOrder(id);
        if (order == null)
            return Response(404, new { error = "Замовлення не знайдено" });

        order.Status = input.Status!;
        await SaveOrder(order);

        return Response(200, order);
    }

    private async Task SaveOrder(Order order)
    {
        await _s3.PutObjectAsync(new PutObjectRequest
        {
            BucketName = _bucket,
            Key = $"orders/{order.Id}.json",
            ContentBody = JsonSerializer.Serialize(order, JsonOptions),
            ContentType = "application/json"
        });
    }

    private async Task<Order?> LoadOrder(string id)
    {
        try
        {
            using var response = await _s3.GetObjectAsync(_bucket, $"orders/{id}.json");
            using var reader = new StreamReader(response.ResponseStream);
            var json = await reader.ReadToEndAsync();
            return JsonSerializer.Deserialize<Order>(json, JsonOptions);
        }
        catch (AmazonS3Exception ex) when (ex.ErrorCode == "NoSuchKey")
        {
            return null;
        }
    }

    private static APIGatewayHttpApiV2ProxyResponse Response(int statusCode, object body)
    {
        return new APIGatewayHttpApiV2ProxyResponse
        {
            StatusCode = statusCode,
            Body = JsonSerializer.Serialize(body, JsonOptions),
            Headers = new Dictionary<string, string> { ["Content-Type"] = "application/json" }
        };
    }
}
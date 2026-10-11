using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

const string ApiUrl = "https://4xzeli51pa.execute-api.eu-central-1.amazonaws.com";

using var http = new HttpClient { BaseAddress = new Uri(ApiUrl) };
var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);


while (true)
{
    ShowMenu();
    var choice = Console.ReadLine();

    Console.Clear();

    try
    {
        var exit = await HandleMenuChoice(choice, http, jsonOptions);
        if (exit) break;
    }
    catch (HttpRequestException ex)
    {
        PrintColored($"\n[Помилка мережі]: Не вдалося звернутися до API -> {ex.Message}", ConsoleColor.Red);
    }

    PrintDivider();
    Console.WriteLine("Натисніть будь-яку клавішу, щоб продовжити...");
    Console.ReadKey();
    Console.Clear();
}

static void ShowMenu()
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("========================================");
    Console.WriteLine("        📦 ORDER MANAGEMENT API         ");
    Console.WriteLine("========================================");
    Console.ResetColor();
    Console.WriteLine(" 1. Створити нове замовлення");
    Console.WriteLine(" 2. Отримати замовлення за ID");
    Console.WriteLine(" 3. Змінити статус замовлення");
    Console.WriteLine(" 0. Вихід");
    Console.WriteLine("----------------------------------------");
    Console.Write("Ваш вибір: ");
}

static async Task<bool> HandleMenuChoice(string? choice, HttpClient http, JsonSerializerOptions jsonOptions)
{
    switch (choice)
    {
        case "1":
            await CreateOrderAsync(http, jsonOptions);
            break;
        case "2":
            await GetOrderAsync(http, jsonOptions);
            break;
        case "3":
            await UpdateStatusAsync(http, jsonOptions);
            break;
        case "0":
            PrintColored("Дякуємо за використання програми! До побачення 👋", ConsoleColor.Green);
            return true;
        default:
            PrintColored("Невідомий пункт меню. Будь ласка, оберіть від 0 до 3.", ConsoleColor.Yellow);
            break;
    }
    return false;
}

static async Task CreateOrderAsync(HttpClient http, JsonSerializerOptions jsonOptions)
{
    PrintHeader("СТВОРЕННЯ ЗАМОВЛЕННЯ");

    Console.Write("Ім'я клієнта: ");
    var customerName = Console.ReadLine()?.Trim() ?? "";
    while (string.IsNullOrEmpty(customerName))
    {
        PrintColored("Ім'я не може бути порожнім. Спробуйте ще раз:", ConsoleColor.Yellow);
        Console.Write("Ім'я клієнта: ");
        customerName = Console.ReadLine()?.Trim() ?? "";
    }

    Console.Write("Товар: ");
    var product = Console.ReadLine()?.Trim() ?? "";
    while (string.IsNullOrEmpty(product))
    {
        PrintColored("Назва товару не може бути порожньою. Спробуйте ще раз:", ConsoleColor.Yellow);
        Console.Write("Товар: ");
        product = Console.ReadLine()?.Trim() ?? "";
    }

    int quantity;
    while (true)
    {
        Console.Write("Кількість: ");
        var input = Console.ReadLine();
        if (int.TryParse(input, out quantity) && quantity > 0)
        {
            break;
        }
        PrintColored("Помилка: кількість має бути додатним цілим числом. Спробуйте ще раз.", ConsoleColor.Red);
    }

    var response = await http.PostAsJsonAsync("/orders", new { customerName, product, quantity });
    await PrintResponseAsync(response, jsonOptions);
}

static async Task GetOrderAsync(HttpClient http, JsonSerializerOptions jsonOptions)
{
    PrintHeader("ПОШУК ЗАМОВЛЕННЯ ЗА ID");

    string id;
    while (true)
    {
        Console.Write("Введіть ID замовлення: ");
        id = Console.ReadLine()?.Trim() ?? "";
        if (!string.IsNullOrEmpty(id))
        {
            break;
        }
        PrintColored("ID не може бути порожнім. Спробуйте ще раз.", ConsoleColor.Red);
    }

    var response = await http.GetAsync($"/orders/{Uri.EscapeDataString(id)}");
    await PrintResponseAsync(response, jsonOptions);
}

static async Task UpdateStatusAsync(HttpClient http, JsonSerializerOptions jsonOptions)
{
    PrintHeader("ЗМІНА СТАТУСУ ЗАМОВЛЕННЯ");

    string id;
    while (true)
    {
        Console.Write("Введіть ID замовлення: ");
        id = Console.ReadLine()?.Trim() ?? "";
        if (!string.IsNullOrEmpty(id))
        {
            break;
        }
        PrintColored("ID не може бути порожнім. Спробуйте ще раз.", ConsoleColor.Red);
    }

    Console.WriteLine("\nДоступні статуси: New, Processing, Completed, Cancelled");

    string status;
    while (true)
    {
        Console.Write("Новий статус: ");
        status = Console.ReadLine()?.Trim() ?? "";

        if (status is "New" or "Processing" or "Completed" or "Cancelled")
        {
            break;
        }
        PrintColored("Невірний статус. Оберіть один із: New, Processing, Completed, Cancelled", ConsoleColor.Red);
    }

    var response = await http.PutAsJsonAsync($"/orders/{Uri.EscapeDataString(id)}/status", new { status });
    await PrintResponseAsync(response, jsonOptions);
}

static async Task PrintResponseAsync(HttpResponseMessage response, JsonSerializerOptions jsonOptions)
{
    var body = await response.Content.ReadAsStringAsync();
    Console.WriteLine();

    if (response.IsSuccessStatusCode)
    {
        var order = JsonSerializer.Deserialize<Order>(body, jsonOptions)!;

        PrintColored("[УСПІХ] Запит виконано успішно!", ConsoleColor.Green);
        Console.WriteLine($"----------------------------------------");
        Console.WriteLine($" ID:        {order.Id}");
        Console.WriteLine($" Клієнт:    {order.CustomerName}");
        Console.WriteLine($" Товар:     {order.Product}");
        Console.WriteLine($" Кількість: {order.Quantity}");

        Console.Write(" Статус:    ");
        PrintColored($"{order.Status}", ConsoleColor.Yellow);

        Console.WriteLine($" Створено:  {order.CreatedAt:dd.MM.yyyy HH:mm:ss} UTC");
        Console.WriteLine($"----------------------------------------");
    }
    else
    {
        string error = body;
        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("error", out var e))
                error = e.GetString() ?? body;
        }
        catch (JsonException) { }

        PrintColored($"[ПОМИЛКА {(int)response.StatusCode}]: {error}", ConsoleColor.Red);
    }
}

static void PrintHeader(string title)
{
    Console.ForegroundColor = ConsoleColor.Magenta;
    Console.WriteLine($"\n--- {title} ---");
    Console.ResetColor();
}

static void PrintColored(string text, ConsoleColor color)
{
    Console.ForegroundColor = color;
    Console.WriteLine(text);
    Console.ResetColor();
}

static void PrintDivider()
{
    Console.WriteLine("========================================");
}

record Order(
    string Id,
    string CustomerName,
    string Product,
    int Quantity,
    string Status,
    DateTime CreatedAt);
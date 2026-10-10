# AWS Serverless Order API

Курсовий/екзаменаційний проєкт з дисципліни **«Створення хмарних рішень з використанням AWS»** (Варіант 4).  
**Автор:** Тімченко Микола (Група П42)  
**Викладач:** Сиворка В. А.

---

## Опис проєкту

Даний проєкт є безсерверним (Serverless) бекендом для обробки та збереження замовлень. Вся інфраструктура працює виключно на хмарних ресурсах AWS без використання виділених серверів.

### Як працює система:
1. **API Gateway** отримує зовнішні HTTP-запити від клієнта.
2. **AWS Lambda** (на платформі .NET 8) обробляє бізнес-логіку.
3. **Amazon S3** виступає основним сховищем — кожне замовлення зберігається у вигляді JSON-документа.
4. **Amazon SQS** приймає фонові сповіщення про нові створені замовлення.
5. **Console Client** — зручна C# програма для тестування CRUD-операцій з консолі.

---

##  Задействовані AWS Сервіси

- **AWS Lambda (`OrderApi`)** — хмарна функція на C#, яка реалізує ендпоїнти.
- **Amazon API Gateway** — маршрутизатор HTTP-запитів до лямбди.
- **Amazon S3** — бакет для файлового збереження замовлень (папка `orders/`).
- **Amazon SQS** — черга повідомлень для обробки подій створення замовлень.
- **IAM (Identity and Access Management)** — політики доступу для безпечної взаємодії Lambda з S3 та SQS.
- **CloudWatch** — сервіс для логування відлагоджувальної інформації.

---

##  Схема архітектури
[ C# Console Client ] ──(HTTP)──> [ API Gateway ] ──> [ AWS Lambda ] ──┬──> [ Amazon S3 (JSON) ]
└──> [ Amazon SQS Queue ]


> **Примітка щодо безпеки:** Консольний клієнт не містить секретних ключів AWS (Access Keys). Доступ Lambda до ресурсів S3 та SQS регулюється IAM-роллю, а назви ресурсів передаються через змінні оточення (`BUCKET_NAME`, `QUEUE_URL`).

---

## Робочий API Endpoint

[https://4xzeli51pa.execute-api.eu-central-1.amazonaws.com](https://4xzeli51pa.execute-api.eu-central-1.amazonaws.com)


Доступні маршрути:
POST /orders — створити нове замовлення (за замовчуванням статус New).

GET /orders/{id} — завантажити замовлення за його ID.

PUT /orders/{id}/status — змінити статус існуючого замовлення.

Можливі статуси замовлень: New, Processing, Shipped, Delivered, Cancelled. 

Приклади запитів та відповідей

## Приклади запитів

Створити замовлення:

```bash
curl -X POST https://4xzeli51pa.execute-api.eu-central-1.amazonaws.com/orders -H "Content-Type: application/json" -d "{\"customerName\":\"Oleg\",\"product\":\"laptop\",\"quantity\":2}"
```

Що повертає:

```json
{
  "id": "36722322-6d67-4257-b5e8-623f636a767b",
  "customerName": "Oleg",
  "product": "laptop",
  "quantity": 2,
  "status": "New",
  "createdAt": "2026-10-10T18:49:02Z"
}
```

Отримати замовлення (можна просто відкрити в браузері):

```bash
curl https://4xzeli51pa.execute-api.eu-central-1.amazonaws.com/orders/36722322-6d67-4257-b5e8-623f636a767b
```

Змінити статус:

```bash
curl -X PUT https://4xzeli51pa.execute-api.eu-central-1.amazonaws.com/orders/36722322-6d67-4257-b5e8-623f636a767b/status -H "Content-Type: application/json" -d "{\"status\":\"Processing\"}"
```


  Структура проєкту
OrderApi/ — джерельні файли бекенду AWS Lambda (.NET 8).

OrderClient/ — вихідний код консольного клієнта для тестування API.


Як розгортати та запускати
Бекенд (AWS Lambda):

Виконати комбінацію команд: dotnet publish -c Release

Архівувати публікацію в .zip та завантажити в консоль AWS Lambda.

Задати змінні оточення BUCKET_NAME і QUEUE_URL.

Налаштувати тригери у API Gateway.

Запуск клієнта:

Відкрити рішення у Visual Studio.

Запустити проєкт OrderClient.

Використовувати інтерактивне меню (1 — створити, 2 — знайти, 3 — змінити статус, 0 — вихід).

Скріншоти підтвердження роботи

1. Функція AWS Lambda
<img width="1903" height="903" alt="Снимок экрана 2026-10-10 203855" src="https://github.com/user-attachments/assets/ada3dd15-c626-4e09-888c-84bea6ed8425" />

2. Маршрутизація в API Gateway
<img width="1919" height="924" alt="Снимок экрана 2026-10-10 204025" src="https://github.com/user-attachments/assets/429f3fed-4d80-4cbb-ab33-c68ae39b5903" />

3. Файли замовлень в Amazon S3
<img width="1919" height="954" alt="Снимок экрана 2026-10-10 204117" src="https://github.com/user-attachments/assets/ed2f6c5e-03db-40d5-9c64-c233713041a4" />

4. Черга повідомлень Amazon SQS
<img width="1893" height="944" alt="Снимок экрана 2026-10-10 204205" src="https://github.com/user-attachments/assets/a1222685-8b93-4bfa-b7e1-9746683c9829" />

5. Робота C# Консольного клієнта
<img width="1090" height="603" alt="Снимок экрана 2026-10-10 214842" src="https://github.com/user-attachments/assets/0c1e9064-0e7b-4e82-b635-96abf1f94359" />
<img width="604" height="393" alt="Снимок экрана 2026-10-10 214917" src="https://github.com/user-attachments/assets/0f6ae853-2942-4b7d-b5df-0241d7c06103" />
<img width="1116" height="580" alt="Снимок экрана 2026-10-10 215011" src="https://github.com/user-attachments/assets/89f3b54f-b41f-474e-bf77-7b1f4e01f3a6" />
<img width="1098" height="587" alt="Снимок экрана 2026-10-10 215101" src="https://github.com/user-attachments/assets/fd9dd6ea-63bd-47d8-a601-dc2f356af6a4" />

6. Перевірка GET-запиту в браузері
<img width="437" height="228" alt="image" src="https://github.com/user-attachments/assets/90a705a1-4498-4557-9bf7-87bc46758019" />








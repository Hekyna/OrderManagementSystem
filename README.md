# Serverless Order API

Екзаменаційна робота з дисципліни **«Створення хмарних рішень з використанням AWS»** (Варіант 4).  
**Виконав:** Тімченко Микола, група П42  
**Викладач:** Сиворка В. А.

---

## 📌 Опис проєкту

Система для управління замовленнями, побудована за безсерверною архітектурою (Serverless) на базі сервісів AWS. Вхідні HTTP-запити приймає **API Gateway** та передає їх у хмарну функцію **AWS Lambda**. Вона обробляє бізнес-логіку, зберігає кожне замовлення у бакет **Amazon S3** у вигляді JSON-файлу, а при створенні нового замовлення надсилає повідомлення в чергу **Amazon SQS**.

Для тестування розроблено консольний клієнт на C# (.NET 8) з інтерактивним меню.

---

## 🛠 Використані сервіси AWS

- **AWS Lambda** — функція `OrderApi` (.NET 8), яка виконує бізнес-логіку обробки замовлень.
- **Amazon API Gateway (HTTP API)** — шлюз із 3 маршрутами для прийому зовнішніх запитів.
- **Amazon S3** — файлове сховище (структура `orders/{id}.json`).
- **Amazon SQS** — черга повідомлень для подій створення замовлення.
- **IAM** — політики безпеки та ролі для доступу Lambda до S3 та SQS.
- **CloudWatch** — логування дій та відлагодження.

---

## 🏗 Архітектура системи

```text
[ C# Console Client ] ──(HTTP)──> [ API Gateway ] ──> [ AWS Lambda ] ──┬──> [ Amazon S3 ]
                                                                       └──> [ Amazon SQS ]
```

> **Безпека:** Клієнт взаємодіє виключно через HTTPS і не містить секретних ключів AWS. Доступ Lambda до S3 та SQS забезпечується через IAM-роль, а параметри передаються через змінні оточення (`BUCKET_NAME`, `QUEUE_URL`).

---

## 🌐 Робочий URL API

```text
[https://4xzeli51pa.execute-api.eu-central-1.amazonaws.com](https://4xzeli51pa.execute-api.eu-central-1.amazonaws.com)
```

### Доступні маршрути:
- `POST /orders` — створити нове замовлення (початковий статус `New`).
- `GET /orders/{id}` — отримати замовлення за його `id`.
- `PUT /orders/{id}/status` — змінити статус замовлення.

*Можливі статуси:* `New`, `Processing`, `Shipped`, `Delivered`, `Cancelled`.

---

## 💻 Приклади запитів

Створити замовлення:

```bash
curl -X POST [https://4xzeli51pa.execute-api.eu-central-1.amazonaws.com/orders](https://4xzeli51pa.execute-api.eu-central-1.amazonaws.com/orders) -H "Content-Type: application/json" -d "{\"customerName\":\"Oleg\",\"product\":\"laptop\",\"quantity\":2}"
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
curl [https://4xzeli51pa.execute-api.eu-central-1.amazonaws.com/orders/36722322-6d67-4257-b5e8-623f636a767b](https://4xzeli51pa.execute-api.eu-central-1.amazonaws.com/orders/36722322-6d67-4257-b5e8-623f636a767b)
```

Змінити статус:

```bash
curl -X PUT [https://4xzeli51pa.execute-api.eu-central-1.amazonaws.com/orders/36722322-6d67-4257-b5e8-623f636a767b/status](https://4xzeli51pa.execute-api.eu-central-1.amazonaws.com/orders/36722322-6d67-4257-b5e8-623f636a767b/status) -H "Content-Type: application/json" -d "{\"status\":\"Processing\"}"
```

---

## 📁 Структура проєкту

- **OrderApi** — проєкт AWS Lambda (Handler: `OrderApi::OrderApi.Function::Handler`).
- **OrderClient** — консольний застосунок для взаємодії з API.

---

## 🚀 Як запустити

1. **Задеплоїти Lambda:**
   - Виконати збірку: `dotnet publish -c Release`
   - Запакувати вміст папки публікації в ZIP та завантажити в консоль AWS Lambda.
   - Прописати змінні оточення `BUCKET_NAME` та `QUEUE_URL`.
   - Налаштувати тригери у шлюзі **API Gateway**.

2. **Запустити консольний клієнт:**
   - Відкрити рішення у Visual Studio.
   - Встановити проєкт **OrderClient** як стартовий і запустити.
   - У меню вибирати відповідні дії (1 — створити, 2 — знайти за Id, 3 — змінити статус, 0 — вихід).

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








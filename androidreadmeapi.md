# راهنمای اتصال کلاینت‌های موبایل (Android/iOS)

با توجه به معماری مونوکیت MVC پروژه، برای اتصال کلاینت‌های موبایل در آینده، توسعه‌دهندگان باید استراتژی‌های زیر را دنبال کنند.

## شبیه‌سازی احراز هویت مبتنی بر کوکی

سامانه از Cookie Authentication استفاده می‌کند. کلاینت اندروید باید مکانیزم ذخیره و ارسال کوکی (Cookie Persistence) را مدیریت کند:

۱. **Login:** درخواست POST به `/Account/Login` ارسال شود. در پاسخ، هدر `Set-Cookie` حاوی توکن احراز هویت است.
۲. **Intercepting:** توسعه‌دهنده موبایل باید از یک `CookieJar` (مثلاً در OkHttp) برای ذخیره این کوکی استفاده کند.
۳. **Subsequent Requests:** در تمام درخواست‌های بعدی، کوکی ذخیره شده باید در هدر `Cookie` ارسال شود تا سرور کاربر را شناسایی کند.

## اتصال به SignalR در موبایل

برای ردیابی زنده وضعیت سفارشات در اپلیکیشن اندروید، از کتابخانه رسمی SignalR Java/Kotlin Client استفاده کنید:

```kotlin
val hubConnection = HubConnectionBuilder.create("http://YOUR_SERVER_IP/orderHub").build()

hubConnection.on("ReceiveStatusUpdate", { orderId, status ->
    // به‌روزرسانی رابط کاربری اپلیکیشن
}, Int::class.java, String::class.java)

hubConnection.start()
```

## دریافت داده‌های منو

از آنجایی که متدهای فعلی Partial View برمی‌گردانند، برای نسخه موبایل دو راهکار وجود دارد:
۱. اضافه کردن اکشن‌های جدید با خروجی `JsonResult`.
۲. استفاده از هدر `Accept: application/json` در درخواست‌ها و مدیریت آن در سمت کنترلر برای بازگرداندن داده خام به جای HTML.

## نکات امنیتی
- تمامی ارتباطات در محیط عملیاتی باید بر بستر HTTPS باشد.
- کوکی‌ها باید دارای اتریبیوت `HttpOnly` و `Secure` باشند (که در تنظیمات Program.cs لحاظ شده است).

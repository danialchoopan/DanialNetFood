# مستندات معماری داخلی DanialNetFood

این سند به تشریح ساختار داخلی، کنترلرها و هاب SignalR پروژه DanialNetFood می‌پردازد.

## ساختار کنترلرها و روت‌های اصلی

### HomeController
- Index: نمایش لیست تمامی رستوران‌ها.
- Menu: نمایش منوی اختصاصی هر رستوران بر اساس شناسه.

### CartController
- AddToCart (POST): افزودن غذا به سبد خرید در Session و بازگرداندن Partial View.
- RemoveFromCart (POST): حذف غذا از سبد خرید در Session.
- Index: مشاهده محتویات سبد خرید.

### OrderController
- Checkout: نمایش صفحه نهایی کردن سفارش و اعمال تخفیف.
- ApplyDiscount (POST): بررسی و اعمال کد تخفیف با استفاده از Strategy Pattern.
- PlaceOrder (POST): ثبت نهایی سفارش در دیتابیس با استفاده از Unit of Work.
- Track: صفحه پیگیری وضعیت سفارش به صورت زنده.
- UpdateStatus (POST): متد مخصوص ادمین برای تغییر وضعیت سفارش.

## مستندات SignalR (OrderHub)

برای ردیابی زنده سفارشات از SignalR استفاده شده است.

### متدهای سمت سرور (Hub)
- JoinOrderGroup(orderId): کلاینت را به گروه اختصاصی یک سفارش اضافه می‌کند.
- UpdateStatus(orderId, status): وضعیت سفارش را به تمامی اعضای گروه اطلاع‌رسانی می‌کند.

### رویدادهای سمت کلاینت
- ReceiveStatusUpdate: این رویداد زمانی که وضعیت سفارش تغییر کند فراخوانی شده و UI را آپدیت می‌کند.

## مدیریت نشست (Session)
اطلاعات سبد خرید در `HttpContext.Session` به صورت JSON سریالایز شده با کلید `UserCart` ذخیره می‌شود. ساختار داده شامل لیستی از غذاها، تعداد و قیمت واحد است.

from playwright.sync_api import sync_playwright, expect
import time

def verify_danialnetfood():
    with sync_playwright() as p:
        browser = p.chromium.launch(headless=True)
        context = browser.new_context(viewport={'width': 1280, 'height': 800})
        page = context.new_page()

        try:
            # 1. Verify Login Page Hints
            print("Checking login page...")
            page.goto("http://localhost:5000/Account/Login")
            expect(page.get_by_text("customer / 123456")).to_be_visible()
            page.screenshot(path="DanialNetFood/DanialNetFood.Web/screenshots/verify_login.png")

            # 2. Login as SuperAdmin
            print("Logging in as admin...")
            page.fill("input[name='username']", "admin")
            page.fill("input[name='password']", "123456")
            page.click("button[type='submit']")

            # Wait for dashboard
            expect(page).to_have_url("http://localhost:5000/SuperAdmin/Dashboard")
            print("Dashboard loaded.")

            # 3. Verify Dashboard Chart
            # Wait for AJAX
            time.sleep(2)
            page.screenshot(path="DanialNetFood/DanialNetFood.Web/screenshots/verify_dashboard.png")

            # 4. Verify Kill Switch
            print("Checking Kill Switch...")
            page.goto("http://localhost:5000/SuperAdmin/KillSwitch")
            expect(page.get_by_role("button", name="قطع موقت خدمات")).to_be_visible()
            page.screenshot(path="DanialNetFood/DanialNetFood.Web/screenshots/verify_killswitch.png")

            # 5. Verify Cart Sidebar (it should be empty initially but visible)
            print("Checking Cart Sidebar...")
            page.goto("http://localhost:5000/")
            expect(page.locator("#cart-sidebar")).to_be_visible()
            expect(page.get_by_text("سبد خرید شما فعلاً خالی است")).to_be_visible()
            page.screenshot(path="DanialNetFood/DanialNetFood.Web/screenshots/verify_home_cart.png")

        except Exception as e:
            print(f"Error: {e}")
            page.screenshot(path="DanialNetFood/DanialNetFood.Web/screenshots/verify_error.png")
        finally:
            browser.close()

if __name__ == "__main__":
    verify_danialnetfood()

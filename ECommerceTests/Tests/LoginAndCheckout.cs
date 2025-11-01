using System.Threading;
using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using NUnit.Framework;

namespace ECommerceTests.Tests
{
    [TestFixture]
    public class RegressionTests : IDisposable
    {
        private IWebDriver? driver;
        private WebDriverWait? wait;

        [SetUp]
        public void Setup()
        {
            Console.WriteLine("Starting Chrome (VISIBLE) with remote debugging...");

            var service = ChromeDriverService.CreateDefaultService();
            service.SuppressInitialDiagnosticInformation = true;

            var options = new ChromeOptions();
            // REMOVE --headless=new
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--window-size=1920,1080");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--disable-extensions");
            options.AddArgument("--remote-debugging-port=9292"); // Critical for localhost

            driver = new ChromeDriver(service, options);
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15)); // Increased timeout
        }

        [TearDown]
        public void TearDown()
        {
            driver?.Dispose();
        }

        public void Dispose() => TearDown();

        [Test]
        [Description("UAT-001: Valid user can login and complete checkout")]
        public void Checkout_Success()
        {
            driver!.Navigate().GoToUrl("http://localhost:5050");

            // Wait for login field
            var login = wait!.Until(d => d.FindElement(By.Id("login")));
            login.Clear();
            Thread.Sleep(1000);
            login.SendKeys("test@user.com");

            var password = driver.FindElement(By.Id("password"));
            password.Clear();
            Thread.Sleep(1000);
            password.SendKeys("Pass123!");

            Thread.Sleep(1000);
            // Click Login → handle alert
            driver.FindElement(By.Id("submit")).Click();
            wait.Until(ExpectedConditions.AlertIsPresent());
            Thread.Sleep(1000);
            driver.SwitchTo().Alert().Accept();

            // Click Add to Cart → handle alert
            Thread.Sleep(1500); // Pause before next action
            driver.FindElement(By.Id("add-to-cart-42")).Click();
            Thread.Sleep(1000);
            wait.Until(ExpectedConditions.AlertIsPresent());
            driver.SwitchTo().Alert().Accept();
            Thread.Sleep(1000);
            // Click Checkout
            driver.FindElement(By.Id("checkout")).Click();

            // WAIT FOR RESULT TO BE VISIBLE
            var result = wait.Until(ExpectedConditions.ElementIsVisible(By.Id("result")));
            Thread.Sleep(2000);
            Assert.That(result.Displayed, Is.True);
            Assert.That(result.Text, Is.EqualTo("Order Confirmed"));
            Thread.Sleep(2000);
        }   
    }
}
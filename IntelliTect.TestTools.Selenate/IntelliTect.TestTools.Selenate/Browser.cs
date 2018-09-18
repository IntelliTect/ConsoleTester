﻿using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.TestTools.Selenate
{
    public enum BrowserType
    {
        Chrome,
        InternetExplorer
    }
    public class Browser
    {
        public Browser(BrowserType browser)
        {
            Driver = InitDriver(browser);
        }

        public IWebDriver Driver { get; }

        // Mike C: Find a good way to abstract this out. Different projects will have different requirements here.
        // Good candidate for an extension? Or maybe an abstract class?
        public IWebDriver InitDriver(BrowserType browser)
        {
            Driver?.Quit();

            IWebDriver driver = null;

            switch (browser)
            {
                case BrowserType.Chrome:
                    ChromeOptions chromeOptions = new ChromeOptions();
                    chromeOptions.AddArgument("--disable-extension");
                    chromeOptions.AddArgument("no-sandbox");
                    chromeOptions.AddArgument("disable-infobars");
                    chromeOptions.AddUserProfilePreference("credentials_enable_service", false);
                    chromeOptions.AddUserProfilePreference("profile.password_manager_enabled", false);
                    driver = new ChromeDriver(Directory.GetCurrentDirectory(), chromeOptions, TimeSpan.FromMinutes(1));

                    break;
                case BrowserType.InternetExplorer:
                    InternetExplorerOptions ieCaps = new InternetExplorerOptions
                    {
                        EnablePersistentHover = true,
                        EnsureCleanSession = true,
                        EnableNativeEvents = true,
                        IgnoreZoomLevel = true,
                        IntroduceInstabilityByIgnoringProtectedModeSettings = true,
                        RequireWindowFocus = false
                    };
                    driver = new InternetExplorerDriver(ieCaps);
                    break;
                default:
                    throw new ArgumentException($"Unknown browser: {browser}");
            }

            driver.Manage().Window.Maximize();
            driver.Manage().Timeouts().PageLoad = TimeSpan.FromMinutes(2);
            return driver;
        }

        /// <summary>
        /// Wraps the Selenium Driver's native web element to wait until the element exists before returning.
        /// If you need to verify an element DOESN'T exist, then call Browser.Driver.FindElement directly.
        /// </summary>
        /// <param name="by">Selenium "By" statement to find the element</param>
        /// <param name="secondsToWait">Seconds to wait while retrying before failing</param>
        /// <returns></returns>
		public Task<IWebElement> FindElement(By by, int secondsToWait = 5)
        {
            Console.WriteLine($"Attempting to find element using selector: {by}");
            ConditionalWait wait = new ConditionalWait();
            return wait.WaitForSeconds<NoSuchElementException, IWebElement>(() => Driver.FindElement(by));
        }

        /// <summary>
        /// Wraps the Selenium Driver's native web element to wait until at least one element exists before returning.
        /// If you need to verify an element DOESN'T exist, then call Browser.Driver.FindElements directly.
        /// </summary>
        /// <param name="by">Selenium "By" statement to find the element</param>
        /// <param name="secondsToWait">Seconds to wait while retrying before failing</param>
        /// <returns></returns>
        public IReadOnlyCollection<WebElement> FindElements(By by, int secondsToWait = 5)
        {
            Console.WriteLine($"Attempting to find all elements using selector: {by}");

            // Eventually swap this out for our own wait
            WebDriverWait wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(secondsToWait));
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(by));

            return new ReadOnlyCollection<WebElement>(
                            Driver.FindElements(by)
                                    .Select(webElement => new WebElement(webElement, by, Driver))
                                    .ToList());
        }

        /// <summary>
        ///Waits until a function evaluates to true OR times out after a specified period of time
        /// </summary>
        /// <param name="func">Function to evaluate</param>
        /// <param name="secondsToWait">Secondes to wait until timeout / return false</param>
        /// <returns></returns>
        public Task<bool> WaitFor(Func<bool> func, int secondsToWait = 15)
        {
            ConditionalWait wait = new ConditionalWait();
            return wait.WaitForSeconds<
                NoSuchElementException,
                StaleElementReferenceException,
                ElementNotVisibleException,
                InvalidElementStateException,
                bool>(func);
        }

        /// <summary>
        /// Switches to each frame in succession to avoid having to explicitely call SwitchTo() multipled times for nested frames
        /// </summary>
        /// <param name="bys"></param>
        /// <returns></returns>
        public async Task FrameSwitchAttempt(params By[] bys)
        {
            // Note, some applications (i.e. CCB) will break out of switching to a frame if it's still loading.
            // See if restarting the whole search like we currently do on PTT is necessary, or if we can just wait for something to finish loading
            ConditionalWait wait = new ConditionalWait();
            foreach (By by in bys)
            {
                IWebElement element = Driver.FindElement(by);
                await wait.WaitForSeconds<
                            NoSuchFrameException,
                            InvalidOperationException,
                            StaleElementReferenceException,
                            NotFoundException>
                        (() => Driver.SwitchTo().Frame(element));
            }
        }

        public async Task<bool> SwitchWindow(string title)
        {
            ConditionalWait wait = new ConditionalWait();
            string currentWindow =
                    wait.WaitForSeconds<NoSuchWindowException, string>(() => Driver.CurrentWindowHandle).GetAwaiter().GetResult();

            var availableWindows = new List<string>(Driver.WindowHandles);

            foreach (string w in availableWindows)
            {
                if (w != currentWindow)
                {
                    await wait.WaitForSeconds<NoSuchWindowException>(() => Driver.SwitchTo().Window(w));
                    if ( Driver.Title == title )
                    {
                        return true;
                    }
                    else
                    {
                        Driver.SwitchTo().Window( currentWindow );
                    }
                }
            }
            return false;
        }

        public Task<IAlert> Alert(int numberOfRetries = 50)
        {
            ConditionalWait wait = new ConditionalWait();
            return wait.WaitForSeconds<
                NoAlertPresentException,
                UnhandledAlertException,
                IAlert>
                (() => Driver.SwitchTo().Alert());
        }

        public void TakeScreenshot()
        {
            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "screenshot", $"{((RemoteWebDriver)this.Driver).Capabilities.BrowserName}_{DateTime.Now:yyyy.MM.dd_hh.mm.ss}");
            Console.WriteLine($"Saving screenshot to location: {fullPath}");
            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "screenshot"));
            if (Driver is ITakesScreenshot takeScreenshot)
            {
                Screenshot screenshot = takeScreenshot.GetScreenshot();
                screenshot?.SaveAsFile(fullPath, ScreenshotImageFormat.Png);
            }
        }
    }
}

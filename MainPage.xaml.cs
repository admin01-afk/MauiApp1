using System;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using Microsoft.Maui.Dispatching;
using System.Diagnostics;

namespace MauiApp1 {
    public partial class MainPage : ContentPage {
        public MainPage()
        {
            InitializeComponent();

            Task.Run(async () => {
                GetData("https://mf.eskisehir.edu.tr/tr/Duyuru");
            });
        }

        async Task GetData(string url)
        {
            MainThread.BeginInvokeOnMainThread(() => {
                console.Text = "Fetching data...";
                progressbar.IsVisible = true;
                progressbar.Progress = 0.01;
            });
            try {
                // FirefoxOptions for headless operation and optional image disabling for faster loading
                FirefoxOptions options = new FirefoxOptions();
                options.AddArgument("--headless");  // Optional: Run in headless mode
                options.AddArgument("--disable-images");  // Optional: Disable images for faster loading

                var driverService = FirefoxDriverService.CreateDefaultService();
                driverService.HideCommandPromptWindow = true;

                using IWebDriver driver = new FirefoxDriver(driverService,options);
                driver.Navigate().GoToUrl(url);

                // Wait for the page to load completely
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

                // Scroll the page to trigger more content loading (if necessary)
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                js.ExecuteScript("window.scrollTo(0, document.body.scrollHeight/2);");
                await Task.Delay(3000);  // Wait a bit for content to load after scrolling

                // Wait for the element to load using the provided XPath
                IWebElement parentDiv = wait.Until(d => d.FindElement(By.XPath("/html/body/div[5]/div/div[2]/div/div/div/div/div[2]/div/div/div[1]")));
                IReadOnlyCollection<IWebElement> divsInsideParent = parentDiv.FindElements(By.XPath(".//div[contains(@class, 'gdlr-core-blog-full-frame gdlr-core-skin-e-background')]"));
                int i = 0;
                foreach (IWebElement div in divsInsideParent){
                    string text = $"Duyuru{i}: " + div.FindElement(By.XPath("div[1]/div/h3/a")).Text + "\n";
                    CreateElement(text);
                    i++;
                    double progressValue = (double)i / 10;
                    MainThread.BeginInvokeOnMainThread(() => {
                        console.Text = $"Data-{i} Fetched";
                        progressbar.ProgressTo(progressValue,200,Easing.Default);
                    });
                }
                MainThread.BeginInvokeOnMainThread(() => {
                    console.Text = "";
                    progressbar.Progress = 0;
                    progressbar.IsVisible = false;
                });
                driver.Quit();  // Ensure the driver quits after operation
            } catch (Exception ex) {
                MainThread.BeginInvokeOnMainThread(() => {
                    console.Text = ex.Message;
                });
            }
        }

        public HorizontalStackLayout CreateElement(string text)
        {
            HorizontalStackLayout element = new HorizontalStackLayout { 
                Spacing=10,
            };

            Button button = new Button {
                Text = "link",
            };

            Label label = new Label {
                Text = text,
                FontSize = 12,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
            element.Add(label);
            element.Add(button);

            MainThread.BeginInvokeOnMainThread(() => {
                list.Add(element);
            });

            button.Clicked += Button_Clicked;

            return element;
        }
        private void Button_Clicked(object sender, EventArgs e)
        {
            Task.Run(async () => {
                FirefoxOptions options = new FirefoxOptions();
                using IWebDriver driver = new FirefoxDriver(options);
                driver.Navigate().GoToUrl("https://mf.eskisehir.edu.tr/tr/Duyuru");
            });
        }

        private void Collapse_btn_Clicked(object sender, EventArgs e)
        {
            list.IsVisible = !list.IsVisible;
            btn.Text = (btn.Text == "<") ? ">" : "<";
        }
    }
}

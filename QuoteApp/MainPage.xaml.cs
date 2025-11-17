namespace QuoteApp;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using OpenAI;
using OpenAI.Chat;
using System;
using System.ClientModel;
using System.ClientModel.Primitives;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static QuoteApp.MainPage;
using static System.Runtime.InteropServices.JavaScript.JSType;

public partial class MainPage : ContentPage
{

    private readonly Random _random = new();
    private List<Quote> _quotes;
    private Quote _quote;
    private readonly ChatClient _client;
    public class QuoteData
    {
        public List<Quote> quotes { get; set; }
    }

    public class Quote
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("text")]
        public string Text { get; set; }
        [JsonPropertyName("author")]
        public string Author { get; set; }
        [JsonPropertyName("era")]
        public string Era { get; set; }
        [JsonPropertyName("year")]
        public string Year { get; set; }
        [JsonPropertyName("origin")]
        public string Origin { get; set; }
        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; }
        [JsonPropertyName("context_prompt")]
        public string ContextPrompt { get; set; }

    }

    public MainPage()
    {
        InitializeComponent();
        string OPENAI_API_KEY = "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";
        _client = new(model: "gpt-4o-mini", apiKey: OPENAI_API_KEY);
        _ = LoadQuotes();
    }

    private void OnNewQuoteClicked(object sender, EventArgs e)
    {
        ShowRandomQuote();
    }

    private async Task LoadQuotes()
    {
        try
        {
            using Stream fileStream = await FileSystem.Current.OpenAppPackageFileAsync("classical_era_quotes.json");
            using StreamReader reader = new(fileStream);
            string jsonContent = await reader.ReadToEndAsync();



            QuoteData dataWrapper = JsonSerializer.Deserialize<QuoteData>(jsonContent);


            _quotes = dataWrapper?.quotes;


            if (_quotes == null || !_quotes.Any())
            {
                await DisplayAlert("Error", "Quote data was loaded, but the list is empty.", "OK");
            }
            else
            {
                ShowRandomQuote();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading quotes: {ex.Message}");
            string mainDir = FileSystem.Current.AppDataDirectory;
            await DisplayAlert("Error", $"Could not load quote data from the asset file. {mainDir} ", "OK");
            QuoteLabel.Text = "Quote data failed to load.";
            AuthorLabel.Text = "";
        }

    }

    private Quote GetQuoteByIndex()
    {
        if (_quotes == null || !_quotes.Any())
        {
            return null;
        }

        int qouteCount = _quotes.Count;
        int number = _random.Next(0, qouteCount);
        _quote = _quotes[number];
        return _quotes[number];
    }


    private async Task GetChatGPTPrompt(string prompt)
    {
        
        MainThread.BeginInvokeOnMainThread(() =>
        {
            ContextLabel.Text = "Generating context with GPT-4o...";
        });

       
        await Task.Delay(100);

        string fullresponse = string.Empty;

        try
        {
            
            CollectionResult<StreamingChatCompletionUpdate> completionUpdates = _client.CompleteChatStreaming(prompt);

            ContextLabel.Text = string.Empty; 

            foreach (StreamingChatCompletionUpdate completionUpdate in completionUpdates)
            {
                if (completionUpdate.ContentUpdate.Count > 0)
                {
                    string chunk = completionUpdate.ContentUpdate[0].Text;
                    fullresponse += chunk;

                  
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        ContextLabel.Text = fullresponse;
                    });
                }
            }

            
            if (string.IsNullOrEmpty(fullresponse) || fullresponse.Equals("null", StringComparison.OrdinalIgnoreCase))
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ContextLabel.Text = "API returned an empty or invalid response. Please check prompt and API dashboard.";
                });
            }
        }
        catch (Exception ex)
        {
            
            string errorMessage = ex is HttpRequestException
                ? $"Network or API connectivity error: {ex.Message}"
                : $"API Call Failed: {ex.Message}";

            Console.WriteLine($"ChatGPT API Error: {errorMessage}");

            MainThread.BeginInvokeOnMainThread(() =>
            {
       
                ContextLabel.Text = $" Error: {errorMessage}";
                DisplayAlert("API Error", errorMessage, "OK");
            });
        }
    }
    private void ShowRandomQuote()
    {
        if (_quotes == null)
        {
            QuoteLabel.Text = "Loading quotes, please wait...";
            AuthorLabel.Text = "";
            return;
        }

        ContextLabel.Text = "";
        Quote quote = GetQuoteByIndex();

        if (quote != null)

        {
             string prompt = "“Give context to author in a sentence or two then Explain " + quote.Author + " quote using real historical context and a soft story-like tone in 4–6 sentences that are accurate but readable. ”" + quote.Text;
            _ = GetChatGPTPrompt(prompt);
            QuoteLabel.Text = $"“{quote.Text}”";
            AuthorLabel.Text = $"“{quote.Author}”";
            EraLabel.Text = $"“{quote.Era}”";
            YearLabel.Text = $"“{quote.Year}”";
            OriginLabel.Text = $"“{quote.Origin}”";
            string quoteString = string.Join(", ", quote.Tags.Select(p => p));
            TagsLabel.Text = $"“{quoteString}”";
        }
        else
        {
            QuoteLabel.Text = "no qoutes to show";
            AuthorLabel.Text = "";
        }
    }

    bool isMenuOpen = false;
    // Use a hardcoded value that is larger than the menu's height for the animation offset
    private const double MenuTranslationOffset = -200;

    private async void OnMenuButtonClicked(object sender, EventArgs e)
    {
        if (!isMenuOpen)
        {
            // Open dropdown
            DropdownMenu.IsVisible = true;
            // Animate from the hidden position (e.g., -200) to the visible position (0) 
            await DropdownMenu.TranslateTo(0, 0, 250, Easing.CubicOut);
            MenuButton.Source = "close_icon.png";
            isMenuOpen = true;
        }
        else
        {
            // Close dropdown
            // Animate from the visible position (0) to the hidden position (e.g., -200)
            await DropdownMenu.TranslateTo(0, MenuTranslationOffset, 250, Easing.CubicIn);
            DropdownMenu.IsVisible = false;
            MenuButton.Source = "menu_icon.png";
            isMenuOpen = false;
        }

    }

    private async void OnSettingsTapped(object sender, EventArgs e)
    {
       await Shell.Current.GoToAsync(nameof(SettingsPage));
   }

    private async void OnReflectionTapped(object sender, EventArgs e)
    {
        //add the tags string to this async
        await Shell.Current.GoToAsync($"ReflectionPage?quote={_quote.Text}");
        
    }

    private async void OnProfileTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ProfilePage)); ;
    }


}
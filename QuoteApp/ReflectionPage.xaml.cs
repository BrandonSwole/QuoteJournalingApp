using static QuoteApp.MainPage;


//Consider using IQueryAttributable for storing and calling the Query, also MVVM needs to be considered and talked about. 
// adding basic text editior, may add a more complex one later.
namespace QuoteApp;

    [QueryProperty(nameof(Quote), "quote")]
    public partial class ReflectionPage : ContentPage
    {
        private string _quote;
        public string Quote
        {
            get => _quote;
            set
            {
                _quote = value;
                SetTextToQuote();
            }
        }
    

        public ReflectionPage()
        {
            InitializeComponent();
            
        }

        private void SetTextToQuote()
        {

        QuoteLabel.Text = $"“{Quote}”";


        }

    //handling the text editor 
    //logic on saving needs to be insalled.
    private void OnEditorTextChanged(object sender, EventArgs e)
    {
        
    }

    private void OnEditorCompleted(object sender, EventArgs e)
    {
        string finalContent = Reflection.Text;
    }

    private void OnSaveButtonClciked(object sender, EventArgs e)
    {
        string contentToSave = Reflection.Text;
        DisplayAlert("Saved", $"Content saved: {contentToSave}", "OK");

    }

    }

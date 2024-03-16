namespace PM2E2GRUPO1
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
        }

        private void btnAgregarFirma_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new Views.agregarVideo());
        }

        private void btnVerFirmas_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new Views.verSitios());
        }
    }

}

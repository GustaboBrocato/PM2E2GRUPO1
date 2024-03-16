namespace PM2E2GRUPO1
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
            labelIntegrantes.Text = "Integrantes - Grupo #1\r\n\r\nAleks Xavier Pineda Macias \t  - 202110060173\r\nClaudia Verónica Rodríguez Paz    - 201910010134\r\nGustavo Ariel Brocato Medina \t  - 202010030250\r\nJensy Lorena Villafranca Navarro  - 202110120039\r\nJonathan Ignacio Marley Ramírez   - 201920060176\r\nKency Angely Rosa Paz \t\t  - 202110050072";
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
        }

        private void btnAgregarSitio_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new Views.agregarVideo());
        }

        private void btnVerSitio_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new Views.verSitios());
        }
    }

}

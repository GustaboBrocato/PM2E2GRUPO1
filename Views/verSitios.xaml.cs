using Microsoft.Maui.Controls;
using PM2E2GRUPO1.Controllers;
using PM2E2GRUPO1.Models;
using PM2E2GRUPO1.ViewModels;
using System.Collections.ObjectModel;

namespace PM2E2GRUPO1.Views;

public partial class verSitios : ContentPage
{
    private ApiService _apiService;
    private readonly GeocodingService _geocodingService;

    public ObservableCollection<sitiosViewModel> SitioItems { get; set; }
    public verSitios()
	{
		InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
        BindingContext = this;
        _apiService = new ApiService();
        _geocodingService = new GeocodingService(Config.Config.GoogleApiKey);

        ShowLoadingDialog();

        InitializeAsync();
    }

    private async void InitializeAsync()
    {
        try
        {
            var sitios = await _apiService.GetDataAsync<sitioModel[]>("/sitios");

            SitioItems = new ObservableCollection<sitiosViewModel>();

            foreach (var item in sitios) 
            {
                var result = await _geocodingService.GetCoordinateDetailsAsync(item.latitud, item.longitud);

                string descripcion = $"{result.Ciudad}, {result.Departamento}";

                sitiosViewModel sitioFrame = new sitiosViewModel
                {
                    IdItem = item.id,
                    Descripcion = item.descripcion,
                    Latitud = item.latitud,
                    Longitud = item.longitud,
                    VideoDigital = item.videoDigital,
                    AudioFile = item.audioFile,
                    Lugar = descripcion,
                    VerMediaTappedCommand = new Command(() => HandleVerMediaTapped(item, descripcion)),
                    VerMapaTappedCommand = new Command(() => HandleVerMapaTapped(item, descripcion)),
                    EditarTappedCommand = new Command(() => HandleEditarTapped(item)),
                    EliminarTappedCommand = new Command(() => HandleEliminarTapped(item))
                };

                SitioItems.Add(sitioFrame);
            }

            carouselView.ItemsSource = SitioItems;
        }
        catch (Exception ex)
        {

        }
        finally
        {
            HideLoadingDialog();
        }
    }

    private async void HandleVerMediaTapped(sitioModel item, string lugar)
    {
        var verMedia = new siteView(item, lugar);

        await Navigation.PushModalAsync(new NavigationPage(verMedia));
    }
    private async void HandleVerMapaTapped(sitioModel item, string lugar)
    {
        await Navigation.PushAsync(new Views.verMapa(item, lugar));
    }

    private async void HandleEditarTapped(sitioModel item)
    {
        await Navigation.PushAsync(new Views.editarSitio(item));
    }

    private async void HandleEliminarTapped(sitioModel sitio)
    {
        var tappedItem = SitioItems.FirstOrDefault(item => item.IdItem == sitio.id);

        if (tappedItem != null)
        {
            bool userConfirmed = await DisplayAlert("Confirmación", "¿Está seguro de que desea eliminar este sitio?", "Si", "No");

            if (userConfirmed)
            {
                bool isDeleted = await _apiService.DeleteDataAsync("sitios", sitio.id);
                if (isDeleted)
                {
                    await DisplayAlert("Alerta", "El sitio ha sido eliminado", "OK");
                    SitioItems.Remove(tappedItem);
                    carouselView.ItemsSource = SitioItems;
                }
            }
        }    
    }

    private void btnRegresar_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new MainPage());
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Limpia los archivos temporales
        //CleanUpTempFiles();
    }

    public void CleanUpTempFiles()
    {
        try
        {
            // Get all files in the LocalApplicationData directory with the specified prefix
            string[] tempVideoFiles = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "tempVideo_*");
            string[] tempAudioFiles = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "tempAudio_*");

            // Delete each temporary file
            foreach (string file in tempVideoFiles)
            {
                File.Delete(file);
            }

            foreach (string file in tempAudioFiles)
            {
                File.Delete(file);
            }
        }
        catch (Exception ex)
        {
            // Handle exceptions
        }
    }

    private void ShowLoadingDialog()
    {
        // Show your loading dialog here
        activityIndicator.IsRunning = true;
        activityIndicator.IsVisible = true;
        btnRegresar.IsEnabled = false;
    }

    private void HideLoadingDialog()
    {
        // Hide your loading dialog here
        activityIndicator.IsRunning = false;
        activityIndicator.IsVisible = false;
        btnRegresar.IsEnabled = true;
    }
}
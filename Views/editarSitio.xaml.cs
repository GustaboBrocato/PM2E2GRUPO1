using CommunityToolkit.Maui.Views;
using Plugin.AudioRecorder;
using PM2E2GRUPO1.Controllers;
using PM2E2GRUPO1.Models;
using PM2E2GRUPO1.Extensions;

namespace PM2E2GRUPO1.Views;

public partial class editarSitio : ContentPage
{
    private ApiService _apiService;

    //video
    private string base64Video;

    //audio
    private AudioRecorderService audioRecorderService;
    private string audioFilePath;
    private bool isRecording;
    private string base64Audio;
    private byte[] audioBytes;
    private double Latitude = 00.00;
    private double Longitude = 00.00;

    //ID
    private int sitioID;

    public editarSitio(sitioModel sitio)
	{
		InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
        _apiService = new ApiService();
        audioRecorderService = new AudioRecorderService();

        base64Video = sitio.videoDigital;
        base64Audio = sitio.audioFile;

        labelLatitude.Text = sitio.latitud.ToString();
        labelLongitude.Text = sitio.longitud.ToString();
        entryDescripcion.Text = sitio.descripcion;
        sitioID = sitio.id;
        Latitude = sitio.latitud;
        Longitude = sitio.longitud;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        //Convierte a byte array
        byte[] videoBytes = Convert.FromBase64String(base64Video);
        audioBytes = Convert.FromBase64String(base64Audio);

        string videoFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "tempVideo.mp4");
        string audioFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "tempAudio.mp3");

        //Crea ubicaciones temporales para guardar los archivos y poder verlos en los mediaElement
        File.WriteAllBytes(videoFilePath, videoBytes);
        File.WriteAllBytes(audioFilePath, audioBytes);


        mediaElementVideo.Source = videoFilePath;
        mediaElementAudio.Source = audioFilePath;
    }

    private async Task StartRecordingAsync()
    {
        try
        {
            var status = await Permissions.RequestAsync<Permissions.Microphone>();
            if (status != PermissionStatus.Granted)
            {
                return;
            }

            // Empieza a grabar
            var audioRecordTask = await audioRecorderService.StartRecording();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error starting recording: {ex.Message}");
        }
    }

    private async Task StopRecordingAsync()
    {
        try
        {
            // Para de grabar
            await audioRecorderService.StopRecording();

            audioFilePath = audioRecorderService.GetAudioFilePath();

            audioBytes = File.ReadAllBytes(audioFilePath);
            base64Audio = Convert.ToBase64String(audioBytes);

            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "tempAudio.mp3");

            File.WriteAllBytes(filePath, audioBytes);

            mediaElementAudio.Source = MediaSource.FromFile(filePath);

            Console.WriteLine("Base64 audio: " + base64Audio);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error stopping recording: {ex.Message}");
        }
    }

    private void ShowLoadingDialog()
    {
        // Show your loading dialog here
        activityIndicator.IsRunning = true;
        activityIndicator.IsVisible = true;
        framePrincipal.IsVisible = false;
        btnAgregar.IsEnabled = false;
    }

    private void HideLoadingDialog()
    {
        // Hide your loading dialog here
        activityIndicator.IsRunning = false;
        activityIndicator.IsVisible = false;
        framePrincipal.IsVisible = true;
        btnAgregar.IsEnabled = true;
    }

    private async void btnGrabarAudio_Pressed(object sender, EventArgs e)
    {
        if (!isRecording)
        {
            await StartRecordingAsync();
            isRecording = true;
        }
    }

    private async void btnGrabarAudio_Released(object sender, EventArgs e)
    {
        if (isRecording)
        {
            await StopRecordingAsync();
            isRecording = false;
        }
    }

    private async void btnAgregar_Clicked(object sender, EventArgs e)
    {
        ShowLoadingDialog();

        if (string.IsNullOrEmpty(base64Video))
        {
            await DisplayAlert("Alerta", "Por favor grabe un video de la ubicación", "OK");
            HideLoadingDialog();
            return;
        }

        if (Latitude == 00.00 || Longitude == 00.00)
        {
            await DisplayAlert("Alerta", "No existen datos de geolocalización", "OK");
            HideLoadingDialog();
            return;
        }

        if (string.IsNullOrEmpty(base64Audio))
        {
            await DisplayAlert("Alerta", "Por favor grabe un audio con la descripción de la ubicación (mantenga presionado el icono del micrófono para grabar)", "OK");
            HideLoadingDialog();
            return;
        }

        if (string.IsNullOrEmpty(entryDescripcion.Text))
        {
            await DisplayAlert("Alerta", "Por favor ingrese un título para el lugar", "OK");
            HideLoadingDialog();
            return;
        }

        string sanitizedInput = InputSanitizer.SanitizeInput(entryDescripcion.Text);

        var data = new
        {
            Id = sitioID,
            Descripcion = sanitizedInput,
            Latitud = Latitude,
            Longitud = Longitude,
            VideoDigital = base64Video,
            AudioFile = base64Audio
        };

        bool isSuccess = await _apiService.UpdateDataAsync("/sitios", sitioID, data);

        if (isSuccess)
        {
            HideLoadingDialog();
            await DisplayAlert("¡Actualizado!", "El sitio ha sido actualizado", "OK");
            mediaElementAudio.Source = null;
            mediaElementVideo.Source = null;
            base64Audio = string.Empty;
            base64Video = string.Empty;
            entryDescripcion.Text = string.Empty;
            await Navigation.PushAsync(new Views.verSitios());
        }




    }
}
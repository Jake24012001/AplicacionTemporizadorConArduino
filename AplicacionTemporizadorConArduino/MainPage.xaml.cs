using InTheHand.Net.Sockets;
using InTheHand.Net.Bluetooth;
using Microsoft.Maui.Controls;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AplicacionTemporizadorConArduino
{
    public partial class MainPage : ContentPage
    {
        private int countdownTime = 0;
        private bool isTimerRunning = false;
        private Stopwatch timer = new();

        private BluetoothClient _client;
        private Stream _stream;

        public MainPage()
        {
            InitializeComponent();
        }

        [Obsolete]
        private async void StartTimerBtn_Clicked(object sender, EventArgs e)
        {
            if (_stream == null || !_stream.CanWrite)
            {
                await DisplayAlert("Bluetooth", "Primero debes conectarte al HC-05", "OK");
                return;
            }

            if (countdownTime <= 0) return;

            isTimerRunning = true;
            await SendCommandToArduino("START");

            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                if (!isTimerRunning || countdownTime <= 0)
                {
                    isTimerRunning = false;
                    return false;
                }

                countdownTime--;
                TimerLabel.Text = $"00:{countdownTime:D2}";
                return true;
            });
        }

        private async void PauseTimerBtn_Clicked(object sender, EventArgs e)
        {
            if (isTimerRunning)
            {
                isTimerRunning = false;
                await SendCommandToArduino("PAUSE");
            }
        }

        private async void ResetTimerBtn_Clicked(object sender, EventArgs e)
        {
            countdownTime = 0;
            TimerLabel.Text = "00:00";
            await SendCommandToArduino("RESET");
        }

        private async void Set14SecTimerBtn_Clicked(object sender, EventArgs e)
        {
            countdownTime = 14;
            TimerLabel.Text = "00:14";
            await SendCommandToArduino("T14");
        }

        private async void Set24SecTimerBtn_Clicked(object sender, EventArgs e)
        {
            countdownTime = 24;
            TimerLabel.Text = "00:24";
            await SendCommandToArduino("T24");
        }

        private async void ConnectBluetoothBtn_Clicked(object sender, EventArgs e)
        {
            try
            {
                _client = new BluetoothClient();
                var devices = _client.DiscoverDevices();

                var hc05 = devices.FirstOrDefault(d => d.DeviceName.Contains("HC-05"));
                if (hc05 == null)
                {
                    await DisplayAlert("Bluetooth", "HC-05 no encontrado", "OK");
                    return;
                }

                if (!hc05.Authenticated)
                {
                    BluetoothSecurity.PairRequest(hc05.DeviceAddress, "1234"); // Cambia el PIN si es necesario
                }

                _client.Connect(hc05.DeviceAddress, BluetoothService.SerialPort);
                _stream = _client.GetStream();

                await DisplayAlert("Bluetooth", "Conectado al HC-05", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void DisconnectBluetoothBtn_Clicked(object sender, EventArgs e)
        {
            if (_stream != null)
            {
                _stream.Close();
                _stream = null;
            }

            if (_client != null)
            {
                _client.Close();
                _client = null;
            }

            await DisplayAlert("Bluetooth", "Desconectado del HC-05", "OK");
        }

        private async Task SendCommandToArduino(string command)
        {
            if (_stream != null && _stream.CanWrite)
            {
                try
                {
                    var writer = new StreamWriter(_stream) { AutoFlush = true };
                    await writer.WriteLineAsync(command);
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", "No se pudo enviar el comando: " + ex.Message, "OK");
                }
            }
        }
    }
}
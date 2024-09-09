using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using NAudio.Wave;

namespace TimerApp
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer _timer;
        private TimeSpan _time;

        private DispatcherTimer _alarmTimer;
        private List<DateTime> _alarms = new List<DateTime>();

        private IWavePlayer _waveOut;
        private AudioFileReader _audioFileReader;

        public MainWindow()
        {
            InitializeComponent();

            HoursInput.Text = "0";
            MinutesInput.Text = "0";
            SecondsInput.Text = "0";

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;

            _alarmTimer = new DispatcherTimer();
            _alarmTimer.Interval = TimeSpan.FromSeconds(1);
            _alarmTimer.Tick += AlarmTimer_Tick;
            _alarmTimer.Start();
        }

        // Метод для воспроизведения MP3
        private void PlayMp3Sound()
        {
            try
            {
                string filePath = "1.mp3"; // Путь к файлу
                _waveOut = new WaveOutEvent();
                _audioFileReader = new AudioFileReader(filePath);
                _waveOut.Init(_audioFileReader);
                _waveOut.Play();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка воспроизведения: {ex.Message}");
            }
        }

        // Метод для остановки звука
        private void StopSound()
        {
            if (_waveOut != null)
            {
                _waveOut.Stop();
                _waveOut.Dispose();
                _audioFileReader.Dispose();
                _waveOut = null;
                _audioFileReader = null;
            }
        }

        // Таймер обратного отсчета
        private async void StartTimer_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(HoursInput.Text, out int hours) || !int.TryParse(MinutesInput.Text, out int minutes) || !int.TryParse(SecondsInput.Text, out int seconds))
            {
                MessageBox.Show("Введите корректные значения для времени.");
                return;
            }

            _time = new TimeSpan(hours, minutes, seconds);
            TimerDisplay.Text = _time.ToString(@"hh\:mm\:ss");
            _timer.Start();
        }

        private void StopTimer_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            StopSound();
        }

        private void ResetTimer_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            _time = TimeSpan.Zero;
            TimerDisplay.Text = "00:00:00";
            StopSound();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_time.TotalSeconds > 0)
            {
                _time = _time.Subtract(TimeSpan.FromSeconds(1));
                TimerDisplay.Text = _time.ToString(@"hh\:mm\:ss");
            }
            else
            {
                _timer.Stop();
                // Запуск звука и показ сообщения
                PlayMp3Sound();
                ShowMessageAndStopSound("Таймер завершён!");
            }
        }

        // Метод для показа сообщения и остановки звука
        private void ShowMessageAndStopSound(string message)
        {
            MessageBox.Show(message, "Уведомление", MessageBoxButton.OK);
            StopSound();  // Останавливаем звук сразу после нажатия OK
        }

        // Будильник
        private void SetAlarm_Click(object sender, RoutedEventArgs e)
        {
            DateTime selectedDate = AlarmDatePicker.SelectedDate ?? DateTime.Now;
            TimeSpan time = TimeSpan.Parse(AlarmTimeInput.Text);
            DateTime alarmTime = selectedDate.Date + time;

            _alarms.Add(alarmTime);
            AlarmList.Items.Add(alarmTime.ToString("g"));
        }

        private void AlarmTimer_Tick(object sender, EventArgs e)
        {
            DateTime currentTime = DateTime.Now;

            for (int i = _alarms.Count - 1; i >= 0; i--)
            {
                if (Math.Abs((currentTime - _alarms[i]).TotalSeconds) < 1)
                {
                    PlayMp3Sound();
                    ShowMessageAndStopSound("Будильник сработал!");
                    _alarms.RemoveAt(i);
                }
            }
        }
    }
}

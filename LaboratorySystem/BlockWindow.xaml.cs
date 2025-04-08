using System;
using System.Windows;
using System.Windows.Threading;

namespace LaboratorySystem
{
    /// <summary>
    /// Окно блокировки системы, отображающее обратный отсчет времени
    /// </summary>
    public partial class BlockWindow : Window
    {
        // Таймер для обратного отсчета
        private readonly DispatcherTimer timer;

        // Оставшееся время блокировки
        private TimeSpan remainingTime;

        /// <summary>
        /// Конструктор окна блокировки
        /// </summary>
        public BlockWindow(TimeSpan blockDuration)
        {
            InitializeComponent();

            // Инициализация оставшегося времени
            remainingTime = blockDuration;
            UpdateTimerDisplay();

            // Настройка таймера
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1); // Обновление каждую секунду
            timer.Tick += Timer_Tick; // Подписка на событие тика
            timer.Start(); // Запуск таймера
        }

        // Обработчик события таймера (срабатывает каждую секунду)
        private void Timer_Tick(object sender, EventArgs e)
        {
            // Уменьшаем оставшееся время на 1 секунду
            remainingTime = remainingTime.Subtract(TimeSpan.FromSeconds(1));
            UpdateTimerDisplay();

            // Проверяем, закончилось ли время блокировки
            if (remainingTime <= TimeSpan.Zero)
            {
                timer.Stop();
                DialogResult = true; // Устанавливаем результат диалога
                Close(); // Закрываем окно
            }
        }

        // Обновление отображения таймера на интерфейсе
        private void UpdateTimerDisplay()
        {
            // Отображаем только секунды (целочисленное значение)
            tbTimer.Text = remainingTime.Seconds.ToString();
        }

        // Обработчик закрытия окна
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            // Останавливаем таймер при закрытии окна
            timer?.Stop();
        }
    }
}
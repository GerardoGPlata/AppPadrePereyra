using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EscolarAppPadres.Models
{
    public class RemindNotificationOptions : INotifyPropertyChanged
    {
        private DateTime _selectedDate;
        private TimeSpan? _selectedTime;

        public string? Icon { get; set; }
        public string? Label { get; set; }
        public bool IsCustomTime { get; set; }
        public DateTime CustomDate { get; set; }
        public TimeSpan? CustomTime { get; set; }

        public CornerRadius CornerRadius { get; set; }

        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (_selectedDate != value)
                {
                    _selectedDate = value;
                    OnPropertyChanged(nameof(SelectedDate));
                }
            }
        }

        public TimeSpan? SelectedTime
        {
            get => _selectedTime;
            set
            {
                if (_selectedTime != value)
                {
                    _selectedTime = value;
                    OnPropertyChanged(nameof(SelectedTime));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

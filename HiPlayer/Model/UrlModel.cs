using SQLite;
using System;
using System.ComponentModel;

namespace HiPlayer.Model
{
    public class UrlModel : INotifyPropertyChanged
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        private string _name;
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged("Name");
                }
            }
        }
        public string Path { get; set; }
        public ResourceType ResourceType { get; set; }

        private bool _playing;
        public bool Playing
        {
            get
            {
                return _playing;
            }
            set
            {
                if (_playing != value)
                {
                    _playing = value;
                    OnPropertyChanged("Playing");
                }
            }
        }
        private bool _isSelected;
        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged("IsSelected");
                }
            }
        }
        public bool HasVideo { get; set; }

        public bool HasAudio { get; set; }

        public string VideoType { get; set; }
        public string AudioType { get; set; }

        public int VideoHeight { get; set; }
        public int VideoWidth { get; set; }
        public float FileSize { get; set; }
        public TimeSpan Duration { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }      
    }
    public enum ResourceType
    {
        Local = 0,
        Net = 1,
        All = -1,
    }
}

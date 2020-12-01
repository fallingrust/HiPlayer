using HiPlayer.Helper;
using HiPlayer.Model;
using HiPlayer.Windows;
using LibVLCSharp.Shared;
using LibVLCSharp.WPF;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace HiPlayer.ViewModel
{
    public class MainViewModel : BindableBase
    {
        private List<UrlModel> AllResources;
        private VideoView _videoView;
        private LibVLC _libVlc;
        private MediaPlayer _mediaPlayer;
      
        public bool _isSeeking = false;
        private ResourceType selectedType = ResourceType.All;
        private DbHelper db;

        private int _curDuration;
        public int CurDuration
        {
            get
            {
                return _curDuration;
            }
            set
            {
                SetProperty(ref _curDuration, value);
            }
        }
        private int _totalDuration;
        public int TotalDuration
        {
            get
            {
                return _totalDuration;
            }
            set
            {
                SetProperty(ref _totalDuration, value);
            }
        }
        private bool _isPlaying;
        public bool IsPlaying
        {
            get
            {
                return _isPlaying;
            }
            set
            {
                SetProperty(ref _isPlaying, value);
            }
        }
        public ObservableCollection<UrlModel> _resourceList;
        public ObservableCollection<UrlModel> ResourceList
        {
            get
            {
                return _resourceList;
            }
            set
            {
                SetProperty(ref _resourceList, value);
            }
        }
        public DelegateCommand<object> PlayCommand { get; set; }
        public DelegateCommand<object> ChangeFilterCommand { get; set; }
        public DelegateCommand<object> VideoViewLoaded { get; set; }
        public DelegateCommand<object> SeekToCommand { get; set; }
        public DelegateCommand<object> VolumeChangeCommand { get; set; }
        public DelegateCommand OpenFileCommand { get; set; }
        public DelegateCommand OpenUrlCommand { get; set; }

        public MainViewModel()
        {       
            PlayCommand = new DelegateCommand<object>((obj) =>
            {
                bool hasSelectedItem = false;
                foreach(var item in ResourceList)
                {
                    if (item.IsSelected)
                    {
                        hasSelectedItem = true;
                        if (!item.Playing)
                        {
                            using var media = new Media(_libVlc, new Uri(item.Path));
                            var result = _mediaPlayer.Play(media);
                            item.Playing = result;
                            IsPlaying = result;
                            if (!result)
                                MessageBox.Show("播放失败！");
                        }
                        else
                        {
                            _mediaPlayer.Pause();
                        }
                    }
                    else
                    {
                        item.Playing = false;
                    }
                }
                if (!hasSelectedItem)
                {
                    if (obj is ToggleButton tgb)
                    {
                        tgb.IsChecked = false;
                    }
                    MessageBox.Show("未选中任何文件！");                   
                }
            });
            ChangeFilterCommand = new DelegateCommand<object>((obj) =>
            {
                if(obj is RadioButton rb)
                {
                    switch (rb.Name)
                    {
                        case "rb_allResource":
                            selectedType = ResourceType.All;
                            break;
                        case "rb_urlResource":
                            selectedType = ResourceType.Net;
                            break;
                        case "rb_localResource":
                            selectedType = ResourceType.Local;
                            break;
                    }
                    ResourceList.Clear();
                    if (selectedType == ResourceType.All)
                    {
                        ResourceList.AddRange(AllResources);
                    }
                    else
                    {
                        ResourceList.AddRange(AllResources.Where(p => p.ResourceType == selectedType));
                    }
                }
            });
            VideoViewLoaded = new DelegateCommand<object>((obj) =>
            {
                if(obj is VideoView vv)
                {
                    _videoView = vv;
                    Task.Factory.StartNew(() =>
                    {
                        _libVlc = new LibVLC();
                        _mediaPlayer = new MediaPlayer(_libVlc);
                       
                        _mediaPlayer.Playing += _mediaPlayer_Playing;
                        _mediaPlayer.PositionChanged += _mediaPlayer_PositionChanged;
                        return;
                    }).ContinueWith((result) =>
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            _videoView.MediaPlayer = _mediaPlayer;
                        });
                    });
                }
                new Thread(() =>
                {
                    string path = Path.Combine(Environment.CurrentDirectory, "hi.db");
                    db = new DbHelper(path);
                    AllResources = new List<UrlModel>(db.UrlModels.ToList());
                    foreach(var item in AllResources)
                    {
                        item.IsSelected = false;
                    }
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ResourceList = new ObservableCollection<UrlModel>(AllResources);
                    });

                }).Start();
            });
            SeekToCommand = new DelegateCommand<object>((obj) =>
            {
                if(_mediaPlayer != null)
                {
                    if (obj is double position)
                    {
                        if(position >= 0 && position <= TotalDuration)
                        {
                            _mediaPlayer.Position = (float)(position / TotalDuration);
                        }
                    }
                }
            });
            VolumeChangeCommand = new DelegateCommand<object>((obj) =>
            {
                if(obj is int volume)
                {
                    if (_mediaPlayer != null && volume >= 0 && volume <= 1)
                    {
                        _mediaPlayer.Volume = volume;
                    }
                }
               
            });
            OpenFileCommand = new DelegateCommand(() =>
            {
                System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog
                {
                    InitialDirectory = Environment.CurrentDirectory,
                    CheckFileExists = true,
                    Filter = "(*.mp4,*.avi,*.mp3,*.aac)|*.mp4;*.avi;*.mp3;*.aac|All files(*.*)|*.*"
                };
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Task.Factory.StartNew(() =>
                    {
                        FileInfo info = new FileInfo(dialog.FileName);
                        var model = new UrlModel() { Name = info.Name, Path = info.FullName, FileSize = info.Length / 1024 / 1024,ResourceType = ResourceType.Local };
                        db.Insert(model);
                        db.Commit();
                        return model;
                    }).ContinueWith((result) =>
                    {
                        UrlModel item = result.Result;
                        item.IsSelected = true;
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            AllResources.Add(item);
                            ResourceList.Clear();
                            if (selectedType == ResourceType.All)
                            {
                                ResourceList.AddRange(AllResources);
                            }
                            else
                            {
                                ResourceList.AddRange(AllResources.Where(p => p.ResourceType == selectedType));
                            }
                        });
                    });
                   
                   
                }
            });
            OpenUrlCommand = new DelegateCommand(() =>
            {
                OpenUrlWindow urlWindow = new OpenUrlWindow
                {
                    Owner = Application.Current.MainWindow
                };
                urlWindow.AddUrlEvent += UrlWindow_AddUrlEvent;
                urlWindow.Closed += delegate (object sender, EventArgs e)
                {
                    urlWindow.AddUrlEvent -= UrlWindow_AddUrlEvent;
                };
                urlWindow.Show();
            });          
        }

        private void UrlWindow_AddUrlEvent(object sender, EventArgs e)
        {
            string url = sender as string;
            Task.Factory.StartNew(() =>
            {
                var model = new UrlModel() { Name = url, Path = url,  ResourceType = ResourceType.Net};
                db.Insert(model);
                db.Commit();
                return model;
            }).ContinueWith((result) =>
            {
                UrlModel item = result.Result;
                item.IsSelected = true;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    AllResources.Add(item);
                    ResourceList.Clear();
                    if (selectedType == ResourceType.All)
                    {
                        ResourceList.AddRange(AllResources);
                    }
                    else
                    {
                        ResourceList.AddRange(AllResources.Where(p => p.ResourceType == selectedType));
                    }
                });
            });
        }

        private void _mediaPlayer_Playing(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                TotalDuration = (int)(_mediaPlayer.Media.Duration / 1000);
            });
        }     

        private void _mediaPlayer_PositionChanged(object sender, MediaPlayerPositionChangedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if(!_isSeeking)
                    CurDuration = (int)(e.Position * TotalDuration);
            });
        }
    }
}

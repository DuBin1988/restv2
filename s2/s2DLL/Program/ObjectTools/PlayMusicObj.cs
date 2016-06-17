using System;
using System.ComponentModel;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Com.Aote.ObjectTools
{
    public class PlayMusicObj : FrameworkElement, INotifyPropertyChanged
    {
        #region IsPlay 
        private bool isPlay = false;
        public bool IsPlay
        {
            get { return isPlay; }
            set
            {
                isPlay = value;
                if (mediaElement != null && IsPlay)
                {
                    Play();
                }
            }
        }
        #endregion


       

        #region MediaElement 播放对象
        private MediaElement mediaElement;
        public MediaElement MediaElement
        {
            get { return mediaElement; }
            set
            {
                mediaElement = value;
                if (mediaElement != null && IsPlay)
                {
                    Play();
                }
            }
        }
        #endregion




        #region Play() 播放音乐,目前支持的是循环模式
        public void Play()
        {
            this.mediaElement.MediaEnded += new RoutedEventHandler(me_MediaEnded);
            this.mediaElement.Play();
            this.mediaElement.AutoPlay = true;
         }

        void me_MediaEnded(object sender, RoutedEventArgs e)
        {
            MediaElement me = (MediaElement)sender;
            me.Stop();
            me.Play();
        }
        #endregion

        #region Stop() 停止播放
        public void Stop()
        {
            this.mediaElement.Stop();
         }
        #endregion



        public event PropertyChangedEventHandler PropertyChanged;
    }
}

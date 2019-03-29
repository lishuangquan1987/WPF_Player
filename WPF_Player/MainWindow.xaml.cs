using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing.Drawing2D;
using System.Drawing.Design;
using System.IO;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Threading;
using System.Text.RegularExpressions;


namespace WPF_Player
{
    using IOPath = System.IO.Path;
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        public MainWindow()
        {
            InitializeComponent();
        }
        private bool move = false;
        MyPlayer player = new MyPlayer();
        MediaPlayer playerhandle = null;
        PlayerStatus currentStatus = PlayerStatus.NerverStart;
        ObservableCollection<Song> playList = new ObservableCollection<Song>();
        private int currentIndex = 0;
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.rollingText.Text = "音乐播放器处于暂停状态";
            //this.rollingText.label1.ForeColor = System.Drawing.Color.Blue;

            this.listBox.DataContext = playList;
            playerhandle = player.GetPlayerHandle();
            playerhandle.MediaEnded += playerhandle_MediaEnded;
            playerhandle.MediaFailed += playerhandle_MediaFailed;
            playerhandle.MediaOpened += playerhandle_MediaOpened;
            player.playEvent += player_playEvent;
            player.playEvent_thread += player_playEvent_thread;

            this.slider.DataContext = player;
          

            //读取配置
            string folderPath =ConfigurationManager.AppSettings["folderPath"];
            folderPath = IOPath.GetFullPath(folderPath);
            if(!Directory.Exists(folderPath))
                return;
            GetMP3Files(folderPath).ToList().ForEach(x => playList.Add(new Song() {Location=x}));
            //获取歌词
            foreach (Song s in playList)
            {
                s.LstLynic = GetLynicBySong(s);
            }

            
            
        }

        void player_playEvent_thread(object obj)
        {
            double PlayedTime =(double)(this.Dispatcher.Invoke((Func<double>)(() => { return playerhandle.Position.TotalMilliseconds; })));
            List<Lynic> lstLy = player.CurrentSong.LstLynic;
            ShowLynic(lstLy, PlayedTime);
        }
        int tempValue = 0;//当达到500时触发界面更新进度
        int lastValue;
        void player_playEvent(object sender)
        {
            //tempValue++;
            if (!playerhandle.NaturalDuration.HasTimeSpan)
            {
                return;
            }
            double PlayedTime = playerhandle.Position.TotalSeconds;
            //if (tempValue % 500 != 0)
            //    return;
            double totalTime=playerhandle.NaturalDuration.TimeSpan.TotalSeconds;
            
            this.lable.Content = string.Format("{0}/{1}", playerhandle.Position.ToString("mm\\:ss"), playerhandle.NaturalDuration.TimeSpan.ToString("mm\\:ss"));
            if (lastValue == (int)PlayedTime)
                return;
            this.trackBar.MaxValue = totalTime;
            this.trackBar.CurrentValue = PlayedTime;
            lastValue = (int)this.trackBar.CurrentValue;
        }
        private void ShowLynic(List<Lynic> lstLy, double playTime)
        {
            if (lstLy == null || lstLy.Count == 0)
            {
                return;
            }
            int index = player.CurrentSong.LstLynic.FindIndex(0, lstLy.Count, x => playTime-x.MiniSencond<100&&playTime-x.MiniSencond>=0);
            if (index == -1)
                return;
            else//满足切换歌词的条件
            {
                #region ~~~
                this.Dispatcher.Invoke((Action)(() =>
                {
                    //第二行歌词
                    System.Windows.Controls.ListBoxItem item2 = lynicBoard.Items[2] as System.Windows.Controls.ListBoxItem;
                    item2.Content = lstLy[index].Content;
                    if (index - 1 >= 0)
                    {
                        //第1行歌词
                        System.Windows.Controls.ListBoxItem item1 = lynicBoard.Items[1] as System.Windows.Controls.ListBoxItem;
                        item1.Content = lstLy[index - 1].Content;
                    }
                    if (index - 2 >= 0)
                    {
                        //第0行歌词
                        System.Windows.Controls.ListBoxItem item0 = lynicBoard.Items[0] as System.Windows.Controls.ListBoxItem;
                        item0.Content = lstLy[index - 2].Content;
                    }
                    if (index + 1 < lstLy.Count)
                    {
                        //第3行歌词
                        System.Windows.Controls.ListBoxItem item3 = lynicBoard.Items[3] as System.Windows.Controls.ListBoxItem;
                        item3.Content = lstLy[index + 1].Content;
                    }
                    if (index + 2 < lstLy.Count)
                    {
                        //第4行歌词
                        System.Windows.Controls.ListBoxItem item4 = lynicBoard.Items[4] as System.Windows.Controls.ListBoxItem;
                        item4.Content = lstLy[index + 2].Content;
                    }
                    if (index == lstLy.Count - 1)//歌词到了最后一行
                    {
                        System.Windows.Controls.ListBoxItem item3 = lynicBoard.Items[3] as System.Windows.Controls.ListBoxItem;
                        System.Windows.Controls.ListBoxItem item4 = lynicBoard.Items[4] as System.Windows.Controls.ListBoxItem;
                        item3.Content = "";
                        item4.Content = "";
                    }
                    if (index == lstLy.Count - 2)//歌词到了倒数第二行
                    {

                        System.Windows.Controls.ListBoxItem item4 = lynicBoard.Items[4] as System.Windows.Controls.ListBoxItem;
                        item4.Content = "";
                    }
                }));
                
                #endregion
            }
        }
        void playerhandle_MediaOpened(object sender, EventArgs e)
        {
            this.rollingText.Text = "当前播放：" + player.CurrentSong.Name;
        }

        void playerhandle_MediaFailed(object sender, ExceptionEventArgs e)
        {
            
        }

        void playerhandle_MediaEnded(object sender, EventArgs e)
        {
            if (playList.Count > 0)
            {
                player.CurrentSong.LstLynic.ForEach(x => x.IsShow = false);
                PlayNext();
            }
        }

        private void btn_Play_Click(object sender, RoutedEventArgs e)
        {
            //1,暂停播放
            //2,从一首新歌播放
            if (currentStatus == PlayerStatus.Pause)
            {            
                Song song = this.listBox.SelectedItem as Song;
                currentIndex = this.listBox.SelectedIndex;
                if (song != player.CurrentSong)
                    player.CurrentSong = song;
                player.Play();
                currentStatus = PlayerStatus.Start;
                this.btn_Play.SetValue(System.Windows.Controls.Button.StyleProperty, System.Windows.Application.Current.Resources["buttonPause_new"]);
            }
            else if( currentStatus== PlayerStatus.NerverStart)
            {
                if (this.listBox.SelectedIndex < 0)
                {
                    this.rollingText.Text = "请选择一首歌曲然后播放！";
                    return;
                }
                Song song = this.listBox.SelectedItem as Song;
                currentIndex = this.listBox.SelectedIndex;
                player.CurrentSong = song;
                player.Play();
                currentStatus = PlayerStatus.Start;
                this.btn_Play.SetValue(System.Windows.Controls.Button.StyleProperty, System.Windows.Application.Current.Resources["buttonPause_new"]);
            }
            else if (currentStatus == PlayerStatus.Start)
            {
                player.Pause();
                currentStatus = PlayerStatus.Pause;
                this.btn_Play.SetValue(System.Windows.Controls.Button.StyleProperty, System.Windows.Application.Current.Resources["buttonPlay"]);
            }

        }

        private void btn_Previous_Click(object sender, RoutedEventArgs e)
        {
            if (playList.Count > 0)
                PlayPrevious();
        }
    
        void PlayPrevious()
        {
            if (currentStatus == PlayerStatus.Start)
                player.Stop();
            currentIndex--;
            if (currentIndex < 0)
                currentIndex = playList.Count - 1;
            this.listBox.SelectedIndex = currentIndex;
            player.CurrentSong = playList[currentIndex];
            player.Play();
            currentStatus = PlayerStatus.Start;
        }
        void PlayNext()
        {
            if (currentStatus == PlayerStatus.Start)
                player.Stop();
            currentIndex++;
            if (currentIndex > playList.Count - 1)
                currentIndex = 0;
            this.listBox.SelectedIndex = currentIndex;
            player.CurrentSong = playList[currentIndex];
            foreach (var item in this.lynicBoard.Items)
            {
                (item as ListBoxItem).Content = "";
            }
            player.Play();
            currentStatus = PlayerStatus.Start;
            
        }
        private void btn_Next_Click(object sender, RoutedEventArgs e)
        {
            if (playList.Count > 0)
                PlayNext();
        }
        enum PlayerStatus
        {
            NerverStart=1,
            Start,
            Pause,
            Stop
        }
        private void loadSong_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "请选择歌曲的文件夹";
            if (fbd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;
            string folderfName = fbd.SelectedPath;
            string[] mp3Files = GetMP3Files(folderfName);
            if(mp3Files!=null)
            {
                playList.Clear();
                mp3Files.ToList().ForEach(x => playList.Add(new Song(){ Location=x}));
                //获取歌词
                foreach (Song s in playList)
                {
                    s.LstLynic = GetLynicBySong(s);
                }
            }
        }
        /// <summary>
        /// 根据歌曲，获取歌词
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private List<Lynic> GetLynicBySong(Song s)
        {            
            string lynicFile1 = IOPath.Combine(IOPath.GetDirectoryName(s.Location), IOPath.GetFileNameWithoutExtension(s.Location) + ".lrc");
            string lynicFile2 = IOPath.Combine(IOPath.GetDirectoryName(s.Location), "Lynic", IOPath.GetFileNameWithoutExtension(s.Location) + ".lrc");
            if (File.Exists(lynicFile1))
            {
                return GetLynics(lynicFile1);
            }
            else if (File.Exists(lynicFile2))
            {
                return GetLynics(lynicFile2);
            }
            else
                return null;
        }
        private string[] GetMP3Files(string folderName)
        {
            if (!Directory.Exists(folderName))
                throw new Exception("文件夹:" + folderName + "不存在！");
            Configuration c = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            c.AppSettings.Settings["folderPath"].Value = folderName;
            c.Save();          
            return Directory.GetFiles(folderName, "*.mp3");
        }

        private void listBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (currentStatus == PlayerStatus.Start)
                player.Stop();
            Song song = this.listBox.SelectedItem as Song;
            currentIndex = this.listBox.SelectedIndex;
            player.CurrentSong = song;
            player.Play();
            currentStatus = PlayerStatus.Start;
            this.btn_Play.SetValue(System.Windows.Controls.Button.StyleProperty, System.Windows.Application.Current.Resources["buttonPause"]);
        }

        private void trackBar_PlayProcessChanged(double obj)
        {
            if (currentStatus == PlayerStatus.Start || currentStatus == PlayerStatus.Pause)
            {
                playerhandle.Position = TimeSpan.FromSeconds(obj);
            }
        }
        #region 歌词
        void ShowLynic(List<Lynic> lstLynic)
        {

            new Thread(() =>
            {
                int t = System.Environment.TickCount;
                int notRunningTime = 0;
                for (int i = 0; i < lstLynic.Count; i++)
                {
                    while (currentStatus == PlayerStatus.Pause)
                    {
                        notRunningTime++;//如果暂停，则将暂停的毫秒数记录下来。
                        Thread.Sleep(1);
                    }
                    while (System.Environment.TickCount - t-notRunningTime < lstLynic[i].MiniSencond)
                    {
                        Thread.Sleep(1);
                    }
                    //Console.WriteLine(lstLynic[i].Content);
                    this.Dispatcher.Invoke((Action)(() =>
                        {
                            System.Windows.Controls.ListBoxItem item2 = lynicBoard.Items[2] as System.Windows.Controls.ListBoxItem;
                            item2.Content = lstLynic[i].Content;
                            if (i - 1 >= 0)
                            {
                                System.Windows.Controls.ListBoxItem item1 = lynicBoard.Items[1] as System.Windows.Controls.ListBoxItem;
                                item1.Content = lstLynic[i-1].Content;
                            }
                            if (i - 2 >= 0)
                            {
                                System.Windows.Controls.ListBoxItem item0 = lynicBoard.Items[0] as System.Windows.Controls.ListBoxItem;
                                item0.Content = lstLynic[i - 2].Content;
                            }
                            if (i + 1 < lstLynic.Count)
                            {
                                System.Windows.Controls.ListBoxItem item3 = lynicBoard.Items[3] as System.Windows.Controls.ListBoxItem;
                                item3.Content = lstLynic[i + 1].Content;
                            }
                            if (i + 2 < lstLynic.Count)
                            {
                                System.Windows.Controls.ListBoxItem item4 = lynicBoard.Items[4] as System.Windows.Controls.ListBoxItem;
                                item4.Content = lstLynic[i + 2].Content;
                            }
                            if (i == lstLynic.Count - 1)//歌词到了最后一行
                            {
                                System.Windows.Controls.ListBoxItem item3 = lynicBoard.Items[3] as System.Windows.Controls.ListBoxItem;
                                System.Windows.Controls.ListBoxItem item4 = lynicBoard.Items[4] as System.Windows.Controls.ListBoxItem;
                                item3.Content = "";
                                item4.Content = "";
                            }
                            if (i == lstLynic.Count - 2)//歌词到了倒数第二行
                            {
                                
                                System.Windows.Controls.ListBoxItem item4 = lynicBoard.Items[4] as System.Windows.Controls.ListBoxItem;          
                                item4.Content = "";
                            }
                        }));
                    lstLynic[i].IsShow = true;
                }
            }).Start();
        }
        Regex r = new Regex("[01][0-9]:[0-9][0-9].[0-9][0-9]");
        List<Lynic> GetLynics(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                    return null;
                List<Lynic> result = new List<Lynic>();
                string[] contents = File.ReadAllLines(path, Encoding.Default);
                contents = contents.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                foreach (string i in contents)
                {
                    string[] timeStr = i.Split('[', ']');
                    Match m = r.Match(timeStr[1]);
                    if (m != null && m.Success)
                    {
                        string[] temp = m.Value.Split(':', '.');
                        Lynic lynic = new Lynic();
                        lynic.MiniSencond = (int.Parse(temp[0]) * 60 + int.Parse(temp[1]) + int.Parse(temp[2]) / 1000) * 1000;
                        lynic.Content = timeStr[2];
                        if (string.IsNullOrEmpty(timeStr[2]))
                            continue;//过滤空的歌词
                        result.Add(lynic);
                    }
                }
                return result;
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
                return null;
            }
        }
        #endregion

        private void lb_TitleMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            move = false;
        }

        private void lb_TitleMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            move = true;
        }

        private void lb_TitleMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (move)
            {
                try
                {
                    this.DragMove();
                }
                catch (Exception)
                { }
            }
        }

        private void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void miniButtonClick(object sender, RoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
        }
    }
}

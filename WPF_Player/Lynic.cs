using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Player
{
    public class Lynic
    {
        private string author;
        /// <summary>
        /// 作者名称
        /// </summary>
        public string Author
        {
            get { return author; }
            set { author = value; }
        }
        private string name;
        /// <summary>
        /// 歌曲名称
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        private int miniSencond;

        public int MiniSencond
        {
            get { return miniSencond; }
            set { miniSencond = value; }
        }
        private string content;

        public string Content
        {
            get { return content; }
            set { content = value; }
        }
        private bool isShow = false;

        public bool IsShow
        {
            get { return isShow; }
            set { isShow = value; }
        }
    }
}

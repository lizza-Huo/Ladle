using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;



namespace LadleThermDetectSys
{
    public partial class ThickBotPic : Form
    {

        AutoResizeForm asc = new AutoResizeForm();
        public ThickBotPic()
        {
            InitializeComponent();
        }

        public ThickBotPic(Image pic)
        {
            InitializeComponent();
            image = pic;
            this.picBxThickBotPic.Image = pic;
        }

        public delegate void ChangePictureHandler(Image pic);
        public event ChangePictureHandler ThickBotPic_ChangePicture;
        private Image image;
        
        private void ThickBotPic_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (ThickBotPic_ChangePicture != null)
            {
                ThickBotPic_ChangePicture(image);
            }
        }


        private void picBxThickBotPic_MouseWheel(object sender, MouseEventArgs e) 
        {
            if (e.Delta > 0) //放大图片
            {
                picBxThickBotPic.Size = new Size(picBxThickBotPic.Width + 50, picBxThickBotPic.Height + 50);
            }
            else
            {  //缩小图片
                picBxThickBotPic.Size = new Size(picBxThickBotPic.Width - 50, picBxThickBotPic.Height - 50);
            }
            //设置图片在窗体居中
            picBxThickBotPic.Location = new Point((this.Width - picBxThickBotPic.Width) / 2, (this.Height - picBxThickBotPic.Height) / 2);
        }

        private void ThickBotPic_Load(object sender, EventArgs e)
        {
            asc.controllInitializeSize(this);
        }

        private void ThickBotPic_SizeChanged(object sender, EventArgs e)
        {
            asc.controlAutoSize(this);
        }
    }
}

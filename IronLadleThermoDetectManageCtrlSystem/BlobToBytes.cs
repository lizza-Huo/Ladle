using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.IO;


namespace IronLadleThermoDetectManageCtrlSystem
{

        public class BlobToBytes
        {

            private int width;
            private int height;



            public static Bitmap  GrayImage(Int16[] data, int width, int height)
            {
                double min = 1000, max = 0;
                double a;
                double[] GrayIgeBlob = new double[width * height];
                byte[] GrayIge = new byte[width * height];
                for (int row = 0; row < height; row++)
                {
                    for (int col = 0; col < width; col++)
                    {
                        a = data[col * height + row];
                        GrayIgeBlob[row * width + col] = a;
                        if (min > a)
                            min = a;
                        if (max < a)
                            max = a;
                    }
                }
                Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
                BitmapData bmpdata = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);
                int offset = bmpdata.Stride - bmpdata.Width;
                IntPtr ptr = bmpdata.Scan0; //获取首地址
                int scanBytes = bmpdata.Stride * bmpdata.Height;
                int posSrc = 0, posScan = 0;
                double thresmin = max * 0.3, thresmax = max * 0.7;
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        double item = GrayIgeBlob[posSrc++];
                        if (item == 0)
                            GrayIge[posScan++] = 0;
                        else
                        {
                            //if ((item < thresmin) || (item > thresmax))
                            GrayIge[posScan++] = (byte)((item - min) * 255 / (max - min));
                            //else
                            // GrayIge[posScan++] = (byte)((item) * 255 / (max - min));
                        }
                    }
                    posScan += offset;
                }


                Marshal.Copy(GrayIge, 0, ptr, scanBytes);
                bitmap.UnlockBits(bmpdata);

                //修改生成位图的索引表，从伪彩修改为灰度
                ColorPalette palette;
                //获取一个Format8bppIndex格式图像的Palette对象
                using (Bitmap bmp = new Bitmap(1, 1, PixelFormat.Format8bppIndexed))
                {
                    palette = bmp.Palette;
                }
                for (int i = 0; i < 256; i++)
                {
                    palette.Entries[i] = Color.FromArgb(i, i, i);
                }
                //修改生成位图的索引表
                bitmap.Palette = palette;

                string CurrentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string FileName = $"ThermalImagerImage\\{DateTime.Now.Year}-{DateTime.Now.Month}";
                if (!Directory.Exists(FileName))
                {
                    Directory.CreateDirectory(FileName, null);
                }
                string strFileName = $"ThermalImagerImage\\{DateTime.Now.Year}-{DateTime.Now.Month}\\" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".jpg";
                //bitmap.Save(strFileName);
                return BytesToBitmap(Bitmap2Byte(bitmap));
            }
            public static byte[] Bitmap2Byte(Bitmap bitmap)
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    bitmap.Save(stream, ImageFormat.Jpeg);
                    byte[] data = new byte[stream.Length];
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.Read(data, 0, Convert.ToInt32(stream.Length));
                    return data;
                }
            }
            /// <summary>
            /// 将buffer中的图像帧数据流入bitmap中
            /// </summary>
            /// <param name="Bytes"></param>
            /// <returns></returns>
            public static Bitmap BytesToBitmap(byte[] Bytes)

            {
                MemoryStream stream = null;
                try
                {
                    stream = new MemoryStream(Bytes);
                    return new Bitmap((Image)new Bitmap(stream));
                }
                finally
                {
                    stream.Close();
                }
            }
        }
}


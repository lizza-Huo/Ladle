using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.IO;

namespace IronLadleThermoDetectManageCtrlSystem
{
    class PaddleXPredict
    {
        private delegate void UpdateUI();  // 声明委托
        #region 接口定义及参数
        static int modelType = 1;  // 模型的类型  0：分类模型；1：检测模型；2：分割模型
        static string modelPath = ""; // 模型目录路径
        static bool useGPU = false;  // 是否使用GPU
        static bool useTrt = false;  // 是否使用TensorRT
        static bool useMkl = true;  // 是否使用MKLDNN加速模型在CPU上的预测性能
        static int mklThreadNum = 8; // 使用MKLDNN时，线程数量
        static int gpuID = 0; // 使用GPU的ID号
        static string key = ""; //模型解密密钥，此参数用于加载加密的PaddleX模型时使用
        static bool useIrOptim = false; // 是否加速模型后进行图优化
        static bool visualize = false;
        // bool isInference = false;  // 是否进行推理   
        static IntPtr model; // 模型

        // 目标物种类，需根据实际情况修改！
        string[] category = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

        // 定义CreatePaddlexModel接口
        [DllImport("paddlex_inference.dll", EntryPoint = "CreatePaddlexModel", CharSet = CharSet.Ansi)]
        static extern IntPtr CreatePaddlexModel(ref int modelType,
                                                string modelPath,
                                                bool useGPU,
                                                bool useTrt,
                                                bool useMkl,
                                                int mklThreadNum,
                                                int gpuID,
                                                string key,
                                                bool useIrOptim);

        // 定义检测接口
        [DllImport("paddlex_inference.dll", EntryPoint = "PaddlexDetPredict", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern bool PaddlexDetPredict(IntPtr model, byte[] image, int height, int width, int channels, int max_box, float[] result, bool visualize);
        #endregion


        //加载模型
        public static void loadmodel(string path)
        {
            FolderBrowserDialog fileDialog = new FolderBrowserDialog();
            modelPath = path;
            model = CreatePaddlexModel(ref modelType, modelPath, useGPU, useTrt, useMkl, mklThreadNum, gpuID, key, useIrOptim);
            MessageBox.Show("已加载模型路径:" + modelPath, "选择文件提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }


        //Inference预测函数，输入图片路径，输出包号
        public static int Inference(string path)
        {
            Bitmap bmp;
            bmp = ReadImageFile(path);
            Bitmap bmpNew = bmp.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height), bmp.PixelFormat);
            //     Bitmap resultShow;
            Mat img = BitmapConverter.ToMat(bmpNew);
            int channel = Image.GetPixelFormatSize(bmp.PixelFormat) / 8;
            byte[] source = GetbyteData(bmp);
            int num1, num2;
            int num;
            int max_box = 10;
            float[] result = new float[max_box * 6 + 1];
            bool res = PaddlexDetPredict(model, source, bmp.Height, bmp.Width, channel, max_box, result, visualize);
            if (res)
            {
                for (int j = 3; j < 37; j = j + 6)
                {
                    for (int k = 3; k < 37; k = k + 6)
                    {
                        //约束 左下角横、纵坐标距离、命中精度
                        if ((Math.Abs(result[j] - result[k]) < 30) && (Math.Abs(result[j + 1] - result[k + 1]) < 20) && (j != k) && (result[j - 1] > 0.45) && (result[k - 1] > 0.45))
                        {
                            if (result[j] < result[k])
                            {
                                num1 = (int)result[j - 2];
                                num2 = (int)result[k - 2];
                            }
                            else
                            {
                                num1 = (int)result[k - 2];
                                num2 = (int)result[j - 2];
                            }
                            num = num1 * 10 + num2;
                            return num;
                        }
                    }
                }
            }
            num = (int)result[1];
            return num;
        }



        //重载Inference预测函数，输入bitmap，输出包号
        public static int Inference(Bitmap bmp)
        {
            Bitmap bmpNew = bmp.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height), bmp.PixelFormat);
            //     Bitmap resultShow;
            //Mat img = BitmapConverter.ToMat(bmpNew);
            int channel = Image.GetPixelFormatSize(bmp.PixelFormat) / 8;
            byte[] source = GetbyteData(bmp);
            int num1, num2;
            int num;
            int max_box = 10;
            float[] result = new float[max_box * 6 + 1];
            bool res = PaddlexDetPredict(model, source, bmp.Height, bmp.Width, channel, max_box, result, visualize);
            if (res)
            {
                for (int j = 3; j < 37; j = j + 6)
                {
                    for (int k = 3; k < 37; k = k + 6)
                    {
                        //约束 左下角横、纵坐标距离、命中精度
                        if ((Math.Abs(result[j] - result[k]) < 30) && (Math.Abs(result[j + 1] - result[k + 1]) < 20) && (j != k) && (result[j - 1] > 0.45) && (result[k - 1] > 0.45))
                        {
                            if (result[j] < result[k])
                            {
                                num1 = (int)result[j - 2];
                                num2 = (int)result[k - 2];
                            }
                            else
                            {
                                num1 = (int)result[k - 2];
                                num2 = (int)result[j - 2];
                            }
                            num = num1 * 10 + num2;
                            return num;
                        }
                    }
                }
            }
            num = (int)result[1];
            return num;
        }

        // 将Btimap类转换为byte[]类函数
        private static byte[] GetbyteData(Bitmap bmp)
        {
            BitmapData bmpData = null;
            bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);
            int numbytes = bmpData.Stride * bmpData.Height;
            byte[] byteData = new byte[numbytes];
            IntPtr ptr = bmpData.Scan0;
            Marshal.Copy(ptr, byteData, 0, numbytes);
            return byteData;
        }
        private static Bitmap ReadImageFile(String path)
        {
            Bitmap bitmap = null;
            try
            {
                FileStream fileStream = File.OpenRead(path);
                Int32 filelength = 0;
                filelength = (int)fileStream.Length;
                Byte[] image = new Byte[filelength];
                fileStream.Read(image, 0, filelength);
                System.Drawing.Image result = System.Drawing.Image.FromStream(fileStream);
                fileStream.Close();
                bitmap = new Bitmap(result);
            }
            catch (Exception ex)
            {
            }
            return bitmap;
        }
    }
}

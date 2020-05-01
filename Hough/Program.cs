using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Hough
{
    class Program
    {

        /// <summary>
        /// 检测直线
        /// </summary>
        /// <param name="cross_num">hough变换后的曲线交点个数，取值越大，找出的直线越少</param>
        public static Bitmap HoughLine(Bitmap inputBitmap, int cross_num = 100)
        {
            int width = inputBitmap.Width;
            int height = inputBitmap.Height;
            int rho_max = (int)Math.Floor(Math.Sqrt(width * width + height * height)) + 1; //由原图数组坐标算出ρ最大值，并取整数部分加1
                                                                                           //此值作为ρ，θ坐标系ρ最大值
            var PreLines = new Dictionary<(int, int), int>();// int[rho_max, 180]; //定义ρ，θ坐标系的数组，初值为0。θ的最大值，180度
            

            static double AngleToRadians(int angle) => angle * Math.PI / 180.0;
            static double GetRho(int x, int y, int k) => (y * Math.Sin(AngleToRadians(k))) + (x * Math.Cos(AngleToRadians(k)));

            //try all thetas for every point
            //to get all rho thetas
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var pixel = inputBitmap.GetPixel(x, y);
                    if (pixel.ToArgb() != Color.White.ToArgb()) //ignore while pixels
                    {
                        for (int k = 0; k < 180; k++)
                        {
                            //将θ值代入hough变换方程，求ρ值
                            var rho = GetRho(x,y,k);
                            //将ρ值与ρ最大值的和的一半作为ρ的坐标值（数组坐标），这样做是为了防止ρ值出现负数
                            var rhoIndex = (int)Math.Round(rho / 2 + rho_max / 2);
                            //在ρθ坐标（数组）中标识点，即计数累加
                            //RhoThetas[rhoIndex, k]++;
                            var key = (rhoIndex, k);
                            if(!PreLines.TryGetValue(key,out var count))
                            {
                                PreLines.Add(key, 1);
                            }
                            else
                            {
                                PreLines[key] = count + 1;
                            }
                        }
                    }
                }
            }

            //Extract lines
            var lines = PreLines.Where(z => z.Value >= cross_num).Select(z => (z.Key.Item1, z.Key.Item2)).ToList();
            //Console.WriteLine("Found Lines:");
            //foreach(var line in lines)
            //{
            //    Console.WriteLine($"Rho={line.Item1},Theta={line.Item2}");
            //}
            //把这些点构成的直线提取出来,输出图像数组为outputBitmap
            var outputBitmap = new Bitmap(width, height);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    //首先设置为白色
                    outputBitmap.SetPixel(x, y, Color.White);

                    var pixel = inputBitmap.GetPixel(x, y);
                    if (pixel.ToArgb() != Color.White.ToArgb()) //ignore while pixels
                    {
                        for (int k = 0; k < 180; k++)
                        {
                            var rho = GetRho(x, y, k);
                            var rho_int = (int)Math.Round(rho / 2 + rho_max / 2);
                            
                            //如果正在计算的点属于100像素以上点，则把它提取出来
                            if(lines.Any(l=>l.Item1 == rho_int && l.Item2 == k))
                            {
                                outputBitmap.SetPixel(x, y, Color.Red);
                            }
                        }
                    }
                }
            }
            return outputBitmap;
        }
        static void Main(string[] args)
        {
            using var inbmp = Bitmap.FromFile("Line.bmp") as Bitmap;
            using var outbmp = HoughLine(inbmp);

            outbmp.Save("Found.bmp");
        }
    }
}

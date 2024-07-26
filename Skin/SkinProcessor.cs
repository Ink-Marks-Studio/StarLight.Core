using System.Drawing;
using System.Drawing.Imaging;

namespace StarLight_Core.Skin
{
    public class SkinProcessor
    {
        /// <summary>
        /// 获取 Alex 皮肤左腿
        /// </summary>
        /// <param name="base64Image"></param>
        /// <param name="outputFilePath"></param>
        /// <param name="isDecoration"></param>
        public static void GetLiftLeg(string base64Image, string outputFilePath, bool isDecoration = false)
        {
            byte[] imageBytes = Convert.FromBase64String(base64Image);

            if (isDecoration)
            {
                using (var ms = new MemoryStream(imageBytes))
                using (var image = Image.FromStream(ms))
                using (var bitmap = new Bitmap(image))
                {
                    var rect1 = new Rectangle(20, 52, 4, 12);
                    var rect2 = new Rectangle(4, 52, 4, 12);
                    
                    using (var bmp1 = new Bitmap(4, 12))
                    using (var bmp2 = new Bitmap(4, 12))
                    using (var graphics1 = Graphics.FromImage(bmp1))
                    using (var graphics2 = Graphics.FromImage(bmp2))
                    {
                        graphics1.DrawImage(bitmap, new Rectangle(0, 0, 4, 12), rect1, GraphicsUnit.Pixel);
                        graphics2.DrawImage(bitmap, new Rectangle(0, 0, 4, 12), rect2, GraphicsUnit.Pixel);
                        
                        using (var combinedImage = new Bitmap(4, 12))
                        using (var combinedGraphics = Graphics.FromImage(combinedImage))
                        {
                            combinedGraphics.Clear(Color.Transparent);
                            combinedGraphics.DrawImage(bmp1, 0, 0);
                            combinedGraphics.DrawImage(bmp2, 0, 0);
                            
                            combinedImage.Save(outputFilePath, ImageFormat.Png);
                        }
                    }
                }
            }
            else
            {
                using (var ms = new MemoryStream(imageBytes))
                using (var image = Image.FromStream(ms))
                using (var bitmap = new Bitmap(image))
                {
                    var rect1 = new Rectangle(20, 52, 4, 12);
                    
                    using (var bmp1 = new Bitmap(4, 12))
                    using (var graphics1 = Graphics.FromImage(bmp1))
                    {
                        graphics1.DrawImage(bitmap, new Rectangle(0, 0, 4, 12), rect1, GraphicsUnit.Pixel);
                        
                        bmp1.Save(outputFilePath, ImageFormat.Png);
                    }
                }
            }
        }
        
        /// <summary>
        /// 获取 Alex 皮肤右腿
        /// </summary>
        /// <param name="base64Image"></param>
        /// <param name="outputFilePath"></param>
        /// <param name="isDecoration"></param>
        public static void GetRightLeg(string base64Image, string outputFilePath, bool isDecoration = false)
        {
            byte[] imageBytes = Convert.FromBase64String(base64Image);

            if (isDecoration)
            {
                using (var ms = new MemoryStream(imageBytes))
                using (var image = Image.FromStream(ms))
                using (var bitmap = new Bitmap(image))
                {
                    var rect1 = new Rectangle(4, 20, 4, 12);
                    var rect2 = new Rectangle(4, 36, 4, 12);
                    
                    using (var bmp1 = new Bitmap(4, 12))
                    using (var bmp2 = new Bitmap(4, 12))
                    using (var graphics1 = Graphics.FromImage(bmp1))
                    using (var graphics2 = Graphics.FromImage(bmp2))
                    {
                        graphics1.DrawImage(bitmap, new Rectangle(0, 0, 4, 12), rect1, GraphicsUnit.Pixel);
                        graphics2.DrawImage(bitmap, new Rectangle(0, 0, 4, 12), rect2, GraphicsUnit.Pixel);
                        
                        using (var combinedImage = new Bitmap(4, 12))
                        using (var combinedGraphics = Graphics.FromImage(combinedImage))
                        {
                            combinedGraphics.Clear(Color.Transparent);
                            combinedGraphics.DrawImage(bmp1, 0, 0);
                            combinedGraphics.DrawImage(bmp2, 0, 0);
                            
                            combinedImage.Save(outputFilePath, ImageFormat.Png);
                        }
                    }
                }
            }
            else
            {
                using (var ms = new MemoryStream(imageBytes))
                using (var image = Image.FromStream(ms))
                using (var bitmap = new Bitmap(image))
                {
                    var rect1 = new Rectangle(4, 20, 4, 12);
                    
                    using (var bmp1 = new Bitmap(4, 12))
                    using (var graphics1 = Graphics.FromImage(bmp1))
                    {
                        graphics1.DrawImage(bitmap, new Rectangle(0, 0, 4, 12), rect1, GraphicsUnit.Pixel);
                        
                        bmp1.Save(outputFilePath, ImageFormat.Png);
                    }
                }
            }
        }
        
        /// <summary>
        /// 获取 Alex 皮肤左手
        /// </summary>
        /// <param name="base64Image"></param>
        /// <param name="outputFilePath"></param>
        /// <param name="isDecoration"></param>
        public static void GetLiftArm_Alex(string base64Image, string outputFilePath, bool isDecoration = false)
        {
            byte[] imageBytes = Convert.FromBase64String(base64Image);

            if (isDecoration)
            {
                using (var ms = new MemoryStream(imageBytes))
                using (var image = Image.FromStream(ms))
                using (var bitmap = new Bitmap(image))
                {
                    var rect1 = new Rectangle(36, 52, 3, 12);
                    var rect2 = new Rectangle(52, 52, 3, 12);
                    
                    using (var bmp1 = new Bitmap(3, 12))
                    using (var bmp2 = new Bitmap(3, 12))
                    using (var graphics1 = Graphics.FromImage(bmp1))
                    using (var graphics2 = Graphics.FromImage(bmp2))
                    {
                        graphics1.DrawImage(bitmap, new Rectangle(0, 0, 3, 12), rect1, GraphicsUnit.Pixel);
                        graphics2.DrawImage(bitmap, new Rectangle(0, 0, 3, 12), rect2, GraphicsUnit.Pixel);
                        
                        using (var combinedImage = new Bitmap(3, 12))
                        using (var combinedGraphics = Graphics.FromImage(combinedImage))
                        {
                            combinedGraphics.Clear(Color.Transparent);
                            combinedGraphics.DrawImage(bmp1, 0, 0);
                            combinedGraphics.DrawImage(bmp2, 0, 0);
                            
                            combinedImage.Save(outputFilePath, ImageFormat.Png);
                        }
                    }
                }
            }
            else
            {
                using (var ms = new MemoryStream(imageBytes))
                using (var image = Image.FromStream(ms))
                using (var bitmap = new Bitmap(image))
                {
                    var rect1 = new Rectangle(36, 52, 3, 12);
                    
                    using (var bmp1 = new Bitmap(3, 12))
                    using (var graphics1 = Graphics.FromImage(bmp1))
                    {
                        graphics1.DrawImage(bitmap, new Rectangle(0, 0, 3, 12), rect1, GraphicsUnit.Pixel);
                        
                        bmp1.Save(outputFilePath, ImageFormat.Png);
                    }
                }
            }
        }
        
        /// <summary>
        /// 获取 Alex 皮肤右手
        /// </summary>
        /// <param name="base64Image"></param>
        /// <param name="outputFilePath"></param>
        /// <param name="isDecoration"></param>
        public static void GetRightArm_Alex(string base64Image, string outputFilePath, bool isDecoration = false)
        {
            byte[] imageBytes = Convert.FromBase64String(base64Image);

            if (isDecoration)
            {
                using (var ms = new MemoryStream(imageBytes))
                using (var image = Image.FromStream(ms))
                using (var bitmap = new Bitmap(image))
                {
                    var rect1 = new Rectangle(44, 20, 3, 12);
                    var rect2 = new Rectangle(44, 36, 3, 12);
                    
                    using (var bmp1 = new Bitmap(3, 12))
                    using (var bmp2 = new Bitmap(3, 12))
                    using (var graphics1 = Graphics.FromImage(bmp1))
                    using (var graphics2 = Graphics.FromImage(bmp2))
                    {
                        graphics1.DrawImage(bitmap, new Rectangle(0, 0, 3, 12), rect1, GraphicsUnit.Pixel);
                        graphics2.DrawImage(bitmap, new Rectangle(0, 0, 3, 12), rect2, GraphicsUnit.Pixel);
                        
                        using (var combinedImage = new Bitmap(3, 12))
                        using (var combinedGraphics = Graphics.FromImage(combinedImage))
                        {
                            combinedGraphics.Clear(Color.Transparent);
                            combinedGraphics.DrawImage(bmp1, 0, 0);
                            combinedGraphics.DrawImage(bmp2, 0, 0);
                            
                            combinedImage.Save(outputFilePath, ImageFormat.Png);
                        }
                    }
                }
            }
            else
            {
                using (var ms = new MemoryStream(imageBytes))
                using (var image = Image.FromStream(ms))
                using (var bitmap = new Bitmap(image))
                {
                    var rect1 = new Rectangle(44, 20, 3, 12);
                    
                    using (var bmp1 = new Bitmap(3, 12))
                    using (var graphics1 = Graphics.FromImage(bmp1))
                    {
                        graphics1.DrawImage(bitmap, new Rectangle(0, 0, 3, 12), rect1, GraphicsUnit.Pixel);
                        
                        bmp1.Save(outputFilePath, ImageFormat.Png);
                    }
                }
            }
        }
        
        /// <summary>
        /// 获取 Steve 皮肤左手
        /// </summary>
        /// <param name="base64Image"></param>
        /// <param name="outputFilePath"></param>
        /// <param name="isDecoration"></param>
        public static void GetLiftArm_Steve(string base64Image, string outputFilePath, bool isDecoration = false)
        {
            byte[] imageBytes = Convert.FromBase64String(base64Image);

            if (isDecoration)
            {
                using (var ms = new MemoryStream(imageBytes))
                using (var image = Image.FromStream(ms))
                using (var bitmap = new Bitmap(image))
                {
                    var rect1 = new Rectangle(36, 52, 4, 12);
                    var rect2 = new Rectangle(52, 52, 4, 12);
                    
                    using (var bmp1 = new Bitmap(4, 12))
                    using (var bmp2 = new Bitmap(4, 12))
                    using (var graphics1 = Graphics.FromImage(bmp1))
                    using (var graphics2 = Graphics.FromImage(bmp2))
                    {
                        graphics1.DrawImage(bitmap, new Rectangle(0, 0, 4, 12), rect1, GraphicsUnit.Pixel);
                        graphics2.DrawImage(bitmap, new Rectangle(0, 0, 4, 12), rect2, GraphicsUnit.Pixel);
                        
                        using (var combinedImage = new Bitmap(4, 12))
                        using (var combinedGraphics = Graphics.FromImage(combinedImage))
                        {
                            combinedGraphics.Clear(Color.Transparent);
                            combinedGraphics.DrawImage(bmp1, 0, 0);
                            combinedGraphics.DrawImage(bmp2, 0, 0);
                            
                            combinedImage.Save(outputFilePath, ImageFormat.Png);
                        }
                    }
                }
            }
            else
            {
                using (var ms = new MemoryStream(imageBytes))
                using (var image = Image.FromStream(ms))
                using (var bitmap = new Bitmap(image))
                {
                    var rect1 = new Rectangle(36, 52, 4, 12);
                    
                    using (var bmp1 = new Bitmap(4, 12))
                    using (var graphics1 = Graphics.FromImage(bmp1))
                    {
                        graphics1.DrawImage(bitmap, new Rectangle(0, 0, 4, 12), rect1, GraphicsUnit.Pixel);
                        
                        bmp1.Save(outputFilePath, ImageFormat.Png);
                    }
                }
            }
        }
        
        /// <summary>
        /// 获取 Steve 皮肤右手
        /// </summary>
        /// <param name="base64Image"></param>
        /// <param name="outputFilePath"></param>
        /// <param name="isDecoration"></param>
        public static void GetRightArm_Steve(string base64Image, string outputFilePath, bool isDecoration = false)
        {
            byte[] imageBytes = Convert.FromBase64String(base64Image);

            if (isDecoration)
            {
                using (var ms = new MemoryStream(imageBytes))
                using (var image = Image.FromStream(ms))
                using (var bitmap = new Bitmap(image))
                {
                    var rect1 = new Rectangle(44, 20, 4, 12);
                    var rect2 = new Rectangle(44, 36, 4, 12);
                    
                    using (var bmp1 = new Bitmap(4, 12))
                    using (var bmp2 = new Bitmap(4, 12))
                    using (var graphics1 = Graphics.FromImage(bmp1))
                    using (var graphics2 = Graphics.FromImage(bmp2))
                    {
                        graphics1.DrawImage(bitmap, new Rectangle(0, 0, 4, 12), rect1, GraphicsUnit.Pixel);
                        graphics2.DrawImage(bitmap, new Rectangle(0, 0, 4, 12), rect2, GraphicsUnit.Pixel);
                        
                        using (var combinedImage = new Bitmap(4, 12))
                        using (var combinedGraphics = Graphics.FromImage(combinedImage))
                        {
                            combinedGraphics.Clear(Color.Transparent);
                            combinedGraphics.DrawImage(bmp1, 0, 0);
                            combinedGraphics.DrawImage(bmp2, 0, 0);
                            
                            combinedImage.Save(outputFilePath, ImageFormat.Png);
                        }
                    }
                }
            }
            else
            {
                using (var ms = new MemoryStream(imageBytes))
                using (var image = Image.FromStream(ms))
                using (var bitmap = new Bitmap(image))
                {
                    var rect1 = new Rectangle(44, 20, 4, 12);
                    
                    using (var bmp1 = new Bitmap(4, 12))
                    using (var graphics1 = Graphics.FromImage(bmp1))
                    {
                        graphics1.DrawImage(bitmap, new Rectangle(0, 0, 4, 12), rect1, GraphicsUnit.Pixel);
                        
                        bmp1.Save(outputFilePath, ImageFormat.Png);
                    }
                }
            }
        }
        
        /// <summary>
        /// 获取皮肤身体
        /// </summary>
        /// <param name="base64Image"></param>
        /// <param name="outputFilePath"></param>
        /// <param name="isDecoration"></param>
        public static void GetBody(string base64Image, string outputFilePath, bool isDecoration = false)
        {
            byte[] imageBytes = Convert.FromBase64String(base64Image);

            if (isDecoration)
            {
                using (var ms = new MemoryStream(imageBytes))
                using (var image = Image.FromStream(ms))
                using (var bitmap = new Bitmap(image))
                {
                    var rect1 = new Rectangle(20, 20, 8, 12);
                    var rect2 = new Rectangle(20, 36, 8, 12);
                    
                    using (var bmp1 = new Bitmap(8, 12))
                    using (var bmp2 = new Bitmap(8, 12))
                    using (var graphics1 = Graphics.FromImage(bmp1))
                    using (var graphics2 = Graphics.FromImage(bmp2))
                    {
                        graphics1.DrawImage(bitmap, new Rectangle(0, 0, 8, 12), rect1, GraphicsUnit.Pixel);
                        graphics2.DrawImage(bitmap, new Rectangle(0, 0, 8, 12), rect2, GraphicsUnit.Pixel);
                        
                        using (var combinedImage = new Bitmap(8, 12))
                        using (var combinedGraphics = Graphics.FromImage(combinedImage))
                        {
                            combinedGraphics.Clear(Color.Transparent);
                            combinedGraphics.DrawImage(bmp1, 0, 0);
                            combinedGraphics.DrawImage(bmp2, 0, 0);
                            
                            combinedImage.Save(outputFilePath, ImageFormat.Png);
                        }
                    }
                }
            }
            else
            {
                using (var ms = new MemoryStream(imageBytes))
                using (var image = Image.FromStream(ms))
                using (var bitmap = new Bitmap(image))
                {
                    var rect1 = new Rectangle(20, 20, 8, 12);
                    
                    using (var bmp1 = new Bitmap(8, 12))
                    using (var graphics1 = Graphics.FromImage(bmp1))
                    {
                        graphics1.DrawImage(bitmap, new Rectangle(0, 0, 8, 12), rect1, GraphicsUnit.Pixel);
                        
                        bmp1.Save(outputFilePath, ImageFormat.Png);
                    }
                }
            }
        }

        /// <summary>
        /// 获取皮肤头像
        /// </summary>
        /// <param name="base64Image"></param>
        /// <param name="outputFilePath"></param>
        public static void GetAvatar(string base64Image, string outputFilePath, bool isHeadwear = false)
        {
            byte[] imageBytes = Convert.FromBase64String(base64Image);

            if (isHeadwear)
            {
                using (var ms = new MemoryStream(imageBytes))
                using (var image = Image.FromStream(ms))
                using (var bitmap = new Bitmap(image))
                {
                    var rect1 = new Rectangle(8, 8, 8, 8);
                    var rect2 = new Rectangle(40, 8, 8, 8);
                    
                    using (var bmp1 = new Bitmap(8, 8))
                    using (var bmp2 = new Bitmap(8, 8))
                    using (var graphics1 = Graphics.FromImage(bmp1))
                    using (var graphics2 = Graphics.FromImage(bmp2))
                    {
                        graphics1.DrawImage(bitmap, new Rectangle(0, 0, 8, 8), rect1, GraphicsUnit.Pixel);
                        graphics2.DrawImage(bitmap, new Rectangle(0, 0, 8, 8), rect2, GraphicsUnit.Pixel);
                        
                        using (var combinedImage = new Bitmap(8, 8))
                        using (var combinedGraphics = Graphics.FromImage(combinedImage))
                        {
                            combinedGraphics.Clear(Color.Transparent);
                            combinedGraphics.DrawImage(bmp1, 0, 0);
                            combinedGraphics.DrawImage(bmp2, 0, 0);
                            
                            combinedImage.Save(outputFilePath, ImageFormat.Png);
                        }
                    }
                }
            }
            else
            {
                using (var ms = new MemoryStream(imageBytes))
                using (var image = Image.FromStream(ms))
                using (var bitmap = new Bitmap(image))
                {
                    var rect1 = new Rectangle(8, 8, 8, 8);
                    
                    using (var bmp1 = new Bitmap(8, 8))
                    using (var graphics1 = Graphics.FromImage(bmp1))
                    {
                        graphics1.DrawImage(bitmap, new Rectangle(0, 0, 8, 8), rect1, GraphicsUnit.Pixel);
                        
                        bmp1.Save(outputFilePath, ImageFormat.Png);
                    }
                }
            }
        }
    }
}
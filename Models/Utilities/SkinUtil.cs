using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace StarLight_Core.Models.Utilities
{
    public static class SkinUtil
    {
        /// <summary>
        /// 皮肤是否为 Alex 类型
        /// </summary>
        /// <param name="base64Image">Base64 编码的图像字符串</param>
        /// <returns>是否为 Alex 皮肤</returns>
        public static bool IsAlexSkin(string base64Image)
        {
            byte[] imageBytes = Convert.FromBase64String(base64Image);

            using (var ms = new MemoryStream(imageBytes))
            using (var image = Image.FromStream(ms))
            using (var bitmap = new Bitmap(image))
            {
                Color pixelColor = bitmap.GetPixel(54, 20);
                return pixelColor.A == 0;
            }
        }
        
        /// <summary>
        /// 是否为双层皮肤
        /// </summary>
        /// <param name="base64Image">Base64 编码的图像字符串</param>
        /// <returns>是</returns>
        public static bool IsNewSkin(string base64Image)
        {
            byte[] imageBytes = Convert.FromBase64String(base64Image);
            
            using (var ms = new MemoryStream(imageBytes))
            {
                using (var image = Image.FromStream(ms))
                {
                    if (image.Width == 64 & image.Height == 64)
                    {
                        return true;
                    }
                    else if (image.Height == 32)
                    {
                        return false;
                    }
                    else
                    {
                        throw new Exception("皮肤不符合规范");
                    }
                }
            }
        }
    }
}
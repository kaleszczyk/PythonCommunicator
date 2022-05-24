using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketClient
{
    public static class BitmapExtensions
    {
        public static byte[] ToByteArray(this Image image, ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, format);
                return ms.ToArray();
            }
        }

        public static Bitmap Resize(this Bitmap bitmap, Size size)
        {
            Bitmap result = null;
            try
            {
                result = new Bitmap(bitmap, size);
            }
            catch (Exception ex)
            {
                throw;
            }

            return result;
        }
    }
}

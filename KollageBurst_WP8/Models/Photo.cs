using System;
using System.Windows.Media.Imaging;

namespace KollageBurst_WP8.Models
{
    public class Photo : IDisposable
    {
        private WriteableBitmap picture;

        public Photo(WriteableBitmap picture)
        {
            this.picture = picture;
        }

        public Photo(System.IO.Stream stream)
        {
            BitmapImage bitmap = new BitmapImage();
            bitmap.SetSource(stream);
            this.picture = new WriteableBitmap(bitmap);
        }

        public WriteableBitmap PhotoThumbnail
        {
            get
            {
                if (picture != null)
                {
                    this.picture.Resize(100, 100, WriteableBitmapExtensions.Interpolation.NearestNeighbor);
                }

                return new WriteableBitmap(100, 100);
            }
        }

        public WriteableBitmap PhotoImage { get { return this.picture; } }

        public void Dispose()
        {
            this.picture = null;
        }
    }
}

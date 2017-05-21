using GalaSoft.MvvmLight.Messaging;
using KollageBurst_WP8.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KollageBurst_WP8.Messages
{
    public class CurrentPhotoChangedMessage : MessageBase
    {
        public Photo CurrentPhoto { get; set; }
        public Action<Photo> Callback { get; set; }

        public CurrentPhotoChangedMessage(Photo currentPhoto)
        {
            this.CurrentPhoto = currentPhoto;
        }
    }
}

using GalaSoft.MvvmLight.Messaging;
using KollageBurst_WP8.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KollageBurst_WP8.Messages
{
    public class NavigateToPageMessage : MessageBase
    {
        public string PageName { get; private set; }

        public NavigateToPageMessage(string pageName)
        {
            this.PageName = pageName;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KollageBurst_WP8.Models
{
    [Table]
    public class PhotoMetadata : INotifyPropertyChanging, INotifyPropertyChanged
    {
        #region Table definition 

        // Define ID: private field, public property and database column.
        private int photoColorId;

        [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "INT NOT NULL Identity", CanBeNull = false, AutoSync = AutoSync.OnInsert)]
        public int PhotoColorId
        {
            get
            {
                return photoColorId;
            }
            set
            {
                if (photoColorId != value)
                {
                    NotifyPropertyChanging("PhotoColorId");
                    photoColorId = value;
                    NotifyPropertyChanged("PhotoColorId");
                }
            }
        }

        // Define photo name: private field, public property and database column.
        private string photoName;

        [Column]
        public string PhotoName
        {
            get
            {
                return photoName;
            }
            set
            {
                if (photoName != value)
                {
                    NotifyPropertyChanging("PhotoName");
                    photoName = value;
                    NotifyPropertyChanged("PhotoName");
                }
            }
        }

        // Define photo name: private field, public property and database column.
        private string photoColor;

        [Column]
        public string PhotoColor
        {
            get
            {
                return photoColor;
            }
            set
            {
                if (photoColor != value)
                {
                    NotifyPropertyChanging("PhotoColor");
                    photoColor = value;
                    NotifyPropertyChanged("PhotoColor");
                }
            }
        }

        // Version column aids update performance.
        [Column(IsVersion = true)]
        private Binary _version;

#endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        // Used to notify the page that a data context property changed
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region INotifyPropertyChanging Members

        public event PropertyChangingEventHandler PropertyChanging;

        // Used to notify the data context that a data context property is about to change
        private void NotifyPropertyChanging(string propertyName)
        {
            if (PropertyChanging != null)
            {
                PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
            }
        }

        #endregion
    }
}

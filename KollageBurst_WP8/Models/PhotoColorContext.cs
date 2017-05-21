using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KollageBurst_WP8.Models
{
    public class PhotoColorContext : DataContext
    {
        private const string DBConnectionString = "Data Source=isostore:/KollageBurst.sdf";

        public PhotoColorContext(string connectionString = DBConnectionString)
            : base(connectionString)
        { }

        public Table<PhotoMetadata> PhotoColors;
    }
}

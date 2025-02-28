using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WpfBeauty.Models;

namespace WpfBeauty
{
    public class Helper
    {
        public static SalonEntities ent;
        public static SalonEntities GetContext()
        {
            if (ent == null)
            {
                ent = new SalonEntities();
            }
            return ent;
        }
    }
}

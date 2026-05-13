using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UP01
{
    public class Core
    {
        private static JokeAndKingEntities _db;

        public static JokeAndKingEntities DB => GetContext();

        public static JokeAndKingEntities GetContext()
        {
            if (_db == null)
            {
                _db = new JokeAndKingEntities();
            }
            return _db;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamWatcher.Steam
{
    public struct Changelist
    {
        public readonly uint ChangeNumber;
        public readonly List<uint> Apps;
        public readonly List<uint> Packages;

        public Changelist(uint changeNumber, IEnumerable<uint> apps, IEnumerable<uint> packages)
        {
            if (apps == null)
            {
                throw new ArgumentNullException(nameof(apps));
            }

            if (packages == null)
            {
                throw new ArgumentNullException(nameof(packages));
            }

            ChangeNumber = changeNumber;
            Apps = new List<uint>(apps);
            Packages = new List<uint>(packages);
        }
    }
}

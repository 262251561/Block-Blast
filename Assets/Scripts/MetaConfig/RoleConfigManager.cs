using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Config
{
    public class RoleConfig
    {
        public int id;

        public int attack;

        public int defence;

        public int hp;

        public float moveSpeed;

        public string prefab;
    }

    public class RoleConfigManager : ConfigManagerBase<RoleConfig, RoleConfigManager>
    {

    }
}

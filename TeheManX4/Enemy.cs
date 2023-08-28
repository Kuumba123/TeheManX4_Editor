using System.Collections.Generic;

namespace TeheManX4
{
    class Enemy
    {
        #region Properties
        public byte id;
        public byte var;
        public byte type;
        public short x;
        public short y;
        public byte range;
        #endregion Properties

        #region Constructors
        public Enemy()
        {
        }
        #endregion Constructors
    }
    class EnemyCollection
    {
        public List<Enemy> enemies;
        public List<Enemy> startEnemies;
    }
}

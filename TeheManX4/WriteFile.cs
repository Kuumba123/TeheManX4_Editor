namespace TeheManX4
{
    class WriteFile
    {
        #region Properties
        public uint lba;
        public string name;
        public string path;
        public int size;
        public bool isFolder = false;
        public long offset = 0;
        #endregion

        #region Constructors
        internal WriteFile()
        {

        }
        internal WriteFile(uint lba, string name, string path, int size)
        {
            this.lba = lba;
            this.name = name;
            this.path = path;
            this.size = size;
        }
        #endregion Constructors
    }
}

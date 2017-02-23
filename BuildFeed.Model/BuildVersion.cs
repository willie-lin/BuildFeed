namespace BuildFeed.Model
{
    public struct BuildVersion
    {
        public uint Major { get; set; }
        public uint Minor { get; set; }

        public BuildVersion(uint major, uint minor)
        {
            Major = major;
            Minor = minor;
        }

        public override string ToString() => $"{Major}.{Minor}";
    }
}
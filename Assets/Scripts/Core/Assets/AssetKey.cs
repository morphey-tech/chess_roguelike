namespace Project.Core.Assets
{
    public readonly struct AssetKey
    {
        public string Address { get; }

        public AssetKey(string address)
        {
            Address = address;
        }

        public static implicit operator AssetKey(string address)
        {
            return new AssetKey(address);
        }

        public static implicit operator string(AssetKey key)
        {
            return key.Address;
        }
    }
}
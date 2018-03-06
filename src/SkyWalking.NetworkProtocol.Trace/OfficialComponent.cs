namespace SkyWalking.NetworkProtocol.Trace
{
    public class OfficialComponent : IComponent
    {
        public OfficialComponent(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; }

        public string Name { get; }
    }
}

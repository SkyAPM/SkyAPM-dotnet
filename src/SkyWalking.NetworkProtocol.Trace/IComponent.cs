namespace SkyWalking.NetworkProtocol.Trace
{
    /// <summary>
    /// The <code>Component</code> represents component library , which has been supported by skywalking
    /// </summary>
    public interface IComponent
    {
        int Id { get; }
        string Name { get; }
    }
}

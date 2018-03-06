using System.Collections.Generic;

namespace SkyWalking.NetworkProtocol.Trace
{
    public class ComponentsDefine
    {
        public static readonly OfficialComponent AspNetCore = new OfficialComponent(1, "AspNetCore");

        private static readonly ComponentsDefine _instance = new ComponentsDefine();

        public ComponentsDefine Instance
        {
            get
            {
                return _instance;
            }
        }

        private Dictionary<int, string> _components;

        private ComponentsDefine()
        {
            _components = new Dictionary<int, string>();
        }

        private void AddComponent(OfficialComponent component)
        {
            _components[component.Id] = component.Name;
        }

        public string GetComponentName(int componentId)
        {
            if (_components.TryGetValue(componentId, out var value))
            {
                return value;
            }
            return null;
        }
    }
}

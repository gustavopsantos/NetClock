using System.Collections.Generic;

namespace Artemis2023.ValueObjects
{
    public class PacketSequenceStorage
    {
        private readonly Dictionary<(Address, DeliveryMethod), int> _registry = new Dictionary<(Address, DeliveryMethod), int>();

        public int Get(Address address, DeliveryMethod deliveryMethod, int defaultValue)
        {
            var key = (address, deliveryMethod);
            if (!_registry.ContainsKey(key))
            {
                _registry.Add(key, defaultValue);
            }

            return _registry[key];
        }

        public void Set(Address address, DeliveryMethod deliveryMethod, int value)
        {
            _registry[(address, deliveryMethod)] = value;
        }
    }
}
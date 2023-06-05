using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandwych.DeviceTree.Model;

public class DeviceTreeDocument : IDeviceTreeElement {
    public IEnumerable<Directive> Directives { get; }
    public IEnumerable<DeviceNode> DeviceNodes { get; }

    public DeviceTreeDocument(IEnumerable<Directive> directives, IEnumerable<DeviceNode> nodes) {
        this.Directives = directives;
        this.DeviceNodes = nodes;
    }
}

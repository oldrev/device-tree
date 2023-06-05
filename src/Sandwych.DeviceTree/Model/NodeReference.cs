using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandwych.DeviceTree.Model;

internal sealed class NodeReference : IDeviceTreeItem {
    public string Reference { get; }

    public NodeReference(string reference) {
        this.Reference = reference;
    }

    public override string ToString() => $"&{this.Reference}";
}

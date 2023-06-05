using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandwych.DeviceTree.Model;

public sealed class IncludeDirective : Directive {

    public string Path { get; }

    public IncludeDirective(string path) {
        Path = path;
    }
}

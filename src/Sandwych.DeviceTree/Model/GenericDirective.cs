using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandwych.DeviceTree.Model;

public sealed class GenericDirective : Directive {

    public string Directive { get; }

    public GenericDirective(string directive) {
        this.Directive = directive;
    }

}


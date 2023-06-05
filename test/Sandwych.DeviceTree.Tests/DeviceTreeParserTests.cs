using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Sandwych.DeviceTree.Tests;


public class DeviceTreeParserTests {

    [Fact]
    public void SuperSimpleDtsShouldBeOk() {
        var dts1 = """
            /dts-v1/;

            / {
                #address-cells = <1>;
                #size-cells = <1>;
                compatible = "my,design";
            };
        """;
        var root = DeviceTreeParser.DeviceTree.Parse(dts1);
    }

    [Fact]
    public void CanParseFullDtsCode() {
        var dts1 = """
            /dts-v1/;

            / {
                #address-cells = <1>;
                #size-cells = <1>;
                compatible = "my,design";
                aliases {
                    serial0 = "/soc/uart@10000000";
                };
                chosen {
                    stdout-path = "/soc/uart@10000000:115200";
                };
                cpus {
                    #address-cells = <1>;
                    #size-cells = <0>;
                    cpu@0 {
                        compatible = "sifive,rocket0", "riscv";
                        device_type = "cpu";
                        reg = <0>;
                        riscv,isa = "rv32imac";
                        status = "okay";
                        timebase-frequency = <1000000>;
                        sifive,dtim = <&dtim>;
                        interrupt-controller {
                            #interrupt-cells = <1>;
                            compatible = "riscv,cpu-intc";
                            interrupt-controller;
                        };
                    };
                };
                soc {
                    #address-cells = <1>;
                    #size-cells = <1>;
                    compatible = "my,design-soc";
                    ranges;
                    dtim: dtim@20000000 {
                        compatible = "sifive,dtim0";
                        reg = <0x20000000 0x10000000>;
                        reg-names = "mem";
                    };
                    uart: uart@10000000 {
                        compatible = "sifive,uart0";
                        reg = <0x10000000 0x1000>;
                        reg-names = "control";
                    };
                };
            };
            """;

        var doc = DeviceTreeParser.DeviceTree.Parse(dts1);

    }

}

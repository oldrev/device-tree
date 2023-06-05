
using System.Data;
using System.Globalization;
using Parlot;
using Parlot.Fluent;
using Sandwych.DeviceTree.Model;
using PP = Parlot.Fluent.Parsers;

namespace Sandwych.DeviceTree;

// https://github.com/vagrantc/device-tree-compiler/blob/master/Documentation/dts-format.txt

public static class DeviceTreeParser {
    public static readonly Parser<IDeviceTreeItem> Instance;

    static readonly char[] DtsNodeNameExtraChars = ",.-+_".ToCharArray();

    // Literals
    static readonly Parser<char> SlashCharLiteral = PP.Literals.Char('/');
    static readonly Parser<TextSpan> HexCharsLiteral = PP.Literals.Pattern(static c => c.IsHex(), 1, 16);
    static readonly Parser<TextSpan> HexByteLiteral = PP.Literals.Pattern(static c => c.IsHex(), 2, 2);
    static readonly Parser<TextSpan> CHexLiteral = PP.Capture(PP.Literals.Text("0x", true).SkipAnd(HexCharsLiteral));

    // Tokens
    static readonly Parser<char> LBraceTerm = PP.Terms.Char('{');
    static readonly Parser<char> RBraceTerm = PP.Terms.Char('}');
    static readonly Parser<char> LBracketTerm = PP.Terms.Char('[');
    static readonly Parser<char> RBracketTerm = PP.Terms.Char(']');
    static readonly Parser<char> ColonTerm = PP.Terms.Char(':');
    static readonly Parser<char> CommaTerm = PP.Terms.Char(',');
    static readonly Parser<char> SemicolonTerm = PP.Terms.Char(';');
    static readonly Parser<char> AndTerm = PP.Terms.Char('&');
    static readonly Parser<char> EqualCharTerm = PP.Terms.Char('=');
    static readonly Parser<char> SlashCharTerm = PP.Terms.Char('/');
    static readonly Parser<char> QuestionTerm = PP.Terms.Char('?');

    static readonly Parser<TextSpan> HexNumberTerm = PP.SkipWhiteSpace(HexCharsLiteral);
    static readonly Parser<TextSpan> CHexNumberTerm = PP.SkipWhiteSpace(CHexLiteral);
    static readonly Parser<byte> HexByteTerm = PP.SkipWhiteSpace(HexByteLiteral).Then(x => byte.Parse(x.Span, NumberStyles.HexNumber));


    // node_name = p.Word(p.alphanums + ",.-+_") ^ p.Literal("/")
    static readonly Parser<TextSpan> RootNodeNameTerm = PP.Capture(SlashCharTerm);

    static readonly Parser<TextSpan> NodeNamePattern = PP.Terms.Pattern(
        static c => char.IsLetterOrDigit(c) || c == ',' || c == '.' || c == '_' || c == '+' || c == '-');

    public static readonly Parser<TextSpan> NodeNameTerm = RootNodeNameTerm.Or(NodeNamePattern);

    // integer = p.pyparsing_common.integer ^ (p.Literal("0x").suppress() + p.pyparsing_common.hex_integer)
    public static readonly Parser<long> DecimalIntegerTerm = PP.Terms.Pattern(static c => char.IsDigit(c))
        .Then(static x => long.Parse(x.Span, NumberStyles.Number));

    public static readonly Parser<long> HexIntegerTerm = CHexNumberTerm
        .Then(static x => long.Parse(x.Span.Slice(2), NumberStyles.HexNumber));

    public static readonly Parser<long> IntegerTerm = PP.OneOf(HexIntegerTerm, DecimalIntegerTerm); // HexIntegerTerm.Or(DecimalIntegerTerm); //, DecimalIntegerTerm);

    // unit_address = p.pyparsing_common.hex_integer
    public static readonly Parser<ulong> UnitAddressLiteral = HexCharsLiteral
        .Then(static x => ulong.Parse(x.Span, NumberStyles.HexNumber));

    // node_handle = node_name("node_name") + p.Optional(p.Literal("@") + unit_address("address"))
    private static readonly Parser<NodeHandle> NodeHandleTerm =
        NodeNameTerm.And(PP.ZeroOrOne(PP.SkipWhiteSpace(PP.Literals.Char('@').SkipAnd(UnitAddressLiteral))))
        .Then(static x => new NodeHandle(x.Item1.Span.ToString(), x.Item2));

    // property_name = p.Word(p.alphanums + ",.-_+?#")
    public static readonly Parser<TextSpan> PropertyNameTerm = PP.Terms.Pattern(
        static c => char.IsLetterOrDigit(c) || c == ',' || c == '.' || c == '-' || c == '_' || c == '+' || c == '?' || c == '#');

    // label = p.Word(p.alphanums + "_").setResultsName("label")
    public static readonly Parser<TextSpan> LabelTerm = PP.Terms.Identifier();

    // label_definition = p.Combine(label + p.Literal(":"))
    public static readonly Parser<TextSpan> LabelDefinitionTerm = LabelTerm.AndSkip(ColonTerm);


    // string = p.QuotedString(quoteChar='"')
    public static readonly Parser<TextSpan> StringTerm = PP.Terms.String(StringLiteralQuotes.Double);

    //stringlist = p.delimitedList(string)
    public static readonly Parser<DeviceTreeValue> StringListTerm = PP.Separated(CommaTerm, StringTerm)
        .Then(static x => {
            var stringList = x.Select(y => y.Span.ToString());
            return new DeviceTreeValue(x.Select(y => y.Span.ToString()));
        });

    // node_path = p.Combine(p.Literal("/") + \
    //         p.delimitedList(node_handle, delim="/", combine=True)).setResultsName("path")
    public static readonly Parser<TextSpan> NodePathTerm = PP.Capture(PP.SkipWhiteSpace(SlashCharLiteral.And(PP.Separated(SlashCharLiteral, NodeHandleTerm))));

    //path_reference = p.Literal("&{").suppress() + node_path + p.Literal("}").suppress()
    public static readonly Parser<TextSpan> PathReferenceTerm = PP.Between(PP.Capture(PP.Terms.Text("&{")), NodePathTerm, RBraceTerm);

    //label_reference = p.Literal("&").suppress() + label
    public static readonly Parser<TextSpan> LabelReferenceTerm = AndTerm.SkipAnd(LabelTerm).Then(static x => x);

    //reference = path_reference ^ label_reference
    public static readonly Parser<DeviceTreeValue> ReferenceTerm = PathReferenceTerm.Or(LabelReferenceTerm)
        .Then(static x => new DeviceTreeValue(x.Span.ToString(), DevicePropertyValueType.Reference));

    //include_directive = p.Literal("/include/") + p.QuotedString(quoteChar='"')
    public static readonly Parser<IDeviceTreeItem> IncludeDirectiveTerm =
        PP.Terms.Text("/include/").SkipAnd(PP.Terms.String()).Then<IDeviceTreeItem>(static x => new IncludeDirective(x.Span.ToString()));

    //generic_directive = p.QuotedString(quoteChar="/", unquoteResults=False) + \
    //        p.Optional(string ^ property_name ^ node_name ^ reference ^ (integer * 2)) + \
    //        p.Literal(";").suppress()
    public static readonly Parser<IDeviceTreeItem> GenericDirectiveTerm =
        PP.Between(SlashCharLiteral,
                   PP.Capture(PP.ZeroOrOne(PP.OneOf(StringTerm, PropertyNameTerm, NodeNameTerm, PP.Capture(ReferenceTerm)))),
                   SlashCharLiteral).AndSkip(SemicolonTerm)
        .Then<IDeviceTreeItem>(static x => new GenericDirective(x.Span.ToString()));

    //directive = include_directive ^ generic_directive
    public static readonly Parser<IDeviceTreeItem> DirectiveTerm = IncludeDirectiveTerm.Or(GenericDirectiveTerm);


    // operator = p.oneOf("~ ! * / + - << >> < <= > >= == != & ^ | && ||")
    public static readonly Parser<TextSpan> OperatorTerm = PP.OneOf("~ ! * / + - << >> < <= > >= == != & ^ | && ||".Split(' ')
                                .Select(static x => PP.Capture(PP.Terms.Text(x))).ToArray());


    // arith_expr = p.Forward()
    public static readonly Parser<TextSpan> ArithExpr = PP.Recursive<TextSpan>(static ae => {

        // ternary_element = arith_expr ^ integer
        var TernaryElement = ae.Or(PP.Capture(IntegerTerm));

        // ternary_expr = ternary_element + p.Literal("?") + ternary_element + p.Literal(":") + ternary_element
        var TernaryExpr = TernaryElement.And(QuestionTerm).And(TernaryElement).And(ColonTerm).And(TernaryElement);

        // arith_expr = p.nestedExpr(content=(p.OneOrMore(operator ^ integer) ^ ternary_expr))
        var result = PP.Capture(IntegerTerm); // PP.OneOrMany(OperatorTerm.Or(IntegerTerm)).Or(TernaryExpr);

        return result;
    });

    // cell_array = p.Literal("<").suppress() + \
    //         p.ZeroOrMore(integer ^ arith_expr ^ string ^ reference ^ label_definition.suppress()) + \
    //         p.Literal(">").suppress()

    private static readonly Parser<DeviceTreeValue> CellArrayItem = PP.OneOf(
            IntegerTerm.Then(static x => new DeviceTreeValue(x)),
            //ArithExpr.Then(static x = ,
            StringTerm.Then(static x => new DeviceTreeValue(x.Span)),
            ReferenceTerm,
            LabelDefinitionTerm.Then(static x => new DeviceTreeValue(x.Span.ToString(), DevicePropertyValueType.Reference))
        );

    public static readonly Parser<DeviceTreeValue> CellArray = PP.Between(
            PP.Terms.Char('<'),
            PP.ZeroOrMany(CellArrayItem),
            PP.Terms.Char('>')
        ).Then(static x => new DeviceTreeValue(new CellArray(x))); //

    // bytestring = p.Literal("[").suppress() + \
    //         (p.OneOrMore(p.Word(p.hexnums, exact=2) ^ label_definition.suppress())) + \
    //         p.Literal("]").suppress()
    public static readonly Parser<DeviceTreeValue> ByteString = PP.Between(
            LBracketTerm,
            PP.OneOrMany(HexByteTerm), // TODO FIXME 增加 label_definition
            RBracketTerm
        ).Then(static x => new DeviceTreeValue(x));

    // property_values = p.Forward()
    // property_values = p.delimitedList(property_values ^ cell_array ^ bytestring ^ stringlist ^ \
    //                                   reference)
    public static readonly Parser<List<DeviceTreeValue>> PropertyValues = PP.Recursive<List<DeviceTreeValue>>(static pv =>
        PP.Separated(CommaTerm, PP.OneOf(CellArray, ByteString, StringListTerm, ReferenceTerm))
    );

    // property_assignment = property_name("property_name") + p.Optional(p.Literal("=").suppress() + \
    //         (property_values)).setResultsName("value") + p.Literal(";").suppress()
    private static readonly Parser<DeviceProperty> PropertyEqualsEmpty = PropertyNameTerm.AndSkip(SemicolonTerm)
        .Then(static x => new DeviceProperty(x.Span.ToString()));

    private static readonly Parser<DeviceProperty> PropertyEqualsValue = PropertyNameTerm.AndSkip(EqualCharTerm).And(PropertyValues).AndSkip(SemicolonTerm)
        .Then(static x => new DeviceProperty(x.Item1.Span.ToString(), x.Item2.Count == 1 ? x.Item2.Single() : new DeviceTreeValue(new CellArray(x.Item2))));

    public static readonly Parser<IDeviceTreeItem> PropertyAssignment = (PropertyEqualsValue.Or(PropertyEqualsEmpty))
        .Then<IDeviceTreeItem>(static x => x);

    // node_opener = p.Optional(label_definition) + node_handle + p.Literal("{").suppress()
    public static readonly Parser<IDeviceTreeItem> NodeOpener = PP.ZeroOrOne(LabelDefinitionTerm).SkipAnd(NodeHandleTerm).AndSkip(LBraceTerm)
        .Then<IDeviceTreeItem>(static x => x);

    // node_reference_opener = reference + p.Literal("{").suppress()
    public static readonly Parser<IDeviceTreeItem> NodeReferenceOpener = ReferenceTerm.AndSkip(LBraceTerm)
        .Then<IDeviceTreeItem>(static x => new NodeReference(x.StringValue));

    // node_closer = p.Literal("}").suppress() + p.Literal(";").suppress()
    public static readonly Parser<TextSpan> NodeCloser = PP.Capture(RBraceTerm.And(SemicolonTerm));

    // node_definition = p.Forward()
    // node_definition << (node_opener ^ node_reference_opener) + \
    //         p.ZeroOrMore(property_assignment ^ directive ^ node_definition) + \
    //         node_closer
    public static readonly Parser<IDeviceTreeItem> NodeDefinition = PP.Recursive<IDeviceTreeItem>(static nd => {
        var nodeBody = PP.ZeroOrMany(PropertyAssignment.Or(DirectiveTerm).Or(nd));
        var nodeStart = NodeOpener.Or(NodeReferenceOpener);
        var oneNode = nodeStart.And(nodeBody).AndSkip(NodeCloser)
            .Then<IDeviceTreeItem>(static x => new DeviceNode(
                x.Item1.ToString(),
                x.Item2.OfType<DeviceProperty>(),
                x.Item2.OfType<DeviceNode>().ToList()
            )
        );
        return oneNode;
    });

    // devicetree = p.ZeroOrMore(directive ^ node_definition)
    public static readonly Parser<DeviceTreeDocument> DeviceTree = PP.ZeroOrMany(DirectiveTerm.Or(NodeDefinition))
        .Then(static x => {
            var nodes = x.OfType<DeviceNode>();
            var directives = x.OfType<Directive>();
            return new DeviceTreeDocument(directives, nodes);
        });



    /*
    // # pylint: disable=expression-not-assigned

    // devicetree.ignore(p.cStyleComment)
    // devicetree.ignore("//" + p.SkipTo(p.lineEnd))

    // Instance = 
    */

    /*
    public static IDtsNode Parse(string input) {
        if (Instance.TryParse(input, out var result)) {
            return result;
        }
        else {
            throw new SyntaxErrorException(input);
        }
    }
    */
}

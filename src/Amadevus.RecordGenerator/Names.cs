namespace Amadevus.RecordGenerator
{
    internal static class Names
    {
        public const string WithPrefix = "With";
        public const string NodeSuffix = "Node";
        public const string CoreSuffix = "Core"; 
        public const string Empty = "Empty";
        public const string Add = nameof(System.Collections.IList.Add);
        public const string AddRange = nameof(System.Collections.Generic.List<int>.AddRange);
        public const string AddRangeAsBuilders = "AddRangeAsBuilders";
        public const string GetEnumerator = nameof(System.Collections.IEnumerable.GetEnumerator);
        public const string ObsoleteFull = "System.Obsolete";

        public const string NamespaceSystemCollectionsGeneric = "System.Collections.Generic";
        public const string NamespaceSystemCollectionsImmutable = "System.Collections.Immutable";

        public const string IBuilder = "IBuilder";
        public const string IBuildable = "IBuildable";
        public const string ICore = "ICore";
        public const string INodeWithCore = "INodeWithCore";

        public const string NodeCore = "NodeCore";
        public const string SourceNode = "SourceNode";
        public const string SourceTree = "SourceTree";

        public const string ArrayNonGenericFull = "System.Array";
        public const string ICollectionNonGenericFull = "System.Collections.ICollection";
        public const string IEnumeratorNonGenericFull = "System.Collections.IEnumerator";
        public const string IEnumeratorGeneric = "IEnumerator";
        public const string IEnumeratorGenericNamespace = NamespaceSystemCollectionsGeneric;
        public const string IEnumeratorGenericFull = "System.Collections.Generic.IEnumerator";

        public const string IEnumerableNonGenericFull = "System.Collections.IEnumerable";
        public const string IEnumerableGeneric = "IEnumerable";
        public const string IEnumerableGenericNamespace = NamespaceSystemCollectionsGeneric;
        public const string IEnumerableGenericFull = "System.Collections.Generic.IEnumerable";

        public const string ImmutableArray = "ImmutableArray";
        public const string ImmutableArrayNamespace = NamespaceSystemCollectionsImmutable;

        public const string ListGeneric = "List";
        public const string ListGenericNamespace = NamespaceSystemCollectionsGeneric;
        public const string ListGenericFull = "System.Collections.Generic.List";

        public const string Builder = "Builder";
        public const string FastSerializationProxy = "FastSerializationProxy";
        public const string FastSerializationEnumerable = "FastSerializationEnumerable";

        public const string ToBuilder = "ToBuilder";
        public const string ToBuildersList = "ToBuildersList";
        public const string ToNode = "ToNode";
        public const string ToImmutable = "ToImmutable";
        public const string ToImmutableRecursive = "ToImmutableRecursive";
        public const string ToSerializationProxy = "ToSerializationProxy";

        public const string XmlArray = "XmlArray";
        public const string XmlAttribute = "XmlAttribute";
        public const string XmlType = "XmlType";
        public const string NotSupportedExceptionFull = "System.NotSupportedException";
        public const string Update = "Update";
        public const string UpdateWith = "UpdateWith";
        public const string ToCoreArray = "ToCoreArray";
        public const string NodeList = "NodeList";
        public const string ToNodeList = "ToNodeList";
        public const string ModelExtensions = "ModelExtensions";
        public const string Deconstruct = "Deconstruct";
    }
}

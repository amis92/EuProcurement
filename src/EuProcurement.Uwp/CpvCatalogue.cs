﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace EuProcurement.Uwp
{
    public class CpvCatalogue
    {
        public CpvCatalogue(ImmutableDictionary<CpvCode, CpvTreeNode> divisions)
        {
            Divisions = divisions;
            AllNodes =
                Divisions.Values.SelectMany(div => div.ThisAndAllDescendants).ToImmutableDictionary(node => node.Code);
        }

        public ImmutableDictionary<CpvCode, CpvTreeNode> AllNodes { get; }

        public ImmutableDictionary<CpvCode, CpvTreeNode> Divisions { get; }
    }

    public static class CpvCatalogueFactory
    {
        public static CpvCatalogue CreateCatalogueFromXml(CpvCatalogueXml xml)
        {
            var descriptors = xml.Descriptors.ToImmutableDictionary(desc => new CpvCode(desc.CpvCode));
            var divisions =
                from code in descriptors.Keys
                group code by code.Division
                into div
                let divCodeOrDef = div.FirstOrDefault(c => c.Group == CpvCode.EmptyGroup)
                let divCode = divCodeOrDef.Equals(CpvCode.Empty) ? div.First() : divCodeOrDef
                select new CpvTreeNode.Builder
                {
                    Descriptor = descriptors[divCode],
                    Children =
                        from code1 in div.Except(new[] { divCode })
                        group code1 by code1.Group
                        into grp
                        let groupCodeOrDef = grp.FirstOrDefault(c => c.Class == CpvCode.EmptyClass)
                        let groupCode = groupCodeOrDef.Equals(CpvCode.Empty) ? grp.First() : groupCodeOrDef
                        select new CpvTreeNode.Builder
                        {
                            Descriptor = descriptors[groupCode],
                            Children =
                                from code2 in grp.Except(new[] { groupCode })
                                group code2 by code2.Class
                                into cls
                                let classCodeOrDef = cls.FirstOrDefault(c => c.Category == CpvCode.EmptyCategory)
                                let classCode = classCodeOrDef.Equals(CpvCode.Empty) ? cls.First() : classCodeOrDef
                                select new CpvTreeNode.Builder
                                {
                                    Descriptor = descriptors[classCode],
                                    Children =
                                        from code3 in cls.Except(new[] { classCode })
                                        group code3 by code3.Category
                                        into cat
                                        let catCodeOrDef = cat.FirstOrDefault(c => c.Subcategory == CpvCode.EmptySubcategory)
                                        let catCode = catCodeOrDef.Equals(CpvCode.Empty) ? cat.First() : catCodeOrDef
                                        select new CpvTreeNode.Builder
                                        {
                                            Descriptor = descriptors[catCode],
                                            Children =
                                                from sub in cat.Except(new[] { catCode })
                                                select new CpvTreeNode.Builder
                                                {
                                                    Descriptor = descriptors[sub],
                                                    Children = Enumerable.Empty<CpvTreeNode>()
                                                }.Build()
                                        }.Build()
                                }.Build()
                        }.Build()
                }.Build();
            return new CpvCatalogue(divisions.ToImmutableDictionary(div => div.Code));
        }
    }

    [DebuggerDisplay("{" + nameof(Code) + "}")]
    public class CpvTreeNode
    {
        public CpvTreeNode(CpvCode code, ImmutableDictionary<string, string> textTranslations, ImmutableDictionary<CpvCode, CpvTreeNode> children)
        {
            Code = code;
            TextTranslations = textTranslations;
            Children = children;
        }

        public CpvCode Code { get; }

        public ImmutableDictionary<string, string> TextTranslations { get; }

        public ImmutableDictionary<CpvCode, CpvTreeNode> Children { get; }

        public IEnumerable<CpvTreeNode> ThisAndAllDescendants
        {
            get { return new[] {this}.Concat(Children.Values.SelectMany(child => child.ThisAndAllDescendants)); }
        }

        public struct Builder
        {
            public CpvDescriptorXml Descriptor { get; set; }

            public IEnumerable<CpvTreeNode> Children { get; set; }

            public CpvTreeNode Build()
            {
                var code = new CpvCode(Descriptor.CpvCode);
                var textTranslations = Descriptor.Translations.ToImmutableDictionary(t => t.LanguageCode, t => t.Text);
                var childrenArray = Children.ToImmutableArray();
                var children = childrenArray.ToImmutableDictionary(child => child.Code);
                return new CpvTreeNode(code, textTranslations, children);
            }
        }
    }
}
